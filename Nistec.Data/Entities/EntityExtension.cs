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
using System.Reflection;
using Nistec.Generic;
using System.Collections;
using System.Data;
using Nistec.Xml;
using System.Xml;
using System.Xml.Serialization;
using Nistec.Data.Entities.Cache;
using Nistec.Runtime;
using System.IO;
using Nistec.Serialization;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Data.SqlClient;
#pragma warning disable CS1591
namespace Nistec.Data.Entities
{

    public static class EntityExtension
    {

        public static string EntityName<T>(this IEntityItem item) where T : IEntityItem
        {
            return EntityMappingAttribute.Name<T>();
        }
        public static string EntityMapping<T>(this IEntityItem item) where T : IEntityItem
        {
            return EntityMappingAttribute.Mapping<T>();
        }
        public static string EntityViewName<T>(this IEntityItem item) where T : IEntityItem
        {
            return EntityMappingAttribute.View<T>();
        }

        public static string EntityPrimaryKey<T>(this IEntityItem item) where T : IEntityItem
        {
            return EntityPropertyBuilder.GetEntityPrimaryKey<T>(item);
        }


        #region update / insert / delete



        public static int Update<T>(this EntityContext<T> context, T newEntity, Func<UpdateCommandType, EntityValidator> validate) where T : IEntityItem
        {
            T current = context.Entity;

            var validator = validate == null ? null : validate(UpdateCommandType.Update);
            if (validator != null && !validator.IsValid)
            {
                throw new Exception("EntityValidator eror: " + validator.Result);
            }

            return context.SaveChanges(newEntity);
        }

        public static int Insert<T>(this EntityContext<T> context, Func<UpdateCommandType, EntityValidator> validate) where T : IEntityItem
        {
            T current = context.Entity;
            var validator = validate == null ? null : validate(UpdateCommandType.Insert);
            if (validator != null && !validator.IsValid)
            {
                throw new Exception("EntityValidator eror: " + validator.Result);
            }

            return context.SaveChanges(UpdateCommandType.Insert);
        }

        public static int Delete<T>(this EntityContext<T> context, Func<UpdateCommandType, EntityValidator> validate) where T : IEntityItem
        {
            T current = context.Entity;
            var validator = validate == null ? null : validate(UpdateCommandType.Delete);
            if (validator != null && !validator.IsValid)
            {
                throw new Exception("EntityValidator eror: " + validator.Result);
            }

            return context.SaveChanges(UpdateCommandType.Delete);
        }


        #endregion

        #region Entity Extension

        public static Dictionary<string, T> CreateEntityList<T>(DataTable dt)
        {

            bool isGR = (typeof(T) == typeof(GenericEntity));
            GenericEntity[] records = GenericEntity.CreateEntities(dt, isGR);
            Dictionary<string, T> values = new Dictionary<string, T>();

            if (isGR)
            {
                foreach (var gr in records)
                {
                    T item = GenericTypes.Cast<T>(gr, true);
                    if (gr.PrimaryKey != null)
                    {
                        values.Add(gr.PrimaryKey.ToString(), item);
                    }
                }
            }
            else
            {
                //EntityKeys entityKeys = EntityKeys.BuildKeys<T>();

                foreach (var gr in records)
                {
                    T item = ActivatorUtil.CreateInstance<T>();

                    //string key = entityKeys.CreateEntityPrimaryKey(gr);
                    //if (!string.IsNullOrEmpty(key))
                    //{
                    //    values.Add(key, item);
                    //}

                    gr.PrimaryKey = EntityPropertyBuilder.SetEntityContextWithKey(item, gr);
                    if (gr.PrimaryKey != null)
                    {
                        values.Add(gr.PrimaryKey.ToString(), item);
                    }
                }
            }


            return values;
        }

