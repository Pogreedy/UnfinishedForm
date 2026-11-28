using System.Collections.Generic;
using UnityEngine;

namespace UnfinishedForm.BaseMod
{
    public enum BossPhase
    {
        None = 0,
        PhaseOne = 1,
        PhaseTwo = 2,
        PhaseThree = 3,
        PhaseFour = 4,
    }

    /// <summary>
    /// 未竟之形 Boss 运行时：原地 Boss 战，阶段伤害/减伤/护盾和测试快捷键。
    /// 目前不使用独立寻路单位，逃逸后仍绑定原来的 CreatureModel。
    /// </summary>
    public static class BossRuntime
    {
        public const float BossMaxHp = 1800f;
        public const float PhaseOneReflectionInterval = 12f;
        public const float PhaseTwoPulseInterval = 10f;
        public const float PhaseThreePulseInterval = 8f;
        public const float ReflectionMentalDamage = 40f;
        public const float PhantomSoulDamage = 30f;
        public const float PhaseFourMaxShield = 2000f;

        private static UnfinishedFormCreature _script;
        private static CreatureModel _model;
        private static bool _active;
        private static BossPhase _phase = BossPhase.None;
        private static float _phaseTimer;
        private static float _shield;
        private static readonly HashSet<string> _belonging = new HashSet<string>();

        public static BossPhase Phase => _phase;
        public static bool Active => _active;
        public static CreatureModel Model => _model;
        public static float Shield => _shield;

        public static void Bind(UnfinishedFormCreature script)
        {
            _script = script;
            if (script != null)
            {
                _model = script.model;
            }
        }

        public static void Begin()
        {
            if (_model == null)
            {
                Debug.LogError("[UnfinishedForm] BossRuntime 没有绑定模型。");
                return;
            }

            if (_active)
            {
                return;
            }

            _active = true;
            _phase = BossPhase.PhaseOne;
            _phaseTimer = 0f;
            _shield = 0f;
            _belonging.Clear();
            Debug.Log("[UnfinishedForm] Boss 第 1 阶段开始：摹仿·反照（原地战斗）。");
        }

        public static void Tick(float deltaTime)
        {
            if (!_active || _model == null || deltaTime <= 0f)
            {
                return;
            }

            _phaseTimer += deltaTime;
            CheckPhaseByHp();

            switch (_phase)
            {
                case BossPhase.PhaseOne:
                    if (_phaseTimer >= PhaseOneReflectionInterval)
                    {
                        _phaseTimer -= PhaseOneReflectionInterval;
                        TriggerReflectionPulse();
                    }
                    break;

                case BossPhase.PhaseTwo:
                    if (_phaseTimer >= PhaseTwoPulseInterval)
                    {
                        _phaseTimer -= PhaseTwoPulseInterval;
                        TriggerDesirePulse();
                    }
                    break;

                case BossPhase.PhaseThree:
                    if (_phaseTimer >= PhaseThreePulseInterval)
                    {
                        _phaseTimer -= PhaseThreePulseInterval;
                        TriggerPhantomPulse();
                    }
                    break;

                case BossPhase.PhaseFour:
                    break;
            }
        }

        public static bool BeforeCreatureDamage(CreatureModel __instance, DamageInfo dmg)
        {
            if (!_active || _model == null || dmg == null || !ReferenceEquals(__instance, _model))
            {
                return true;
            }

            float multiplier = 1f;

            if (_phase == BossPhase.PhaseTwo)
            {
                multiplier = 0.65f;
            }
            else if (_phase == BossPhase.PhaseThree)
            {
                multiplier = 0.8f;
            }
            else if (_phase == BossPhase.PhaseFour && _shield > 0f)
            {
                // 占位护盾：每层护盾最多吸收 100 点伤害。
                float incoming = Mathf.Max(0f, dmg.min);
                float absorbed = Mathf.Min(_shield, incoming);
                _shield -= absorbed;

                if (incoming > 0f)
                {
                    multiplier = Mathf.Max(0f, 1f - absorbed / incoming);
                }
            }

            dmg.min *= multiplier;
            dmg.max *= multiplier;
            return true;
        }

