using System;
using UnfinishedForm.Escape;

namespace UnfinishedForm.EGO
{
    /// <summary>
    /// 「未竟之形」三件套的装备者状态与终极联动。
    /// 当前只处理数据与效果入口，不直接改动游戏内员工对象。
    /// </summary>
    public sealed class UnfinishedFormEgoSet
    {
        public const string SetId = UnfinishedCrownDefinition.SetId;
        public const string WeaponId = UnfinishedCrownDefinition.WeaponId;
        public const string SuitId = UnfinishedCrownDefinition.SuitId;
        public const string CrownId = UnfinishedCrownDefinition.ItemId;

        public const int RefuseBelongingMaxUses = 2;
        public const int CrownReflectionMaxUses = 1;
        public const int DeathSaveDesireStacks = 3;
        public const float DeathSaveInvincibleSeconds = 3f;
        public const float UnfinishedFormDurationSeconds = 5f;
        public const float CrownReflectionCocoonReductionPerGaze = 0.05f;
        public const float CrownReflectionShieldReductionPerGaze = 100f;

        private readonly EgoLoadout _loadout;

        public UnfinishedFormEgoSet(EgoLoadout loadout)
        {
            _loadout = loadout ?? new EgoLoadout();

            if (IsCrownEquipped)
            {
                FortitudeBonus = UnfinishedCrownDefinition.AllStatsBonus;
                PrudenceBonus = UnfinishedCrownDefinition.AllStatsBonus;
                TemperanceBonus = UnfinishedCrownDefinition.AllStatsBonus;
                JusticeBonus = UnfinishedCrownDefinition.AllStatsBonus;
                SpBonus = UnfinishedCrownDefinition.SpBonus;
                WorkSpeedMultiplier = 1f + UnfinishedCrownDefinition.WorkSpeedBonus;
                AttackSpeedMultiplier = 1f + UnfinishedCrownDefinition.AttackSpeedBonus;
            }
            else
            {
                WorkSpeedMultiplier = 1f;
                AttackSpeedMultiplier = 1f;
            }

            MaxHp = 100;
            Hp = MaxHp;

            RefuseBelongingUsesRemaining = HasThreePieceSet ? RefuseBelongingMaxUses : 0;
            CrownReflectionUsesRemaining = HasThreePieceSet ? CrownReflectionMaxUses : 0;
        }

        public EgoLoadout Loadout => _loadout;

        public bool IsCrownEquipped =>
            string.Equals(_loadout.GiftId, CrownId, StringComparison.Ordinal);

        public bool HasThreePieceSet =>
            string.Equals(_loadout.WeaponId, WeaponId, StringComparison.Ordinal)
            && string.Equals(_loadout.SuitId, SuitId, StringComparison.Ordinal)
            && IsCrownEquipped;

        public int FortitudeBonus { get; }

        public int PrudenceBonus { get; }

        public int TemperanceBonus { get; }

        public int JusticeBonus { get; }

        public int SpBonus { get; }

        public float WorkSpeedMultiplier { get; }

        public float AttackSpeedMultiplier { get; }

        /// <summary>装备未竟之冠时，每层注视的伤害加成减半。</summary>
        public float GazeDamageBonusMultiplier =>
            IsCrownEquipped ? UnfinishedCrownDefinition.GazeDamageBonusMultiplier : 1f;

        /// <summary>反显影「被定义」时长：未竟之冠为 2 秒，否则 4 秒。</summary>
        public float AntiManifestDefinedDuration =>
            IsCrownEquipped
                ? UnfinishedCrownDefinition.CrownAntiManifestDefinedSeconds
                : UnfinishedCrownDefinition.BaseAntiManifestDefinedSeconds;

        public int DesireStacks { get; set; }

        public int LightShadowStacks { get; set; }

        public int GazeStacks { get; set; }

        public bool IsManifesting { get; private set; }

        public bool IsInvincible => InvincibleSeconds > 0f;

        public float InvincibleSeconds { get; private set; }

        public bool DeathSaveUsed { get; private set; }

        public int Hp { get; private set; }

        public int MaxHp { get; }

        public bool IsAlive => Hp > 0;

        public int RefuseBelongingUsesRemaining { get; private set; }

        public int CrownReflectionUsesRemaining { get; private set; }

        /// <summary>把注视伤害加成按未竟之冠效果缩放。</summary>
        public float ScaleGazeDamageBonus(float baseBonus)
        {
            return baseBonus * GazeDamageBonusMultiplier;
        }

