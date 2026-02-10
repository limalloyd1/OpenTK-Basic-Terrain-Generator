using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace FBXModelLoaderSpace
{
    public sealed class GLMeshPN : IDisposable
    {
        private readonly int _vao;
        private readonly int _vbo;
        private readonly int _ebo;

        public int IndexCount { get; }

        public Vector3 Position { get; set; } = Vector3.Zero;
        public Vector3 Scale { get; set; } = Vector3.One;
        public Vector4 Color { get; set; } = Vector4.One;

        public GLMeshPN(float[] vertexData, uint[] indices)
        {
            IndexCount = indices.Length;

            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();
            _ebo = GL.GenBuffer();

            GL.BindVertexArray(_vao);


            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertexData.Length * sizeof(float), vertexData, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            int stride = 6 * sizeof(float);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            GL.BindVertexArray(0);
        }

        public void Draw(Shader shader)
        {
            shader.SetVector4("color", Color.X, Color.Y, Color.Z, Color.W);

            Matrix4 model = Matrix4.CreateScale(Scale) * Matrix4.CreateTranslation(Position);
            shader.SetMatrix4("model", model);

            GL.BindVertexArray(_vao);
            GL.DrawElements(PrimitiveType.Triangles, IndexCount, DrawElementsType.UnsignedInt, 0);
        }

        public void Dispose()
        {
            GL.DeleteBuffer(_vbo);
            GL.DeleteBuffer(_ebo);
            GL.DeleteVertexArray(_vao);
        }
    }
}
