using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VoxelRendererQuery.Includes;
using VoxelRendererQuery.Includes.Errors;
using VoxelRendererQuery.Transpiler.Meta;
using VoxelRendererQuery.Transpiler.Processors;
using VoxelRendererQuery.Transpiler.Processors.OOP;
using VoxelRendererQuery.Transpiler.Tokenizer;

namespace VoxelRendererQuery.Transpiler
{
    internal class NHLSLTranspiler : IComponent, IProcessor, IMethodContainer
    {
        public string VoxelProgramName { get; private set; }
        public List<IComponent> Components { get; set; }
        public List<MethodProcessor> Methods { get; set; }
        public List<Field> Fields { get; set; }

        public IEnumerator<NHLSLToken> TokenStream { get; set; }

        public string MainEntry { get; private set; }
        public string VoxelShader { get; private set; }
        public string RayGen { get; private set; }

        private Dictionary<string, StructProcessor> _structProcessors = new Dictionary<string, StructProcessor>();

        private static ConcurrentDictionary<Thread, NHLSLTranspiler> _CURRENT_INSTANCES 
            = new ConcurrentDictionary<Thread, NHLSLTranspiler>();

        public static NHLSLTranspiler Current()
        {
            return _CURRENT_INSTANCES[Thread.CurrentThread];
        }

        public NHLSLTranspiler(IEnumerator<NHLSLToken> tokenStream)
        {
            this.TokenStream = tokenStream;
            this.Components = new List<IComponent>();
            this.Methods = new List<MethodProcessor>();
            this.Fields = new List<Field>();

            _CURRENT_INSTANCES[Thread.CurrentThread] = this;
        }

        public void Run()
        {
            Stack<NHLSLToken> _tokenStack = new Stack<NHLSLToken>();

            TokenStream.MoveNext();
            if (TokenStream.Current.Identifier != NHLSLTokenizer.Token.SRC_BEGIN)
                throw new IllegalSourceException("The source file must start with: bismIllah");

            bool _isPointer = false;

            while(TokenStream.MoveNext())
            {
                NHLSLToken token = TokenStream.Current;

                if (token.Identifier == NHLSLTokenizer.Token.FORBIDDEN)
                    throw new IllegalOverrideException(
                        "The source contains an illegal symbol at line " + token.Col + " and row " + token.Row + ": " + token.Raw);

                if (token.Identifier == NHLSLTokenizer.Token.STRUCT)
                {
                    StructProcessor structProcessor = new StructProcessor(TokenStream);
                    structProcessor.Run();
                    this.Components.Add(structProcessor);
                }
                else if (token.Identifier == NHLSLTokenizer.Token.OOP_POINTER)
                {
                    _isPointer = true;
                }
                else if(token.Identifier == NHLSLTokenizer.Token.BRACE_O)
                {
                    Stack<NHLSLToken> _temp = new Stack<NHLSLToken>();

                    while(
                        _tokenStack.Count > 0 && 
                        _tokenStack.Peek().Identifier != NHLSLTokenizer.Token.SEMICOLON &&
                        _tokenStack.Peek().Identifier != NHLSLTokenizer.Token.BRACKET_C)
                    {

                        NHLSLToken cur = _tokenStack.Pop();
                        _temp.Push(cur);
                        Components.Remove(cur);
                    }

                    MethodProcessor methodProcessor = new MethodProcessor(this, TokenStream);

                    while (_temp.Count > 1)
                        methodProcessor.Type.Add(_temp.Pop());

                    this.Methods.Add(methodProcessor);

                    methodProcessor.Name = _temp.Pop().Raw;
                    methodProcessor.IsPointerType = _isPointer;

                    methodProcessor.Run();

                    Components.Add(methodProcessor);
                }
                else if(token.Identifier == NHLSLTokenizer.Token.VOXELPROGRAM)
                {
                    TokenStream.MoveNext();
                    this.VoxelProgramName = TokenStream.Current.Raw;
                    TokenStream.MoveNext(); // {

                    while (TokenStream.MoveNext())
                    {
                        NHLSLToken cur = TokenStream.Current;
                        if ((cur.Identifier & NHLSLTokenizer.Token.PROGRAM_ROUTINE) == NHLSLTokenizer.Token.PROGRAM_ROUTINE)
                        {
                            TokenStream.MoveNext(); // =
                            TokenStream.MoveNext(); // transpile
                            if (TokenStream.Current.Identifier != NHLSLTokenizer.Token.TRANSPILE)
                                throw new IllegalSourceException("Keyword missing: transpile");

                            TokenStream.MoveNext();

                            if ((cur.Identifier & NHLSLTokenizer.Token.ENTRY_POINT) == NHLSLTokenizer.Token.ENTRY_POINT)
                                MainEntry = TokenStream.Current.Raw;
                            else if ((cur.Identifier & NHLSLTokenizer.Token.VOXEL_SHADER) == NHLSLTokenizer.Token.VOXEL_SHADER)
                                VoxelShader = TokenStream.Current.Raw;
                            else if ((cur.Identifier & NHLSLTokenizer.Token.RAY_GEN) == NHLSLTokenizer.Token.RAY_GEN)
                                RayGen = TokenStream.Current.Raw;

                            TokenStream.MoveNext(); // (
                            TokenStream.MoveNext(); // )
                            TokenStream.MoveNext(); // ;
                        }
                        else if (cur.Identifier == NHLSLTokenizer.Token.BRACKET_C)
                            break;
                    }
                }
                else
                {
                    Components.Add(token);
                    _tokenStack.Push(token);
                }
            }

            _tokenStack.Clear();
            TokenStream.Dispose();
        }

        public string Transpile()
        {
            StringBuilder srcBuilder = new StringBuilder();

            foreach (var component in Components)
            {
                if(component is StructProcessor)
                {
                    var strcprcs = (StructProcessor)component;
                    _structProcessors.Add(strcprcs.Name, strcprcs);

                    srcBuilder.Append(component.Transpile());
                }
                else if(component is MethodProcessor)
                {
                    var mthdprcs = (MethodProcessor)component;
                    foreach (var intrinsics in mthdprcs.IntrinsicCalls)
                    {
                        var raytestRoutine = VoxelRendererQuery.Properties.Resources.volumeRayTest;
                        raytestRoutine = raytestRoutine.Replace("$$$RTRCRESULT$$$", intrinsics.OfStructure);
                        raytestRoutine = raytestRoutine.Replace("$$$VOLUME_RAYTEST$$$", "volumeRayTest" + intrinsics.Id);

                        string assignRoutine = _structProcessors[intrinsics.OfStructure].GetAssignments();

                        raytestRoutine = raytestRoutine.Replace("$$$ASSIGNMENT_ROUTINE$$$", assignRoutine);

                        srcBuilder.AppendLine(raytestRoutine);

                    }

                    srcBuilder.Append(component.Transpile());

                }else
                {
                    srcBuilder.Append(component.Transpile() + " ");
                    if (component is NHLSLToken && ((NHLSLToken)component).Identifier == NHLSLTokenizer.Token.SEMICOLON)
                        srcBuilder.AppendLine();
                }
            }

            return srcBuilder.ToString();
        }

    }
}
