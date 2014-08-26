using DerekTjt.DynamicProxy.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace DerekTjt.DynamicProxy.ProxyClassBuilder
{
    public interface IProxyClassBuilder
    {
        /// <summary>
        ///  方法筛选器
        /// </summary>
        List<IMethodsSearchFilter> Filter { get; set; }

        Type DefineClass(ModuleBuilder mb, string className, Type sourceType);
    }
}
