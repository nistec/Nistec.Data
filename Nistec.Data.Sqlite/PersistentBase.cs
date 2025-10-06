//licHeader
//===============================================================================================================
// System  : Nistec.Lib - Nistec.Lib Class Library
// Author  : Nissim Trujman  (nissim@nistec.net)
// Updated : 01/07/2015
// Note    : Copyright 2007-2015, Nissim Trujman, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class that is part of nistec library.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: http://nistec.net/license/nistec.cache-license.txt.  
// This notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who      Comments
// ==============================================================================================================
// 10/01/2006  Nissim   Created the code
//===============================================================================================================
//licHeader|
using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Xml;
using System.IO;
using System.Reflection;
using Nistec.Generic;
using Nistec.Xml;
using Nistec.Runtime;
using System.Collections.Concurrent;
using Nistec.Data.Entities;
using System.Transactions;
using Nistec.Config;
using System.Data.SQLite;
using System.Threading.Tasks;
using System.Data;
using Nistec.Serialization;
using Nistec.IO;
using Nistec.Data.Persistance;
using System.Threading;

namespace Nistec.Data.Sqlite
{

    /// <summary>
    /// Represent a Config file as Dictionary key-value
    /// <example>
    /// <sppSttings>
    /// <myname value='nissim' />
    /// <mycompany value='mcontrol' />
    /// </sppSttings>
    /// </example>
    /// </summary>
    [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
    public abstract class PersistentBase<T,PI> where PI : IPersistItem //:IDictionary<string,T>
    {
        #region g
        internal long _ProcCounter = 0;

        public long Increment_ProcCounter()
        {
            return Interlocked.Increment(ref _ProcCounter);
        }
        public long Decrement_ProcCounter()
        {
            return Interlocked.Decrement(ref _ProcCounter);
        }
        public long Read_ProcCounter()
        {
            return Interlocked.Read(ref _ProcCounter);
        }
        #endregion

        #region sql comannds

        //protected const string sqlcreate = @"CREATE TABLE IF NOT EXISTS {0} (
        //                  key TEXT PRIMARY KEY,
        //                  body BLOB,
        //                  name TEXT,
        //                  timestamp DATETIME DEFAULT CURRENT_TIMESTAMP     
        //                ) WITHOUT ROWID;";

        protected const string sqlinsert = "insert into {0} (key, body, name) values (@key, @body, @name)";
        protected const string sqldelete = "delete from {0} where key=@key";
        protected const string sqlupdate = "update {0} set body=@body, timestamp=CURRENT_TIMESTAMP where key=@key";
        protected const string sqlinsertOrIgnore = "insert or IGNORE into {0}(key, body, name) values(@key, @body, @name)";
        protected const string sqlinsertOrReplace = "insert or REPLACE into {0}(key, body, name) values(@key, @body, @name)";

        //const string sqldeleteTrans = "PRAGMA busy_timeout = 5000;  BEGIN TRANSACTION; PRAGMA read_uncommitted = true; delete from {0} where key=@key; COMMIT TRANSACTION;";
        protected const string sqldeleteTrans = "BEGIN TRANSACTION; PRAGMA read_uncommitted = true; delete from {0} where key=@key; COMMIT TRANSACTION;";
        protected const string sqlupdateTrans = "BEGIN TRANSACTION; PRAGMA read_uncommitted = true; update {0} set body=@body, timestamp=CURRENT_TIMESTAMP where key=@key; COMMIT TRANSACTION;";
        protected const string sqlinsertOrIgnoreTrans = "BEGIN TRANSACTION; PRAGMA read_uncommitted = true; insert or IGNORE into {0}(key, body, name) values(@key, @body, @name); COMMIT TRANSACTION;";
        protected const string sqlinsertOrReplaceTrans = "BEGIN TRANSACTION; PRAGMA read_uncommitted = true; insert or REPLACE into {0}(key, body, name) values(@key, @body, @name); COMMIT TRANSACTION;";
        protected const string sqlinsertTrans = "BEGIN TRANSACTION; insert into {0}(key, body, name) values(@key, @body, @name); COMMIT TRANSACTION;";
        protected const string sqlrollback = "ROLLBACK;";
        const string sqlupdatestate = "update {0} set state=@state,timestamp=CURRENT_TIMESTAMP where key=@key";
        protected const string sqlselect = "select {1} from {0} where key=@key";
        protected const string sqlselectall = "select {1} from {0}";

        #endregion

        protected readonly ConcurrentDictionary<string, T> dictionary;
        //protected readonly ConcurrentDictionary<string, DbLite> databases=new ConcurrentDictionary<string, DbLite>();

        //XDictionaryettings settings;
        //string connectionString;
        //string dbName;
        public const string DbProvider = DbLite.ProviderName;
        protected CommitMode _CommitMode;
        protected bool _EnableTasker;
        
        #region properties

        public DbLiteSettings Settings { get; protected set; }
        public string ConnectionString { get; protected set; }
        public string Name { get; protected set; }

        public bool EnableCompress { get; protected set; }
        public int CompressLevel { get; protected set; }
        /// <summary>
        /// Get value indicating if the config file exists
        /// </summary>
        public bool FileExists
        {
            get
            {
                return File.Exists(Settings.DbFilename);
            }
        }

        public long FileSize(string measure = "kb")
        {
            if (measure == "gb")
                return FileStreamHelper.FileSize(Settings.DbFilename) / 1024 / 1024 / 1024;
            else if (measure == "mb")
                return FileStreamHelper.FileSize(Settings.DbFilename) / 1024 / 1024;
            else if (measure == "kb")
                return FileStreamHelper.FileSize(Settings.DbFilename) / 1024;
            else //b
                return FileStreamHelper.FileSize(Settings.DbFilename);
        }


        // /// <summary>
        // ///     Gets a collection containing the keys in the System.Collections.Generic.Dictionary{TKey,TValue}.
        // ///
        // /// Returns:
        // ///     An System.Collections.Generic.ICollection{TKey} containing the keys in the
        // ///     System.Collections.Generic.Dictionary{TKey,TValue}.
        // /// </summary>
        // public ICollection<string> Keys  
        //{
        //    get { return dictionary.Keys; }
        //}


        // /// <summary>
        // ///     Gets a collection containing the values in the System.Collections.Generic.Dictionary{TKey,TValue}.
        // ///
        // /// Returns:
        // ///     An System.Collections.Generic.ICollection{TValue} containing the values in
        // ///     the System.Collections.Generic.Dictionary{TKey,TValue}.
        // /// </summary>
        // public ICollection<T> Values
        // {
        //     get { return dictionary.Values; }
        // }


        ///// <summary>
        ///// Get all items as array of KeyValuePair
        ///// </summary>
        ///// <returns></returns>
        //public KeyValuePair<string, T>[] ToArray()
        //{
        //    return dictionary.ToArray();
        //}

        #endregion

        #region events

        bool ignorEvent = false;
        //public event ConfigChangedHandler ItemChanged;
        //public event EventHandler ConfigFileChanged;
        public event GenericEventHandler<string> ErrorOcurred;
        public event EventHandler Initilaized;
        public event EventHandler BeginLoading;
        public event GenericEventHandler<string,int> LoadCompleted;
        //public event GenericEventHandler<string, T> ItemLoaded;
        public event GenericEventHandler<string, string, T> ItemChanged;
        public event EventHandler ClearCompleted;

        public Action<T> ItemLoaded { get; set; }

        protected virtual void OnInitilaized(EventArgs e)
        {
            if (Initilaized != null)
            {
                Initilaized(this, e);
            }
        }
        protected virtual void OnBeginLoading()
        {
            if (BeginLoading != null)
            {
                BeginLoading(this, EventArgs.Empty);
            }
        }
        protected virtual void OnClearCompleted()
        {
            if (ClearCompleted != null)
            {
                ClearCompleted(this, EventArgs.Empty);
            }
        }
        protected virtual void OnLoadCompleted(string message, int count)
        {
            if (LoadCompleted != null)
            {
                LoadCompleted(this, new GenericEventArgs<string, int>(message,count));
            }
        }
        protected virtual void OnLoadCompleted(GenericEventArgs<string, int> e)
        {
            if (LoadCompleted != null)
            {
                LoadCompleted(this, e);
            }
        }

        protected virtual void OnItemLoaded(T value)
        {
            if (ItemLoaded != null)
                ItemLoaded(value);
            //OnItemLoaded(new GenericEventArgs<string, T>(key, value));
        }

        //protected virtual void OnItemLoaded(GenericEventArgs<string, T> e)
        //{
        //    if (ItemLoaded != null)
        //        ItemLoaded(this, e);
        //}

        protected virtual void OnErrorOcurred(string action, string message)
        {
            if (ErrorOcurred != null)
            {
                OnErrorOcurred(new GenericEventArgs<string>("ErrorOcurred in: " + action + ", Messgae: " + message));
            }
        }
        protected virtual void OnErrorOcurred(GenericEventArgs<string> e)
        {
            if (ErrorOcurred != null)
            {
                ErrorOcurred(this, e);
            }
        }

        protected virtual void OnItemChanged(string action, string key, T value)
        {
            if (!ignorEvent)
                OnItemChanged(new GenericEventArgs<string, string, T>(action, key, value));
        }

        protected virtual void OnItemChanged(GenericEventArgs<string, string, T> e)
        {
            if (ItemChanged != null)
                ItemChanged(this, e);
        }

        //protected virtual void OnItemChanged(ConfigChangedArgs e)
        //{
        //    //if (settings.AutoSave)
        //    //{
        //    //    Save();
        //    //}
        //    if (ItemChanged != null)
        //        ItemChanged(this, e);
        //}

        // protected virtual void OnConfigFileChanged(EventArgs e)
        //{
        //    FileToDictionary();

        //    if (ConfigFileChanged != null)
        //        ConfigFileChanged(this, e);
        //}

        #endregion

        #region ctor
        /// <summary>
        /// PersistentDictionary ctor with a specefied filename
        /// </summary>
        /// <param name="filename"></param>
        public PersistentBase(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("PersistentDictionary.filename");
            }
            dictionary = new ConcurrentDictionary<string, T>();
            this.Settings = new DbLiteSettings()
            {
                Name = name,
                CommitMode = CommitMode.OnDisk
            //DbPath = dbpath
        };
            Name = Settings.Name;
            _CommitMode = CommitMode.OnDisk;
            Init();
        }

        /// <summary>
        /// PersistentDictionary ctor with a specefied Dictionary
        /// </summary>
        /// <param name="dict"></param>
        public PersistentBase(string dbpath, string name, IDictionary<string, T> dict)
        {
            if (string.IsNullOrEmpty(dbpath))
            {
                throw new ArgumentNullException("PersistentDictionary.dbpath");
            }
            dictionary = new ConcurrentDictionary<string, T>();
            this.Settings = new DbLiteSettings()
            {
                DbPath = dbpath,
                Name = name,
                CommitMode = CommitMode.OnDisk
            };
            Name = Settings.Name;
            _CommitMode = CommitMode.OnDisk;
            Init();
            Load(dict);
            OnInitilaized(EventArgs.Empty);
        }

        /// <summary>
        /// XConfig ctor with default filename CallingAssembly '.mconfig' in current direcory
        /// </summary>
        public PersistentBase(DbLiteSettings settings)
        {
            dictionary = new ConcurrentDictionary<string, T>();
            this.Settings = settings;
            Name = Settings.Name;
            _CommitMode = Settings.CommitMode;
            Init();
            //if (loadPersistItems)
            //    LoadDb();
            //else
            //    Clear();
            OnInitilaized(EventArgs.Empty);
        }
        #endregion

        private void Init()
        {
            string filename = null;
            try
            {
                if (this.Settings.InMemory)
                {
                    //filename = DbLiteSettings.InMemoryFilename;
                    throw new Exception("PersistentDictionary do not supported in memory db");
                }
                else
                {
                    filename = this.Settings.DbFilename;

                    DbLiteUtil.CreateFolder(Settings.DbPath);
                    DbLiteUtil.CreateFile(filename);
                    DbLiteUtil.ValidateConnection(filename, true);
                }
                ConnectionString = Settings.GetConnectionString();
                EnableCompress = Settings.EnableCompress;
                CompressLevel = Settings.ValidCompressLevel;
                
                using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
                {
                    var sql = DbCreateCommand();
                    db.ExecuteCommandNonQuery(sql);
                }
            }
            catch (Exception ex)
            {
                OnErrorOcurred(new GenericEventArgs<string>("Initilaized Error: " + ex.Message));
                throw ex;
            }

        }

        //using (var connection = new SQLiteConnection(ConnectionString))
        //{
        //    connection.Open();
        //    using (var trans = connection.BeginTransaction())
        //    {
        //        try
        //        {
        //            using (var cmd = connection.CreateCommand())
        //            {
        //                cmd.CommandText = cmdText;
        //                cmd.CommandType = System.Data.CommandType.Text;
        //                res = cmd.ExecuteNonQuery();
        //            }
        //            if (res > 0)
        //            {
        //                dictionary.Clear();
        //                trans.Commit();
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            string err = ex.Message;
        //        }
        //    }

        //}

        //public virtual void LoadDb()
        //{
        //    IList<KeyValueItem> list;

        //    using (TransactionScope tran = new TransactionScope())
        //    {
        //        try
        //        {
        //            using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
        //            {

        //                var sql = "select * from " + Name;
        //                list = db.Query<KeyValueItem>(sql, null);
        //                if (list != null && list.Count > 0)
        //                {
        //                    foreach (var entry in list)
        //                    {
        //                        var val= BinarySerializer.Deserialize<T>((byte[])entry.Value);

        //                        var o = BinarySerializer.Deserialize((byte[])entry.Value);

        //                        //BinaryStreamer streamer = new BinaryStreamer(new NetStream((byte[])entry.Value), null);

        //                        dictionary[entry.Key] = GenericTypes.Convert<T>(val);
        //                    }
        //                }

        //            }

        //            tran.Complete();
        //        }
        //        catch (Exception ex)
        //        {
        //            string err = ex.Message;
        //        }
        //    }
        //}
        public void LoadDbAsync()
        {
            Task.Factory.StartNew(() => LoadDb());
            //Task task=new Task(() => LoadDb());
            //task.Start();
            //task.Wait();

        }


        public virtual void LoadDb()
        {
            IList<PI> list;
            OnBeginLoading();

            try
            {
                //using (TransactionScope tran = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromMinutes(25)))
                //{
                //    using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
                //    {
                //        var sql = "select * from " + Name;
                //        list = db.Query<PI>(sql, null);
                //        if (list != null && list.Count > 0)
                //        {
                //            foreach (var entry in list)
                //            {
                //                var val = FromPersistItem(entry);
                //                if (val != null)
                //                {
                //                    dictionary[entry.key] = val;
                //                    OnItemLoaded(val);
                //                    //if (onTake != null)
                //                    //    onTake(val);
                //                }
                //            }
                //        }

                //    }

                //    tran.Complete();
                //}

                using (TransactionScope tran = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromMinutes(25)))
                {
                    using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
                    {
                        var sql = "select * from " + Name;
                        list = db.Query<PI>(sql, null);
                    }

                    tran.Complete();
                }
                if (list != null && list.Count > 0)
                {
                    foreach (var entry in list)
                    {
                        var val = FromPersistItem(entry);
                        if (val != null)
                        {
                            dictionary[entry.key] = val;
                            OnItemLoaded(val);
                            //if (onTake != null)
                            //    onTake(val);
                        }
                    }
                }
                OnLoadCompleted(this.Name, this.Count);
            }
            catch (Exception ex)
            {
                OnErrorOcurred("LoadDb", ex.Message);
            }

        }

