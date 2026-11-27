using System;
using System.Collections.Generic;

namespace UnfinishedForm.Containment
{
    /// <summary>
    /// 「未竟之形」的收容逻辑。当前只包含计数器、静默饱和、审判与恐慌规则；
    /// 出逃后的 Boss 战由 Escape 模块负责，本类只保留 OnEscapeTriggered 入口。
    /// </summary>
    public sealed class ContainmentManager
    {
        public const string AbnormalityId = "unfinished-form";
        public const string AbnormalityName = "未竟之形";

        /// <summary>逆卡巴拉计数器极值。</summary>
        public const int QliphothCounterMax = 5;

        /// <summary>每日开始时的计数器值。</summary>
        public const int DailyStartCounter = 4;

        /// <summary>审判工作直接判为「差」的概率。</summary>
        public const double JudgmentBadChance = 0.9d;

        /// <summary>审判工作的 PE-BOX 产量。</summary>
        public const int JudgmentPeBoxes = 1;

        /// <summary>「被定义」的持续时间（秒）。</summary>
        public const float DefinedDurationSeconds = 60f;

        /// <summary>「被定义」的精神抗性修正。</summary>
        public const float DefinedMentalResistanceDelta = -0.2f;

        /// <summary>「未竟回响」的精神伤害。</summary>
        public const float EchoMentalDamage = 10f;

        private readonly HashSet<string> _processedPanicEventIds = new HashSet<string>();
        private readonly Random _random;

        public ContainmentManager()
            : this(new Random())
        {
        }

        public ContainmentManager(Random random)
        {
            _random = random ?? new Random();
        }

        /// <summary>当前逆卡巴拉计数器。</summary>
        public int Counter { get; private set; } = DailyStartCounter;

        /// <summary>当天是否处于「静默饱和」。</summary>
        public bool IsSilentSaturated { get; private set; }

        /// <summary>当天是否完成过至少一次「优」级本能工作。</summary>
        public bool HasGoodInstinctToday { get; private set; }

        /// <summary>计数器是否曾归零并触发过出逃入口。</summary>
        public bool EscapeTriggered { get; private set; }

        /// <summary>
        /// 执行一次工作。普通工作必须传入 result；审判工作会忽略 result 并内部掷骰。
        /// 返回 null 表示本次工作不被接受（饱和、已归零、Id 不匹配或参数缺失）。
        /// </summary>
        public WorkRecord PerformWork(
            string abnormalityId,
            string agentName,
            WorkType workType,
            WorkResult? result = null,
            int peBoxes = 0)
        {
            if (!string.Equals(abnormalityId, AbnormalityId, StringComparison.Ordinal))
            {
                return null;
            }

            if (Counter <= 0 || IsSilentSaturated)
            {
                return null;
            }

            if (workType == WorkType.Judgment)
            {
                return PerformJudgment(agentName);
            }

            if (result == null || !IsBasicWork(workType))
            {
                return null;
            }

            bool escapedBefore = EscapeTriggered;
            WorkResult finalResult = result.Value;
            var record = new WorkRecord
            {
                AbnormalityId = AbnormalityId,
                AgentName = agentName,
                WorkType = workType,
                Result = finalResult,
                PeBoxes = peBoxes,
                CounterBefore = Counter,
            };

            switch (finalResult)
            {
                case WorkResult.Bad:
                    DecreaseCounter();
                    break;

                case WorkResult.Good:
                    if (workType == WorkType.Insight || workType == WorkType.Attachment)
                    {
                        IncreaseCounter();
                    }
                    else if (workType == WorkType.Instinct)
                    {
                        HasGoodInstinctToday = true;
                    }
                    break;
            }

            record.CounterAfter = Counter;
            record.TriggeredEscape = !escapedBefore && EscapeTriggered;
            return record;
        }

        /// <summary>
        /// 通知「其他异想体」收容单元内发生了一次员工恐慌。
        /// 同一 panicEventId 当天只处理一次；未竟之形自己的收容单元不计数。
        /// </summary>
        public void NotifyOtherUnitPanic(string sourceAbnormalityId, string panicEventId)
        {
            if (string.Equals(sourceAbnormalityId, AbnormalityId, StringComparison.Ordinal))
            {
                return;
            }

            if (string.IsNullOrEmpty(panicEventId) || !_processedPanicEventIds.Add(panicEventId))
            {
                return;
            }

            if (HasGoodInstinctToday || IsSilentSaturated || Counter <= 0)
            {
                return;
            }

            DecreaseCounter();
        }

        /// <summary>开始新的一天：计数器回到 4，并清空全部“当天”状态。</summary>
        public void StartNewDay()
        {
            Counter = DailyStartCounter;
            IsSilentSaturated = false;
            HasGoodInstinctToday = false;
            EscapeTriggered = false;
            _processedPanicEventIds.Clear();
        }

        private WorkRecord PerformJudgment(string agentName)
        {
            bool escapedBefore = EscapeTriggered;
            var record = new WorkRecord
            {
                AbnormalityId = AbnormalityId,
                AgentName = agentName,
                WorkType = WorkType.Judgment,
                PeBoxes = JudgmentPeBoxes,
                CounterBefore = Counter,
                MentalResistanceDelta = DefinedMentalResistanceDelta,
                MentalRecoveryBlockSeconds = DefinedDurationSeconds,
            };

            // 审判开始时立即 -1，随后再按结果结算。
            DecreaseCounter();

            WorkResult result = _random.NextDouble() < JudgmentBadChance
                ? WorkResult.Bad
                : WorkResult.Normal;

            record.Result = result;

            if (result == WorkResult.Bad)
            {
                record.MentalDamage = EchoMentalDamage;
                record.DesireStacks = 1;
                DecreaseCounter();
            }

            record.CounterAfter = Counter;
            record.TriggeredEscape = !escapedBefore && EscapeTriggered;
            return record;
        }

        private static bool IsBasicWork(WorkType workType)
        {
            return workType == WorkType.Instinct
                || workType == WorkType.Insight
                || workType == WorkType.Attachment
                || workType == WorkType.Repression;
        }

        private void IncreaseCounter()
        {
            if (IsSilentSaturated || Counter >= QliphothCounterMax)
            {
                return;
            }

            Counter++;

            if (Counter == QliphothCounterMax)
            {
                IsSilentSaturated = true;
            }
        }

        private void DecreaseCounter()
        {
            if (IsSilentSaturated || Counter <= 0)
            {
                return;
            }

            Counter--;

            if (Counter == 0 && !EscapeTriggered)
            {
                EscapeTriggered = true;
                OnEscapeTriggered();
            }
        }

        /// <summary>计数器归零时的出逃事件入口。Boss 战不在本模块实现。</summary>
        private void OnEscapeTriggered()
        {
            // 出逃与 Boss 阶段由 Escape 模块接管；此处只保留入口。
        }
    }
}
