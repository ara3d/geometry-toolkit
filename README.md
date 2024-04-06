# 📐 Ara 3D Geometry Toolkit

A set of components, scripts, and libraries for geometry creation and manipulation for use in Unity. 
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
We recommend using it to test and debug geometry algorithms, and build editor tools.  

## 🔎 Related Work

Other libraries related to procedural geometry creation in Unity:

* https://github.com/Syomus/ProceduralToolkit
* https://github.com/gradientspace/geometry3Sharp
* https://github.com/keenanwoodall/Deform
* https://github.com/MonoGame/MonoGame
* https://github.com/stride3d/stride
* https://github.com/dotnet/Silk.NET 
