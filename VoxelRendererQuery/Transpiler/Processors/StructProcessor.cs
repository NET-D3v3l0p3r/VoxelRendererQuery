using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelRendererQuery.Includes;
using VoxelRendererQuery.Includes.Errors;
using VoxelRendererQuery.Transpiler.Tokenizer;

namespace VoxelRendererQuery.Transpiler.Processors
{
    internal class StructProcessor : IComponent, IProcessor
    {
        internal struct Slot
        {
            public string Name;
            public string MappedName;
        }

        private static Dictionary<string, string> _SLOTS = new Dictionary<string, string>()
        {
            { "DEPTH", "depth" },
            { "HITF32", "hitPoint" },
            { "RESULT", "result" },
            { "HITI32", "hitPointInt" },
            { "NORMAL", "normal" },
            { "VOXEL", "getFromInt32(voxelData)" },
            { "DBG_ITERATIONS", "iterations" }
        };

        public string Name { get; set; }

        public List<IComponent> Components { get; set; }
        public IEnumerator<NHLSLToken> TokenStream { get; set; }

        private readonly List<Slot> _slotAssignment = new List<Slot>();

        public StructProcessor(IEnumerator<NHLSLToken> tokenStream)
        {
            this.TokenStream = tokenStream;
            this.TokenStream.MoveNext();

            this.Components = new List<IComponent>();
        }

        public void Run()
        {
            this.Name = TokenStream.Current.Raw;
            TokenStream.MoveNext();

            while (TokenStream.MoveNext())
            {
                NHLSLToken token = TokenStream.Current;

                if (token.Identifier == NHLSLTokenizer.Token.FORBIDDEN)
                    throw new IllegalOverrideException(
                         "The source contains an illegal symbol at line " + token.Col + " and row " + token.Row + ": " + token.Raw);

                if (token.Identifier == NHLSLTokenizer.Token.BRACKET_C)
                    break;

                if (token.Identifier == NHLSLTokenizer.Token.COLON)
                {
                    var variableName = ((NHLSLToken)Components[Components.Count - 1]).Raw;
                    TokenStream.MoveNext();
                    var slot = TokenStream.Current.Raw;
                    _slotAssignment.Add(new Slot() { Name = variableName, MappedName = _SLOTS[slot] });
                }
                else Components.Add(token);

            }
        }

        public string Transpile()
        {
            StringBuilder srcBuilder = new StringBuilder();

            srcBuilder.AppendLine("struct " + Name + " {");
            foreach (var component in Components)
            {
                if (component is NHLSLToken)
                {
                    NHLSLToken token = (NHLSLToken)component;
                    srcBuilder.Append(token.Raw + " ");
                    if (token.Identifier == NHLSLTokenizer.Token.SEMICOLON)
                        srcBuilder.AppendLine();
                }
            }

            Components.Clear();

            srcBuilder.Append("}");

            return srcBuilder.ToString();
        }

        public string GetAssignments()
        {
            StringBuilder srcBuilder = new StringBuilder();

            srcBuilder.AppendLine(Name + " __temprslt = (" + Name + ")0;");
            foreach (var slot in _slotAssignment)
                srcBuilder.AppendLine("__temprslt." + slot.Name + " = " + slot.MappedName + ";");
           
            srcBuilder.AppendLine("return __temprslt;");

            return srcBuilder.ToString();
        }

       
    }
}
