using UnityEngine;

namespace UnfinishedForm.BaseMod
{
    /// <summary>
    /// 「未竟之形」的 BaseMod 脚本。
    /// 继承 DummyCreature 只是为了获得可注册的最小实现；
    /// 必须覆盖教程用的强制成功/失败，否则每次工作都会打满 PE-BOX。
    /// </summary>
    public sealed class UnfinishedFormCreature : DummyCreature
    {
        public UnfinishedFormCreature()
        {
        }

        public override bool ForcelySuccess(UseSkill skill)
        {
            return false;
        }

        public override bool ForcelyFail(UseSkill skill)
        {
            return false;
        }

        public override void OnFinishWork(UseSkill skill)
        {
            base.OnFinishWork(skill);

            if (skill == null || model == null)
            {
                return;
            }

            try
            {
                UnityEngine.Debug.Log("[UnfinishedForm] OnFinishWork 进入自定义脚本。");
                ContainmentRuntime.OnFinishWork(this, skill);

                // 压迫/审判完成后直接补齐观察信息，避免 work_p 因单次 PE 不足而漏掉。
                if (skill.skillTypeInfo != null
                    && skill.skillTypeInfo.rwbpType == RwbpType.P
                    && model.observeInfo != null)
                {
                    model.observeInfo.ObserveAll(new string[0]);
                    model.OnObservationLevelChanged();
                    Debug.Log("[UnfinishedForm] 压迫完成：已补齐观察区域并刷新观察等级。");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("[UnfinishedForm] OnFinishWork 异常: " + ex);
            }
        }
    }
}
