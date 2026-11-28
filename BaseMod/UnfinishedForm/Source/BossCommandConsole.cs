using UnityEngine;

namespace UnfinishedForm.BaseMod
{
    /// <summary>
    /// 非按键触发的测试命令控制台。Ctrl+F10 打开/关闭，输入 uf 指令。
    /// </summary>
    public sealed class BossCommandConsole : MonoBehaviour
    {
        private static BossCommandConsole _instance;
        private bool _open;
        private string _input = string.Empty;
        private string _message = "输入 uf help 查看指令";

        public static void Create()
        {
            if (_instance != null)
            {
                return;
            }

            GameObject root = new GameObject("UnfinishedFormCommandConsole");
            Object.DontDestroyOnLoad(root);
            _instance = root.AddComponent<BossCommandConsole>();
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F10))
            {
                _open = !_open;
                if (_open)
                {
                    _message = "输入 uf help 查看指令";
                }
            }
        }

        private void OnGUI()
        {
            if (!_open)
            {
                return;
            }

            GUILayout.BeginArea(new Rect(20f, Screen.height * 0.3f, 520f, 220f), GUI.skin.box);
            GUILayout.Label("UnfinishedForm 测试指令（Ctrl+F10 关闭）");
            _input = GUILayout.TextField(_input ?? string.Empty);

            if (GUILayout.Button("执行 / Enter"))
            {
                Execute();
            }

            GUILayout.Label(_message ?? string.Empty);
            GUILayout.Label("uf escape | uf phase4 | uf phase1/2/3 | uf damage 300 | uf reset | uf help");
            GUILayout.EndArea();

            Event e = Event.current;
            if (e != null && e.type == EventType.KeyDown && (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter))
            {
                Execute();
                e.Use();
            }
        }

        private void Execute()
        {
            string command = (_input ?? string.Empty).Trim().ToLowerInvariant();
            _input = string.Empty;

            if (command == "uf help" || command == "help")
            {
                _message = "uf escape | uf phase4 | uf phase1/2/3 | uf damage 300 | uf reset";
                return;
            }

            if (command == "uf escape")
            {
                BossRuntime.ForceEscape();
                _message = "已执行：强制出逃";
                return;
            }

            if (command == "uf phase4")
            {
                BossRuntime.ForcePhaseFour();
                _message = "已执行：跳到第 4 阶段";
                return;
            }

            if (command == "uf phase1" || command == "uf phase2" || command == "uf phase3")
            {
                string number = command.Substring(command.Length - 1);
                int phase = int.Parse(number);
                BossRuntime.ForcePhase((BossPhase)phase);
                _message = "已执行：跳到第 " + phase + " 阶段";
                return;
            }

            if (command.StartsWith("uf damage "))
            {
                float amount;
                string value = command.Substring("uf damage ".Length);
                if (float.TryParse(value, out amount))
                {
                    BossRuntime.TestDamage(amount);
                    _message = "已执行：对 Boss 造成 " + amount + " 伤害";
                }
                else
                {
                    _message = "伤害数值无法解析。";
                }
                return;
            }

            if (command == "uf reset")
            {
                BossRuntime.ResetTest();
                _message = "已复位 Boss 测试状态。";
                return;
            }

            _message = "未知指令。输入 uf help 查看。";
        }
    }
}
