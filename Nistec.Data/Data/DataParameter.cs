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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.ComponentModel;
using System.Data.Common;
using System.Globalization;
using System.Xml;
using System.Reflection;
using Nistec.Generic;
using Nistec.Runtime;
using Nistec.Data.Entities;
using System.Collections.Specialized;
#pragma warning disable CS1591
namespace Nistec.Data
{

    /// <summary>
    /// Represents a parameter to a DbCommand and optionally,its mapping to a System.Data.DataSet column.
    /// </summary>
    public class DataParameter : DbParameter, ICloneable, IDbDataParameter, IDataParameter//IDbDataParameter
    {
        public DalParamType ParamType { get; set; }

        #region internal
        internal string ParameterNameFixed
        {
            get
            {
                string parameterName = this.ParameterName;
                if ((0 < parameterName.Length) && ('@' != parameterName[0]))
                {
                    parameterName = "@" + parameterName;
                }
                return parameterName;
            }
        }

        internal object ValueFixed
        {
            get
            {
                object value = this.Value;
                if (value==null)
                {
                    value = DBNull.Value;
                }
                return value;
            }
        }

       
        #endregion

        #region properties

        // Summary:
        //     Gets or sets the System.Data.DbType of the parameter.
        //
        // Returns:
        //     One of the System.Data.DbType values. The default is System.Data.DbType.String.
        //
        // Exceptions:
        //   System.ArgumentException:
        //     The property is not set to a valid System.Data.DbType.
        public override DbType DbType { get; set; }
        //
        // Summary:
        //     Gets or sets a value that indicates whether the parameter is input-only,
        //     output-only, bidirectional, or a stored procedure return value parameter.
        //
        // Returns:
        //     One of the System.Data.ParameterDirection values. The default is Input.
        //
        // Exceptions:
        //   System.ArgumentException:
        //     The property is not set to one of the valid System.Data.ParameterDirection
        //     values.
        public override ParameterDirection Direction { get; set; }
        //
        // Summary:
        //     Gets or sets a value that indicates whether the parameter accepts null values.
        //
        // Returns:
        //     true if null values are accepted; otherwise false. The default is false.
        public override bool IsNullable { get; set; }
        //
        // Summary:
        //     Gets or sets the name of the System.Data.Common.DbParameter.
        //
        // Returns:
        //     The name of the System.Data.Common.DbParameter. The default is an empty string
        //     ("").
        public override string ParameterName { get; set; }
        //
        // Summary:
        //     Gets or sets the maximum size, in bytes, of the data within the column.
        //
        // Returns:
        //     The maximum size, in bytes, of the data within the column. The default value
        //     is inferred from the parameter value.
        public override int Size { get; set; }
        //
        // Summary:
        //     Gets or sets the name of the source column mapped to the System.Data.DataSet
        //     and used for loading or returning the System.Data.Common.DbParameter.Value.
        //
        // Returns:
        //     The name of the source column mapped to the System.Data.DataSet. The default
        //     is an empty string.
        public override string SourceColumn { get; set; }
        //
        // Summary:
        //     Sets or gets a value which indicates whether the source column is nullable.
        //     This allows System.Data.Common.DbCommandBuilder to correctly generate Update
        //     statements for nullable columns.
        //
        // Returns:
        //     true if the source column is nullable; false if it is not.
        public override bool SourceColumnNullMapping { get; set; }
        //
        // Summary:
        //     Gets or sets the System.Data.DataRowVersion to use when you load System.Data.Common.DbParameter.Value.
        //
        // Returns:
        //     One of the System.Data.DataRowVersion values. The default is Current.
        //
        // Exceptions:
        //   System.ArgumentException:
        //     The property is not set to one of the System.Data.DataRowVersion values.
        public override DataRowVersion SourceVersion { get; set; }
        //
        // Summary:
        //     Gets or sets the value of the parameter.
        //
        // Returns:
        //     An System.Object that is the value of the parameter. The default value is
        //     null.
        public override object Value { get; set; }


        // Summary:
        //     Indicates the precision of numeric parameters.
        //
        // Returns:
        //     The maximum number of digits used to represent the Value property of a data
        //     provider Parameter object. The default value is 0, which indicates that a
        //     data provider sets the precision for Value.
        new byte Precision { get; set; }
        //
        // Summary:
        //     Indicates the scale of numeric parameters.
        //
        // Returns:
        //     The number of decimal places to which System.Data.OleDb.OleDbParameter.Value
        //     is resolved. The default is 0.
        new byte Scale { get; set; }

