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

        public int DesireStacks { get; set; }

        public int LightShadowStacks { get; set; }

        public string HighestStatName { get; set; }

        public float HighestStatValue { get; set; }
    }
}