        /// <summary>把饰品被动同步到 Boss 目标，供反显影协议判断时长。</summary>
        public void ApplyCrownPassive(BossTarget target)
        {
            if (target != null)
            {
                target.HasUnfinishedCrown = IsCrownEquipped;
            }
        }

        /// <summary>进入「显影」；三件套触发渴望逆转。</summary>
        public void EnterManifestation()
        {
            IsManifesting = true;

            if (HasThreePieceSet)
            {
                int converted = DesireStacks;
                DesireStacks = 0;
                LightShadowStacks += converted;
            }
        }

        public void ExitManifestation()
        {
            IsManifesting = false;
        }

        /// <summary>拒绝归属：净化一名归属员工，使其获得 5 秒未竟形态。</summary>
        public bool UseRefuseBelonging(BossTarget target)
        {
            if (!HasThreePieceSet
                || !IsManifesting
                || RefuseBelongingUsesRemaining <= 0
                || target == null
                || !target.IsAlive
                || !target.IsBelonging)
            {
                return false;
            }

            target.IsBelonging = false;
            target.UnfinishedFormSeconds = UnfinishedFormDurationSeconds;
            RefuseBelongingUsesRemaining--;
            return true;
        }

        /// <summary>
        /// 冠冕反照：清空自身注视。茧阶段按每层 5% 降低 Boss 减伤，
        /// 四阶段按每层 100 点削减护盾，自身获得等量光影。
        /// </summary>
        public bool UseCrownReflection(BossEncounter boss)
        {
            if (!HasThreePieceSet
                || CrownReflectionUsesRemaining <= 0
                || boss == null
                || GazeStacks <= 0)
            {
                return false;
            }

            int cleared = GazeStacks;

            if (boss.IsPhaseTwo)
            {
                boss.ReduceCocoonDamageReduction(cleared * CrownReflectionCocoonReductionPerGaze);
            }
            else if (boss.IsPhaseFour && !boss.IsShieldBroken)
            {
                boss.ReducePhaseFourShield(cleared * CrownReflectionShieldReductionPerGaze);
            }
            else
            {
                return false;
            }

            GazeStacks = 0;
            LightShadowStacks += cleared;
            CrownReflectionUsesRemaining--;
            return true;
        }

        /// <summary>
        /// 受到伤害。首次致命伤害保留 1 点生命、3 秒无敌并获得 3 层渴望。
        /// 返回 true 表示本次触发了免死。
        /// </summary>
        public bool ApplyDamage(int damage)
        {
            if (damage <= 0 || !IsAlive || IsInvincible)
            {
                return false;
            }

            Hp -= damage;

            if (Hp > 0)
            {
                return false;
            }

            if (!DeathSaveUsed)
            {
                DeathSaveUsed = true;
                Hp = 1;
                InvincibleSeconds = DeathSaveInvincibleSeconds;
                DesireStacks += DeathSaveDesireStacks;
                return true;
            }

            Hp = 0;
            return false;
        }

        /// <summary>推进无敌计时与目标身上的未竟形态计时。</summary>
        public void Tick(float deltaTime, BossTarget target = null)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            if (InvincibleSeconds > 0f)
            {
                InvincibleSeconds = Math.Max(0f, InvincibleSeconds - deltaTime);
            }

            if (target != null && target.UnfinishedFormSeconds > 0f)
            {
                target.UnfinishedFormSeconds = Math.Max(0f, target.UnfinishedFormSeconds - deltaTime);
            }
        }

        public void ResetForBattle()
        {
            Hp = MaxHp;
            DesireStacks = 0;
            LightShadowStacks = 0;
            GazeStacks = 0;
            IsManifesting = false;
            InvincibleSeconds = 0f;
            DeathSaveUsed = false;
            RefuseBelongingUsesRemaining = HasThreePieceSet ? RefuseBelongingMaxUses : 0;
            CrownReflectionUsesRemaining = HasThreePieceSet ? CrownReflectionMaxUses : 0;
        }

        /// <summary>注册到通用套装管理器时使用的套装定义。</summary>
        public static EgoSetDefinition CreateDefinition()
        {
            var set = new EgoSetDefinition
            {
                Id = SetId,
                Name = "未竟之形",
                WeaponId = WeaponId,
                SuitId = SuitId,
                GiftId = CrownId,
            };

            set.Bonuses.Add(new EgoSetBonusDefinition
            {
                RequiredPieces = 3,
                EffectId = "unfinished-form-ultimate",
                Description = "触发渴望逆转、拒绝归属、冠冕反照与免死被动。",
            });

            return set;
        }
    }
}
