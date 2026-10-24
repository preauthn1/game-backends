using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;


namespace FW.Core.Helper.Reflection
{
    public static class TypeExtensions
    {
        /// <summary>
        /// 检查类型继承关系
        /// </summary>
        /// <param name="fromType"></param>
        /// <param name="checkType"></param>
        /// <returns></returns>
        public static bool CanAssignFrom(this Type fromType, Type checkType)
        {
            if (!fromType.IsGenericType)
            {
                return fromType.IsAssignableFrom(checkType);
            }
            if (checkType == null)
            {
                return false;
            }

            if (fromType.ToString() == checkType.ToString())
            {
                return true;
            }

            if (checkType.IsGenericType && fromType.ToString() == checkType.GetGenericTypeDefinition().ToString())
            {
                return true;
            }

            if (fromType.IsInterface)
            {
                var its = checkType.GetInterfaces();
                foreach (var it in its)
                {
                    if (it.ToString() == fromType.ToString())
                    {
                        return true;
                    }
                }
            }

            if (checkType == null || checkType == typeof(object))
            {
                return false;
            }

            return CanAssignFrom(fromType, checkType.BaseType);

        }

        /// <summary>
        /// 查找子类型
        /// </summary>
        /// <param name="parentType"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        public static IList<Type> GetChildTypes(this Type parentType, Type[] types)
        {
            IList<Type> rets = new List<Type>();

            Array.ForEach(types, t => {
                if (parentType == t)
                {
                    return;
                }

                if (parentType.CanAssignFrom(t))
                {
                    rets.Add(t);
                }
            });


            return rets;
        }

        /// <summary>
        /// 在指定的程序集中查找子类
        /// </summary>
        /// <param name="parentType"></param>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static IList<Type> GetChildTypesFromAssembly(this Type parentType, Assembly assembly)
        {
            return parentType.GetChildTypes(assembly.GetTypes());
        }

        /// <summary>
        /// 在指定的程序集集合中查找子类
        /// </summary>
        /// <param name="parentType"></param>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public static IList<Type> GetChildTypesFromAssemblies(this Type parentType, Assembly[] assemblies)
        {
            List<Type> rets = new List<Type>();
            Array.ForEach(assemblies, assem => {
                rets.AddRange(parentType.GetChildTypes(assem.GetTypes()));
            });

            return rets;
        }

        /// <summary>
        /// 从当前应用程序域中查找子类
        /// </summary>
        /// <param name="parentType"></param>
        /// <returns></returns>
        public static IList<Type> GetChildTypeFromAppDomain(this Type parentType)
        {
           return parentType.GetChildTypesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());
        }

        /// <summary>
        /// 从bin目录的所有dll中查找子类型
        /// </summary>
        /// <param name="parentType"></param>
        /// <returns></returns>
        public static IList<Type> GetChildTypesFromBinDirectory(this Type parentType)
        {
            return parentType.GetChildTypesFromAssemblies(Helper.SystemEnvironment.Assemblies.ToArray());
        }


    }
}
