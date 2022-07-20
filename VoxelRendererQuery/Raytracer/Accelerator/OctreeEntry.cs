using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoxelRendererQuery.Raytracer.Accelerator
{
    internal struct OctreeEntry
    {
        public int childrenStartIndex;
        public int childrenCount;
    }
}
