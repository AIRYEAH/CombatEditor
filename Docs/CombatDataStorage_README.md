# CombatDataStorage 系统使用指南

## 概述

CombatDataStorage系统将原本硬编码在CombatController中的CombatDatas数据迁移到外部的ScriptableObject中，实现了运行时动态加载功能。

## 系统组件

### 1. CombatDataStorage
- **位置**: `Assets/CombatEditor/Scripts/CombatObj/CombatDataStorage.cs`
- **功能**: 存储CombatDatas数据的ScriptableObject
- **创建方式**: 
  - 菜单栏: `Assets/Create/Combat/Combat Data Storage`
  - 或在Project窗口右键创建

### 2. CombatDataManager
- **位置**: `Assets/CombatEditor/Scripts/CombatObj/CombatDataManager.cs`
- **功能**: 运行时管理CombatDataStorage的加载和访问
- **特性**: 
  - 单例模式
  - 支持同步/异步加载
  - 支持Resources和AssetBundle加载
  - 缓存已加载的资��

### 3. CombatDataStorageEditor
- **位置**: `Assets/CombatEditor/Scripts/Editor/CombatDataStorageEditor.cs`
- **功能**: 提供编辑器界面来管理CombatDataStorage

## 使用方法

### 第一步：创建CombatDataStorage资源

1. 在Project窗口右键选择 `Create/Combat/Combat Data Storage`
2. 将资源保存到合适的位置（如 `Assets/Resources/CombatData/`）
3. 在Inspector窗口中添加CombatGroup数据

### 第二步：配置CombatController

#### 方法一：Inspector直接引用
```csharp
// 在CombatController的Inspector中直接拖拽CombatDataStorage资源
[SerializeField]
private CombatDataStorage _combatDataStorage;
```

#### 方法二：运行时动态加载
```csharp
// 同步加载
_combatController.SetCombatDataFromManager("CombatData/DefaultCombatData");

// 异步加载
yield return _combatController.SetCombatDataFromManagerAsync("CombatData/DefaultCombatData", (success) => {
    if (success) {
        // 加载成功
    }
});
```

### 第三步：使用示例

参考 `CombatDataStorageExample.cs` 脚本：

```csharp
public class MyCombatSystem : MonoBehaviour
{
    [SerializeField] private CombatController _combatController;
    
    private void Start()
    {
        // 方法1：直接引用
        _combatController.SetCombatDataStorage(_combatDataStorage);
        
        // 方法2：从Resources加载
        _combatController.SetCombatDataFromManager("CombatData/MyCombatData");
        
        // 方法3：异步加载
        StartCoroutine(LoadCombatDataAsync());
    }
    
    private IEnumerator LoadCombatDataAsync()
    {
        yield return _combatController.SetCombatDataFromManagerAsync("CombatData/MyCombatData", (success) => {
            if (success) {
                Debug.Log("Combat data loaded successfully!");
            }
        });
    }
}
```

## 高级功能

### 动态切换CombatData

```csharp
// 切换到不同的CombatDataStorage
_combatController.SetCombatDataFromManager("CombatData/BossCombatData");

// 异步切换
yield return _combatController.SetCombatDataFromManagerAsync("CombatData/BossCombatData");
```

### AssetBundle支持

```csharp
// 从AssetBundle加载
var bundle = AssetBundle.LoadFromFile("path/to/bundle");
var storage = CombatDataManager.Instance.LoadCombatDataStorageFromAssetBundle(bundle, "MyCombatData");
_combatController.SetCombatDataStorage(storage);
```

### 状态检查

```csharp
// 检查CombatData是否已加载
bool isLoaded = _combatController.IsCombatDataLoaded();

// 获取加载信息
string info = _combatController.GetCombatDataInfo();
```

## 文件结构建议

```
Assets/
├── Resources/
│   └── CombatData/
│       ├── DefaultCombatData.asset
│       ├── PlayerCombatData.asset
│       └── EnemyCombatData.asset
├── CombatEditor/
│   ├── Scripts/
│   │   └── CombatObj/
│   │       ├── CombatDataStorage.cs
│   │       ├── CombatDataManager.cs
│   │       └── CombatDataStorageExample.cs
│   └── Editor/
│       └── CombatDataStorageEditor.cs
└── AssetBundles/
    └── combat_data.bundle
```

## 注意事项

1. **资源路径**: 确保Resources文件夹中的路径正确
2. **内存管理**: 使用CombatDataManager.Instance.UnloadCombatDataStorage()释放不需要的资源
3. **异步加载**: 在协程中使用异步加载避免卡顿
4. **错误处理**: 始终检查加载结果，处理加载失败的情况

## 迁移指南

如果从旧版本迁移：

1. 将硬编码的CombatDatas数据导出到CombatDataStorage资源中
2. 修改CombatController的初始化代码
3. 测试所有战斗功能确保正常工作

## 性能优化

- 使用CombatDataManager的缓存机制避免重复加载
- 对于频繁切换的场景，预加载常用CombatDataStorage
- 使用AssetBundle进行资源分包加载