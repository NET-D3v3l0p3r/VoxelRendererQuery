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

using VoxelRendererQuery.Transpiler;
using VoxelRendererQuery.Transpiler.Meta;
using VoxelRendererQuery.Transpiler.Tokenizer;

namespace VoxelRendererQuery.Raytracer
{
    /// <summary>
    /// Raytracer based on https://nbn-resolving.org/urn:nbn:de:bsz:960-opus4-22505
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RTVoxelQuery<T>
    {
        public GraphicsDevice GraphicsDevice { get; private set; }

        internal HLSLStructMapper<T> _structMapper { get; private set; }

        private Effect _raytracer;
        private List<Texture3D> _gpuVoxelData;

        private Texture2D _backbuffer;

        /// <summary>
        /// Volume to render.
        /// </summary>
        public VoxelVolume3D<T> Volume { get; set; }

        /// <summary>
        /// Default is version 3.8.1.2.
        /// </summary>
        public string HLSLCompilerSource { get; set; }

        /// <summary>
        /// RTvoXel-specific source code to transpile to hlsl.
        /// </summary>
        public string RTvoXelSource { get; set; }

        public EffectParameterCollection Parameters { get { return _raytracer.Parameters; } }

        public RTVoxelQuery(GraphicsDevice graphicsDevice, ContentManager content)
        {
            this.GraphicsDevice = graphicsDevice;
            this._structMapper = new HLSLStructMapper<T>();


            this._gpuVoxelData = new List<Texture3D>();


            _backbuffer = new Texture2D(
                GraphicsDevice,
                GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height,
                false,
                SurfaceFormat.Color,
                ShaderAccess.ReadWrite);


            this.HLSLCompilerSource = @"C:\Users\" + Environment.UserName + @"\.nuget\packages\monogame.content.builder.task.compute\3.8.1.2\tools\net5.0\any\mgfxc.exe";

        }

        public enum CompilerProfile
        {
            DirectX_11,
            OpenGL,
            PlayStation4,
            XboxOne,
            Switch
        }

        /// <summary>
        /// Compile raytracer.
        /// </summary>
        /// <param name="profile"></param>
        public void Compile(CompilerProfile profile)
        {

            var hlsl_src = VoxelRendererQuery.Properties.Resources.raytracer;

            var hlsl_struct = _structMapper.GenerateHLSLStruct();
            var hlsl_conver = _structMapper.GenerateHLSLConverter();

            var final_src = new StringBuilder();
            var tokens = NHLSLTokenizer.Default().Run(RTvoXelSource);

            NHLSLTranspiler transpiler = new NHLSLTranspiler(tokens.GetEnumerator());
            transpiler.Run();
            var intermediateSource = transpiler.Transpile();

            final_src = final_src
                .AppendLine(hlsl_struct)
                .AppendLine(hlsl_conver)
                .AppendLine(hlsl_src
                    .Replace("$$$VOXEL$$$", _structMapper.StructName)
                    .Replace("$$$RAY_GEN$$$", transpiler.RayGen)
                    .Replace("$$$VOXEL_SHADER$$$", transpiler.VoxelShader)
                    .Replace("$$$ENTRY$$$", transpiler.MainEntry)
                    .Replace("$$$__ROUTINE__$$$", intermediateSource));


            var totalSource = final_src.ToString();

            Process compilerProcess = new Process();
            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = HLSLCompilerSource;

            File.WriteAllText(Path.GetTempPath() + "nsrtxshdr.dat", totalSource);

            processStartInfo.Arguments = '"' + Path.GetTempPath() + "nsrtxshdr.dat" + '"' + " " + '"' + Path.GetTempPath() + "nsrtxshdr.byte" + '"' + " /Profile:" + profile.ToString();
            processStartInfo.CreateNoWindow = true;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;

            compilerProcess.StartInfo = processStartInfo;
            
            
            compilerProcess.Start();
            compilerProcess.WaitForExit();

            //File.Delete(Path.GetTempPath() + "nsrtxshdr.dat");

            try
            {             
                var bytecode = File.ReadAllBytes(Path.GetTempPath() + "nsrtxshdr.byte");
                _raytracer = new Effect(GraphicsDevice, bytecode);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Compiled.");
                Console.ForegroundColor = ConsoleColor.Gray;


                _raytracer.Parameters["backBuffer"].SetValue(_backbuffer);
                File.Delete(Path.GetTempPath() + "nsrtxshdr.byte");
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error compiling: " + compilerProcess.StandardError.ReadToEnd());
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        /// <summary>
        /// Draw volume specified in Volume-Property.
        /// </summary>
        /// <param name="sbatch"></param>
        public void Draw(SpriteBatch sbatch)
        {
            _raytracer.Parameters["volumeInitialSize"].SetValue(Volume.Width);
            _raytracer.Parameters["voxelDataBuffer"].SetValue(Volume);

            _raytracer.CurrentTechnique.Passes[0].ApplyCompute();

            GraphicsDevice.DispatchCompute(
                (int)MathF.Ceiling(GraphicsDevice.Viewport.Width / 8),
                (int)MathF.Ceiling(GraphicsDevice.Viewport.Height / 8), 1);


            sbatch.Begin();
            sbatch.Draw(_backbuffer, new Vector2(0, 0), Color.White);
            sbatch.End();
        }



    }
}
