using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.ClonerExample
{
    [ExecuteAlways]
    public class Repeater : MonoBehaviour, IPipelineComponent
    {
        public int Count => count;
        public int count;
        public object Input;
        public List<object> List = new();

        public void Recompute()
        {
            List = Enumerable.Repeat(Input, Count).ToList();
        }

        public void SetInput(object input)
        {
            Input = input;
            List = null;
        }

        public object GetOutput()
        {
            if (List == null || List.Count != count)
                Recompute();
            return List;
        }

        public Type GetInputType()
            => typeof(object);

        public Type GetOutputType()
            => typeof(List<>);
    }
}