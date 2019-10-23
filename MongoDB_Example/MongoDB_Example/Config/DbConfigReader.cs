using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace MongoDB_GridFS.Config
{
    public class DbConfigReader
    {
        private IConfiguration _Configuration;
        public DbConfigReader(IConfiguration configuration)
        {
            this._Configuration = configuration;
        }
        public List<DbConfig> GetConfigs(string databaseType)
        {
            List<DbConfig> configs = new List<DbConfig>();
            this._Configuration.GetSection($"{databaseType}").Bind(configs);
            return configs;
        }
    }

    public class DbConfig
    {
        public string DatabaseName { get; set; }
        public string DatabaseType { get; set; }
        public List<string> Connections { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public Dictionary<string, string> Options { get; set; }
    }
}
