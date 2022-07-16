using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelRendererQuery.Includes;
using VoxelRendererQuery.Transpiler.Tokenizer;

namespace VoxelRendererQuery.Transpiler.Processors
{
    internal interface IProcessor
    {
        IEnumerator<NHLSLToken> TokenStream { get; set; }
        List<IComponent> Components { get; set; }
        void Run();

    }
}
