using System;

namespace UnfinishedForm.EGO
{
    /// <summary>
    /// 单件 EGO 的静态数据，对应 Config/ego.json 中的一条记录。
    /// </summary>
    [Serializable]
    public sealed class EgoItemDefinition
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public EgoItemType Type { get; set; }

        /// <summary>所属套装 Id；为空表示不属于任何套装。</summary>
        public string SetId { get; set; }

        /// <summary>品级，用于排序或数值成长。</summary>
        public int Tier { get; set; }
    }
}
