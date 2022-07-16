using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelRendererQuery.Includes;
using VoxelRendererQuery.Includes.InternalStructs;
using VoxelRendererQuery.Transpiler.Tokenizer;

namespace VoxelRendererQuery.Transpiler.Processors.Helper
{
    internal class IntrinsicsHelper
    {
        private static List<Intrinsics> _USED_INTRINSICS = new List<Intrinsics>();


        private MethodProcessor _methodProcessor;
        private bool _rayCreated;

        public Dictionary<NHLSLTokenizer.Token, Action> Actions;

        private IntrinsicsHelper(MethodProcessor methodProcessor)
        {
            this._methodProcessor = methodProcessor;

            this.Actions = new Dictionary<NHLSLTokenizer.Token, Action>()
            {
                { NHLSLTokenizer.Token.INTRINSICS_NSRAYARRAYCHECK, ()=>
                {
                    _methodProcessor.TokenStream.MoveNext();
                    if (_methodProcessor.TokenStream.Current.Identifier == NHLSLTokenizer.Token.TGENERIC_O)
                        _methodProcessor.TokenStream.MoveNext();

                    string ofStruct = _methodProcessor.TokenStream.Current.Raw;

                    if (!_USED_INTRINSICS.Exists(p => p.OfStructure.Equals(ofStruct)))
                    {
                        _methodProcessor.IntrinsicCalls.Add(
                            new Intrinsics() { Id = Intrinsics.INTRINSICS_COUNTER++, OfStructure = ofStruct });
                        _USED_INTRINSICS.Add(_methodProcessor.IntrinsicCalls[_methodProcessor.IntrinsicCalls.Count - 1]);
                    }

                    _methodProcessor.TokenStream.MoveNext();
                    if (_methodProcessor.TokenStream.Current.Identifier == NHLSLTokenizer.Token.TGENERIC_C)
                    {
                        _methodProcessor.Components.Add(new NHLSLToken()
                        {
                            Identifier = NHLSLTokenizer.Token.INTRINSICS_NSRAYARRAYCHECK,
                            Raw = "volumeRayTest" + _USED_INTRINSICS.Find(p => p.OfStructure.Equals(ofStruct)).Id
                        });
                    }
                } 
                },

                { NHLSLTokenizer.Token.INTRINSICS_SET_RAY_ORIGIN, ()=>
                {
                    this.CreateRay();

                    foreach (var _t in NHLSLTokenizer.Default().Run("ray.origin = "))
                        _methodProcessor.Components.Add(_t);

                    _methodProcessor.TokenStream.MoveNext();
                    while (
                    _methodProcessor.TokenStream.MoveNext() && 
                    _methodProcessor.TokenStream.Current.Identifier != NHLSLTokenizer.Token.SEMICOLON)
                        _methodProcessor.Components.Add(_methodProcessor.TokenStream.Current);

                    _methodProcessor.Components.RemoveAt(_methodProcessor.Components.Count - 1);
                    _methodProcessor.Components.Add(_methodProcessor.TokenStream.Current);
                }
                },

                { NHLSLTokenizer.Token.INTRINSICS_SET_RAY_DIRECTION, ()=>
                {
                    this.CreateRay();

                    foreach (var _t in NHLSLTokenizer.Default().Run("ray.dir = "))
                        _methodProcessor.Components.Add(_t);

                    _methodProcessor.TokenStream.MoveNext();
                    while (_methodProcessor.TokenStream.MoveNext() && _methodProcessor.TokenStream.Current.Identifier != NHLSLTokenizer.Token.SEMICOLON)
                        _methodProcessor.Components.Add(_methodProcessor.TokenStream.Current);

                    _methodProcessor.Components.RemoveAt(_methodProcessor.Components.Count - 1);
                    _methodProcessor.Components.Add(_methodProcessor.TokenStream.Current);

                    foreach (var _t in NHLSLTokenizer.Default().Run("ray.dirRcp = rcp(ray.dir);"))
                        _methodProcessor.Components.Add(_t);

                }
                },

                { NHLSLTokenizer.Token.INTRINSICS_CREATE_RAY, ()=>
                {
                     _methodProcessor.TokenStream.MoveNext(); // (
                    this.CreateRay();

                    foreach (var _t in NHLSLTokenizer.Default().Run("ray;"))
                        _methodProcessor.Components.Add(_t);
                    _methodProcessor.TokenStream.MoveNext(); // )
                    _methodProcessor.TokenStream.MoveNext();
                }
                },

                { NHLSLTokenizer.Token.INTRINSICS_DEFAULT, ()=>
                {
                    _methodProcessor.TokenStream.MoveNext(); // (
                    _methodProcessor.TokenStream.MoveNext(); // object

                    var objectToken = _methodProcessor.TokenStream.Current;

                    foreach (var _t in NHLSLTokenizer.Default().Run("(" + objectToken.Raw + ")0;"))
                        _methodProcessor.Components.Add(_t);

                    _methodProcessor.TokenStream.MoveNext();
                    _methodProcessor.TokenStream.MoveNext();
                }
                }
            };
        }

        public static IntrinsicsHelper New(MethodProcessor methodProcessor)
        {
            return new IntrinsicsHelper(methodProcessor);
        }

        public void CreateRay()
        {
            if (!_rayCreated)
            {
                foreach (var _t in NHLSLTokenizer.Default().Run("Ray ray = (Ray)0;"))
                    _methodProcessor.Components.Add(_t);
                _rayCreated = true;
            }
        }
    }
}
