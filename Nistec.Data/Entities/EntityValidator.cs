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
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using Nistec.Generic;
#pragma warning disable CS1591
namespace Nistec.Data.Entities
{
    public enum RequiredOperation
    {
        NA = 0,
        Update = 1,
        Insert = 2,
        Delete = 3,
        Upsert = 4,
        Updel = 5,
        All = 6
    }
    public enum EntityOperation
    {
        NA = 0,
        Update = 1,
        Insert = 2,
        Delete = 3,
        Upsert = 4
    }

    public class EntityDictionary
    {
        static Dictionary<string, string> dic;

        internal static Dictionary<string, string> RM
        {
            get
            {
                if (dic == null)
                {
                    dic = new Dictionary<string, string>();
                }
                return dic;
            }
        }

        internal static void Add(string name, string defaultLang, string langsKeyValue)
        {
            string mkey = string.Format("{0}:{1}", defaultLang, name);
            string mvalue = "";
            if (RM.TryGetValue(mkey, out mvalue))
            {
                return;
            }

            string[] args = langsKeyValue.Split('|');
            foreach (string a in args)
            {
                string[] names = a.Split(':');
                if (names.Length < 2)
                    continue;
                string key = string.Format("{0}:{1}", names[0], name);
                string value = names[1];
                if (!RM.TryGetValue(key, out value))
                {
                    RM[key] = value;
                }
            }
        }

        public static string Get(string name, string lang)
        {
            string value = name;
            string key = string.Format("{0}:{1}", lang, name);
            if (RM.TryGetValue(key, out value))
            {
                return value;
            }
            return name;
        }

        public static bool TryGet(string name, string lang, out string value)
        {
            string key = string.Format("{0}:{1}", lang, name);
            return RM.TryGetValue(key, out value);
        }

    }

