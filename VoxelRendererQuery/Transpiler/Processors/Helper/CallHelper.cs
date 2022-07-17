using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelRendererQuery.Includes.InternalStructs;
using VoxelRendererQuery.Transpiler.Processors.OOP;
using VoxelRendererQuery.Transpiler.Tokenizer;

namespace VoxelRendererQuery.Transpiler.Processors.Helper
{
    internal class CallHelper
    {
        private CallHelper() { }

        private static CallHelper _CALL_HELPER;
        public static CallHelper Default()
        {
            if (_CALL_HELPER == null)
                _CALL_HELPER = new CallHelper();
            return _CALL_HELPER;
        }

        public enum AssignmentType
        {
            INSTANCIATION,
            REASSIGNING,
            NOTHING,
            ERROR
        }

        public List<Argument> GetAssignmentSpecs(IEnumerator<NHLSLToken> tokenStream, out AssignmentType assignmentType)
        {
            assignmentType = AssignmentType.ERROR;
            List<Argument> _args = new List<Argument>();
            if (tokenStream.Current.Raw.Equals("="))
            {
                assignmentType = AssignmentType.INSTANCIATION;

                tokenStream.MoveNext();
                if (tokenStream.Current.Identifier == NHLSLTokenizer.Token.OOP_KEYWORD_NEW)
                {
                    assignmentType = AssignmentType.INSTANCIATION;

                    tokenStream.MoveNext();
                    NHLSLToken _instanceType = tokenStream.Current;
                    tokenStream.MoveNext(); // class type
                    tokenStream.MoveNext(); // (

                    // TODO: CONSTRUCTOR
                    if (tokenStream.Current.Identifier != NHLSLTokenizer.Token.BRACE_C)
                    {
                        OOPClassProcessor _class = OOPHandler.Default().Classes[_instanceType.Raw];

                        Argument _currentArgument = new Argument();
                        _currentArgument.Tokens = new List<NHLSLToken>();
                        _currentArgument.Tokens.Add(tokenStream.Current);

                        int _braceCounter = 1;

                        while (tokenStream.MoveNext())
                        {
                            NHLSLToken _token = tokenStream.Current;

                            if (_token.Identifier == NHLSLTokenizer.Token.COMMA)
                            {
                                if (_braceCounter == 1)
                                {
                                    _args.Add(_currentArgument);
                                    _currentArgument = new Argument();
                                    _currentArgument.Tokens = new List<NHLSLToken>();
                                }
                                else _currentArgument.Tokens.Add(_token);
                            }
                            else if (_token.Identifier == NHLSLTokenizer.Token.BRACE_O)
                            {
                                _braceCounter++;
                                _currentArgument.Tokens.Add(_token);
                            }
                            else if (_token.Identifier == NHLSLTokenizer.Token.BRACE_C)
                            {
                                _braceCounter--;
                                if (_braceCounter > 0)
                                    _currentArgument.Tokens.Add(_token);
                            }
                            else
                            {
                                _currentArgument.Tokens.Add(_token);
                            }
                            if (_braceCounter == 0)
                                break;
                        }

                        _args.Add(_currentArgument);

                    }
                }
                else
                {
                    assignmentType = AssignmentType.REASSIGNING;
                }

            }else if(tokenStream.Current.Identifier == NHLSLTokenizer.Token.SEMICOLON)
            {
                assignmentType = AssignmentType.NOTHING;
            }

            return _args;
        }
    }
}
