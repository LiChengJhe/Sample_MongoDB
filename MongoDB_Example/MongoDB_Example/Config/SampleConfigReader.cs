using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace MongoDB_GridFS.Config
{
    public class SampleConfigReader
    {
        private IConfiguration _Configuration;
        public SampleConfigReader(IConfiguration configuration)
        {
            this._Configuration = configuration;
        }
        public SampleConfig GetConfig(string configName = "Sample")
        {
            SampleConfig config = new SampleConfig();
            this._Configuration.GetSection(configName).Bind(config);
            return config;
        }
    }

    public class SampleConfig
    {
        public string UploadFilePath { get; set; }
        public DbConfig StorageDB { get; set; }
    }
}
