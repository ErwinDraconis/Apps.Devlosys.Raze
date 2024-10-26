using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Apps.Devlosys.Core
{
    public static class GenericInterfaceExtensions
    {
        public static IGenericInterface AsGenericInterface(this object @this, Type type)
        {
            Type interfaceType = (
                    from @interface in @this.GetType().GetInterfaces()
                    where @interface.IsGenericType
                    let definition = @interface.GetGenericTypeDefinition()
                    where definition == type
                    select @interface
                )
                .SingleOrDefault();

            return interfaceType != null
                ? new GenericInterfaceImpl(@this, interfaceType)
                : null;
        }

        private class GenericInterfaceImpl : IGenericInterface
        {
            private static readonly Regex ActionDelegateRegex = new Regex(@"^System\.Action(`\d{1,2})?", RegexOptions.Compiled);
            private static readonly Regex FuncDelegateRegex = new Regex(@"^System\.Func`(\d{1,2})", RegexOptions.Compiled);

            private readonly object _instance;

            public Type Type { get; }

            public Type[] GenericArguments => Type.GetGenericArguments();

            public GenericInterfaceImpl(object instance, Type interfaceType)
            {
                _instance = instance;
                Type = interfaceType;
            }

            public TDelegate GetMethod<TDelegate>(string methodName, params Type[] argTypes)
            {
                return GetDelegateType<TDelegate>() switch
                {
                    DelegateType.Action => GetAction<TDelegate>(methodName),
                    DelegateType.ActionWithParams => GetActionWithParams<TDelegate>(methodName, argTypes),
                    DelegateType.NotSupported => throw new NotImplementedException(),
                    DelegateType.Func => throw new NotImplementedException(),
                    DelegateType.FuncWithParams => throw new NotImplementedException(),
                    _ => throw new NotSupportedException(),
                };
            }

            private TDelegate GetActionWithParams<TDelegate>(string methodName, params Type[] argTypes)
            {
                System.Reflection.MethodInfo methodInfo = Type.GetMethod(methodName) ?? throw new ArgumentException(nameof(methodName));

                Type[] argTypeList = argTypes.Any() ? argTypes : typeof(TDelegate).GetGenericArguments();
                (ParameterExpression expression, Type type)[] argObjectParameters = argTypeList
                    .Select(item => (Expression.Parameter(typeof(object)), item))
                    .ToArray();

                TDelegate method = Expression.Lambda<TDelegate>(
                        Expression.Call(
                            Expression.Constant(_instance),
                            methodInfo,
                            argObjectParameters.Select(item => Expression.Convert(item.expression, item.type))),
                        argObjectParameters.Select(item => item.expression))
                    .Compile();

                return method;
            }

            private TDelegate GetAction<TDelegate>(string methodName)
            {
                System.Reflection.MethodInfo methodInfo = Type.GetMethod(methodName) ?? throw new ArgumentException(nameof(methodName));

                TDelegate method = Expression.Lambda<TDelegate>(
                        Expression.Call(
                            Expression.Constant(_instance),
                            methodInfo))
                    .Compile();

                return method;
            }

            private static DelegateType GetDelegateType<TDelegate>()
            {
                Match actionMatch = ActionDelegateRegex.Match(typeof(TDelegate).FullName ?? throw new InvalidOperationException());
                if (actionMatch.Success)
                {
                    return actionMatch.Groups.Count > 1 ? DelegateType.ActionWithParams : DelegateType.Action;
                }

                Match funcMatch = FuncDelegateRegex.Match(typeof(TDelegate).FullName ?? throw new InvalidOperationException());
                return funcMatch.Success
                    ? int.Parse(actionMatch.Groups[1].Value) > 1 ? DelegateType.FuncWithParams : DelegateType.Func
                    : DelegateType.NotSupported;
            }

            private enum DelegateType
            {
                NotSupported,
                Action,
                Func,
                ActionWithParams,
                FuncWithParams
            }
        }
    }
}
