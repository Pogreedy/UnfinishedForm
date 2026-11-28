using System;
using System.Collections.Generic;

namespace UnfinishedForm.EGO
{
    /// <summary>
    /// 套装注册与三件套匹配入口。
    /// </summary>
    public sealed class EgoSetManager
    {
        private readonly Dictionary<string, EgoSetDefinition> _sets =
            new Dictionary<string, EgoSetDefinition>();

        /// <summary>已注册的套装。</summary>
        public IReadOnlyDictionary<string, EgoSetDefinition> Sets => _sets;

        /// <summary>注册一个 EGO 套装；同 Id 会覆盖旧定义。</summary>
        public void Register(EgoSetDefinition set)
        {
            if (set == null || string.IsNullOrEmpty(set.Id))
            {
                return;
            }

            _sets[set.Id] = set;
        }

        /// <summary>按套装 Id 查询。</summary>
        public bool TryGet(string setId, out EgoSetDefinition set)
        {
            if (string.IsNullOrEmpty(setId))
            {
                set = null;
                return false;
            }

            return _sets.TryGetValue(setId, out set);
        }

        /// <summary>返回当前装备组合对应的三件套套装。</summary>
        public EgoSetDefinition ResolveActiveSet(EgoLoadout loadout)
        {
            if (loadout == null)
            {
                return null;
            }

            foreach (EgoSetDefinition set in _sets.Values)
            {
                if (ResolvePieceCount(set, loadout) >= 3)
                {
                    return set;
                }
            }

            return null;
        }

        /// <summary>返回当前装备组合已满足件数条件的套装加成。</summary>
        public IReadOnlyList<EgoSetBonusDefinition> ResolveActiveBonuses(EgoLoadout loadout)
        {
            var activeBonuses = new List<EgoSetBonusDefinition>();
            EgoSetDefinition set = ResolveActiveSet(loadout);

            if (set == null)
            {
                return activeBonuses;
            }

            int pieceCount = ResolvePieceCount(set, loadout);

            for (int i = 0; i < set.Bonuses.Count; i++)
            {
                EgoSetBonusDefinition bonus = set.Bonuses[i];

                if (bonus != null && bonus.RequiredPieces <= pieceCount)
                {
                    activeBonuses.Add(bonus);
                }
            }

            return activeBonuses;
        }

        /// <summary>计算某个套装的已装备件数。</summary>
        public int ResolvePieceCount(EgoSetDefinition set, EgoLoadout loadout)
        {
            if (set == null || loadout == null)
            {
                return 0;
            }

            int count = 0;

            if (!string.IsNullOrEmpty(set.WeaponId)
                && string.Equals(set.WeaponId, loadout.WeaponId, StringComparison.Ordinal))
            {
                count++;
            }

            if (!string.IsNullOrEmpty(set.SuitId)
                && string.Equals(set.SuitId, loadout.SuitId, StringComparison.Ordinal))
            {
                count++;
            }

            if (!string.IsNullOrEmpty(set.GiftId)
                && string.Equals(set.GiftId, loadout.GiftId, StringComparison.Ordinal))
            {
                count++;
            }

            return count;
        }
    }
}
