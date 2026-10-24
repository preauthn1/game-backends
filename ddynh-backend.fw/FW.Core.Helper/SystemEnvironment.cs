using System;
using System.Collections;
using example.invalid;
using System.Reflection;
using System.Web;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Web.Compilation;

namespace FW.Core.Helper
{
    public static class SystemEnvironment
    {
        private static ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();
        private static List<Assembly> _assemblyCache;

        /// <summary>
        /// 程序启动目录（Dll目录）
        /// </summary>
        public static string StartupDirectory
        {
            get
            {
                if (IsWebContext)
                {
                    return System.AppDomain.CurrentDomain.BaseDirectory + "\\Bin\\";
                }
                else
                {
                    return AppDomain.CurrentDomain.BaseDirectory + "\\";
                }
            }
        }

        /// <summary>
        /// 应用程序根目录
        /// </summary>
        public static string RootDirectory
        {
            get
            {
                if (System.AppDomain.CurrentDomain.BaseDirectory.EndsWith("\\"))
                {
                    return System.AppDomain.CurrentDomain.BaseDirectory;
                }

                return System.AppDomain.CurrentDomain.BaseDirectory + "\\";
            }
        }

        /// <summary>
        /// 是否运行在Web上下文中
        /// </summary>
        public static bool IsWebContext
        {
            get
            {
                return HttpContext.Current != null;
            }
        }

        /// <summary>
        /// 系统所依赖的所有非系统程序集
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Assembly> Assemblies
        {
            get
            {
                if (IsWebContext)
                {
                    var assems = BuildManager.GetReferencedAssemblies();
                    var lst = new List<Assembly>();
                    foreach (var assem in assems)
                    {
                        var assembly = assem as Assembly;
                        if (assembly == null)
                        {
                            continue;
                        }

                        if (!IsSystemAssembly(assembly))
                        {
                            lst.Add(assembly);
                        }
                    }

                    return lst;
                }

                try
                {
                    _locker.EnterReadLock();

                    if (_assemblyCache != null)
                    {
                        return _assemblyCache;
                    }



                }
                finally
                {
                    _locker.ExitReadLock();
                }

                try
                {

                    _locker.EnterWriteLock();

                    var dllFolder = new DirectoryInfo(StartupDirectory);

                    _assemblyCache = dllFolder.GetFiles()
                        .Where(f => (String.Compare(f.Extension, ".dll", true) == 0 || String.Compare(f.Extension, ".exe", true) == 0))
                        .Select(f => Assembly.LoadFrom(f.FullName))
                        .Where(a =>
                        {
                            return !IsSystemAssembly(a);
                        })
                        .ToList();

                    return _assemblyCache;
                }
                finally
                {
                    _locker.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// 是否系统程序集
        /// </summary>
        /// <param name="assem"></param>
        /// <returns></returns>
        public static bool IsSystemAssembly(Assembly assem)
        {
            AssemblyProductAttribute pa = assem.GetCustomAttribute<AssemblyProductAttribute>();
            if ((pa != null &&
                (pa.Product.IndexOf("Microsoft") != -1
                || pa.Product.IndexOf("example.invalid") != -1
                || pa.Product.IndexOf("Autofac") != -1
                || pa.Product.IndexOf("ServiceStack") != -1))
                || assem.FullName.IndexOf("Microsoft.") != -1
                || assem.FullName.IndexOf("MongoDB.") != -1)
            {
                return true;
            }

            return false;
        }

       

        
    }
}
