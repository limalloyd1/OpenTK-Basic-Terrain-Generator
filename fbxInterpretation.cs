using Assimp;
using System;
using System.Collections.Generic;
using OpenTK.Mathematics;

namespace FBXModelLoaderSpace
{
    public class FBXModelLoader
    {
        private AssimpContext _assimpContext;

        public FBXModelLoader()
        {
            _assimpContext = new AssimpContext();
        }

        /// Loads an FBX file and returns the scene with mesh data
        public Scene LoadFBX(string filePath)
        {
            try
            {
                // Load scene with post-processing flags
                Scene scene = _assimpContext.ImportFile(filePath, 
                    PostProcessSteps.Triangulate | 
                    PostProcessSteps.GenerateNormals | 
                    PostProcessSteps.GenerateSmoothNormals | 
                    PostProcessSteps.JoinIdenticalVertices);

                if (scene == null || scene.MeshCount == 0)
                {
                    throw new Exception("Failed to load FBX or no meshes found");
                }

                Console.WriteLine($"Successfully loaded FBX with {scene.MeshCount} mesh(es)");
                return scene;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading FBX: {ex.Message}");
                return null;
            }
        }

        /// Extracts vertex data from a mesh (positions, indices, normals)
        public void ExtractMeshData(Mesh mesh, out float[] vertices, out uint[] indices, out float[] normals)
        {
            try
            {
                // Extract vertex positions
                vertices = new float[mesh.VertexCount * 3];
                for (int i = 0; i < mesh.VertexCount; i++)
                {
                    vertices[i * 3] = mesh.Vertices[i].X;
                    vertices[i * 3 + 1] = mesh.Vertices[i].Y;
                    vertices[i * 3 + 2] = mesh.Vertices[i].Z;
                }

                // Extract face indices
                indices = new uint[mesh.FaceCount * 3];
                for (int i = 0; i < mesh.FaceCount; i++)
                {
                    Face face = mesh.Faces[i];
                    for (int j = 0; j < 3; j++)
                    {
                        indices[i * 3 + j] = (uint)face.Indices[j];
                    }
                }

                // Extract normals
                normals = new float[mesh.VertexCount * 3];
                for (int i = 0; i < mesh.VertexCount; i++)
                {
                    normals[i * 3] = mesh.Normals[i].X;
                    normals[i * 3 + 1] = mesh.Normals[i].Y;
                    normals[i * 3 + 2] = mesh.Normals[i].Z;
                }

                Console.WriteLine("ExtractMeshData completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in ExtractMeshData: {ex.Message}");
                vertices = null;
                indices = null;
                normals = null;
                throw;
            }
        }

        /// Extracts UV coordinates (texture coordinates) from a mesh
        public void ExtractUVData(Mesh mesh, out float[] uvCoordinates)
        {
            try
            {
                uvCoordinates = new float[mesh.VertexCount * 2];

                if (mesh.HasTextureCoords(0))
                {
                    for (int i = 0; i < mesh.VertexCount; i++)
                    {
                        Vector3D uv = mesh.TextureCoordinateChannels[0][i];
                        uvCoordinates[i * 2] = uv.X;
                        uvCoordinates[i * 2 + 1] = uv.Y;
                    }
                    Console.WriteLine("ExtractUVData completed successfully");
                }
                else
                {
                    Console.WriteLine("Warning: Mesh has no UV coordinates");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in ExtractUVData: {ex.Message}");
                uvCoordinates = null;
                throw;
            }
        }

        /// Prints mesh information for debugging/inspection
        public void PrintMeshInfo(Scene scene)
        {
            try
            {
                for (int i = 0; i < scene.MeshCount; i++)
                {
                    Mesh mesh = scene.Meshes[i];
                    Console.WriteLine($"\nMesh {i}: {mesh.Name}");
                    Console.WriteLine($"  Vertices: {mesh.VertexCount}");
                    Console.WriteLine($"  Faces: {mesh.FaceCount}");
                    Console.WriteLine($"  Has Normals: {mesh.HasNormals}");
                    Console.WriteLine($"  Has Texture Coords: {mesh.HasTextureCoords(0)}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in PrintMeshInfo: {ex.Message}");
            }
        }

        /// Cleans up resources
        public void Dispose()
        {
            _assimpContext?.Dispose();
        }
    }
}