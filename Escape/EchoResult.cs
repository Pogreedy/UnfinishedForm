using System;

namespace UnfinishedForm.Escape
{
    /// <summary>一次未竟回响的结算结果。</summary>
    [Serializable]
    public sealed class EchoResult
    {
        public string TargetName { get; set; }

        public bool TargetHadEgoSet { get; set; }

        public float SoulDamage { get; set; }

        public bool LightShadowRemoved { get; set; }

        public int LightShadowStacksAfter { get; set; }

        public int EchoCount { get; set; }
    }
}