        public static ConcurrentDictionary<string, T> CreateConcurrentEntityList<T>(IEntity context, DataTable dt)
        {

            bool isGR = (typeof(T) == typeof(GenericEntity));
            GenericEntity[] records = null;
            EntityAttribute keyattr = null;
            ConcurrentDictionary<string, T> values = new ConcurrentDictionary<string, T>();

            if (isGR)
            {
                string[] fieldsKey = context.EntityDb.EntityKeys.ToArray();
                records = GenericEntity.CreateEntities(dt, fieldsKey);

                foreach (var gr in records)
                {
                    T item = GenericTypes.Cast<T>(gr, true);

                    if (gr.PrimaryKey != null)
                    {
                        values.TryAdd(gr.PrimaryKey.ToString(), item);
                    }
                }
            }
            else
            {
                keyattr = GetEntityAttribute(context);
                records = GenericEntity.CreateEntities(dt, false);
                //EntityKeys entityKeys = EntityKeys.BuildKeys<T>();

                foreach (var gr in records)
                {
                    T item = ActivatorUtil.CreateInstance<T>();
                    //string key = entityKeys.CreateEntityPrimaryKey(gr);
                    //if (!string.IsNullOrEmpty(key))
                    //{
                    //    values.TryAdd(key, item);
                    //}
                    gr.PrimaryKey = EntityPropertyBuilder.SetEntityContextWithKey(item, gr);
                    if (gr.PrimaryKey != null)
                    {
                        values.TryAdd(gr.PrimaryKey.ToString(), item);
                    }
                }
            }

            return values;
        }

        public static Dictionary<string, T> CreateEntityList<T>(IEntity context, DataTable dt)
        {

            bool isGR = (typeof(T) == typeof(GenericEntity));
            GenericEntity[] records = null;
            EntityAttribute keyattr = null;
            Dictionary<string, T> values = new Dictionary<string, T>();

            if (isGR)
            {
                string[] fieldsKey = context.EntityDb.EntityKeys.ToArray();
                records = GenericEntity.CreateEntities(dt, fieldsKey);

                foreach (var gr in records)
                {
                    T item = GenericTypes.Cast<T>(gr, true);

                    if (gr.PrimaryKey != null)
                    {
                        values.Add(gr.PrimaryKey.ToString(), item);
                    }
                }
            }
            else
            {
                keyattr = GetEntityAttribute(context);
                records = GenericEntity.CreateEntities(dt, false);
                //EntityKeys entityKeys = EntityKeys.BuildKeys<T>();

                foreach (var gr in records)
                {
                    T item = ActivatorUtil.CreateInstance<T>();
                    //string key = entityKeys.CreateEntityPrimaryKey(gr);
                    //if (!string.IsNullOrEmpty(key))
                    //{
                    //    values.Add(key, item);
                    //}
                    gr.PrimaryKey = EntityPropertyBuilder.SetEntityContextWithKey(item, gr);
                    if (gr.PrimaryKey != null)
                    {
                        values.Add(gr.PrimaryKey.ToString(), item);
                    }
                }
            }

            return values;
        }

        public static Dictionary<string, object> CreateEntityList(IEntity context, DataTable dt)
        {

            GenericEntity[] records = GenericEntity.CreateEntities(dt);

            Type type = context.GetType();
            Dictionary<string, object> values = new Dictionary<string, object>();

            foreach (var gr in records)
            {
                object item = ActivatorUtil.CreateInstance(type);
                var entityKey = EntityPropertyBuilder.SetEntityContextWithKey(item, gr.Record);
                if (entityKey != null)
                {
                    gr.PrimaryKey = entityKey;//.FieldsKey;
                    values.Add(entityKey.ToString(), item);
                }
            }

            return values;
        }

        #endregion

        #region EntityProperties/EntityAttribute

        public static EntityAttribute GetEntityAttribute(IEntity context)
        {
            EntityAttribute keyattr = AttributeProvider.GetCustomAttribute<EntityAttribute>(context.GetType());

            if (keyattr == null)
            {
                keyattr = new EntityAttribute(context.EntityDb);
            }
            if (keyattr == null)
            {
                throw new Exception("GenericRecord.CreateEntityList<T>, DataTable has no PrimaryKey");
            }
            return keyattr;
        }

        public static PropertyInfo[] GetEntityProperties(object instance, bool canRead, bool canWright)
        {
            return AttributeProvider.GetActiveProperties<EntityPropertyAttribute>(instance, canRead, canWright);
        }

