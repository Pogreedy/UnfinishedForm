using System;

namespace UnfinishedForm.Escape
{
    /// <summary>一次反照判定的结果。</summary>
    [Serializable]
    public sealed class ReflectionResult
    {
        public string TargetName { get; set; }

        public bool TargetHadEgoSet { get; set; }

        public float BossHeal { get; set; }

        public float BossHpAfter { get; set; }

        public string CopiedStatName { get; set; }

        public float CopiedStatValue { get; set; }

        public int GazeStacksGained { get; set; }

        public int GazeStacksAfter { get; set; }

        public float MentalDamage { get; set; }

        public int ReflectionCount { get; set; }

        public BossPhase PhaseAfter { get; set; }

        public bool PhaseChanged { get; set; }
    }
}
