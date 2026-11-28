using System;

namespace UnfinishedForm.EGO
{
    /// <summary>
    /// 头部饰品「未竟之冠」的基础数据与被动常量。
    /// </summary>
    public static class UnfinishedCrownDefinition
    {
        public const string ItemId = "unfinished-crown";
        public const string Name = "未竟之冠";
        public const string SetId = "unfinished-form";
        public const string WeaponId = "unfinished-form-weapon";
        public const string SuitId = "unfinished-form-suit";

        public const int AllStatsBonus = 10;
        public const int SpBonus = 10;
        public const float WorkSpeedBonus = 0.05f;
        public const float AttackSpeedBonus = 0.05f;

        /// <summary>每层注视伤害加成的减半系数。</summary>
        public const float GazeDamageBonusMultiplier = 0.5f;

        /// <summary>反显影「被定义」的原时长。</summary>
        public const float BaseAntiManifestDefinedSeconds = 4f;

        /// <summary>装备未竟之冠后的「被定义」时长。</summary>
        public const float CrownAntiManifestDefinedSeconds = 2f;

        public static EgoItemDefinition CreateDefinition()
        {
            return new EgoItemDefinition
            {
                Id = ItemId,
                Name = Name,
                Type = EgoItemType.Gift,
                SetId = SetId,
                Tier = 0,
            };
        }
    }
}