        public static void CheckPhaseByHp()
        {
            if (_model == null)
            {
                return;
            }

            float ratio = _model.hp / BossMaxHp;

            if (_phase == BossPhase.PhaseOne && ratio <= 0.5f)
            {
                SetPhase(BossPhase.PhaseTwo);
            }
            else if (_phase == BossPhase.PhaseTwo && ratio <= 0.2f)
            {
                SetPhase(BossPhase.PhaseThree);
            }
            else if (_phase == BossPhase.PhaseThree && ratio <= 0.3f)
            {
                SetPhase(BossPhase.PhaseFour);
            }
        }

        public static void SetPhase(BossPhase phase)
        {
            if (_model == null)
            {
                return;
            }

            _phase = phase;
            _phaseTimer = 0f;

            if (phase == BossPhase.PhaseTwo)
            {
                Debug.Log("[UnfinishedForm] Boss 第 2 阶段：茧中梦（渴望/归属占位）。");
            }
            else if (phase == BossPhase.PhaseThree)
            {
                Debug.Log("[UnfinishedForm] Boss 第 3 阶段：残影 / 我不是你。");
            }
            else if (phase == BossPhase.PhaseFour)
            {
                _shield = PhaseFourMaxShield;
                Debug.Log("[UnfinishedForm] Boss 第 4 阶段：光影反噬，护盾=" + _shield + "。");
            }
            else
            {
                Debug.Log("[UnfinishedForm] Boss 第 1 阶段：摹仿·反照。");
            }
        }

        public static void ForceEscape()
        {
            if (_model == null)
            {
                Debug.LogError("[UnfinishedForm] 找不到未竟之形模型。");
                return;
            }

            _model.SetQliphothCounter(0);
            _model.Escape();
            Begin();
            Debug.Log("[UnfinishedForm] F6：已强制出逃。");
        }

        public static void ForcePhaseFour()
        {
            ForcePhase(BossPhase.PhaseFour);
            Debug.Log("[UnfinishedForm] 已跳第 4 阶段。");
        }

        public static void ForcePhase(BossPhase phase)
        {
            if (_model == null)
            {
                return;
            }

            _active = true;
            _phase = phase;
            _phaseTimer = 0f;

            if (phase == BossPhase.PhaseFour)
            {
                _model.hp = BossMaxHp * 0.2f;
                _shield = PhaseFourMaxShield;
            }

            SetPhase(phase);
            Debug.Log("[UnfinishedForm] 已强制进入 Boss 阶段 " + (int)phase + "。");
        }

        public static void TestDamage(float amount)
        {
            if (_model == null)
            {
                return;
            }

            _model.hp = Mathf.Max(0f, _model.hp - amount);
            CheckPhaseByHp();
            Debug.Log("[UnfinishedForm] Boss 受到 " + amount + " 测试伤害，当前 HP=" + _model.hp);
        }

        public static void TestDamage()
        {
            TestDamage(300f);
        }

        public static void ResetTest()
        {
            _active = false;
            _phase = BossPhase.None;
            _shield = 0f;
            _phaseTimer = 0f;
            _belonging.Clear();

            if (_model != null)
            {
                _model.hp = BossMaxHp;
            }

            Debug.Log("[UnfinishedForm] F9：Boss 测试状态已复位。");
        }

        private static void TriggerReflectionPulse()
        {
            Debug.Log("[UnfinishedForm] 第1阶段反照脉冲（占位：对全设施员工施加精神伤害）。");
            DamageAllAgents(ReflectionMentalDamage);
        }

        private static void TriggerDesirePulse()
        {
            Debug.Log("[UnfinishedForm] 第2阶段渴望脉冲（占位：累积渴望并标记归属）。");
            DamageAllAgents(10f);
            _belonging.Add("placeholder");
        }

        private static void TriggerPhantomPulse()
        {
            Debug.Log("[UnfinishedForm] 第3阶段残影脉冲（占位：对全设施员工施加灵魂伤害）。");
            DamageAllAgents(PhantomSoulDamage);
        }

        private static void DamageAllAgents(float damage)
        {
            AgentManager manager = AgentManager.instance;
            if (manager == null)
            {
                return;
            }

            IList<AgentModel> agents = manager.GetAgentList();
            for (int i = 0; i < agents.Count; i++)
            {
                AgentModel agent = agents[i];
                if (agent != null && agent.hp > 0f)
                {
                    agent.TakeMentalDamage(damage);
                }
            }
        }
    }
}
