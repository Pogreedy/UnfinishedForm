using System;
using System.Collections.Generic;
using UnfinishedForm.Containment;

namespace UnfinishedForm.Escape
{
    /// <summary>
    /// 「未竟之形」出逃战。第一阶段为摹仿·反照，第二阶段为茧中梦；
    /// 第三阶段只保留阶段接口，不实现技能。
    /// </summary>
    public sealed partial class BossEncounter
    {
        public const string PhaseOneName = "摹仿·反照";
        public const string PhaseTwoName = "茧中梦";

        public const float MaxHp = 1800f;
        public const float PhysicalResistance = 0.8f;
        public const float MentalResistance = 0.5f;
        public const float ErosionResistance = 0.6f;
        public const float SoulResistance = 0.9f;

        public const float PhaseTwoHpThreshold = MaxHp * BossPhaseStateMachine.PhaseTwoHpRatio;

        public const int MaxGazeStacks = 5;
        public const float GazeMentalDamage = 40f;
        public const float EchoIntervalSeconds = 12f;
        public const float EchoSoulDamage = 50f;
        public const float NonEgoSetHeal = 80f;

        public const float CocoonMaxHp = 1500f;
        public const float BodyMaxHp = 1200f;
        public const float CocoonPhysicalResistance = 0.2f;
        public const float CocoonMentalResistance = 0.5f;
        public const float CocoonErosionResistance = 0.4f;
        public const float CocoonSoulResistance = -1.0f;

        public const float CocoonDesirePulseIntervalSeconds = 10f;
        public const float DesireDamageReductionPerEmployee = 0.05f;
        public const float DesireDamageReductionCap = 0.40f;
        public const float BelongingDamageReductionPerEmployee = 0.10f;
        public const float BelongingHealIntervalSeconds = 5f;
        public const float BelongingHealPerTickPerEmployee = 30f;
        public const float CocoonOverflowToBodyRatio = 0.30f;

        private readonly List<BossTarget> _employees = new List<BossTarget>();
        private readonly Random _random;
        private float _echoTimer;
        private float _cocoonDesireTimer;
        private float _cocoonHealTimer;

        public BossEncounter(ContainmentManager containment)
            : this(containment, new Random())
        {
        }

        public BossEncounter(ContainmentManager containment, Random random)
        {
            _random = random ?? new Random();
            Escape = new EscapeStateMachine();
            Phase = new BossPhaseStateMachine();
            Alarm = new GlobalAlarm();
            Alarm.DesirePulse += ApplyAlarmPulse;
            Phase.PhaseChanged += OnPhaseChanged;

            if (containment != null)
            {
                containment.Escaped += Activate;

                if (containment.EscapeTriggered)
                {
                    Activate();
                }
            }
        }

        public EscapeStateMachine Escape { get; }

        public BossPhaseStateMachine Phase { get; }

        public GlobalAlarm Alarm { get; }

        /// <summary>第一阶段 Boss 生命值。</summary>
        public float Hp { get; private set; } = MaxHp;

        /// <summary>第二阶段光茧生命值；未进入第二阶段时为 0。</summary>
        public float CocoonHp { get; private set; }

        /// <summary>隐藏的本体生命值。</summary>
        public float BodyHp { get; private set; } = BodyMaxHp;

        public int GazeStacks { get; private set; }

        public int ReflectionCount { get; private set; }

        public int EchoCount { get; private set; }

        public BossTarget LastTarget { get; private set; }

        public EchoResult LastEcho { get; private set; }

        /// <summary>最近一次对茧的结算伤害。</summary>
        public float LastCocoonDamage { get; private set; }

        /// <summary>最近一次破茧时的溢出伤害。</summary>
        public float LastOverflowDamage { get; private set; }

        /// <summary>最近一次转移到本体的伤害。</summary>
        public float LastBodyDamage { get; private set; }

        public float CopiedPowerBonus { get; private set; }

        public string LastCopiedStatName { get; private set; }

        public float LastCopiedStatValue { get; private set; }

        public bool IsActive => Escape.CurrentState == EscapeState.Escaped;

        public bool IsPhaseOne => Phase.CurrentPhase == BossPhase.PhaseOne;

        public bool IsPhaseTwo => Phase.CurrentPhase == BossPhase.PhaseTwo;

        public bool IsPhaseThree => Phase.CurrentPhase == BossPhase.PhaseThree;

