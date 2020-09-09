using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;

namespace Ghoul.Configuration
{
    public class JsonConfiguration : IConfiguration
    {
        static IDictionary<string, IConfigurationRoot> m_configs = new Dictionary<string, IConfigurationRoot>();

        public JsonConfiguration() : this(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json", "appsettings.dev.json") { }

        public JsonConfiguration(string basePath, params string[] fileNames)
        {
            if (fileNames == null || fileNames.Length == 0)
                throw new Exception("At least one configuration file must be specified");

            if (!m_configs.ContainsKey(fileNames[0]))
            {
                lock (m_configs)
                {
                    if (!m_configs.ContainsKey(fileNames[0]))
                    {

                        if (!Directory.Exists(basePath))
                            throw new DirectoryNotFoundException(basePath);

                        var filePath = Path.Combine(basePath, fileNames[0]);
                        if (!File.Exists(filePath))
                            throw new FileNotFoundException(filePath);

                        //https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/indexs
                        var builder = new ConfigurationBuilder()
                        .SetBasePath(basePath);   //set config file path

                        foreach (var fileName in fileNames)
                        {
                            if (string.IsNullOrEmpty(fileName))
                                throw new Exception($"FileName can not be null");
                            builder.AddJsonFile(fileName, optional: true, reloadOnChange: true);  //load file config
                        }

                        this.ConfigurationRoot = m_configs[fileNames[0]] = builder.Build();
                    }
                }
            }
        }

        public IConfigurationRoot ConfigurationRoot { get; private set; }

        public T Get<T>(string name)
        {
            var section = ConfigurationRoot.GetSection(name);
            var errMsg = $"Configuration file {name}’ cannot find";
            if (section == null)
                throw new Exception(errMsg);

            var config = section.Get<T>();
            if (config == null)
                throw new Exception(errMsg);

            return config;
        }

        public IConfigurationSection Get(string name)
        {
            var section = ConfigurationRoot.GetSection(name);
            var errMsg = $"Configuration file {name}’ cannot find";
            if (section == null)
                throw new Exception(errMsg);

            return section;
        }

        public T Get<T>(string name, T defaultValue = default(T))
        {
            var section = ConfigurationRoot.GetSection(name);
            if (section == null)
                return defaultValue;

            return section.Get<T>();
        }

        private static JsonConfiguration _appsettings;
        public static JsonConfiguration AppSettings
        {
            get
            {
                if (_appsettings == null)
                    _appsettings = new JsonConfiguration();
                return _appsettings;
            }
        }
    }
}
