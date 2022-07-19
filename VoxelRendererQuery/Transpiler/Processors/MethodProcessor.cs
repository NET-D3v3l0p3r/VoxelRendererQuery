using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelRendererQuery.Includes;
using VoxelRendererQuery.Includes.Errors;
using VoxelRendererQuery.Includes.InternalStructs;
using VoxelRendererQuery.Transpiler.Meta;
using VoxelRendererQuery.Transpiler.Processors.Helper;
using VoxelRendererQuery.Transpiler.Processors.OOP;
using VoxelRendererQuery.Transpiler.Tokenizer;

namespace VoxelRendererQuery.Transpiler.Processors
{
    internal class MethodProcessor : IComponent, IProcessor
    {

        public string Name { get; set; }
        public List<NHLSLToken> Type { get; set; }

        public bool IsPointerType { get; set; }
        public bool IsConstructor { get; set; }

        public IEnumerator<NHLSLToken> TokenStream { get; set; }
        public List<IComponent> Components { get; set; }
        public List<Intrinsics> IntrinsicCalls { get; }

        public ParametersProcessor MethodParameters { get; private set; }

        private IMethodContainer _caller;
        private IntrinsicsHelper _intrinsicsHelper;

        // PROCESSORS
        private List<InstanceProcessor> _instanceProcessors = new List<InstanceProcessor>();

        public MethodProcessor(IMethodContainer caller, IEnumerator<NHLSLToken> tokenStream)
        {
            this._caller = caller;
            this.TokenStream = tokenStream;
            this.TokenStream.MoveNext();

            this.Type = new List<NHLSLToken>();
            this.Components = new List<IComponent>();
            this.IntrinsicCalls = new List<Intrinsics>();

            

            _intrinsicsHelper = IntrinsicsHelper.New(this);
        }

        public void Run()
        {
            MethodParameters = new ParametersProcessor(this.TokenStream);
            MethodParameters.Run();

            this.Components.Add(MethodParameters);

            int _bracketCounter = 0;

            Stack<NHLSLToken> _tokenStack = new Stack<NHLSLToken>();

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

                //if (token.Identifier == NHLSLTokenizer.Token.SQ_BRACKET_O)
                //{
                //    while (TokenStream.MoveNext() && TokenStream.Current.Identifier != NHLSLTokenizer.Token.SEMICOLON)
                //    {
                //        Console.WriteLine(TokenStream.Current);
                //    }

                //    Console.ForegroundColor = ConsoleColor.Red;
                //    Console.WriteLine("NOT IMPLEMENTED YET -- WARNING");
                //    Console.ForegroundColor = ConsoleColor.Gray;

                //    continue;
                //}

                //if (token.Identifier == NHLSLTokenizer.Token.EQUALS)
                //{
                //    Components.Add(token); //  ADD EQ TO COMPONENTS
                //    TokenStream.MoveNext();

                //    token = TokenStream.Current;

                //    if (_intrinsicsHelper.Actions.ContainsKey(token.Identifier))
                //    {
                //        _intrinsicsHelper.Actions[token.Identifier]();
                //        continue;
                //    }
                //    else if (token.Identifier == NHLSLTokenizer.Token.OOP_KEYWORD_NEW)
                //    {

                //    }
                //    else if (token.Identifier == NHLSLTokenizer.Token.STRING) // CHECK FOR FUNCTION CALL
                //    {
                //        Queue<NHLSLToken> _callQueue = new Queue<NHLSLToken>();
                //        _callQueue.Enqueue(token);


                //        // a.x.y.z.f();

                //        while (TokenStream.MoveNext() && TokenStream.Current.Identifier != NHLSLTokenizer.Token.BRACE_O)
                //        {
                //            _callQueue.Enqueue(TokenStream.Current);
                //        }
                //    }
                //}

                Components.Add(token);
                _tokenStack.Push(token);

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

            bool _hasMultiplier = false;
            foreach(var component in Components)
            {
                if (component is NHLSLToken)
                {
                    var token = (NHLSLToken)component;

                    if (token.Identifier == NHLSLTokenizer.Token.EQUALS && _hasMultiplier)
                        srcBuilder.Remove(srcBuilder.Length - 1, 1);

                    _hasMultiplier = false;

                    srcBuilder.Append(token.Raw + " ");

                    if (token.Raw.Equals("*"))
                        _hasMultiplier = true;

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
