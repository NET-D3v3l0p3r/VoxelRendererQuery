using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelRendererQuery.Raytracer;

namespace VoxelRendererQuery.Ressources
{
    public class VoxelVolume3D<T> : Texture3D
    {
        public RTVoxelQuery<T> VoxelQueryRT { get; private set; }
        public VoxelVolume3D(RTVoxelQuery<T> vxlQry , int width, int height, int depth)
            : base(vxlQry.GraphicsDevice, width, height, depth, false, SurfaceFormat.Single, ShaderAccess.ReadWrite)
        {
            this.VoxelQueryRT = vxlQry;
            
        }
        public void SetVoxelData(T[] data)
        {
            int[] packedData = new int[data.Length];
            for (int i = 0; i < packedData.Length; i++)
                packedData[i] = this.VoxelQueryRT._structMapper.GetInt32(data[i]);
            
            this.SetData<int>(packedData);

            packedData = new int[0];
        }
    }
}
