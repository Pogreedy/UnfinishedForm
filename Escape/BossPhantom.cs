using System;

namespace UnfinishedForm.Escape
{
    /// <summary>第三阶段「面孔投射」生成的一个残影分身。</summary>
    public sealed class BossPhantom
    {
        public const float MaxHpValue = 300f;

        public BossPhantom(int id)
        {
            Id = id;
            Hp = MaxHpValue;
        }

        public int Id { get; }

        public float Hp { get; private set; }

        public float MaxHp => MaxHpValue;

        public bool IsAlive => Hp > 0f;

        public float TakeDamage(float amount)
        {
            if (!IsAlive || amount <= 0f)
            {
                return 0f;
            }

            float previousHp = Hp;
            Hp = Math.Max(0f, Hp - amount);
            return previousHp - Hp;
        }
    }
}
