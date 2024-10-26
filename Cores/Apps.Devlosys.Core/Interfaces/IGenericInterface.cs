using System;

namespace Apps.Devlosys.Core
{
    public interface IGenericInterface
    {
        Type Type { get; }

        Type[] GenericArguments { get; }

        TDelegate GetMethod<TDelegate>(string methodName, params Type[] argTypes);
    }
}
