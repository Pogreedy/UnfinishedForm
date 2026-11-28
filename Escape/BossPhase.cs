namespace UnfinishedForm.Escape
{
    /// <summary>出逃后的 Boss 阶段。具体阶段数量与触发条件待定。</summary>
    public enum BossPhase
    {
        /// <summary>未进入 Boss 阶段。</summary>
        None = 0,

        PhaseOne = 1,
        PhaseTwo = 2,
        PhaseThree = 3,

        /// <summary>第四阶段：光影反噬。</summary>
        PhaseFour_Backlash = 4,

        /// <summary>狂暴或最终阶段。</summary>
        Enrage = 5,
    }
}