        public static PropertyInfo[] GetEntityProperties(object instance, bool canRead, bool canWright, bool disableIdentity)
        {
            return AttributeProvider.GetActiveProperties<EntityPropertyAttribute>(instance, canRead, canWright, disableIdentity);
        }

        #endregion

        #region EntityDictionary

        public static IDictionary CreateEntityList(IEntity entity, DataFilter filter, Action<Exception> onError)
        {
            try
            {
                //err = null;

                if (entity == null)
                    return null;
                if (!entity.EntityDb.HasConnection)
                    return null;
                DataTable dt = entity.EntityDb.DoCommand<DataTable>(filter);

                return CreateEntityList(entity, dt);// GenericRecord.CreateEntityList(entity, dt);// EntityFormatter.EntityIDictionary(entity, dt);

            }
            catch (Exception ex)
            {
                if (onError != null)
                {
                    onError(ex);
                }
            }
            return null;
        }

        public static Dictionary<string, T> CreateEntityList<T>(IEntity<T> entity, DataFilter filter, Action<Exception> onError)
        {

            try
            {
                if (entity == null)
                    return null;
                if (!entity.EntityDb.HasConnection)
                    return null;
                DataTable dt = entity.EntityDb.DoCommand<DataTable>(filter);

                return CreateEntityList<T>(entity, dt);

            }
            catch (Exception ex)
            {
                if (onError != null)
                {
                    onError(ex);
                }
            }
            return null;
        }


        public static ConcurrentDictionary<string, T> CreateConcurrentEntityList<T>(IEntity<T> entity, DataFilter filter, Action<Exception> onError)
        {

            try
            {
                if (entity == null)
                    return null;
                if (!entity.EntityDb.HasConnection)
                    return null;
                DataTable dt = entity.EntityDb.DoCommand<DataTable>(filter);

                return CreateConcurrentEntityList<T>(entity, dt);

            }
            catch (Exception ex)
            {
                if (onError != null)
                {
                    onError(ex);
                }
            }
            return null;
        }

        public static ConcurrentDictionary<string, T> CreateConcurrentEntityList<T>(IEntity<T> entity, DataTable dt, Action<Exception> onError)
        {

            try
            {
                if (entity == null)
                    return null;
                //if (!entity.EntityDb.HasConnection)
                //    return null;
                //DataTable dt = entity.EntityDb.DoCommand<DataTable>(filter);

                return CreateConcurrentEntityList<T>(entity, dt);

            }
            catch (Exception ex)
            {
                if (onError != null)
                {
                    onError(ex);
                }
            }
            return null;
        }

        public static DataTable CreateEntityData<T>(IEntity<T> entity, DataFilter filter, Action<Exception> onError)
        {

            try
            {
                if (entity == null)
                    return null;
                if (!entity.EntityDb.HasConnection)
                    return null;
                DataTable dt = entity.EntityDb.DoCommand<DataTable>(filter);

                return dt;

            }
            catch (Exception ex)
            {
                if (onError != null)
                {
                    onError(ex);
                }
            }
            return null;
        }

        #endregion

        #region ReadStream/WriteStream

        public static int WriteStream<T>(IEntity<T> entity, Stream stream, Action<Exception> onError)
        {
            int length = 0;

            try
            {
                if (stream == null)
                {
                    throw new ArgumentNullException("WriteStream.stream");
                }
                if (stream.CanWrite == false)
                {
                    throw new IOException("WriteStream.stream can not write");
                }
                entity.EntityWrite(stream, null);
            }
            catch (Exception)
            {
            }
            return length;
        }

        public static void ReadStream<T>(IEntity<T> entity, Stream stream, Action<Exception> onError, int bufferSize = 8192)
        {

            try
            {
                if (stream == null)
                {
                    throw new ArgumentNullException("ReadStream.stream");
                }
                if (stream.CanRead == false)
                {
                    throw new IOException("ReadStream.stream can not read");
                }
                entity.EntityRead(stream, null);

            }
            catch (Exception)
            {
            }
        }
        #endregion

        #region ToSqlParameters

