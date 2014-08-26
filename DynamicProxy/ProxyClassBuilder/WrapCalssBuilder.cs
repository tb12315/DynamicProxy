using DerekTjt.DynamicProxy;
using DerekTjt.DynamicProxy.Filter;
using DerekTjt.DynamicProxyCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace DerekTjt.DynamicProxy.ProxyClassBuilder
{
    public class WrapCalssBuilder:IProxyClassBuilder
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
            TypeBuilder cb = null;
            lock (mb)
            {
                cb = mb.DefineType(className, TypeAttributes.Public, sourceType);
            }

            var ihfieldBuiler = EmitILUtil.DefineInterceptorHandlerVar(cb, sourceType);

            var ctorBuilder = EmitILUtil.DefineConstructorForProxyClass(cb, sourceType, ihfieldBuiler);

            var pProxyUsemb = EmitILUtil.DefineGetMethodInfoMethod(cb, sourceType);

            #region 包装源类的方法
            var virMethods = new HashSet<MethodInfo>();
            Filter.ForEach(c =>
            {
                c.Filter(sourceType).ToList().ForEach(m => virMethods.Add(m));
            });

            foreach (var method in virMethods)
            {
                var paras = method.GetParameters().Select(c => c.ParameterType).ToArray();
                var wrapmb = cb.DefineMethod(method.Name, MethodAttributes.Public | MethodAttributes.Virtual,
                     CallingConventions.Standard, method.ReturnType, paras);

                var wrapIL = wrapmb.GetILGenerator();
                var localMethodPara = wrapIL.DeclareLocal(typeof(MethodInfo));//method
                var localInputParas = wrapIL.DeclareLocal(typeof(object[]));//paras
                Label returnLabel = wrapIL.DefineLabel();
                //执行GetMethod方法，赋给method参数
                wrapIL.Emit(OpCodes.Ldarg_0);
                wrapIL.Emit(OpCodes.Ldstr, method.Name);
                wrapIL.Emit(OpCodes.Callvirt, pProxyUsemb);
                wrapIL.Emit(OpCodes.Stloc, localMethodPara);

                wrapIL.Emit(OpCodes.Ldc_I4, paras.Length);
                wrapIL.Emit(OpCodes.Newarr, typeof(object));
                wrapIL.Emit(OpCodes.Stloc, localInputParas);
                if (paras.Length > 0)
                    wrapIL.Emit(OpCodes.Ldloc, localInputParas);

                for (int i = 0; i < paras.Length; i++)
                {
                    var inputPara = paras[0];
                    wrapIL.Emit(OpCodes.Ldc_I4, i);
                    wrapIL.Emit(OpCodes.Ldarg, i + 1);
                    wrapIL.Emit(OpCodes.Box, paras[0]);
                    wrapIL.Emit(OpCodes.Stelem_Ref);
                    wrapIL.Emit(OpCodes.Ldloc, localInputParas);
                }

                wrapIL.Emit(OpCodes.Ldarg_0);
                wrapIL.Emit(OpCodes.Ldfld, ihfieldBuiler);
                wrapIL.Emit(OpCodes.Ldarg_0);
                wrapIL.Emit(OpCodes.Ldloc, localMethodPara);
                wrapIL.Emit(OpCodes.Ldloc, localInputParas);
                wrapIL.Emit(OpCodes.Callvirt, typeof(IInterceptorHandler).GetMethod("Invoke"));

                if (method.ReturnType.Equals(typeof(void)))
                {
                    wrapIL.Emit(OpCodes.Pop);
                }
                else
                {
                    var localReturn = wrapIL.DeclareLocal(method.ReturnType);//返回值
                    wrapIL.Emit(OpCodes.Unbox_Any, method.ReturnType);
                    wrapIL.Emit(OpCodes.Stloc, localReturn);
                    wrapIL.Emit(OpCodes.Br_S, returnLabel);
                    wrapIL.MarkLabel(returnLabel);
                    wrapIL.Emit(OpCodes.Ldloc, localReturn);
                }

                wrapIL.Emit(OpCodes.Ret);
            }
            #endregion

            var proxyType = cb.CreateType();
            return proxyType;
        }
    }
}
