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
    /// 未竟之形 Boss 的最小运行时。先负责出逃、阶段、测试快捷键；
    /// 具体技能随阶段逐步接入。
    /// </summary>
    public static class BossRuntime
    {
        public const float BossMaxHp = 1800f;

        private static UnfinishedFormCreature _script;
        private static CreatureModel _model;
        private static bool _active;
        private static BossPhase _phase = BossPhase.None;
        private static float _lastLoggedHp;

        public static BossPhase Phase => _phase;
        public static bool Active => _active;
        public static CreatureModel Model => _model;

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
            _lastLoggedHp = _model.hp;
            Debug.Log("[UnfinishedForm] Boss 第 1 阶段开始。");
        }

        public static void Tick()
        {
            if (!_active || _model == null)
            {
                return;
            }

            float ratio = BossMaxHp <= 0f ? 0f : _model.hp / BossMaxHp;
            BossPhase next = _phase;

            if (_phase == BossPhase.PhaseOne && ratio <= 0.5f)
            {
                next = BossPhase.PhaseTwo;
            }
            else if (_phase == BossPhase.PhaseTwo && ratio <= 0.2f)
            {
                next = BossPhase.PhaseThree;
            }
            else if (_phase == BossPhase.PhaseThree && ratio <= 0.3f)
            {
                next = BossPhase.PhaseFour;
            }

            if (next != _phase)
            {
                SetPhase(next);
            }
        }

        public static void SetPhase(BossPhase phase)
        {
            if (_model == null)
            {
                return;
            }

            _phase = phase;
            switch (phase)
            {
                case BossPhase.PhaseTwo:
                    Debug.Log("[UnfinishedForm] Boss 第 2 阶段：茧中梦。");
                    break;
                case BossPhase.PhaseThree:
                    Debug.Log("[UnfinishedForm] Boss 第 3 阶段：残影 / 我不是你。");
                    break;
                case BossPhase.PhaseFour:
                    Debug.Log("[UnfinishedForm] Boss 第 4 阶段：光影反噬，护盾测试阶段。");
                    break;
                default:
                    Debug.Log("[UnfinishedForm] Boss 第 1 阶段：摹仿·反照。");
                    break;
            }
        }

        /// <summary>F6：直接把计数器清零并触发出逃。</summary>
        public static void ForceEscape()
        {
            if (_model == null)
            {
                Debug.LogError("[UnfinishedForm] 找不到未竟之形模型，无法触发出逃。");
                return;
            }

            _model.SetQliphothCounter(0);
            _model.Escape();
            Begin();
            Debug.Log("[UnfinishedForm] F6：已强制出逃。");
        }

        /// <summary>F7：测试用，直接进入第四阶段。</summary>
        public static void ForcePhaseFour()
        {
            if (_model == null)
            {
                return;
            }

            _model.hp = BossMaxHp * 0.2f;
            _active = true;
            SetPhase(BossPhase.PhaseFour);
            Debug.Log("[UnfinishedForm] F7：已跳到第 4 阶段。");
        }

        /// <summary>F8：测试用，给 Boss 一次固定伤害。</summary>
        public static void TestDamage()
        {
            if (_model == null)
            {
                return;
            }

            _model.hp = Mathf.Max(0f, _model.hp - 300f);
            Tick();
            Debug.Log("[UnfinishedForm] F8：Boss 受到 300 测试伤害，当前 HP=" + _model.hp);
        }

        /// <summary>F9：测试用，复位阶段。</summary>
        public static void ResetTest()
        {
            _active = false;
            _phase = BossPhase.None;
            if (_model != null)
            {
                _model.hp = BossMaxHp;
            }
            Debug.Log("[UnfinishedForm] F9：Boss 测试状态已复位。");
        }
    }
}
