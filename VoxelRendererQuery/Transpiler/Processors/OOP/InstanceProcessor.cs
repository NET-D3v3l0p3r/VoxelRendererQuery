using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelRendererQuery.Includes;
using VoxelRendererQuery.Includes.InternalStructs;
using VoxelRendererQuery.Transpiler.Processors.Helper;
using VoxelRendererQuery.Transpiler.Processors.OOP;
using VoxelRendererQuery.Transpiler.Tokenizer;

namespace VoxelRendererQuery.Transpiler.Processors.OOP
{
    internal class InstanceProcessor : IComponent, IProcessor
    {
        public IEnumerator<NHLSLToken> TokenStream { get; set; }
        public List<IComponent> Components { get; set; }
        public List<Field> InstanceVariables { get; private set; }

        public InstanceProcessor(IEnumerator<NHLSLToken> tokenStream)
        {
            this.TokenStream = tokenStream;
            this.Components = new List<IComponent>();
            this.InstanceVariables = new List<Field>();
        }

        public void Run()
        {
            this.TokenStream.MoveNext();

            NHLSLToken _typeName = this.TokenStream.Current;
            OOPHandler.Default().CreatePseudoHeap(OOPHandler.Default().Classes[_typeName.Raw]);
            this.TokenStream.MoveNext();

            NHLSLToken _variableName = this.TokenStream.Current;
            this.TokenStream.MoveNext();

            Field _field = new Field()
            {
                IsPointer = true,
                Name = _variableName,
                Type = _typeName
            };

            InstanceVariables.Add(_field);

            var argumentStream = CallHelper.Default().GetArguments(this.TokenStream);
            while (argumentStream.MoveNext())
            {
                var arg = argumentStream.Current;


            }



        }

        public string Transpile()
        {
            return "";
        }
    }
}