        // Summary:
        //     Resets the DbType property to its original settings.
        public override void ResetDbType()
        {

        }


        #endregion

        object ICloneable.Clone()
        {
            return new DataParameter(this.ParameterName, this.Value)
            {
                DbType = this.DbType,
                ParamType = this.ParamType,
                Direction = this.Direction,
                IsNullable = this.IsNullable,
                Precision = this.Precision,
                Scale = this.Scale,
                Size = this.Size,
                SourceColumn = this.SourceColumn,
                SourceColumnNullMapping = this.SourceColumnNullMapping,
                SourceVersion = this.SourceVersion,
                Value = this.Value
            };
        }

        #region ctor

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public DataParameter(string name, object value)
        {
            this.ParamType = DalParamType.Default;
            this.ParameterName = name;
            this.Value = value;
            this.IsNullable = false;
            this.DbType = ConvertToDbType(value.GetType(), System.Data.DbType.Object);
        }
       
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="paramType"></param>
        /// <param name="dbType"></param>
        public DataParameter(string name, object value, DalParamType paramType, DbType dbType)
        {
            this.ParamType = paramType;
            this.ParameterName = name;
            this.Value = value;
            this.IsNullable = false;
            this.DbType = dbType;
        }
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="isNullAble"></param>
        public DataParameter(string name, object value, bool isNullAble)
        {
            this.ParamType = DalParamType.Default;
            this.ParameterName = name;
            this.Value = value;
            this.IsNullable = isNullAble;
            this.DbType = ConvertToDbType(value.GetType(), System.Data.DbType.Object);
        }
        
        // Summary:
        //     Initializes a new instance of the DataParameter class
        //     that uses the parameter name, the type of the parameter, the size of the
        //     parameter, a System.Data.ParameterDirection, the precision of the parameter,
        //     the scale of the parameter, the source column, a System.Data.DataRowVersion
        //     to use, and the value of the parameter.
        //
        // Parameters:
        //   parameterName:
        //     The name of the parameter to map.
        //
        //   dbType:
        //     One of the System.Data.DbType values.
        //
        //   size:
        //     The length of the parameter.
        //
        //   direction:
        //     One of the System.Data.ParameterDirection values.
        //
        //   isNullable:
        //     true if the value of the field can be null; otherwise false.
        //
        //   precision:
        //     The total number of digits to the left and right of the decimal point to
        //     which DataParameter.Value is resolved.
        //
        //   scale:
        //     The total number of decimal places to which DataParameter.Value
        //     is resolved.
        //
        //   sourceColumn:
        //     The name of the source column.
        //
        //   sourceVersion:
        //     One of the System.Data.DataRowVersion values.
        //
        //   value:
        //     An System.Object that is the value of the DataParameter.
        //
        // Exceptions:
        //   System.ArgumentException:
        //     The value supplied in the dbType parameter is an invalid back-end data type.
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public DataParameter(string parameterName, DbType dbType, int size, ParameterDirection direction, bool isNullable, byte precision, byte scale, string sourceColumn, DataRowVersion sourceVersion, object value)
        {
            this.ParamType = DalParamType.Default;
            
            this.ParameterName = parameterName;
            this.Value = value;
            this.IsNullable = isNullable;
            this.Direction = direction;

            this.DbType = ConvertToDbType(value.GetType(), System.Data.DbType.Object);

            this.Size = size;
            this.SourceColumn = sourceColumn;
            this.SourceVersion = sourceVersion;

        }
 
        public static DbType ConvertToDbType(System.Type type, DbType defaultValue)
        {
            SqlParameter p1;
            System.ComponentModel.TypeConverter tc;
            p1 = new SqlParameter();
            tc = System.ComponentModel.TypeDescriptor.GetConverter(p1.DbType);
            if (tc.CanConvertFrom(type))
            {
                p1.DbType = (DbType)tc.ConvertFrom(type.Name);
            }
            else
            {
                //'Try brute force
                try
                {
                    p1.DbType = (DbType)tc.ConvertFrom(type.Name);
                }
                catch (Exception)
                {
                    return defaultValue;
                }

            }
            return p1.DbType;
        }

