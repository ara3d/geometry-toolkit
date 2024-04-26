using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.ClonerExample
{
    public static class UnityExtensions
    {
        // TODO: this could cause problems if the wrong type is previous. 
        public static T GetPreviousComponent<T>(this MonoBehaviour self)
        {
            var comps = self.gameObject.GetComponents<T>();
            var r = default(T);
            foreach (var comp in comps)
            {
                if (comp is MonoBehaviour mb)
                {
                    if (mb == self)
                        return r;
                    if (!mb.enabled)
                        continue;
                    if (mb is T t)
                        r = t;
                }
            }
            return r;
        }
    }
}