using System;
using System.Collections.Generic;

namespace UnfinishedForm.Escape
{
    public sealed partial class BossEncounter
    {
        public const float SoulResistanceSwapIntervalSeconds = 20f;
        public const float PhaseThreePhysicalResistance = 0.6f;
        public const float PhaseThreeMentalResistance = 0.3f;
        public const float PhaseThreeErosionResistance = 0.4f;
        public const float PhaseThreeSoulResistanceHigh = 1.0f;
        public const float PhaseThreeSoulResistanceLow = 0.5f;

        public const float GoldenClawIntervalSeconds = 6f;
        public const int GoldenClawMinDamage = 60;
        public const int GoldenClawMaxDamage = 80;
        public const float FaceProjectionIntervalSeconds = 30f;
        public const int PhantomsPerProjection = 3;
        public const float PhantomAttackIntervalSeconds = 3f;
        public const float PhantomAttackDamage = 20f;
        public const float PhantomDamageReduction = 0.5f;

        public const int NotMeDesireThreshold = 5;
        public const float NotMeMentalDamage = 200f;
        public const float LightShadowReflectionDurationSeconds = 5f;
        public const float LightShadowReflectionResistancePenalty = -0.2f;

        public const int AntiManifestGazeCost = 3;
        public const float BelongingReviveSeconds = 5f;

        private readonly List<BossPhantom> _phantoms = new List<BossPhantom>();
        private readonly Dictionary<BossTarget, float> _belongingReviveTimers =
            new Dictionary<BossTarget, float>();
        private readonly bool[] _notMeTriggered = new bool[3];

        private float _soulSwapTimer;
        private float _goldenClawTimer;
        private float _projectionTimer;
        private float _phantomAttackTimer;
        private float _lightShadowReflectionTimer;
        private bool _antiManifestUsed;
        private int _nextPhantomId;
        private int _notMeTriggerCount;

        /// <summary>当前灵魂抗性，在 1.0 与 0.5 之间每 20 秒切换。</summary>
        public float CurrentSoulResistance { get; private set; } = PhaseThreeSoulResistanceHigh;

        public int SoulResistanceSwapCount { get; private set; }

        public bool HasLightShadowReflection => _lightShadowReflectionTimer > 0f;

        public float LightShadowReflectionRemainingSeconds => _lightShadowReflectionTimer;

        public bool IsDefeated => IsPhaseThree && BodyHp <= 0f;