    /// <summary>
    /// This attribute defines properties of method's properties
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ValidatorAttribute : Attribute
    {

        #region Private members
        const int defaultMaxLength = 999999999;

        private string m_name = "";
        private string m_lang = "";
        private string m_defaultLang = "en";
        bool m_Required = false;
        RequiredOperation m_RequiredOperation = RequiredOperation.NA;
        private string m_RequiredVar;
        private object m_MinValue;
        private object m_MaxValue;
        private int m_MinLength=0;
        private int m_MaxLength= defaultMaxLength;
        private string m_regex;
        //bool m_Exists  = false;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidatorAttribute"/> class
        /// </summary>
        public ValidatorAttribute() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidatorAttribute"/> class
        /// </summary>
        /// <param name="name">Is a value of <see cref="Name"/> property</param>
        /// <param name="required">Is required property</param>
        public ValidatorAttribute(string name, bool required)
        {
            Name = name;
            Required = required;
        }

         
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidatorAttribute"/> class with the specified arguments.
        /// </summary>
        /// <param name="name">Is a value of <see cref="Name"/> property</param>
        /// <param name="required">Is required property</param>
        /// <param name="minValue">min value property</param>
        /// <param name="maxValue">max value property</param>
        public ValidatorAttribute(string name, bool required,object minValue, object maxValue)
        {
            Name = name;
            Required = required;
            MinValue = minValue;
            MaxValue = maxValue;
        }

        /// <summary>
        /// An attribute builder method
        /// </summary>
        /// <param name="attr"></param>
        /// <returns></returns>
        public static CustomAttributeBuilder GetAttributeBuilder(ValidatorAttribute attr)
        {
            string name = attr.m_name;
            Type[] arrParamTypes = new Type[] { typeof(string), typeof(bool), typeof(object), typeof(object), typeof(object) };
            object[] arrParamValues = new object[] { name, attr.Required, attr.MinValue, attr.MaxValue, attr.RequiredOperation};
            ConstructorInfo ctor = typeof(ValidatorAttribute).GetConstructor(arrParamTypes);
            return new CustomAttributeBuilder(ctor, arrParamValues);
        }
       #endregion

        #region Properties

        /// <summary>
        /// Parameter name.
        /// </summary>
        public string Name
        {
            get { return m_name == null ? string.Empty : m_name; }
            set { m_name = value; }
        }

        /// <summary>
        /// Parameter langs.
        /// </summary>
        public string Langs
        {
            get { return m_lang == null ? string.Empty : m_lang; }
            set { m_lang = value; }
        }

        /// <summary>
        /// Parameter default lang.
        /// </summary>
        public string DefaultLang
        {
            get { return m_defaultLang == null ? "en" : m_defaultLang; }
            set { m_defaultLang = value; }
        }

        /// <summary>
        /// Parameter RegexPattern.
        /// </summary>
        public string RegexPattern
        {
            get { return m_regex == null ? string.Empty : m_regex; }
            set { m_regex = value; }

        }
        /// <summary>
        /// Indicate if parameter is required.
        /// </summary>
        public bool Required
        {
            get { return m_Required; }
            set { m_Required = value; }
        }

        /// <summary>
        /// Indicate if parameter is required by operation.
        /// </summary>
        public RequiredOperation RequiredOperation
        {
            get { return m_RequiredOperation; }
            set { m_RequiredOperation = value; }
        }


        /// <summary>
        /// Indicate if parameter required is vari.
        /// </summary>
        public string RequiredVar
        {
            get { return m_RequiredVar; }
            set { m_RequiredVar = value; }
        }

        /// <summary>
        /// Indicate the parameter min value.
        /// </summary>
        public object MinValue
        {
            get { return m_MinValue; }
            set { m_MinValue = value; }
        }

        /// <summary>
        /// Indicate the parameter max value.
        /// </summary>
        public object MaxValue
        {
            get { return m_MaxValue; }
            set { m_MaxValue = value; }
        }

        /// <summary>
        /// Indicate the parameter min length.
        /// </summary>
        public int MinLength
        {
            get { return m_MinLength; }
            set { m_MinLength = value; }
        }

        /// <summary>
        /// Indicate the parameter max length.
        /// </summary>
        public int MaxLength
        {
            get { return m_MaxLength; }
            set { m_MaxLength = value; }
        }
        #endregion

        #region Is defined properties

        public bool IsRequiredOperationDefined
        {
            get { return m_RequiredOperation !=  RequiredOperation.NA; }

        }

        public bool IsRequiredVarDefined
        {
            get { return m_RequiredVar != null && m_RequiredVar.Length > 0; }

        }
        /// <summary>
        /// Is Name Defined
        /// </summary>
        public bool IsNameDefined
        {
            get { return m_name != null && m_name.Length > 0; }
        }

        /// <summary>
        /// Is Langs Defined
        /// </summary>
        public bool IsLangsDefined
        {
            get { return m_lang != null && m_lang.Length > 0; }
        }

        /// <summary>
        /// Is MinValue Defined
        /// </summary>
        public bool IsMinValueDefined
        {
            get { return m_MinValue != null; }
        }

        /// <summary>
        /// Is MaxValue Defined
        /// </summary>
        public bool IsMaxValueDefined
        {
            get { return m_MinValue != null; }
        }

        /// <summary>
        /// Is MinValue Defined
        /// </summary>
        public bool IsMinLengthDefined
        {
            get { return m_MinLength != 0 && !(m_MinLength < 0); }
        }

        /// <summary>
        /// Is MaxValue Defined
        /// </summary>
        public bool IsMaxLengthDefined
        {
            get { return m_MaxLength != defaultMaxLength && !(m_MaxLength < 0) && !(m_MaxLength > defaultMaxLength); }
        }

        /// <summary>
        /// Is MaxValue Defined
        /// </summary>
        public bool IsRangeDefined
        {
            get { return m_MinValue != null && m_MaxValue!=null; }
        }

        /// <summary>
        /// Is Regex Defined
        /// </summary>
        public bool IsRegexDefined
        {
            get { return m_regex != null && m_regex.Length > 0; }
        }

        //public bool IsExistsDefined
        //{
        //    get { return m_MinValue != null && m_MaxValue != null; }
        //}


        #endregion

        public string GetName(string lang)
        {
            if (!IsLangsDefined)
                return Name;
            EntityDictionary.Add(Name,DefaultLang, Langs);

            return EntityDictionary.Get(Name, lang);
        }

    }

