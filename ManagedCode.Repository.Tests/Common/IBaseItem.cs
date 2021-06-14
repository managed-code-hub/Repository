using System;
using ManagedCode.Repository.Core;

namespace ManagedCode.Repository.Tests.Common
{
    public interface IBaseItem<TId> : IItem<TId>
    {
        string StringData { get; set; }
        int IntData { get; set; }
        float FloatData { get; set; }
        DateTime DateTimeData { get; set; }
    }
}