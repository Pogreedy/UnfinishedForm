namespace UnfinishedForm.BaseMod
{
    /// <summary>武器脚本占位；武器本体数据由 Equipment/txts 提供。</summary>
    public sealed class UnfinishedForm_weapon : EquipmentScriptBase
    {
    }

    /// <summary>护甲脚本占位；护甲本体数据由 Equipment/txts 提供。</summary>
    public sealed class UnfinishedForm_armor : EquipmentScriptBase
    {
    }

    /// <summary>「未竟之冠」的基础数据常量，供后续套装逻辑使用。</summary>
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
    }

    /// <summary>
    /// BaseMod 侧的三件套识别常量。真正的套装被动/主动仍在后续阶段接入。
    /// </summary>
    public static class UnfinishedFormEgoSet
    {
        public const string SetId = UnfinishedCrownDefinition.SetId;
        public const string WeaponId = UnfinishedCrownDefinition.WeaponId;
        public const string SuitId = UnfinishedCrownDefinition.SuitId;
        public const string CrownId = UnfinishedCrownDefinition.ItemId;

        public static bool IsThreePiece(string weaponId, string suitId, string giftId)
        {
            return weaponId == WeaponId && suitId == SuitId && giftId == CrownId;
        }
    }
}
