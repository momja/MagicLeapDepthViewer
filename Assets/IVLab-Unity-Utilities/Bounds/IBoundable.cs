using System.Collections.Generic;
using UnityEngine;

namespace IVLab.Utilities
{
    public interface IBoundable
    {
        Bounds Bounds { get; }
    }

    public static class Boundable
    {
        public static Bounds EncapsulateAll(IEnumerable<IBoundable> boundables)
        {
            Bounds bounds = default;
            bool firstTime = true;

            foreach (var renderstrategy in boundables)
            {
                if (renderstrategy.Bounds.size == Vector3.zero)
                    continue;

                if (firstTime == true)
                {
                    bounds = renderstrategy.Bounds;
                    firstTime = false;
                }
                else
                {
                    bounds.Encapsulate(renderstrategy.Bounds);
                }
            }
            return bounds;
        }
    }
}
