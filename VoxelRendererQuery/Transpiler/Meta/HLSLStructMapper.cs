using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VoxelRendererQuery.Includes;

namespace VoxelRendererQuery.Transpiler.Meta
{
    internal class HLSLStructMapper<T> 
    {
        public string StructName { get; private set; }
        public int TotalSize { get; private set; }
        internal struct FieldData
        {
            internal FieldInfo field;
            internal string name;
            internal string type;
            internal int bits;

            internal Type typeRaw;
            
        }

        private List<FieldData> _fieldData = new List<FieldData>();
        private Dictionary<Type, string> _supportedTypes = new Dictionary<Type, string>()
        {
            { typeof(int), "int" },
            { typeof(byte), "int" },
            { typeof(bool), "bool" }
        };

        public HLSLStructMapper()
        {
            _verify();
        }

        private void _verify()
        {
            Type type = typeof(T);
            bool throwExc = true;
            foreach (var attribute in type.GetCustomAttributes(true))
            {
                if (attribute.GetType().Equals(typeof(VoxelDefinition)))
                {
                    throwExc = false;
                    break;
                }
            }

            if (throwExc)
                throw new InvalidCastException();


            foreach (var field in type.GetFields())
            {
                foreach (var attribute in field.GetCustomAttributes(true))
                {
                    if (attribute.GetType().Equals(typeof(SizeInBits)))
                    {
                        TotalSize += ((SizeInBits)attribute).bits;
                        _fieldData.Add(new FieldData()
                        {
                            field = field,
                            bits = ((SizeInBits)attribute).bits,
                            name = field.Name,
                            type = _supportedTypes[field.FieldType],
                            typeRaw = field.FieldType
                        });


                        break;
                    }
                }
            }


            if (TotalSize > 32)
                throw new IndexOutOfRangeException();

            this.StructName = type.Name;
        }

        public string GenerateHLSLStruct()
        {
            StringBuilder _structBuilder = new StringBuilder();

            _structBuilder.AppendLine("struct " + StructName + " {");
            foreach (var field in _fieldData)
            {
                _structBuilder.AppendLine("\t" + field.type + " " + field.name + ";");
            }

            _structBuilder.AppendLine("};");

            return _structBuilder.ToString();

        }


        public string GenerateHLSLConverter()
        {
            StringBuilder _converterBuilder = new StringBuilder();

            _converterBuilder.AppendLine("inline " + StructName + " getFromInt32(int rawVoxel) {");
            _converterBuilder.AppendLine("\t" + StructName + " voxel = (" + StructName + ")0;");
            int offset = 0;
            foreach (var field in _fieldData)
            {
                _converterBuilder.AppendLine("\tvoxel." + field.name + " = (rawVoxel >> " + offset + ") & " +
                    ((1<<field.bits) - 1) + ";");
                offset += field.bits;
            }

            _converterBuilder.AppendLine("\treturn voxel;");
            _converterBuilder.AppendLine("};");

            return _converterBuilder.ToString();
        }

        public int GetInt32(T instance)
        {
            int packedVoxel = 0;
            Type type = typeof(T);

            int offset = 0;
            foreach (var field in _fieldData)
            {
                int value = (int)field.field.GetValue(instance);
                packedVoxel |= (value << offset);
                offset += field.bits;
            }

            return packedVoxel;
        }
        
        
    }
}