        internal static DbType GetDbTypeFromObject(object comVal)
        {
            if ((comVal != null) && (DBNull.Value != comVal))
            {
                if (comVal is float)
                {
                    return  DbType.Single;
                }
                if (comVal is string)
                {
                    return DbType.String;
                }
                if (comVal is double)
                {
                    return DbType.Double;
                }
                if (comVal is byte[])
                {
                    return DbType.Binary;
                }
                if (comVal is char)
                {
                    return DbType.StringFixedLength;
                }
                if (comVal is char[])
                {
                    return DbType.StringFixedLength;
                }
                if (comVal is Guid)
                {
                    return DbType.Guid;
                }
                if (comVal is bool)
                {
                    return DbType.Boolean;
                }
                if (comVal is byte)
                {
                    return DbType.Byte;
                }
                if (comVal is short)
                {
                    return DbType.Int16;
                }
                if (comVal is int)
                {
                    return DbType.Int32;
                }
                if (comVal is long)
                {
                    return DbType.Int64;
                }
                if (comVal is decimal)
                {
                    return DbType.Decimal;
                }
                if (comVal is DateTime)
                {
                    return DbType.DateTime;
                }
                if (comVal is XmlReader)
                {
                    return DbType.Xml;
                }
                if ((comVal is TimeSpan) || (comVal is DateTimeOffset))
                {
                    return DbType.Time;
                }
            }
            return DbType.Object;
        }


        internal static Type GetTypeFromDbType(DbType dbType)
        {
            switch (dbType)
            {
                case DbType.Binary:
                    return typeof(byte[]);
                case DbType.Boolean:
                    return typeof(bool);
                case DbType.Byte:
                    return typeof(byte);
                case DbType.Currency:
                    return typeof(decimal);
                case DbType.Date:
                case DbType.DateTime:
                case DbType.DateTime2:
                case DbType.DateTimeOffset:
                    return typeof(DateTime);
                case DbType.Decimal:
                    return typeof(decimal);
                case DbType.Double:
                    return typeof(double);
                case DbType.Guid:
                    return typeof(Guid);
                case DbType.Int16:
                    return typeof(Int16);
                case DbType.Int32:
                    return typeof(Int32);
                case DbType.Int64:
                    return typeof(Int64);
                case DbType.SByte:
                    return typeof(byte);
                case DbType.Single:
                    return typeof(float);
                case DbType.String:
                    return typeof(string);
                case DbType.StringFixedLength:
                    return typeof(string);
                case DbType.Time:
                    return typeof(string);
                case DbType.UInt16:
                    return typeof(UInt16);
                case DbType.UInt32:
                    return typeof(UInt32);
                case DbType.UInt64:
                    return typeof(UInt64);
                case DbType.VarNumeric:
                    return typeof(double);
                case DbType.Xml:
                    return typeof(string);
            }
            return typeof(object);
        }

        internal static bool IsNumericType(Type type)
        {

            if (type==typeof(float)
                || type==typeof(double)
                || type==typeof(byte)
                || type==typeof(short)
                || type==typeof(int)
                || type==typeof(long)
                || type==typeof(decimal)
                || type == typeof(Int16)
                || type == typeof(Int32)
                || type == typeof(Int64)
                || type == typeof(UInt16)
                || type == typeof(UInt32)
                || type == typeof(UInt64)
                || type == typeof(uint))
            {
                return true;
            }
            
            return false;
        }


        #endregion

        #region methods

        public IDbDataParameter ToDbParameter(DBProvider provider)
        {
             if (provider == DBProvider.SqlServer)
                return new SqlParameter(ParameterName, Value);
           else if (provider == DBProvider.OleDb)
                return new OleDbParameter(ParameterName, Value);
             else
                throw new ArgumentException("Provider not supported");
        }

        #endregion

        #region static methods

        public static IDbDataParameter Create(string name, object value, DBProvider provider = DBProvider.SqlServer)
        {
            if (provider == DBProvider.SqlServer)
                return new SqlParameter(name, value);
            else if (provider == DBProvider.OleDb)
                return new OleDbParameter(name, value);
            else
                return new DataParameter(name, value);
        }
        
        public bool ShouldCopy(IDbDataParameter prm)
        {
            return prm is DataParameter;
        }

        public static void Copy(IDbDataParameter src, IDbDataParameter dest)
        {
            dest.DbType = src.DbType;
            dest.Direction = src.Direction;
            dest.ParameterName = src.ParameterName;
            dest.Precision = src.Precision;
            dest.Scale = src.Scale;
            dest.Size = src.Size;
            dest.Scale = src.Scale;
            dest.SourceColumn = src.SourceColumn;
            dest.Value = src.Value;
        }

