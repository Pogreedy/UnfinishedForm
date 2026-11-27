using System;

namespace UnfinishedForm.Escape
{
    /// <summary>
    /// 出逃状态机骨架。状态迁移条件、计时与事件载荷均未实现。
    /// </summary>
    public sealed class EscapeStateMachine
    {
        /// <summary>当前出逃状态。</summary>
        public EscapeState CurrentState { get; private set; } = EscapeState.Contained;

        /// <summary>当前状态已持续的秒数。</summary>
        public float StateElapsedSeconds { get; private set; }

        /// <summary>状态变化通知：(旧状态, 新状态)。</summary>
        public event Action<EscapeState, EscapeState> StateChanged;

        /// <summary>每帧推进出逃计时与条件检查。</summary>
        public void Tick(float deltaTime)
        {
            // TODO: 累加 StateElapsedSeconds，并检查出逃/镇压条件。
        }

        /// <summary>请求开始出逃。</summary>
        public void RequestEscape()
        {
            // TODO: 从 Contained 进入 Escaping，并触发对应表现。
        }

        /// <summary>通知异想体已被镇压。</summary>
        public void NotifySuppressed()
        {
            // TODO: 进入 Suppressed，停止出逃计时。
        }

        /// <summary>复位到收容状态。</summary>
        public void Reset()
        {
            // TODO: 回到 Contained 并清空计时。
        }

        private void TransitionTo(EscapeState nextState)
        {
            if (CurrentState == nextState)
            {
                return;
            }

            EscapeState previousState = CurrentState;
            CurrentState = nextState;
            StateElapsedSeconds = 0f;
            StateChanged?.Invoke(previousState, nextState);
        }
    }
}
