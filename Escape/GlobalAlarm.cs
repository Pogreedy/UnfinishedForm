using System;

namespace UnfinishedForm.Escape
{
    /// <summary>出逃后持续生效的全设施警报。</summary>
    public sealed class GlobalAlarm
    {
        public const float DesirePulseIntervalSeconds = 60f;
        public const int DesireStacksPerPulse = 1;
        public const float WorkSuccessRatePenalty = -0.10f;
        public const float EnergyCollectionRatePenalty = -0.30f;

        /// <summary>警报是否正在生效。</summary>
        public bool IsActive { get; private set; }

        /// <summary>当前警报周期内已累计的秒数。</summary>
        public float ElapsedSeconds { get; private set; }

        /// <summary>已经发出的渴望脉冲次数。</summary>
        public int PulseCount { get; private set; }

        /// <summary>每次脉冲触发，参数为全设施员工应增加的「渴望」层数。</summary>
        public event Action<int> DesirePulse;

        /// <summary>警报生效时工作成功率修正。</summary>
        public float WorkSuccessRateModifier => IsActive ? WorkSuccessRatePenalty : 0f;

        /// <summary>警报生效时能量收集效率修正。</summary>
        public float EnergyCollectionRateModifier => IsActive ? EnergyCollectionRatePenalty : 0f;

        /// <summary>启动警报，并从零开始计时。</summary>
        public void Activate()
        {
            IsActive = true;
            ElapsedSeconds = 0f;
            PulseCount = 0;
        }

        /// <summary>停止警报，但保留已累计的脉冲次数。</summary>
        public void Deactivate()
        {
            IsActive = false;
        }

        /// <summary>完全复位警报。</summary>
        public void Reset()
        {
            IsActive = false;
            ElapsedSeconds = 0f;
            PulseCount = 0;
        }

        /// <summary>每帧推进警报计时，每满 60 秒发出一次渴望脉冲。</summary>
        public void Tick(float deltaTime)
        {
            if (!IsActive || deltaTime <= 0f)
            {
                return;
            }

            ElapsedSeconds += deltaTime;

            while (ElapsedSeconds >= DesirePulseIntervalSeconds)
            {
                ElapsedSeconds -= DesirePulseIntervalSeconds;
                PulseCount++;
                DesirePulse?.Invoke(DesireStacksPerPulse);
            }
        }
    }
}
