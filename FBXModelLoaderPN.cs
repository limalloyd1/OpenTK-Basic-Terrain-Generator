using Assimp;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;

namespace FBXModelLoaderSpace
{
    public sealed class FBXModelLoaderPN : IDisposable
    {
        private readonly AssimpContext _ctx = new AssimpContext();

        public List<GLMeshPN> LoadToGL(
            string filePath,
            Vector3 position,
            Vector3 scale,
            Vector4 color)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Model file not found", filePath);

            var flags =
                PostProcessSteps.Triangulate |
                PostProcessSteps.JoinIdenticalVertices |
                PostProcessSteps.GenerateNormals;

            Scene scene = _ctx.ImportFile(filePath, flags);

            if (scene == null || scene.MeshCount == 0)
                throw new Exception("Failed to load model or no meshes found.");

            var result = new List<GLMeshPN>();

            foreach (var mesh in scene.Meshes)
            {
                var glMesh = new GLMeshPN(
                    BuildVertexData(mesh),
                    BuildIndexData(mesh)
                )
                {
                    Position = position,
                    Scale = scale,
                    Color = color
                };

                result.Add(glMesh);
            }

            return result;
        }

        private static float[] BuildVertexData(Mesh mesh)
        {
            float[] data = new float[mesh.VertexCount * 6];

            for (int i = 0; i < mesh.VertexCount; i++)
            {
                data[i * 6 + 0] = mesh.Vertices[i].X;
                data[i * 6 + 1] = mesh.Vertices[i].Y;
                data[i * 6 + 2] = mesh.Vertices[i].Z;

                var n = mesh.HasNormals ? mesh.Normals[i] : new Assimp.Vector3D(0, 1, 0);
                data[i * 6 + 3] = n.X;
                data[i * 6 + 4] = n.Y;
                data[i * 6 + 5] = n.Z;
            }

            return data;
        }

        private static uint[] BuildIndexData(Mesh mesh)
        {
            uint[] indices = new uint[mesh.FaceCount * 3];

            for (int i = 0; i < mesh.FaceCount; i++)
            {
                var face = mesh.Faces[i];
                indices[i * 3 + 0] = (uint)face.Indices[0];
                indices[i * 3 + 1] = (uint)face.Indices[1];
                indices[i * 3 + 2] = (uint)face.Indices[2];
            }

            return indices;
        }

        public void Dispose() => _ctx.Dispose();
    }
}
