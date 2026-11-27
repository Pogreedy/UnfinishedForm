namespace UnfinishedForm.Config
{
    /// <summary>
    /// 数值 JSON 的读取入口。序列化、校验与缺省值处理均未实现。
    /// </summary>
    public sealed class ConfigLoader
    {
        public ConfigLoader(string configDirectory)
        {
            ConfigDirectory = configDirectory;
        }

        /// <summary>Config 目录的绝对路径。</summary>
        public string ConfigDirectory { get; }

        /// <summary>读取全部数值表并组装为 ModConfig。</summary>
        public ModConfig LoadAll()
        {
            // TODO: 依次读取 abnormalities.json / escape.json / ego.json / ego_sets.json。
            return new ModConfig();
        }

        /// <summary>读取单个 JSON 文件并反序列化为指定类型。</summary>
        public T LoadJson<T>(string fileName) where T : class
        {
            // TODO: 使用 Newtonsoft.Json 反序列化；文件不存在时返回 null。
            return null;
        }
    }
}
