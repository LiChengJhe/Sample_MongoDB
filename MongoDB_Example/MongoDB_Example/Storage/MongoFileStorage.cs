using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using MongoDB_GridFS.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MongoDB_GridFS.Storage
{
    public class MongoFileStorage : IDisposable
    {
        private MongoClient _Conn;
        private DbConfig _DbConfig;
        public MongoFileStorage(DbConfig dbConfig)
        {
            this._DbConfig = dbConfig;
            this._Conn = new MongoClient(this.GetConnString());
            ConventionPack conventionPack = new ConventionPack { new IgnoreExtraElementsConvention(true) };
            ConventionRegistry.Register("IgnoreExtraElements", conventionPack, type => true);
        }
        private string ToConnStr(DbConfig dbConfig) {
            string connStr = "mongodb://";
            if (!String.IsNullOrEmpty(dbConfig.User) && !String.IsNullOrEmpty(dbConfig.Password)) connStr += $"{dbConfig.User}:{dbConfig.Password}@";
            string hostStr = string.Empty;
            foreach (string i in dbConfig.Connections)
            {
                hostStr += $"{i},";
            }
            hostStr = hostStr.Substring(0, hostStr.Length - 1);
            connStr += $"{hostStr}/";
            if (!String.IsNullOrEmpty(dbConfig.DatabaseName)) connStr += dbConfig.DatabaseName;
            string optionStr = string.Empty;
            if (dbConfig.Options != null)
            {
                foreach (KeyValuePair<string, string> i in dbConfig.Options)
                {
                    optionStr += $"{i.Key}={i.Value}&";
                }
                optionStr = optionStr.Substring(0, optionStr.Length - 1);
                connStr += $"?{optionStr}";
            }
            return connStr;
        }
        public MongoClient GetConn()
        {
            return this._Conn;
        }
        public string GetConnString()
        {
            return this.ToConnStr(this._DbConfig);
        }
        public DbConfig GetDbConfig()
        {
            return this._DbConfig;
        }
       
        public IMongoDatabase GetDB()
        {
            return this._Conn.GetDatabase(this._DbConfig.DatabaseName);
        }

        public string Upload(string fileFullName, byte[] fileBytes)
        {
            GridFSBucket gridFS = new GridFSBucket(this.GetDB());
            return gridFS.UploadFromBytes(fileFullName, fileBytes).ToString();
        }
        public string Upload(string fileFullName, Stream fileStream)
        {
            GridFSBucket gridFS = new GridFSBucket(this.GetDB());
            return gridFS.UploadFromStream(fileFullName, fileStream).ToString();
        }
        public FileData Download(string fileId)
        {
            FileData file = null;
            try
            {
                GridFSBucket gridFS = new GridFSBucket(this.GetDB());
                GridFSFileInfo gridFSFile = gridFS.Find(Builders<GridFSFileInfo>.Filter.Eq("_id", new ObjectId(fileId))).FirstOrDefault();
                if (gridFSFile != null)
                {
                    file = new FileData
                    {
                        Id = fileId,
                        Name = gridFSFile.Filename,
                        Type = gridFSFile.Filename.Substring(gridFSFile.Filename.IndexOf('.') + 1),
                        Bytes = gridFS.DownloadAsBytes(new ObjectId(fileId)),
                        UploadDateTime = gridFSFile.UploadDateTime.ToUniversalTime()
                    };
                }
                else throw new Exception($"cannot find {fileId} file.");
            }
            catch { throw; }
            return file;
        }
        public void Download(string fileId,Stream destination)
        {
            try
            {
                GridFSBucket gridFS = new GridFSBucket(this.GetDB());
                GridFSFileInfo gridFSFile = gridFS.Find(Builders<GridFSFileInfo>.Filter.Eq("_id", new ObjectId(fileId))).FirstOrDefault();
                if (gridFSFile != null)
                {
                    gridFS.DownloadToStream(new ObjectId(fileId), destination);
                }
                else throw new Exception($"cannot find {fileId} file.");
            }
            catch { throw; }
        }
        public bool Delete(string fileId)
        {
            GridFSBucket gridFS = new GridFSBucket(this.GetDB());
            gridFS.Delete(new ObjectId(fileId));
            return true;
        }

        public List<GridFSFileInfo> GetGridFSFiles(FilterDefinition<GridFSFileInfo> filter)
        {
            GridFSBucket gridFS = new GridFSBucket(this.GetDB());
            List<GridFSFileInfo> gridFSFiles = gridFS.Find(filter).ToList();
            return gridFSFiles;
        }

        public void Rename(string fileId, string newFilename)
        {
            GridFSBucket gridFS = new GridFSBucket(this.GetDB());
            gridFS.Rename(new ObjectId(fileId), newFilename);
        }

        #region IDisposable Support
        private bool disposedValue = false; // 偵測多餘的呼叫

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 處置 Managed 狀態 (Managed 物件)。
                }

                // TODO: 釋放 Unmanaged 資源 (Unmanaged 物件) 並覆寫下方的完成項。
                // TODO: 將大型欄位設為 null。
                this._Conn = null;
                disposedValue = true;
            }
        }

        // TODO: 僅當上方的 Dispose(bool disposing) 具有會釋放 Unmanaged 資源的程式碼時，才覆寫完成項。
        ~MongoFileStorage()
        {
            // 請勿變更這個程式碼。請將清除程式碼放入上方的 Dispose(bool disposing) 中。
            Dispose(false);
        }

        // 加入這個程式碼的目的在正確實作可處置的模式。
        public void Dispose()
        {
            // 請勿變更這個程式碼。請將清除程式碼放入上方的 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果上方的完成項已被覆寫，即取消下行的註解狀態。
            GC.SuppressFinalize(this);
        }
        #endregion
    }
    public class FileData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public DateTime UploadDateTime { get; set; }
        public byte[] Bytes { get; set; }
    }
}
