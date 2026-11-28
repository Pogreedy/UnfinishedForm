using System.Reflection;
using Harmony;
using UnityEngine;

namespace UnfinishedForm.BaseMod
{
    /// <summary>
    /// BaseMod 会扫描 DLL，并实例化名为 Harmony_Patch 的类型；
    /// 同时由 UnfinishedFormCreature 主动调用 Install()，避免 BaseMod 没有实例化入口。
    /// </summary>
    public sealed class Harmony_Patch
    {
        private static bool _installed;

        public Harmony_Patch()
        {
            Install();
        }

        public static void Install()
        {
            if (_installed)
            {
                return;
            }

            Debug.Log("[UnfinishedForm] Harmony_Patch 正在安装。");
            HarmonyInstance harmony = HarmonyInstance.Create("UnfinishedForm.BaseMod");
            Patch(harmony, typeof(CreatureBase), "OnStageStart", nameof(ContainmentRuntime.OnStageStart), false);
            Patch(harmony, typeof(AgentModel), "Panic", nameof(ContainmentRuntime.SuppressPanic), true);
            Patch(harmony, typeof(CommandWindow.CommandWindow), "CreateWindow", nameof(ContainmentRuntime.OnCreateCommandWindow), false);
            Patch(harmony, typeof(CommandWindow.CommandWindow), "OnEnable", nameof(ContainmentRuntime.OnCommandWindowEnable), false);
            Patch(harmony, typeof(CommandWindow.WorkAllocateRegion), "SetData", nameof(ContainmentRuntime.OnWorkAllocateSetData), false);
            _installed = true;
            Debug.Log("[UnfinishedForm] Harmony_Patch 安装完成。");
        }

        private static void Patch(HarmonyInstance harmony, System.Type type, string target, string patchMethod, bool prefix)
        {
            MethodInfo original = type.GetMethod(target, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            MethodInfo patch = typeof(ContainmentRuntime).GetMethod(patchMethod, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (ReferenceEquals(original, null) || ReferenceEquals(patch, null))
            {
                Debug.LogError("[UnfinishedForm] Harmony target/patch not found: " + type.FullName + "." + target);
                return;
            }

            if (prefix)
            {
                harmony.Patch(original, new HarmonyMethod(patch), null, null);
            }
            else
            {
                harmony.Patch(original, null, new HarmonyMethod(patch), null);
            }
        }
    }
}