        public static IList<SqlParameter> ToSqlParameters<T>(this T instance) where T : IEntityItem
        {
            List<SqlParameter> parameters = new List<SqlParameter>();

            var props = DataProperties.GetEntityProperties(typeof(T));
            //props = props.Where(p => p.Attribute.Order > 0).OrderBy(p => p.Attribute.Order);

            foreach (var pa in props)
            {
                PropertyInfo property = pa.Property;
                EntityPropertyAttribute attr = pa.Attribute;
                if (attr != null)
                {
                    if (attr.ParameterType == EntityPropertyType.NA || attr.ParameterType == EntityPropertyType.View)
                        continue;

                    string key = attr.GetParamName(pa.Property.Name);//attr.IsColumnDefined ? attr.Column : pa.Property.Name;
                    object val = property.GetValue(instance, null);
                    parameters.Add(new SqlParameter(key, val));
                }
                else
                {
                    parameters.Add(new SqlParameter(property.Name, property.GetValue(instance, null)));
                }
            }
            return parameters;
        }

        public static IList<SqlParameter> ToSqlParameters<T>(this T instance, params string[] Exclude) where T : IEntityItem
        {
            List<SqlParameter> parameters = new List<SqlParameter>();

            var props = DataProperties.GetEntityProperties(typeof(T));
            //props = props.Where(p => p.Attribute.Order > 0).OrderBy(p => p.Attribute.Order);

            foreach (var pa in props)
            {
                PropertyInfo property = pa.Property;
                EntityPropertyAttribute attr = pa.Attribute;
                if (!Exclude.Contains(property.Name))
                {
                    if (attr != null)
                    {
                        if (attr.ParameterType == EntityPropertyType.NA || attr.ParameterType == EntityPropertyType.View)
                            continue;

                        string key = attr.GetParamName(pa.Property.Name);//attr.IsColumnDefined ? attr.Column : pa.Property.Name;
                        object val = property.GetValue(instance, null);
                        parameters.Add(new SqlParameter(key, val));
                    }
                    else
                    {
                        parameters.Add(new SqlParameter(property.Name, property.GetValue(instance, null)));
                    }
                }
            }
            return parameters;
        }

        public static IList<SqlParameter> ToSqlParameters<T>(this T instance, bool addReturnValue) where T : IEntityItem
        {
            var parameters = ToSqlParameters(instance);
            if (addReturnValue)
            {
                DataParameter.AddReturnValueParameter(parameters);
            }
            return parameters;
        }

        public static IList<SqlParameter> ToSqlParameters<T>(this T instance,bool addReturnValue, params object[] keyValueParameters) where T : IEntityItem
        {
            var parameters = ToSqlParameters(instance);
            DataParameter.AddToSqlList(parameters,keyValueParameters);
            if (addReturnValue)
            {
                DataParameter.AddReturnValueParameter(parameters);
            }
            return parameters;
        }

        public static IList<SqlParameter> ToSqlParameters<T>(this T instance, List<string> refList, params object[] keyValueParameters) where T : IEntityItem
        {
            var parameters = ToSqlParameters(instance,false, keyValueParameters);
            foreach (var item in parameters)
            {
                if (refList.Contains(item.ParameterName))
                {
                    item.Direction = ParameterDirection.InputOutput;
                }
            }
            return parameters;
        }

