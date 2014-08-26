using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DerekTjt.DynamicProxy.ProxyException
{
    public class CanNotCreateException : Exception
    {
        public CanNotCreateException()
            : base("创建失败！")
        {
        }

        public CanNotCreateException(string objName)
            : base(string.Format("创建{0}失败！", objName))
        {
        }

        public CanNotCreateException(string objName, Exception innerException)
            : base(string.Format("创建{0}失败！", objName), innerException)
        {
        }
    }
}