        public int LivingDesireEmployeeCount
        {
            get
            {
                int count = 0;

                for (int i = 0; i < _employees.Count; i++)
                {
                    BossTarget employee = _employees[i];

                    if (employee.IsAlive && employee.DesireStacks > 0)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        public int BelongingEmployeeCount
        {
            get
            {
                int count = 0;

                for (int i = 0; i < _employees.Count; i++)
                {
                    BossTarget employee = _employees[i];

                    if (employee.IsAlive && employee.IsBelonging)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        /// <summary>渴望员工提供的减伤，最高 40%。</summary>
        public float DesireDamageReduction =>
            Math.Min(LivingDesireEmployeeCount * DesireDamageReductionPerEmployee, DesireDamageReductionCap);

        /// <summary>归属员工提供的额外减伤。</summary>
        public float BelongingDamageReduction =>
            BelongingEmployeeCount * BelongingDamageReductionPerEmployee;

        /// <summary>茧当前总减伤；只在第二阶段计算。</summary>
        public float CocoonDamageReduction
        {
            get
            {
                if (!IsPhaseTwo)
                {
                    return 0f;
                }

                return Math.Min(1f, DesireDamageReduction + BelongingDamageReduction);
            }
        }

        public IReadOnlyList<BossTarget> TrackedEmployees => _employees;

        public event Action<ReflectionResult> ReflectionResolved;

        public event Action<EchoResult> EchoTriggered;

        public event Action PhaseTwoStarted;

        /// <summary>第三阶段入口事件。技能尚未实现。</summary>
        public event Action PhaseThreeStarted;

        /// <summary>茧每 10 秒施加渴望时触发，参数为每名存活员工获得的层数。</summary>
        public event Action<int> CocoonDesirePulse;

        /// <summary>把员工加入全设施名单，警报与茧的渴望会作用于名单。</summary>
        public void TrackEmployee(BossTarget employee)
        {
            if (employee != null && !_employees.Contains(employee))
            {
                _employees.Add(employee);
            }
        }

        public void UntrackEmployee(BossTarget employee)
        {
            if (employee != null)
            {
                _employees.Remove(employee);
            }
        }

        /// <summary>员工恐慌时标记为「归属」。标记会保留到后续阶段。</summary>
        public void MarkBelonging(BossTarget employee)
        {
            if (employee == null)
            {
                return;
            }

            TrackEmployee(employee);

            if (employee.IsAlive)
            {
                employee.IsBelonging = true;
            }
        }

        /// <summary>开始出逃：重置 Boss、进入第一阶段、启动警报并切换到出逃状态。</summary>
        public void Activate()
        {
            if (Escape.CurrentState == EscapeState.Escaped)
            {
                return;
            }

            ResetState();
            Phase.StartPhaseOne();
            Alarm.Activate();
            Escape.RequestEscape();
        }

        /// <summary>每帧推进出逃、警报、第一阶段回响或第二阶段茧机制。</summary>
        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            Escape.Tick(deltaTime);
            Alarm.Tick(deltaTime);

            if (!IsActive)
            {
                return;
            }

            if (IsPhaseOne)
            {
                _echoTimer += deltaTime;

                while (_echoTimer >= EchoIntervalSeconds)
                {
                    _echoTimer -= EchoIntervalSeconds;
                    TriggerEcho();
                }

                return;
            }

            if (IsPhaseTwo)
            {
                TickCocoon(deltaTime);
                return;
            }

            if (IsPhaseThree)
            {
                TickPhaseThree(deltaTime);
            }
        }

        /// <summary>
        /// Boss 在走廊移动时对遇到的员工进行一次反照判定。
        /// 非第一阶段或未出逃时返回 null。
        /// </summary>
        public ReflectionResult ResolveReflection(BossTarget target)
        {
            if (target == null || !IsActive || !IsPhaseOne)
            {
                return null;
            }

            TrackEmployee(target);

            BossPhase phaseBefore = Phase.CurrentPhase;
            int gazeBefore = GazeStacks;
            var result = new ReflectionResult
            {
                TargetName = target.Name,
                TargetHadEgoSet = target.HasEgoSet,
            };

            if (!target.HasEgoSet)
            {
                float missingHp = Math.Max(0f, MaxHp - Hp);
                float heal = Math.Min(NonEgoSetHeal, missingHp);
                Hp += heal;

                float copiedValue = Math.Max(0f, target.HighestStatValue);
                CopiedPowerBonus += copiedValue;
                LastCopiedStatName = target.HighestStatName;
                LastCopiedStatValue = copiedValue;

                result.BossHeal = heal;
                result.CopiedStatName = LastCopiedStatName;
                result.CopiedStatValue = copiedValue;
            }
            else
            {
                result.MentalDamage = GazeMentalDamage;
                AddGaze(1);
            }

            if (target.DesireStacks > 0)
            {
                AddGaze(1);
            }

            LastTarget = target;
            ReflectionCount++;
            result.GazeStacksGained = GazeStacks - gazeBefore;
            result.GazeStacksAfter = GazeStacks;
            result.ReflectionCount = ReflectionCount;
            result.BossHpAfter = Hp;

            Phase.NotifyReflectionCount(ReflectionCount);
            result.PhaseAfter = Phase.CurrentPhase;
            result.PhaseChanged = phaseBefore != Phase.CurrentPhase;

            ReflectionResolved?.Invoke(result);
            return result;
        }

        /// <summary>
        /// 对当前形态造成伤害。第一阶段返回实际扣血，第二阶段返回结算后的有效伤害，
        /// 第三阶段返回本体扣血。第二阶段的溢出伤害详情见 LastOverflowDamage。
        /// </summary>
        public float ApplyDamage(DamageType damageType, float amount)
        {
            if (!IsActive || amount <= 0f)
            {
                return 0f;
            }

            if (IsPhaseOne)
            {
                return ApplyPhaseOneDamage(damageType, amount);
            }

            if (IsPhaseTwo)
            {
                return ApplyCocoonDamage(damageType, amount);
            }

            if (IsPhaseThree)
            {
                return ApplyPhaseThreeDamage(damageType, amount);
            }

            return 0f;
        }

        /// <summary>第一阶段 Boss 抗性倍率。</summary>
        public float GetResistance(DamageType damageType)
        {
            switch (damageType)
            {
                case DamageType.Physical:
                    return PhysicalResistance;

                case DamageType.Mental:
                    return MentalResistance;

                case DamageType.Erosion:
                    return ErosionResistance;

                case DamageType.Soul:
                    return SoulResistance;

                default:
                    return 1f;
            }
        }

        /// <summary>第二阶段光茧抗性。负抗性表示弱点，会按 1 + |抗性| 放大伤害。</summary>
        public float GetCocoonResistance(DamageType damageType)
        {
            switch (damageType)
            {
                case DamageType.Physical:
                    return CocoonPhysicalResistance;

                case DamageType.Mental:
                    return CocoonMentalResistance;

                case DamageType.Erosion:
                    return CocoonErosionResistance;

                case DamageType.Soul:
                    return CocoonSoulResistance;

                default:
                    return 1f;
            }
        }

        /// <summary>镇压后结束出逃并停止全局警报。</summary>
        public void EndEscape()
        {
            Escape.NotifySuppressed();
            Alarm.Deactivate();
        }

        /// <summary>复位 Boss 与出逃状态，保留员工名单、渴望与归属标记。</summary>
        public void Reset()
        {
            ResetState();
        }

        private void TickCocoon(float deltaTime)
        {
            _cocoonDesireTimer += deltaTime;

            while (_cocoonDesireTimer >= CocoonDesirePulseIntervalSeconds)
            {
                _cocoonDesireTimer -= CocoonDesirePulseIntervalSeconds;
                ApplyCocoonDesirePulse();
            }

            _cocoonHealTimer += deltaTime;

            while (_cocoonHealTimer >= BelongingHealIntervalSeconds)
            {
                _cocoonHealTimer -= BelongingHealIntervalSeconds;
                HealCocoonFromBelonging();
            }
        }

        private void ApplyCocoonDesirePulse()
        {
            for (int i = 0; i < _employees.Count; i++)
            {
                BossTarget employee = _employees[i];

                if (employee.IsAlive)
                {
                    employee.DesireStacks += 1;
                }
            }

            CocoonDesirePulse?.Invoke(1);
        }

        private void HealCocoonFromBelonging()
        {
            int count = BelongingEmployeeCount;

            if (count <= 0)
            {
                return;
            }

            float heal = BelongingHealPerTickPerEmployee * count;
            CocoonHp = Math.Min(CocoonMaxHp, CocoonHp + heal);
        }

        private float ApplyPhaseOneDamage(DamageType damageType, float amount)
        {
            ResetLastDamageFields();

            float actualDamage = amount * GetResistance(damageType);
            Hp = Math.Max(0f, Hp - actualDamage);
            Phase.NotifyHpChanged(Hp, MaxHp);
            return actualDamage;
        }

        private float ApplyCocoonDamage(DamageType damageType, float amount)
        {
            ResetLastDamageFields();

            float multiplier = ResistanceToMultiplier(GetCocoonResistance(damageType));
            float reduction = CocoonDamageReduction;
            float effectiveDamage = amount * multiplier * Math.Max(0f, 1f - reduction);

            if (effectiveDamage <= 0f)
            {
                return 0f;
            }

            float previousCocoonHp = CocoonHp;
            CocoonHp = Math.Max(0f, CocoonHp - effectiveDamage);
            LastCocoonDamage = previousCocoonHp - CocoonHp;

            if (CocoonHp <= 0f)
            {
                float overflow = Math.Max(0f, effectiveDamage - LastCocoonDamage);
                LastOverflowDamage = overflow;
                LastBodyDamage = overflow * CocoonOverflowToBodyRatio;
                BodyHp = Math.Max(0f, BodyHp - LastBodyDamage);
                Phase.NotifyCocoonBroken();
            }

            return effectiveDamage;
        }

        private void ResetState()
        {
            Escape.Reset();
            Alarm.Reset();
            Phase.Reset();

            Hp = MaxHp;
            CocoonHp = 0f;
            BodyHp = BodyMaxHp;
            GazeStacks = 0;
            ReflectionCount = 0;
            EchoCount = 0;
            LastTarget = null;
            LastEcho = null;
            CopiedPowerBonus = 0f;
            LastCopiedStatName = null;
            LastCopiedStatValue = 0f;
            _echoTimer = 0f;
            _cocoonDesireTimer = 0f;
            _cocoonHealTimer = 0f;
            ResetPhaseThreeState();
            ResetLastDamageFields();
        }

        private void ResetLastDamageFields()
        {
            LastCocoonDamage = 0f;
            LastOverflowDamage = 0f;
            LastBodyDamage = 0f;
        }

        private void OnPhaseChanged(BossPhase previousPhase, BossPhase currentPhase)
        {
            if (currentPhase == BossPhase.PhaseTwo)
            {
                EnterPhaseTwo();
            }
            else if (currentPhase == BossPhase.PhaseThree)
            {
                _cocoonDesireTimer = 0f;
                _cocoonHealTimer = 0f;
                EnterPhaseThree();
            }
        }

        private void EnterPhaseTwo()
        {
            CocoonHp = CocoonMaxHp;
            BodyHp = BodyMaxHp;
            _cocoonDesireTimer = 0f;
            _cocoonHealTimer = 0f;
            ResetLastDamageFields();
            PhaseTwoStarted?.Invoke();
        }

        private void AddGaze(int amount)
        {
            GazeStacks += amount;

            if (GazeStacks > MaxGazeStacks)
            {
                GazeStacks = MaxGazeStacks;
            }
        }

        private void TriggerEcho()
        {
            EchoCount++;

            if (LastTarget == null)
            {
                return;
            }

            var result = new EchoResult
            {
                TargetName = LastTarget.Name,
                TargetHadEgoSet = LastTarget.HasEgoSet,
                SoulDamage = EchoSoulDamage,
                LightShadowStacksAfter = LastTarget.LightShadowStacks,
                EchoCount = EchoCount,
            };

            if (LastTarget.HasEgoSet && LastTarget.LightShadowStacks > 0)
            {
                LastTarget.LightShadowStacks--;
                result.LightShadowRemoved = true;
                result.LightShadowStacksAfter = LastTarget.LightShadowStacks;
            }

            LastEcho = result;
            EchoTriggered?.Invoke(result);
        }

        private void ApplyAlarmPulse(int stacks)
        {
            for (int i = 0; i < _employees.Count; i++)
            {
                BossTarget employee = _employees[i];

                if (employee.IsAlive)
                {
                    employee.DesireStacks += stacks;
                }
            }
        }

        private static float ResistanceToMultiplier(float resistance)
        {
            return resistance >= 0f ? resistance : 1f + Math.Abs(resistance);
        }
    }
}
