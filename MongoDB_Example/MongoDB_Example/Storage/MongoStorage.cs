using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using MongoDB_GridFS.Config;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace MongoDB_Example.Storage
{
    public class MongoStorage :  IDisposable
    {
        private MongoClient _Conn;
        private IClientSessionHandle _Session;
        private DbConfig _DbConfig;
        public MongoStorage(DbConfig dbConfig)
        {
            this._DbConfig = dbConfig;
            this._Conn = new MongoClient(this.GetConnString());
            this._Session = this._Conn.StartSession();
            ConventionPack conventionPack = new ConventionPack { new IgnoreExtraElementsConvention(true) };
            ConventionRegistry.Register("IgnoreExtraElements", conventionPack, type => true);
        }
        private string ToConnStr(DbConfig dbConfig)
        {
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
        public IClientSessionHandle GetSession()
        {
            return this._Session;
        }

        public void StartTransaction()
        {
            this.GetSession().StartTransaction();
        }
        public void CommitTransaction()
        {
            this.GetSession().CommitTransaction();
        }
        public void AbortTransaction()
        {
            this.GetSession().AbortTransaction();
        }
        public DbConfig GetDbConfig()
        {
            return this._DbConfig;
        }
        public string GetConnString()
        {
            return this.ToConnStr(this._DbConfig);
        }
        public IMongoDatabase GetDB()
        {
            return this.GetSession().Client.GetDatabase(this._DbConfig.DatabaseName);
        }

        public void Insert<T>(T data,string tableName=null)
        {
            try
            {
                if (tableName == null) tableName =typeof(T).Name;
                this.GetDB().GetCollection<T>(tableName).InsertOne(this.GetSession(), data);

            }
            catch
            {
                throw;
            }
        }
        public void Insert<T>(List<T> dataSet, string tableName = null)
        {
            try
            {
                if (tableName == null) tableName = typeof(T).Name;
                this.GetDB().GetCollection<T>(tableName).InsertMany(this.GetSession(), dataSet);

            }
            catch
            {
                throw;
            }
        }
        public List<T> Query<T>(Expression<Func<T, bool>> filter, string tableName = null)
        {
            try
            {
                if (tableName == null) tableName = typeof(T).Name;
                return this.GetDB().GetCollection<T>(tableName).Find(this.GetSession(), filter).ToList();
            }
            catch
            {
                throw;
            }
        }

        public void Delete<T>(Expression<Func<T, bool>> filter, string tableName = null)
        {
            try
            {
                if (tableName == null) tableName = typeof(T).Name;
                this.GetDB().GetCollection<T>(tableName).DeleteMany(this.GetSession(), Builders<T>.Filter.Where(filter));
            }
            catch
            {
                throw;
            }
        }


        public void Replace<T>(Expression<Func<T, bool>> filter, T data, string tableName = null)
        {
            try
            {
                if (tableName == null) tableName = typeof(T).Name;
                ReplaceOneResult result = null;
                while (result==null|| result.MatchedCount!= result.ModifiedCount) {
                    result = this.GetDB().GetCollection<T>(tableName).ReplaceOne(this._Session, filter, data);
                }
            }
            catch
            {
                throw;
            }
        }

        public void Update<T>(Expression<Func<T, bool>> filter, UpdateDefinition<T> updateDef, string tableName = null)
        {
            try
            {
                if (tableName == null) tableName = typeof(T).Name;
                this.GetDB().GetCollection<T>(tableName).UpdateMany(this._Session, filter, updateDef);
            }
            catch
            {
                throw;
            }
        }

        public long Count<T>(Expression<Func<T, bool>> filter, string tableName = null)
        {
            try
            {
                if (tableName == null) tableName = typeof(T).Name;
                return this.GetDB().GetCollection<T>(tableName).CountDocuments(this.GetSession(), filter);
            }
            catch
            {
                throw;
            }
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
                if (this._Session != null) this._Session.Dispose();
                this._Session = null;
                disposedValue = true;
            }
        }

        // TODO: 僅當上方的 Dispose(bool disposing) 具有會釋放 Unmanaged 資源的程式碼時，才覆寫完成項。
        ~MongoStorage()
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
}
