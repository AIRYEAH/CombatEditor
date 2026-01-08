using System.Collections;
using System.Collections.Generic;
using System.IO;
using Animancer;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace CombatEditor
{
	public class CombatInspector : EditorWindow
	{
		//Object Property
		CombatEditor combatEditor;
		GameObject previewWeapon_L;
		GameObject previewWeapon_R;
		ReorderableList NodeList;
		SerializedObject go_combatController;   //选中游戏对象的CombatController
		SerializedObject go_combatDataStorge;   //选中游戏对象的CombatDataStorge

		//GUI Property
		float Height_Top = 40;
		Editor InspectedEditor;
		Vector2 InspectorScrollPos;
		int SelectedClipIndex;
		Vector2 Scroll;
		int CurrentGroupIndex = -1;
		int CurrentAbilityIndex = -1;
		public Rect WindowRect = new Rect(20, 20, 120, 50);


		[MenuItem("Tools/CombatInspector")]
		static void Init()
		{
			// Get existing open window or if none, make a new one:
			CombatInspector.CreateWindow();
			//window.InitWindow();
		}

		void OnEnable()
		{

		}

		void OnDisable()
		{
			// if (EditorUtility.DisplayDialog("提示", "存在未保存的修改，是否保存？", "保存", "放弃"))
			// {
			// 	// SaveChanges();
			// }
			if (combatEditor.SelectedController != null)
				combatEditor.SelectedController._combatDataStorage = null;      //退出时清空CombatDataStorage
		}
		public static CombatInspector CreateWindow()
		{
			CombatInspector window = (CombatInspector)EditorWindow.GetWindow(typeof(CombatInspector));
			window.Show();
			return window;
		}
		public void ResetInspector()
		{
			go_combatDataStorge = null;
			NodeList = null;
		}


		private void OnGUI()
		{
			PaintInspector();
		}

		public void PaintInspector()
		{
			if (!CombatEditorUtility.EditorExist())
			{
				return;
			}
			combatEditor = CombatEditorUtility.GetCurrentEditor();
			if (combatEditor.SelectedController == null)
				return;

			CombatController controller = combatEditor.SelectedController;
			go_combatController = controller != null ? new SerializedObject(controller) : null;
			go_combatDataStorge = combatEditor.SelectedController._combatDataStorage != null ? new SerializedObject(controller._combatDataStorage) : null;

			#region Init
			var HeaderStyle = combatEditor.HeaderStyle;
			var inspectedType = combatEditor.CurrentInspectedType;

			var CurrentAbilityObj = combatEditor.SelectedAbilityObj;

			CurrentGroupIndex = combatEditor.CurrentGroupIndex;
			CurrentAbilityIndex = combatEditor.CurrentAbilityIndexInGroup;

			var SelectedTrackIndex = combatEditor.SelectedTrackIndex;
			var TrackHeight = CombatEditor.LineHeight;
			#endregion
			GUILayout.Box("Inspector", HeaderStyle, GUILayout.Height(Height_Top));
			Rect InspectorRect = new Rect(new Rect(0, Height_Top, position.width, position.height));
			if (inspectedType == CombatEditor.InspectedType.Null)
			{
				return;
			}
			EditorGUILayout.BeginScrollView(Scroll);
			if (inspectedType == CombatEditor.InspectedType.PreviewConfig)
			{
				combatEditor.PlayTimeMultiplier = EditorGUILayout.FloatField(new GUIContent("PlaySpeed"), combatEditor.PlayTimeMultiplier);
				combatEditor.LoopWaitTime = EditorGUILayout.FloatField(new GUIContent("LoopInterval"), combatEditor.LoopWaitTime);
			}
			if (inspectedType == CombatEditor.InspectedType.AnimationConfig)
			{
				float DefaultWidth = EditorGUIUtility.labelWidth;
				EditorGUIUtility.labelWidth = 80;
				SerializedObject so = new SerializedObject(combatEditor.SelectedController._combatDataStorage);
				SerializedProperty combatDatas = so.FindProperty("CombatDatas");
				so.Update();
				if (combatEditor.CurrentGroupIndex < combatDatas.arraySize && combatEditor.CurrentGroupIndex >= 0)
				{
					SerializedProperty ObjsProperty = combatDatas.GetArrayElementAtIndex(combatEditor.CurrentGroupIndex).FindPropertyRelative("CombatObjs");
					if (combatEditor.CurrentAbilityIndexInGroup < ObjsProperty.arraySize && combatEditor.CurrentAbilityIndexInGroup >= 0)
					{
						SerializedProperty TargetObj = ObjsProperty.GetArrayElementAtIndex(combatEditor.CurrentAbilityIndexInGroup);
						EditorGUI.BeginChangeCheck();
						EditorGUILayout.PropertyField(TargetObj, new GUIContent("ConfigFile"));
						if (EditorGUI.EndChangeCheck())
						{
							combatEditor.SelectedAbilityObj = (AbilityScriptableObject)TargetObj.objectReferenceValue;
							combatEditor.LoadL3();
							combatEditor.Repaint();
							combatEditor.FlushAndInsPreviewToFrame0();
							Repaint();
						}
						if (TargetObj.objectReferenceValue == null)
						{
							if (GUILayout.Button("CreatConfig"))
							{
								TargetObj.objectReferenceValue = CreateAbilityScriptableObject();
								combatEditor.SelectedAbilityObj = (AbilityScriptableObject)TargetObj.objectReferenceValue;
								combatEditor.LoadL3();
								combatEditor.Repaint();
								combatEditor.FlushAndInsPreviewToFrame0();
							}
						}

						if (TargetObj.objectReferenceValue != null)
						{
							DrawAnimationClipSelector((AbilityScriptableObject)TargetObj.objectReferenceValue);
							DrawAbilityEventSelector((AbilityScriptableObject)TargetObj.objectReferenceValue);
						}
					}
				}
				so.ApplyModifiedProperties();
				EditorGUIUtility.labelWidth = DefaultWidth;
			}
			if (inspectedType == CombatEditor.InspectedType.Track)
			{
				//myRect.center = Vector2.one * 200;
				float DefaultWidth = EditorGUIUtility.labelWidth;
				EditorGUIUtility.labelWidth = 80;
				if (CurrentAbilityObj != null)
				{
					if (SelectedTrackIndex - 1 < CurrentAbilityObj.events.Count && SelectedTrackIndex - 1 >= 0 && CurrentAbilityObj.events.Count > 0)
					{
						string name = CurrentAbilityObj.events[SelectedTrackIndex - 1].Obj.name;
						CurrentAbilityObj.events[SelectedTrackIndex - 1].Obj.name = EditorGUILayout.TextField("Name", name);
					}
					if (InspectedEditor != null)
					{
						if (InspectedEditor.target != null)
						{
							InspectedEditor.OnInspectorGUI();
						}
					}
					EditorGUIUtility.labelWidth = DefaultWidth;
				}
			}
			if (inspectedType == CombatEditor.InspectedType.CombatConfig)
			{
				if (combatEditor.SelectedController != null)
				{
					InitAnimator();
					InitCombatDataStorge();
					InitPreviewWeapon();
					InitNodeReorableList();
				}
			}
			EditorGUILayout.EndScrollView();
		}

		public AbilityScriptableObject CreateAbilityScriptableObject()
		{
			if (!System.IO.Directory.Exists(CombatEditor.SandBoxPath))
			{
				System.IO.Directory.CreateDirectory(CombatEditor.SandBoxPath);
			}
			AbilityScriptableObject InsObj = CreateInstance("AbilityScriptableObject") as AbilityScriptableObject;
			InsObj.name = "NewAbilityScriptableObject";
			string path = CombatEditor.SandBoxPath;
			int index = 0;
			string TargetPath = path + InsObj.name + ".asset";
			while (true)
			{
				//??????ļ??????
				if (File.Exists(TargetPath))
				{
					TargetPath = path + InsObj.name + "_" + index + ".asset";
				}
				else
				{
					break;
				}
				index += 1;
			}
			AssetDatabase.CreateAsset(InsObj, TargetPath);
			Debug.Log("Combat Editor file Create");
			return InsObj;
		}

		public void CreateInspectedObj(Object InspectedObj)
		{
			ClearInspectedObj();

			InspectedEditor = Editor.CreateEditor(InspectedObj);
			Repaint();
		}
		public void ClearInspectedObj()
		{
			if (InspectedEditor != null)
			{
				DestroyImmediate(InspectedEditor);
			}
		}
		public static CombatInspector GetInspector()
		{
			return EditorWindow.GetWindow<CombatInspector>(false);
		}

		public void DrawAbilityConfigSelector(AbilityScriptableObject CurrentAbilityObj)
		{
			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.ObjectField("ConfigObj", CurrentAbilityObj, typeof(AbilityScriptableObject), false);
			EditorGUI.EndDisabledGroup();
		}
		public void DrawAbilityEventSelector(AbilityScriptableObject CurrentAbilityObj)
		{
			if (EditorGUILayout.DropdownButton(new GUIContent("Copy Events From Template"), FocusType.Passive))
			{
				if (!System.IO.Directory.Exists(CombatEditor.TemplatesPath))
				{
					System.IO.Directory.CreateDirectory(CombatEditor.TemplatesPath);
				}
				AbilityScriptableObject[] TemplatesObjs = CombatEditor.GetAtPath<AbilityScriptableObject>(CombatEditor.TemplatesPath);
				List<string> TemplatesObjNames = new List<string>();
				GenericMenu menu = new GenericMenu();
				for (int i = 0; i < TemplatesObjs.Length; i++)
				{
					TemplatesObjNames.Add(TemplatesObjs[i].name);
					menu.AddItem(new GUIContent(TemplatesObjs[i].name), false, CopyAbilityEvent, TemplatesObjs[i]);
				}
				menu.ShowAsContext();
			}
		}

		public void CopyAbilityEvent(object obj)
		{
			var editor = CombatEditorUtility.GetCurrentEditor();
			AbilityScriptableObject CurrentObj = editor.SelectedAbilityObj;
			for (int i = 0; i < CurrentObj.events.Count; i++)
			{
				string path = AssetDatabase.GetAssetPath(CurrentObj.events[i].Obj);
				var EveObj = CurrentObj.events[i].Obj;
				AssetDatabase.RemoveObjectFromAsset(EveObj);
				DestroyImmediate(EveObj, true);
			}
			CurrentObj.events = new List<AbilityEvent>();



			List<AbilityEvent> TargetEves = new List<AbilityEvent>();
			TargetEves = (obj as AbilityScriptableObject).events;

			for (int i = 0; i < TargetEves.Count; i++)
			{
				if (TargetEves[i].Obj == null) continue;
				AbilityEvent eve = new AbilityEvent();
				var EveObj = Instantiate(TargetEves[i].Obj);
				eve.Obj = EveObj;
				string path = AssetDatabase.GetAssetPath(editor.SelectedAbilityObj);
				eve.Obj.name = eve.Obj.name.Replace("(Clone)", "");
				AssetDatabase.AddObjectToAsset(EveObj, path);
				CurrentObj.events.Add(eve);
			}

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			editor.LoadL3();
			//Debug.Log("CopyAbility!");
		}
		public void DrawAnimationClipSelector(AbilityScriptableObject CurrentAbilityObj)
		{
			if (CurrentAbilityObj == null)
			{
				return;
			}


			CurrentAbilityObj.Clip = (AnimationClip)EditorGUILayout.ObjectField("Clip", CurrentAbilityObj.Clip, typeof(AnimationClip), false);

			CombatController controller = combatEditor.SelectedController;
			HybridAnimancerComponent animator = controller._animator;
			if (animator == null) return;

			if (animator.runtimeAnimatorController == null)
			{
				string msg = combatEditor.SelectedController._combatDataStorage.ToString() + " is null or not set animator controller!";
				EditorGUILayout.HelpBox(msg, MessageType.Warning);
				return;
			}

			var clips = animator.runtimeAnimatorController.animationClips;

			//Animator clips may change, so the index should auto change.
			bool ClipExist = false;
			for (int i = 0; i < clips.Length; i++)
			{
				if (CurrentAbilityObj.Clip == clips[i])
				{
					SelectedClipIndex = i + 1;
					ClipExist = true;
				}
			}
			if (!ClipExist)
			{
				SelectedClipIndex = 0;
			}

			List<string> clipsNames = new List<string>();
			//clipsNames.Add("Null");
			for (int i = 0; i < clips.Length; i++)
			{
				clipsNames.Add(clips[i].name);
			}


			string[] ClipNamesArray = clipsNames.ToArray();


			GenericMenu menu = new GenericMenu();
			if (EditorGUILayout.DropdownButton(new GUIContent("Select Clip From Animator"), FocusType.Passive))
			{
				for (int i = 0; i < clipsNames.Count; i++)
				{
					menu.AddItem(new GUIContent(clipsNames[i]), false, (object index) =>
					{
						int ClipIndex = 0;
						int.TryParse(index.ToString(), out ClipIndex);
						CurrentAbilityObj.Clip = clips[ClipIndex];

						combatEditor.LoadL3();
						//combatEditor.Repaint();
					}, i);
					menu.ShowAsContext();
				}
			}

			combatEditor.LoadL3();
		}

		public void SelectCombatConfig()
		{
			// CombatControllerSO = new SerializedObject(combatEditor.SelectedController);
			if (combatEditor == null)
				combatEditor = CombatEditorUtility.GetCurrentEditor();

			combatEditor.CurrentInspectedType = CombatEditor.InspectedType.CombatConfig;
			Repaint();
			InitNodeReorableList();
		}

		private void InitAnimator()
		{
			SerializedObject preview_so = new SerializedObject(combatEditor.SelectedController);
			//Animtor引用
			EditorGUILayout.PropertyField(preview_so.FindProperty("_animator"));
			if (combatEditor.SelectedController._animator != null)
			{
				if (combatEditor.SelectedController._animator.transform == combatEditor.SelectedController.transform)
				{
					EditorGUILayout.HelpBox("Animator transform should be the child transform of Combatcontroller!", MessageType.Error);
				}
			}
			else
			{
				combatEditor.SelectedController._animator = combatEditor.SelectedController.GetComponentInChildren<HybridAnimancerComponent>();
			}

			preview_so.ApplyModifiedProperties();
		}

		private void InitCombatDataStorge()
		{
			SerializedObject preview_so = new SerializedObject(combatEditor.SelectedController);
			EditorGUILayout.PropertyField(preview_so.FindProperty("_combatDataStorage"));
			preview_so.ApplyModifiedProperties();

			if (combatEditor.SelectedController._combatDataStorage == null)
			{
				EditorGUILayout.HelpBox("Combat Data Storge is Empty! Please replace it as your SO object! ", MessageType.Error);
				return;
				// CombatDataStorage temp = new CombatDataStorage();
				// temp.isTemplate = true;
				// combatEditor.SelectedController._combatDataStorage = temp;
			}

			// if (combatEditor.SelectedController._combatDataStorage.isTemplate)

			if (go_combatDataStorge != null)
			{
				// CombatDataStorge = new SerializedObject(combatEditor.SelectedController._combatDataStorage);
				combatEditor.SelectedController._animator.runtimeAnimatorController = go_combatDataStorge.targetObject.GetType().GetField("animator").GetValue(go_combatDataStorge.targetObject) as RuntimeAnimatorController;
			}
			else
			{
				combatEditor.SelectedController._animator.runtimeAnimatorController = null;
				return;
			}
		}

		private void InitNodeReorableList()
		{
			//Bug很多，会引起窗口错误打开，先不用了
			// SerializedObject preview_so = new SerializedObject(combatEditor.SelectedController);

			// NodeList = new ReorderableList(preview_so, preview_so.FindProperty("Nodes"), true, true, true, true);
			// NodeList.drawHeaderCallback = (Rect rect) =>
			// 	 {
			// 		 GUI.Label(rect, "CharacterNodes");
			// 	 };
			// NodeList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
			// {
			// 	EditorGUI.PropertyField(new Rect(rect.x, rect.y + 2, rect.width, rect.height), NodeList.serializedProperty.GetArrayElementAtIndex(index));
			// };
			// NodeList.DoLayoutList();

			// preview_so.ApplyModifiedProperties();
		}


		private void InitPreviewWeapon()
		{
			EditorGUI.BeginChangeCheck();
			combatEditor.currentPreviewWeaponModel_L = (GameObject)EditorGUILayout.ObjectField("WeaponModel_L", previewWeapon_L, typeof(GameObject), false);

			if (EditorGUI.EndChangeCheck())
			{
				//清理旧的预览武器
				if (previewWeapon_L != null)
					DestroyImmediate(previewWeapon_L);

				if (combatEditor.SelectedController.GetNodeTranform(CharacterNode.NodeType.Weapon_L))
					LoadPreviewWeapon(ref combatEditor.currentPreviewWeaponModel_L, combatEditor.SelectedController.GetNodeTranform(CharacterNode.NodeType.Weapon_L), out previewWeapon_L);

				Repaint();
			}

			EditorGUI.BeginChangeCheck();
			combatEditor.currentPreviewWeaponModel_R = (GameObject)EditorGUILayout.ObjectField("WeaponModel_R", previewWeapon_R, typeof(GameObject), false);
			if (EditorGUI.EndChangeCheck())
			{
				if (previewWeapon_R != null)
					DestroyImmediate(previewWeapon_R);

				if (combatEditor.SelectedController.GetNodeTranform(CharacterNode.NodeType.Weapon_R))
					LoadPreviewWeapon(ref combatEditor.currentPreviewWeaponModel_R, combatEditor.SelectedController.GetNodeTranform(CharacterNode.NodeType.Weapon_R), out previewWeapon_R);

				Repaint();
			}

			// 显示提示信息
			if (combatEditor.SelectedController == null)
			{
				EditorGUILayout.HelpBox("Please select a CombatController first", MessageType.Warning);
			}
			else if (Application.isPlaying)
			{
				EditorGUILayout.HelpBox("Preview weapon cannot be created during play mode", MessageType.Info);
			}

			if (combatEditor.SelectedController.GetNodeTranform(CharacterNode.NodeType.Weapon_L) == null)
				EditorGUILayout.HelpBox("CharacterNode Weapon_L is not set", MessageType.Warning);


			if (combatEditor.SelectedController.GetNodeTranform(CharacterNode.NodeType.Weapon_R) == null)
				EditorGUILayout.HelpBox("CharacterNode Weapon_R is not set", MessageType.Warning);
		}

		private void LoadPreviewWeapon(ref GameObject targetWeapon, Transform targetHangPoint, out GameObject currentLoadedWeapon)
		{
			if (targetWeapon != null && combatEditor.SelectedController != null)
			{
				currentLoadedWeapon = Instantiate(targetWeapon, targetHangPoint);
				currentLoadedWeapon.name = targetHangPoint.ToString();

				// 确保预览武器不会在游戏运行时存在
				if (Application.isPlaying)
				{
					DestroyImmediate(currentLoadedWeapon);
					currentLoadedWeapon = null;
					EditorUtility.DisplayDialog("Warning", "Cannot create preview weapon during play mode", "OK");
				}
			}
			else
			{
				currentLoadedWeapon = null;
			}
		}
	}
}
