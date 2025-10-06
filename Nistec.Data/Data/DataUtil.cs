//licHeader
//===============================================================================================================
// System  : Nistec.Data - Nistec.Data Class Library
// Author  : Nissim Trujman  (nissim@nistec.net)
// Updated : 01/07/2015
// Note    : Copyright 2007-2015, Nissim Trujman, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class that is part of data library.
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
using System.Data;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using System.Collections;
using Nistec.Data.Entities;
using Nistec.Generic;
using System.Reflection;
using System.Linq;
using System.Data.SqlClient;
#pragma warning disable CS1591

namespace Nistec.Data
{
 
	/// <summary>
	/// Data Util.
	/// </summary>
	public static class DataUtil
	{

        public const int MaxRowForNameIndexColumns = 1000;

        public static DateTime? ValidateSqlDate(this DateTime? date)
        {
            if (date.HasValue)
            {
                DateTime? minValue = new DateTime(1900, 1, 1);

                return date < minValue ? minValue : date;
            }
            return date;
        }

        public static DateTime ValidateSqlDate(this DateTime date)
        {
            DateTime minValue = MinSqlValue(date);
            return date < minValue ? minValue : date;
        }

        public static DateTime MinSqlValue(this DateTime date)
        {
            return new DateTime(1900, 1, 1);
        }
        public static DateTime MaxSqlValue(this DateTime date)
        {
            return new DateTime(2900, 21, 31);
        }

        public static object[] CreateValues(params object[] values)
        {
            return values;
        }

        public static MissingSchemaAction GetSchemaAction(bool addWithKey)
        {
            return addWithKey ? MissingSchemaAction.AddWithKey : MissingSchemaAction.Add;
        }

        
        public static DataView DataTop(DataView dv, int top)
        {
            if (dv.Count <= top || top <= 0)
                return dv;

            DataTable dt = dv.Table;
            DataTable cloneDataTable = dt.Clone();
            for (int i = 0; i < top; i++)
            {
                cloneDataTable.ImportRow(dt.Rows[i]);
            }
            return new DataView(cloneDataTable);
        }

        
 
        public static string[] ColumnsFromDataTable(DataTable dt)
        {
            if (dt == null)
                return null;
            List<string> columns = new List<string>();
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                columns.Add(dt.Columns[i].ColumnName);
            }
            return columns.ToArray();
        }

        public static string[] GetColumnsPrimaryKey(DataTable dt)
        {
            if (dt == null || dt.PrimaryKey==null)
                return null;

            List<string> columns = new List<string>();
            foreach (DataColumn col in dt.PrimaryKey)
            {
                columns.Add(col.ColumnName);
            }
            return columns.ToArray();
        }
        public static string[] GetColumnsPrimaryKey(DataColumn[] cols)
        {
            if (cols == null)
                return null;

            List<string> columns = new List<string>();
            foreach (DataColumn col in cols)
            {
                columns.Add(col.ColumnName);
            }
            return columns.ToArray();
        }

        public static string[] ColumnsFromDataRow(DataRow dr)
        {
            List<string> columns = new List<string>();
            if (dr == null)
                return null;
            return ColumnsFromDataTable(dr.Table);
        }

        public static Dictionary<string, Dictionary<string, object>> DatatableToDictionary(DataTable dt, string id)
        {
            var cols = dt.Columns.Cast<DataColumn>();//.Where(c => c.ColumnName != id);
            return dt.Rows.Cast<DataRow>()
                     .ToDictionary(r => r[id].ToString(),
                                   r => cols.ToDictionary(c => c.ColumnName, c => r[c.ColumnName]));
        }
        
        public static void LoadDictionaryFromDataRow(IDictionary instance, DataRow dr, string[] columns)
        {
            if (dr == null || columns ==null)
                return;
            for (int i = 0; i < columns.Length; i++)
            {
                string colName = columns[i];
                instance[colName] = dr[colName];
            }
        }

