using System.Collections.Generic;
using UnityEngine;

namespace UnfinishedForm.BaseMod
{
    /// <summary>
    /// BaseMod 侧的收容逻辑桥接：把游戏内工作结果接到「未竟之形」的规则上。
    /// 目前接入：计数增减、审判(压迫)立即减计数、审判差的精神伤害、以及本能优后的恐慌免疫。
    /// </summary>
    public static class ContainmentRuntime
    {
        private const long UnfinishedFormId = 29902L;
        private const float EchoMentalDamage = 10f;

        /// <summary>当天完成过“优”级本能工作的员工，恐慌入口会被拦下。</summary>
        private static readonly HashSet<long> PanicImmuneAgents = new HashSet<long>();

        public static void OnStageStart()
        {
            PanicImmuneAgents.Clear();
        }

        public static void OnFinishWork(CreatureBase __instance, UseSkill skill)
        {
            if (__instance == null || __instance.model == null || skill == null)
            {
                return;
            }

            CreatureModel model = __instance.model;
            if (model.metaInfo == null || model.metaInfo.id != UnfinishedFormId)
            {
                return;
            }

            CreatureFeelingState state = skill.GetCurrentFeelingState();
            if (state == CreatureFeelingState.NONE)
            {
                return;
            }

            AgentModel agent = skill.agent;
            RwbpType workType = skill.skillTypeInfo != null
                ? skill.skillTypeInfo.rwbpType
                : RwbpType.N;

            // 当前版本把“压迫”当作审判：审判无论结果如何都立即 -1。
            if (workType == RwbpType.P)
            {
                // 压迫/审判工作完成即视为观察到 work_p，避免因单次 PE 较少而卡在观察等级 3。
                if (model.observeInfo != null)
                {
                    model.observeInfo.OnObserveRegion("work_p");
                }

                model.SubQliphothCounter();

                if (state == CreatureFeelingState.BAD)
                {
                    model.SubQliphothCounter();
                    if (agent != null)
                    {
                        agent.TakeMentalDamage(EchoMentalDamage);
                    }
                    Debug.Log("[UnfinishedForm] 审判差：触发未竟回响，额外减少计数器并造成精神伤害。");
                }

                return;
            }

            if (state == CreatureFeelingState.BAD)
            {
                model.SubQliphothCounter();
                return;
            }

            if (state != CreatureFeelingState.GOOD || agent == null)
            {
                return;
            }

            if (workType == RwbpType.W || workType == RwbpType.B)
            {
                model.AddQliphothCounter();
            }
            else if (workType == RwbpType.R)
            {
                PanicImmuneAgents.Add(agent.instanceId);
                Debug.Log("[UnfinishedForm] 本能优：该员工当天免疫恐慌。");
            }
        }

        /// <summary>拦截已获得“本能优”免疫的员工的恐慌入口。</summary>
        public static bool SuppressPanic(AgentModel __instance)
        {
            return __instance != null && PanicImmuneAgents.Contains(__instance.instanceId);
        }
    }
}
