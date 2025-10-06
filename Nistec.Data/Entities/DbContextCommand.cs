using Nistec.Data.Ado;
using Nistec.Data.Advanced;
using Nistec.Data.Factory;
using Nistec.Generic;
using Nistec.Runtime;
using Nistec.Serialization;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
#pragma warning disable CS1591
namespace Nistec.Data.Entities
{
    public abstract class DbContextCommand 
    {

        /// <summary>
        /// The time (in seconds) to wait for a connection to open. The default value is 15 seconds.
        /// </summary>
        public const int DefaultConnectionTimeout = 15;

        /// <summary>
        /// The time (in seconds) to wait for a connection to open. The default value is 30 seconds.
        /// </summary>
        public const int DefaultCommandTimeout = 30;


        IDbConnection _Connection;
        IDbTransaction _Transaction;

        #region IDbConnection

        ///// <summary>
        ///// Create IDbCmd
        ///// </summary>
        ///// <returns></returns>
        //public static IDbCmd Create<Dbc>() where Dbc : IDbContext
        //{
        //    IDbContext db = ActivatorUtil.CreateInstance<Dbc>();//System.Activator.CreateInstance<Dbc>();
        //    if (db == null)
        //    {
        //        throw new Entities.EntityException("Create Instance of IDbContext was failed");
        //    }
        //    return Create(db.ConnectionString, db.Provider);
        //}


        ///// <summary>
        ///// Create IDbCmd
        ///// </summary>
        ///// <param name="db"></param>
        ///// <returns></returns>
        //public static IDbCmd Create(IDbContext db)
        //{
        //    if (db == null)
        //    {
        //        throw new ArgumentNullException("db");
        //    }
        //    return Create(db.ConnectionString, db.Provider);
        //}
        ///// <summary>
        ///// Create IDbCmd
        ///// </summary>
        ///// <param name="cnn"></param>
        ///// <returns></returns>
        //public static IDbCmd Create(IDbConnection cnn)
        //{
        //    if (cnn is SqlConnection)
        //        return new SqlClient.DbSqlCmd(cnn as SqlConnection);
        //    else if (cnn is OleDbConnection)
        //        return new OleDb.DbOleCmd(cnn as OleDbConnection);
        //    else
        //        throw new ArgumentException("Provider not supported");
        //}
        /*
        /// <summary>
        /// Create IDbConnection
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static IDbConnection CreateConnection(string connectionString, DBProvider provider)
        {
            if (provider == DBProvider.SqlServer)
                return new SqlConnection(connectionString);
            else if (provider == DBProvider.OleDb)
                return new OleDbConnection(connectionString);
            else
                throw new ArgumentException("Provider not supported");
        }

        /// <summary>
        /// Create IDbConnection
        /// </summary>
        /// <param name="connectionKey"></param>
        /// <returns></returns>
        public static IDbConnection CreateConnection(string connectionKey)
        {
            ConnectionStringSettings cnn = NetConfig.ConnectionContext(connectionKey);
            if (cnn == null)
            {
                throw new Exception("ConnectionStringSettings configuration not found");
            }
            DBProvider provider = GetProvider(cnn.ProviderName);
            if (provider == DBProvider.SqlServer)
                return new SqlConnection(cnn.ConnectionString);
            else if (provider == DBProvider.OleDb)
                return new OleDbConnection(cnn.ConnectionString);
            else
                throw new ArgumentException("Provider not supported");
        }

        public static DBProvider GetProvider(IDbConnection cnn)
        {
            if (cnn is SqlConnection)
                return DBProvider.SqlServer;
            else if (cnn is OleDbConnection)
                return DBProvider.OleDb;
            else
                throw new ArgumentException("Provider not supported");
        }

        /// <summary>
        /// Get Provider
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public static DBProvider GetProvider(IDbCommand cmd)
        {
            if (cmd is SqlCommand)
                return DBProvider.SqlServer;
            else if (cmd is OleDbCommand)
                return DBProvider.OleDb;
            else
                throw new ArgumentException("Provider not supported");
        }

        /// <summary>
        /// Get Provider
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static DBProvider GetProvider(string provider)
        {
            DBProvider Provider;
            switch (provider.ToLower())
            {
                case "system.data.Sqlclient":
                case "Sqlclient":
                case "sqlserver":
                    Provider = DBProvider.SqlServer; break;
                case "oledb":
                    Provider = DBProvider.OleDb; break;
                case "db2":
                    Provider = DBProvider.DB2; break;
                case "firebird":
                    Provider = DBProvider.Firebird; break;
                case "mysql":
                    Provider = DBProvider.MySQL; break;
                case "oracle":
                    Provider = DBProvider.Oracle; break;
                case "sybasease":
                    Provider = DBProvider.SybaseASE; break;
                default:
                    Provider = DBProvider.SqlServer;
                    break;
            }
            return Provider;
        }
        */
        #endregion

        #region Ctor

        /// <summary>
        /// ctor
        /// </summary>
        public DbContextCommand()
        {
            AddWithKey = false;
            ConnectionTimeout = DefaultConnectionTimeout;
            CommandTimeout = DefaultCommandTimeout;
            OwnsConnection = false;
        }
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="cnn"></param>
        public DbContextCommand(IDbConnection cnn):this()
        {
            Database = cnn.Database;
            ConnectionString = cnn.ConnectionString;
            Provider = GetProvider(cnn);
            _Connection = cnn;
            //Init(cnn);
        }

        /// <summary>
        /// ctor DbContextCommand
        /// </summary>
        /// <param name="connectionKey"></param>
        /// <param name="createFromProvider"></param>
        public DbContextCommand(string connectionKey, bool createFromProvider = false) : this()
        {
            if (createFromProvider)
                CreateFromProvider(connectionKey);
            else
                CreateFromConfig(connectionKey);

            _Connection = CreateConnection();
            if (_Connection != null)
                Database = _Connection.Database;
        }

        protected void CreateFromConfig(string connectionKey)
        {
            ConnectionStringSettings cnn = NetConfig.ConnectionContext(connectionKey);
            if (cnn == null)
            {
                throw new Exception("ConnectionStringSettings configuration not found");
            }
            Provider = DbFactory.GetProvider(cnn.ProviderName);
            ConnectionName = cnn.Name;
            ConnectionString = cnn.ConnectionString;
        }

        protected void CreateFromProvider(string connectionKey)
        {
            var cp = ConnectionSettings.Instance.Get(connectionKey);
            if (cp == null)
            {
                throw new InvalidOperationException("Invalid connection provider in ConnectionSettings collection");
            }
            Provider = cp.Provider;
            ConnectionName = cp.FriendlyName;
            ConnectionString = cp.ConnectionString;
        }

        /// <summary>
        /// ctor DbContextCommand
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="provider"></param>
        public DbContextCommand(string connectionString, DBProvider provider) : this()
        {
            ConnectionString = connectionString;
            Provider = provider;
            _Connection = CreateConnection();
            if (_Connection != null)
                Database = _Connection.Database;
            //Init(connectionString, provider);
        }

        #endregion

        #region Dispose

        private bool disposed = false;