        protected bool IsConnectionState(DbLite db, ConnectionState state )
        {
            return (db.SQLiteConnection != null && db.SQLiteConnection.State == state);
        }
        protected bool IsConnectionState(SQLiteConnection connection, ConnectionState state)
        {
            return (connection != null && connection.State == state);
        }

        protected void CloseConnection(DbLite db)
        {
            if (db.SQLiteConnection != null && db.SQLiteConnection.State != System.Data.ConnectionState.Closed)
                db.SQLiteConnection.Close();
        }

        //protected DbLite GetDb()
        //{
        //    DbLite db;
        //    if(!databases.TryGetValue(ConnectionString, out db))
        //    {
        //        db = new DbLite(ConnectionString, DBProvider.SQLite);
        //        databases.TryAdd(ConnectionString, db);
        //    }
        //    return db;
        //}

        //protected SQLiteConnection GetConnection()
        //{
        //    DbLite db= GetDb();
        //    if(IsConnectionState(db, ConnectionState.Open))
        //    {
        //        return new SQLiteConnection(db.SQLiteConnection);
        //    }
        //    return db.SQLiteConnection;
        //}

        //string sqlCreateTable = "create table demo_score (name varchar(20), score int)";

        //protected virtual string DbCreateCommand()
        //{
        //    return string.Format(sqlcreate, Name);
        //}
        protected virtual string DbAddCommand(bool isTrans = false)
        {
            if (isTrans)
                return string.Format(sqlinsertTrans, Name);
            return string.Format(sqlinsert, Name);
        }
        protected virtual string DbAddIgnoreCommand(bool isTrans = false)
        {
            if (isTrans)
                return string.Format(sqlinsertOrIgnoreTrans, Name);
            return string.Format(sqlinsertOrIgnore, Name);
        }
        protected virtual string DbAddReplaceCommand(bool isTrans = false)
        {
            if (isTrans)
                return string.Format(sqlinsertOrReplaceTrans, Name);
            return string.Format(sqlinsertOrReplace, Name);
        }
        protected virtual string DbDeleteCommand(bool isTrans = false)
        {
            if (isTrans)
                return string.Format(sqldeleteTrans, Name);
            return string.Format(sqldelete, Name);
        }

