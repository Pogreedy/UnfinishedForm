using System;

namespace UnfinishedForm.Escape
{
    /// <summary>
    /// Boss 阶段状态机。第一阶段在完成 3 次反照判定，
    /// 或生命值降至 50% 及以下时进入第二阶段。
    /// </summary>
    public sealed class BossPhaseStateMachine
    {
        /// <summary>进入第二阶段所需的完整反照判定次数。</summary>
        public const int ReflectionsForPhaseTwo = 3;

        /// <summary>进入第二阶段的生命值比例。</summary>
        public const float PhaseTwoHpRatio = 0.5f;

        /// <summary>当前 Boss 阶段。</summary>
        public BossPhase CurrentPhase { get; private set; } = BossPhase.None;

        /// <summary>阶段变化通知：(旧阶段, 新阶段)。</summary>
        public event Action<BossPhase, BossPhase> PhaseChanged;

        /// <summary>阶段切换由反照计数与生命值事件驱动，无独立计时。</summary>
        public void Tick(float deltaTime)
        {
        }

        /// <summary>生命值变化时检查 50% 阈值。</summary>
        public void NotifyHpChanged(float currentHp, float maxHp)
        {
            if (CurrentPhase != BossPhase.PhaseOne || maxHp <= 0f)
            {
                return;
            }

            if (currentHp / maxHp <= PhaseTwoHpRatio)
            {
                EnterPhase(BossPhase.PhaseTwo);
            }
        }

        /// <summary>反照判定完成后检查 3 次阈值。</summary>
        public void NotifyReflectionCount(int reflectionCount)
        {
            if (CurrentPhase == BossPhase.PhaseOne && reflectionCount >= ReflectionsForPhaseTwo)
            {
                EnterPhase(BossPhase.PhaseTwo);
            }
        }

        /// <summary>茧的生命值归零后进入第三阶段。第三阶段技能暂未实现。</summary>
        public void NotifyCocoonBroken()
        {
            if (CurrentPhase == BossPhase.PhaseTwo)
            {
                EnterPhase(BossPhase.PhaseThree);
            }
        }

        /// <summary>直接进入指定阶段（出逃初始化或调试使用）。</summary>
        public void EnterPhase(BossPhase phase)
        {
            if (CurrentPhase == phase)
            {
                return;
            }

            BossPhase previousPhase = CurrentPhase;
            CurrentPhase = phase;
            PhaseChanged?.Invoke(previousPhase, phase);
        }

        /// <summary>从第一阶段开始。</summary>
        public void StartPhaseOne()
        {
            EnterPhase(BossPhase.PhaseOne);
        }

        /// <summary>复位到未进入 Boss 阶段。</summary>
        public void Reset()
        {
            EnterPhase(BossPhase.None);
        }
    }
}
