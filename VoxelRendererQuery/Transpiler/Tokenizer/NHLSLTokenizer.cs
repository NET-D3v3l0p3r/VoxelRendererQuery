using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace VoxelRendererQuery.Transpiler.Tokenizer
{
    internal class NHLSLTokenizer
    {
        [Flags]
        public enum Token : long
        {
            SRC_BEGIN = 1L << 0,
            STRUCT = 1L << 1,
            STRING = 1L << 2,
            BRACKET_O = 1L << 3,
            BRACKET_C = 1L << 4,
            COLON = 1L << 5,
            SEMICOLON = 1L << 6,

            INTRINSICS_RAY = 1L << 7,
            INTRINSICS_NSRAYARRAYCHECK = 1L << 8,

            INTRINSICS_SET_RAY_ORIGIN = 1L << 9,
            INTRINSICS_SET_RAY_DIRECTION = 1L << 10,
            INTRINSICS_CREATE_RAY = 1L << 11,
            INTRINSICS_DEFAULT = 1L << 12,

            BRACE_O = 1L << 13,
            BRACE_C = 1L << 14,


            COMMA = 1L << 15,

            TGENERIC_O = 1L << 16,
            TGENERIC_C = 1L << 17,

            VOXELPROGRAM = 1L << 18,
            VOXELPASS = 1L << 19,

            PROGRAM_ROUTINE = 1L << 20,
            ENTRY_POINT = 1L << 21,
            VOXEL_SHADER = 1L << 22,
            RAY_GEN = 1L << 23,

            TRANSPILE = 1L << 24,

            FORBIDDEN = 1L << 25,

            QUOTES = 1L << 26,


            // OOP

            OOP_MODIFIER = 1L << 27,
            OOP_MODIFIER_PRIVATE = 1L << 28,
            OOP_MODIFIER_PUBLIC = 1L << 29,

            OOP_OVERRIDE = 1L << 30,

            OOP_CLASS = 1L << 31,
            OOP_POINTER = 1L << 32,

            OOP_KEYWORD_NEW = 1L << 33,

            POINTER_ARROW = 1L << 34,

            SQ_BRACKET_O = 1L << 35,
            SQ_BRACKET_C = 1L << 36,

            EQUALS = 1L << 37,

            DOT = 1L << 38
        }

        private static Dictionary<string, Token> _STR_TOKEN_MAPPER = new Dictionary<string, Token>()
        {
            { "@bismIllah", Token.SRC_BEGIN },

            { "struct", Token.STRUCT },

            { "{", Token.BRACKET_O },
            { "}", Token.BRACKET_C },

            { ":", Token.COLON },
            { ";", Token.SEMICOLON },

            { "nsRay", Token.INTRINSICS_RAY },
            { "nsRayArrayCheck", Token.INTRINSICS_NSRAYARRAYCHECK },

            { "nsSetRayOrigin", Token.INTRINSICS_SET_RAY_ORIGIN },
            { "nsSetRayDirection", Token.INTRINSICS_SET_RAY_DIRECTION },
            { "nsCreateRay", Token.INTRINSICS_CREATE_RAY },

            { "(", Token.BRACE_O },
            { ")", Token.BRACE_C },

            { "default", Token.INTRINSICS_DEFAULT },

            { "<<<", Token.TGENERIC_O },
            { ">>>", Token.TGENERIC_C },

            { "voxelprogram", Token.VOXELPROGRAM },

            { "entry_point", Token.ENTRY_POINT | Token.PROGRAM_ROUTINE },
            { "voxel_shader", Token.VOXEL_SHADER | Token.PROGRAM_ROUTINE },
            { "ray_gen", Token.RAY_GEN | Token.PROGRAM_ROUTINE },

            { "transpile", Token.TRANSPILE },

            { ",", Token.COMMA },

            { "string", Token.FORBIDDEN },
            { "\"", Token.FORBIDDEN  }, // unlikely
            { "Ray", Token.FORBIDDEN },
            { "backBuffer", Token.FORBIDDEN },
            { "voxelDataBuffer", Token.FORBIDDEN },
            { "volumeInitialSize", Token.FORBIDDEN },
            { "getData", Token.FORBIDDEN },
            { "AABB", Token.FORBIDDEN },
            { "createAABB", Token.FORBIDDEN },
            { "checkHit", Token.FORBIDDEN },
            { "arrayRayHit", Token.FORBIDDEN },
            { "RC", Token.FORBIDDEN },


            { "$", Token.OOP_POINTER },
            { "class", Token.OOP_CLASS },
            { "public", Token.OOP_MODIFIER_PUBLIC | Token.OOP_MODIFIER },
            { "private", Token.OOP_MODIFIER_PRIVATE | Token.OOP_MODIFIER },
            { "new", Token.OOP_KEYWORD_NEW },

            { "override", Token.OOP_OVERRIDE },

            { "->", Token.POINTER_ARROW },

            { "[", Token.SQ_BRACKET_O},
            { "]", Token.SQ_BRACKET_C},

            { "=", Token.EQUALS },

            //{ ".", Token.DOT }
        };


        public static Dictionary<Token, string> INTRINSICS_MAPPER = new Dictionary<Token, string>()
        {
            { Token.INTRINSICS_RAY, "Ray" }
        };


        private NHLSLTokenizer()
        {

        }

        private static NHLSLTokenizer _TOKENIZER;
        public static NHLSLTokenizer Default()
        {
            if (_TOKENIZER == null)
                _TOKENIZER = new NHLSLTokenizer();

            return _TOKENIZER;
        }


        public IEnumerable<NHLSLToken> Run(string src)
        {

            string _totalAccumulator = "";
            string _specialAccumulator = "";
            string _conventionalAccumulator = "";

            int col = 1;
            int row = 0;

            for (int i = 0; i < src.Length; i++)
            {
                char currentchar = src[i];


                if (_STR_TOKEN_MAPPER.ContainsKey(_specialAccumulator) && _specialAccumulator.Length > 1)
                {
                    if (_conventionalAccumulator.Length > 0)
                        yield return new NHLSLToken()
                        {
                            Row = row,
                            Col = col,
                            Identifier = _STR_TOKEN_MAPPER.ContainsKey(_conventionalAccumulator) ? _STR_TOKEN_MAPPER[_conventionalAccumulator] : Token.STRING,
                            Raw = _conventionalAccumulator
                        };

                    yield return new NHLSLToken()
                    {
                        Row = row,
                        Col = col,
                        Identifier = _STR_TOKEN_MAPPER[_specialAccumulator],
                        Raw = _specialAccumulator
                    };

                    _conventionalAccumulator = currentchar + "";
                    _totalAccumulator = currentchar + "";
                    _specialAccumulator = "";

                    if (_STR_TOKEN_MAPPER.ContainsKey(currentchar + ""))
                    {
                        yield return new NHLSLToken()
                        {
                            Row = row,
                            Col = col,
                            Identifier = _STR_TOKEN_MAPPER[currentchar + ""],
                            Raw = currentchar + ""
                        };

                        _totalAccumulator = "";
                        _conventionalAccumulator = "";
                        _specialAccumulator = "";
                    }


                }
                else if (_STR_TOKEN_MAPPER.ContainsKey(currentchar + ""))
                {
                    if (_totalAccumulator.Length > 0)
                        yield return new NHLSLToken()
                        {
                            Row = row,
                            Col = col,
                            Identifier = _STR_TOKEN_MAPPER.ContainsKey(_totalAccumulator) ? _STR_TOKEN_MAPPER[_totalAccumulator] : Token.STRING,
                            Raw = _totalAccumulator
                        };

                    yield return new NHLSLToken()
                    {
                        Row = row,
                        Col = col,
                        Identifier = _STR_TOKEN_MAPPER[currentchar + ""],
                        Raw = currentchar + ""
                    };

                    _totalAccumulator = "";
                    _conventionalAccumulator = "";
                    _specialAccumulator = "";
                }
                else if (
                  currentchar == '\r' ||
                  currentchar == '\n' ||
                  currentchar == ' ' ||
                  currentchar == '\t')
                {
                    if (_totalAccumulator.Length > 0)
                        yield return new NHLSLToken()
                        {
                            Row = row,
                            Col = col,
                            Identifier = _STR_TOKEN_MAPPER.ContainsKey(_totalAccumulator) ? _STR_TOKEN_MAPPER[_totalAccumulator] : Token.STRING,
                            Raw = _totalAccumulator
                        };


                    _totalAccumulator = "";
                    _conventionalAccumulator = "";
                    _specialAccumulator = "";

                }
                else
                {
                    if (char.IsLetterOrDigit(currentchar))
                        _conventionalAccumulator += currentchar;
                    else if (!char.IsLetterOrDigit(currentchar))
                        _specialAccumulator += currentchar;

                    _totalAccumulator += currentchar;
                }

                if (currentchar == '\n')
                {
                    col++;
                    row = 0;
                }
                if (currentchar == '\t')
                    row += 4;
                else
                    row++;
            }
        }
    }
}