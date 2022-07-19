using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelRendererQuery.Includes;
using VoxelRendererQuery.Includes.InternalStructs;
using VoxelRendererQuery.Transpiler.Meta;
using VoxelRendererQuery.Transpiler.Processors.Helper;
using VoxelRendererQuery.Transpiler.Processors.OOP;
using VoxelRendererQuery.Transpiler.Tokenizer;
using static VoxelRendererQuery.Transpiler.Processors.Helper.CallHelper;

namespace VoxelRendererQuery.Transpiler.Processors.OOP
{
    internal class InstanceProcessor : IComponent, IProcessor
    {
        public IEnumerator<NHLSLToken> TokenStream { get; set; }
        public List<IComponent> Components { get; set; }
        public Field InstanceVariable { get; private set; }

        public IMethodContainer InstanceType { get; private set; }

        private IMethodContainer _caller;

        public InstanceProcessor(IMethodContainer caller, IEnumerator<NHLSLToken> tokenStream)
        {
            this._caller = caller;
            this.TokenStream = tokenStream;
            this.Components = new List<IComponent>();
        }

        public void Run()
        {
            this.TokenStream.MoveNext();

            NHLSLToken _typeName = this.TokenStream.Current;
            InstanceType = OOPHandler.Default().Classes[_typeName.Raw];
            OOPHandler.Default().CreatePseudoHeap((OOPClassProcessor)InstanceType);
            this.TokenStream.MoveNext();

            NHLSLToken _variableName = this.TokenStream.Current;
            this.TokenStream.MoveNext();

            Field _field = new Field()
            {
                IsPointer = true,
                Name = _variableName,
                Type = _typeName
            };

            InstanceVariable = _field;

            var _argumentStream = CallHelper.Default().GetAssignmentSpecs(_caller, this.TokenStream, out OOPClassProcessor _instanceType, out AssignmentType _assignmentType);
            switch (_assignmentType)
            {
                case AssignmentType.INSTANCIATION:
                    _instanceType.ProcessArgumentStream(_argumentStream);
                    break;


                case AssignmentType.ERROR:
                    throw new Exception("Error handling current token: token lost");
            }
            



        }

        public string Transpile()
        {
            return "";
        }
    }
}
