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
using System.Collections;
using System.Reflection;
using System.Data;

using System.Runtime.InteropServices;
using System.Collections.Generic;
using Nistec.Data.Entities;
using Nistec.Data;
#pragma warning disable CS1591
namespace Nistec.Data
{

    //public class Args: Dictionary<string,object>
    //{

    //    public static Args Get(params object[] keyValueParameters)
    //    {
    //        int count = keyValueParameters.Length;
    //        if (count % 2 != 0)
    //        {
    //            throw new ArgumentException("values parameter not correct, Not match key value arguments");
    //        }
    //        Args a = new Args();
    //        for (int i = 0; i < count; i++)
    //        {
    //            a.Add(keyValueParameters[i].ToString(), keyValueParameters[++i]);
    //        }
    //        return a;
    //    }

    //    public static IDbDataParameter[] Get(params object[] keyValueParameters)
    //    {

    //        int count = keyValueParameters.Length;
    //        if (count % 2 != 0)
    //        {
    //            throw new ArgumentException("values parameter not correct, Not match key value arguments");
    //        }
    //        List<IDbDataParameter> list = new List<IDbDataParameter>();
    //        for (int i = 0; i < count; i++)
    //        {
    //            list.Add(new DataParameter(keyValueParameters[i].ToString(), keyValueParameters[++i]));
    //        }

    //        return list.ToArray();
    //    }
    //}

    /*
     [StructLayout(LayoutKind.Sequential)]
     public struct KeyValueItem
     {
         public readonly string Key;
         public readonly object Value;
         public KeyValueItem(string key, object value)
         {
             this.Key = key;
             this.Value = value;
         }

         public bool Equals(KeyValueItem item)
         {
             return item.Key.Equals(Key) && item.Value.Equals(Value);
         }

         public bool Equals(string key, object value)
         {
             return key.Equals(Key) && value.Equals(Value);
         }

      }
       */
    public class KeyValueIndexItem : KeyValueItem<object>, IKeyValueItem //,IEntityItem
    {
        public Int64 Index { get; set; }

        public static KeyValueIndexItem Create(DataRow values)
        {
            if (values == null || values.ItemArray.Length < 3)
            {
                throw new ArgumentException("KeyValueItem.Create DataRow is  incorrect");
            }
            return new KeyValueIndexItem()
            {
                Key = string.Format("{0}", values[0]),
                Value = values[1],
                Index = Types.ToLong(values[2])
            };
        }
        public static IList<KeyValueIndexItem> CreateList(DataTable values, bool allowNull = true)
        {

            if (values == null)// || values.Rows.Count == 0)
            {
                throw new ArgumentNullException("KeyValueItem.CreateList.values");
            }

            IList<KeyValueIndexItem> list = new List<KeyValueIndexItem>();

            foreach (DataRow row in values.Rows)
            {
                var item = new KeyValueIndexItem()
                {
                    Key = string.Format("{0}", row[0]),
                    Value = row[1],
                    Index = Types.ToLong(row[2])
                };
                if (item.Value == null && allowNull == false)
                    continue;
                list.Add(item);
            }

            return list;
        }
    }

    public class KeyValueItem : KeyValueItem<object>, IKeyValueItem //,IEntityItem
    {
        public KeyValueItem() { }
        public KeyValueItem(string key, object value) { Key = key;Value = value; }

        //public Int64 Index { get; set; }
        public static KeyValueItem Create(DataRow values)
        {
            if (values == null || values.ItemArray.Length < 2)
            {
                throw new ArgumentException("KeyValueItem.Create DataRow is  incorrect");
            }
            int length = values.ItemArray.Length;

            return new KeyValueItem()
            {
                Key = string.Format("{0}", values[0]),
                Value = values[1]
                //Index = (length == 3) ? Types.ToLong(values[2]) : 0
            };
        }
        public static IList<KeyValueItem> CreateList(DataTable values, bool allowNull = true)
        {

            if (values == null)// || values.Rows.Count == 0)
            {
                throw new ArgumentNullException("KeyValueItem.CreateList.values");
            }

            IList<KeyValueItem> list = new List<KeyValueItem>();

            int length = values.Columns.Count;
            foreach (DataRow row in values.Rows)
            {
                var item = new KeyValueItem()
                {
                    Key = string.Format("{0}", row[0]),
                    Value = row[1]
                    //Index = (length == 3) ? Types.ToLong(row[2]) : 0
                };
                if (item.Value == null && allowNull == false)
                    continue;
                list.Add(item);
            }

            return list;
        }
    }

    public class KeyValueItem<T> : IKeyValueItem<T> //IEntityItem
    {

        public static List<KeyValueItem<T>> GetList(params object[] keyValueArgs)
        {
            List<KeyValueItem<T>> list = new List<KeyValueItem<T>>();
            if (keyValueArgs == null)
                return list;
            int count = keyValueArgs.Length;
            if (count % 2 != 0)
            {
                throw new ArgumentException("keyValueArgs is  incorrect, Not match key value arguments");
            }
            for (int i = 0; i < count; i++)
            {
                object val = keyValueArgs[i];
                string name = (string)keyValueArgs[++i];

                T value = GenericTypes.Convert<T>(val);
                var entity = new KeyValueItem<T>() { Value = value, Key = name };
                list.Add(entity);
            }
            return list;
        }
        public static KeyValueItem<T> Get(params object[] keyValueArgs)
        {
            if (keyValueArgs == null)
                return new KeyValueItem<T>();
            int count = keyValueArgs.Length;
            if (count % 2 != 0)
            {
                throw new ArgumentException("keyValueArgs is  incorrect, Not match key value arguments");
            }
            object val = keyValueArgs[0];
            string name = (string)keyValueArgs[1];
            T value = GenericTypes.Convert<T>(val);
            var entity = new KeyValueItem<T>() { Value = value, Key = name };
            return entity;
        }
        public static KeyValueItem<T> Create(DataRow values, string fieldKey, string fieldValue)
        {
            return new KeyValueItem<T>()
            {
                Key = string.Format("{0}", values[fieldKey]),
                Value = GenericTypes.Convert<T>(values[fieldValue])
            };
        }
        public static IList<KeyValueItem<T>> CreateList(DataTable values, string fieldKey, string fieldValue, bool allowNull = true)
        {

            if (values == null)
            {
                throw new ArgumentNullException("KeyValueItem.CreateList.values");
            }
            if (string.IsNullOrEmpty(fieldKey) || string.IsNullOrEmpty(fieldValue))
            {
                throw new ArgumentNullException("KeyValueItem.CreateList.fields");
            }

            if (!values.Columns.Contains(fieldKey))
            {
                throw new ArgumentException("KeyValueItem.CreateList, Invalid column name " + fieldKey);
            }
            if (!values.Columns.Contains(fieldValue))
            {
                throw new ArgumentException("KeyValueItem.CreateList, Invalid column name " + fieldValue);
            }

            IList<KeyValueItem<T>> list = new List<KeyValueItem<T>>();

            foreach (DataRow row in values.Rows)
            {
                var item = Create(row, fieldKey, fieldValue);
                if (item.Value == null && allowNull == false)
                    continue;
                list.Add(item);
            }

            return list;
        }
 

        public T Value { get; set; }

        [EntityProperty(EntityPropertyType.Key)]
        public string Key { get; set; }
    }


  
}
