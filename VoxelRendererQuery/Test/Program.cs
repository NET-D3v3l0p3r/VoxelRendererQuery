using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using VoxelRendererQuery.Transpiler.Tokenizer;
using VoxelRendererQuery.Transpiler;
using System.IO;

namespace VoxelRendererQuery.Test
{
    public static class Program
    {
        [DllImport("kernel32.dll")]
        static extern bool AllocConsole();
        public static void Main(string[] args)
        {
            AllocConsole();

            var stream = NHLSLTokenizer.Default().Run(File.ReadAllText(@"example.nhlsl"));

            NHLSLTranspiler transpiler = new NHLSLTranspiler(stream.GetEnumerator());
            transpiler.Run();
            var s = transpiler.Transpile();

            Console.WriteLine(s);


            NHLSLTokenizer.Token token = NHLSLTokenizer.Token.BRACE_O | NHLSLTokenizer.Token.BRACE_C;

            Console.WriteLine((token & NHLSLTokenizer.Token.BRACKET_C) == NHLSLTokenizer.Token.BRACE_O);

            Console.Read();
        }
    }
}
