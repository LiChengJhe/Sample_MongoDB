using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using MongoDB_Example.Storage;
using MongoDB_GridFS.Config;
using MongoDB_GridFS.Storage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MongoDB_GridFS
{
    class Program
    {
        static void Main(string[] args)
        {
            #region DI
            IServiceProvider provider = GetProvider();
            SampleConfig sampleConfig = provider.GetRequiredService<SampleConfig>();
            #endregion 

            #region Upload and Download File

            MongoFileStorage fileStorage = new MongoFileStorage(sampleConfig.StorageDB);

            // Get file name and bytes
            string fileFullName = new FileInfo(sampleConfig.UploadFilePath).Name;
            byte[] fileBytes = File.ReadAllBytes(sampleConfig.UploadFilePath);

            string id = fileStorage.Upload(fileFullName, fileBytes);
            Console.WriteLine($"Upload {fileFullName} => the file id :{id}");
            Console.WriteLine($"==============================================================================================");

            FileData file = fileStorage.Download(id);
            Console.WriteLine($"Download {file.Id} => the file name is {file.Name}");
            Console.WriteLine($"==============================================================================================");

            bool isDel = fileStorage.Delete(file.Id);
            Console.WriteLine($"Delete { file.Id} => {isDel}");
            Console.WriteLine($"==============================================================================================");
            #endregion

            #region CRUD
            MongoStorage storage = new MongoStorage(sampleConfig.StorageDB);
            try
            {
                storage.StartTransaction();

                storage.Insert<SampleConfig>(sampleConfig, tableName: "test");
                Console.WriteLine($"Insert => {JsonConvert.SerializeObject(sampleConfig)}");
                Console.WriteLine($"==============================================================================================");

                List<SampleConfig> dataSet = storage.Query<SampleConfig>(o => o.UploadFilePath.Length>0, tableName: "test");
                Console.WriteLine($"Query => {JsonConvert.SerializeObject(dataSet)}");
                Console.WriteLine($"==============================================================================================");

                storage.Update<SampleConfig>(o=>o.UploadFilePath.Length>0, Builders<SampleConfig>.Update.Set(o=>o.UploadFilePath, "Test Path"), tableName: "test");
                Console.WriteLine($"Update => UploadFilePath = Test Path");
                Console.WriteLine($"==============================================================================================");

                dataSet = storage.Query<SampleConfig>(o => o.UploadFilePath.Length > 0, tableName: "test");
                Console.WriteLine($"Query => {JsonConvert.SerializeObject(dataSet)}");
                Console.WriteLine($"==============================================================================================");

                storage.Replace<SampleConfig>(o => o.UploadFilePath.Length > 0, sampleConfig, tableName: "test");
                Console.WriteLine($"Replace => {JsonConvert.SerializeObject(sampleConfig)}");
                Console.WriteLine($"==============================================================================================");

                dataSet = storage.Query<SampleConfig>(o => o.UploadFilePath.Length > 0, tableName: "test");
                Console.WriteLine($"Query => {JsonConvert.SerializeObject(dataSet)}");
                Console.WriteLine($"==============================================================================================");

                storage.Delete<SampleConfig>(o => o.UploadFilePath.Length > 0, tableName: "test");
                Console.WriteLine($"Query => {JsonConvert.SerializeObject(dataSet)}");
                Console.WriteLine($"==============================================================================================");

                dataSet = storage.Query<SampleConfig>(o => o.UploadFilePath.Length > 0, tableName: "test");
                Console.WriteLine($"Query => {JsonConvert.SerializeObject(dataSet)}");
                Console.WriteLine($"==============================================================================================");

               long count = storage.Count<SampleConfig>(o => o.UploadFilePath.Length > 0, tableName: "test");
                Console.WriteLine($"Count => {count}");
                Console.WriteLine($"==============================================================================================");
                storage.CommitTransaction();
            }
            catch(Exception ex)
            {
                storage.AbortTransaction();
                throw;
            }

            #endregion



            Console.ReadKey();
        }

        private static IConfiguration GetConfiguration()
        {

            string EnvironmentVariable = string.Empty;

#if DEBUG
            EnvironmentVariable = "Debug";
#else
            EnvironmentVariable = "Release";
#endif

            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory()).AddJsonFile(
                    $"appsettings.{EnvironmentVariable}.json",
                    optional: true,
                    reloadOnChange: true).Build();
            return config;
        }

        private static IServiceProvider GetProvider()
        {
            ServiceCollection services = new ServiceCollection();
            IConfiguration configs = GetConfiguration();

            List<DbConfig> mongoConfigs = new DbConfigReader(configs).GetConfigs("MongoDB");
            services.AddSingleton(mongoConfigs);


            SampleConfig storageServiceConfig = new SampleConfigReader(configs).GetConfig();
            storageServiceConfig.StorageDB = mongoConfigs.Find(o => o.DatabaseType == storageServiceConfig.StorageDB.DatabaseType && o.DatabaseName == storageServiceConfig.StorageDB.DatabaseName);
            services.AddSingleton(storageServiceConfig);

            ServiceProvider provider = services.BuildServiceProvider();
            return provider;
        }
    }
}
