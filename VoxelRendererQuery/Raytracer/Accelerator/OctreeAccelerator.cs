//////////////////////////////////////////// - 1 - ////////////////////////////////////////////////
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using VoxelRendererQuery.Tools;

namespace VoxelRendererQuery.Raytracer.Accelerator
{
    internal class OctreeAccelerator<T>
    {
        private VoxelRenderQuery<T> _renderQuery;
        private StructuredBuffer _octreeDataBuffer;
        private int _maxIterations;
        private int _dispatchCount;
        private Effect _acceleratorEffect;

        private int _nodeSize;

        public OctreeAccelerator(VoxelRenderQuery<T> voxelRenderQuery, int nodeSize)
        {
            _renderQuery = voxelRenderQuery;
            _acceleratorEffect = Toolkit.CompileEffect(
                _renderQuery.GraphicsDevice, 
                Properties.Resources.axlr, 
                _renderQuery.HLSLCompilerPath, 
                Toolkit.CompilerProfile.DirectX_11,
                "NSAxlr");

            _nodeSize = nodeSize;
        }

        private void _createInitialOctree()
        {
            OctreeEntry[] rwBuffer = new OctreeEntry[_octreeDataBuffer.ElementCount];

            int index = 0;

            OctreeEntry root = new();
            root.childrenStartIndex = 1;
            root.childrenCount = 0;

            rwBuffer[index++] = root;
            while (index < rwBuffer.Length)
            {
                OctreeEntry current = new()
                {
                    childrenStartIndex = index * 8 + 1,
                    childrenCount = 0,
                };
                rwBuffer[index++] = current;
            }

            _octreeDataBuffer.SetData<OctreeEntry>(rwBuffer);
            _ = Array.Empty<OctreeEntry>();

        }

        private void _createOctreeBuffer(int size)
        {
            _maxIterations = (int)Math.Ceiling(Math.Log(size / (double)_nodeSize) / Math.Log(2)); // basic algebra
            int maxSize = (int)(1 - Math.Pow(8, _maxIterations + 1)) / (1 - 8); // geometric series, q^0 + q^1 + ... + q^(n-1) = (1-q^n)/(1-q)
            _octreeDataBuffer = new StructuredBuffer(_renderQuery.GraphicsDevice, typeof(OctreeEntry), maxSize, BufferUsage.None, ShaderAccess.ReadWrite);

            _dispatchCount = (int)Math.Ceiling((double)size / 4);
        }

        public void Load(int volumeSize)
        {
            _createOctreeBuffer(volumeSize);
            _createInitialOctree();

            _acceleratorEffect.Parameters["accelerationStructureBuffer"].SetValue(_octreeDataBuffer);
            _acceleratorEffect.Parameters["volumeInitialSize"].SetValue(volumeSize);
            _acceleratorEffect.Parameters["maxDepth"].SetValue(_maxIterations);
        }


        public StructuredBuffer Create(Texture3D data)
        {
            _acceleratorEffect.Parameters["voxelDataBuffer"].SetValue(data);
            _acceleratorEffect.Techniques["AcceleratorTechnique"].Passes["GenerateOctree"].ApplyCompute();
            _renderQuery.GraphicsDevice.DispatchCompute(_dispatchCount, _dispatchCount, _dispatchCount);

            return _octreeDataBuffer;
        }

    }
}
