using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelRendererQuery.Includes.InternalStructs;
using VoxelRendererQuery.Transpiler.Meta;
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

        public MethodProcessor GetAppropiateSignaure(List<MethodProcessor> methods, List<ArgumentProcessor> arguments)
        {
            MethodProcessor _correct = null;

            List<MethodProcessor> _comparisionGroup = methods.FindAll(p => p.MethodParameters.Parameters.Count == arguments.Count);
            foreach (var _currentMethod in _comparisionGroup)
            {
                _correct = _currentMethod;
                for (int i = 0; i < _currentMethod.MethodParameters.Parameters.Count; i++)
                {
                    var _param = _currentMethod.MethodParameters.Parameters[i];
                    if (!_param.Type[0].Raw.Equals(arguments[i].SpecificType))
                    {
                        _correct = null;
                        break;
                    }
                }

                if (_correct != null)
                    break;

            }


            return _correct;

        }

        public List<ArgumentProcessor> GetAssignmentSpecs(IMethodContainer caller, IEnumerator<NHLSLToken> tokenStream, out OOPClassProcessor instanceType, out AssignmentType assignmentType)
        {
            instanceType = null;
            assignmentType = AssignmentType.ERROR;
            List<ArgumentProcessor> _args = new List<ArgumentProcessor>();
         
            if (tokenStream.Current.Raw.Equals("="))
            {
                assignmentType = AssignmentType.INSTANCIATION;

                tokenStream.MoveNext();
                if (tokenStream.Current.Identifier == NHLSLTokenizer.Token.OOP_KEYWORD_NEW)
                {
                    assignmentType = AssignmentType.INSTANCIATION;

                    tokenStream.MoveNext();
                    NHLSLToken _instanceType = tokenStream.Current;
                    instanceType = OOPHandler.Default().Classes[_instanceType.Raw];
                    tokenStream.MoveNext(); // class type
                    tokenStream.MoveNext(); // (

                    // TODO: CONSTRUCTOR
                    if (tokenStream.Current.Identifier != NHLSLTokenizer.Token.BRACE_C)
                    {
                        #region Handle NEW Keyword/TODO: Move to seperate method
                        OOPClassProcessor _class = OOPHandler.Default().Classes[_instanceType.Raw];

                        List<NHLSLToken> _currentArgumentTokens = new List<NHLSLToken>();
                        _currentArgumentTokens.Add(tokenStream.Current);

                        ArgumentProcessor _argumentProcessor = null;

                        int _braceCounter = 1;

                        while (tokenStream.MoveNext())
                        {
                            NHLSLToken _token = tokenStream.Current;

                            if (_token.Identifier == NHLSLTokenizer.Token.COMMA)
                            {
                                if (_braceCounter == 1)
                                {
                                    _argumentProcessor = new ArgumentProcessor(caller, _currentArgumentTokens.GetEnumerator());
                                    _argumentProcessor.Run();
                                    _args.Add(_argumentProcessor);

                                    _currentArgumentTokens.Clear();
                                }
                                else _currentArgumentTokens.Add(_token);
                            }
                            else if (_token.Identifier == NHLSLTokenizer.Token.BRACE_O)
                            {
                                _braceCounter++;
                                _currentArgumentTokens.Add(_token);
                            }
                            else if (_token.Identifier == NHLSLTokenizer.Token.BRACE_C)
                            {
                                _braceCounter--;
                                if (_braceCounter > 0)
                                    _currentArgumentTokens.Add(_token);
                            }
                            else
                            {
                                _currentArgumentTokens.Add(_token);
                            }
                            if (_braceCounter == 0)
                                break;
                        }

                        _argumentProcessor = new ArgumentProcessor(caller, _currentArgumentTokens.GetEnumerator());
                        _argumentProcessor.Run();
                        _args.Add(_argumentProcessor);

                        #endregion
                    }
                    else 
                    {
                        // Check constructor of instanceType
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
