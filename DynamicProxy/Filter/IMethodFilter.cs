using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DerekTjt.DynamicProxy.Filter
{
    public interface IMethodsSearchFilter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceType">源类（即代理类的包装对象）</param>
        /// <returns></returns>
        IEnumerable<MethodInfo> Filter(Type sourceType);
    }
}
