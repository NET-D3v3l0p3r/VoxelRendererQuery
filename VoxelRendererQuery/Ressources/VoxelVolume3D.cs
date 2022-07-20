using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelRendererQuery.Raytracer;
using VoxelRendererQuery.Raytracer.Accelerator;

namespace VoxelRendererQuery.Ressources
{
    public class VoxelVolume3D<T> : Texture3D
    {
        public enum NodeSize
        {
            Small = 16,
            Medium = 32,
            Large = 64
        }

        public VoxelRenderQuery<T> VoxelRenderQuery { get; private set; }
        public bool IsAccelerated { get; private set; }

        internal StructuredBuffer AccelerationBuffer { get; private set; }
        internal NodeSize MinimumNodeSize { get; private set; }

        private OctreeAccelerator<T> _axlr;


        public VoxelVolume3D(
            VoxelRenderQuery<T> renderQuery, 
            int width, 
            int height, 
            int depth, 
            bool accelerated  = false,
            NodeSize nodeSize = NodeSize.Medium)
            
            : base(
                  renderQuery.GraphicsDevice, 
                  width, 
                  height, 
                  depth, 
                  false, 
                  SurfaceFormat.Single, 
                  ShaderAccess.ReadWrite)
        {
            this.VoxelRenderQuery = renderQuery;
            this.IsAccelerated = accelerated;


            if (this.IsAccelerated)
            {
                if ((this.Width + this.Height + this.Depth) % 3 != 0)
                    throw new Exception("Fatal error: Acceleration structures only support cubes.");

                _axlr = new OctreeAccelerator<T>(VoxelRenderQuery, (int)nodeSize);
                _axlr.Load(Width);

                this.MinimumNodeSize = nodeSize;
            }
        }

        public void SetVoxelData(T[] data)
        {
            int[] packedData = new int[data.Length];
            for (int i = 0; i < packedData.Length; i++)
                packedData[i] = this.VoxelRenderQuery._structMapper.GetInt32(data[i]);

            this.SetData<int>(packedData);

            packedData = new int[0];

            if (IsAccelerated)
                AccelerationBuffer = _axlr.Create(this);
        }

   
    }
}
