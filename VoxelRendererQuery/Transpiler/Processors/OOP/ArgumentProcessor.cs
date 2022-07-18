using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelRendererQuery.Includes;
using VoxelRendererQuery.Transpiler.Meta;
using VoxelRendererQuery.Transpiler.Tokenizer;

namespace VoxelRendererQuery.Transpiler.Processors.OOP
{
    internal class ArgumentProcessor : IProcessor, IComponent
    {
        public IEnumerator<NHLSLToken> TokenStream { get; set; }
        public List<IComponent> Components { get; set; }

        private IMethodContainer _caller;

        public string SpecificType { get; private set; }

        private bool _needSpecialParser = false;

        public ArgumentProcessor(IMethodContainer caller, IEnumerator<NHLSLToken> tokenStream)
        {
            this._caller = caller;
            this.TokenStream = tokenStream;
            this.Components = new List<IComponent>();
        }

        public void Run()
        {
            bool _tryParseNum = true;
            bool _doneTypeParsing = false;
            Stack<NHLSLToken> _tokenStack = new Stack<NHLSLToken>();



            while (TokenStream.MoveNext())
            {
                var currentToken = TokenStream.Current;
                var data = currentToken.Raw;

                bool _isInt = int.TryParse(data, out _);
                bool _isDouble = double.TryParse(data, out _) && (data[data.Length - 1] != 'f') && data.Contains("."); 
                bool _isFloat  = float.TryParse(data, out _) && (data[data.Length - 1] == 'f');

                if (!_doneTypeParsing)
                {
                    if (_tryParseNum)
                    {
                        if (_isInt)
                            SpecificType = "int";
                        else if (_isDouble)
                            SpecificType = "double";
                        else if (_isFloat)
                            SpecificType = "float";

                        _doneTypeParsing = _isFloat | _isDouble | _isInt;
                    }
                    else
                    {

                        if (currentToken.Identifier == NHLSLTokenizer.Token.BRACE_O)
                        {
                            var _potentialFunctionName = _tokenStack.Pop();
                            var _method = _caller.Methods.Find(p => p.Name.Equals(_potentialFunctionName.Raw));
                            if (_method == null)
                                SpecificType = _potentialFunctionName.Raw;
                            else
                            {
                                _needSpecialParser = _method.IsPointerType;

                                string _typeName = _method.Type[0].Raw;
                                SpecificType = _typeName;

                                _doneTypeParsing = true;
                            }
                        }
                    }
                }


                _tokenStack.Push(currentToken);
                this.Components.Add(currentToken);

                _tryParseNum = false;
            }
        }

        public string Transpile()
        {
            throw new NotImplementedException();
        }
    }
}
