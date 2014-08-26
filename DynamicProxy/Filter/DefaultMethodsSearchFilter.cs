using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DerekTjt.DynamicProxy.Filter
{
    public class DefaultMethodsSearchFilter : IMethodsSearchFilter
    {
        public IEnumerable<System.Reflection.MethodInfo> Filter(Type sourceType)
        {
            var virMethods = sourceType.GetMethods().Where(c => c.IsVirtual);
            return virMethods;
        }
    }
}
