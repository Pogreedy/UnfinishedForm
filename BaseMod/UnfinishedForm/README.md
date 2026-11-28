# UnfinishedForm 最小 BaseMod 包

这是第一阶段的可加载占位包，目标是先验证：

- LMM / BaseMod 能加载模组；
- 「未竟之形」能出现在收容单元；
- 员工能进行本能、洞察、沟通、压迫四类工作；
- 「审判」暂时映射到压迫（原版没有第五类工作按钮）。

当前**不包含**完整的四阶段 Boss 逻辑、员工状态、EGO 三件套与 JSON 配置加载。

## 安装

把整个 `UnfinishedForm` 文件夹复制到：

```text
LobotomyCorp_Data\BaseMods\UnfinishedForm
```

然后通过 LMM 启动游戏。

## 占位表现

- 模型/动画：原版 `Unit/CreatureAnimator/DummyCreature`
- 立绘：原版 `CreatureSprite/Dummy`
- 不使用、也不复制「永恒之花」的美术资源。

## 构建

```powershell
dotnet build .\Source\UnfinishedForm.BaseMod.csproj -c Release `
  -p:GameDir="D:\SteamLibrary\steamapps\common\LobotomyCorp"
```

构建后 DLL 会复制到本目录根下。

## 已知限制

- 目前继承 `DummyCreature`，只验证生成与工作流程，不包含未竟之形的完整收容规则。
- 没有 EGO 武器/护甲/饰品，PE-BOX 产出暂未接入。
- 没有四阶段 Boss 行为与阶段转换。
- 审判没有独立 UI，使用压迫按钮。
