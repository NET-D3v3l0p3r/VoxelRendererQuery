using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelRendererQuery.Includes;
using VoxelRendererQuery.Includes.Errors;
using VoxelRendererQuery.Includes.InternalStructs;
using VoxelRendererQuery.Transpiler.Tokenizer;

namespace VoxelRendererQuery.Transpiler.Processors
{
    internal class ParametersProcessor : IComponent, IProcessor
    {
        public IEnumerator<NHLSLToken> TokenStream { get; set; }
        public List<IComponent> Components { get; set; }
        public List<MethodParam> Parameters { get; private set; }

        public ParametersProcessor(IEnumerator<NHLSLToken> tokenStream)
        {
            this.TokenStream = tokenStream;
            this.Components = new List<IComponent>();
            this.Parameters = new List<MethodParam>();
        }

        public void Run()
        {

            if (TokenStream.Current.Identifier == NHLSLTokenizer.Token.BRACE_C)
            {
                return;
            }
            

            NHLSLToken _paramName = null;

            MethodParam _currentParam = new MethodParam();
            _currentParam.Type = new List<NHLSLToken>();
            _currentParam.Type.Add(TokenStream.Current);

            if (TokenStream.Current.Identifier == NHLSLTokenizer.Token.FORBIDDEN)
                throw new IllegalOverrideException(
                              "The source contains an illegal symbol at line " + TokenStream.Current.Col + " and row " + TokenStream.Current.Row + ": " + TokenStream.Current.Raw);

            while (TokenStream.MoveNext())
            {
                NHLSLToken token = TokenStream.Current;

                if (token.Identifier == NHLSLTokenizer.Token.FORBIDDEN)
                    throw new IllegalOverrideException(
                        "The source contains an illegal symbol at line " + token.Col + " and row " + token.Row + ": " + token.Raw);


                if (token.Identifier == NHLSLTokenizer.Token.COMMA || token.Identifier == NHLSLTokenizer.Token.BRACE_C)
                {
                    _currentParam.Type.RemoveAt(_currentParam.Type.Count - 1);
                    _currentParam.Name = _paramName;
                    this.Parameters.Add(_currentParam);

                    _currentParam = new MethodParam();
                    _currentParam.Type = new List<NHLSLToken>();

                    if (token.Identifier == NHLSLTokenizer.Token.BRACE_C)
                        break;

                    continue;
                }

                if(token.Identifier == NHLSLTokenizer.Token.OOP_POINTER)
                {
                    _currentParam.IsPointer = true;
                    continue;
                }

                _paramName = token;
                _currentParam.Type.Add(token);
            }
        }

        public string Transpile()
        {
            StringBuilder srcBuilder = new StringBuilder();

            srcBuilder.Append("(");

            foreach (var param in Parameters)
            {
                if (!param.IsPointer)
                    srcBuilder.Append(param.Type[0].Raw + " ");
                else
                    srcBuilder.Append("int ");


                srcBuilder.Append(param.Name.Raw + ",");
            }

            if (Parameters.Count > 0)
                srcBuilder.Remove(srcBuilder.Length - 1, 1);

            srcBuilder.Append(")");


            return srcBuilder.ToString();
        }
    }
}
