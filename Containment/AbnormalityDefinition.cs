using System;
using System.Collections.Generic;

namespace UnfinishedForm.Containment
{
    /// <summary>
    /// 异想体档案的静态数据，对应 Config/abnormalities.json 中的一条记录。
    /// </summary>
    [Serializable]
    public sealed class AbnormalityDefinition
    {
        /// <summary>档案编号，例如 F-00-000。</summary>
        public string Id { get; set; }

        /// <summary>显示名称。</summary>
        public string Name { get; set; }

        /// <summary>危险等级。</summary>
        public RiskLevel RiskLevel { get; set; }

        /// <summary>Qliphoth Counter 上限。</summary>
        public int QliphothCounterMax { get; set; }

        /// <summary>PE-Box 上限。</summary>
        public int PeBoxMax { get; set; }

        /// <summary>四类工作的偏好等级。</summary>
        public Dictionary<WorkType, WorkPreferenceLevel> WorkPreferences { get; set; }

        public AbnormalityDefinition()
        {
            WorkPreferences = new Dictionary<WorkType, WorkPreferenceLevel>();
        }
    }
}
