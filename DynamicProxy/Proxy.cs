using DerekTjt.DynamicProxy.ProxyException;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using DerekTjt.DynamicProxy;
using DerekTjt.DynamicProxy.Filter;
using DerekTjt.DynamicProxy.Common;
using DerekTjt.DynamicProxy.ProxyClassBuilder;

namespace DerekTjt.DynamicProxy
{
    public sealed class Proxy
    {
        private static string proxyAssemblyName = ConstVar.ProxyAssemblyName;
        private static readonly string moduleName = ConstVar.ModuleName;

        private static ModuleBuilder module = null;
        private static AssemblyBuilder assembly = null;
        private static Dictionary<string, Type> cache = new Dictionary<string, Type>();

        private static Proxy singletonProxy = new Proxy();
        private static object syncRoot = new object();
        private static ProxyCreateMode defaultCreateMode = ProxyCreateMode.WrapClass;

        /// <summary>
        ///  方法筛选器
        /// </summary>
        public List<IMethodsSearchFilter> Filter = new List<IMethodsSearchFilter>();

        public Proxy()
        {
            Filter.Add(new DefaultMethodsSearchFilter());
        }

        static Proxy()
        {
            lock (syncRoot)
            {
                if (assembly == null)
                {
                    AssemblyName dynamicAss = new AssemblyName(proxyAssemblyName);
                    var ass = AppDomain.CurrentDomain.DefineDynamicAssembly(dynamicAss, AssemblyBuilderAccess.RunAndSave);
                    assembly = ass;
                    module = ass.DefineDynamicModule(moduleName);
                }
            }
        }

        /// <summary>
        /// 将拦截器融合入源类生成代理类
        /// </summary>
        /// <param name="source">源类的某个实例</param>
        /// <param name="h">拦截器</param>
        /// <returns>代理类</returns>
        public dynamic NewInstanceProxy<T>(T source, IInterceptorHandler h)
        {
            return NewInstanceProxy<T>(source, h, defaultCreateMode);
        }

        /// <summary>
        /// 将拦截器融合入源类生成代理类
        /// </summary>
        /// <param name="source">源类的某个实例</param>
        /// <param name="h">拦截器</param>
        /// <param name="createMode">代理类创建模式</param>
        /// <returns>代理类</returns>
        public dynamic NewInstanceProxy<T>(T source, IInterceptorHandler h, ProxyCreateMode createMode)
        {
            var obj = this.NewInstanceProxy(typeof(T), h, createMode);
            return obj;
        }

        /// <summary>
        /// 将拦截器融合入源类生成代理类
        /// </summary>
        /// <param name="source">源类的类型</param>
        /// <param name="h">拦截器</param>
        /// <returns>代理类</returns>
        public dynamic NewInstanceProxy(Type source, IInterceptorHandler h)
        {
            return NewInstanceProxy(source, h, defaultCreateMode);
        }

        /// <summary>
        /// 将拦截器融合入源类生成代理类
        /// </summary>
        /// <param name="source">源类的类型</param>
        /// <param name="h">拦截器</param>
        /// <param name="createMode">代理类创建模式</param>
        /// <returns>代理类</returns>
        public dynamic NewInstanceProxy(Type source, IInterceptorHandler h, ProxyCreateMode createMode)
        {
            Type dynamicProxyClassType = null;
            string proxyClassName = source.Name + ConstVar.ProxyClassSuffix;

            try
            {
                lock (cache)
                {
                    if (cache.ContainsKey(proxyClassName))
                    {
                        dynamicProxyClassType = cache[proxyClassName];
                    }
                    else
                    {
                        dynamicProxyClassType = DefineClass(proxyClassName, source, createMode);
                        cache.Add(proxyClassName, dynamicProxyClassType);
                        assembly.Save(moduleName);
                    }
                }
            }
            catch (Exception ex)
            {
                var exception = new CanNotCreateException(source.Name + "代理类", ex);
                throw exception;
            }

            var constructor = dynamicProxyClassType.GetConstructor(new[] { typeof(IInterceptorHandler) });
            var proxyObj = constructor.Invoke(new object[] { h });
            return proxyObj;
        }

        /// <summary>
        /// 单例模式
        /// </summary>
        /// <returns>代理类生成器</returns>
        public static Proxy InstanceProxyCreator()
        {
            return singletonProxy;
        }

        /// <summary>
        /// 是否是代理类
        /// </summary>
        /// <param name="checkObj">被检查的对象</param>
        /// <returns>布尔值</returns>
        public static bool IsProxyClass(object checkObj)
        {
            var typeName = checkObj.GetType().Name;
            bool isProxy = false;
            lock (cache)
            {
                isProxy = cache.ContainsKey(typeName);
            }

            return isProxy;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="className"></param>
        /// <param name="parentType"></param>
        /// <param name="createMode"></param>
        /// <returns></returns>
        private Type DefineClass(string className, Type parentType, ProxyCreateMode createMode)
        {
            Type proxyType = null;
            IProxyClassBuilder proxyClassBuilder = null;

            #region 根据不同的创建类型选择创建类
            switch (createMode)
            {
                case ProxyCreateMode.WrapInterface:
                    proxyClassBuilder = new WrapInterfaceClassBuilder();
                    break;
                case ProxyCreateMode.WrapClass:
                default:
                    proxyClassBuilder = new WrapCalssBuilder();
                    break;
            }

            #endregion

            if (proxyClassBuilder.Filter == null)
            {
                proxyClassBuilder.Filter = new List<IMethodsSearchFilter>();
            }
            proxyClassBuilder.Filter.AddRange(this.Filter);

            proxyType = proxyClassBuilder.DefineClass(module, className, parentType);
            return proxyType;
        }
    }
}
