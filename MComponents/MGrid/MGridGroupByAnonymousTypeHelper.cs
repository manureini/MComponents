using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace MComponents.MGrid
{
    public static class MGridGroupByAnonymousTypeHelper
    {
        private static List<(IMPropertyInfo[], Type)> mTypeCache = new List<(IMPropertyInfo[], Type)>();

        public static Type GetAnonymousType(IEnumerable<IMPropertyInfo> pProperties)
        {
            var props = pProperties.ToArray();

            var cached = mTypeCache.FirstOrDefault(t => Enumerable.SequenceEqual(t.Item1, props));

            if (cached.Item1 != null)
            {
                return cached.Item2;
            }

            AssemblyName dynamicAssemblyName = new AssemblyName("TempAssembly");
            AssemblyBuilder dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(dynamicAssemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder dynamicModule = dynamicAssembly.DefineDynamicModule("TempAssembly");

            TypeBuilder dynamicAnonymousType = dynamicModule.DefineType("AnonymousType", TypeAttributes.Public);

            foreach (var property in props)
                AddProperty(dynamicAnonymousType, property.Name, property.PropertyType);

            AddEquals(dynamicAnonymousType);
            AddGetHashCode(dynamicAnonymousType);

            var ret = dynamicAnonymousType.CreateType();

            mTypeCache.Add((props, ret));
            return ret;
        }

        private static void AddProperty(TypeBuilder typeBuilder, string propertyName, Type propertyType)
        {
            const MethodAttributes getSetAttr = MethodAttributes.Public | MethodAttributes.HideBySig;

            FieldBuilder field = typeBuilder.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);
            PropertyBuilder property = typeBuilder.DefineProperty(propertyName, PropertyAttributes.None, propertyType,
                new[] { propertyType });

            MethodBuilder getMethodBuilder = typeBuilder.DefineMethod("get_value", getSetAttr, propertyType,
                Type.EmptyTypes);
            ILGenerator getIl = getMethodBuilder.GetILGenerator();
            getIl.Emit(OpCodes.Ldarg_0);
            getIl.Emit(OpCodes.Ldfld, field);
            getIl.Emit(OpCodes.Ret);

            MethodBuilder setMethodBuilder = typeBuilder.DefineMethod("set_value", getSetAttr, null,
                new[] { propertyType });
            ILGenerator setIl = setMethodBuilder.GetILGenerator();
            setIl.Emit(OpCodes.Ldarg_0);
            setIl.Emit(OpCodes.Ldarg_1);
            setIl.Emit(OpCodes.Stfld, field);
            setIl.Emit(OpCodes.Ret);

            property.SetGetMethod(getMethodBuilder);
            property.SetSetMethod(setMethodBuilder);
        }

        private static void AddEquals(TypeBuilder pTypeBuilder)
        {
            var myMethod = pTypeBuilder.DefineMethod("Equals", MethodAttributes.Public | MethodAttributes.Virtual, typeof(bool), new[] { typeof(object) });
            var il = myMethod.GetILGenerator();

            Label lbl_6 = il.DefineLabel();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.EmitCall(OpCodes.Call, typeof(MGridGroupByAnonymousTypeHelper).GetMethod(nameof(AnonymousTypeEquals)), new[] { typeof(object), typeof(object) });

            /*
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Br_S, lbl_6);
            il.MarkLabel(lbl_6);
            il.Emit(OpCodes.Ldloc_0);
            */

            il.Emit(OpCodes.Ret);
        }

        private static void AddGetHashCode(TypeBuilder pTypeBuilder)
        {
            var myMethod = pTypeBuilder.DefineMethod("GetHashCode", MethodAttributes.Public | MethodAttributes.Virtual, typeof(int), null);
            var il = myMethod.GetILGenerator();

            /*
            il.Emit(OpCodes.Ldc_I4_S, 0x2A);
            il.Emit(OpCodes.Ret);
            */

            /*
            Label lbl_5 = il.DefineLabel();

            il.Emit(OpCodes.Ldarg_0);

            il.EmitCall(OpCodes.Call, typeof(MGridGroupByHelper).GetMethod(nameof(AnonymousTypeHashCode)), new[] { typeof(object) });
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Br_S, lbl_5);
            il.MarkLabel(lbl_5);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);
            */


            il.Emit(OpCodes.Ldarg_0);
            il.EmitCall(OpCodes.Call, typeof(MGridGroupByAnonymousTypeHelper).GetMethod(nameof(AnonymousTypeHashCode)), new[] { typeof(object) });
            il.Emit(OpCodes.Ret);

        }

        public static bool AnonymousTypeEquals(object pThis, object pOther)
        {
            if (pThis.GetType() != pOther.GetType())
                return false;

            var properties = pThis.GetType().GetProperties();

            foreach (var property in properties)
            {
                var val1 = property.GetValue(pThis);
                var val2 = property.GetValue(pOther);

                if (!val1.Equals(val2))
                    return false;
            }

            return true;
        }

        public static int AnonymousTypeHashCode(object pThis)
        {
            int hash = 42;

            foreach (var property in pThis.GetType().GetProperties())
            {
                hash = hash * 23 + property.GetValue(pThis).GetHashCode();
            }

            return hash;
        }
    }
}
