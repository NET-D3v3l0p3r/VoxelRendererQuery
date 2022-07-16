using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoxelRendererQuery.Includes.Errors
{
    public class IllegalSourceException : Exception
    {
        public IllegalSourceException(string message) : base(message)
        {
        }
    }
}
