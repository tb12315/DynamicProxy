using DerekTjt.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace DerekTjt.DynamicProxyCommon
{
    public static class EmitILUtil
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cb"></param>
        /// <param name="sourceType"></param>
        /// <returns></returns>
        public static MethodBuilder DefineGetMethodInfoMethod(TypeBuilder cb, Type sourceType)
        {
            #region 定义一个私有方法
            var privateMethodName = "GetBaseMethodByName_ProxyClass";
            var oldMethod = sourceType.GetMethod(privateMethodName);
            if (oldMethod != null)
            {
                throw new Exception(string.Format("原始类中已存在名为{0}的方法！无法创建相同的方法", privateMethodName));
            }

            var pProxyUsemb = cb.DefineMethod(privateMethodName, MethodAttributes.Public, CallingConventions.HasThis, typeof(MethodInfo), new[] { typeof(System.String) });
            var pProxyUseIL = pProxyUsemb.GetILGenerator();
            pProxyUseIL.DeclareLocal(typeof(MethodInfo));
            Label targetLabel = pProxyUseIL.DefineLabel();
            pProxyUseIL.Emit(OpCodes.Ldarg_0);
            pProxyUseIL.Emit(OpCodes.Call, typeof(System.Object).GetMethod("GetType"));
            pProxyUseIL.Emit(OpCodes.Callvirt, typeof(System.Type).GetMethod("get_BaseType"));
            pProxyUseIL.Emit(OpCodes.Ldarg_1);
            pProxyUseIL.Emit(OpCodes.Callvirt, typeof(System.Type).GetMethod("GetMethod", new[] { typeof(System.String) }));
            pProxyUseIL.Emit(OpCodes.Stloc_0);
            pProxyUseIL.Emit(OpCodes.Br_S, targetLabel);
            pProxyUseIL.MarkLabel(targetLabel);
            pProxyUseIL.Emit(OpCodes.Ldloc_0);
            pProxyUseIL.Emit(OpCodes.Ret);
            #endregion

            return pProxyUsemb;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cb"></param>
        /// <param name="sourceType"></param>
        /// <returns></returns>
        public static FieldBuilder DefineInterceptorHandlerVar(TypeBuilder cb, Type sourceType)
        {
            var interceptorfieldName = "interceptorHandler";
            var oldfield = sourceType.GetField(interceptorfieldName);
            if (oldfield != null)
            {
                throw new Exception(string.Format("原始类中已存在名为{0}的字段！无法创建相同的字段", interceptorfieldName));
            }

            var ihfieldBuiler = cb.DefineField(interceptorfieldName, typeof(IInterceptorHandler), FieldAttributes.Private);
            ihfieldBuiler.SetConstant(null);

            return ihfieldBuiler;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cb"></param>
        /// <param name="sourceType"></param>
        /// <param name="interceptorVarBuilder"></param>
        /// <returns></returns>
        public static ConstructorBuilder DefineConstructorForProxyClass(TypeBuilder cb, Type sourceType, FieldBuilder interceptorVarBuilder)
        {
            #region 定义构造函数
            var ctorBuilder = cb.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, new Type[] { typeof(IInterceptorHandler) });
            var ctorIL = ctorBuilder.GetILGenerator();
            ctorIL.Emit(OpCodes.Ldarg_0);
            ctorIL.Emit(OpCodes.Ldnull);
            ctorIL.Emit(OpCodes.Stfld, interceptorVarBuilder);
            ctorIL.Emit(OpCodes.Ldarg_0);
            ctorIL.Emit(OpCodes.Call, sourceType.GetConstructor(Type.EmptyTypes));
            ctorIL.Emit(OpCodes.Ldarg_0);
            ctorIL.Emit(OpCodes.Ldarg_1);
            ctorIL.Emit(OpCodes.Stfld, interceptorVarBuilder);
            ctorIL.Emit(OpCodes.Ret);
            #endregion

            return ctorBuilder;
        }
    }
}
