using System;

namespace target.Models.ViewModels
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SwaggerIgnoreAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class SwaggerApiRequiredFieldAttribute : Attribute
    {
    } 
    
    /// <summary>      
    /// 當輸出swagger文件時，若處理分頁API，不輸出此欄位在文件上      
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class SwaggerIgnoreOutputWhenPagedList : Attribute
    {
    }

}
