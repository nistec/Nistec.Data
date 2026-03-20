using Nistec.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Nistec.Data.Advanced
{
    public class SqlReader<T, Dbc>
        where T: IEntityItem
        where Dbc:IDbContext
    {
        public int BatchSize { get; protected set; }// = int.Parse(ConfigurationManager.AppSettings["BatchSize"]);
        public string Cmd { get; protected set; }// = "SELECT * FROM Auto_Session_Queue ORDER BY QueueId";
        PropertyInfo[] Props;
        public SqlReader(int batchSize,string cmd)
        {
            BatchSize = batchSize;
            Cmd = cmd;
            Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        }

        public IEnumerable<List<T>> ReadInBatches()
        {
            //int batchSize = int.Parse(ConfigurationManager.AppSettings["BatchSize"]);
            //string sql = "SELECT * FROM Auto_Session_Queue ORDER BY QueueId";
            //using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlConn"].ConnectionString))
            using (var db = (Dbc)Activator.CreateInstance(typeof(Dbc), null))
            using (var conn = new SqlConnection(db.ConnectionString))
            using (var cmd = new SqlCommand(Cmd, conn))
            {
                conn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    List<T> batch = new List<T>();

                    while (r.Read())
                    {
                        batch.Add(Map(r));

                        if (batch.Count >= BatchSize)
                        {
                            yield return batch;
                            batch = new List<T>();
                        }
                    }

                    if (batch.Count > 0)
                        yield return batch;
                }
            }
        }
        protected virtual T Map(SqlDataReader r)
        {
            var item = (T)Activator.CreateInstance(typeof(T), null);
            PropertyInfo[] props =(PropertyInfo[]) Props.Clone();

            foreach (PropertyInfo p in props)
            {

                if (p.PropertyType == typeof(int))
                    p.SetValue(item, r.GetInt32(r.GetOrdinal(p.Name)));
                else if (p.PropertyType == typeof(decimal))
                    p.SetValue(item, r.GetDecimal(r.GetOrdinal(p.Name)));
                else if (p.PropertyType == typeof(DateTime))
                    p.SetValue(item, r.GetDateTime(r.GetOrdinal(p.Name)));
                else if (p.PropertyType == typeof(byte))
                    p.SetValue(item, r.GetByte(r.GetOrdinal(p.Name)));
                else if (p.PropertyType == typeof(bool))
                    p.SetValue(item, r.GetBoolean(r.GetOrdinal(p.Name)));
                else if (p.PropertyType == typeof(int?))
                    p.SetValue(item, r.IsDBNull(r.GetOrdinal(p.Name)) ? (int?)null : r.GetInt32(r.GetOrdinal(p.Name)));
                else if (p.PropertyType == typeof(short?))
                    p.SetValue(item, r.IsDBNull(r.GetOrdinal(p.Name)) ? (short?)null : r.GetInt16(r.GetOrdinal(p.Name)));
                else if (p.PropertyType == typeof(byte?))
                    p.SetValue(item, r.IsDBNull(r.GetOrdinal(p.Name)) ? (byte?)null : r.GetByte(r.GetOrdinal(p.Name)));
                else if (p.PropertyType == typeof(bool?))
                    p.SetValue(item, r.IsDBNull(r.GetOrdinal(p.Name)) ? (bool?)null : r.GetBoolean(r.GetOrdinal(p.Name)));
                else if (p.PropertyType == typeof(DateTime?))
                    p.SetValue(item, r.IsDBNull(r.GetOrdinal(p.Name)) ? (DateTime?)null : r.GetDateTime(r.GetOrdinal(p.Name)));
                else if (p.PropertyType == typeof(string))
                    p.SetValue(item, r.IsDBNull(r.GetOrdinal(p.Name)) ? null : r.GetString(r.GetOrdinal(p.Name)));

            }
            return item;
        }

        /*
        private static AutoSessionQueue __Map(SqlDataReader r)
        {
            return new AutoSessionQueue
            {
                QueueId = r.GetString(r.GetOrdinal("QueueId")),
                TransId = r.GetInt32(r.GetOrdinal("TransId")),
                SessionId = r.GetInt32(r.GetOrdinal("SessionId")),
                MessageId = r.GetInt32(r.GetOrdinal("MessageId")),
                ItemPrice = r.GetDecimal(r.GetOrdinal("ItemPrice")),
                UnitsQouta = r.IsDBNull(r.GetOrdinal("UnitsQouta")) ? (byte?)null : r.GetByte(r.GetOrdinal("UnitsQouta")),
                State = r.GetInt32(r.GetOrdinal("State")),
                Creation = r.GetDateTime(r.GetOrdinal("Creation")),
                Expiration = r.GetDateTime(r.GetOrdinal("Expiration")),
                ExecTime = r.GetDateTime(r.GetOrdinal("ExecTime")),
                Account_Id = r.GetInt32(r.GetOrdinal("Account_Id")),
                BatchId = r.GetInt32(r.GetOrdinal("BatchId")),
                Args = r.IsDBNull(r.GetOrdinal("Args")) ? null : r.GetString(r.GetOrdinal("Args")),
                Server = r.GetInt32(r.GetOrdinal("Server")),
                Priority = r.GetByte(r.GetOrdinal("Priority")),
                PersonalDisplay = r.IsDBNull(r.GetOrdinal("PersonalDisplay")) ? null : r.GetString(r.GetOrdinal("PersonalDisplay")),
                Personal = r.IsDBNull(r.GetOrdinal("Personal")) ? null : r.GetString(r.GetOrdinal("Personal")),
                DataId = r.IsDBNull(r.GetOrdinal("DataId")) ? (int?)null : r.GetInt32(r.GetOrdinal("DataId")),
                Data = r.IsDBNull(r.GetOrdinal("Data")) ? null : r.GetString(r.GetOrdinal("Data")),
                Platform = r.GetByte(r.GetOrdinal("Platform")),
                Sender = r.IsDBNull(r.GetOrdinal("Sender")) ? null : r.GetString(r.GetOrdinal("Sender")),
                Target = r.IsDBNull(r.GetOrdinal("Target")) ? null : r.GetString(r.GetOrdinal("Target")),
                Mt_Id = r.IsDBNull(r.GetOrdinal("Mt_Id")) ? (byte?)null : r.GetByte(r.GetOrdinal("Mt_Id")),
                AproxUnits = r.IsDBNull(r.GetOrdinal("AproxUnits")) ? (byte?)null : r.GetByte(r.GetOrdinal("AproxUnits")),
                EventId = r.IsDBNull(r.GetOrdinal("EventId")) ? (int?)null : r.GetInt32(r.GetOrdinal("EventId")),
                EventFieldType = r.GetByte(r.GetOrdinal("EventFieldType")),
                EventCat = r.IsDBNull(r.GetOrdinal("EventCat")) ? (short?)null : r.GetInt16(r.GetOrdinal("EventCat")),
                Ex_Key = r.IsDBNull(r.GetOrdinal("Ex_Key")) ? null : r.GetString(r.GetOrdinal("Ex_Key")),
                ActualDate = r.IsDBNull(r.GetOrdinal("ActualDate")) ? (DateTime?)null : r.GetDateTime(r.GetOrdinal("ActualDate")),
                BulkKey = r.IsDBNull(r.GetOrdinal("BulkKey")) ? null : r.GetString(r.GetOrdinal("BulkKey"))
            };
        }
        */
    }
}
