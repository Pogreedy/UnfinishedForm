using System.Collections.Generic;

namespace UnfinishedForm.EGO
{
    /// <summary>
    /// 套装注册与联动查询入口。三件套匹配与加成判定均未实现。
    /// </summary>
    public sealed class EgoSetManager
    {
        private readonly Dictionary<string, EgoSetDefinition> _sets =
            new Dictionary<string, EgoSetDefinition>();

        /// <summary>已注册的套装。</summary>
        public IReadOnlyDictionary<string, EgoSetDefinition> Sets => _sets;

        /// <summary>注册一个 EGO 套装。</summary>
        public void Register(EgoSetDefinition set)
        {
            // TODO: 校验三件套 Id、件数需求与重复注册。
        }

        /// <summary>按套装 Id 查询。</summary>
        public bool TryGet(string setId, out EgoSetDefinition set)
        {
            // TODO
            set = null;
            return false;
        }

        /// <summary>返回当前装备组合对应的生效套装。</summary>
        public EgoSetDefinition ResolveActiveSet(EgoLoadout loadout)
        {
            // TODO: 判断武器/护甲/饰品是否同属一个套装。
            return null;
        }

        /// <summary>返回当前装备组合已满足件数条件的套装加成。</summary>
        public IReadOnlyList<EgoSetBonusDefinition> ResolveActiveBonuses(EgoLoadout loadout)
        {
            // TODO
            return null;
        }
    }
}
