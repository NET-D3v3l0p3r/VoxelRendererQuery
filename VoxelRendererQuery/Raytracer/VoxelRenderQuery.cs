using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelRendererQuery.Ressources;
using VoxelRendererQuery.Tools;
using VoxelRendererQuery.Transpiler;
using VoxelRendererQuery.Transpiler.Meta;
using VoxelRendererQuery.Transpiler.Tokenizer;
using static VoxelRendererQuery.Tools.Toolkit;

namespace VoxelRendererQuery.Raytracer
{
    /// <summary>
    /// Raytracer based on https://nbn-resolving.org/urn:nbn:de:bsz:960-opus4-22505
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class VoxelRenderQuery<T>
    {
        private bool _dirty;

        private Vector4[] _positionLookUps;
        private Texture2D _positionLookUp;

        private Effect _raytracer;
        private Texture2D _backbuffer;
        private VoxelVolume3D<T> _volume;

        internal HLSLStructMapper<T> StructMapper { get; private set; }


        public GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// Volume to render.
        /// </summary>
        public VoxelVolume3D<T> Volume { get { return _volume; } set { _volume = value; _dirty = true; } }

        /// <summary>
        /// Default is version 3.8.1.2.
        /// </summary>
        public string HLSLCompilerPath { get; set; }
        /// <summary>
        /// RTvoXel-specific source code to transpile to hlsl.
        /// </summary>
        public string NHLSLSource { get; set; }

        public EffectParameterCollection Parameters { get { return _raytracer.Parameters; } } // risky

        public VoxelRenderQuery(GraphicsDevice graphicsDevice)
        {
            this.GraphicsDevice = graphicsDevice;

            StructMapper = new HLSLStructMapper<T>();

            _backbuffer = new Texture2D(
                GraphicsDevice,
                GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height,
                false,
                SurfaceFormat.Color,
                ShaderAccess.ReadWrite);


            this.HLSLCompilerPath = @"C:\Users\" + Environment.UserName + @"\.nuget\packages\monogame.content.builder.task.compute\3.8.1.2\tools\net5.0\any\mgfxc.exe";

            _dirty = true;

            _positionLookUps = new Vector4[]
               {
                new Vector4(0, 0, 0, 1),
                new Vector4(1, 0, 0, 1),
                new Vector4(0, 0, 1, 1),
                new Vector4(1, 0, 1, 1),


                new Vector4(0, 1, 0, 1),
                new Vector4(1, 1, 0, 1),
                new Vector4(0, 1, 1, 1),
                new Vector4(1, 1, 1, 1),
               };

            _positionLookUp = new Texture2D(this.GraphicsDevice, 8, 1, false, SurfaceFormat.Vector4);
            _positionLookUp.SetData<Vector4>(_positionLookUps);

        }


        /// <summary>
        /// Compile raytracer.
        /// </summary>
        /// <param name="compilerProfile"></param>
        public void Compile(CompilerProfile compilerProfile)
        {
            var hlsl_src = VoxelRendererQuery.Properties.Resources.raytracer;

            var hlsl_struct = StructMapper.GenerateHLSLStruct();
            var hlsl_conver = StructMapper.GenerateHLSLConverter();

            var final_src = new StringBuilder();
            var tokens = NHLSLTokenizer.Default().Run(NHLSLSource);

            NHLSLTranspiler transpiler = new NHLSLTranspiler(tokens.GetEnumerator());
            transpiler.Run();
            var intermediateSource = transpiler.Transpile();

            final_src = final_src
                .AppendLine(hlsl_struct)
                .AppendLine(hlsl_conver)
                .AppendLine(hlsl_src
                    .Replace("$$$VOXEL$$$", StructMapper.StructName)
                    .Replace("$$$RAY_GEN$$$", transpiler.RayGen)
                    .Replace("$$$VOXEL_SHADER$$$", transpiler.VoxelShader)
                    .Replace("$$$ENTRY$$$", transpiler.MainEntry)
                    .Replace("$$$__ROUTINE__$$$", intermediateSource));


            var totalSource = final_src.ToString();

            _raytracer = Toolkit.CompileEffect(
                GraphicsDevice, 
                totalSource, 
                HLSLCompilerPath, 
                compilerProfile,
                "NSRaytracer");

            _raytracer.Parameters["backBuffer"].SetValue(_backbuffer);
        }

        /// <summary>
        /// Draw volume specified in Volume-Property.
        /// </summary>
        /// <param name="sbatch"></param>
        public void Draw(SpriteBatch sbatch)
        {
            if (_dirty && Volume.IsAccelerated)
            {
                _raytracer.Parameters["nodeMinimumSize"].SetValue((int)Volume.MinimumNodeSize);
                _raytracer.Parameters["octantVectorLookUp"].SetValue(_positionLookUp);
                _raytracer.Parameters["accelerationStructureBuffer"].SetValue(Volume.AccelerationBuffer);
                _raytracer.Parameters["useAccelerator"].SetValue(Volume.IsAccelerated);

                _dirty = false;
            }


            _raytracer.Parameters["volumeInitialSize"].SetValue(Volume.Width);
            _raytracer.Parameters["voxelDataBuffer"].SetValue(Volume);

            _raytracer.CurrentTechnique.Passes[0].ApplyCompute();

            GraphicsDevice.DispatchCompute(
                (int)MathF.Ceiling(GraphicsDevice.Viewport.Width / 8),
                (int)MathF.Ceiling(GraphicsDevice.Viewport.Height / 8), 1);


            sbatch.Draw(_backbuffer, new Vector2(0, 0), Color.White);
        }



    }
}
