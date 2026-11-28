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
    }
}