        protected virtual string DbUpdateCommand(bool isTrans = false)
        {
            if (isTrans)
                return string.Format(sqlupdateTrans, Name);
            return string.Format(sqlupdate, Name);
        }

        protected virtual string DbUpsertCommand(bool isTrans = false)
        {
            if (isTrans)
                return string.Format(sqlinsertOrReplaceTrans, Name);
            return string.Format(sqlinsertOrReplace, Name);
        }

        protected virtual string DbSelectCommand(string select, string where)
        {
            if (where == null)
                return string.Format(sqlselectall, Name, select);

            return string.Format(sqlselect, Name, select, where);
        }

        protected virtual string DbLookupCommand()
        {
            return string.Format(sqlselect, Name, "body");
        }

        protected virtual string DbUpdateStateCommand()
        {
            return string.Format(sqlupdatestate, Name);
        }

        protected abstract string DbCreateCommand();// DbLite db);
        //protected abstract string DbUpsertCommand();// string key, T value);
        //protected abstract string DbAddCommand(bool isTrans=false);//string key, T value);
        //protected abstract string DbAddIgnoreCommand(bool isTrans = false);
        //protected abstract string DbAddReplaceCommand(bool isTrans = false);
        //protected abstract string DbUpdateCommand();//string key, T value);
        //protected abstract string DbDeleteCommand();//string key);
        //protected abstract string DbSelectCommand(string select, string where);
        //protected abstract string DbLookupCommand();//string key);
        //protected abstract string DbUpdateStateCommand();//string key, int state);
        protected abstract string GetItemKey(PI value);
        protected abstract object GetDataValue(T item);
        protected abstract T DecompressValue(PI value);
        protected abstract PI ToPersistItem(string key,T item);
        protected abstract T FromPersistItem(PI value);


        #region Commands
        /// <summary>
        ///    Adds a key/value pair to the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        ///     if the key does not already exist, or updates a key/value pair in the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        ///     if the key already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="value">The value to be added or updated for an absent key</param>
        /// <returns></returns>
        public virtual int AddOrUpdate(string key, T value)
        {
            int res = 0;

            //dictionary.AddOrUpdate(key, value, (oldkey, oldValue) =>
            //{
            //    isExists = true;
            //    return value;
            //});

            bool iscommited = false;

            try
            {
                var body=GetDataValue(value);
                var cmdText = DbUpsertCommand();

                switch (_CommitMode)
                {
                    case CommitMode.OnDisk:
                        using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
                        {
                            res = db.ExecuteTransCommandNonQuery(cmdText, DataParameter.Get<SQLiteParameter>("key", key, "body", body), (result) =>
                            {
                                if (result > 0)
                                {
                                    dictionary[key] = value;
                                    //trans.Commit();
                                    iscommited = true;
                                }
                                return iscommited;
                            });
                        }
                        break;
                    default:
                        dictionary.AddOrUpdate(key, value, (oldkey, oldValue) =>
                        {
                            return value;
                        });
                        if (_CommitMode == CommitMode.OnMemory)
                        {
                            var task = new PersistentTask()
                            {
                                DdProvider = DBProvider.SQLite,
                                CommandText = cmdText,
                                CommandType = "DbUpsert",
                                ConnectionString = ConnectionString,
                                Parameters = DataParameter.Get<SQLiteParameter>("key", key, "body", body)
                            };
                            task.ExecuteTask(_EnableTasker);
                        }
                        res = 1;
                        iscommited = true;
                        break;
                }

                if (iscommited)
                {
                    OnItemChanged("AddOrUpdate", null, default(T));
                }
            }
            catch (Exception ex)
            {
                OnErrorOcurred("AddOrUpdate", ex.Message);
            }

            return res;
        }

        /// <summary>
        ///    updates a key/value pair to the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        ///    if the key already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="value">The value to be added or updated for an absent key</param>
        /// <returns></returns>
        public virtual int Update(string key, T value)
        {
            bool iscommited = false;
            int res = 0;

            try
            {
                var body = GetDataValue(value);
                var cmdText = DbUpdateCommand();

                switch (_CommitMode)
                {
                    case CommitMode.OnDisk:
                        using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
                        {
                            res = db.ExecuteTransCommandNonQuery(cmdText, DataParameter.Get<SQLiteParameter>("key", key, "body", body), (result) =>
                            {
                                if (result > 0)
                                {
                                    dictionary[key] = value;
                                    //trans.Commit();
                                    iscommited = true;
                                }
                                return iscommited;
                            });
                        }

                        break;
                    default:
                        dictionary[key] = value;
                        if (_CommitMode == CommitMode.OnMemory)
                        {
                            var task = new PersistentTask()
                            {
                                DdProvider = DBProvider.SQLite,
                                CommandText = cmdText,
                                CommandType = "DbUpdate",
                                ConnectionString = ConnectionString,
                                Parameters = DataParameter.Get<SQLiteParameter>("key", key, "body", body)
                            };
                            task.ExecuteTask(_EnableTasker);
                        }
                        res = 1;
                        iscommited = true;
                        break;
                }
                if (iscommited)
                {
                    OnItemChanged("Update", null, default(T));
                }
            }
            catch (Exception ex)
            {
                OnErrorOcurred("Update", ex.Message);
            }

            return res;
        }

