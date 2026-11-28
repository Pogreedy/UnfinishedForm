using System;

namespace UnfinishedForm.Escape
{
    /// <summary>金光爪击的结算结果。</summary>
    [Serializable]
    public sealed class GoldenClawResult
    {
        public string TargetName { get; set; }

        public int Damage { get; set; }

        public int DesireStacksAfter { get; set; }

        public bool SoulPenetration { get; set; }
    }

    /// <summary>分身攻击的结算结果。</summary>
    [Serializable]
    public sealed class PhantomAttackResult
    {
        public int PhantomId { get; set; }

        public string TargetName { get; set; }

        public float Damage { get; set; }

        public bool Blocked { get; set; }

        public bool SoulPenetration { get; set; }
    }

    /// <summary>「我不是你」的结算结果。</summary>
    [Serializable]
    public sealed class NotMeResult
    {
        public float TriggerAtHpRatio { get; set; }

        public int AffectedCount { get; set; }

        public int ClearedDesire { get; set; }

        public bool BuffGranted { get; set; }

        public float MentalDamage { get; set; }
    }

    /// <summary>反显影协议的结算结果。</summary>
    [Serializable]
    public sealed class AntiManifestResult
    {
        public string TargetName { get; set; }

        public bool Interrupted { get; set; }

        public int GazeConsumed { get; set; }

        public int GazeStacksAfter { get; set; }

        public bool AlreadyUsedThisPhase { get; set; }

        /// <summary>反显影成功后施加的「被定义」秒数。</summary>
        public float DefinedSeconds { get; set; }

        public string Reason { get; set; }
    }
}
