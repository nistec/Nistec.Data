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
using Nistec.Generic;
//using Nistec.Channels.Config;
#pragma warning disable CS1591
namespace Nistec.Data.Entities.Config
{
    /// <summary>
    /// Entities configuration
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
    public class EntityConfig : ConfigurationSection
    {

        private static EntityConfig settings;
        //= ConfigurationManager.GetSection("MData") as EntityConfig;

        /// <summary>
        /// Get the <see cref="EntityConfig"/>
        /// </summary>
        public static EntityConfig Settings
        {
            get
            {
                if (settings == null)
                {
                    settings = (EntityConfig)ConfigurationManager.GetSection("EntitySettings") ?? new EntityConfig();
                }
                return settings;
            }
        }

        public static EntityConfig GetConfig()
        {
            if (settings == null)
            {
                settings = (EntityConfig)ConfigurationManager.GetSection("EntitySettings") ?? new EntityConfig();
            }
            return settings;

            //return (EntityConfig)System.Configuration.ConfigurationManager.GetSection("EntitySettings");// ?? new EntityConfig();
        }



        [System.Configuration.ConfigurationProperty("Entities")]
        [ConfigurationCollection(typeof(EntityConfigItems), AddItemName = "Entity")]
        public EntityConfigItems EntitySettings
        {
            get
            {
                object o = this["Entities"];
                return o as EntityConfigItems;
            }
        }

        [System.Configuration.ConfigurationProperty("EntityCache")]
        public EntityConfigCacheItem EntityCache
        {
            get
            {
                object o = this["EntityCache"];
                return o as EntityConfigCacheItem;
            }
        }

 
    }
}
