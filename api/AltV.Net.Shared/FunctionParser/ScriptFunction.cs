using System;
using System.Reflection;
using System.Linq;
using System.Threading.Tasks;
using AltV.Net.Elements.Entities;
using AltV.Net.Shared.Elements.Entities;

namespace AltV.Net.FunctionParser
{
    public class ScriptFunction
    {
        private static void WrongReturnType(MethodInfo methodInfo, Type expected, Type got)
        {
            Console.WriteLine(
                $"{methodInfo.DeclaringType?.FullName}.{methodInfo.Name}({string.Join(", ", methodInfo.GetParameters().Select(m => $"{m.ParameterType.FullName} {m.Name}"))}): Expected {expected} return type, but got {got}");
        }

        private static void WrongType(MethodInfo methodInfo, Type expected, Type got)
        {
            Console.WriteLine(
                $"{methodInfo.DeclaringType?.FullName}.{methodInfo.Name}({string.Join(", ", methodInfo.GetParameters().Select(m => $"{m.ParameterType.FullName} {m.Name}"))}): Expected {expected} param, but got {got}");
        }

        private static void WrongLength(MethodInfo methodInfo, int expected, int got)
        {
            Console.WriteLine(
                $"{methodInfo.DeclaringType?.FullName}.{methodInfo.Name}({string.Join(", ", methodInfo.GetParameters().Select(m => $"{m.ParameterType.FullName} {m.Name}"))}): Expected {expected} parameters, but got {got}");
        }

        private struct ScriptFunctionParameter
        {
            public readonly bool BaseObjectCheck;

            public readonly Type ParameterType;

            public ScriptFunctionParameter(bool baseObjectCheck, Type parameterType)
            {
                BaseObjectCheck = baseObjectCheck;
                ParameterType = parameterType;
            }
        }

        public static ScriptFunction Create(Delegate @delegate, Type[] types, bool isAsync = false)
        {
            var parameters = @delegate.Method.GetParameters();
            if (parameters.Length != types.Length)
            {
                WrongLength(@delegate.Method, types.Length, parameters.Length);
                return null;
            }

            var scriptFunctionParameters = new ScriptFunctionParameter[types.Length];

            for (int i = 0, length = types.Length; i < length; i++)
            {
                var type = types[i];
                if (typeof(ISharedBaseObject).IsAssignableFrom(type))
                {
                    if (type.IsAssignableFrom(parameters[i].ParameterType))
                    {
                        scriptFunctionParameters[i] = new ScriptFunctionParameter(true, type);
                        continue;
                    }

                    WrongType(@delegate.Method, type, parameters[i].ParameterType);
                    return null;
                }

                if (type == parameters[i].ParameterType)
                {
                    scriptFunctionParameters[i] = new ScriptFunctionParameter(false, type);
                    continue;
                }

                WrongType(@delegate.Method, type, parameters[i].ParameterType);
                return null;
            }

            if (isAsync && !typeof(Task).IsAssignableFrom(@delegate.Method.ReturnType))
            {
                WrongReturnType(@delegate.Method, typeof(Task), @delegate.Method.ReturnType);
            }

            return new ScriptFunction(@delegate, scriptFunctionParameters);
        }

        private readonly object[] args;

        private readonly Delegate @delegate;

        private readonly FunctionParserMethodInfo functionParserMethodInfo;

        private readonly ScriptFunctionParameter[] scriptFunctionParameters;

        private readonly object target;

        private bool valid = true;

        private int currentIndex;

        private ScriptFunction(Delegate @delegate, ScriptFunctionParameter[] scriptFunctionParameters)
        {
            args = new object[scriptFunctionParameters.Length];
            this.@delegate = @delegate;
            this.functionParserMethodInfo = new FunctionParserMethodInfo(@delegate.Method);
            this.scriptFunctionParameters = scriptFunctionParameters;
            this.target = @delegate.Target;
        }

        private ScriptFunction(Delegate @delegate, ScriptFunctionParameter[] scriptFunctionParameters,
            FunctionParserMethodInfo functionParserMethodInfo)
        {
            args = new object[scriptFunctionParameters.Length];
            this.@delegate = @delegate;
            this.functionParserMethodInfo = functionParserMethodInfo;
            this.scriptFunctionParameters = scriptFunctionParameters;
            this.target = @delegate.Target;
        }

        public ScriptFunction Clone()
        {
            return new ScriptFunction(this.@delegate, this.scriptFunctionParameters, this.functionParserMethodInfo);
        }

        public void Set(object value)
        {
            //if (!valid) return;
            var index = currentIndex++;
            /*var scriptFunctionParameter = scriptFunctionParameters[index];
            if (scriptFunctionParameter.BaseObjectCheck)
            {
                if (!value.GetType().IsAssignableFrom(scriptFunctionParameter.ParameterType))
                {
                    valid = false;
                    WrongType(@delegate.Method, scriptFunctionParameter.ParameterType, value.GetType());
                    return;
                }
            }*/

            args[index] = value;
        }

        public object Call()
        {
            valid = true;
            currentIndex = 0;
            if (!valid) return null;
            try
            {
                //@delegate.DynamicInvoke(args);
                return functionParserMethodInfo.Invoke(target, args);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return null;
            }
        }

        public async Task CallAsync()
        {
            valid = true;
            currentIndex = 0;
            if (!valid) return;
            try
            {
                var task = (Task) functionParserMethodInfo.Invoke(target, args);
                await task.ConfigureAwait(false);
                await task;
                //var resultProperty = task.GetType().GetProperty("Result");
                //return resultProperty.GetValue(task);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
    }
}