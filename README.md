# 📐 Ara 3D Geometry Toolkit

A set of components, scripts, and libraries for geometry creation and manipulation primarily in the Unity editor. 
This library is intended to help in the creation of editor tools, and for learning and experimenting 
with geometric routines. 

The code is being built for the built-in (standard) rendering pipeline.  

## ℹ️ About Ara 3D Geometry and Mathematics

The [geometry](https://github.com/ara3d/geometry) and [math](https://github.com/ara3d/mathematics) algorithms and data-structures 
are written from the ground-up in .NET Standard 2.0 compatible C#.
This makes it possible to reuse the mathematical and geometry code in other applications without any dependencies on Unity. 

The Ara 3D geometry algorithms differ from Unity libraries:

* It uses a Z-up coordinate system
* Normals are computed assuming a counter-clock-wise vertex ordering

## ⚠️ Use at your own risk

This library is not intended for use in real-time games, and might not represent best practices.  

## 🔭The Future

This Ara 3D mathematics and geometry libraries will be ported to the [Plato](https://github.com/cdiggins/plato). 
Plato is a high-performance pure functional language designed especially for mathematical computation.

We will be next writing an open-source Plato to C# compiler optimized for the Unity burst compiler. 
If your organization is interested in accelerating development, [let's talk](mailto:cdiggins@gmail.com)!

## 🌱 References

Many technical problems in developing this code were solved with help from these people and posts:

* https://github.com/noisecrime/Unity-InstancedIndirectExamples 
* https://github.com/ttvertex/Unity-InstancedIndirectExamples
* https://forum.unity.com/members/bgolus.163285/ 
* https://toqoz.fyi/thousands-of-meshes.html 


## 🔎 Related Work

Other libraries related to procedural geometry creation in Unity:

* https://github.com/Syomus/ProceduralToolkit
* https://github.com/gradientspace/geometry3Sharp
* https://github.com/keenanwoodall/Deform
* https://github.com/MonoGame/MonoGame
* https://github.com/stride3d/stride
* https://github.com/dotnet/Silk.NET 
