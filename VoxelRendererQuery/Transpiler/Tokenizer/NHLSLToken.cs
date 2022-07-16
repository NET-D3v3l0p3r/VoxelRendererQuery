using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelRendererQuery.Includes;
using VoxelRendererQuery.Includes.Errors;
using static VoxelRendererQuery.Transpiler.Tokenizer.NHLSLTokenizer;

namespace VoxelRendererQuery.Transpiler.Tokenizer
{
    internal class NHLSLToken : IComponent
    {

        public int Row { get; set; }
        public int Col { get; set; }

        public Token Identifier { get; set; }

        private string _raw;
        public string Raw
        {
            get
            {
                return _raw;
            }
            set
            {
                _raw = value;
                if (NHLSLTokenizer.INTRINSICS_MAPPER.ContainsKey(this.Identifier))
                    _raw = NHLSLTokenizer.INTRINSICS_MAPPER[this.Identifier];
            }
        }

        public string Transpile()
        {
            return Raw;
        }

        public override string ToString()
        {
            return "[" + Identifier + "]" + Raw;
        }
    }
}
