﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Dieser Code wurde von einem Tool generiert.
//     Laufzeitversion:4.0.30319.42000
//
//     Änderungen an dieser Datei können falsches Verhalten verursachen und gehen verloren, wenn
//     der Code erneut generiert wird.
// </auto-generated>
//------------------------------------------------------------------------------

namespace VoxelRendererQuery.Properties {
    using System;
    
    
    /// <summary>
    ///   Eine stark typisierte Ressourcenklasse zum Suchen von lokalisierten Zeichenfolgen usw.
    /// </summary>
    // Diese Klasse wurde von der StronglyTypedResourceBuilder automatisch generiert
    // -Klasse über ein Tool wie ResGen oder Visual Studio automatisch generiert.
    // Um einen Member hinzuzufügen oder zu entfernen, bearbeiten Sie die .ResX-Datei und führen dann ResGen
    // mit der /str-Option erneut aus, oder Sie erstellen Ihr VS-Projekt neu.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Gibt die zwischengespeicherte ResourceManager-Instanz zurück, die von dieser Klasse verwendet wird.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("VoxelRendererQuery.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Überschreibt die CurrentUICulture-Eigenschaft des aktuellen Threads für alle
        ///   Ressourcenzuordnungen, die diese stark typisierte Ressourcenklasse verwenden.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die uniform int volumeInitialSize;
        ///
        ///uniform RWTexture2D&lt;float4&gt; backBuffer;
        ///
        ///
        ///globallycoherent RWTexture3D&lt;int&gt; voxelDataBuffer;
        ///int getData(int3 position)
        ///{
        ///    return voxelDataBuffer[position];
        ///}
        ///
        ///
        ///struct Ray
        ///{
        ///    float3 origin;
        ///    float3 dir;
        ///    float3 dirRcp;
        ///    
        ///};
        ///
        ///struct AABB
        ///{
        ///    float3 center;
        ///    float3 maxSize;
        ///};
        ///AABB createAABB(float3 center, float3 max)
        ///{
        ///    AABB aabb = (AABB) 0;
        ///    aabb.center = center;
        ///    aabb.maxSize = max;
        ///    return aabb;
        ///}
        ///
        ///bool check [Rest der Zeichenfolge wurde abgeschnitten]&quot;; ähnelt.
        /// </summary>
        internal static string raytracer {
            get {
                return ResourceManager.GetString("raytracer", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die $$$RTRCRESULT$$$ $$$VOLUME_RAYTEST$$$(Ray ray)
        ///{
        ///    float depth = 0;
        ///    float3 hitPoint = (float3) 0;
        ///    float3 hitPointInt = (float3) 0;
        ///    float3 normal = (float3) 0;
        ///    AABB voxelAABB = (AABB) 0;
        ///    AABB voxelAABBLast = (AABB) 0;
        ///    int voxelData = 0;
        ///    int iterations = 0;
        ///                        
        ///    bool result = arrayRayHit(
        ///                        ray,
        ///                        createAABB(float3(0, 0, 0), float3(volumeInitialSize, volumeInitialSize, volumeInitialSize)),
        ///          [Rest der Zeichenfolge wurde abgeschnitten]&quot;; ähnelt.
        /// </summary>
        internal static string volumeRayTest {
            get {
                return ResourceManager.GetString("volumeRayTest", resourceCulture);
            }
        }
    }
}
