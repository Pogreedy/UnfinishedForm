using System;
using System.Collections.Generic;

namespace UnfinishedForm.EGO
{
    /// <summary>
    /// EGO 套装定义：绑定武器、护甲、饰品三件套，并列出联动加成。
    /// </summary>
    [Serializable]
    public sealed class EgoSetDefinition
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string WeaponId { get; set; }

        public string SuitId { get; set; }

        public string GiftId { get; set; }

        /// <summary>按件数生效的套装加成。</summary>
        public List<EgoSetBonusDefinition> Bonuses { get; set; }

        public EgoSetDefinition()
        {
            Bonuses = new List<EgoSetBonusDefinition>();
        }
    }
}
