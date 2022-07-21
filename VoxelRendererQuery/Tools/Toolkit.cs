using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
namespace VoxelRendererQuery.Tools
{
    public static class Toolkit
    {
        public enum CompilerProfile
        {
            DirectX_11,
            OpenGL,
            PlayStation4,
            XboxOne,
            Switch
        }

        public static Random RAND = new Random();

        public static Effect CompileEffect(
            GraphicsDevice graphicsDevice, 
            string effectSource, 
            string effectCompilerPath, 
            CompilerProfile compilerProfile,
            string customName = "unnamed")
        {
            string fileName = "f_effect_" + RAND.Next(1_000_000, 9_999_999);

            Process compilerProcess = new Process();
            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = effectCompilerPath;

            File.WriteAllText(Path.GetTempPath() + fileName, effectSource);

            processStartInfo.Arguments = '"' + Path.GetTempPath() + fileName + '"' + " " + '"' + Path.GetTempPath() + fileName + ".byte" + '"' + " /Profile:" + compilerProfile.ToString();
            processStartInfo.CreateNoWindow = true;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;

            compilerProcess.StartInfo = processStartInfo;

            compilerProcess.Start();
            compilerProcess.WaitForExit();

            Effect effect = null;

            File.Delete(Path.GetTempPath() + fileName);

            try
            {
                var bytecode = File.ReadAllBytes(Path.GetTempPath() + fileName + ".byte");
                effect = new Effect(graphicsDevice, bytecode);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Compiled effect: " + customName);
                Console.ForegroundColor = ConsoleColor.Gray;

                File.Delete(Path.GetTempPath() + fileName + ".byte");
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error compiling: " + compilerProcess.StandardError.ReadToEnd());
                Console.ForegroundColor = ConsoleColor.Gray;
            }


            return effect;
        }

    }
}
