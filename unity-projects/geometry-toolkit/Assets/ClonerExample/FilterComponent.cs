using System;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

namespace Assets.ClonerExample
{
    [Serializable]
    public class FallOffParameters
    {
        // TODO: 
        // Distance from point
        // Distance from plane 
        // Distance from lane
    }

    [ExecuteAlways]
    public abstract class BaseFilterComponent : MonoBehaviour, IFilter
    {
        public float OverallStrength = 1f;
        public bool InterpolateArray = true;
        public Type Input { get; }
        public Type Output { get; }

        public BaseFilterComponent(Type input, Type output)
        {
            Input = input;
            Output = output;
        }

        public void Update()
        { }

        public abstract object Eval(object input);
    }

    public abstract class FilterComponent<TInput, TOutput> : BaseFilterComponent
    {
        protected FilterComponent()
            : base(typeof(TInput), typeof(TOutput))
        { }

        public abstract TOutput EvalImpl(TInput input);

        public override object Eval(object input) 
            => EvalImpl((TInput)input);
    }
}