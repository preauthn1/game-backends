using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using target.Models.Extensions;

namespace target.Models.Attributes
{
    /// <summary>
    /// 用來標記文件用的attribute 實際無用途
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class ReferenceText : System.Attribute
    {
        public string Value { get; set; }

        public ReferenceText(string value)
        {
            this.Value = value;
        }

        public ReferenceText(object value)
        {
            if(value != null)
            {
                this.Value = value.ToString();
            }
        }
    }

    public class ReferType : System.Attribute
    {
        public Type Type { get; set; }

        public ReferType(Type value)
        {
            this.Type = value;
        }
    }

    public class ReferDbStringType : System.Attribute
    {
        public bool IsNVarchar { get; set; }

        public int TextLength { get; set; }

        public ReferDbStringType()
        {

        }

        public ReferDbStringType(bool isNVarchar , int textLegnth)
        {
            this.IsNVarchar = isNVarchar;
            this.TextLength = textLegnth;
        }
    }

    public class ReferDbDecimalType : System.Attribute
    {

        /// <summary>
        /// 整數位數
        /// </summary>
        public byte Precision { get; set; }

        /// <summary>
        /// 小數點位數
        /// </summary>
        public byte Scale { get; set; }


        public ReferDbDecimalType()
        {

        }

        public ReferDbDecimalType(byte myPrecision, byte myScale)
        {
            this.Precision = myPrecision;
            this.Scale = myScale;
        }
    }



}
