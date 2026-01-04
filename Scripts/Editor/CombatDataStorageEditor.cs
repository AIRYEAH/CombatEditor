using UnityEditor;
using UnityEngine;

namespace CombatEditor
{
    [CustomEditor(typeof(CombatDataStorage))]
    public class CombatDataStorageEditor : Editor
    {
        private CombatDataStorage _storage;

        private void OnEnable()
        {
            _storage = target as CombatDataStorage;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // EditorGUILayout.PropertyField(serializedObject.FindProperty("isTemplate"), true);
            // Animator引用
            EditorGUILayout.PropertyField(serializedObject.FindProperty("animator"), true);
            // 显示CombatDatas列表
            EditorGUILayout.PropertyField(serializedObject.FindProperty("CombatDatas"), true);

            EditorGUILayout.Space();

            // 添加操作按钮
            if (GUILayout.Button("Add New Combat Group"))
            {
                _storage.AddCombatGroup(new CombatGroup());
                EditorUtility.SetDirty(_storage);
            }

            if (GUILayout.Button("Clear All Data"))
            {
                if (EditorUtility.DisplayDialog("Clear Combat Data", "Are you sure you want to clear all combat data?", "Yes", "No"))
                {
                    _storage.ClearCombatDatas();
                    EditorUtility.SetDirty(_storage);
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Combat Groups: {_storage.GetCombatGroupCount()}", EditorStyles.boldLabel);

            serializedObject.ApplyModifiedProperties();
        }
    }

    public class CombatDataStorageCreator
    {
        [MenuItem("Assets/Create/Combat/Combat Data Storage")]
        public static void CreateCombatDataStorage()
        {
            var storage = ScriptableObject.CreateInstance<CombatDataStorage>();

            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(path))
            {
                path = "Assets";
            }
            else if (!System.IO.Directory.Exists(path))
            {
                path = System.IO.Path.GetDirectoryName(path);
            }

            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/CombatDataStorage.asset");

            AssetDatabase.CreateAsset(storage, assetPathAndName);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = storage;
        }
    }
}