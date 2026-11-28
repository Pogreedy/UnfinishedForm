namespace UnfinishedForm.BaseMod
{
    /// <summary>武器脚本占位；武器本体数据由 Equipment/txts 提供。</summary>
    public sealed class UnfinishedForm_weapon : EquipmentScriptBase
    {
    }

    /// <summary>护甲脚本占位；护甲本体数据由 Equipment/txts 提供。</summary>
    public sealed class UnfinishedForm_armor : EquipmentScriptBase
    {
        private const int WeaponId = 30002;
        private const int ArmorId = 30102;
        private const int GiftId = 30202;

        public override EGObonusInfo GetBonus(UnitModel actor)
        {
            EGObonusInfo baseBonus = base.GetBonus(actor);
            EGObonusInfo result = new EGObonusInfo();

            if (baseBonus != null)
            {
                result.hp = baseBonus.hp;
                result.mental = baseBonus.mental;
                result.cubeSpeed = baseBonus.cubeSpeed;
                result.workProb = baseBonus.workProb;
                result.movement = baseBonus.movement;
                result.attackSpeed = baseBonus.attackSpeed;
                result.damage = baseBonus.damage;
            }

            if (IsThreePiece(actor))
            {
                // 非 Boss 依赖的三件套基础增益。
                result.hp += 10;
                result.mental += 10;
                result.workProb += 10;
                result.damage += 10;
                result.attackSpeed += 10;
            }

            return result;
        }

        private static bool IsThreePiece(UnitModel actor)
        {
            if (actor == null || actor.Equipment == null)
            {
                return false;
            }

            UnitEquipSpace equipment = actor.Equipment;
            return equipment.HasEquipment(WeaponId)
                && equipment.HasEquipment(ArmorId)
                && equipment.HasEquipment(GiftId);
        }
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