        /// <summary>
        /// Attempts to add the specified key and value to the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add. The value can be a null reference (Nothing
        ///     in Visual Basic) for reference types.</param>
        /// <returns>
        /// true if the key/value pair was added to the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        ///     successfully. If the key already exists, this method returns false.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">key is null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="System.OverflowException">The dictionary already contains the maximum number of elements, System.Int32.MaxValue.</exception>
        public virtual bool TryAdd(string key, T value)
        {

            bool iscommited = false;

            try
            {
                var body = GetDataValue(value);
                var cmdText = DbAddCommand();

                switch (_CommitMode)
                {
                    case CommitMode.OnDisk:
                        using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
                        {
                            db.ExecuteTransCommandNonQuery(cmdText, DataParameter.Get<SQLiteParameter>("key", key, "body", body), (result) =>
                            {
                                if (result > 0)
                                {
                                    if (dictionary.TryAdd(key, value))
                                    {
                                        //trans.Commit();
                                        iscommited = true;
                                    }
                                }
                                return iscommited;
                            });
                        }
                        break;
                    default:
                        if (dictionary.TryAdd(key, value))
                        {
                            if (_CommitMode == CommitMode.OnMemory)
                            {
                                var task = new PersistentTask()
                                {
                                    DdProvider = DBProvider.SQLite,
                                    CommandText = cmdText,
                                    CommandType = "DbAdd",
                                    ConnectionString = ConnectionString,
                                    Parameters = DataParameter.Get<SQLiteParameter>("key", key, "body", body)
                                };
                                task.ExecuteTask(_EnableTasker);
                            }
                            iscommited = true;
                        }
                        break;
                }

                if (iscommited)
                {
                    OnItemChanged("TryAdd", key, value);
                }
            }
            catch (Exception ex)
            {
                string err = ex.Message;
                OnErrorOcurred("TryAdd", ex.Message);
            }
            return iscommited;

        }

        /// <summary>
        /// Summary:
        ///     Attempts to remove and return the value with the specified key from the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>.
        ///
        /// Parameters:
        ///   key:
        ///     The key of the element to remove and return.
        ///
        ///   value:
        ///     When this method returns, value contains the object removed from the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        ///     or the default value of if the operation failed.
        ///
        /// Returns:
        ///     true if an object was removed successfully; otherwise, false.
        ///
        /// Exceptions:
        ///   System.ArgumentNullException:
        ///     key is a null reference (Nothing in Visual Basic).
        /// </summary>
        public virtual bool TryRemove(string key, out T value)
        {

            bool iscommited = false;
            T outval = value = default(T);
            try
            {
                var cmdText = DbDeleteCommand();

                switch (_CommitMode)
                {
                    case CommitMode.OnDisk:
                        using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
                        {
                            db.ExecuteTransCommandNonQuery(cmdText, DataParameter.Get<SQLiteParameter>("key", key), (result) =>
                            {
                                if (result > 0)
                                {
                                    if (dictionary.TryRemove(key, out outval))
                                    {
                                        //trans.Commit();
                                        iscommited = true;
                                    }
                                }
                                return iscommited;
                            });
                        }
                        break;
                    default:
                        if (dictionary.TryRemove(key, out outval))
                        {
                            if (_CommitMode == CommitMode.OnMemory)
                            {
                                var task = new PersistentTask()
                                {
                                    DdProvider = DBProvider.SQLite,
                                    CommandText = cmdText,
                                    CommandType = "DbDelete",
                                    ConnectionString = ConnectionString,
                                    Parameters = DataParameter.Get<SQLiteParameter>("key", key)
                                };
                                task.ExecuteTask(_EnableTasker);
                            }
                            iscommited = true;
                        }
                        break;
                }

                value = outval;

                if (iscommited)
                {
                    OnItemChanged("TryRemove", key, value);
                }
            }
            catch (Exception ex)
            {
                OnErrorOcurred("TryRemove", ex.Message);
            }
            return iscommited;
        }


        /// <summary>
        /// Summary:
        ///     Compares the existing value for the specified key with a specified value,
        ///     and if they are equal, updates the key with a third value.
        ///
        /// Parameters:
        ///   key:
        ///     The key whose value is compared with comparisonValue and possibly replaced.
        ///
        ///   newValue:
        ///     The value that replaces the value of the element with key if the comparison
        ///     results in equality.
        ///
        ///   comparisonValue:
        ///     The value that is compared to the value of the element with key.
        ///
        /// Returns:
        ///     true if the value with key was equal to comparisonValue and replaced with
        ///     newValue; otherwise, false.
        ///
        /// Exceptions:
        ///   System.ArgumentNullException:
        ///     key is a null reference.
        /// </summary>
        public virtual bool TryUpdate(string key, T newValue, T comparisonValue)
        {

            bool iscommited = false;

            try
            {
                var body = GetDataValue(newValue);
                var cmdText = DbUpdateCommand();

                switch (_CommitMode)
                {
                    case CommitMode.OnDisk:
                        using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
                        {
                            db.ExecuteTransCommandNonQuery(cmdText, DataParameter.Get<SQLiteParameter>("key", key, "body", body), (result) =>
                            {
                                if (result > 0)
                                {
                                    if (dictionary.TryUpdate(key, newValue, comparisonValue))
                                    {
                                        //trans.Commit();
                                        iscommited = true;
                                    }
                                }
                                return iscommited;
                            });
                        }
                        break;
                    default:

                        if (dictionary.TryUpdate(key, newValue, comparisonValue))
                        {
                            if (_CommitMode == CommitMode.OnMemory)
                            {
                                var task = new PersistentTask()
                                {
                                    DdProvider = DBProvider.SQLite,
                                    CommandText = cmdText,
                                    CommandType = "DbUpdate",
                                    ConnectionString = ConnectionString,
                                    Parameters = DataParameter.Get<SQLiteParameter>("key", key, "body", body)
                                };
                                task.ExecuteTask(_EnableTasker);
                            }
                            iscommited = true;
                        }
                        break;
                }

                if (iscommited)
                {
                    OnItemChanged("TryUpdate", key, newValue);
                }
            }
            catch (Exception ex)
            {
                string err = ex.Message;
                OnErrorOcurred("TryUpdate", ex.Message);
            }
            return iscommited;

        }
        #endregion

        #region ConcurrentDictionary<string,T>


        /// <summary>
        /// Gets a value that indicates whether the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue> is empty.
        /// Returns:
        ///     true if the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        ///     is empty; otherwise, false.
        /// </summary>

        public bool IsEmpty { get { return dictionary.IsEmpty; } }

