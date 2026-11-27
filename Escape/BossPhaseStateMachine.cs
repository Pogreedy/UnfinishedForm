using System;

namespace UnfinishedForm.Escape
{
    /// <summary>
    /// Boss 阶段状态机骨架。阶段阈值、血量驱动与技能切换均未实现。
    /// </summary>
    public sealed class BossPhaseStateMachine
    {
        /// <summary>当前 Boss 阶段。</summary>
        public BossPhase CurrentPhase { get; private set; } = BossPhase.None;

        /// <summary>阶段变化通知：(旧阶段, 新阶段)。</summary>
        public event Action<BossPhase, BossPhase> PhaseChanged;

        /// <summary>每帧推进阶段计时与条件检查。</summary>
        public void Tick(float deltaTime)
        {
            // TODO: 阶段计时、技能循环或外部条件检查。
        }

        /// <summary>通知当前血量变化，用于按比例推进阶段。</summary>
        public void NotifyHpChanged(float currentHp, float maxHp)
        {
            // TODO: 根据 Config/escape.json 中的血量比例切换阶段。
        }

        /// <summary>直接进入指定阶段（调试或脚本事件使用）。</summary>
        public void EnterPhase(BossPhase phase)
        {
            // TODO: 校验阶段顺序并触发 PhaseChanged。
        }

        /// <summary>复位到未进入 Boss 阶段。</summary>
        public void Reset()
        {
            // TODO
        }

        private void TransitionTo(BossPhase nextPhase)
        {
            if (CurrentPhase == nextPhase)
            {
                return;
            }

            BossPhase previousPhase = CurrentPhase;
            CurrentPhase = nextPhase;
            PhaseChanged?.Invoke(previousPhase, nextPhase);
        }
    }
}