    public class EntityResults
    {
        public int EffectiveRecords { get; set; }

        public bool IsValid { get; set; }

        public string Result { get; set; }

    }

    public class EntityValidator
    {
        public string Title { get; set; }
        public string Lang { get; private set; }
        public EntityOperation EntityOperation { get; set; }

        public string RequieredFormat { get; set; }
        public string RangeFormat { get; set; }
        public string RegexFormat { get; set; }
        public string CrLf { get; set; }
        
        public bool IsValid 
        {
            get { return Result.Length == 0; }
        }

        public string Result 
        {
            get { return sb.ToString(); }
        }


        StringBuilder sb = new StringBuilder();

        public EntityValidator()
        {
            CrLf = "";
        }

        Dictionary<string, string> _langTitle = new Dictionary<string, string>();
        public Dictionary<string, string> LangTitle
        {
            get { return _langTitle; }
        }
       
        internal void FillLangTitle(string entityName, string mappingName)
        {

            LangTitle["he_RequieredFormat"] = "חובה לציין {0}";
            LangTitle["he_RangeFormat"] = "מחוץ לטווח {0}";
            LangTitle["he_Title"] = entityName;

            LangTitle["en_RequieredFormat"] = "{0} Is Required.";
            LangTitle["en_RangeFormat"] = "{0} Is out of range.";
            LangTitle["en_Title"] = mappingName;

            string[] args = entityName.Split(';', ',');
            foreach (string s in args)
            {
                string[] arg = s.Split(':');
                if (arg.Length == 2)
                    LangTitle[arg[0] + "_Title"] = arg[1];
            }
        }

        internal string ParseTitle(string title, string lang = "en")
        {
            string val=title;
            Dictionary<string, string> langTitle = new Dictionary<string, string>();
            string[] args = title.Split(';', ',');
            foreach (string s in args)
            {
                string[] arg = s.Split(':');
                if (arg.Length == 2)
                    langTitle[arg[0]] = arg[1];
            }
            if(langTitle.TryGetValue(lang,out val))
            {
                return val;
            }
            return title;
        }

        public EntityValidator(string title, string lang = "en")
        {
            Lang = lang;
            switch (lang)
            {
                case "he":
                    RequieredFormat = "חובה לציין {0}";
                    RangeFormat = "מחוץ לטווח {0}";
                    RegexFormat = "ערך לא תקין {0}";
                    break;
                default:
                    RequieredFormat = "{0} Is Required.";
                    RangeFormat = "{0} Is out of range.";
                    RegexFormat = "Incorrect value {0}";
                    break;
            }
            CrLf = "\r\n";
            Title = title;// ParseTitle(title, lang);
        }

        public void Append(string message)
        {
            sb.Append(message + CrLf);
        }

        public void Required(string value, string field)
        {
            if (Types.IsEmpty(value,false))
                sb.AppendFormat(RequieredFormat+ CrLf,field);
        }

        public void Required<T>(T value, string field)
        {
            if (Types.IsEmpty(value,false))
                sb.AppendFormat(RequieredFormat + CrLf, field);
        }

        protected bool ValidateOperation(ValidatorAttribute attr, bool isValid=false)
        {

            switch (EntityOperation)
            {
                case EntityOperation.NA:
                    return isValid;
                case EntityOperation.Delete:
                    return !(attr.RequiredOperation == Entities.RequiredOperation.Delete || attr.RequiredOperation == Entities.RequiredOperation.Updel || attr.RequiredOperation == Entities.RequiredOperation.All);
                case EntityOperation.Upsert:
                    return !(attr.RequiredOperation == Entities.RequiredOperation.Upsert || attr.RequiredOperation == Entities.RequiredOperation.All);
                case EntityOperation.Update:
                    return !(attr.RequiredOperation == Entities.RequiredOperation.Update || attr.RequiredOperation == Entities.RequiredOperation.Upsert || attr.RequiredOperation == Entities.RequiredOperation.Updel || attr.RequiredOperation == Entities.RequiredOperation.All);
                case EntityOperation.Insert:
                    return !(attr.RequiredOperation == Entities.RequiredOperation.Insert || attr.RequiredOperation == Entities.RequiredOperation.Upsert || attr.RequiredOperation == Entities.RequiredOperation.All);
                default:
                    return isValid;
            }
        }
        public void Required(string value, ValidatorAttribute attr)
        {
            if (Types.IsEmpty(value,false))
            {
                bool isValid = ValidateOperation(attr);
                if (!isValid)
                    sb.AppendFormat(RequieredFormat + CrLf, attr.GetName(Lang));
            }
        }

