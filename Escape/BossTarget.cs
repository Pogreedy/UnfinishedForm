using System;

namespace UnfinishedForm.Escape
{
    /// <summary>
    /// Boss 反照判定遇到的一名员工。属性值由外层员工系统提供。
    /// </summary>
    [Serializable]
    public sealed class BossTarget
    {
        public string Name { get; set; }

        public bool HasEgoSet { get; set; }

        public bool IsAlive { get; set; } = true;

        public bool IsBelonging { get; set; }

        /// <summary>是否处于套装员工专属的「显影」状态。</summary>
        public bool IsManifesting { get; set; }

        /// <summary>是否装备「未竟之冠」。</summary>
        public bool HasUnfinishedCrown { get; set; }

        /// <summary>反显影「被定义」剩余秒数。</summary>
        public float DefinedSecondsRemaining { get; set; }

        /// <summary>「未竟形态」剩余秒数。</summary>
        public float UnfinishedFormSeconds { get; set; }

        public int DesireStacks { get; set; }

        public int LightShadowStacks { get; set; }

        public string HighestStatName { get; set; }

        public float HighestStatValue { get; set; }
    }
}
