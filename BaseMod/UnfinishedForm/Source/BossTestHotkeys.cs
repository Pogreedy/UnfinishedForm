using UnityEngine;

namespace UnfinishedForm.BaseMod
{
    /// <summary>Boss 测试快捷键；只在测试时使用。</summary>
    public static class BossTestHotkeys
    {
        public static void OnGameManagerUpdate()
        {
            if (Input.GetKeyDown(KeyCode.F6))
            {
                BossRuntime.ForceEscape();
            }
            else if (Input.GetKeyDown(KeyCode.F7))
            {
                BossRuntime.ForcePhaseFour();
            }
            else if (Input.GetKeyDown(KeyCode.F8))
            {
                BossRuntime.TestDamage();
            }
            else if (Input.GetKeyDown(KeyCode.F9))
            {
                BossRuntime.ResetTest();
            }

            BossRuntime.Tick(Time.deltaTime);
        }
    }
}
