using DerekTjt.DynamicProxy.Filter;
using DerekTjt.DynamicProxy.ProxyClassBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace DerekTjt.DynamicProxy.ProxyClassBuilder
{
    public class WrapInterfaceClassBuilder : IProxyClassBuilder
    {
        private List<IMethodsSearchFilter> filter = new List<IMethodsSearchFilter>();

        /// <summary>
        ///  方法筛选器
        /// </summary>
        public List<IMethodsSearchFilter> Filter
        {
            get
            {
                return filter;
            }
            set
            {
                filter = value;
            }
        }

        public Type DefineClass(ModuleBuilder mb, string className, Type sourceType)
        {
            throw new Exception("暂未实现");
        }
    }
}
