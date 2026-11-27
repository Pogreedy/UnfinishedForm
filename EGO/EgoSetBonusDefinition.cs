using System;

namespace UnfinishedForm.EGO
{
    /// <summary>
    /// 按已装备件数触发的套装加成。
    /// </summary>
    [Serializable]
    public sealed class EgoSetBonusDefinition
    {
        /// <summary>触发该加成所需的件数（2 或 3）。</summary>
        public int RequiredPieces { get; set; }

        /// <summary>效果标识，由后续效果系统解释。</summary>
        public string EffectId { get; set; }

        /// <summary>面向玩家的描述文本。</summary>
        public string Description { get; set; }
    }
}
