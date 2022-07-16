using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelRendererQuery.Includes;
using VoxelRendererQuery.Includes.InternalStructs;
using VoxelRendererQuery.Transpiler.Tokenizer;

namespace VoxelRendererQuery.Transpiler.Processors.OOP
{
    internal partial class OOPClassProcessor : IComponent, IProcessor
    {

        public string Name { get; private set; }

        public OOPClassProcessor Parent { get; private set; }
        public List<OOPClassProcessor> Childrens { get; private set; }

        public List<IComponent> Components { get; set; }
        public IEnumerator<NHLSLToken> TokenStream { get; set; }

        public List<Field> Fields { get; private set; }
        public List<MethodProcessor> Constructors { get; private set; }
        public List<MethodProcessor> Methods { get; private set; }

        public OOPClassProcessor(IEnumerator<NHLSLToken> tokenStream)
        {
            this.TokenStream = tokenStream;
            this.Components = new List<IComponent>();
            this.Fields = new List<Field>();
            this.Childrens = new List<OOPClassProcessor>();
            this.Constructors = new List<MethodProcessor>();
            this.Methods = new List<MethodProcessor>();

            this.TokenStream.MoveNext();
         
        }

        public void AddChildren(OOPClassProcessor child)
        {
            if (Parent != null)
                OOPHandler.Default().Classes[Parent.Name].AddChildren(child);
            this.Childrens.Add(child);
        }

        public void Run()
        {
            this.Name = this.TokenStream.Current.Raw;
            OOPHandler.Default().Classes.Add(Name, this);

            this.TokenStream.MoveNext();

            int _bracketCounter = 0;

            NHLSLToken _definition = this.TokenStream.Current;
            List<IComponent> _copyList = new List<IComponent>();

            if (_definition.Identifier == NHLSLTokenizer.Token.COLON)
            {
                this.TokenStream.MoveNext();
                string _superClassName = this.TokenStream.Current.Raw;

                this.Parent = OOPHandler.Default().Classes[_superClassName];
                OOPHandler.Default().Classes[_superClassName].AddChildren(this);

                bool _copy = false;
                IEnumerator<IComponent> _copyStream = this.
                    Parent.
                    Components.
                    AsEnumerable().
                    GetEnumerator();

                while (_copyStream.MoveNext())
                {
                    var component = _copyStream.Current;
                    if (component is NHLSLToken)
                    {
                        var token = (NHLSLToken)component;
                        if ((token.Identifier & NHLSLTokenizer.Token.OOP_MODIFIER) == NHLSLTokenizer.Token.OOP_MODIFIER)
                        {
                            if (_copy)
                                break;

                            if ((token.Identifier & NHLSLTokenizer.Token.OOP_MODIFIER_PUBLIC) == NHLSLTokenizer.Token.OOP_MODIFIER_PUBLIC)
                            {
                                _copyStream.MoveNext();
                                _copy = true;
                                continue;
                            }
                        }
                        else if (token.Identifier == NHLSLTokenizer.Token.BRACKET_C)
                            break;
                    }

                    if (_copy)
                        _copyList.Add(_copyStream.Current);
                }


            }
            else if (this.TokenStream.Current.Identifier == NHLSLTokenizer.Token.BRACKET_O)
            {
                this.Components.Add(_definition);
                _bracketCounter = 1;
            }


            Stack<NHLSLToken> _tokenStack = new Stack<NHLSLToken>();
            
            bool _override = false;
            bool _isPointer = false;

            NHLSLToken _modifier = null;
             
            while (this.TokenStream.MoveNext())
            {
                NHLSLToken token = this.TokenStream.Current;

                if (token.Identifier == NHLSLTokenizer.Token.BRACKET_O)
                    _bracketCounter++;
                if (token.Identifier == NHLSLTokenizer.Token.BRACKET_C)
                    _bracketCounter--;

                if (token.Identifier == NHLSLTokenizer.Token.OOP_OVERRIDE)
                {
                    _override = true;
                    continue;
                }

                if ((token.Identifier & NHLSLTokenizer.Token.OOP_MODIFIER) == NHLSLTokenizer.Token.OOP_MODIFIER)
                    _modifier = token;

                if ((token.Identifier & NHLSLTokenizer.Token.OOP_MODIFIER_PUBLIC) == NHLSLTokenizer.Token.OOP_MODIFIER_PUBLIC)
                {
                    this.TokenStream.MoveNext(); // :

                    this.Components.Add(token);
                    this.Components.Add(this.TokenStream.Current);

                    foreach (var _copyC in _copyList)
                        this.Components.Add(_copyC);

                    continue;
                }

                if(token.Identifier == NHLSLTokenizer.Token.OOP_POINTER)
                {
                    _isPointer = true;
                    continue;
                }

                // VARIABLE
                if(token.Identifier == NHLSLTokenizer.Token.SEMICOLON)
                {

                    Field field = new Field();

                    field.IsPointer = _isPointer;
                    field.Modifier = _modifier;
                    field.Name = _tokenStack.Pop();
                    field.Type = _tokenStack.Pop();

                    Fields.Add(field);

                    _isPointer = false;
                }

                // METHOD
                if (token.Identifier == NHLSLTokenizer.Token.BRACE_O)
                {
                    Stack<NHLSLToken> _temp = new Stack<NHLSLToken>();

                    while (
                        _tokenStack.Count > 0 &&
                        _tokenStack.Peek().Identifier != NHLSLTokenizer.Token.SEMICOLON &&
                        _tokenStack.Peek().Identifier != NHLSLTokenizer.Token.BRACKET_C)
                    {

                        NHLSLToken cur = _tokenStack.Pop();
                        _temp.Push(cur);
                        Components.Remove(cur);
                    }

                    MethodProcessor methodProcessor = new MethodProcessor(this, this.TokenStream);

                    while (_temp.Count > 1)
                        methodProcessor.Type.Add(_temp.Pop());

                    methodProcessor.Name = _temp.Pop().Raw;
                    methodProcessor.IsPointerType = _isPointer;

                    if (methodProcessor.Type.Count == 0)
                    {
                        if (!methodProcessor.Name.Equals(this.Name))
                            throw new Exception("Error in column " + TokenStream.Current.Col + " and line " + TokenStream.Current.Row + " with folling exception: Wrong constructor definition.");

                        methodProcessor.IsConstructor = true;
                        this.Constructors.Add(methodProcessor);
                    }
                    else this.Methods.Add(methodProcessor);   

                    methodProcessor.Run();

                    if (!_override)
                        Components.Add(methodProcessor);
                    else
                    {
                        int _indexOld = this.Components.FindIndex(p => p is MethodProcessor && ((MethodProcessor)p).Name.Equals(methodProcessor.Name));
                        Components[_indexOld] = methodProcessor;
                    }

                    _override = false;
                    _isPointer = false;
                    continue;
                }



                this.Components.Add(token);
                _tokenStack.Push(token);

                if (_bracketCounter == 0)
                    break;
            }
        }

