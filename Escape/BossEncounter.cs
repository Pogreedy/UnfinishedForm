using System;
using System.Collections.Generic;
using UnfinishedForm.Containment;

namespace UnfinishedForm.Escape
{
    /// <summary>
    /// 「未竟之形」出逃战的第一阶段：摹仿·反照。
    /// 负责出逃触发、全局警报、反照判定、未竟回响与阶段切换。
    /// </summary>
    public sealed class BossEncounter
    {
        public const string PhaseOneName = "摹仿·反照";

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

        private readonly List<BossTarget> _employees = new List<BossTarget>();
        private float _echoTimer;

        public BossEncounter(ContainmentManager containment)
        {
            Escape = new EscapeStateMachine();
            Phase = new BossPhaseStateMachine();
            Alarm = new GlobalAlarm();
            Alarm.DesirePulse += ApplyAlarmPulse;

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

        public float Hp { get; private set; } = MaxHp;

        public int GazeStacks { get; private set; }

        public int ReflectionCount { get; private set; }

        public int EchoCount { get; private set; }

        public BossTarget LastTarget { get; private set; }

        public EchoResult LastEcho { get; private set; }

        /// <summary>从非套装员工处复制的最高属性累计值。</summary>
        public float CopiedPowerBonus { get; private set; }

        public string LastCopiedStatName { get; private set; }

        public float LastCopiedStatValue { get; private set; }

        public bool IsActive => Escape.CurrentState == EscapeState.Escaped;

        public bool IsPhaseOne => Phase.CurrentPhase == BossPhase.PhaseOne;

        public IReadOnlyList<BossTarget> TrackedEmployees => _employees;

        public event Action<ReflectionResult> ReflectionResolved;

        public event Action<EchoResult> EchoTriggered;

        /// <summary>把员工加入全设施名单，警报脉冲会自动给其增加「渴望」。</summary>
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

        /// <summary>每帧推进出逃、警报与未竟回响计时。</summary>
        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            Escape.Tick(deltaTime);
            Alarm.Tick(deltaTime);

            if (!IsActive || !IsPhaseOne)
            {
                return;
            }

            _echoTimer += deltaTime;

            while (_echoTimer >= EchoIntervalSeconds)
            {
                _echoTimer -= EchoIntervalSeconds;
                TriggerEcho();
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

        /// <summary>按抗性倍率对 Boss 造成伤害，并检查 50% 阶段阈值。</summary>
        public float ApplyDamage(DamageType damageType, float amount)
        {
            if (!IsActive || amount <= 0f)
            {
                return 0f;
            }

            float actualDamage = amount * GetResistance(damageType);
            Hp = Math.Max(0f, Hp - actualDamage);
            Phase.NotifyHpChanged(Hp, MaxHp);
            return actualDamage;
        }

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

        /// <summary>镇压后结束出逃并停止全局警报。</summary>
        public void EndEscape()
        {
            Escape.NotifySuppressed();
            Alarm.Deactivate();
        }

        /// <summary>复位 Boss 与出逃状态，保留已登记的员工名单。</summary>
        public void Reset()
        {
            ResetState();
        }

        private void ResetState()
        {
            Escape.Reset();
            Alarm.Reset();
            Phase.Reset();

            Hp = MaxHp;
            GazeStacks = 0;
            ReflectionCount = 0;
            EchoCount = 0;
            LastTarget = null;
            LastEcho = null;
            CopiedPowerBonus = 0f;
            LastCopiedStatName = null;
            LastCopiedStatValue = 0f;
            _echoTimer = 0f;
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
                _employees[i].DesireStacks += stacks;
            }
        }
    }
}