        public static void LoadDictionaryFromDataRow(IDictionary instance, DataRow dr)
        {
            if (dr == null)
                return;
            DataTable dt = dr.Table;
            if (dt == null)
                return;
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                string colName = dt.Columns[i].ColumnName;
                instance[colName] = dr[colName];
            }
       }

        public static void LoadDictionaryEntityFromDataRow(IDictionary<string,string> instance, DataRow dr, string[] columns)
        {
            if (dr == null || columns == null)
                return;
            for (int i = 0; i < columns.Length; i++)
            {
                string colName = columns[i];
                instance[colName] = GenericTypes.NZ(dr[colName], null);
            }
        }

        public static void LoadDictionaryEntityFromDataRow(IDictionary<string, string> instance, DataRow dr)
        {
            if (dr == null)
                return;
            DataTable dt = dr.Table;
            if (dt == null)
                return;
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                string colName = dt.Columns[i].ColumnName;
                instance[colName] = GenericTypes.NZ(dr[colName], null);
            }
        }

        public static void LoadDictionaryEntityFromDataRow(IDictionary<string, object> instance, DataRow dr, string[] columns)
        {
            if (dr == null || columns == null)
                return;
            for (int i = 0; i < columns.Length; i++)
            {
                string colName = columns[i];
                instance[colName] = dr[colName];
            }
        }
        public static void LoadDictionaryEntityFromDataRow(IDictionary<string, object> instance, DataRow dr, Dictionary<string,int> columnsNameIndex)
        {
            if (dr == null || columnsNameIndex == null)
                return;
            foreach (var entry in columnsNameIndex)
            {
                instance[entry.Key] = dr[entry.Value];
            }
        }
        public static Dictionary<string, int> GetEntityColumnsFromDataTable(DataTable dt, string[] columns)
        {
            if (dt == null)
                return null;
            Dictionary<string, int> columnsNameIndex = new Dictionary<string, int>();
            if (columns != null && columns.Length > 0)
            {
                foreach (DataColumn entry in dt.Columns)
                {
                    if(columns.IndexOf(entry.ColumnName)>=0)
                    {
                        columnsNameIndex.Add(entry.ColumnName, entry.Ordinal);
                    }
                }
            }
            else
            {
                foreach (DataColumn entry in dt.Columns)
                {
                    columnsNameIndex.Add(entry.ColumnName, entry.Ordinal);
                }
            }
            return columnsNameIndex;
        }
        public static void LoadDictionaryEntityFromDataRow(IDictionary<string, object> instance, DataRow dr)
        {
            if (dr == null)
                return;
            DataTable dt = dr.Table;
            if (dt == null)
                return;
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                string colName = dt.Columns[i].ColumnName;
                instance[colName] = dr[colName];
            }
        }

        public static void CopyDictionary<K,V>(IDictionary<K,V> dest, IDictionary<K,V> copyFrom)
        {
            foreach (KeyValuePair<K,V> entry in copyFrom)
            {
                dest[entry.Key] = entry.Value;
            }
        }

        public static void CopyIDictionary(IDictionary dest, IDictionary copyFrom)
        {
            foreach (DictionaryEntry entry in copyFrom)
            {
                dest[entry.Key] = entry.Value;
            }
        }

        public static IDictionary/*<string, object>*/ DataRowToHashtable(DataRow dr)
        {
            if (dr == null)
                return null;
            DataTable dt = dr.Table;
            if (dt == null)
                return null;
            //Hashtable hash = new Hashtable();
            IDictionary/*<string, object>*/ hash = new Hashtable();// Dictionary<string, object>();
           
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                string colName=dt.Columns[i].ColumnName;
                hash[colName] = dr[colName];
            }

            return hash;
        }

        public static DataRow HashtableToDataRow(IDictionary/*<string, object>*/ h, string tableName=null)
        {
            if (h == null || h.Count == 0)
                return null;

            string[] names = new string[h.Count];
            object[] values = new object[h.Count];
            h.Keys.CopyTo(names, 0);
            h.Values.CopyTo(values, 0);

            DataTable dt = new DataTable(tableName);
            for (int i = 0; i < h.Count; i++)
            {
                dt.Columns.Add(names[i]);
            }
            dt.Rows.Add(values);

            return dt.Rows[0];
        }

 
        public static Hashtable ToDictionary(DataTable dt, string keyName, string valueName)
        {

            if (dt == null)
            {
                throw new ArgumentNullException("dt");
            }
            DataRow[] drs = dt.Select(null);
            if (drs == null || drs.Length == 0)
                return null;

            Hashtable hashtable = Hashtable.Synchronized(new Hashtable());

            foreach (DataRow dr in drs)
            {
                string hashKey = string.Format("{0}", dr[keyName]);
                hashtable[hashKey] = dr[valueName];
            }
            return hashtable;
        }
 

        public static string[] GetTableFields(DataTable dt)
        {
           
            if (dt == null)
                return null;
            
            IDictionary/*<string, object>*/ hash = new GenericRecord();
            List<string> fields = new List<string>();
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                string colName = dt.Columns[i].ColumnName;
                fields.Add(colName);
            }

            return fields.ToArray();
        }


        public static DataTable GetFilteredData(DataView dv, string filterExperssion)
		{
			return GetFilteredData(dv.Table,filterExperssion,"",DataViewRowState.None);
		}
		public static DataTable GetFilteredData(DataView dv,string filterExperssion,string sort)
		{
			return GetFilteredData(dv.Table,filterExperssion,sort,DataViewRowState.None);
		}

		public static DataTable GetFilteredData(DataView dv,string filterExperssion,string sort,DataViewRowState state)
		{
			return GetFilteredData(dv.Table,filterExperssion,sort,state);
		}

		public static DataTable GetFilteredData(DataTable dtv,string filterExperssion,string sort,DataViewRowState state)
		{
			if(dtv==null)
				return null;
			DataTable dt=new DataTable(dtv.TableName);
			dt=dtv.Clone();
			DataRow[] drs=dtv.Select(filterExperssion, sort,state);
			foreach(DataRow dr in drs)
			{
				dt.ImportRow(dr);
			}
			return dt;
		}

		public static DataRow[] GetDataRows(DataSet ds,string TableName,string filter,string sort)
		{
			return ds.Tables[TableName].Select(filter,sort);
		}

		public static DataRow GetDataRow(DataSet ds,string TableName,string filter,string sort)
		{
			DataRow[] dr= ds.Tables[TableName].Select(filter,sort);
			if(dr==null)
				return null;
			return dr[0];
		}

		public static object GetDataSetValue(DataSet ds,string TableName,string columnName,string filter,string sort)
		{
            DataRow[] dr = ds.Tables[TableName].Select(filter, sort);
			if(dr==null)
				return null;
			return dr[0][columnName];
		}

        public static DataTable GetTableSchema(IDataReader reader,string tableName)
        {
            DataTable schemaTable = reader.GetSchemaTable();
            DataTable table = new DataTable(tableName);

            foreach (DataRow row in schemaTable.Rows)
            {
                DataColumn col = new DataColumn(row[schemaTable.Columns["ColumnName"]].ToString(), Type.GetType( row[schemaTable.Columns["DataType"]].ToString()));//, column.DataType, column.Expression);
                table.Columns.Add(col);


            }
            return table;
        }


 

        public static void SummaryRunning(DataTable dt,string summaryColumn)
        {
            decimal total = 0;
            int count = dt.Rows.Count;

            for (int i = 0; i < count; i++)
            {
                DataRow dr = dt.Rows[i];
                object val = dr[summaryColumn];
                dr[summaryColumn] = total += Types.ToDecimal(val, 0);
    
            }
        }


        /// <summary>
        /// CreateTableSchema
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static DataTable CreateTableSchema(string tableName, string[] columns)
        {
            Dictionary<string, Type> colls=new Dictionary<string,Type>(columns.Length);
            foreach(string s in columns)
            {
                colls.Add(s,typeof(string));
            }
            return CreateTableSchema(tableName, colls);
        }

        /// <summary>
        /// CreateTableSchema
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
       public static DataTable CreateTableSchema(string tableName, DataColumn[] columns)
        {
            DataTable dt = null;

            dt = new DataTable(tableName);
            dt.Columns.AddRange(columns);
            return dt;
        }

        
        /// <summary>
        /// CreateTableSchema
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static DataTable CreateTableSchema(string tableName, Dictionary<string, Type> columns)
        {
            DataTable dt = null;
            DataColumn colx = null;

            dt = new DataTable(tableName);

            foreach (string s in columns.Keys)
            {
                colx = new DataColumn(s, columns[s]);// System.Type.GetType("System.String"));
                dt.Columns.Add(colx);
            }
            return dt;
        }

        /// <summary>
        /// Equals Schema
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="matchColumnName"></param>
        /// <returns></returns>
        public static bool EqualsSchema(DataTable a, DataTable b, bool matchColumnName)
        {
            if (a == null || b == null)
                return false;
            if (a.Columns.Count != b.Columns.Count)
                return false;
            for (int i = 0; i < a.Columns.Count; i++)
            {
                if (a.Columns[i].DataType != b.Columns[i].DataType)
                    return false;
                if (matchColumnName)
                {
                    if (a.Columns[i].ColumnName != b.Columns[i].ColumnName)
                        return false;
                }
            }
            return true;
        }


        /// <summary>
        /// FillDataTable
        /// </summary>
        /// <param name="dt">DataTable to fill</param>
        /// <param name="rows">columnName,Value</param>
        public static void FillDataTable(DataTable dt, Dictionary<string, object>[] rows)
        {
            DataRow rowx = null;
            int rowLength = rows.Length;

            for (int i = 0; i < rowLength; i++)
            {
                rowx = dt.NewRow();
                foreach (string s in rows[i].Keys)
                {
                    rowx[s] = rows[i][s];
                }
                dt.Rows.Add(rowx);
            }
        }

        ///// <summary>
        ///// FillDataTable
        ///// </summary>
        ///// <param name="rows">columnName,Value</param>
        //public static DataTable ToDataTable(IEnumerable<IEnumerable<KeyValuePair<string, object>>> rows)
        //{
        //    DataTable dt = new DataTable();
        //    int i = 0;
        //    DataRow rowx = null;
        //    //int rowLength = rows.Length;
        //    foreach (var row in rows)
        //    {
        //        //var drowx = ((IEnumerable<object>)row.Value).ToArray().Cast<IEnumerable<KeyValuePair<string, object>>>();
        //        foreach (var rx in rows)
        //        {
        //            if (i == 0)
        //            {
        //                foreach (var elm in rx)
        //                    dt.Columns.Add(elm.Key);
        //            }
        //            rowx = dt.NewRow();
        //            foreach (var elm in rx)
        //                rowx[elm.Key] = elm.Value;

        //            dt.Rows.Add(rowx);
        //            i++;
        //        }
        //    }
        //    return dt;
        //}


        /// <summary>
        /// Convert KeyValuePair to DataTable
        /// </summary>
        /// <param name="rows"></param>
        /// <returns></returns>
        public static DataTable ToDataTable(IEnumerable<KeyValuePair<string, object>> rows)
        {
            //var dic1 = Nistec.Serialization.JsonSerializer.Deserialize<Dictionary<string, object>>(fileContents);
            //var rows = dic1.Where(x => x.Key == "value");


            DataTable dt = new DataTable();
            int i = 0;
            DataRow rowx = null;
            foreach (var row in rows)
            {
                var drowx = ((IEnumerable<object>)row.Value).ToArray().Cast<IEnumerable<KeyValuePair<string, object>>>();
                foreach (var rx in drowx)
                {
                    if (i == 0)
                    {
                        foreach (var elm in rx)
                            dt.Columns.Add(elm.Key);
                    }
                    rowx = dt.NewRow();
                    foreach (var elm in rx)
                        rowx[elm.Key] = elm.Value;

                    dt.Rows.Add(rowx);
                    i++;
                }

                //var drow = ((IEnumerable<object>)row.Value).ToArray();
                //for (int j = 0; j < drow.Count(); j++)
                //{
                //    var elements = (IEnumerable<KeyValuePair<string, object>>)drow.ElementAt(j);
                //    if (i == 0)
                //    {
                //        foreach (var elm in elements)
                //            dt.Columns.Add(elm.Key);
                //    }
                //    rowx = dt.NewRow();
                //    foreach (var elm in elements)
                //        rowx[elm.Key] = elm.Value;

                //    dt.Rows.Add(rowx);
                //    i++;
                //}
            }
            return dt;
        }

        /// <summary>
        /// FillDataTable
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="rows"></param>
        public static void FillDataTable(DataTable dt, Dictionary<object, object> rows)
        {
            DataRow rowx = null;
            foreach (KeyValuePair<object, object> k in rows)
            {
                rowx = dt.NewRow();
                rowx[0] = k.Key;
                rowx[1] = k.Value;
                dt.Rows.Add(rowx);
            }
        }

        /// <summary>
        /// FillDataTable
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="rows"></param>
        public static void FillDataTable(DataTable dt, List<object>[] rows)
        {
            DataRow rowx = null;
            int rowLength = rows.Length;
            int colCount = 0;

            for (int i = 0; i < rowLength; i++)
            {
                rowx = dt.NewRow();
                colCount = rows[i].Count;
                for (int j = 0; j < colCount; i++)
                {
                    rowx[j] = rows[i];
                }
                dt.Rows.Add(rowx);
            }
        }

        /// <summary>
        /// FillDataTable
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="rows"></param>
        public static void FillDataTable(DataTable dt, object[] rows)
        {
            DataRow rowx = null;
            int rowLength = rows.Length;

            for (int i = 0; i < rowLength; i++)
            {
                rowx = dt.NewRow();
                rowx[0] = rows[i];
                dt.Rows.Add(rowx);
            }
        }
        internal static void FillDataTable(DataTable dt, object[] rows,bool duplicate)
        {
            DataRow rowx = null;
            int rowLength = rows.Length;
            int colCount = dt.Columns.Count;

            for (int i = 0; i < rowLength; i++)
            {
                rowx = dt.NewRow();
                if (duplicate)
                {
                    for (int j = 0; j < colCount; j++)
                    {
                        rowx[j] = rows[i];
                    }
                }
                else
                {
                    rowx[0] = rows[i];
                }
                dt.Rows.Add(rowx);
            }
        }
        /// <summary>
        /// FillDataTable
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="rows"></param>
        public static void FillDataTable(DataTable dt, object[,] rows)
        {
            DataRow rowx = null;
            int rowLength = rows.Length/2;

            for (int i = 0; i < rowLength; i++)
            {
                rowx = dt.NewRow();
                rowx[0] = rows[i,0];
                rowx[1] = rows[i,1];
                dt.Rows.Add(rowx);
            }
        }
        public static T GetParameterValue<T>(this IEnumerable<SqlParameter> parameters, string parameterName)
        {
            var parameter = parameters.Where(p => p.ParameterName == parameterName).FirstOrDefault();
            if (parameter == null)
                return default(T);
            return GenericTypes.Convert<T>(parameter.Value);
        }

        public static SqlParameter GetParameter(this IEnumerable<SqlParameter> parameters, string parameterName)
        {
            var parameter = parameters.Where(p => p.ParameterName == parameterName).FirstOrDefault();
            return parameter;
        }



	}
}
