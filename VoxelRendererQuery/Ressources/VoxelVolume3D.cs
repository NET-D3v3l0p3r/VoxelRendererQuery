using Microsoft.Xna.Framework;
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

        private Matrix _rotMat;
        private Vector3 _rotation;
        public Vector3 Rotation
        {
            get
            {
                return _rotation;
            }
            set
            {
                _rotation = value;
                
                float angle = _rotation.Length();
                if(angle == 0)
                {
                    _rotMat = Matrix.Identity;
                    return;
                }

                Vector3 normalizedRotationAxis = new Vector3(_rotation.X, _rotation.Y, _rotation.Z);
                normalizedRotationAxis.Normalize();

                _rotMat = Matrix.CreateFromAxisAngle(normalizedRotationAxis, angle);

                VoxelRenderQuery.Parameters["srMatrix"].SetValue(Matrix.Invert(_scaleMat * _rotMat));
            }
        }

        private Matrix _scaleMat;
        private Vector3 _scale;
        public Vector3 Scale
        {
            get
            {
                return _scale;
            }
            set
            {
                _scale = value;
                _scaleMat = Matrix.CreateScale(_scale);

                VoxelRenderQuery.Parameters["srMatrix"].SetValue(Matrix.Invert(_scaleMat * _rotMat));
            }
        }

        private Matrix _translationMat;
        private Vector3 _translation;

        public Vector3 Translation
        {
            get 
            {
                return _translation;
            }

            set
            {
                _translation = value;
                _translationMat = Matrix.CreateTranslation(-_translation);
                VoxelRenderQuery.Parameters["translationMatrix"].SetValue(_translationMat);
            }
        }

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


            _rotMat = Matrix.Identity;
            _scaleMat = Matrix.Identity;
            _translationMat = Matrix.Identity;

            Rotation = Vector3.Zero;
            Scale = new Vector3(1, 1, 1);
            Translation = Vector3.Zero;
        }

        public void SetVoxelData(T[] data)
        {
            int[] packedData = new int[data.Length];
            for (int i = 0; i < packedData.Length; i++)
                packedData[i] = this.VoxelRenderQuery.StructMapper.GetInt32(data[i]);

            this.SetData<int>(packedData);

            packedData = new int[0];

            if (IsAccelerated)
                AccelerationBuffer = _axlr.Create(this);
        }

   
    }
}
