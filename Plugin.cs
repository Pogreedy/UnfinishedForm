using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnfinishedForm.Config;
using UnfinishedForm.Containment;
using UnfinishedForm.EGO;
using UnfinishedForm.Escape;

namespace UnfinishedForm
{
    /// <summary>
    /// Mod 入口。当前只保留生命周期与模块字段，所有玩法逻辑均为空。
    /// </summary>
    [BepInPlugin(PluginInfo.Guid, PluginInfo.Name, PluginInfo.Version)]
    [BepInProcess("LobotomyCorp.exe")]
    public sealed class UnfinishedFormPlugin : BaseUnityPlugin
    {
        internal static UnfinishedFormPlugin Instance { get; private set; }

        internal static ManualLogSource Log { get; private set; }

        internal ContainmentManager Containment { get; private set; }

        internal EscapeStateMachine EscapeMachine { get; private set; }

        internal BossPhaseStateMachine BossPhaseMachine { get; private set; }

        internal EgoSetManager EgoSets { get; private set; }

        internal ConfigLoader Configs { get; private set; }

        private Harmony _harmony;

        private void Awake()
        {
            Instance = this;
            Log = Logger;
            _harmony = new Harmony(PluginInfo.Guid);

            // TODO: 按以下顺序完成初始化，具体实现后续补充：
            // 1. Configs = new ConfigLoader(configDirectory); ModConfig config = Configs.LoadAll();
            // 2. Containment = new ContainmentManager(); 注册 config.Abnormalities；
            // 3. 创建 EscapeMachine / BossPhaseMachine / EgoSets 并注入配置；
            // 4. 用 Harmony 挂接工作、出逃、Boss 阶段与装备事件。
        }

        private void OnDestroy()
        {
            // TODO: 卸载补丁并释放各模块资源。
            _harmony?.UnpatchSelf();
            Instance = null;
            Log = null;
        }
    }
}