        public void Required<T>(T value, ValidatorAttribute attr)
        {
            if (Types.IsEmpty(value,false))
            {
                bool isValid = ValidateOperation(attr);
                if (!isValid)
                    sb.AppendFormat(RequieredFormat + CrLf, attr.GetName(Lang));
            }
        }

        public void RequiredVar<T>(T value, ValidatorAttribute attr, IDictionary<string, object> reqArgs)
        {
            if (Types.IsEmpty(value,false))
            {
                if (ValidateRequiredVar(attr, reqArgs))
                {
                    return;
                }
                sb.AppendFormat(RequieredFormat + CrLf, attr.GetName(Lang));
            }
        }
        protected bool ValidateRequiredVar(ValidatorAttribute attr, IDictionary<string, object> reqArgs)
        {
            if (string.IsNullOrEmpty(attr.RequiredVar))
                return true;
            if (reqArgs == null)
                return false;
            object val = null;
            var arg = attr.RequiredVar.Split('=');
            if (arg.Length != 2)
                return false;
            if (reqArgs.TryGetValue(arg[0], out val))
            {
                if (val == null || arg[1]==null)
                    return false;
                if (val.ToString().ToLower() == arg[1].ToLower())
                {
                    return false;
                }
            }
            return true;
        }

        public void RequiredVar<T>(T value, ValidatorAttribute attr, object[] reqArgs)
        {
            if (Types.IsEmpty(value, false))
            {
                if (ValidateRequiredVar(attr, reqArgs))
                {
                    return;
                }
                sb.AppendFormat(RequieredFormat + CrLf, attr.GetName(Lang));
            }
        }
        protected bool ValidateRequiredVar(ValidatorAttribute attr, object[] reqArgs)
        {
            if (string.IsNullOrEmpty(attr.RequiredVar))
                return true;
            if (reqArgs == null)
                return false;
            
            var arg = attr.RequiredVar.Split('=');
            if (arg.Length != 2)
                return false;

            if(reqArgs.IsMatch(arg[0],arg[1]))
                return false;
            
            //if (reqArgs.TryGetValue(arg[0], out val))
            //{
            //    if (val == null || arg[1] == null)
            //        return false;
            //    if (val.ToString().ToLower() == arg[1].ToLower())
            //    {
            //        return false;
            //    }
            //}
            return true;
        }
        public void Validate(string value, string message)
        {
            if (Types.IsEmpty(value, false))
                sb.AppendFormat(message + CrLf);
        }

        public void Validate(int value, string message)
        {
            if (Types.IsEmpty(value, false))
                sb.AppendFormat(message + CrLf);
        }
        public void Validate(string value, int min, int max, string message)
        {
            if (Types.IsEmpty(value, false) || value.Length < min || value.Length > max)
                sb.AppendFormat(message + CrLf);
        }

        public void Validate(DateTime value, string message)
        {
            if (value < value.MinSqlValue())
                sb.AppendFormat(message + CrLf);
        }

