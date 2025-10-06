using Nistec.Collections;
using Nistec.Data.Entities;
using Nistec.Generic;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#pragma warning disable CS1591
namespace Nistec.Data.Persistance
{

    public class TaskerDb : QueueListener<PersistentDbTask>
    {
        public static readonly TaskerDb TaskQueue = new TaskerDb();

        //protected override void OnMessageArraived(Generic.GenericEventArgs<TaskPersistance> e)
        //{
        //    base.OnMessageArraived(e);
        //}

        protected override void OnMessageReceived(GenericEventArgs<PersistentDbTask> e)
        {
            base.OnMessageReceived(e);
            e.Args.ExecuteAsync();
        }

    }

    public class PersistentDbTask
    {
        public string CommandType { get; set; }
        public string CommandText { get; set; }
        public string ConnectionString { get; set; }
        public IDbDataParameter[] Parameters { get; set; }
        public int Result { get; set; }
        public DBProvider DdProvider { get; set; }


        public void ExecuteTask(bool enableTasker)
        {
            //TaskItem item = new TaskItem(() => Execute(), 0);
            if (enableTasker)
                TaskerDb.TaskQueue.Enqueue(this);
            else
                ExecuteAsync();
        }

        public void ExecuteAsync()
        {
            Task.Factory.StartNew(() => Execute());
        }

        public virtual void Execute()
        {
            using (var db = new DbContext(ConnectionString, DdProvider))
            {
                Result = db.ExecuteCommandNonQuery(CommandText, Parameters);
            }
        }
    }
}