        public static void CopySql(IDbDataParameter src, SqlParameter dest)
        {
            bool isStructed = src.Value != null && src.Value is DataTable;
            if (isStructed)
                dest.SqlDbType = SqlDbType.Structured;
            else
                dest.DbType = src.DbType;

            dest.Direction = src.Direction;
            dest.ParameterName = src.ParameterName;
            dest.Precision = src.Precision;
            dest.Scale = src.Scale;
            dest.Size = src.Size;
            dest.Scale = src.Scale;
            dest.SourceColumn = src.SourceColumn;
            dest.Value = src.Value;
        }

        public static void AddParameters(IDbCommand cmd, IDbDataParameter[] parameters)
        {
            if (parameters == null)
                return;
            if (parameters is Nistec.Data.DataParameter[])
            {
                foreach (IDbDataParameter p in parameters)
                {
                    IDbDataParameter parameter = cmd.CreateParameter();
                    Copy(p, parameter);
                    cmd.Parameters.Add(parameter);
                }
            }
            else
            {
                foreach (IDbDataParameter p in parameters)
                {
                    cmd.Parameters.Add(p);
                }
            }
        }

        public static void AddSqlParameters(SqlCommand cmd, IDbDataParameter[] parameters)
        {
            if (parameters == null)
                return;
            if (parameters is Nistec.Data.DataParameter[])
            {
                SqlCommand scmd = (SqlCommand)cmd;
                foreach (IDbDataParameter p in parameters)
                {
                    SqlParameter parameter = scmd.CreateParameter();
                    CopySql(p, parameter);
                    cmd.Parameters.Add(parameter);
                }
            }
            else
            {
                cmd.Parameters.AddRange(parameters);
            }
        }

        public static IDbDataParameter[] CreateParameters(IDictionary<string, object> values, DBProvider provider= DBProvider.SqlServer)
        {
            if (values == null)
                return null;
            List<IDbDataParameter> prm = new List<IDbDataParameter>();
            foreach (var p in values)
            {
                prm.Add(DataParameter.Create(p.Key, p.Value, provider));
            }
            return prm.ToArray();
        }

  
        public static IDbDataParameter[] CreateParameters(DataParameter[] values, DBProvider provider = DBProvider.SqlServer)
        {
            if (values == null)
                return null;
            List<IDbDataParameter> prm = new List<IDbDataParameter>();
            foreach (DataParameter p in values)
            {
                prm.Add(p.ToDbParameter(provider));
            }
            return prm.ToArray();
        }

        public static object[] ToArgs(params object[] keyValueParameters)
        {
            return keyValueParameters;
        }

        public static object[] ToNameValue(NameValueCollection args)
        {
            List<object> o = new  List<object>();
           
            foreach (string key in args)
            {
                o.Add(key);
                o.Add(args[key]);
            }
            return o.ToArray();
        }

        public static object[] ToNameValue(object instance)
        {
            if (instance == null)
                return null;
            List<object> o = new List<object>();
            Type type = instance.GetType();
            PropertyInfo[] pr = type.GetProperties(BindingFlags.Public | BindingFlags.Instance, false);
            foreach (PropertyInfo p in pr)
            {
                o.Add(p.Name);
                o.Add(p.GetValue(instance));
            }
            return o.ToArray();
        }

        ///// <summary>
        ///// Create IDbCmd
        ///// </summary>
        ///// <param name="provider"></param>
        ///// <param name="keyValueParameters"></param>
        ///// <returns></returns>
        //public static IDbCmd Create(DBProvider provider, params object[] keyValueParameters)
        //{
        //    if (provider == DBProvider.SqlServer)
        //        return new SqlClient.DbSqlCmd(connectionString);
        //    else if (provider == DBProvider.OleDb)
        //        return new OleDb.DbOleCmd(connectionString);
        //    else
        //        throw new ArgumentException("Provider not supported");
        //}


        /// <summary>
        /// Create KeyValueParameters
        /// </summary>
        /// <param name="command"></param>
        /// <param name="keyValueParameters"></param>
        /// <returns></returns>
        public static IDbDataParameter[] Get(IDbCommand command,params object[] keyValueParameters)
        {

            int count = keyValueParameters.Length;
            if (count % 2 != 0)
            {
                throw new ArgumentException("values parameter not correct, Not match key value arguments");
            }
            List<IDbDataParameter> list = new List<IDbDataParameter>();
            for (int i = 0; i < count; i++)
            {
                var parameter= command.CreateParameter();
                string key = keyValueParameters[i].ToString();
                object value = keyValueParameters[++i];
                parameter.ParameterName = key;
                parameter.Value = value;
                parameter.DbType = GetDbTypeFromObject(value);
                list.Add(parameter);
                //list.Add(new DataParameter(keyValueParameters[i].ToString(), keyValueParameters[++i]));
            }
            return list.ToArray();
        }

