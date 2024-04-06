# 📐 Ara 3D Geometry Toolkit

A set of components, scripts, and libraries for geometry creation and manipulation for use in Unity.

The Ara 3D Geometry Toolkit is built on top of the following libraries:
* [Ara3D.Mathematics](https://github.com/ara3d/mathematics)
* [Ara3D.Geometry](https://github.com/ara3d/geometry)
* [Ara3D.Graphics](https://github.com/ara3d/graphics)
* [Ara3D.Collections](https://github.com/ara3d/mathematics)
* [Ara3D.Utils](https://github.com/ara3d/utils)

This project is built and tested using the Universal Rendering Pipeline (URP) and Unity 2021. 

## ⚠️ Warning: Not for use in real-time games! 

This library is not optimized. It is not intended to be used as-is in real-time games.
We are using it to test and debug geometry algorithms. 

## ℹ️ About Ara 3D Geometry 

The geometry algorithms and data-structures in Ara3D.Geeometry are written from the ground-up in .NET Standard 2.0 compatible C#.
This makes it possible to reuse the mathematical and geometry code in other applications without any dependencies on Unity. 

The Ara 3D libraries differ from Unity in that they assume a Z-up coordinate system, and normals are computed assuming a counter-clock-wise ordering of vertices.

## 🔎 Related Work

Other libraries related to procedural geometry creation in Unity:

* https://github.com/Syomus/ProceduralToolkit
* https://github.com/gradientspace/geometry3Sharp
* https://github.com/keenanwoodall/Deform
  

  