        public void ValidateField(int value, int min, int max, string field)
        {
            if (Types.IsEmpty(value, false) || value < min || value > max)
                sb.AppendFormat(RangeFormat + CrLf, field);
        }
        public void ValidateField(long value, long min, long max, string field)
        {
            if (Types.IsEmpty(value, false) || value < min || value > max)
                sb.AppendFormat(RangeFormat + CrLf, field);
        }
        public void ValidateField(Int16 value, Int16 min, Int16 max, string field)
        {
            if (Types.IsEmpty(value, false) || value < min || value > max)
                sb.AppendFormat(RangeFormat + CrLf, field);
        }
        public void ValidateField(byte value, byte min, byte max, string field)
        {
            if (Types.IsEmpty(value, false) || value < min || value > max)
                sb.AppendFormat(RangeFormat + CrLf, field);
        }
        public void ValidateField(decimal value, decimal min, decimal max, string field)
        {
            if (Types.IsEmpty(value, false) || value < min || value > max)
                sb.AppendFormat(RangeFormat + CrLf, field);
        }
        public void ValidateField(float value, float min, float max, string field)
        {
            if (Types.IsEmpty(value) || value < min || value > max)
                sb.AppendFormat(RangeFormat + CrLf, field);
        }
        public void ValidateField(double value, double min, double max, string field)
        {
            if (Types.IsEmpty(value, false) || value < min || value > max)
                sb.AppendFormat(RangeFormat + CrLf, field);
        }

        public void ValidateField(string value, int min, int max, string field)
        {
            if (Types.IsEmpty(value, false) || value.Length < min || value.Length > max)
                sb.AppendFormat(RangeFormat + CrLf, field);
        }

        public void ValidateField(DateTime value, DateTime min, DateTime max, string field)
        {
            if (value < value.MinSqlValue() || value < min || value > max)
                sb.AppendFormat(RangeFormat + CrLf, field);
        }

        public void ValidateRegex(object value, string pattern, ValidatorAttribute attr)
        {
            string field = attr.GetName(Lang);
            if (Types.IsEmpty(value,false) || Types.IsEmpty(pattern, false))
            {
                if(attr.Required)
                    sb.AppendFormat(RegexFormat + CrLf, field);
            }
            else if(!Regx.RegexValidateIgnoreCase(pattern, value.ToString()))
                sb.AppendFormat(RegexFormat + CrLf, field);
        }

        public void ValidateRange(object value, object min, object max, string field)
        {
            if (value == null || min == null || max == null)
                return;
            Type type = value.GetType();
            if (type == typeof(DateTime))
                ValidateField((DateTime)value, Types.ToDateTime(min), Types.ToDateTime(max), field);
            if (type == typeof(int))
                ValidateField((int)value, (int)min, (int)max, field);
            if (type == typeof(Int16))
                ValidateField((Int16)value, (Int16)min, (Int16)max, field);
            if (type == typeof(long))
                ValidateField((long)value, (long)min, (long)max, field);
            if (type == typeof(decimal))
                ValidateField((decimal)value, (decimal)min, (decimal)max, field);
            if (type == typeof(float))
                ValidateField((float)value, (float)min, (float)max, field);
            if (type == typeof(double))
                ValidateField((double)value, (double)min, (double)max, field);
            if (type == typeof(byte))
                ValidateField((byte)value, (byte)min, (byte)max, field);
            if (type == typeof(string))
                ValidateField((string)value, (int)min, (int)max, field);
        }

        public void ValidateRange(object value, object min, object max, ValidatorAttribute attr)
        {
            if (value == null || min == null || max == null)
                return;
            string field= attr.GetName(Lang);
            Type type = value.GetType();
            if (type == typeof(DateTime))
                ValidateField((DateTime)value, Types.ToDateTime(min), Types.ToDateTime(max), field);
            if (type == typeof(int))
                ValidateField((int)value, (int)min, (int)max, field);
            if (type == typeof(Int16))
                ValidateField((Int16)value, (Int16)min, (Int16)max, field);
            if (type == typeof(long))
                ValidateField((long)value, (long)min, (long)max, field);
            if (type == typeof(decimal))
                ValidateField((decimal)value, (decimal)min, (decimal)max, field);
            if (type == typeof(float))
                ValidateField((float)value, (float)min, (float)max, field);
            if (type == typeof(double))
                ValidateField((double)value, (double)min, (double)max, field);
            if (type == typeof(byte))
                ValidateField((byte)value, (byte)min, (byte)max, field);
            if (type == typeof(string))
                ValidateField((string)value, (int)min, (int)max, field);
        }