        // Summary:
        //     Adds a key/value pair to the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        //     if the key does not already exist, or updates a key/value pair in the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        //     if the key already exists.
        //
        // Parameters:
        //   key:
        //     The key to be added or whose value should be updated
        //
        //   addValueFactory:
        //     The function used to generate a value for an absent key
        //
        //   updateValueFactory:
        //     The function used to generate a new value for an existing key based on the
        //     key's existing value
        //
        // Returns:
        //     The new value for the key. This will be either be the result of addValueFactory
        //     (if the key was absent) or the result of updateValueFactory (if the key was
        //     present).
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     key is a null reference (Nothing in Visual Basic).-or-addValueFactory is
        //     a null reference (Nothing in Visual Basic).-or-updateValueFactory is a null
        //     reference (Nothing in Visual Basic).
        //
        //   System.OverflowException:
        //     The dictionary already contains the maximum number of elements, System.Int32.MaxValue.
        //public T AddOrUpdate(string key, Func<string, T> addValueFactory, Func<string, T, T> updateValueFactory)
        //{
        //    return dictionary.AddOrUpdate(key, addValueFactory, updateValueFactory);
        //}
        //
        // Summary:
        //     Adds a key/value pair to the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        //     if the key does not already exist, or updates a key/value pair in the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        //     if the key already exists.
        //
        // Parameters:
        //   key:
        //     The key to be added or whose value should be updated
        //
        //   addValue:
        //     The value to be added for an absent key
        //
        //   updateValueFactory:
        //     The function used to generate a new value for an existing key based on the
        //     key's existing value
        //
        // Returns:
        //     The new value for the key. This will be either be addValue (if the key was
        //     absent) or the result of updateValueFactory (if the key was present).
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     key is a null reference (Nothing in Visual Basic).-or-updateValueFactory
        //     is a null reference (Nothing in Visual Basic).
        //
        //   System.OverflowException:
        //     The dictionary already contains the maximum number of elements, System.Int32.MaxValue.
        //public T AddOrUpdate(string key, T addValue, Func<string, T, T> updateValueFactory)
        //{
        //    return dictionary.AddOrUpdate(key,addValue,updateValueFactory);
        //}


      

        //public virtual int AddOrUpdate2(string key, T value)
        //{
        //    int res = 0;
        //    bool isExists = false;
        //    using (TransactionScope tran = new TransactionScope())
        //    {
        //        dictionary.AddOrUpdate(key, value, (oldkey, oldValue) =>
        //        {
        //            isExists = true;
        //            return value;
        //        });

        //        //Insert create script here.
        //        using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
        //        {
        //            var sql = isExists ? DbUpdateCommand(key, value) : DbAddCommand(key, value);// DbUpsertCommand(key,value);
        //            try
        //            {
        //                res = db.ExecuteCommandNonQuery(sql);
        //            }
        //            catch (Exception ex)
        //            {
        //                string err = ex.Message;
        //                if (isExists)
        //                {
        //                    sql = DbUpsertCommand(key, value);
        //                    res = db.ExecuteCommandNonQuery(sql);
        //                }
        //            }
        //        }

        //        if (res > 0)
        //        {
        //            //dictionary[key] = value;

        //            //Indicates that creating the SQLiteDatabase went succesfully, so the database can be committed.
        //            tran.Complete();

        //        }
        //    }
        //    if (res > 0)
        //        OnItemChanged("AddOrUpdate", key, value);

        //    return res;
        //}

        /// <summary>
        ///    Adds a key/value pair to the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        ///    if the key does not already exist.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="value">The value to be added or updated for an absent key</param>
        /// <returns></returns>
        public virtual void Add(string key, T value)
        {

            TryAdd(key, value);

            //int res = 0;
            //using (TransactionScope tran = new TransactionScope())
            //{
            //    //Insert create script here.
            //    using (var db = new DbLite(ConnectionString))
            //    {
            //        DbAdd(db, key, value);
            //    }
            //    if (res > 0)
            //    {
            //        dictionary[key] = value;

            //        //Indicates that creating the SQLiteDatabase went succesfully, so the database can be committed.
            //        tran.Complete();

            //        OnItemChanged("Add",key,value);
            //    }
            //}

            //return res;
        }

 
        /// <summary>
        ///    updates a key/value pair to the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        ///    if the key already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="value">The value to be added or updated for an absent key</param>
        /// <returns></returns>
        public virtual bool Remove(string key)
        {
            T value = default(T);
            return TryRemove(key, out value);

            //int res = 0;
            //T value=default(T);
            //using (TransactionScope tran = new TransactionScope())
            //{
            //    //Insert create script here.
            //    using (var db = new DbLite(ConnectionString))
            //    {
            //        res = DbDelete(db, key);
            //    }
            //    if (res > 0)
            //    {
            //        if(dictionary.TryRemove(key, out value))
            //        {
            //            //Indicates that creating the SQLiteDatabase went succesfully, so the database can be committed.
            //            tran.Complete();

            //            OnItemChanged("Remove", key, value);
            //        }
            //        else
            //        {
            //            res = -1;
            //        }
            //    }
            //}

            //return value;
        }


        //public T AddOrUpdate(GenericEntity entity)
        //{
        //    //var value=entity.Record.ToJson();
        //    string key = entity.PrimaryKey.ToString();
        //    int res = 0;
        //               using (TransactionScope tran = new TransactionScope())
        //    {
        //        //Insert create script here.
        //        using (var db = new DbLite(connectionString))
        //        {
        //            res = db.EntityUpsert(entity);

        //        }
        //        if (res > 0)
        //        {
        //            dictionary[key] = entity;

        //            //Indicates that creating the SQLiteDatabase went succesfully, so the database can be committed.
        //            tran.Complete();
        //        }
        //    }

        //    return res;
        //}


        //
        // Summary:
        //     Removes all keys and values from the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>.

        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            string cmdText = "delete from [" + Name + "]";
            bool iscommited = false;

