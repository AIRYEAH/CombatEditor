using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CombatEditor
{
    /// <summary>
    /// CombatDataStorage使用示例
    /// </summary>
    public class CombatDataStorageExample : MonoBehaviour
    {
        [Header("Combat Data Storage Settings")]
        [SerializeField]
        private CombatDataStorage _combatDataStorage;

        [SerializeField]
        private string _resourcePath = "CombatData/DefaultCombatData";

        [SerializeField]
        private CombatController _combatController;

        [Header("Runtime Loading Options")]
        [SerializeField]
        private bool _loadOnStart = true;

        [SerializeField]
        private bool _useAsyncLoading = true;

        private void Start()
        {
            if (_loadOnStart)
            {
                if (_combatDataStorage != null)
                {
                    // 直接使用Inspector中设置的CombatDataStorage
                    _combatController.SetCombatDataStorage(_combatDataStorage);
                    Debug.Log("CombatDataStorage loaded from Inspector reference");
                }
                else if (_useAsyncLoading)
                {
                    // 异步从Resources加载
                    StartCoroutine(LoadCombatDataAsync());
                }
                else
                {
                    // 同步从Resources加载
                    LoadCombatDataSync();
                }
            }
        }

        /// <summary>
        /// 异步加载CombatDataStorage示例
        /// </summary>
        private IEnumerator LoadCombatDataAsync()
        {
            Debug.Log($"Starting async load of CombatDataStorage from: {_resourcePath}");

            yield return _combatController.SetCombatDataFromManagerAsync(_resourcePath, (success) =>
            {
                if (success)
                {
                    Debug.Log($"CombatDataStorage loaded successfully from Resources: {_resourcePath}");
                    OnCombatDataLoaded();
                }
                else
                {
                    Debug.LogError($"Failed to load CombatDataStorage from Resources: {_resourcePath}");
                }
            });
        }

        /// <summary>
        /// 同步加载CombatDataStorage示例
        /// </summary>
        private void LoadCombatDataSync()
        {
            Debug.Log($"Starting sync load of CombatDataStorage from: {_resourcePath}");

            _combatController.SetCombatDataFromManager(_resourcePath);

            if (_combatController.IsCombatDataLoaded())
            {
                Debug.Log($"CombatDataStorage loaded successfully from Resources: {_resourcePath}");
                OnCombatDataLoaded();
            }
            else
            {
                Debug.LogError($"Failed to load CombatDataStorage from Resources: {_resourcePath}");
            }
        }

        /// <summary>
        /// 动态切换CombatDataStorage示例
        /// </summary>
        public void SwitchCombatDataStorage(string newResourcePath)
        {
            if (_combatController.IsCombatDataLoaded())
            {
                Debug.Log("Switching to new CombatDataStorage...");
                _combatController.SetCombatDataFromManager(newResourcePath);
                OnCombatDataLoaded();
            }
        }

        /// <summary>
        /// 动态切换CombatDataStorage（异步版本）
        /// </summary>
        public void SwitchCombatDataStorageAsync(string newResourcePath)
        {
            StartCoroutine(SwitchCombatDataStorageCoroutine(newResourcePath));
        }

        private IEnumerator SwitchCombatDataStorageCoroutine(string newResourcePath)
        {
            yield return _combatController.SetCombatDataFromManagerAsync(newResourcePath, (success) =>
            {
                if (success)
                {
                    Debug.Log($"Successfully switched to CombatDataStorage: {newResourcePath}");
                    OnCombatDataLoaded();
                }
                else
                {
                    Debug.LogError($"Failed to switch to CombatDataStorage: {newResourcePath}");
                }
            });
        }

        /// <summary>
        /// CombatData加载完成后的回调
        /// </summary>
        private void OnCombatDataLoaded()
        {
            // 这里可以添加加载完成后的逻辑
            Debug.Log($"Combat data loaded successfully! Total groups: {_combatController.CombatDatas.Count}");

            // 示例：打印所有战斗组信息
            for (int i = 0; i < _combatController.CombatDatas.Count; i++)
            {
                var group = _combatController.CombatDatas[i];
                Debug.Log($"Group {i}: {group.Label} - {group.CombatObjs.Count} abilities");
            }
        }

        /// <summary>
        /// 检查CombatData是否已加载
        /// </summary>
        public bool IsCombatDataLoaded()
        {
            return _combatController != null && _combatController.IsCombatDataLoaded();
        }

        /// <summary>
        /// 获取当前加载的CombatDataStorage信息
        /// </summary>
        public string GetCurrentCombatDataInfo()
        {
            if (!IsCombatDataLoaded())
            {
                return "No CombatData loaded";
            }

            var datas = _combatController.CombatDatas;
            return $"CombatData loaded: {datas.Count} groups, {GetTotalAbilities(datas)} abilities";
        }

        private int GetTotalAbilities(List<CombatGroup> groups)
        {
            int total = 0;
            foreach (var group in groups)
            {
                total += group.CombatObjs.Count;
            }
            return total;
        }
    }
}