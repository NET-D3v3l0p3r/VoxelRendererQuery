using VoxelRendererQuery.Transpiler.Tokenizer;

namespace VoxelRendererQuery.Transpiler.Processors.OOP
{
    internal struct Field
    {
        public bool IsPointer;
        public NHLSLToken Modifier;
        public NHLSLToken Type;
        public NHLSLToken Name;
    }
}