        /// <summary>
        /// Create KeyValueParameters
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="keyValueParameters"></param>
        /// <returns></returns>
        public static IDbDataParameter[] Get(IDbConnection connection, params object[] keyValueParameters)
        {

            int count = keyValueParameters.Length;
            if (count % 2 != 0)
            {
                throw new ArgumentException("values parameter not correct, Not match key value arguments");
            }
            List<IDbDataParameter> list = new List<IDbDataParameter>();

            using (var command = connection.CreateCommand())
            {
                for (int i = 0; i < count; i++)
                {
                    var parameter = command.CreateParameter();
                    string key = keyValueParameters[i].ToString();
                    object value = keyValueParameters[++i];
                    parameter.ParameterName = key;
                    parameter.Value = value;
                    parameter.DbType = GetDbTypeFromObject(value);
                    list.Add(parameter);
                    //list.Add(new DataParameter(keyValueParameters[i].ToString(), keyValueParameters[++i]));
                }
            }
            return list.ToArray();
        }


        ///// <summary>
        ///// Create KeyValueParameters
        ///// </summary>
        ///// <param name="keyValueParameters"></param>
        ///// <returns></returns>
        //public static IDbDataParameter[] Get(params object[] keyValueParameters)
        //{

        //    int count = keyValueParameters.Length;
        //    if (count % 2 != 0)
        //    {
        //        throw new ArgumentException("values parameter not correct, Not match key value arguments");
        //    }
        //    List<IDbDataParameter> list = new List<IDbDataParameter>();
        //    for (int i = 0; i < count; i++)
        //    {
        //        //list.Add(new OleDbParameter(keyValueParameters[i].ToString(), keyValueParameters[++i]));

        //        list.Add(new DataParameter(keyValueParameters[i].ToString(), keyValueParameters[++i]));
        //    }
        //    return list.ToArray();
        //}

        /// <summary>
        /// Create KeyValueParameters
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keyValueParameters"></param>
        /// <returns></returns>
        public static IDbDataParameter[] GetDbParam<T>(params object[] keyValueParameters) where T : IDbDataParameter
        {

            int count = keyValueParameters.Length;
            if (count % 2 != 0)
            {
                throw new ArgumentException("values parameter not correct, Not match key value arguments");
            }
            List<IDbDataParameter> list = new List<IDbDataParameter>();
            bool isSql = typeof(T) == typeof(SqlParameter);
            for (int i = 0; i < count; i++)
            {
                string key = keyValueParameters[i].ToString();
                object value = keyValueParameters[++i];

                if (isSql)
                {
                    list.Add(new SqlParameter(key, value));
                }
                else
                {
                    T instance = ActivatorUtil.CreateInstance<T>();
                    instance.ParameterName = key;
                    instance.Value = value;
                    instance.DbType = GetDbTypeFromObject(value);
                    list.Add(instance);
                }
            }

            return list.ToArray();
        }

        /// <summary>
        /// Create KeyValueParameters
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keyValueParameters"></param>
        /// <returns></returns>
        public static T[] Get<T>(params object[] keyValueParameters) where T: IDbDataParameter
        {

            int count = keyValueParameters.Length;
            if (count % 2 != 0)
            {
                throw new ArgumentException("values parameter not correct, Not match key value arguments");
            }
            List<T> list = new List<T>();
            for (int i = 0; i < count; i++)
            {
                T instance = ActivatorUtil.CreateInstance<T>();
                string key= keyValueParameters[i].ToString();
                object value = keyValueParameters[++i];
                instance.ParameterName = key;
                instance.Value = value;
                instance.DbType = GetDbTypeFromObject(value);

                list.Add(instance);
            }

            return list.ToArray();
        }

        ///// <summary>
        ///// Create KeyValueParameters
        ///// </summary>
        ///// <param name="keyValueParameters"></param>
        ///// <returns></returns>
        //public static IDbDataParameter[] GetIdb(params object[] keyValueParameters)
        //{

        //    int count = keyValueParameters.Length;
        //    if (count % 2 != 0)
        //    {
        //        throw new ArgumentException("values parameter not correct, Not match key value arguments");
        //    }
        //    List<IDbDataParameter> list = new List<IDbDataParameter>();
        //    for (int i = 0; i < count; i++)
        //    {
        //        list.Add(new DataParameter(keyValueParameters[i].ToString(), keyValueParameters[++i]));
        //    }

