using System;
using System.Linq.Expressions;

namespace Assets.ClonerExample
{
    public interface IFilter
    {
        Type Input { get; }
        Type Output { get; }
        object Eval(object input);
    }
}