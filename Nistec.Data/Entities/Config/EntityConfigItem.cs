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
using System.Configuration;
#pragma warning disable CS1591
namespace Nistec.Data.Entities.Config
{

    /// <summary>
    /// Represents a configuration element containing a collection of child elements.
    /// </summary>
    public class EntityConfigItems : ConfigurationElementCollection
    {

        public string Get(string key)
        {
            EntityConfigItem item = this[key];
            if (item == null)
                return null;
            return item.EntityName;
        }

  
        public EntityConfigItem this[int index]
        {
            get
            {
                return base.BaseGet(index) as EntityConfigItem;
            }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                this.BaseAdd(index, value);
            }
        }

        public new EntityConfigItem this[string key]
        {
            get { return (EntityConfigItem)BaseGet(key); }
            set
            {
                if (BaseGet(key) != null)
                {
                    BaseRemoveAt(BaseIndexOf(BaseGet(key)));
                }
                BaseAdd(value);
            }
        }

        protected override System.Configuration.ConfigurationElement CreateNewElement()
        {
            return new EntityConfigItem();
        }

        protected override object GetElementKey(System.Configuration.ConfigurationElement element)
        {
            return ((EntityConfigItem)element).EntityName;
        }
    }


    /// <summary>
    /// Represents a entity section  settings within a configuration file.
    /// </summary>
    /// <example>
    ///<section name="EntitySettings" type="Nistec.Data.Entities.Config.EntityConfig, Nistec.Data, Version=4.7.2.0, Culture=neutral, PublicKeyToken=734a739868a76423" requirePermission="false"/>
    ///<EntitySettings>
    ///   <Entities>
    ///     <Entity Name="UserProfile" MappingName="UserProfile" ConnectionKey="Default" Mode="Config" SourceType="Table" EntityKey="UserId" LangResources=""/>
    ///     <Entity Name="UserAuth" MappingName="sp_User_Auth" ConnectionKey="Default" Mode="Config" SourceType="Procedure" EntityKey="UserId" LangResources=""/>
    ///  </Entities>
    ///</EntitySettings>
    /// </example>
    public class EntityConfigItem : System.Configuration.ConfigurationElement
    {

        /// <summary>Get the entity name</summary>
        [ConfigurationProperty("Name", DefaultValue = null, IsRequired = true)]
        public string EntityName
        {
            get { return (string)this["Name"]; }
        }

        /// <summary>Get connection key</summary>
        [ConfigurationProperty("ConnectionKey", DefaultValue = null, IsRequired = true)]
        public string ConnectionKey
        {
            get { return (string)this["ConnectionKey"]; }
        }

        /// <summary>Get the mapping name</summary>
        [ConfigurationProperty("MappingName", DefaultValue = null, IsRequired = true)]
        public string MappingName
        {
            get { return (string)this["MappingName"]; }
        }

        /// <summary>Parameter LangResources. usefull for multi lang</summary>
        [ConfigurationProperty("LangResources", DefaultValue = null, IsRequired = false)]
        public string LangResources
        {
            get { return (string)this["LangResources"]; }
        }

        /// <summary>Parameter EntityMode represent the entity mode.</summary>
        [ConfigurationProperty("Mode", DefaultValue = "NA", IsRequired = false)]
        public string Mode
        {
            get 
            {
                return (string)this["Mode"]; 
                //return GenericTypes.ConvertEnum<EntityMode>((string)this["Mode"], EntityMode.NA); 
            }
        }

        /// <summary>Parameter EntitySourceType represent the entity source type.</summary>
        [ConfigurationProperty("SourceType", DefaultValue = "Table", IsRequired = false)]
        public string SourceType
        {
            get 
            {
                return (string)this["SourceType"]; 
                //return GenericTypes.ConvertEnum<EntitySourceType>(st, EntitySourceType.Table); 
            }
        }
         /// <summary>Get entity keys</summary>
         /// <example>{"ID","Category"}</example>
        [ConfigurationProperty("EntityKey", DefaultValue = null, IsRequired = false)]
        public string EntityKey
        {
            get { return (string)this["EntityKey"]; }
        }
        
    }

    /// <summary>
    /// Represents a entity cache section  settings within a configuration file.
    /// </summary>
    public class EntityConfigCacheItem : System.Configuration.ConfigurationElement
    {
        /// <summary>Get the indicate if entity cache enabled</summary>
        [ConfigurationProperty("Enable", DefaultValue = false, IsRequired = false)]
        public bool Enable
        {
            get {return Types.ToBool( this["Enable"],false); }
        }

        /// <summary>Get cache timeout in minutes</summary>
        [ConfigurationProperty("Timeout", DefaultValue = 30, IsRequired = false)]
        public int Timeout
        {
            get { return Types.ToInt(this["Timeout"],30); }
        }

        /// <summary>Get the remote cache network protocol</summary>
        [ConfigurationProperty("Protocol", DefaultValue = "tcp", IsRequired = false)]
        public string Protocol
        {
            get { return (string)this["Protocol"]; }
        }


    }
}