        public void ValidateMin(object value, object min, ValidatorAttribute attr)
        {
            if (value == null || min == null)
                return;
            string field = attr.GetName(Lang);
            Type type = value.GetType();
            if (type == typeof(DateTime))
                ValidateField((DateTime)value, Types.ToDateTime(min), DateTime.MaxValue, field);
            if (type == typeof(int))
                ValidateField((int)value, (int)min, int.MaxValue, field);
            if (type == typeof(Int16))
                ValidateField((Int16)value, (Int16)min, Int16.MaxValue, field);
            if (type == typeof(long))
                ValidateField((long)value, (long)min, long.MaxValue, field);
            if (type == typeof(decimal))
                ValidateField((decimal)value, (decimal)min, decimal.MaxValue, field);
            if (type == typeof(float))
                ValidateField((float)value, (float)min, float.MaxValue, field);
            if (type == typeof(double))
                ValidateField((double)value, (double)min, double.MaxValue, field);
            if (type == typeof(byte))
                ValidateField((byte)value, (byte)min, byte.MaxValue, field);
            if (type == typeof(string))
                ValidateField((string)value, (int)min, int.MaxValue, field);
        }
        public void ValidateMax(object value, object max, ValidatorAttribute attr)
        {
            if (value == null || max == null)
                return;
            string field = attr.GetName(Lang);
            Type type = value.GetType();
            if (type == typeof(DateTime))
                ValidateField((DateTime)value, Types.MinDate, Types.ToDateTime(max), field);
            if (type == typeof(int))
                ValidateField((int)value, int.MinValue, (int)max, field);
            if (type == typeof(Int16))
                ValidateField((Int16)value, Int16.MinValue, (Int16)max, field);
            if (type == typeof(long))
                ValidateField((long)value, long.MinValue, (long)max, field);
            if (type == typeof(decimal))
                ValidateField((decimal)value, decimal.MinValue, (decimal)max, field);
            if (type == typeof(float))
                ValidateField((float)value, float.MinValue, (float)max, field);
            if (type == typeof(double))
                ValidateField((double)value, double.MinValue, (double)max, field);
            if (type == typeof(byte))
                ValidateField((byte)value, byte.MinValue, (byte)max, field);
            if (type == typeof(string))
                ValidateField((string)value, int.MinValue, (int)max, field);
        }

        public void ValidateLength(object value, ValidatorAttribute attr)
        {
            if (value == null)
                return;
            string field = attr.GetName(Lang);
            ValidateField(value.ToString(), attr.MinLength, attr.MaxLength, field);
        }

        public void ValidateEntity(object entity,object[] args)
        {
            if (entity == null)
                return;
            //var validateArgs = DataParameter.ToDictionary<object>(args);
            var props = DataProperties.GetEntityValidator(entity.GetType());

            foreach (var pa in props)
            {

                ValidatorAttribute attr = pa.Attribute;

                if (attr != null)
                {

                    PropertyInfo property = pa.Property;
                    if (!property.CanWrite)
                    {
                        continue;
                    }
                    string field = property.Name;
                    var val = property.GetValue(entity, null);

                    if (!attr.IsNameDefined)
                        attr.Name = field;
                    
                    if (attr.IsRequiredVarDefined)
                    {
                        this.RequiredVar(val, attr, args);
                    }
                    else if (attr.Required)
                    {
                        this.Required(val, attr);
                    }

                    if (attr.IsMinLengthDefined || attr.IsMaxLengthDefined)
                    {
                        this.ValidateLength(val, attr);
                    }
 
                    if (attr.IsRangeDefined)
                    {
                        this.ValidateRange(val, attr.MinValue, attr.MaxValue, attr);
                    }
                    else if (attr.IsMinValueDefined)
                    {
                        this.ValidateMin(val, attr.MinValue, attr);
                    }
                    else if (attr.IsMaxValueDefined)
                    {
                        this.ValidateMax(val, attr.MaxValue, attr);
                    }

                    if (attr.IsRegexDefined)
                    {
                        this.ValidateRegex(val, attr.RegexPattern, attr);
                    }

                    //if (attr.IsRangeDefined)
                    //{
                    //    this.ValidateRange(val, attr.MinValue, attr.MaxValue, attr);
                    //}
                }
            }
            
        }

