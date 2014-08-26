using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DerekTjt.DynamicProxy
{
    public enum ProxyCreateMode
    {
        /// <summary>
        /// 包装类创建代理
        /// 此方式以继承类的方式创建代理类。
        /// 类方法必须virtual
        /// </summary>
        WrapClass = 1,

        /// <summary>
        /// 包装借口创建代理
        /// 此方式以继承类的接口的方式创建代理类。
        /// </summary>
        WrapInterface = 2
    }
}
