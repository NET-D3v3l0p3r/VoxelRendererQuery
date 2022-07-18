using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelRendererQuery.Transpiler.Processors;

namespace VoxelRendererQuery.Transpiler.Meta
{
    internal interface IMethodContainer
    {
        List<MethodProcessor> Methods { get; set; }
    }
}
