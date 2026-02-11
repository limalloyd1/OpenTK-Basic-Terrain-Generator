using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Assimp;
using Assimp.Configs;
using System;
using System.Collections.Generic;
using FBXModelLoaderSpace;
using GameSpace;

namespace BasicTerrainGen
{
    public class FBXModelRenderer : IDisposable
    {
        private GLMeshPN mesh;  // Single combined mesh
        
        public Vector3 Position 
        { 
            get => mesh.Position;
            set => mesh.Position = value;
        }
        
        public Vector3 Scale 
        { 
            get => mesh.Scale;
            set => mesh.Scale = value;
        }
        
        public Vector4 Color 
        { 
            get => mesh.Color;
            set => mesh.Color = value;
        }
        
        private bool disposed = false;

        /// <summary>
        /// Creates a new FBX model renderer
        /// </summary>
        /// <param name="filePath">Path to the FBX file</param>
        /// <param name="position">Position in world space</param>
        /// <param name="scale">Scale factor (uniform or per-axis)</param>
        /// <param name="color">RGBA color (0-1 range)</param>
        public FBXModelRenderer(string filePath, Vector3 position, Vector3 scale, Vector4 color)
        {
            LoadModel(filePath);
            this.Position = position;
            this.Scale = scale;
            this.Color = color;
        }

        /// <summary>
        /// Overload with uniform scale and RGB color (alpha defaults to 1.0)
        /// </summary>
        public FBXModelRenderer(string filePath, Vector3 position, float uniformScale, Vector3 colorRGB)
            : this(filePath, position, new Vector3(uniformScale), new Vector4(colorRGB.X, colorRGB.Y, colorRGB.Z, 1.0f))
        {
        }

        /// <summary>
        /// Overload with uniform scale and RGBA color
        /// </summary>
        public FBXModelRenderer(string filePath, Vector3 position, float uniformScale, Vector4 color)
            : this(filePath, position, new Vector3(uniformScale), color)
        {
        }

        private void LoadModel(string filePath)
        {
            AssimpContext importer = new AssimpContext();
            
            // Configure the importer
            importer.SetConfig(new NormalSmoothingAngleConfig(66.0f));
            
            // Import the scene with specific post-processing flags
            Scene scene = importer.ImportFile(filePath,
                PostProcessSteps.Triangulate |
                PostProcessSteps.GenerateNormals |
                PostProcessSteps.FlipUVs |
                PostProcessSteps.JoinIdenticalVertices |
                PostProcessSteps.OptimizeMeshes);

            if (scene == null || scene.SceneFlags == SceneFlags.Incomplete || scene.RootNode == null)
            {
                throw new Exception($"Failed to load model: {filePath}");
            }

            // Combine all meshes into one
            List<float> allVertices = new List<float>();
            List<uint> allIndices = new List<uint>();
            uint indexOffset = 0;

            ProcessNode(scene.RootNode, scene, allVertices, allIndices, ref indexOffset);
            
            // Create the single GLMeshPN
            mesh = new GLMeshPN(allVertices.ToArray(), allIndices.ToArray());
            
            importer.Dispose();
        }

        private void ProcessNode(Node node, Scene scene, List<float> allVertices, List<uint> allIndices, ref uint indexOffset)
        {
            // Process all meshes in this node
            for (int i = 0; i < node.MeshCount; i++)
            {
                Mesh assMesh = scene.Meshes[node.MeshIndices[i]];
                ProcessMesh(assMesh, allVertices, allIndices, ref indexOffset);
            }

            // Recursively process child nodes
            for (int i = 0; i < node.ChildCount; i++)
            {
                ProcessNode(node.Children[i], scene, allVertices, allIndices, ref indexOffset);
            }
        }

        private void ProcessMesh(Mesh assMesh, List<float> allVertices, List<uint> allIndices, ref uint indexOffset)
        {
            // Extract vertex data (position and normals)
            for (int i = 0; i < assMesh.VertexCount; i++)
            {
                // Position
                allVertices.Add(assMesh.Vertices[i].X);
                allVertices.Add(assMesh.Vertices[i].Y);
                allVertices.Add(assMesh.Vertices[i].Z);

                // Normal
                if (assMesh.HasNormals)
                {
                    allVertices.Add(assMesh.Normals[i].X);
                    allVertices.Add(assMesh.Normals[i].Y);
                    allVertices.Add(assMesh.Normals[i].Z);
                }
                else
                {
                    // Default normal pointing up if none exist
                    allVertices.Add(0.0f);
                    allVertices.Add(1.0f);
                    allVertices.Add(0.0f);
                }
            }

            // Extract indices with offset
            for (int i = 0; i < assMesh.FaceCount; i++)
            {
                Face face = assMesh.Faces[i];
                for (int j = 0; j < face.IndexCount; j++)
                {
                    allIndices.Add((uint)face.Indices[j] + indexOffset);
                }
            }

            // Update offset for next mesh
            indexOffset += (uint)assMesh.VertexCount;
        }

        /// <summary>
        /// Draw the model using your shader
        /// </summary>
        public void Draw(Shader shader)
        {
            mesh.Draw(shader);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    mesh?.Dispose();
                }
                disposed = true;
            }
        }

        ~FBXModelRenderer()
        {
            Dispose(false);
        }
    }
}