            try
            {
                //Insert create script here.
                using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
                {
                    db.ExecuteTransCommandNonQuery(cmdText, (result) =>
                     {
                         if (result > 0)
                         {
                             dictionary.Clear();
                             //trans.Commit();
                             iscommited = true;
                         }
                         return iscommited;
                     });
                }
                if (iscommited)
                {
                    OnClearCompleted(); //OnItemChanged("Clear", null, default(T));
                }
            }
            catch (Exception ex)
            {
                string err = ex.Message;
                OnErrorOcurred("Clear", ex.Message);
            }

        }


        /// <summary>
        /// Determines whether the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        ///     contains the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>.</param>
        /// <returns>
        ///     true if the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        ///     contains an element with the specified key; otherwise, false.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">key is a null reference</exception>
        public bool ContainsKey(string key)
        {
            return dictionary.ContainsKey(key);
        }
        
        //
        // Summary:
        //     Adds a key/value pair to the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        //     if the key does not already exist.
        //
        // Parameters:
        //   key:
        //     The key of the element to add.
        //
        //   valueFactory:
        //     The function used to generate a value for the key
        //
        // Returns:
        //     The value for the key. This will be either the existing value for the key
        //     if the key is already in the dictionary, or the new value for the key as
        //     returned by valueFactory if the key was not in the dictionary.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     key is a null reference (Nothing in Visual Basic).-or-valueFactory is a null
        //     reference (Nothing in Visual Basic).
        //
        //   System.OverflowException:
        //     The dictionary already contains the maximum number of elements, System.Int32.MaxValue.
        //public T GetOrAdd(string key, Func<string, T> valueFactory)
        //{
        //    return dictionary.GetOrAdd(key,valueFactory);
        //}
        //
        // Summary:
        //     Adds a key/value pair to the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        //     if the key does not already exist.
        //
        // Parameters:
        //   key:
        //     The key of the element to add.
        //
        //   value:
        //     the value to be added, if the key does not already exist
        //
        // Returns:
        //     The value for the key. This will be either the existing value for the key
        //     if the key is already in the dictionary, or the new value if the key was
        //     not in the dictionary.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     key is a null reference (Nothing in Visual Basic).
        //
        //   System.OverflowException:
        //     The dictionary already contains the maximum number of elements, System.Int32.MaxValue.

        /// <summary>
        /// Adds a key/value pair to the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        ///     if the key does not already exist.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">the value to be added, if the key does not already exist</param>
        /// <returns>
        /// The value for the key. This will be either the existing value for the key
        ///     if the key is already in the dictionary, or the new value if the key was
        ///     not in the dictionary.
        /// </returns>
        /// <exception cref="System.ArgumentNullException"> key is a null reference.</exception>
        /// <exception cref="System.OverflowException"> The dictionary already contains the maximum number of elements, System.Int32.MaxValue.</exception>
        public T GetOrAdd(string key, T value)
        {
            T val;
            if (dictionary.TryGetValue(key, out val))
            {
                return val;
            }
            if(TryAdd(key,value))
            {
                return value;
            }
            //int res = TryAdd(key, value);
            //if (res > 0)
            //{
            //    return value;
            //}
            return default(T);
            //return dictionary.GetOrAdd(key, value);
        }
      
        /// <summary>
        /// Copies the key and value pairs stored in the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        ///     to a new array.
        /// </summary>
        /// <returns>
        ///  A new array containing a snapshot of key and value pairs copied from the
        ///     System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>.
        /// </returns>
        public KeyValuePair<string, T>[] ToArray()
        {
            return dictionary.ToArray();
        }


 
        /// <summary>
        /// Summary:
        ///     Attempts to get the value associated with the specified key from the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>.
        ///
        /// Parameters:
        ///   key:
        ///     The key of the value to get.
        ///
        ///   value:
        ///     When this method returns, value contains the object from the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        ///     with the specified key or the default value of , if the operation failed.
        ///
        /// Returns:
        ///     true if the key was found in the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>;
        ///     otherwise, false.
        ///
        /// Exceptions:
        ///   System.ArgumentNullException:
        ///     key is a null reference (Nothing in Visual Basic).
        /// </summary>
        public bool TryGetValue(string key, out T value)
        {
            //switch (_CommitMode)
            //{
            //    case CommitMode.OnDisk:
            //         if(TryRemove(key,out value))
            //        {

            //        }
            //        break;
            //    default:

            //        break;
            //}
                    return dictionary.TryGetValue(key, out value);
        }

        #endregion

        #region IDictionary implemenation

        /// <summary>
        /// Summary:
        ///     Gets an System.Collections.Generic.ICollection<T> containing the keys of
        ///     the System.Collections.Generic.IDictionary<TKey,TValue>.
        ///
        /// Returns:
        ///     An System.Collections.Generic.ICollection<T> containing the keys of the object
        ///     that implements System.Collections.Generic.IDictionary<TKey,TValue>.
        /// </summary>
        public ICollection<string> Keys { get { return dictionary.Keys; } }

        /// <summary>
        /// Summary:
        ///     Gets an System.Collections.Generic.ICollection<T> containing the values in
        ///     the System.Collections.Generic.IDictionary<TKey,TValue>.
        ///
        /// Returns:
        ///     An System.Collections.Generic.ICollection<T> containing the values in the
        ///     object that implements System.Collections.Generic.IDictionary<TKey,TValue>.
        /// </summary>
        public ICollection<T> Values { get { return dictionary.Values; } }

        /// <summary>
        /// Summary:
        ///     Gets or sets the element with the specified key.
        ///
        /// Parameters:
        ///   key:
        ///     The key of the element to get or set.
        ///
        /// Returns:
        ///     The element with the specified key.
        ///
        /// Exceptions:
        ///   System.ArgumentNullException:
        ///     key is null.
        ///
        ///   System.Collections.Generic.KeyNotFoundException:
        ///     The property is retrieved and key is not found.
        ///
        ///   System.NotSupportedException:
        ///     The property is set and the System.Collections.Generic.IDictionary<TKey,TValue>
        ///     is read-only.
        /// </summary>
        public T this[string key]
        {
            get { return dictionary[key]; }
            set
            {

                AddOrUpdate(key, value);
            }
        }

        // Summary:
        //     Adds an element with the provided key and value to the System.Collections.Generic.IDictionary<TKey,TValue>.
        //
        // Parameters:
        //   key:
        //     The object to use as the key of the element to add.
        //
        //   value:
        //     The object to use as the value of the element to add.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     key is null.
        //
        //   System.ArgumentException:
        //     An element with the same key already exists in the System.Collections.Generic.IDictionary<TKey,TValue>.
        //
        //   System.NotSupportedException:
        //     The System.Collections.Generic.IDictionary<TKey,TValue> is read-only.
        //public void Add(string key, T value)
        //{
        //    dictionary.Add(key,value);
        //}
        //
        // Summary:
        //     Determines whether the System.Collections.Generic.IDictionary<TKey,TValue>
        //     contains an element with the specified key.
        //
        // Parameters:
        //   key:
        //     The key to locate in the System.Collections.Generic.IDictionary<TKey,TValue>.
        //
        // Returns:
        //     true if the System.Collections.Generic.IDictionary<TKey,TValue> contains
        //     an element with the key; otherwise, false.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     key is null.
        //public bool ContainsKey(string key)
        //{
        //    return dictionary.ContainsKey(key);
        //}
        //
        // Summary:
        //     Removes the element with the specified key from the System.Collections.Generic.IDictionary<TKey,TValue>.
        //
        // Parameters:
        //   key:
        //     The key of the element to remove.
        //
        // Returns:
        //     true if the element is successfully removed; otherwise, false. This method
        //     also returns false if key was not found in the original System.Collections.Generic.IDictionary<TKey,TValue>.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     key is null.
        //
        //   System.NotSupportedException:
        //     The System.Collections.Generic.IDictionary<TKey,TValue> is read-only.
        //public bool Remove(string key)
        //{
        //    return dictionary.Remove(key);
        //}
        //
        // Summary:
        //     Gets the value associated with the specified key.
        //
        // Parameters:
        //   key:
        //     The key whose value to get.
        //
        //   value:
        //     When this method returns, the value associated with the specified key, if
        //     the key is found; otherwise, the default value for the type of the value
        //     parameter. This parameter is passed uninitialized.
        //
        // Returns:
        //     true if the T that implements System.Collections.Generic.IDictionary<TKey,TValue>
        //     contains an element with the specified key; otherwise, false.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     key is null.
        //public bool TryGetValue(string key, out T value)
        //{
        //    return dictionary.TryGetValue(key,out value);
        //}

        #endregion

        #region ICollection<KeyValuePair<TKey, TValue>>

        // Summary:
        //     Gets the number of elements contained in the System.Collections.Generic.ICollection<T>.
        //
        // Returns:
        //     The number of elements contained in the System.Collections.Generic.ICollection<T>.
        public int Count { get { return dictionary.Count; } }
        //
        // Summary:
        //     Gets a value indicating whether the System.Collections.Generic.ICollection<T>
        //     is read-only.
        //
        // Returns:
        //     true if the System.Collections.Generic.ICollection<T> is read-only; otherwise,
        //     false.
        public bool IsReadOnly { 
            get 
            {
                return false;// dictionary.IsReadOnly; 
            } 
        }

        // Summary:
        //     Adds an item to the System.Collections.Generic.ICollection<T>.
        //
        // Parameters:
        //   item:
        //     The object to add to the System.Collections.Generic.ICollection<T>.
        //
        // Exceptions:
        //   System.NotSupportedException:
        //     The System.Collections.Generic.ICollection<T> is read-only.
        public void Add(KeyValuePair<string, T> item)
        {
            Add(item.Key, item.Value);
            //dictionary.Add(item);
        }
        //
        // Summary:
        //     Removes all items from the System.Collections.Generic.ICollection<T>.
        //
        // Exceptions:
        //   System.NotSupportedException:
        //     The System.Collections.Generic.ICollection<T> is read-only.
        //public void Clear()
        //{

        //}
        //
        // Summary:
        //     Determines whether the System.Collections.Generic.ICollection<T> contains
        //     a specific value.
        //
        // Parameters:
        //   item:
        //     The object to locate in the System.Collections.Generic.ICollection<T>.
        //
        // Returns:
        //     true if item is found in the System.Collections.Generic.ICollection<T>; otherwise,
        //     false.
        public bool Contains(KeyValuePair<string, T> item)
        {
            T val;
            if(TryGetValue(item.Key,out val))
            {
                return item.Value.Equals(val);
            }
            return false;

            //return dictionary.ContainsKey(item.Key);
        }
        //
        // Summary:
        //     Copies the elements of the System.Collections.Generic.ICollection<T> to an
        //     System.Array, starting at a particular System.Array index.
        //
        // Parameters:
        //   array:
        //     The one-dimensional System.Array that is the destination of the elements
        //     copied from System.Collections.Generic.ICollection<T>. The System.Array must
        //     have zero-based indexing.
        //
        //   arrayIndex:
        //     The zero-based index in array at which copying begins.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     array is null.
        //
        //   System.ArgumentOutOfRangeException:
        //     arrayIndex is less than 0.
        //
        //   System.ArgumentException:
        //     The number of elements in the source System.Collections.Generic.ICollection<T>
        //     is greater than the available space from arrayIndex to the end of the destination
        //     array.
        public void CopyTo(KeyValuePair<string, T>[] array, int arrayIndex)
        {
            array=dictionary.ToArray();//.CopyTo(array,arrayIndex);
        }
        //
        // Summary:
        //     Removes the first occurrence of a specific object from the System.Collections.Generic.ICollection<T>.
        //
        // Parameters:
        //   item:
        //     The object to remove from the System.Collections.Generic.ICollection<T>.
        //
        // Returns:
        //     true if item was successfully removed from the System.Collections.Generic.ICollection<T>;
        //     otherwise, false. This method also returns false if item is not found in
        //     the original System.Collections.Generic.ICollection<T>.
        //
        // Exceptions:
        //   System.NotSupportedException:
        //     The System.Collections.Generic.ICollection<T> is read-only.
        public bool Remove(KeyValuePair<string, T> item)
        {
            T val;
            return TryRemove(item.Key, out val);
           // return dictionary.Remove(item);
        }

        #endregion

        #region IEnumerable<out T> : IEnumerable
       
            // Summary:
            //     Returns an enumerator that iterates through the collection.
            //
            // Returns:
            //     A System.Collections.Generic.IEnumerator<T> that can be used to iterate through
            //     the collection.
        public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
        {
            return dictionary.GetEnumerator();// ((IDictionary<string, T>)dictionary).GetEnumerator();
        }


        #endregion

        #region select query

        public T SelectValue(string key)
        {
            //T value-default(T);
            var sql = DbLookupCommand();
            using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
            {
                var res = db.QuerySingle<PI>(sql, "key", key);
                
                if (EnableCompress)
                {
                    return DecompressValue(res);
                }
                return FromPersistItem(res);
            }
        }

        public IEnumerable<IPersistEntity> QueryDictionaryItems()
        {

            List<IPersistEntity> list = new List<IPersistEntity>();
            try
            {
                if (dictionary != null && dictionary.Count > 0)
                {
                    foreach (var g in this.dictionary)
                    {
                        list.Add(new PersistItem() { body = g.Value, key = g.Key, name = this.Name, timestamp = DateTime.Now });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return list;
        }
        public IList<PersistItem> QueryItems(string select, string where, params object[] keyValueParameters)
        {
            using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
            {
                var sql = DbSelectCommand(select, where);
                var list = db.Query<PersistItem>(sql, keyValueParameters);
                return list;
            }
        }

        public IList<PersistItem> QueryLabels(string select, string where, params object[] keyValueParameters)
        {
            using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
            {
                var sql = DbSelectCommand(select, where);
                var list = db.Query<PersistItem>(sql, keyValueParameters);
                return list;
            }
        }
        

        public IList<T> Query(string select, string where, params object[] keyValueParameters)
        {
            IList<T> list = new List<T>();
            using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
            {
                var sql = DbSelectCommand(select, where);
                var res = db.Query<PI>(sql, keyValueParameters);
                if (EnableCompress)
                {
                    for(int i=0;i< res.Count;i++)
                    {
                        var item = DecompressValue(res[i]);
                        list.Add(item);
                        //res[i] = item;
                    }
                }
                else
                {
                    for (int i = 0; i < res.Count; i++)
                    {
                        var item = FromPersistItem(res[i]);
                        list.Add(item);
                        //res[i] = item;
                    }
                }
                return list;
            }
        }

        public T QuerySingle(string select, string where, params object[] keyValueParameters)
        {
            using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
            {
                var sql = DbSelectCommand(select, where);
                var res = db.QuerySingle<PI>(sql, keyValueParameters);
                if (EnableCompress)
                {
                    return DecompressValue(res);
                }
                return FromPersistItem(res);
            }
        }

        //public TVal QuerySingle<TVal>(string select, string where, TVal returnIfNull, params object[] keyValueParameters)
        //{
        //    using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
        //    {
        //        var sql = DbSelectCommand(select, where);
        //        var res = db.QueryScalar<TVal>(sql, returnIfNull, keyValueParameters);

        //        return res;
        //    }
        //}


        #endregion

        public void ReloadOrClearPersist(bool loadPersistItems)
        {
            if (loadPersistItems)
            {
                dictionary.Clear();
                LoadDbAsync();
            }
            else
                Clear();
        }

        public void Load(IDictionary<string, T> dict)
        {
            ignorEvent = true;
            try
            {

                foreach (KeyValuePair<string, T> entry in dict)
                {
                    AddOrUpdate(entry.Key, entry.Value);
                }
            }
            finally
            {
                ignorEvent = false;
            }
        }

        /// <summary>
        /// Compress data to byte array
        /// </summary>
        /// <param name="value"></param>
        /// <param name="level">from 1-9, 1 is fast p is bettr</param>
        /// <returns></returns>
        public byte[] Compress(string value,int level)
        {
           return Nistec.Generic.NetZipp.Compress(value);
        }

        public string Decompress(byte[] b)
        {
            return Nistec.Generic.NetZipp.Decompress(b);
        }

        /// <summary>
        /// Compress data to byte array
        /// </summary>
        /// <param name="value"></param>
        /// <param name="level">from 1-9, 1 is fast p is bettr</param>
        /// <returns></returns>
        public string Zip(string value, int level)
        {
            return Nistec.Generic.NetZipp.Zip(value);
        }

        public string UnZip(string compressed)
        {
            return Nistec.Generic.NetZipp.UnZip(compressed);
        }

        public Task<int> ExecuteTask(string cmdText, IDbDataParameter[] parameters)
        {
            return Task.Run(() =>
            {
                return Execute(cmdText, parameters);
            });
            //return task.Result;
            //return Task.Factory.StartNew<int>(() => Execute(cmdText, parameters));
        }

        public void ExecuteAsync(string cmdText, IDbDataParameter[] parameters, Action<int> action)
        {
            Task.Run(() =>
            {
                var result = Execute(cmdText, parameters);
                action(result);
            });// ..ConfigureAwait(false).GetAwaiter().GetResult();//.Result;
            //var result = Task.Factory.StartNew<int>(() => Execute(cmdText, parameters));
            //return (result == null) ? 0 : result.Result;
        }

        public int Execute(string cmdText, IDbDataParameter[] parameters)
        {
            using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
            {
                return db.ExecuteCommandNonQuery(cmdText, parameters);
            }
        }

        #region watcher
        /*
        FileSystemWatcher watcher;

        /// <summary>
        /// Initilaize the file system watcher
        /// </summary>
        void InitWatcher()
        {
            string fpath = Path.GetDirectoryName(settings.Filename);
            string filter = Path.GetExtension(settings.Filename);

             // Create a new FileSystemWatcher and set its properties.
            watcher = new FileSystemWatcher();
            watcher.Path = fpath;
            // Watch for changes in LastAccess and LastWrite times, and 
            //   the renaming of files or directories. 
         * 
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
               | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            // Only watch text files.
            watcher.Filter ="*"+ filter;// "*.txt";

            // Add event handlers.
            watcher.Changed += new FileSystemEventHandler(WatchFile_Changed);
            watcher.Created += new FileSystemEventHandler(WatchFile_Changed);
            watcher.Deleted += new FileSystemEventHandler(WatchFile_Changed);
            watcher.Renamed += new RenamedEventHandler(WatchFile_Renamed);

            // Begin watching.
            watcher.EnableRaisingEvents = true;

            // Wait for the user to quit the program.
            //Console.WriteLine("Press \'q\' to quit the sample.");
            //while (Console.Read() != 'q') ;
        }

        /// <summary>
        /// Occoured when file changed
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnFileChanged(FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
            Console.WriteLine("File: " + e.FullPath + " " + e.ChangeType);
            if (e.ChangeType == WatcherChangeTypes.Changed || e.ChangeType == WatcherChangeTypes.Created)
            {
                OnConfigFileChanged(EventArgs.Empty);
            }
        }
        /// <summary>
        /// Occoured when file renamed
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnFileRenamed(RenamedEventArgs e)
        {
            // Specify what is done when a file is renamed.
            Console.WriteLine("File: {0} renamed to {1}", e.OldFullPath, e.FullPath);
            if (e.FullPath == settings.Filename)
            {
                OnConfigFileChanged(EventArgs.Empty);
            }
        }


        void WatchFile_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.FullPath == settings.Filename)
            {
                if (ignorEvent == false)
                    OnFileChanged(e);
            }
        }
        void WatchFile_Renamed(object sender, RenamedEventArgs e)
        {
            if (e.OldFullPath == settings.Filename || e.FullPath == settings.Filename)
            {
                OnFileRenamed(e);
            }
        }
        */
        #endregion


        /*
        /// <summary>
        /// Read Config File
        /// </summary>
        public void Read()
        {
            try
            {
                FileToDictionary();
            }
            catch (Exception ex)
            {
                OnErrorOcurred(new GenericEventArgs<string>("Error occured when try to read Config To Dictionary, Error: " + ex.Message));
            }
        }

        /// <summary>
        /// Save Changes 
        /// </summary>
        public void Save()
        {
            try
            {
                DictionaryToFile();
            }
            catch (Exception ex)
            {
                OnErrorOcurred(new GenericEventArgs<string>("Error occured when try to save Dictionary To Config, Error: " + ex.Message));
            }
        }
        /// <summary>
        /// Print all item to Console
        /// </summary>
        public void PrintConfig()
        {
            Console.WriteLine("<" + settings.RootTag + ">");

            foreach (KeyValuePair<string, T> entry in dictionary)
            {
                Console.WriteLine("key={0}, value={1}, Type={2}", entry.Key , entry.Value,entry.Value==null? "String": entry.Value.GetType().ToString());
            }
            Console.WriteLine("</" + settings.RootTag + ">");

        }
   
    
        /// <summary>
        /// Init new config file from dictionary
        /// </summary>
        /// <param name="dict"></param>
        private void Init()
        {
 
            XmlBuilder builder = new XmlBuilder();
            builder.AppendXmlDeclaration();
            builder.AppendEmptyElement(settings.RootTag, 0);
            foreach (KeyValuePair<string,T> entry in dictionary)
            {
                if (entry.Value == null)
                {
                    builder.AppendElementAttributes(0, "Add", "", new string[]{"key", entry.Key,"value", string.Empty, "type", "String"});
                }
                else
                {
                    builder.AppendElementAttributes(0, "Add", "", new string[] { "key", entry.Key, "value", entry.Value.ToString(), "type", entry.Value.GetType().ToString() });
                }
            }

            if (settings.Encrypted)
                EncryptFile(builder.Document.OuterXml);
            else
                builder.Document.Save(settings.Filename);

            if (settings.UseFileWatcher)
                InitWatcher();
        }
        
        private void FileToDictionary()
        {

            Dictionary<string,T> dict=new Dictionary<string,T>();

            XmlDocument doc = new XmlDocument();

            if (settings.Encrypted)
                doc.LoadXml(DecryptFile());
            else
                doc.Load(settings.Filename);

            Console.WriteLine("Load Config: " + settings.Filename);
            //XmlParser parser=new XmlParser(filename);

            XmlNode app = doc.SelectSingleNode("//" + settings.RootTag);
            XmlNodeList list = app.ChildNodes;

            for (int i = 0; i < list.Count; i++)
            {
                XmlNode node = list[i];
                var attkey = node.Attributes["key"];
                if (attkey != null)
                    dict[attkey.Value] = GetValue(node);

            }
            dictionary = dict;
        }

        private object GetValue(XmlNode node)
        {
            string value = node.Attributes["value"].Value;
            string type ="string";
            XmlAttribute attrib= node.Attributes["type"];

            if (attrib == null)
            {
                return value;
            }

            type = attrib.Value;

            return Types.StringToObject(type, value);
         }

        private void ValidateConfig()
        {
            if (!FileExists && dictionary.Count > 0)
            {
                Init();
            }
        }

        private void DictionaryToFile()
        {
            ValidateConfig();

            XmlDocument doc = new XmlDocument();
            if (settings.Encrypted)
                doc.LoadXml(DecryptFile());
            else
                doc.Load(settings.Filename);

            XmlNode app = doc.SelectSingleNode("//" + settings.RootTag);
            XmlNodeList list = app.ChildNodes;

            for (int i = 0; i < list.Count; i++)
            {
                XmlNode node = list[i];
              
                string key=node.Attributes["key"].Value;
                object value=dictionary[key];

                if (value != null)
                {
                  node.Attributes["value"].Value =value.ToString();
                  node.Attributes["type"].Value = value.GetType().ToString();
                }
                else
                {
                    node.Attributes["value"].Value = string.Empty;
                    node.Attributes["type"].Value ="String";
                }
            }

            if (settings.Encrypted)
            {
                EncryptFile(doc.OuterXml);
            }
            else
                doc.Save(settings.Filename);
        }

       

        private string DecryptFile()
        {
            Encryption en = new Encryption(settings.Password);
            return en.DecryptFileToString(settings.Filename, true);
        }

        private bool EncryptFile(string ouput)
        {
            Encryption en = new Encryption(settings.Password);
            return en.EncryptStringToFile(ouput, settings.Filename, true);
        }
        */

    }
}
