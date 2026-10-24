using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity
{
    public class InvalidParameterException : Exception
    {

        public string ColumnName { get; set; }


        public InvalidParameterException(string columnName)
        {
            this.ColumnName = columnName;
        }


        public override string Message
        {
            get
            {
                return $"您表单送出的栏位未填写或格式不合法 ({ColumnName})";
            }
        }
    }
}
