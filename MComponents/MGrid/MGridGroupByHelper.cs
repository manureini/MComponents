using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace MComponents.MGrid
{

    public class TestType
    {
        public bool IsGoodWeather { get; set; }

        public bool IsGoodWeather1 { get; set; }

        public bool IsGoodWeather2 { get; set; }

        public override bool Equals(object obj)
        {
            return MGridGroupByHelper.AnonymousTypeEquals(this, obj);
        }

        public override int GetHashCode()
        {
            //return MGridGroupByHelper.AnonymousTypeHashCode(this);
            return 42;
        }
    }

    public class MGridGroupByHelper
    {
        private static readonly MethodInfo GroupByMethod =
             typeof(Queryable).GetMethods()
             .Where(method => method.Name == "GroupBy")
             .Where(method => method.GetParameters().Length == 2)
             .First();

        private static readonly MethodInfo SelectMethod =
             typeof(Queryable).GetMethods()
             .Where(method => method.Name == "Select")
             .Where(method => method.GetParameters().Length == 2)
             .First();

        private static readonly MethodInfo CountMethod =
             typeof(Enumerable).GetMethods()
             .Where(method => method.Name == "Count")
             .Where(method => method.GetParameters().Length == 1)
             .First();


        public static IQueryable GetGroupKeyCounts<T>(IQueryable<T> pQueryable, IEnumerable<IMPropertyInfo> pProperties)
        {
            pQueryable = pQueryable.ToArray().AsQueryable();

            var anType = CreateAnonymousType(pProperties);

            ParameterExpression parameter = Expression.Parameter(typeof(T), "t");

            List<MemberBinding> bindings = new List<MemberBinding>();

            foreach (var property in pProperties)
            {
                var field = anType.GetProperty(property.Name);
                var exprBind = Expression.Bind(field, property.GetMemberExpression(parameter));

                bindings.Add(exprBind);
            }

            Expression selector = Expression.Lambda(Expression.MemberInit(Expression.New(anType.GetConstructor(Type.EmptyTypes)), bindings), parameter);

            var miGroupBy = GroupByMethod.MakeGenericMethod(new[] { typeof(T), anType });
            var groupedQueryable = (IEnumerable)miGroupBy.Invoke(null, new object[] { pQueryable, selector });

            var igroupType = groupedQueryable.GetType().GenericTypeArguments.First();

            ParameterExpression parameterGrouped = Expression.Parameter(igroupType, "t");
            var keyprop = Expression.PropertyOrField(parameterGrouped, "Key");

            var miCount = CountMethod.MakeGenericMethod(new[] { typeof(T) });
            var countprop = Expression.Call(miCount, new[] { parameterGrouped });


            var tupleGenericType = Type.GetType($"System.Tuple`2");

            var keyPropertyTypes = new[] { anType, typeof(int) };
            var tupleType = tupleGenericType.MakeGenericType(keyPropertyTypes);
            var tupleConstructor = tupleType.GetConstructor(keyPropertyTypes);

            var newTupleExpression = Expression.New(tupleConstructor, new Expression[] { keyprop, countprop });

            var lambda = Expression.Lambda(newTupleExpression, new[] { parameterGrouped });

            var miSelect = SelectMethod.MakeGenericMethod(new[] { igroupType, tupleType });
            var groupSelectedQueryable = miSelect.Invoke(null, new object[] { groupedQueryable, lambda });

            return (IQueryable)groupSelectedQueryable;
        }






        public static Type CreateAnonymousType(IEnumerable<IMPropertyInfo> pProperties)
        {
            AssemblyName dynamicAssemblyName = new AssemblyName("TempAssembly");
            AssemblyBuilder dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(dynamicAssemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder dynamicModule = dynamicAssembly.DefineDynamicModule("TempAssembly");

            TypeBuilder dynamicAnonymousType = dynamicModule.DefineType("AnonymousType", TypeAttributes.Public);

            foreach (var property in pProperties)
                AddProperty(dynamicAnonymousType, property.Name, property.PropertyType);

            AddEquals(dynamicAnonymousType);
            AddGetHashCode(dynamicAnonymousType);

            return dynamicAnonymousType.CreateType();
        }

        public static void AddProperty(TypeBuilder typeBuilder, string propertyName, Type propertyType)
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

        public static void AddEquals(TypeBuilder pTypeBuilder)
        {
            var myMethod = pTypeBuilder.DefineMethod("Equals", MethodAttributes.Public | MethodAttributes.Virtual, typeof(bool), new[] { typeof(object) });
            var il = myMethod.GetILGenerator();

            Label lbl_6 = il.DefineLabel();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.EmitCall(OpCodes.Call, typeof(MGridGroupByHelper).GetMethod(nameof(AnonymousTypeEquals)), new[] { typeof(object), typeof(object) });

            /*
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Br_S, lbl_6);
            il.MarkLabel(lbl_6);
            il.Emit(OpCodes.Ldloc_0);
            */

            il.Emit(OpCodes.Ret);
        }

        public static void AddGetHashCode(TypeBuilder pTypeBuilder)
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
            il.EmitCall(OpCodes.Call, typeof(MGridGroupByHelper).GetMethod(nameof(AnonymousTypeHashCode)), new[] { typeof(object) });
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

        public static IEnumerable<(object, int)> GetKeys(IQueryable pKeyCounts, int pSkip, int pTake, List<object[]> hiddenGroupByKeys)
        {
            int currentIndex = 0;

            List<(object, int)> keys = new List<(object, int)>();

            int rowsMissing = pTake;

            foreach (dynamic entry in pKeyCounts)
            {
                var dynamicKeyType = entry.Item1;

                if (rowsMissing <= 0)
                    break;

                int countInGroupPart = entry.Item2;
                currentIndex += countInGroupPart;

                var properties = (IEnumerable<PropertyInfo>)dynamicKeyType.GetType().GetProperties();
                var values = properties.Select(p => p.GetValue(dynamicKeyType)).ToArray();

                if (hiddenGroupByKeys.Any(k => k.All(n => values.Contains(n))))
                {
                    pSkip += countInGroupPart;
                    continue;
                }

                if (currentIndex >= pSkip)
                {
                    var offset = pSkip - (currentIndex - countInGroupPart);
                    keys.Add((dynamicKeyType, offset));

                    int entryCount = countInGroupPart - offset;

                    rowsMissing = pTake - entryCount;
                }
            }

            return keys;
        }
    }
}
