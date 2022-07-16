using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelRendererQuery.Transpiler.Tokenizer;

namespace VoxelRendererQuery.Includes.InternalStructs
{
    internal struct MethodParam
    {
        public bool IsPointer;
        public List<NHLSLToken> Type;
        public NHLSLToken Name;
    }

}
