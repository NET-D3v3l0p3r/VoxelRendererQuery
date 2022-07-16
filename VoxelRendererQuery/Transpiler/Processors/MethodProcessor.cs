using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelRendererQuery.Includes;
using VoxelRendererQuery.Includes.Errors;
using VoxelRendererQuery.Includes.InternalStructs;
using VoxelRendererQuery.Transpiler.Processors.Helper;
using VoxelRendererQuery.Transpiler.Processors.OOP;
using VoxelRendererQuery.Transpiler.Tokenizer;

namespace VoxelRendererQuery.Transpiler.Processors
{
    internal class MethodProcessor : IComponent, IProcessor
    {
        public IProcessor Caller { get; private set; }
        public string Name { get; set; }
        public List<NHLSLToken> Type { get; set; }

        public bool IsPointerType { get; set; }
        public bool IsConstructor { get; set; }

        public IEnumerator<NHLSLToken> TokenStream { get; set; }
        public List<IComponent> Components { get; set; }
        public List<Intrinsics> IntrinsicCalls { get; }


        private IntrinsicsHelper _intrinsicsHelper;


        // PROCESSORS

        private ParametersProcessor _parametersProcessor;
        private List<InstanceProcessor> _instanceProcessors = new List<InstanceProcessor>();

        public MethodProcessor(IProcessor caller, IEnumerator<NHLSLToken> tokenStream)
        {
            this.Caller = caller;
            this.TokenStream = tokenStream;
            this.TokenStream.MoveNext();

            this.Type = new List<NHLSLToken>();
            this.Components = new List<IComponent>();
            this.IntrinsicCalls = new List<Intrinsics>();

            

            _intrinsicsHelper = IntrinsicsHelper.New(this);
        }

        public void Run()
        {
            _parametersProcessor = new ParametersProcessor(this.TokenStream);
            _parametersProcessor.Run();

            this.Components.Add(_parametersProcessor);

            int _bracketCounter = 0;

            while (TokenStream.MoveNext())
            {
                NHLSLToken token = TokenStream.Current;

                if (token.Identifier == NHLSLTokenizer.Token.FORBIDDEN)
                    throw new IllegalOverrideException(
                        "The source contains an illegal symbol at line " + token.Col + " and row " + token.Row + ": " + token.Raw);

                if (token.Identifier == NHLSLTokenizer.Token.BRACKET_O)
                    _bracketCounter++;
                if (token.Identifier == NHLSLTokenizer.Token.BRACKET_C)
                    _bracketCounter--;
             
                if (_intrinsicsHelper.Actions.ContainsKey(token.Identifier))
                {
                    _intrinsicsHelper.Actions[token.Identifier]();
                    continue;
                }

                if(token.Identifier == NHLSLTokenizer.Token.OOP_POINTER)
                {
                    InstanceProcessor _instanceProcessor = new InstanceProcessor(this.TokenStream);
                    _instanceProcessor.Run();

                    this.Components.Add(_instanceProcessor);
                    _instanceProcessors.Add(_instanceProcessor);

                    continue;
                }

                Components.Add(token);
                if (_bracketCounter == 0)
                    break;
            }
        }

        public string Transpile()
        {
            StringBuilder srcBuilder = new StringBuilder();

            if (!IsPointerType)
                foreach (var type in Type)
                {
                    srcBuilder.Append(type.Raw + " ");
                }
            else 
                srcBuilder.Append("int ");

            srcBuilder.Append(Name + " ");

            foreach(var component in Components)
            {
                if (component is NHLSLToken)
                {
                    var token = (NHLSLToken)component;
                    srcBuilder.Append(token.Raw + " ");
                    if (token.Identifier == NHLSLTokenizer.Token.BRACKET_O || token.Identifier == NHLSLTokenizer.Token.BRACKET_C)
                        srcBuilder.AppendLine();
                    if (token.Identifier == NHLSLTokenizer.Token.SEMICOLON)
                        srcBuilder.AppendLine();
                }
                else srcBuilder.AppendLine(component.Transpile());
            }
            
            return srcBuilder.ToString();
        }
    }
}