        /// <summary>
        /// Validate entity.
        /// </summary>
        /// <param name="Entity"></param>
        /// <param name="title"></param>
        /// <param name="lang"></param>
        /// <param name="args"></param>
        /// <exception cref="EntityException"></exception>
        public static void Validate(object Entity, string title, string lang, object[] args=null)
        {
            if (Entity == null)
            {
                throw new EntityException("EntityValidator.Error Invalid Entity");
            }
            EntityValidator validator = new EntityValidator(title, lang);
            validator.ValidateEntity(Entity, args);
            if (!validator.IsValid)
            {
                throw new EntityException(validator.Result);
            }
        }

        public static void Validate<T>(T Entity, object[] args = null) where T : IEntityItem
        {
            if (Entity==null)
            {
                throw new EntityException("EntityValidator.Error Invalid Entity");
            }
            if(Entity is GenericEntity)
            {
                return;
            }
            var map= EntityMappingAttribute.Get<T>();
            if (map == null)
            {
                throw new EntityException("Validate Error: Invalid EntityMappingAttribute");
            }
            string title = map.EntityName;// ?? map.MappingName;
            EntityValidator validator = new EntityValidator(title, map.Lang);
            validator.ValidateEntity(Entity, args);
            if (!validator.IsValid)
            {
                throw new EntityException(validator.Result);
            }
        }
        public static EntityValidator ValidateEntity(object Entity, string title, string lang, object[] args = null)
        {
            if (Entity == null)
            {
                throw new EntityException("EntityValidator.Error Invalid Entity");
            }
            EntityValidator validator = new EntityValidator(title, lang);
            validator.ValidateEntity(Entity, args);
            return validator;
        }
       
        public static EntityValidator ValidateEntityItem<T>(T Entity, object[] args = null) where T : IEntityItem
        {
            if (Entity == null)
            {
                throw new EntityException("EntityValidator.Error Invalid Entity");
            }
            var map = EntityMappingAttribute.Get<T>();
            string title = map.EntityName ?? map.MappingName;
            EntityValidator validator = new EntityValidator(title, map.Lang);
            validator.ValidateEntity(Entity, args);
            return validator;
        }

        public static EntityValidator ValidateEntity(object Entity, string title, string lang, EntityOperation operation, object[] args = null)
        {
            if (Entity == null)
            {
                throw new EntityException("EntityValidator.Error Invalid Entity");
            }
            EntityValidator validator = new EntityValidator(title, lang);
            validator.EntityOperation = operation;

            validator.ValidateEntity(Entity, args);
            return validator;
        }

        public static EntityValidator ValidateEntityItem<T>(T Entity, EntityOperation operation, object[] args = null) where T : IEntityItem
        {
            if (Entity == null)
            {
                throw new EntityException("EntityValidator.Error Invalid Entity");
            }
            var map = EntityMappingAttribute.Get<T>();
            string title = map.EntityName ?? map.MappingName;
            EntityValidator validator = new EntityValidator(title, map.Lang);
            validator.EntityOperation = operation;
            validator.ValidateEntity(Entity, args);
            return validator;
        }

        public static bool ValidateContent(string content, string regexPattern, bool cleanNewLine, bool isNotMatch=false)//enableException=true)//, out string result)
        {
            if (cleanNewLine)
            {
                string NewLine = @"(\r\n)+|\r+|\n+|\t+";
                content = Regex.Replace(content, NewLine, " ", RegexOptions.IgnoreCase);
                content = Regex.Replace(content, "\",\"", " ", RegexOptions.IgnoreCase);
            }
            Match m = Regex.Match(content, regexPattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            //result = m.Value;
            if(isNotMatch)
            {
                if (m.Success)
                {
                    throw new EntityException("EntityValidator.Error The Content is not valid");
                }
                return !m.Success;
            }
            else if (!m.Success)
            {
                throw new EntityException("EntityValidator.Error The Content is not valid");
            }
            return m.Success;
        }
    }
}
