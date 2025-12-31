using System.Collections.Generic;
using UnityEngine;

namespace CombatEditor
{
    /// <summary>
    /// CombatData管理器，用于运行时加载和管理CombatDataStorage资源
    /// </summary>
    public class CombatDataManager : MonoBehaviour
    {
        private static CombatDataManager _instance;
        public static CombatDataManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<CombatDataManager>();
                    if (_instance == null)
                    {
                        GameObject obj = new GameObject("CombatDataManager");
                        _instance = obj.AddComponent<CombatDataManager>();
                        DontDestroyOnLoad(obj);
                    }
                }
                return _instance;
            }
        }

        private Dictionary<string, CombatDataStorage> _loadedStorages = new Dictionary<string, CombatDataStorage>();

        /// <summary>
        /// 从Resources文件夹加载CombatDataStorage
        /// </summary>
        public CombatDataStorage LoadCombatDataStorage(string resourcePath)
        {
            if (_loadedStorages.ContainsKey(resourcePath))
            {
                return _loadedStorages[resourcePath];
            }

            var storage = Resources.Load<CombatDataStorage>(resourcePath);
            if (storage != null)
            {
                _loadedStorages[resourcePath] = storage;
                return storage;
            }

            Debug.LogError($"Failed to load CombatDataStorage from Resources: {resourcePath}");
            return null;
        }

        /// <summary>
        /// 从AssetBundle加载CombatDataStorage
        /// </summary>
        public CombatDataStorage LoadCombatDataStorageFromAssetBundle(AssetBundle bundle, string assetName)
        {
            string key = $"{bundle.name}:{assetName}";
            if (_loadedStorages.ContainsKey(key))
            {
                return _loadedStorages[key];
            }

            var storage = bundle.LoadAsset<CombatDataStorage>(assetName);
            if (storage != null)
            {
                _loadedStorages[key] = storage;
                return storage;
            }

            Debug.LogError($"Failed to load CombatDataStorage from AssetBundle: {assetName}");
            return null;
        }

        /// <summary>
        /// 异步加载CombatDataStorage
        /// </summary>
        public System.Collections.IEnumerator LoadCombatDataStorageAsync(string resourcePath, System.Action<CombatDataStorage> onComplete)
        {
            if (_loadedStorages.ContainsKey(resourcePath))
            {
                onComplete?.Invoke(_loadedStorages[resourcePath]);
                yield break;
            }

            var request = Resources.LoadAsync<CombatDataStorage>(resourcePath);
            yield return request;

            if (request.asset != null)
            {
                var storage = request.asset as CombatDataStorage;
                _loadedStorages[resourcePath] = storage;
                onComplete?.Invoke(storage);
            }
            else
            {
                Debug.LogError($"Failed to load CombatDataStorage asynchronously from: {resourcePath}");
                onComplete?.Invoke(null);
            }
        }

        /// <summary>
        /// 卸载指定的CombatDataStorage
        /// </summary>
        public void UnloadCombatDataStorage(string key)
        {
            if (_loadedStorages.ContainsKey(key))
            {
                _loadedStorages.Remove(key);
            }
        }

        /// <summary>
        /// 清空所有已加载的CombatDataStorage
        /// </summary>
        public void ClearAllStorages()
        {
            _loadedStorages.Clear();
        }

        /// <summary>
        /// 获取所有已加载的CombatDataStorage
        /// </summary>
        public List<CombatDataStorage> GetAllLoadedStorages()
        {
            return new List<CombatDataStorage>(_loadedStorages.Values);
        }

        /// <summary>
        /// 检查指定路径的CombatDataStorage是否已加载
        /// </summary>
        public bool IsStorageLoaded(string key)
        {
            return _loadedStorages.ContainsKey(key);
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
                ClearAllStorages();
            }
        }
    }

    /// <summary>
    /// CombatDataManager的静态访问扩展
    /// </summary>
    public static class CombatDataManagerExtensions
    {
        /// <summary>
        /// 为CombatController快速设置CombatDataStorage
        /// </summary>
        public static void SetCombatDataFromManager(this CombatController controller, string resourcePath)
        {
            var storage = CombatDataManager.Instance.LoadCombatDataStorage(resourcePath);
            if (storage != null)
            {
                controller.SetCombatDataStorage(storage);
            }
        }

        /// <summary>
        /// 异步为CombatController设置CombatDataStorage
        /// </summary>
        public static System.Collections.IEnumerator SetCombatDataFromManagerAsync(this CombatController controller, string resourcePath, System.Action<bool> onComplete = null)
        {
            yield return CombatDataManager.Instance.LoadCombatDataStorageAsync(resourcePath, (storage) =>
            {
                if (storage != null)
                {
                    controller.SetCombatDataStorage(storage);
                    onComplete?.Invoke(true);
                }
                else
                {
                    onComplete?.Invoke(false);
                }
            });
        }
    }
}