# UnfinishedForm

《Lobotomy Corporation》异想体 Mod 项目骨架（BepInEx 5 + Harmony）。

> 当前状态：只有目录、数据模型与空方法，**没有任何玩法逻辑**。所有待实现点统一用 `TODO` 标记。

## 环境

| 项 | 值 |
| --- | --- |
| 游戏 | Lobotomy Corporation（Unity 2017.4.7，x64） |
| 加载器 | BepInEx 5.4.x（x64） |
| 目标框架 | .NET Framework 4.7.2 |
| 游戏根目录 | 通过 `GameDir` 或环境变量 `LOBOTOMY_CORP_DIR` 指定 |

本机示例路径：`D:\SteamLibrary\steamapps\common\LobotomyCorp`。

## 目录结构

```text
UnfinishedForm/
├─ Containment/              异想体档案与工作逻辑
│  ├─ AbnormalityDefinition.cs
│  ├─ ContainmentManager.cs
│  ├─ RiskLevel.cs
│  ├─ WorkPreferenceLevel.cs
│  ├─ WorkRecord.cs
│  ├─ WorkResult.cs
│  └─ WorkType.cs
├─ Escape/                   出逃与 Boss 阶段状态机
│  ├─ BossPhase.cs
│  ├─ BossPhaseStateMachine.cs
│  ├─ EscapeState.cs
│  └─ EscapeStateMachine.cs
├─ EGO/                      三件套与套装联动
│  ├─ EgoItemDefinition.cs
│  ├─ EgoItemType.cs
│  ├─ EgoLoadout.cs
│  ├─ EgoSetBonusDefinition.cs
│  ├─ EgoSetDefinition.cs
│  └─ EgoSetManager.cs
├─ Config/                   数值 JSON 与加载入口
│  ├─ ConfigLoader.cs
│  ├─ EscapeConfig.cs
│  ├─ ModConfig.cs
│  ├─ abnormalities.json
│  ├─ ego.json
│  ├─ ego_sets.json
│  └─ escape.json
├─ Plugin.cs                 BepInEx 插件入口（空实现）
├─ PluginInfo.cs             插件元数据
├─ UnfinishedForm.csproj
└─ UnfinishedForm.sln
```

## 模块说明

### Containment：异想体档案与工作

- `AbnormalityDefinition`：档案静态数据（编号、名称、危险等级、Qliphoth 上限、PE-Box 上限、四类工作偏好）。
- `ContainmentManager`：档案注册、查询与工作结算入口。
- `WorkRecord`：一次工作的结算记录。
- `WorkType` / `WorkResult` / `WorkPreferenceLevel` / `RiskLevel`：工作与风险枚举。

### Escape：出逃与 Boss 阶段

- `EscapeState`：收容中 → 出逃中 → 已出逃 → 已镇压。
- `EscapeStateMachine`：出逃状态迁移、计时与事件占位。
- `BossPhase`：三阶段 Boss + 狂暴阶段的占位枚举。
- `BossPhaseStateMachine`：血量/计时驱动的阶段迁移占位。

### EGO：三件套与套装联动

- `EgoItemType`：武器 / 护甲 / 饰品。
- `EgoItemDefinition`：单件 EGO 数据，`SetId` 关联套装。
- `EgoSetDefinition`：绑定三件套 Id，并持有加成列表。
- `EgoSetBonusDefinition`：按已装备件数（2 或 3）触发。
- `EgoLoadout`：当前装备组合。
- `EgoSetManager`：套装注册、生效套装与加成查询入口。

### Config：数值 JSON

- `abnormalities.json`：异想体档案表，顶层键为 `abnormalities`。
- `escape.json`：出逃与 Boss 阶段数值，顶层键为 `escape`。
- `ego.json`：单件 EGO 表，顶层键为 `items`。
- `ego_sets.json`：套装与加成表，顶层键为 `sets`。
- `ModConfig`：聚合后的配置对象。
- `ConfigLoader`：JSON 读取入口，序列化与校验待实现。

当前 JSON 均为空数组或零值占位，字段名与 C# 属性名按 camelCase 对应。

## 构建

1. 安装 BepInEx 5.4.x（x64）到游戏根目录。
2. 确认已安装 .NET Framework 4.7.2 Targeting Pack。
3. 指定游戏目录并构建：

```powershell
$env:LOBOTOMY_CORP_DIR = 'D:\SteamLibrary\steamapps\common\LobotomyCorp'
dotnet build .\UnfinishedForm.sln -c Release
```

如果 BepInEx 不在游戏根目录下，可额外指定：

```powershell
dotnet build .\UnfinishedForm.sln -c Release `
  -p:GameDir="$env:LOBOTOMY_CORP_DIR" `
  -p:BepInExDir="D:\path\to\BepInEx\core"
```

构建产物为 `bin\Release\UnfinishedForm.dll`。若 `$(GameDir)\BepInEx\plugins` 存在，构建后会自动复制到 `BepInEx\plugins\UnfinishedForm\`。

## 待实现

- [ ] JSON 读取、校验与热重载
- [ ] 异想体注册与工作结算
- [ ] 出逃 / Boss 阶段迁移条件
- [ ] EGO 三件套获取、装备与套装联动
- [ ] Harmony 补丁与游戏生命周期接入
- [ ] 本地化与美术资源

## 加载器说明

本骨架按 BepInEx 插件组织。LC 也常见通过 `LobotomyCorp_Data\BaseMods` 加载模组；若改用 BaseMods，需要新增 `LobotomyBaseMod` 入口并在其中调用本项目的初始化流程，领域模型与状态机代码可以复用。
