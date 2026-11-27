namespace UnfinishedForm.Escape
{
    /// <summary>异想体的收容/出逃状态。</summary>
    public enum EscapeState
    {
        /// <summary>正常收容中。</summary>
        Contained = 0,

        /// <summary>出逃预警或出逃动画阶段。</summary>
        Escaping = 1,

        /// <summary>已出逃，可在设施内活动。</summary>
        Escaped = 2,

        /// <summary>已被镇压，等待收容复位。</summary>
        Suppressed = 3,
    }
}
