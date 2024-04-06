# 📐 Ara 3D Geometry Toolkit

A set of components, scripts, and libraries for geometry creation and manipulation in the Unity editor. 
This library is intended to help in the creation of editor tools, and for learning and experimenting 
with geometric routines. 

This project is built and tested using the Universal Rendering Pipeline (URP) and Unity 2021. 

## ℹ️ About Ara 3D Geometry and Mathematics

The [geometry](https://github.com/ara3d/geometry) and [math](https://github.com/ara3d/mathematics) algorithms and data-structures 
are written from the ground-up in .NET Standard 2.0 compatible C#.
This makes it possible to reuse the mathematical and geometry code in other applications without any dependencies on Unity. 

The Ara 3D geometry libraries differ from Unity in that they assume a Z-up coordinate system, and normals are computed assuming a 
counter-clock-wise ordering of vertices.

## ⚠️ Warning: Not optimized for use in real-time games

This library is not optimized. It is not intended to be used as-is in real-time games.

## 🔭The Future

This Ara 3D mathematics and geometry libraries will be ported to the [Plato](https://github.com/cdiggins/plato). 
Plato is a high-performance pure functional language designed especially for mathematical computation.

We will be next writing an open-source Plato to C# compiler optimized for the Unity burst compiler. 
If your organization is interested in accelerating development, [let's talk](mailto:cdiggins@gmail.com)!

## 🔎 Related Work

Other libraries related to procedural geometry creation in Unity:

* https://github.com/Syomus/ProceduralToolkit
* https://github.com/gradientspace/geometry3Sharp
* https://github.com/keenanwoodall/Deform
* https://github.com/MonoGame/MonoGame
* https://github.com/stride3d/stride
* https://github.com/dotnet/Silk.NET 
