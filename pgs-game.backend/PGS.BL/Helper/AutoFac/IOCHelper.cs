using Autofac;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace target.Utility.IOC
{
    public static class IOCHelper
    {

        public static IContainer Container { get; set; }


        public static Dictionary<string, object> MockInstanceMapping = new Dictionary<string, object>();

        /// <summary>
        /// 如果單元測試會強制把這改成true
        /// </summary>
        private static bool _isMSTestMode = false;


        public static void SetContainer(IContainer container)
        {
            Container = container;
        }

        public static void SetMSTestMode()
        {
            _isMSTestMode = true;
        }

        /// <summary>
        /// 若要使用此函式，Runtime啟動環境 必須在global或intial的時候        
        /// 呼叫 IOCInitializeHelper.InitializeIOC(projectList);
        /// </summary>
        public static T Resolve<T>()
        {
            return Container.Resolve<T>();
        }

        /// <summary>
        /// 有些程式碼呼叫頻率太高、為了節省一點new instance浪費。改成自己另外new instance (僅在程式碼需要的時候)
        /// </summary>
        public static T ResolveServiceByCode<T>()
        {
            return Resolve<T>();
        }

        public static object Resolve(Type type)
        {
            if(_isMSTestMode)
            {
                return InstanceMockInterface(type);
            }

            return Container.Resolve(type);
        }


        public static T ResolveByKey<T>(object key)
        {
            if (_isMSTestMode)
            {
                return InstanceMockInterfaceByKey<T>(key);
            }

            return Container.ResolveKeyed<T>(key);
        }


        public static T InstanceRepository<T>() where T: class
        {
            return (T)Activator.CreateInstance(typeof(T));
        }


        public static object InstanceMockInterface(Type type)
        {
            if (MockInstanceMapping.ContainsKey(type.Name))
                return MockInstanceMapping[type.Name];

            return Container.Resolve(type);
        }

        public static bool IsMatchAnyMock(Type type)
        {
            if (MockInstanceMapping.ContainsKey(type.Name))
                return true;

            return false;
        }


        public static T InstanceMockInterfaceByKey<T>(object key)
        {
            string name = typeof(T).Name;

            if (MockInstanceMapping.ContainsKey(name))
                return (T)MockInstanceMapping[name];

            return Container.ResolveKeyed<T>(key);
        }


        public static void MSTestAddMockInstance<T>(object mockInstance)
        {
            string typeName = typeof(T).Name;

            //提示: 若使用mock，必須先呼叫 AddMockInstance、並需自己instance service (不能使用自動IOC注入)
            if (MockInstanceMapping.ContainsKey(typeName))
                return;

            MockInstanceMapping.Add(typeName, mockInstance);
        }


        public static void MSTestRemoveMockInstance<T>()
        {
            string typeName = typeof(T).Name;
            MockInstanceMapping.Remove(typeName);
        }

        public static void MSTestRemoveAllMocks()
        {
            MockInstanceMapping = new Dictionary<string, object>();
        }

    }
}
