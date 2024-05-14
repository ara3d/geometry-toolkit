using System;

namespace Assets.ClonerExample
{
    public interface IPipelineComponent
    {
        Type GetInputType();
        Type GetOutputType();
        object GetOutput();
        void SetInput(object input);
    }
}