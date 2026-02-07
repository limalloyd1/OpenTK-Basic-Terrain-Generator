using System;
using System.IO;
using OpenTK;
using GameSpace;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;

namespace CubeRenderer
{
	public class Cube
	{
		public int _squareVBO { get; private set; }
		public int _squareVAO { get; private set; }
		public int _squareEBO { get; private set; }
		public int IndexCount { get; private set; }

		public Vector3 Position { get; set; }
		public Vector3 Scale { get; set; }
		public Vector4 Color { get; set; }

		public Cube(Vector3 position, Vector3 scale, Vector4 color)
		{
			Position = position;
			Scale = scale;
			Color = color;

			float[] vertices = {
				-0.2f, -0.4f, 0.2f,
				0.2f, -0.4f, 0.2f,
				0.2f, 0.4f, 0.2f,
				-0.2f, 0.4f, 0.2f, 

				-0.2f, -0.4f, -0.2f,
				-0.2f, 0.4f, -0.2f,
				0.2f, 0.4f, -0.2f,
				0.2f, -0.4f, 0.2f,
				
				-0.2f, 0.4f, -0.2f,
				-0.2f, 0.4f, 0.2f,
				0.2f, 0.4f, 0.2f,
				0.2f, 0.4f, -0.2f,

				-0.2f, -0.4f, -0.2f,
				0.2f, -0.4f, -0.2f, 
				0.2f, -0.4f, 0.2f,
				-0.2f, -0.4f, 0.2f,

				0.2f, -0.4f, -0.2f,
			       	0.2f, 0.4f, -0.2f,
				0.2f, 0.4f, 0.2f,
				0.2f, -0.4f, 0.2f,
				
				-0.2f, -0.4f, -0.2f,
				-0.2f, -0.4f, 0.2f,
				-0.2f, 0.4f, 0.2f, 
				-0.2f, 0.4f, -0.2f	

			};

			uint[] indices =
			{
				0, 1, 2, 2, 3, 0,       
				4, 5, 6, 6, 7, 4,
				8, 9, 10, 10, 11, 8,    
				12, 13, 14, 14, 15, 12, 
				16, 17, 18, 18, 19, 16, 
				20, 21, 22, 22, 23, 20  
			};

			IndexCount = indices.Length;

			_squareVAO = GL.GenVertexArray(); // Set Array Obj
			GL.BindVertexArray(_squareVAO);

			_squareVBO = GL.GenBuffer(); // Set Buffer Obj
			GL.BindBuffer(BufferTarget.ArrayBuffer, _squareVBO);
			GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

			_squareEBO = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, _squareEBO);
			GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

			GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float,false, 3 * sizeof(float), 0);
			GL.EnableVertexArrayAttrib(_squareVAO, 0);
		}

		public void Draw(Shader shader)
        {
            // Set Color
			shader.SetVector4("color", Color.X, Color.Y, Color.Z, Color.W);

			// Set model matrix
			Matrix4 model = Matrix4.CreateScale(Scale) * Matrix4.CreateTranslation(Position);
			shader.SetMatrix4("model", model);

			//Draw
			GL.BindVertexArray(_squareVAO);
			GL.DrawElements(PrimitiveType.Triangles, IndexCount, DrawElementsType.UnsignedInt, 0);
        }

		public void Dispose()
        {
            GL.DeleteBuffer(_squareVBO);
			GL.DeleteBuffer(_squareEBO);
			GL.DeleteVertexArray(_squareVAO);

        }

	}	
}