        public int AlivePhantomCount
        {
            get
            {
                int count = 0;

                for (int i = 0; i < _phantoms.Count; i++)
                {
                    if (_phantoms[i].IsAlive)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        public IReadOnlyList<BossPhantom> Phantoms => _phantoms;

        /// <summary>第三阶段本体减伤：有存活分身时 50%，每名存活归属员工再 10%。</summary>
        public float PhaseThreeDamageReduction
        {
            get
            {
                if (!IsPhaseThree)
                {
                    return 0f;
                }

                float reduction = AlivePhantomCount > 0 ? PhantomDamageReduction : 0f;
                reduction += BelongingEmployeeCount * BelongingDamageReductionPerEmployee;
                return Math.Min(1f, reduction);
            }
        }

        /// <summary>存活归属员工会阻挡远程攻击。</summary>
        public bool IsRangedAttackBlocked => IsPhaseThree && BelongingEmployeeCount > 0;

        public int NotMeTriggerCount => _notMeTriggerCount;

        public bool AntiManifestUsedThisPhase => _antiManifestUsed;

        public GoldenClawResult LastGoldenClaw { get; private set; }

        public PhantomAttackResult LastPhantomAttack { get; private set; }

        public NotMeResult LastNotMe { get; private set; }

        public AntiManifestResult LastAntiManifest { get; private set; }

        public event Action<GoldenClawResult> GoldenClawUsed;

        public event Action<IReadOnlyList<BossPhantom>> PhantomsSpawned;

        public event Action<PhantomAttackResult> PhantomAttacked;

        public event Action<NotMeResult> NotMeTriggered;

        public event Action<AntiManifestResult> ManifestationInterrupted;

        public event Action<BossTarget> BelongingRevived;

        /// <summary>第三阶段抗性。光影反照期间全抗性额外 -0.2。</summary>
        public float GetPhaseThreeResistance(DamageType damageType)
        {
            float resistance;

            switch (damageType)
            {
                case DamageType.Physical:
                    resistance = PhaseThreePhysicalResistance;
                    break;

                case DamageType.Mental:
                    resistance = PhaseThreeMentalResistance;
                    break;

                case DamageType.Erosion:
                    resistance = PhaseThreeErosionResistance;
                    break;

                case DamageType.Soul:
                    resistance = CurrentSoulResistance;
                    break;

                default:
                    resistance = 1f;
                    break;
            }

            if (HasLightShadowReflection)
            {
                resistance += LightShadowReflectionResistancePenalty;
            }

            return Math.Max(0f, resistance);
        }

        /// <summary>
        /// 金光爪击：对单个近战目标造成 60–80 物理伤害并附加 1 层渴望。
        /// 返回 null 表示当前不在第三阶段或目标无效。
        /// </summary>
        public GoldenClawResult UseGoldenClaw(BossTarget target)
        {
            if (!IsActive || !IsPhaseThree || IsDefeated || target == null || !target.IsAlive)
            {
                return null;
            }

            TrackEmployee(target);

            int damage = _random.Next(GoldenClawMinDamage, GoldenClawMaxDamage + 1);
            target.DesireStacks += 1;

            var result = new GoldenClawResult
            {
                TargetName = target.Name,
                Damage = damage,
                DesireStacksAfter = target.DesireStacks,
                SoulPenetration = HasLightShadowReflection,
            };

            LastGoldenClaw = result;
            GoldenClawUsed?.Invoke(result);
            return result;
        }

        /// <summary>面孔投射：生成 3 个生命值 300 的残影分身。</summary>
        public IReadOnlyList<BossPhantom> CastFaceProjection()
        {
            if (!IsActive || !IsPhaseThree || IsDefeated)
            {
                return null;
            }

            _phantoms.RemoveAll(phantom => !phantom.IsAlive);

            var spawned = new List<BossPhantom>(PhantomsPerProjection);

            for (int i = 0; i < PhantomsPerProjection; i++)
            {
                var phantom = new BossPhantom(++_nextPhantomId);
                _phantoms.Add(phantom);
                spawned.Add(phantom);
            }

            _phantomAttackTimer = 0f;
            PhantomsSpawned?.Invoke(spawned);
            return spawned;
        }

        /// <summary>分身攻击员工，优先选择穿戴 EGO 套装的员工；归属员工会阻挡远程攻击。</summary>
        public IReadOnlyList<PhantomAttackResult> ResolvePhantomAttacks()
        {
            var results = new List<PhantomAttackResult>();

            if (!IsActive || !IsPhaseThree || IsDefeated)
            {
                return results;
            }

            BossTarget target = FindPhantomTarget();

            if (target == null || AlivePhantomCount <= 0)
            {
                return results;
            }

            bool blocked = IsRangedAttackBlocked;
            bool soulPenetration = HasLightShadowReflection;

            for (int i = 0; i < _phantoms.Count; i++)
            {
                BossPhantom phantom = _phantoms[i];

                if (!phantom.IsAlive)
                {
                    continue;
                }

                var result = new PhantomAttackResult
                {
                    PhantomId = phantom.Id,
                    TargetName = target.Name,
                    Damage = blocked ? 0f : PhantomAttackDamage,
                    Blocked = blocked,
                    SoulPenetration = soulPenetration,
                };

                LastPhantomAttack = result;
                PhantomAttacked?.Invoke(result);
                results.Add(result);
            }

            return results;
        }

        /// <summary>对残影分身造成伤害；全部分身死亡后本体失去 50% 减伤。</summary>
        public float ApplyDamageToPhantom(BossPhantom phantom, float amount)
        {
            if (!IsActive || !IsPhaseThree || phantom == null)
            {
                return 0f;
            }

            return phantom.TakeDamage(amount);
        }

        /// <summary>员工死亡入口。归属员工会在 5 秒后原地满血复活。</summary>
        public void NotifyEmployeeKilled(BossTarget employee)
        {
            if (employee == null)
            {
                return;
            }

            employee.IsAlive = false;

            if (employee.IsBelonging && IsPhaseThree && !IsDefeated)
            {
                TrackEmployee(employee);
                _belongingReviveTimers[employee] = BelongingReviveSeconds;
            }
        }

        /// <summary>
        /// 反显影协议：套装员工进入「显影」时调用。消耗 3 层注视强制打断，
        /// 每个第三阶段只能成功一次。
        /// </summary>
        public AntiManifestResult NotifyManifestation(BossTarget employee)
        {
            var result = new AntiManifestResult
            {
                TargetName = employee != null ? employee.Name : null,
                GazeStacksAfter = GazeStacks,
            };

            if (!IsPhaseThree)
            {
                result.Reason = "不在第三阶段";
                LastAntiManifest = result;
                return result;
            }

            if (employee == null || !employee.IsAlive)
            {
                result.Reason = "目标无效";
                LastAntiManifest = result;
                return result;
            }

            if (!employee.HasEgoSet)
            {
                result.Reason = "目标未穿戴 EGO 套装";
                LastAntiManifest = result;
                return result;
            }

            if (!employee.IsManifesting)
            {
                result.Reason = "目标未处于显影状态";
                LastAntiManifest = result;
                return result;
            }

            if (_antiManifestUsed)
            {
                result.AlreadyUsedThisPhase = true;
                result.Reason = "本阶段已使用过反显影协议";
                LastAntiManifest = result;
                return result;
            }

            if (GazeStacks < AntiManifestGazeCost)
            {
                result.Reason = "注视层数不足";
                LastAntiManifest = result;
                return result;
            }

            GazeStacks -= AntiManifestGazeCost;
            employee.IsManifesting = false;
            _antiManifestUsed = true;

            result.Interrupted = true;
            result.GazeConsumed = AntiManifestGazeCost;
            result.GazeStacksAfter = GazeStacks;
            result.Reason = "显影已打断";

            LastAntiManifest = result;
            ManifestationInterrupted?.Invoke(result);
            return result;
        }

        private void EnterPhaseThree()
        {
            ResetPhaseThreeState();
            PhaseThreeStarted?.Invoke();
        }

        private void ResetPhaseThreeState()
        {
            _phantoms.Clear();
            _belongingReviveTimers.Clear();

            for (int i = 0; i < _notMeTriggered.Length; i++)
            {
                _notMeTriggered[i] = false;
            }

            _soulSwapTimer = 0f;
            CurrentSoulResistance = PhaseThreeSoulResistanceHigh;
            SoulResistanceSwapCount = 0;
            _goldenClawTimer = 0f;
            _projectionTimer = 0f;
            _phantomAttackTimer = 0f;
            _lightShadowReflectionTimer = 0f;
            _antiManifestUsed = false;
            _nextPhantomId = 0;
            _notMeTriggerCount = 0;
            LastGoldenClaw = null;
            LastPhantomAttack = null;
            LastNotMe = null;
            LastAntiManifest = null;
        }

        private void TickPhaseThree(float deltaTime)
        {
            if (IsDefeated)
            {
                return;
            }

            TickBelongingRevives(deltaTime);

            _soulSwapTimer += deltaTime;

            while (_soulSwapTimer >= SoulResistanceSwapIntervalSeconds)
            {
                _soulSwapTimer -= SoulResistanceSwapIntervalSeconds;
                ToggleSoulResistance();
            }

            if (_lightShadowReflectionTimer > 0f)
            {
                _lightShadowReflectionTimer -= deltaTime;

                if (_lightShadowReflectionTimer < 0f)
                {
                    _lightShadowReflectionTimer = 0f;
                }
            }

            _goldenClawTimer += deltaTime;

            while (_goldenClawTimer >= GoldenClawIntervalSeconds)
            {
                _goldenClawTimer -= GoldenClawIntervalSeconds;
                UseGoldenClaw(FindFirstLivingEmployee());
            }

            _phantomAttackTimer += deltaTime;

            while (_phantomAttackTimer >= PhantomAttackIntervalSeconds)
            {
                _phantomAttackTimer -= PhantomAttackIntervalSeconds;
                ResolvePhantomAttacks();
            }

            _projectionTimer += deltaTime;

            while (_projectionTimer >= FaceProjectionIntervalSeconds)
            {
                _projectionTimer -= FaceProjectionIntervalSeconds;
                CastFaceProjection();
            }
        }

        private void ToggleSoulResistance()
        {
            CurrentSoulResistance = CurrentSoulResistance > 0.75f
                ? PhaseThreeSoulResistanceLow
                : PhaseThreeSoulResistanceHigh;

            SoulResistanceSwapCount++;
        }

        private BossTarget FindFirstLivingEmployee()
        {
            for (int i = 0; i < _employees.Count; i++)
            {
                if (_employees[i].IsAlive)
                {
                    return _employees[i];
                }
            }

            return null;
        }

        private BossTarget FindPhantomTarget()
        {
            BossTarget fallback = null;

            for (int i = 0; i < _employees.Count; i++)
            {
                BossTarget employee = _employees[i];

                if (!employee.IsAlive)
                {
                    continue;
                }

                if (employee.HasEgoSet)
                {
                    return employee;
                }

                if (fallback == null)
                {
                    fallback = employee;
                }
            }

            return fallback;
        }

        private void TickBelongingRevives(float deltaTime)
        {
            if (_belongingReviveTimers.Count == 0)
            {
                return;
            }

            var employees = new List<BossTarget>(_belongingReviveTimers.Keys);

            for (int i = 0; i < employees.Count; i++)
            {
                BossTarget employee = employees[i];
                float remaining = _belongingReviveTimers[employee] - deltaTime;

                if (remaining > 0f)
                {
                    _belongingReviveTimers[employee] = remaining;
                    continue;
                }

                _belongingReviveTimers.Remove(employee);
                employee.IsAlive = true;
                BelongingRevived?.Invoke(employee);
            }
        }

        private float ApplyPhaseThreeDamage(DamageType damageType, float amount)
        {
            ResetLastDamageFields();

            if (IsDefeated)
            {
                return 0f;
            }

            float resistance = GetPhaseThreeResistance(damageType);
            float effectiveDamage = amount
                * ResistanceToMultiplier(resistance)
                * Math.Max(0f, 1f - PhaseThreeDamageReduction);

            if (effectiveDamage <= 0f)
            {
                return 0f;
            }

            BodyHp = Math.Max(0f, BodyHp - effectiveDamage);
            LastBodyDamage = effectiveDamage;
            CheckNotMeThresholds();
            return effectiveDamage;
        }

        private void CheckNotMeThresholds()
        {
            float ratio = BodyHp / BodyMaxHp;

            if (ratio <= 0.75f && !_notMeTriggered[0])
            {
                _notMeTriggered[0] = true;
                TriggerNotMe(0.75f);
            }

            if (ratio <= 0.50f && !_notMeTriggered[1])
            {
                _notMeTriggered[1] = true;
                TriggerNotMe(0.50f);
            }

            if (ratio <= 0.25f && !_notMeTriggered[2])
            {
                _notMeTriggered[2] = true;
                TriggerNotMe(0.25f);
            }
        }

        private void TriggerNotMe(float hpRatio)
        {
            int affected = 0;
            int clearedDesire = 0;

            for (int i = 0; i < _employees.Count; i++)
            {
                BossTarget employee = _employees[i];

                if (!employee.IsAlive || employee.DesireStacks <= 0)
                {
                    continue;
                }

                affected++;
                clearedDesire += employee.DesireStacks;
                employee.DesireStacks = 0;
            }

            bool buffGranted = clearedDesire >= NotMeDesireThreshold;

            if (buffGranted)
            {
                _lightShadowReflectionTimer = LightShadowReflectionDurationSeconds;
            }

            _notMeTriggerCount++;

            var result = new NotMeResult
            {
                TriggerAtHpRatio = hpRatio,
                AffectedCount = affected,
                ClearedDesire = clearedDesire,
                BuffGranted = buffGranted,
                MentalDamage = NotMeMentalDamage,
            };

            LastNotMe = result;
            NotMeTriggered?.Invoke(result);
        }
    }
}