        //    return list.ToArray();
        //}

        public static List<SqlParameter> GetSqlParameters<T>(T instance) where T : IEntityItem
        {
            bool enableAttributeColumn = false;
            List<SqlParameter> list = new List<SqlParameter>();
            //T instance = ActivatorUtil.CreateInstance<T>();

            var props = Nistec.Data.DataProperties.GetEntityProperties(typeof(T), true);
            foreach (var pa in props)
            {
                PropertyInfo property = pa.Property;
                EntityPropertyAttribute attr = pa.Attribute;

                if (!property.CanRead)
                {
                    continue;
                }

                if (attr != null)
                {
                    if (attr.ParameterType == EntityPropertyType.NA)
                    {
                        continue;
                    }
                    if (attr.ParameterType == EntityPropertyType.View)
                    {
                        continue;
                    }
                    if (property.CanWrite)
                    {

                        string field = attr.GetColumn(property.Name, enableAttributeColumn);
                        object value = property.GetValue(instance, null);

                        if (value == null)
                        {
                            if (attr.ParameterType == EntityPropertyType.Optional)
                                continue;
                            value = attr.AsNull;
                        }

                        //var val = Types.ChangeType(value, property.PropertyType);

                        list.Add(new SqlParameter(field, value));
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Create sql parameter from <see cref="GenericRecord"/>
        /// </summary>
        /// <param name="keyValueParameters"></param>
        /// <param name="sortedByKey"></param>
        /// <returns></returns>
        public static SqlParameter[] GetSqlParameters(GenericRecord keyValueParameters, bool sortedByKey)
        {
            if (keyValueParameters == null)
                return null;

            //_Data = EntityPropertyBuilder.CreateGenericRecord(_instance, true);
            int count = keyValueParameters.Count;
            List<SqlParameter> list = new List<SqlParameter>();
            if (sortedByKey)
                foreach (var entry in keyValueParameters.Sorted())
                {
                    var p = new SqlParameter(entry.Key, entry.Value);
                    list.Add(p);
                }
            else
                foreach (var entry in keyValueParameters)
                {
                    var p = new SqlParameter(entry.Key, entry.Value);
                    list.Add(p);
                }

            return list.ToArray();
        }
        public static SqlParameter[] GetSqlParameters(GenericKeyValue keyValueParameters)
        {
            if (keyValueParameters == null || keyValueParameters.Count == 0)
                return null;

            int count = keyValueParameters.Count;
            List<SqlParameter> list = new List<SqlParameter>();
            foreach (var entry in keyValueParameters)
            {
                var p = new SqlParameter(entry.Key, entry.Value);
                list.Add(p);
            }

            return list.ToArray();
        }
        public static SqlParameter[] GetSql(params object[] keyValueParameters)
        {
            if (keyValueParameters == null)
                return null;
            int count = keyValueParameters.Length;
            if (count % 2 != 0)
            {
                throw new ArgumentException("values parameter not correct, Not match key value arguments");
            }
            List<SqlParameter> list = new List<SqlParameter>();
            for (int i = 0; i < count; i++)
            {
                var p= new SqlParameter(keyValueParameters[i].ToString(), keyValueParameters[++i]);
                list.Add(p);
            }

            return list.ToArray();
        }
        public static int GetMaxSize(SqlDbType dbType)
        {
            switch (dbType)
            {
                case SqlDbType.NVarChar:
                case SqlDbType.NChar:
                    return 4000;
                case SqlDbType.VarChar:
                case SqlDbType.Char:
                case SqlDbType.VarBinary:
                case SqlDbType.Binary:
                    return 8000;
                case SqlDbType.NText:
                case SqlDbType.Text:
                    return 8000;
                case SqlDbType.Xml:
                    return 8000;
                default:
                    return 50;
            }
        }
        public static int GetMaxSize(DbType dbType)
        {
            switch (dbType)
            {
                case DbType.String:
                    return 4000;
                case DbType.AnsiString:
                case DbType.Binary:
                    return 8000;
                case DbType.Xml:
                    return 8000;
                default:
                    return 50;
            }
        }

        public static SqlParameter[] GetSqlWithDirection(params object[] keyValueDirectionParameters) 
        {
            if (keyValueDirectionParameters == null)
                return null;
            int count = keyValueDirectionParameters.Length;
            if (count % 3 != 0)
            {
                throw new ArgumentException("values parameter not correct, Not match key value arguments");
            }
            List<SqlParameter> list = new List<SqlParameter>();
            for (int i = 0; i < count; i++)
            {
                var p = new SqlParameter(keyValueDirectionParameters[i].ToString(), keyValueDirectionParameters[++i]);
                switch ((int)keyValueDirectionParameters[++i])
                 {
                     case 2://Output
                         p.Direction = System.Data.ParameterDirection.Output;
                        p.Size = GetMaxSize(p.SqlDbType);
                        break;
                     case 3://InputOutput
                         p.Direction = System.Data.ParameterDirection.InputOutput;
                         p.Size = GetMaxSize(p.SqlDbType);
                        break;
                     case 6://ReturnValue
                         p.Direction = System.Data.ParameterDirection.ReturnValue;
                         break;
                     default://Input = 1,
                         break;
                 }
                list.Add(p);
            }

            return list.ToArray();
        }
       
        public static T[] GetWithDirection<T>(params object[] keyValueDirectionParameters) where T : IDbDataParameter
        {
            if (keyValueDirectionParameters == null)
                return null;
            int count = keyValueDirectionParameters.Length;
            if (count % 3 != 0)
            {
                throw new ArgumentException("values parameter not correct, Not match key value arguments");
            }
            List<T> list = new List<T>();
            for (int i = 0; i < count; i++)
            {
                T p = ActivatorUtil.CreateInstance<T>();
                string key = keyValueDirectionParameters[i].ToString();
                object value = keyValueDirectionParameters[++i];
                p.ParameterName = key;
                p.Value = value;
                p.DbType = GetDbTypeFromObject(value);

                //var p = new SqlParameter(keyValueDirectionParameters[i].ToString(), keyValueDirectionParameters[++i]);
                switch ((int)keyValueDirectionParameters[++i])
                {
                    case 2://Output
                        p.Direction = System.Data.ParameterDirection.Output;
                        p.Size = GetMaxSize(p.DbType);
                        break;
                    case 3://InputOutput
                        p.Direction = System.Data.ParameterDirection.InputOutput;
                        p.Size = GetMaxSize(p.DbType);
                        break;
                    case 6://ReturnValue
                        p.Direction = System.Data.ParameterDirection.ReturnValue;
                        break;
                    default://Input = 1,
                        break;
                }
                list.Add(p);
            }

            return list.ToArray();
        }
     
        public static SqlParameter[] GetSqlWithReturnValue(params object[] keyValueParameters)
        {
            List<SqlParameter> list=GetSqlList(keyValueParameters);
            if (list == null)
                return null;
            AddReturnValueParameter(list, "ReturnVal");
            return list.ToArray();
        }
     

        public static List<T> GetList<T>(params object[] keyValueParameters) where T : IDbDataParameter
        {
            if (keyValueParameters == null || keyValueParameters.Length == 0)
                return null;
            int count = keyValueParameters.Length;
            if (count % 2 != 0)
            {
                throw new ArgumentException("values parameter not correct, Not match key value arguments");
            }
            List<T> list = new List<T>();
            for (int i = 0; i < count; i++)
            {
                T instance = ActivatorUtil.CreateInstance<T>();
                string key = keyValueParameters[i].ToString();
                object value = keyValueParameters[++i];
                instance.ParameterName = key;
                instance.Value = value;
                instance.DbType = GetDbTypeFromObject(value);

                list.Add(instance);

                //var p = new SqlParameter(keyValueParameters[i].ToString(), keyValueParameters[++i]);
                //list.Add(p);
            }

            return list;
        }

        public static List<SqlParameter> GetSqlList(params object[] keyValueParameters)
        {
            if (keyValueParameters == null || keyValueParameters.Length == 0)
                return null;
            int count = keyValueParameters.Length;
            if (count % 2 != 0)
            {
                throw new ArgumentException("values parameter not correct, Not match key value arguments");
            }
            List<SqlParameter> list = new List<SqlParameter>();
            for (int i = 0; i < count; i++)
            {
                var p = new SqlParameter(keyValueParameters[i].ToString(), keyValueParameters[++i]);
                list.Add(p);
            }

            return list;
        }

        public static void AddToSqlList(IList<SqlParameter> list, params object[] keyValueParameters)
        {
            if (keyValueParameters == null || keyValueParameters.Length==0)
                return;
            int count = keyValueParameters.Length;
            if (count % 2 != 0)
            {
                throw new ArgumentException("values parameter not correct, Not match key value arguments");
            }
            for (int i = 0; i < count; i++)
            {
                var p = new SqlParameter(keyValueParameters[i].ToString(), keyValueParameters[++i]);
                list.Add(p);
            }
        }

        public static void AddOutputParameter(List<SqlParameter> list, string name, SqlDbType dbType, int size)
        {
            SqlParameter p = new SqlParameter(name, dbType, size);
            p.Direction = ParameterDirection.Output;
            list.Add(p);
        }
        public static void AddReturnValueParameter(List<SqlParameter> list, string name)
        {
            SqlParameter p = new SqlParameter(name, SqlDbType.Int);
            p.Direction = ParameterDirection.ReturnValue;
            list.Add(p);
        }
        public static void AddReturnValueParameter(IList<SqlParameter> list)
        {
            SqlParameter p = new SqlParameter("ReturnVal", SqlDbType.Int);
            p.Direction = ParameterDirection.ReturnValue;
            list.Add(p);
        }

        public static SqlParameter[] GetSqlWithType(params string[] keyValueTypeParameters)
        {
            if (keyValueTypeParameters == null)
                return null;
            int count = keyValueTypeParameters.Length;
            if (count % 3 != 0)
            {
                throw new ArgumentException("values parameter not correct, Not match key value type arguments");
            }
            List<SqlParameter> list = new List<SqlParameter>();
            for (int i = 0; i < count; i++)
            {

                string name = keyValueTypeParameters[i];

                string sval = keyValueTypeParameters[++i];

                string type = keyValueTypeParameters[++i];

                object val = Types.StringToObject(type, sval);

                var p = new SqlParameter(name, val);
                list.Add(p);
            }

            return list.ToArray();
        }

        public static string ToQueryString(object[] keyValueParameters, string excludeKeys=null)
        {
            if (keyValueParameters == null || keyValueParameters.Length == 0)
            {
                throw new ArgumentNullException("keyValueParameters");
            }
            StringBuilder sb = new StringBuilder();
            int count = keyValueParameters.Length;
            if (count % 2 != 0)
            {
                throw new ArgumentException("values parameter not correct, Not match key value arguments");
            }
            if (excludeKeys != null)
                excludeKeys = excludeKeys.ToLower();
            List<DataParameter> list = new List<DataParameter>();
            for (int i = 0; i < count; i++)
            {
                string key = keyValueParameters[i].ToString().ToLower();
                if (excludeKeys != null)
                {
                    if (excludeKeys.Contains(key))
                        continue;
                }
                sb.AppendFormat("{0}={1}&", key, keyValueParameters[++i]);
            }
            sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }
        public static string ToQueryString(IDbDataParameter[] parameters)
        {
            if (parameters == null || parameters.Length == 0)
            {
                throw new ArgumentNullException("parameters");
            }
            StringBuilder sb = new StringBuilder();

            foreach (DataParameter p in parameters)
            {
                sb.AppendFormat("{0}={1}&", p.ParameterName, p.Value);
            }
            sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }

        public static IDictionary<string, T> ToDictionary<T>(DataParameter[] parameters, bool matchCase)
        {
            if (parameters == null || parameters.Length == 0)
            {
                throw new ArgumentNullException("parameters");
            }
            Dictionary<string, T> sb = new Dictionary<string, T>();

            foreach (DataParameter p in parameters)
            {
                if (matchCase)
                    sb.Add(p.ParameterName, (T)p.Value);
                else
                    sb.Add(p.ParameterName.ToLower(), (T)p.Value);
            }
            return sb;
        }

        public static IDictionary<string, T> ToDictionary<T>(params object[] keyValueParameters)
        {
            if (keyValueParameters == null)
                return null;
            int count = keyValueParameters.Length;
            if (count % 2 != 0)
            {
                throw new ArgumentException("values parameter not correct, Not match key value arguments");
            }
            Dictionary<string, T> list = new Dictionary<string, T>();
            for (int i = 0; i < count; i++)
            {
                var key = keyValueParameters[i].ToString();
                T val = GenericTypes.Convert<T>(keyValueParameters[++i]);
                list[key] = val;
            }

            return list;
        }
        
#endregion

        #region internal static

        internal  static DataParameter[] GetCommandParam(KeySet keys)
        {
            if (keys == null)
            {
                return null;
            }
            int count = keys.Count;
           
            List<DataParameter> prm = new List<DataParameter>();
            foreach (var item in keys)
            {
                prm.Add(new DataParameter(item.Key, item.Value));
            }
            return prm.ToArray();
        }

        #endregion
    }
}
