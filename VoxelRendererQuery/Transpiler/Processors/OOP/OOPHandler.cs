using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoxelRendererQuery.Transpiler.Processors.OOP
{
    internal class OOPHandler
    {
        public Dictionary<string, OOPClassProcessor> Classes { get; private set; }


        private StringBuilder _heapBuilder = new StringBuilder();

        private OOPHandler()
        {
            this.Classes = new Dictionary<string, OOPClassProcessor>();
        }


        public void CreatePseudoHeap(OOPClassProcessor forClass)
        {
            string _heapName = forClass.Name;
            if (forClass.Childrens.Count > 0)
                _heapName = "MANAGED_" + forClass.Name;

           
            _heapBuilder.AppendLine(_heapName + " " + forClass.Name + "_HEAP[MAX];");
            _heapBuilder.AppendLine("int " + forClass.Name + "_HEAP_POINTER;");
        }


        private static OOPHandler _OOP_HANDLER;
        public static OOPHandler Default()
        {
            if (_OOP_HANDLER == null)
                _OOP_HANDLER = new OOPHandler();
            return _OOP_HANDLER;
        }
    }
}