        protected void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (_Connection != null)
                    {
                        if (_Connection.State != ConnectionState.Closed)
                            _Connection.Close();
                        _Connection.Dispose();
                        _Connection = null;
                    }
                    ConnectionName = null;
                    ConnectionString = null;
                    Database = null;
                }
                disposed = true;
            }
        }
      
   
        #endregion

        #region override

        /// <summary>
        /// Create Connection
        /// </summary>
        /// <returns></returns>
        protected virtual IDbConnection CreateConnection()//string connectionString, DBProvider provider)
        {
            if(!HasConnection)
            {
                throw new Exception("Invalid connection string");
            }
            if (Provider == DBProvider.SqlServer)
                return new SqlConnection(ConnectionString);
            else if (Provider == DBProvider.OleDb)
                return new OleDbConnection(ConnectionString);
            else
                throw new ArgumentException("Provider not supported");
        }

        /// <summary>
        /// Get Provider
        /// </summary>
        /// <param name="cnn"></param>
        /// <returns></returns>
        protected virtual DBProvider GetProvider(IDbConnection cnn)
        {
            if (cnn is SqlConnection)
                return DBProvider.SqlServer;
            else if (cnn is OleDbConnection)
                return DBProvider.OleDb;
            else
                throw new ArgumentException("Provider not supported");
        }

        /// <summary>
        /// DB Constructor with IAutoBase
        /// </summary>
        internal void Init(IAutoBase dalBase)
        {
            ConnectionString = dalBase.Connection.ConnectionString;
            Provider = dalBase.DBProvider;
            OwnsConnection = false;
            _Connection = dalBase.Connection;
            if (_Connection != null)
                Database = _Connection.Database;
        }


        /// <summary>
        /// DB Constructor with IDbContext
        /// </summary>
        internal void Init<T>() where T : IDbContext
        {
            IDbContext db = ActivatorUtil.CreateInstance<T>();
            ConnectionString = db.ConnectionString;
            Provider = db.Provider;
            OwnsConnection = false;
            _Connection = CreateConnection();
            if (_Connection != null)
                Database = _Connection.Database;
        }


        ///// <summary>
        ///// DB Constructor with connection string
        ///// </summary>
        //internal void Init(string connectionString, DBProvider provider, bool ownConnection=false)
        //{
        //    _Connection = new SqlConnection(ConnectionString);
        //    OwnConnection = ownConnection;
        //    ConnectionString = connectionString;
        //    Provider = provider;
        //}

        ///// <summary>
        ///// DB Constructor with connection
        ///// </summary>
        //internal void Init(IDbConnection connection, bool ownConnection = false)
        //{
        //    _Connection = connection;
        //    OwnConnection = ownConnection;
        //    ConnectionString = connection.ConnectionString;
        //    Provider = DbFactory.GetProvider(connection);

        //}


        /// <summary>
        /// Create Command
        /// </summary>
        /// <param name="cmdText"></param>
        /// <returns></returns>
        protected virtual IDbCommand CreateCommand(string cmdText)
        {
            SqlFormatter.ValidateSql(cmdText, null);
            if (_Connection == null)
            {
                throw new ArgumentNullException("cnn");
            }

            IDbCommand cmd = _Connection.CreateCommand();
            cmd.CommandText = cmdText;
            return cmd;

        }
        /// <summary>
        /// Create Command
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="parameters">SqlParameter array key value.</param>
        /// <returns></returns>
        protected virtual IDbCommand CreateCommand(string cmdText, IDbDataParameter[] parameters)
        {
            SqlFormatter.ValidateSql(cmdText, null);
           
            if (_Connection is SqlConnection)
            {
                SqlCommand cmd = new SqlCommand(cmdText, _Connection as SqlConnection);
                if (parameters != null && parameters.Length > 0)
                {
                    DataParameter.AddSqlParameters(cmd, parameters);
                    //cmd.Parameters.AddRange(parameters);
                }
                return cmd;
            }
            else
            {
                IDbCommand cmd = _Connection.CreateCommand();
                cmd.CommandText = cmdText;
                if (parameters != null && parameters.Length > 0)
                {
                    DataParameter.AddParameters(cmd, parameters);
                }
                return cmd;
            }
        }

        /// <summary>
        /// Create Command
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="parameters">SqlParameter array key value.</param>
        /// <param name="commandType"></param>
        /// <param name="commandTimeOut"></param>
        /// <returns></returns>
        protected virtual IDbCommand CreateCommand(string cmdText, IDbDataParameter[] parameters, CommandType commandType = CommandType.Text, int commandTimeOut = 0)
        {
            SqlFormatter.ValidateSql(cmdText, null);

            if (_Connection is SqlConnection)
            {
                SqlCommand cmd = new SqlCommand(cmdText, _Connection as SqlConnection);
                if (parameters != null && parameters.Length > 0)
                {
                    DataParameter.AddSqlParameters(cmd, parameters);
                    //cmd.Parameters.AddRange(parameters);
                }
                cmd.CommandType = commandType;
                if (commandTimeOut > 0)
                {
                    cmd.CommandTimeout = commandTimeOut;
                }
                return cmd;
            }
            else
            {
                IDbCommand cmd = _Connection.CreateCommand();
                cmd.CommandText = cmdText;
                if (parameters != null && parameters.Length > 0)
                {
                    DataParameter.AddParameters(cmd, parameters);
                }
                cmd.CommandType = commandType;
                if (commandTimeOut > 0)
                {
                    cmd.CommandTimeout = commandTimeOut;
                }
                return cmd;
            }
        }

        #endregion

        #region Connection

        /// <summary>
        /// Validate if entity has connection properties
        /// </summary>
        /// <exception cref="EntityException"></exception>
        public void ValidateConnectionSettings()
        {
            if (string.IsNullOrEmpty(ConnectionString))
            {
                throw new EntityException("Invalid ConnectionString");
            }
        }

        /// <summary>
        /// ConnectionOpen
        /// </summary>
        internal void ConnectionOpen()
        {
            ValidateConnectionSettings();

            if (_Connection == null)
            {
                Connection.Open();
            }
            else if (_Connection.State == ConnectionState.Closed)
            {
                _Connection.Open();
            }
        }

        /// <summary>
        /// ConnectionAutoClose
        /// </summary>
        /// <param name="dispose"></param>
        internal void ConnectionAutoClose(bool dispose = true)
        {
            if (OwnsConnection)
            {
                return;
            }

            if (_Transaction != null)
            {
                _Transaction.Dispose();
                _Transaction = null;
            }
            if (_Connection != null)
            {
                if (_Connection.State != ConnectionState.Closed)
                    _Connection.Close();

                if (dispose)
                {
                    _Connection.Dispose();
                    _Connection = null;
                }
            }
        }

       

   
        /// <summary>
        /// ConnectionClose
        /// </summary>
        protected void ConnectionClose()
        {
            if (_Transaction != null)
            {
                _Transaction.Dispose();
                _Transaction = null;
            }
            if (_Connection != null)
            {
                if (_Connection.State != ConnectionState.Closed)
                    _Connection.Close();
            }
        }

        #endregion

        #region properties

        ///// <summary>
        ///// Get indicate if DbEntites IsEmpty
        ///// </summary>
        //public bool IsEmpty
        //{
        //    get { return string.IsNullOrEmpty(ConnectionString) /*|| Items.Count == 0*/; }
        //}

        /// <summary>
        /// Get indicate if entity has connection properties
        /// </summary>
        public bool HasConnection
        {
            get
            {
                if (string.IsNullOrEmpty(ConnectionString))
                {
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Get indicate if the connection is valid
        /// </summary>
        public bool IsValidConnection
        {
            get
            {
                if (!HasConnection)
                    return false;
                if (_Connection == null || string.IsNullOrEmpty(_Connection.ConnectionString))
                    return false;
                return true;

            }
        }

        /// <summary>
        /// Get or Set the time to wait while trying to establish a connection before terminating the attempt and generating an error.
        /// The time (in seconds) to wait for a connection to open. The default value is 15 seconds.
        /// </summary>
        public int ConnectionTimeout
        {
            get;
            set;
        }
        /// <summary>
        /// Get or Set the time to wait while trying to establish a connection before terminating the attempt and generating an error.
        /// The time (in seconds) to wait for a connection to open. The default value is 30 seconds.
        /// </summary>
        public int CommandTimeout
        {
            get;
            set;
        }


        /// <summary>
        ///     Adds the necessary columns and primary key information to complete the schema.
        ///     For more information about how primary key information is added to a System.Data.DataTable,
        ///     see System.Data.IDataAdapter.FillSchema(System.Data.DataSet,System.Data.SchemaType).To
        ///     function properly with the .NET Framework Data Provider for OLE DB, AddWithKey
        ///     requires that the native OLE DB provider obtains necessary primary key information
        ///     by setting the DBPROP_UNIQUEROWS property, and then determines which columns
        ///     are primary key columns by examining DBCOLUMN_KEYCOLUMN in the IColumnsRowset.
        ///     As an alternative, the user may explicitly set the primary key constraints on
        ///     each System.Data.DataTable. This ensures that incoming records that match existing
        ///     records are updated instead of appended. When using AddWithKey, the .NET Framework
        ///     Data Provider for SQL Server appends a FOR BROWSE clause to the statement being
        ///     executed. The user should be aware of potential side effects, such as interference
        ///     with the use of SET FMTONLY ON statements. See SQL Server Books Online for more
        ///     information.
        /// </summary>
        public bool AddWithKey
        {
            get;
            set;
        }
        

        /// <summary>
        /// Get or Set if <see cref="DbContextCommand"/> own the Connection, Default is false
        /// </summary>
        public bool OwnsConnection
        {
            get;
            set;
        }

        /// <summary>
        /// Get or Set ConnectionName
        /// </summary>
        public string ConnectionName
        {
            get;
            protected set;
        }

        /// <summary>
        /// Get or Set ConnectionString
        /// </summary>
        public string ConnectionString
        {
            get;
            protected set;
        }

        /// <summary>
        /// Get or Set Database
        /// </summary>
        public string Database
        {
            get;
            protected set;
        }

        DBProvider m_Provider = DBProvider.SqlServer;
        /// <summary>
        /// Get or Set DBProvider
        /// </summary>
        public DBProvider Provider
        {
            get { return m_Provider; }
            protected set { m_Provider = value; }
        }

        public IDbConnection Connection
        {
            get
            {
                ValidateConnectionSettings();

                if (_Connection == null || string.IsNullOrEmpty(_Connection.ConnectionString))
                {
                    _Connection = CreateConnection();
                }
                return _Connection;
            }
            protected set { _Connection = value; }
        }

        public IDbTransaction Transaction
        {
            get
            {
                return _Transaction;
            }
            //protected set { _Transaction = value; }
        }

        [EntityProperty(EntityPropertyType.NA)]
        public virtual JsonSettings JsonSettings
        {
            get; set;
        }

        #endregion

        #region Execute none query

        /// <summary>
        /// Executes a command NonQuery and returns the number of rows affected.
        /// </summary>
        /// <param name="cmdText">Sql command.</param>
        /// <returns></returns> 
        public int ExecuteCommandNonQuery(string cmdText)
        {
            return ExecuteCommandNonQuery(cmdText, null);
        }
        
        /// <summary>
        /// Executes a command NonQuery and returns the number of rows affected.
        /// </summary>
        /// <param name="cmdText">Sql command.</param>
        /// <param name="parameters">SqlParameter array key value.</param>
        /// <param name="commandType">Specifies how a command string is interpreted.</param>
        /// <param name="commandTimeOut">Set the command time out, default =0</param>
        /// <returns></returns> 
        public int ExecuteCommandNonQuery(string cmdText, IDbDataParameter[] parameters, CommandType commandType = CommandType.Text, int commandTimeOut = 0)
        {
            if (cmdText == null)
            {
                throw new ArgumentNullException("ExecuteNonQuery.commandText");
            }
            try
            {
                ConnectionOpen();
                using (IDbCommand cmd = CreateCommand(cmdText, parameters))
                {
                    cmd.CommandType = commandType;
                    if (commandTimeOut > 0)
                    {
                        cmd.CommandTimeout = commandTimeOut;
                    }
                    return cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ConnectionAutoClose();
            }
        }

        /// <summary>
        /// Executes a command NonQuery and returns <see cref="EntityCommandResult"/> OutptResults and the number of rows affected.
        /// </summary>
        /// <param name="cmdText">Sql command.</param>
        /// <param name="parameters">SqlParameter array key value.</param>
        /// <param name="commandType">Specifies how a command string is interpreted.</param>
        /// <param name="commandTimeOut">Set the command time out, default =0</param>
        /// <returns></returns> 
        public EntityCommandResult ExecuteCommandOutput(string cmdText, IDbDataParameter[] parameters, CommandType commandType = CommandType.Text, int commandTimeOut = 0)
        {
            if (cmdText == null)
            {
                throw new ArgumentNullException("ExecuteNonQuery.ExecuteCommandOutput.cmdText");
            }
            try
            {

                ConnectionOpen();

                int res = 0;
                using (IDbCommand cmd = CreateCommand(cmdText, parameters))
                {
                    cmd.CommandType = commandType;
                    if (commandTimeOut > 0)
                    {
                        cmd.CommandTimeout = commandTimeOut;
                    }
                    res = cmd.ExecuteNonQuery();
                    return RenderOutpuResult(cmd, res);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ConnectionAutoClose();
            }
        }

        /// <summary>
        /// Executes a command NonQuery and returns the ReturnValue from StoredProcedure.
        /// </summary>
        /// <param name="cmdText">Sql command.</param>
        /// <param name="parameters">SqlParameter array key value.</param>
        /// <param name="returnIfNull">Specifies default value to return if null.</param>
        /// <param name="commandTimeOut">Set the command time out, default =0</param>
        /// <returns></returns> 
        public int ExecuteCommandReturnValue(string cmdText, IDbDataParameter[] parameters, int returnIfNull, int commandTimeOut = 0)
        {
            if (cmdText == null)
            {
                throw new ArgumentNullException("ExecuteNonQuery.ExecuteCommandOutput.cmdText");
            }
            if (parameters == null || parameters.Length==0)
            {
                throw new ArgumentNullException("ExecuteNonQuery.ExecuteCommandOutput.parameters");
            }
            
            try
            {

                var retuenParam= parameters.Where(p => p.Direction == ParameterDirection.ReturnValue).FirstOrDefault();
                if (retuenParam==null)
                {
                    throw new ArgumentException("Invalid ReturnValue Parameter");
                }
                ConnectionOpen();

                //T res = default(T);
                using (IDbCommand cmd = CreateCommand(cmdText, parameters))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (commandTimeOut > 0)
                    {
                        cmd.CommandTimeout = commandTimeOut;
                    }
                    var res = cmd.ExecuteNonQuery();
                    var result = retuenParam.Value;
                    return GenericTypes.Convert<int>(result, returnIfNull);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ConnectionAutoClose();
            }
        }


//        using (SqlConnection conn = new SqlConnection(getConnectionString()))
//using (SqlCommand cmd = conn.CreateCommand())
//{
//    cmd.CommandText = parameterStatement.getQuery();
//    cmd.CommandType = CommandType.StoredProcedure;
//    cmd.Parameters.AddWithValue("SeqName", "SeqNameValue");

//    var returnParameter = cmd.Parameters.Add("@ReturnVal", SqlDbType.Int);
//    returnParameter.Direction = ParameterDirection.ReturnValue;

//    conn.Open();
//    cmd.ExecuteNonQuery();
//    var result = returnParameter.Value;
//}

 
        protected EntityCommandResult RenderOutpuResult(IDbCommand command, int affectedRecords)
        {
            var outputValues = new Dictionary<string, object>();
            foreach (IDbDataParameter prm in command.Parameters)
            {
                if (prm.Direction != ParameterDirection.Input)
                {
                    outputValues.Add(prm.ParameterName.Replace("@", ""), prm.Value);
                }
            }
            return new EntityCommandResult(affectedRecords, outputValues,null);
        }

        #endregion

        #region Execute none query Trans

        /// <summary>
        /// Commit transactin, return 1 for commit, -1 if rollback succefully, -2 if rolback failed  
        /// </summary>
        /// <returns>return 1 for commit, -1 if rollback succefully, -2 if rolback failed</returns>
        public int Commit(IDbTransaction transaction)
        {

            try
            {
                transaction.Commit();
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Commit Exception Type: {0}", ex.GetType());
                Console.WriteLine("  Message: {0}", ex.Message);

                // Attempt to roll back the transaction.
                try
                {
                    transaction.Rollback();
                    return -1;
                }
                catch (Exception ex2)
                {
                    // This catch block will handle any errors that may have occurred
                    // on the server that would cause the rollback to fail, such as
                    // a closed connection.
                    Console.WriteLine("Rollback Exception Type: {0}", ex2.GetType());
                    Console.WriteLine("  Message: {0}", ex2.Message);

                    return -2;
                }
            }
         }


        /// <summary>
        /// Executes a command NonQuery and returns the number of rows affected.
        /// </summary>
        /// <param name="cmdText">Sql command.</param>
        /// <param name="transAction"></param>
        /// <returns></returns> 
        public int ExecuteTransCommandNonQuery(string cmdText, Func<int, bool> transAction)
        {
            return ExecuteTransCommandNonQuery(cmdText, null, transAction);
        }

        /// <summary>
        /// Executes a command NonQuery and returns the number of rows affected.
        /// </summary>
        /// <param name="cmdText">Sql command.</param>
        /// <param name="parameters">SqlParameter array key value.</param>
        /// <param name="transAction">trnsaction function.</param>
        /// <param name="commandType">Specifies how a command string is interpreted.</param>
        /// <param name="commandTimeOut">Set the command time out, default =0</param>
        /// <param name="level"></param>
        /// <returns></returns> 
        public int ExecuteTransCommandNonQuery(string cmdText, IDbDataParameter[] parameters, Func<int, bool> transAction, CommandType commandType = CommandType.Text, int commandTimeOut = 0, IsolationLevel level = IsolationLevel.Serializable)
        {
            if (cmdText == null)
            {
                throw new ArgumentNullException("ExecuteNonQuery.commandText");
            }
            int res = 0;
            try
            {
                ConnectionOpen();
                _Transaction = Connection.BeginTransaction(level);
               
                using (IDbCommand cmd = CreateCommand(cmdText, parameters, commandType, commandTimeOut))
                {
                    cmd.Transaction = _Transaction;
                    res = cmd.ExecuteNonQuery();
                }
                //if (!OwnsConnection)
                //{
                //    if (transAction.Invoke(res))
                //    {
                //        _Transaction.Commit();
                //    }
                //}
                if (!OwnsConnection)
                {
                    int reesult = Commit(_Transaction);
                    //continue action after commited
                    transAction.Invoke(reesult);
                }
                return res;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ConnectionAutoClose();
            }
        }
        /// <summary>
        /// Executes a command NonQuery and returns <see cref="EntityCommandResult"/> OutptResults and the number of rows affected.
        /// </summary>
        /// <param name="cmdText">Sql command.</param>
        /// <param name="parameters">SqlParameter array key value.</param>
        /// <param name="transAction"></param>
        /// <param name="commandType">Specifies how a command string is interpreted.</param>
        /// <param name="commandTimeOut">Set the command time out, default =0</param>
        /// <param name="level"></param>
        /// <returns></returns> 
        public EntityCommandResult ExecuteTransCommandOutput(string cmdText, IDbDataParameter[] parameters, Action<int, IDbTransaction> transAction, CommandType commandType = CommandType.Text, int commandTimeOut = 0, IsolationLevel level = IsolationLevel.Serializable)
        {
            if (cmdText == null)
            {
                throw new ArgumentNullException("ExecuteNonQuery.ExecuteCommandOutput.cmdText");
            }
            try
            {

                ConnectionOpen();
                _Transaction = Connection.BeginTransaction(level);
                int res = 0;
                using (IDbCommand cmd = CreateCommand(cmdText, parameters, commandType, commandTimeOut))
                {
                    res = cmd.ExecuteNonQuery();
                    transAction.Invoke(res, _Transaction);
                    return RenderOutpuResult(cmd, res);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ConnectionAutoClose();
            }
        }

        /// <summary>
        /// Executes a command NonQuery and returns the ReturnValue from StoredProcedure.
        /// </summary>
        /// <param name="cmdText">Sql command.</param>
        /// <param name="parameters">SqlParameter array key value.</param>
        /// <param name="transAction"></param>
        /// <param name="returnIfNull">Specifies default value to return if null.</param>
        /// <param name="commandTimeOut">Set the command time out, default =0</param>
        /// <param name="level"></param>
        /// <returns></returns> 
        public int ExecuteTransCommandReturnValue(string cmdText, IDbDataParameter[] parameters, Action<int, IDbTransaction> transAction, int returnIfNull, int commandTimeOut = 0, IsolationLevel level = IsolationLevel.Serializable)
        {
            if (cmdText == null)
            {
                throw new ArgumentNullException("ExecuteNonQuery.ExecuteCommandOutput.cmdText");
            }
            if (parameters == null || parameters.Length == 0)
            {
                throw new ArgumentNullException("ExecuteNonQuery.ExecuteCommandOutput.parameters");
            }

            try
            {

                var retuenParam = parameters.Where(p => p.Direction == ParameterDirection.ReturnValue).FirstOrDefault();
                if (retuenParam == null)
                {
                    throw new ArgumentException("Invalid ReturnValue Parameter");
                }
                ConnectionOpen();
                _Transaction = Connection.BeginTransaction(level);
                //T res = default(T);
                using (IDbCommand cmd = CreateCommand(cmdText, parameters, CommandType.StoredProcedure, commandTimeOut))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (commandTimeOut > 0)
                    {
                        cmd.CommandTimeout = commandTimeOut;
                    }
                    var res = cmd.ExecuteNonQuery();
                    var result = retuenParam.Value;
                    transAction.Invoke(res, _Transaction);
                    return GenericTypes.Convert<int>(result, returnIfNull);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ConnectionAutoClose();
            }
        }


        protected EntityCommandResult RenderOutpuResultTrans(IDbCommand command, int affectedRecords)
        {
            var outputValues = new Dictionary<string, object>();
            foreach (IDbDataParameter prm in command.Parameters)
            {
                if (prm.Direction != ParameterDirection.Input)
                {
                    outputValues.Add(prm.ParameterName.Replace("@", ""), prm.Value);
                }
            }
            return new EntityCommandResult(affectedRecords, outputValues, null);
        }

        #endregion

        #region CommandScalar
        /*
        /// <summary>
        /// Executes Command and returns T value as scalar.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmdText">Sql command.</param>
        /// <returns></returns> 
        public T ExecuteCommandScalar<T>(string cmdText)
        {
            return ExecuteCommandScalar<T>(cmdText, null,default(T));
        }
       
        /// <summary>
        /// Executes Command and returns T value as scalar.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmdText">Sql command.</param>
        /// <param name="parameters">SqlParameter array key value.</param>
        /// <param name="commandType">Specifies how a command string is interpreted.</param>
        /// <param name="commandTimeOut">Set the command time out, default =0</param>
        /// <returns></returns> 
        public T ExecuteCommandScalar<T>(string cmdText, DataParameter[] parameters, CommandType commandType = CommandType.Text, int commandTimeOut = 0)
        {
            if (cmdText == null)
            {
                throw new ArgumentNullException("ExecuteScalar.commandText");
            }
            try
            {
                ConnectionOpen();
                using (IDbCommand cmd = DbFactory.CreateCommand(cmdText, _Connection, parameters))
                {

                    cmd.CommandType = commandType;
                    if (commandTimeOut > 0)
                    {
                        cmd.CommandTimeout = commandTimeOut;
                    }
                    object result = cmd.ExecuteScalar();
                    return GenericTypes.Convert<T>(result);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ConnectionAutoClose();
            }
        }
        */
        /// <summary>
        /// Executes Command and returns T value as scalar.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmdText">Sql command.</param>
        /// <param name="parameters">SqlParameter array key value.</param>
        /// <param name="returnIfNull">The value will return if result is null.</param>
        /// <param name="commandType">Specifies how a command string is interpreted.</param>
        /// <param name="commandTimeOut">Set the command time out, default =0</param>
        /// <returns></returns> 
        public T ExecuteCommandScalar<T>(string cmdText, IDbDataParameter[] parameters, T returnIfNull, CommandType commandType = CommandType.Text, int commandTimeOut = 0)
        {
            if (cmdText == null)
            {
                throw new ArgumentNullException("ExecuteScalar.commandText");
            }
            try
            {
                ConnectionOpen();
                using (IDbCommand cmd = CreateCommand(cmdText, parameters))
                {

                    cmd.CommandType = commandType;
                    if (commandTimeOut > 0)
                    {
                        cmd.CommandTimeout = commandTimeOut;
                    }
                    object result = cmd.ExecuteScalar();

                    return GenericTypes.Convert<T>(result, returnIfNull);
                }

            }
            catch (Exception ex)
            {
                string s = ex.Message;
                return returnIfNull;
            }
            finally
            {
                ConnectionAutoClose();
            }
        }


        #endregion

        #region Execute command

        //public T ExecuteCommand<T>(string mappingName, DataFilter filter, CommandType commandType = CommandType.Text, int commandTimeOut = 0, bool addWithKey = false)
        //{
           
        //    if (mappingName == null)
        //    {
        //        throw new ArgumentNullException("ExecuteCommandFilter.mappingName");
        //    }
        //    try
        //    {
        //        string cmdText = filter == null ? SqlFormatter.SelectString(mappingName) : filter.Select(mappingName);
        //        IDbDataParameter[] parameters = filter == null ? null : filter.Parameters;
        //        ConnectionOpen();
        //        using (IDbCommand cmd = DbFactory.CreateCommand(cmdText, _Connection, parameters))
        //        {
        //            cmd.CommandType = commandType;
        //            if (commandTimeOut > 0)
        //            {
        //                cmd.CommandTimeout = commandTimeOut;
        //            }
        //            return AdapterFactory.ExecuteDataOrScalar<T>(cmd, null, addWithKey);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    finally
        //    {
        //        ConnectionAutoClose();
        //    }
        //}

        /// <summary>
        /// Executes Command and returns T value (DataSet|DataTable|DataRow).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmdText">Sql command.</param>
        /// <param name="addWithKey">Adds the primary key columns to complete the schema.</param>
        /// <returns></returns> 
        public T ExecuteCommand<T>(string cmdText, bool addWithKey = false)
        {
            IDbDataParameter[] parameters = null;
            return ExecuteCommand<T>(cmdText, parameters, CommandType.Text, 0, addWithKey);
        }

        /// <summary>
        /// Executes Command and returns T value (DataSet|DataTable|DataRow) .
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="cmdText">Sql command.</param>
        /// <param name="parameters">SqlParameter array key value.</param>
        /// <param name="commandType">Specifies how a command string is interpreted.</param>
        /// <param name="commandTimeOut">Set the command time out, default =0</param>
        /// <param name="addWithKey">Adds the primary key columns to complete the schema.</param>
        /// <returns></returns> 
        public TResult ExecuteCommand<TItem, TResult>(string cmdText, IDbDataParameter[] parameters, CommandType commandType = CommandType.Text, int commandTimeOut = 0, bool addWithKey = false)
        {
            if (cmdText == null)
            {
                throw new ArgumentNullException("ExecuteCommand.commandText");
            }
            try
            {
                 ConnectionOpen();
                 using (IDbCommand cmd = CreateCommand(cmdText, parameters))
                 {
                     cmd.CommandType = commandType;
                     if (commandTimeOut > 0)
                     {
                         cmd.CommandTimeout = commandTimeOut;
                     }
                     return ExecuteDataList<TItem, TResult>(cmd, null, addWithKey);
                 }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ConnectionAutoClose();
            }
        }


        /// <summary>
        /// Executes Command and returns T value (DataSet|DataTable|DataRow) .
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmdText">Sql command.</param>
        /// <param name="parameters">SqlParameter array key value.</param>
        /// <param name="commandType">Specifies how a command string is interpreted.</param>
        /// <param name="commandTimeOut">Set the command time out, default =0</param>
        /// <param name="addWithKey">Adds the primary key columns to complete the schema.</param>
        /// <returns></returns> 
        public T ExecuteCommand<T>(string cmdText, IDbDataParameter[] parameters, CommandType commandType = CommandType.Text, int commandTimeOut = 0, bool addWithKey = false)
        {
            if (cmdText == null)
            {
                throw new ArgumentNullException("ExecuteCommand.commandText");
            }
            try
            {
                 ConnectionOpen();
                 using (IDbCommand cmd = CreateCommand(cmdText, parameters))
                 {

                     cmd.CommandType = commandType;
                     if (commandTimeOut > 0)
                     {
                         cmd.CommandTimeout = commandTimeOut;
                     }
                     return ExecuteDataOrScalar<T>(cmd, null, addWithKey);
                 }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ConnectionAutoClose();
            }
        }

        /// <summary>
        /// Executes Command and returns T value (DataSet|DataTable|DataRow) .
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmdText">Sql command.</param>
        /// <param name="parameters">SqlParameter array key value.</param>
        /// <param name="cacheKey">Get or Set data from/to cache.</param>
        /// <param name="commandType">Specifies how a command string is interpreted.</param>
        /// <param name="commandTimeOut">Set the command time out, default =0</param>
        /// <param name="addWithKey">Adds the primary key columns to complete the schema.</param>
        /// <returns></returns> 
        public T ExecuteCommand<T>(string cmdText, IDbDataParameter[] parameters, string cacheKey, CommandType commandType = CommandType.Text, int commandTimeOut = 0, bool addWithKey = false)
        {
            if (cmdText == null)
            {
                throw new ArgumentNullException("ExecuteCommand.commandText");
            }
            object value = default(T);
            try
            {
                if (AutoDataCache.DW.GetTryParse(cacheKey, out value))
                {
                    return (T)value;
                }
                ConnectionOpen();
                using (IDbCommand cmd = CreateCommand(cmdText, parameters))
                {
                    cmd.CommandType = commandType;
                    if (commandTimeOut > 0)
                    {
                        cmd.CommandTimeout = commandTimeOut;
                    }

                    value = ExecuteDataOrScalar<T>(cmd, null, addWithKey);

                    if (!string.IsNullOrEmpty(cacheKey))
                    {
                        AutoDataCache.DW[cacheKey] = value;
                    }
                    return (T)value;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ConnectionAutoClose();
            }
        }

        #endregion

        #region Execute dataTable

        /// <summary>
        /// Executes Adapter and returns DataTable.
        /// </summary>
        /// <param name="mappingName"></param>
        /// <returns></returns>
        public DataTable ExecuteDataTable(string mappingName)
        {
            string cmdText = SqlFormatter.SelectString(mappingName);
            return ExecuteCommand<DataTable>(cmdText, false);
        }
        /// <summary>
        /// Executes Adapter and returns DataTable.
        /// </summary>
        /// <param name="mappingName"></param>
        /// <param name="addWithKey">Adds the primary key columns to complete the schema.</param>
        /// <returns></returns>
        public DataTable ExecuteDataTable(string mappingName, bool addWithKey = false)
        {
            string cmdText = SqlFormatter.SelectString(mappingName);
            return ExecuteCommand<DataTable>(cmdText, addWithKey);
        }
        /// <summary>
        /// Executes Adapter and returns DataTable.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="cmdText"></param>
        /// <param name="addWithKey">Adds the primary key columns to complete the schema.</param>
        /// <returns></returns>
        public DataTable ExecuteDataTable(string tableName, string cmdText, bool addWithKey = false)
        {
            DataTable dt = ExecuteCommand<DataTable>(cmdText, addWithKey);
            dt.TableName = tableName;
            return dt;
        }

        /// <summary>
        /// Executes Adapter and returns DataTable.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="cmdText"></param>
        /// <param name="commandTimeout">Timeout in seconds</param>
        /// <param name="addWithKey">Adds the primary key columns to complete the schema.</param>
        /// <returns></returns>
        public DataTable ExecuteDataTable(string tableName, string cmdText, int commandTimeout, bool addWithKey = false)
        {
            DataTable dt = ExecuteCommand<DataTable>(cmdText, null, CommandType.Text, commandTimeout, addWithKey);
            dt.TableName = tableName;
            return dt;
        }
        #endregion

        #region DataAdapter

        /// <summary>
        /// CreateIDataAdapter
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        protected virtual IDbDataAdapter CreateIDataAdapter(IDbCommand cmd)
        {
            if (cmd is SqlCommand)
                return new SqlDataAdapter((SqlCommand)cmd) as IDbDataAdapter;
            else if (cmd is OleDbCommand)
                return new OleDbDataAdapter((OleDbCommand)cmd) as IDbDataAdapter;
            else
                throw new DalException("Provider not supported,Only SQL Server and OleDb are valid database types");
        }

        /// <summary>
        /// Fill DataTable using DbDataAdapter
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="mappingName"></param>
        /// <param name="addWithKey">Adds the primary key columns to complete the schema.</param>
        /// <returns></returns>
        protected virtual DataTable ExecuteDataTable(IDbCommand cmd, string mappingName, bool addWithKey)
        {
            DataTable dt = new DataTable(mappingName);
            using (DbDataAdapter dbAdapter = (DbDataAdapter)CreateIDataAdapter(cmd))
            {
                dbAdapter.SelectCommand = (DbCommand)cmd;
                if (addWithKey)
                    dbAdapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;

                dbAdapter.Fill(dt);
            }
            return dt;
        }

        /// <summary>
        /// Fill DataTable using DbDataAdapter
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="mappingName"></param>
        /// <param name="addWithKey">Adds the primary key columns to complete the schema.</param>
        /// <returns></returns>
        protected virtual DataSet ExecuteDataSet(IDbCommand cmd, string mappingName, bool addWithKey)
        {
            DataSet ds = new DataSet();
            if (!string.IsNullOrEmpty(mappingName))
            {
                ds.DataSetName = mappingName;
            }
            using (DbDataAdapter dbAdapter = (DbDataAdapter)CreateIDataAdapter(cmd))
            {
                dbAdapter.SelectCommand = (DbCommand)cmd;
                if (addWithKey)
                    dbAdapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;

                dbAdapter.Fill(ds);

            }
            return ds;
        }

        /// <summary>
        /// Executes command and returns T value (DataSet|DataTable|DataRow|IEntityItem) or any type for scalar.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmd"></param>
        /// <param name="mappingName"></param>
        /// <param name="addWithKey">Adds the primary key columns to complete the schema.</param>
        /// <returns></returns>
        protected T ExecuteDataOrScalar<T>(IDbCommand cmd, string mappingName, bool addWithKey)
        {
            Type type = typeof(T);
            object result = null;
            try
            {

                if (type == typeof(DataSet))
                {
                    result = ExecuteDataSet(cmd, mappingName, addWithKey);
                    return (T)result;
                }
                else if (type == typeof(DataTable))
                {
                    result = ExecuteDataTable(cmd, mappingName, addWithKey);
                    return (T)result;
                }
                else if (type == typeof(DataView))
                {
                    DataTable dt = ExecuteDataTable(cmd, mappingName, addWithKey);
                    if (dt != null)
                    {
                        result = ((DataTable)dt).DefaultView;
                    }
                    return (T)result;
                }
                else if (type == typeof(DataRow))
                {
                    DataTable dt = ExecuteDataTable(cmd, mappingName, addWithKey);
                    if (dt != null && dt.Rows.Count > 0)
                        result = dt.Rows[0];
                    return (T)result;
                }
                else if (type == typeof(System.Data.DataRow[]))
                {
                    DataTable dt = ExecuteDataTable(cmd, mappingName, addWithKey);
                    if (dt != null && dt.Rows.Count > 0)
                        result = dt.Select();
                    return (T)result;
                }
                else if (type == typeof(KeyValueList))
                {
                    DataTable dt = ExecuteDataTable(cmd, mappingName, addWithKey);
                    if (dt != null)//&& dt.Rows.Count > 0)
                        result = KeyValueList.CreateList(dt);
                    return (T)result;
                }
                //else if (typeof(INameValue).IsAssignableFrom(type))
                //{
                //    DataTable dt = ExecuteDataTable(cmd, mappingName, addWithKey);
                //    if (dt != null && dt.Rows.Count > 0)
                //        result = dt.ToNameValue();
                //    return (T)result;
                //}
                else if (typeof(IEntityItem).IsAssignableFrom(type))
                {
                    DataTable dt = ExecuteDataTable(cmd, mappingName, addWithKey);
                    if (dt != null && dt.Rows.Count > 0)
                        return dt.Rows[0].ToEntity<T>();
                    return (T)result;
                }
                else if (typeof(IKeyValueItem).IsAssignableFrom(type))
                {
                    DataTable dt = ExecuteDataTable(cmd, mappingName, addWithKey);
                    if (dt != null)//&& dt.Rows.Count > 0)
                        result = KeyValueItem.Create(dt.Rows[0]);
                    return (T)result;
                }
                else if (type == typeof(GenericRecord))
                {
                    DataTable dt = ExecuteDataTable(cmd, mappingName, addWithKey);
                    if (dt != null && dt.Rows.Count > 0)
                        result = GenericRecord.Parse(dt);
                    return (T)result;
                }
                else if (type == typeof(JsonResults))
                {
                    DataTable dt = ExecuteDataTable(cmd, mappingName, addWithKey);
                    if (dt != null && dt.Rows.Count > 0)
                        result = dt.ToJsonResult(this.JsonSettings);
                    return (T)result;
                }
                else if (typeof(IDataTableAdaptor).IsAssignableFrom(type))
                {
                    DataTable dt = ExecuteDataTable(cmd, mappingName, addWithKey);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        T instance = ActivatorUtil.CreateInstance<T>();
                        ((IDataTableAdaptor)instance).Prepare(dt);
                        return instance;
                    }
                    return (T)result;
                }
                else if (typeof(IDataRowAdaptor).IsAssignableFrom(type))
                {
                    DataTable dt = ExecuteDataTable(cmd, mappingName, addWithKey);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        T instance = ActivatorUtil.CreateInstance<T>();
                        ((IDataRowAdaptor)instance).Prepare(dt.Rows[0]);
                        return instance;
                    }
                    return (T)result;
                }
                else if (SerializeTools.IsEntityClass(type))
                {
                    DataTable dt = ExecuteDataTable(cmd, mappingName, addWithKey);
                    if (dt != null && dt.Rows.Count > 0)
                        return dt.Rows[0].ToEntity<T>();
                    return (T)result;
                }
                else //if (type == typeof(object))
                {
                    result = cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                throw new DalException(ex.Message);
            }

            return InternalCmd.ReturnValue<T>(result);
        }

        /// <summary>
        /// Executes command and returns T value (DataSet|DataTable|DataRow|IEntityItem|List of IEntityItem) or any type for scalar.
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="cmd"></param>
        /// <param name="mappingName"></param>
        /// <param name="addWithKey">Adds the primary key columns to complete the schema.</param>
        /// <returns></returns>
        protected TResult ExecuteDataList<TItem, TResult>(IDbCommand cmd, string mappingName, bool addWithKey)
        {
            Type type = typeof(TResult);
            Type titem = typeof(TItem);
            object result = null;
            try
            {
               if (AdapterFactory.IsAssignableOfList(type, typeof(DataRow)))
                {
                    DataTable dt = ExecuteDataTable(cmd, mappingName, addWithKey);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        IList<DataRow> list = dt.Select().ToList();
                        return (TResult)list;
                    }
                    return (TResult)result;
                }
                else if (AdapterFactory.IsAssignableOfList(type, typeof(IEntityItem)))
                {
                    DataTable dt = ExecuteDataTable(cmd, mappingName, addWithKey);
                    if (dt != null)//&& dt.Rows.Count > 0)
                        result = dt.EntityList<TItem>();
                    return (TResult)result;
                }
                else if (AdapterFactory.IsAssignableOfList(type, typeof(IKeyValueItem)))
                {
                    DataTable dt = ExecuteDataTable(cmd, mappingName, addWithKey);
                    if (dt != null)//&& dt.Rows.Count > 0)
                        result = KeyValueItem.CreateList(dt);
                    return (TResult)result;
                }
                else if (AdapterFactory.IsAssignableOfList(type, typeof(TItem)))
                {
                    DataTable dt = ExecuteDataTable(cmd, mappingName, addWithKey);
                    if (dt != null)//&& dt.Rows.Count > 0)
                    {
                        if (Serialization.SerializeTools.IsSimple(typeof(TItem)))
                            result = dt.ToSimpleList<TItem>(null);
                        else
                            result = dt.EntityList<TItem>();
                    }
                    return (TResult)result;
                }
                else if (AdapterFactory.IsAssignableOfList(type, typeof(GenericRecord)))
                {
                    DataTable dt = ExecuteDataTable(cmd, mappingName, addWithKey);
                    if (dt != null && dt.Rows.Count > 0)
                        result = GenericRecord.ParseList(dt);
                    return (TResult)result;
                }
                else //if (type == typeof(object))
                {
                    DataTable dt = ExecuteDataTable(cmd, mappingName, addWithKey);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        if (Serialization.SerializeTools.IsSimple(typeof(TItem)))
                            result = dt.ToSimpleList<TItem>(null);
                        else
                            result = dt.EntityList<TItem>();
                    }
                    return (TResult)result;

                    //result = cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                throw new DalException(ex.Message);
            }

            //return InternalCmd.ReturnValue<TResult>(result);
        }

        /*
        /// <summary>
        /// Executes command and returns T value (DataSet|DataTable|DataRow|IEntityItem|List of IEntityItem) or any type for scalar.
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="cmd"></param>
        /// <param name="mappingName"></param>
        /// <param name="addWithKey">Adds the primary key columns to complete the schema.</param>
        /// <returns></returns>
        protected TResult ExecuteDataOrScalar<TItem, TResult>(IDbCommand cmd, string mappingName, bool addWithKey)
        {
            Type type = typeof(TResult);
            Type titem = typeof(TItem);
            object result = null;
            try
            {

                if (type == typeof(DataSet))
                {
                    result = ExecuteDataSet(cmd, mappingName, addWithKey);
                    return (TResult)result;
                }
                else if (type == typeof(DataTable))
                {
                    result = ExecuteDataTable(cmd, mappingName, addWithKey);
                    return (TResult)result;
                }
                else if (type == typeof(DataView))
                {
                    DataTable dt = ExecuteDataTable(cmd, mappingName, addWithKey);
                    if (dt != null)
                    {
                        result = ((DataTable)dt).DefaultView;
                    }
                    return (TResult)result;
                }
                else if (type == typeof(DataRow))
                {
                    DataTable dt = ExecuteDataTable(cmd, mappingName, addWithKey);
                    if (dt != null && dt.Rows.Count > 0)
                        result = dt.Rows[0];
                    return (TResult)result;
                }
                else if (type == typeof(System.Data.DataRow[]))
                {
                    DataTable dt = ExecuteDataTable(cmd, mappingName, addWithKey);
                    if (dt != null && dt.Rows.Count > 0)
                        result = dt.Select();
                    return (TResult)result;
                }
                else if (type == typeof(KeyValueList))
                {
                    DataTable dt = ExecuteDataTable(cmd, mappingName, addWithKey);
                    if (dt != null)//&& dt.Rows.Count > 0)
                        result = KeyValueList.CreateList(dt);
                    return (TResult)result;
                }
                else if (typeof(INameValue).IsAssignableFrom(type))
                {
                    DataTable dt = ExecuteDataTable(cmd, mappingName, addWithKey);
                    if (dt != null && dt.Rows.Count > 0)
                        result = dt.ToNameValue();
                    return (TResult)result;
                }
                else if (typeof(IEntityItem).IsAssignableFrom(type))
                {
                    DataTable dt = ExecuteDataTable(cmd, mappingName, addWithKey);
                    if (dt != null && dt.Rows.Count > 0)
                        return dt.Rows[0].ToEntity<TResult>();
                    return (TResult)result;
                }
                else if (typeof(IEntityItem[]).IsAssignableFrom(type))
                {
                    DataTable dt = ExecuteDataTable(cmd, mappingName, addWithKey);
                    if (dt != null)// && dt.Rows.Count > 0)
                        result = dt.EntityList<TItem>().ToArray();
                    return (TResult)result;
                }
                else if (AdapterFactory.IsAssignableOfList(type, typeof(IEntityItem)))
                {
                    DataTable dt = ExecuteDataTable(cmd, mappingName, addWithKey);
                    if (dt != null)//&& dt.Rows.Count > 0)
                        result = dt.EntityList<TItem>();
                    return (TResult)result;
                }
                else if (AdapterFactory.IsAssignableOfList(type, typeof(IKeyValueItem)))
                {
                    DataTable dt = ExecuteDataTable(cmd, mappingName, addWithKey);
                    if (dt != null)//&& dt.Rows.Count > 0)
                        result = KeyValueItem.CreateList(dt);
                    return (TResult)result;
                }
                else if (AdapterFactory.IsAssignableOfList(type, typeof(TItem)))
                {
                    DataTable dt = ExecuteDataTable(cmd, mappingName, addWithKey);
                    if (dt != null)//&& dt.Rows.Count > 0)
                    {
                        if (Serialization.SerializeTools.IsSimple(typeof(TItem)))
                            result = dt.ToSimpleList<TItem>(null);
                        else
                            result = dt.EntityList<TItem>();
                    }
                    return (TResult)result;
                }
                else if (type == typeof(GenericRecord))
                {
                    DataTable dt = ExecuteDataTable(cmd, mappingName, addWithKey);
                    if (dt != null && dt.Rows.Count > 0)
                        result = GenericRecord.Parse(dt);
                    return (TResult)result;
                }
                else if (type == typeof(JsonResults))
                {
                    DataTable dt = ExecuteDataTable(cmd, mappingName, addWithKey);
                    if (dt != null && dt.Rows.Count > 0)
                        result = dt.ToJsonResult();
                    return (TResult)result;
                }
                else if (typeof(IDataTableAdaptor).IsAssignableFrom(type))
                {
                    DataTable dt = ExecuteDataTable(cmd, mappingName, addWithKey);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        TResult instance = ActivatorUtil.CreateInstance<TResult>();
                        ((IDataTableAdaptor)instance).Prepare(dt);
                        return instance;
                    }
                    return (TResult)result;
                }
                else if (typeof(IDataRowAdaptor).IsAssignableFrom(type))
                {
                    DataTable dt = ExecuteDataTable(cmd, mappingName, addWithKey);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        TResult instance = ActivatorUtil.CreateInstance<TResult>();
                        ((IDataRowAdaptor)instance).Prepare(dt.Rows[0]);
                        return instance;
                    }
                    return (TResult)result;
                }
                else //if (type == typeof(object))
                {
                    result = cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                throw new DalException(ex.Message);
            }

            return InternalCmd.ReturnValue<TResult>(result);
        }
        */
        #endregion

        #region Execute Reader

        /// <summary>
        /// GetCommand
        /// </summary>
        /// <param name="cmdText"></param>
        /// <returns></returns>
        internal IDbCommand GetCommand(string cmdText)
        {
            SqlFormatter.ValidateSql(cmdText, "drop|alter");
            return DbFactory.CreateCommand(cmdText, this.Connection);
        }
        /// <summary>
        /// Execute Reader
        /// </summary>
        /// <param name="cmdText">Sql command.</param>
        /// <param name="behavior"></param>
        /// <returns></returns>
        public IDataReader ExecuteReader(string cmdText, CommandBehavior behavior)
        {
            return this.GetCommand(cmdText).ExecuteReader(behavior);
        }

        /// <summary>
        /// Execute Reader
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="behavior"></param>
        /// <param name="parameters">SqlParameter array key value.</param>
        /// <returns></returns>
        public IDataReader ExecuteReader(string cmdText, CommandBehavior behavior, IDbDataParameter[] parameters)
        {
            IDbCommand cmd = CreateCommand(cmdText, this.Connection, parameters);
            return cmd.ExecuteReader(behavior);
        }

        #endregion

        #region static

        /// <summary>
        /// Create Command
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="cnn"></param>
        /// <param name="parameters">SqlParameter array key value.</param>
        /// <returns></returns>
        public static IDbCommand CreateCommand(string cmdText, IDbConnection cnn, IDbDataParameter[] parameters)
        {
            SqlFormatter.ValidateSql(cmdText, null);
            if (cnn == null)
            {
                throw new ArgumentNullException("cnn");
            }
            if (cnn is SqlConnection)
            {
                SqlCommand cmd = new SqlCommand(cmdText, cnn as SqlConnection);
                if (parameters != null && parameters.Length > 0)
                {
                    DataParameter.AddSqlParameters(cmd, parameters);
                    //cmd.Parameters.AddRange(parameters);
                }
                return cmd;
            }
            else
            {
                IDbCommand cmd = cnn.CreateCommand();
                cmd.CommandText = cmdText;
                if (parameters != null && parameters.Length > 0)
                {
                    DataParameter.AddParameters(cmd, parameters);
                }
                return cmd;
            }

        }

        #endregion
    }

}