        public string GetPolymorphType()
        {
            if (this.Childrens.Count < 1)
                return "";

            StringBuilder _srcBuilder = new StringBuilder();

            _srcBuilder.AppendLine("struct MANAGED_" + Name + " {");
            foreach (var component in Components)
            {
                if (component is NHLSLToken)
                {
                    NHLSLToken token = (NHLSLToken)component;
                    if (token.Identifier == NHLSLTokenizer.Token.BRACKET_O || token.Identifier == NHLSLTokenizer.Token.BRACKET_C)
                        continue;

                    if ((token.Identifier & NHLSLTokenizer.Token.OOP_MODIFIER) == NHLSLTokenizer.Token.OOP_MODIFIER)
                        continue;
                    if (token.Identifier == NHLSLTokenizer.Token.COLON)
                        continue;
                    _srcBuilder.Append(token.Raw + " ");
                
                    if (token.Identifier == NHLSLTokenizer.Token.SEMICOLON)
                        _srcBuilder.AppendLine();
                }

                if(component is MethodProcessor)
                {
                    MethodProcessor methodProcessor = (MethodProcessor)component;
                    _srcBuilder.AppendLine(methodProcessor.Transpile());
                }
            }


            foreach (var child in Childrens)
            {
                _srcBuilder.AppendLine(child.Name + " " + child.Name.ToLower() + ";");
            }

            _srcBuilder.AppendLine("}");
            return _srcBuilder.ToString();
        }

        public string Transpile()
        {

            StringBuilder _srcBuilder = new StringBuilder();

            _srcBuilder.AppendLine("struct " + Name + " {");
            foreach (var component in Components)
            {
                if (component is NHLSLToken)
                {
                    NHLSLToken token = (NHLSLToken)component;
                    if (token.Identifier == NHLSLTokenizer.Token.BRACKET_O || token.Identifier == NHLSLTokenizer.Token.BRACKET_C)
                        continue;

                    if ((token.Identifier & NHLSLTokenizer.Token.OOP_MODIFIER) == NHLSLTokenizer.Token.OOP_MODIFIER)
                        continue;
                    if (token.Identifier == NHLSLTokenizer.Token.COLON)
                        continue;
                    _srcBuilder.Append(token.Raw + " ");

                    if (token.Identifier == NHLSLTokenizer.Token.SEMICOLON)
                        _srcBuilder.AppendLine();
                }

                if (component is MethodProcessor)
                {
                    MethodProcessor methodProcessor = (MethodProcessor)component;
                    _srcBuilder.AppendLine(methodProcessor.Transpile());
                }
            }


            _srcBuilder.AppendLine("}");

            return _srcBuilder.ToString();
        }

        public void VerifyConstructorArguments(IEnumerator<NHLSLToken> arguments)
        {

        }
    }
}
