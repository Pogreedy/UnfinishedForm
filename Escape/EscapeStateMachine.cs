using System;

namespace UnfinishedForm.Escape
{
    /// <summary>
    /// 出逃状态机。当前实现收容中、出逃中、已出逃与已镇压之间的直接切换。
    /// </summary>
    public sealed class EscapeStateMachine
    {
        /// <summary>当前出逃状态。</summary>
        public EscapeState CurrentState { get; private set; } = EscapeState.Contained;

        /// <summary>当前状态已持续的秒数。</summary>
        public float StateElapsedSeconds { get; private set; }

        /// <summary>是否已经处于出逃状态。</summary>
        public bool IsEscaped => CurrentState == EscapeState.Escaped;

        /// <summary>状态变化通知：(旧状态, 新状态)。</summary>
        public event Action<EscapeState, EscapeState> StateChanged;

        /// <summary>每帧累加当前状态计时。</summary>
        public void Tick(float deltaTime)
        {
            if (deltaTime > 0f)
            {
                StateElapsedSeconds += deltaTime;
            }
        }

        /// <summary>请求开始出逃：收容中 → 出逃中 → 已出逃。</summary>
        public void RequestEscape()
        {
            if (CurrentState == EscapeState.Escaped)
            {
                return;
            }

            TransitionTo(EscapeState.Escaping);
            TransitionTo(EscapeState.Escaped);
        }

        /// <summary>通知异想体已被镇压。</summary>
        public void NotifySuppressed()
        {
            if (CurrentState != EscapeState.Escaping && CurrentState != EscapeState.Escaped)
            {
                return;
            }

            TransitionTo(EscapeState.Suppressed);
        }

        /// <summary>复位到收容状态。</summary>
        public void Reset()
        {
            TransitionTo(EscapeState.Contained);
            StateElapsedSeconds = 0f;
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
