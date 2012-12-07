using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace GilesTrinity.ItemRules.Core
{
    internal static class ExpressionCompiler
    {
        private const MethodAttributes MethodAttribut = MethodAttributes.Public | MethodAttributes.Static;

        
        public static void CompileExpression(IList<ParseNode> expressionTrees)
        {
            AssemblyName assemblyName = new AssemblyName("ScriptedRule");
            AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(
                                                assemblyName,
                                                AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name, false);

            TypeBuilder typeBuilder = moduleBuilder.DefineType(string.Format("ScriptedRule_{0:N}", Guid.NewGuid()), TypeAttributes.Public);
            GenerateBaseOperationFloatMethod(typeBuilder);
            GenerateBaseOperationIntMethod(typeBuilder);
            GenerateBaseOperationStringMethod(typeBuilder);

            int counter = 0;
            foreach (ParseNode tree in expressionTrees)
            {
                GenerateMethodForNode(tree, string.Format("Rule{0}",counter));
                counter++;
            }
        }

        private static void GenerateMethodForNode(ParseNode tree, string methodName)
        {
            throw new NotImplementedException();
        }

        private static void GenerateBaseOperationFloatMethod(TypeBuilder typeBuilder)
        {
            MethodBuilder methodBuilder = typeBuilder.DefineMethod("Add", MethodAttribut, typeof(float), new Type[] { typeof(float), typeof(float) });
            ILGenerator methIL = methodBuilder.GetILGenerator();
            methIL.Emit(OpCodes.Ldarg_0);
            methIL.Emit(OpCodes.Ldarg_1);
            methIL.Emit(OpCodes.Add);
            methIL.Emit(OpCodes.Ret);
            methodBuilder = typeBuilder.DefineMethod("Substract", MethodAttribut, typeof(float), new Type[] { typeof(float), typeof(float) });
            methIL = methodBuilder.GetILGenerator();
            methIL.Emit(OpCodes.Ldarg_0);
            methIL.Emit(OpCodes.Ldarg_1);
            methIL.Emit(OpCodes.Sub);
            methIL.Emit(OpCodes.Ret);
            methodBuilder = typeBuilder.DefineMethod("Multiply", MethodAttribut, typeof(float), new Type[] { typeof(float), typeof(float) });
            methIL = methodBuilder.GetILGenerator();
            methIL.Emit(OpCodes.Ldarg_0);
            methIL.Emit(OpCodes.Ldarg_1);
            methIL.Emit(OpCodes.Mul);
            methIL.Emit(OpCodes.Ret);
            methodBuilder = typeBuilder.DefineMethod("Divide", MethodAttribut, typeof(float), new Type[] { typeof(float), typeof(float) });
            methIL = methodBuilder.GetILGenerator();
            methIL.Emit(OpCodes.Ldarg_0);
            methIL.Emit(OpCodes.Ldarg_1);
            methIL.Emit(OpCodes.Div);
            methIL.Emit(OpCodes.Ret);

        }

        private static void GenerateBaseOperationIntMethod(TypeBuilder typeBuilder)
        {
            MethodBuilder methodBuilder = typeBuilder.DefineMethod("Add", MethodAttribut, typeof(int), new Type[] { typeof(int), typeof(int) });
            ILGenerator methIL = methodBuilder.GetILGenerator();
            methIL.Emit(OpCodes.Ldarg_0);
            methIL.Emit(OpCodes.Ldarg_1);
            methIL.Emit(OpCodes.Add);
            methIL.Emit(OpCodes.Ret);
            methodBuilder = typeBuilder.DefineMethod("Substract", MethodAttribut, typeof(int), new Type[] { typeof(int), typeof(int) });
            methIL = methodBuilder.GetILGenerator();
            methIL.Emit(OpCodes.Ldarg_0);
            methIL.Emit(OpCodes.Ldarg_1);
            methIL.Emit(OpCodes.Sub);
            methIL.Emit(OpCodes.Ret);
            methodBuilder = typeBuilder.DefineMethod("Multiply", MethodAttribut, typeof(int), new Type[] { typeof(int), typeof(int) });
            methIL = methodBuilder.GetILGenerator();
            methIL.Emit(OpCodes.Ldarg_0);
            methIL.Emit(OpCodes.Ldarg_1);
            methIL.Emit(OpCodes.Mul);
            methIL.Emit(OpCodes.Ret);
            methodBuilder = typeBuilder.DefineMethod("Divide", MethodAttribut, typeof(int), new Type[] { typeof(int), typeof(int) });
            methIL = methodBuilder.GetILGenerator();
            methIL.Emit(OpCodes.Ldarg_0);
            methIL.Emit(OpCodes.Ldarg_1);
            methIL.Emit(OpCodes.Div);
            methIL.Emit(OpCodes.Ret);
        }

        private static void GenerateBaseOperationStringMethod(TypeBuilder typeBuilder)
        {
            MethodBuilder methodBuilder = typeBuilder.DefineMethod("Add", MethodAttribut, typeof(string), new Type[] { typeof(string), typeof(string) });
            ILGenerator methIL = methodBuilder.GetILGenerator();
            methIL.Emit(OpCodes.Ldarg_0);
            methIL.Emit(OpCodes.Ldarg_1);
            methIL.Emit(OpCodes.Add);
            methIL.Emit(OpCodes.Ret);
        }
    }
}
