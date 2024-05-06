using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;
using static Unity.VisualScripting.Member;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

namespace Assets.ClonerExample
{
    public interface ICloneJob
    {
        JobHandle Schedule(ICloneJob previous);
        JobHandle Handle { get; }
        ref CloneData CloneData { get; }
        int Count { get; }
    }

    [ExecuteAlways]
    public class ClonerRenderer : MonoBehaviour
    {
        public Mesh mesh; 
        public Material material;
        private Material _material;
        public CloneRenderData _cloneRenderData;
        public ShadowCastingMode shadowCasting = ShadowCastingMode.Off;
        public bool receiveShadows = false;
        public bool Experimental;
        public CloneData _cloneData;
        public int Count;
        public JobHandle _handle;
        public int BatchSize = 256;

        public void OnEnable()
        {
            _cloneRenderData = new CloneRenderData();
        }

        public void OnValidate()
        {
            _material = null;
        }

        public void OnDisable()
        {
            _cloneRenderData.Dispose();
            _cloneRenderData = null;
        }

        public JobHandle ScheduleJobs()
        {
            var jobComponents = gameObject.GetComponents<ClonerJobComponent>().Where(jc => jc.enabled).ToList();

            if (Experimental)
            {
                _handle = default;
                ICloneJob prev = null;
                foreach (var jc in jobComponents)
                {
                    var cj = jc as ICloneJob;
                    if (cj == null) throw new NotImplementedException();
                    _handle = cj.Schedule(prev);
                    Count = cj.Count;
                    prev = cj;
                }
                JobHandle.ScheduleBatchedJobs();
                _cloneData = prev.CloneData;
                return _handle;
            }   
            else
            {
                _handle = default;
                foreach (var jc in jobComponents)
                {
                    (_cloneData, _handle) = jc.Schedule(_cloneData, _handle, BatchSize);
                }

                JobHandle.ScheduleBatchedJobs();
                return _handle;
            }
        }

        public void Update()
        {
            if (_material == null)
                _material = new Material(material);
            
            if (_cloneRenderData == null)
                return;

            _handle.Complete();
            _cloneRenderData.UpdateGpuData(mesh, _cloneData.GpuArray.GetSubArray(0, Count), _material);
            _cloneRenderData.Render(shadowCasting, receiveShadows);
        }

        public void LateUpdate()
        {
            ScheduleJobs();
        }
    }
}