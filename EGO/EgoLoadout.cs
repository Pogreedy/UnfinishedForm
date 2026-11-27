using System;

namespace UnfinishedForm.EGO
{
    /// <summary>
    /// 当前装备组合。Id 为空表示该槽位未装备。
    /// </summary>
    [Serializable]
    public sealed class EgoLoadout
    {
        public string WeaponId { get; set; }

        public string SuitId { get; set; }

        public string GiftId { get; set; }
    }
}
