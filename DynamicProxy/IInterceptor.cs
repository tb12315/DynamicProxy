using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DerekTjt.DynamicProxy
{
    public interface IInterceptorHandler
    {
        object Invoke(object proxy, MethodInfo method, object[] args);
    }
}
