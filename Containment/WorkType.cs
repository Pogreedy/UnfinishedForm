namespace UnfinishedForm.Containment
{
    /// <summary>异想体工作类型。</summary>
    public enum WorkType
    {
        /// <summary>本能</summary>
        Instinct = 0,

        /// <summary>洞察</summary>
        Insight = 1,

        /// <summary>沟通</summary>
        Attachment = 2,

        /// <summary>压迫</summary>
        Repression = 3,

        /// <summary>
        /// 专属工作：审判。不属于本能/洞察/沟通/压迫四类，
        /// 仅对「未竟之形」可用。
        /// </summary>
        Judgment = 100,
    }
}
