using System;

namespace UnfinishedForm.Config
{
    /// <summary>
    /// 出逃与 Boss 阶段的数值配置，对应 Config/escape.json。
    /// 当前全部为占位零值，具体含义与单位在实现时确定。
    /// </summary>
    [Serializable]
    public sealed class EscapeConfig
    {
        /// <summary>触发工作特殊事件的 Qliphoth Counter 阈值。</summary>
        public int QliphothCounterThreshold { get; set; }

        /// <summary>出逃预警到实际出逃的间隔（秒）。</summary>
        public float EscapeIntervalSeconds { get; set; }

        /// <summary>Boss 阶段总血量。</summary>
        public float BossMaxHp { get; set; }

        /// <summary>进入第二阶段的血量比例。</summary>
        public float PhaseTwoHpRatio { get; set; }

        /// <summary>进入第三阶段的血量比例。</summary>
        public float PhaseThreeHpRatio { get; set; }

        /// <summary>进入狂暴阶段的延迟（秒）。</summary>
        public float EnrageDelaySeconds { get; set; }
    }
}
