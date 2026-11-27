using System;
using System.Collections.Generic;
using UnfinishedForm.Containment;
using UnfinishedForm.EGO;

namespace UnfinishedForm.Config
{
    /// <summary>
    /// 聚合后的 Mod 配置。由 ConfigLoader 从 Config/*.json 组装。
    /// </summary>
    [Serializable]
    public sealed class ModConfig
    {
        public List<AbnormalityDefinition> Abnormalities { get; set; }

        public EscapeConfig Escape { get; set; }

        public List<EgoItemDefinition> EgoItems { get; set; }

        public List<EgoSetDefinition> EgoSets { get; set; }

        public ModConfig()
        {
            Abnormalities = new List<AbnormalityDefinition>();
            Escape = new EscapeConfig();
            EgoItems = new List<EgoItemDefinition>();
            EgoSets = new List<EgoSetDefinition>();
        }
    }
}