        public static Dictionary<string, object> ToDictionary<T>(this IEntityItem entity, params object[] keyValueParameters)
        {
            bool enableAttributeColumn = false;
            Dictionary<string, object> args = new Dictionary<string, object>();
            T instance = ActivatorUtil.CreateInstance<T>();

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
                        //if (attr.ParameterType == EntityPropertyType.Optional)
                        //{
                        //    Console.WriteLine("Optional");
                        //}

                        object value = property.GetValue(instance, null);// form[field];
                        if (value == null)
                        {
                            if (attr.ParameterType == EntityPropertyType.Optional)
                                continue;
                            value = attr.AsNull;
                        }
                        //GenericTypes.Convert(value, property.PropertyType);
                        var val = Types.ChangeType(value, property.PropertyType);
                        args[field] = val;

                        //property.SetValue(instance, Types.ChangeType(value, property.PropertyType), null);
                        //list.Add(new SqlParameter(field, value));

                        //args[field] = value;
                    }
                }
            }


            int count = keyValueParameters.Length;
            if (count > 0)
            {
                if (count % 2 != 0)
                {
                    throw new ArgumentException("values parameter not correct, Not match key value arguments");
                }

                for (int i = 0; i < count; i++)
                {
                    string key = keyValueParameters[i].ToString();
                    object value = keyValueParameters[++i];
                    args[key] = value;
                }
            }

            return args;
        }

        #endregion

        public static object[] EntityToNameValue<T>(this T instance, params string[] Exclude) where T : IEntityItem
        {
            List<object> keyValues = new List<object>();

            var props = DataProperties.GetEntityProperties(typeof(T));
            //props = props.Where(p => p.Attribute.Order > 0).OrderBy(p => p.Attribute.Order);

            foreach (var pa in props)
            {
                PropertyInfo property = pa.Property;
                EntityPropertyAttribute attr = pa.Attribute;
                if (!Exclude.Contains(property.Name))
                {
                    if (attr != null)
                    {
                        if (attr.ParameterType == EntityPropertyType.NA || attr.ParameterType == EntityPropertyType.View)
                            continue;


                        string key = attr.GetParamName(pa.Property.Name);//attr.IsColumnDefined ? attr.Column : pa.Property.Name;
                        object val = property.GetValue(instance, null);

                        keyValues.Add(key);
                        keyValues.Add(val);

                    }
                    else
                    {
                        keyValues.Add(property.Name);
                        keyValues.Add(property.GetValue(instance, null));
                    }
                }
            }
            return keyValues.ToArray();
        }

        public static T ToEntity<T>(string commaString, char splitterList = '|', char spliterKeyValue = '=')
        {
            var collection = KeyValueUtil.ParseCommaString(commaString, splitterList, spliterKeyValue);

            return ToEntity<T>(collection);
        }

        public static T ToEntity<T>(NameValueCollection form)
        {

            T instance = ActivatorUtil.CreateInstance<T>();

            PropertyInfo[] props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance, false);

            foreach (var property in props)
            {

                if (!property.CanRead)
                {
                    continue;
                }

                if (property.CanWrite)
                {

                    object value = form.Get(property.Name);
                    if (value == null)
                    {
                        value = GenericTypes.Default(property.GetType());
                    }
                    property.SetValue(instance, Types.ChangeType(value, property.PropertyType), null);
                }
            }

            return instance;
        }

        public static T Create<T>(string commaString, char splitterList = '|', char spliterKeyValue = '=', bool enableAttributeColumn = false) where T : IEntityItem
        {
            var collection = KeyValueUtil.ParseCommaString(commaString, splitterList, spliterKeyValue);

            return Create<T>(collection);
        }
        public static T Create<T>(this NameValueCollection form, bool enableAttributeColumn = false) where T : IEntityItem
        {

            T instance = ActivatorUtil.CreateInstance<T>();

            var props = DataProperties.GetEntityProperties(typeof(T), true);
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
                        //if (attr.ParameterType == EntityPropertyType.Optional)
                        //{
                        //    Console.WriteLine("Optional");
                        //}

                        object value = form[field];
                        if (value == null)
                        {
                            if (attr.ParameterType == EntityPropertyType.Optional)
                                continue;
                            value = attr.AsNull;
                        }
                        property.SetValue(instance, Types.ChangeType(value, property.PropertyType), null);
                    }
                }
            }


            return instance;
        }

        public static IDictionary EntityAsDictionary(object context)
        {
            if (context == null)
            {
                return null;// throw new ArgumentNullException("EntityDictionary.dt");
            }

            Type type = context.GetType();

            if (typeof(IEntityDictionary).IsAssignableFrom(type))
            {
                return ((IEntityDictionary)context).EntityDictionary();
            }

            return (IDictionary)context;
        }

        public static byte[] EntityAsBinary(object context, Formatters formatter = Formatters.BinarySerializer)
        {
            if (context == null)
            {
                return null;// throw new ArgumentNullException("EntityDictionary.dt");
            }

            Type type = context.GetType();

            if (typeof(IEntityDictionary).IsAssignableFrom(type))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    ((IEntityDictionary)context).EntityWrite(ms, null);
                    return ms.ToArray();
                }

            }


            return NetSerializer.SerializeBinary(context, formatter);
        }

    }
}
