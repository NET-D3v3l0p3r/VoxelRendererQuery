using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoxelRendererQuery.Transpiler.Processors.Helper
{
    internal class InstantiatorHelper
    {
        private InstantiatorHelper()
        {

        }

        public static InstantiatorHelper New()
        {
            return new InstantiatorHelper();
        }

    }
}
