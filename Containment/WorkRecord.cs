using System;

namespace UnfinishedForm.Containment
{
    /// <summary>
    /// 一次工作的结算记录，同时携带需要由外层系统应用的员工效果。
    /// </summary>
    [Serializable]
    public sealed class WorkRecord
    {
        public string AbnormalityId { get; set; }

        public string AgentName { get; set; }

        public WorkType WorkType { get; set; }

        public WorkResult Result { get; set; }

        public int PeBoxes { get; set; }

        /// <summary>本次工作前的计数器值。</summary>
        public int CounterBefore { get; set; }

        /// <summary>本次工作后的计数器值。</summary>
        public int CounterAfter { get; set; }

        /// <summary>本次工作是否导致计数器归零并触发 OnEscapeTriggered。</summary>
        public bool TriggeredEscape { get; set; }

        /// <summary>「被定义」的精神抗性修正；未触发时为 0。</summary>
        public float MentalResistanceDelta { get; set; }

        /// <summary>「被定义」持续秒数；未触发时为 0。</summary>
        public float MentalRecoveryBlockSeconds { get; set; }

        /// <summary>「未竟回响」造成的精神伤害；未触发时为 0。</summary>
        public float MentalDamage { get; set; }

        /// <summary>「渴望」层数；未触发时为 0。</summary>
        public int DesireStacks { get; set; }
    }
}
