using System;

namespace UnfinishedForm.Escape
{
    public sealed partial class BossEncounter
    {
        public const float PhaseFourBaseShield = 2000f;
        public const float PhaseFourShieldPerDesire = 100f;
        public const float PhaseFourShieldBonusCap = 1000f;
        public const float PhaseFourMaxShield = PhaseFourBaseShield + PhaseFourShieldBonusCap;

        public const float PhaseFourShieldPhysicalResistance = 0.4f;
        public const float PhaseFourShieldMentalResistance = 0.2f;
        public const float PhaseFourShieldErosionResistance = 0.3f;
        public const float PhaseFourShieldSoulResistance = 0.7f;

        public const float PhaseFourBrokenPhysicalResistance = 0.7f;
        public const float PhaseFourBrokenMentalResistance = 0.5f;
        public const float PhaseFourBrokenErosionResistance = 0.6f;
        public const float PhaseFourBrokenSoulResistance = 1.3f;

        /// <summary>第四阶段当前护盾值。</summary>
        public float ShieldHp { get; private set; }

        /// <summary>进入第四阶段时获得的初始护盾值。</summary>
        public float PhaseFourShieldInitial { get; private set; }

        /// <summary>进入第四阶段时吸收的渴望总层数。</summary>
        public int PhaseFourAbsorbedDesire { get; private set; }

        /// <summary>吸收渴望提供的护盾加成，最高 1000。</summary>
        public float PhaseFourShieldBonus { get; private set; }

        public bool IsShieldBroken => IsPhaseFour && ShieldHp <= 0f;

        public bool IsSuppressed { get; private set; }

        public bool IsPhaseFour => Phase.CurrentPhase == BossPhase.PhaseFour_Backlash;

        public event Action PhaseFourStarted;

        public event Action PhaseFourShieldBroken;

        public event Action BossSuppressed;

        /// <summary>第四阶段抗性：护盾存在时使用护盾抗性，破碎后使用本体抗性。</summary>
        public float GetPhaseFourResistance(DamageType damageType)
        {
            bool shielded = ShieldHp > 0f;

            switch (damageType)
            {
                case DamageType.Physical:
                    return shielded
                        ? PhaseFourShieldPhysicalResistance
                        : PhaseFourBrokenPhysicalResistance;

                case DamageType.Mental:
                    return shielded
                        ? PhaseFourShieldMentalResistance
                        : PhaseFourBrokenMentalResistance;

                case DamageType.Erosion:
                    return shielded
                        ? PhaseFourShieldErosionResistance
                        : PhaseFourBrokenErosionResistance;

                case DamageType.Soul:
                    return shielded
                        ? PhaseFourShieldSoulResistance
                        : PhaseFourBrokenSoulResistance;

                default:
                    return 1f;
            }
        }

        private void EnterPhaseFour()
        {
            ResetPhaseThreeState();
            ResetPhaseFourState();

            int absorbedDesire = 0;

            for (int i = 0; i < _employees.Count; i++)
            {
                BossTarget employee = _employees[i];

                if (employee.DesireStacks > 0)
                {
                    absorbedDesire += employee.DesireStacks;
                    employee.DesireStacks = 0;
                }
            }

            PhaseFourAbsorbedDesire = absorbedDesire;
            PhaseFourShieldBonus = Math.Min(
                absorbedDesire * PhaseFourShieldPerDesire,
                PhaseFourShieldBonusCap);
            ShieldHp = PhaseFourBaseShield + PhaseFourShieldBonus;
            PhaseFourShieldInitial = ShieldHp;
            GazeStacks = 0;

            PhaseFourStarted?.Invoke();
        }

        private void ResetPhaseFourState()
        {
            ShieldHp = 0f;
            PhaseFourShieldInitial = 0f;
            PhaseFourAbsorbedDesire = 0;
            PhaseFourShieldBonus = 0f;
            IsSuppressed = false;
        }

        private void TickPhaseFour(float deltaTime)
        {
            // 第四阶段暂无独立技能循环；护盾与本体伤害由 ApplyDamage 处理。
        }

        private float ApplyPhaseFourDamage(DamageType damageType, float amount)
        {
            ResetLastDamageFields();

            if (IsSuppressed || amount <= 0f)
            {
                return 0f;
            }

            float resistance = GetPhaseFourResistance(damageType);
            float effectiveDamage = amount * ResistanceToMultiplier(resistance);

            if (effectiveDamage <= 0f)
            {
                return 0f;
            }

            float overflowDamage;

            if (ShieldHp > 0f)
            {
                float absorbedDamage = Math.Min(ShieldHp, effectiveDamage);
                ShieldHp -= absorbedDamage;
                overflowDamage = effectiveDamage - absorbedDamage;

                if (ShieldHp <= 0f)
                {
                    ShieldHp = 0f;
                    PhaseFourShieldBroken?.Invoke();
                }
            }
            else
            {
                overflowDamage = effectiveDamage;
            }

            if (overflowDamage > 0f)
            {
                BodyHp = Math.Max(0f, BodyHp - overflowDamage);
                LastBodyDamage = overflowDamage;

                if (BodyHp <= 0f)
                {
                    MarkSuppressed();
                }
            }

            return effectiveDamage;
        }

        private void MarkSuppressed()
        {
            if (IsSuppressed)
            {
                return;
            }

            IsSuppressed = true;
            BodyHp = 0f;
            BossSuppressed?.Invoke();
        }
    }
}
