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

			float[] vertices = 
			{
				// Front face (normal pointing toward +Z)
				-0.2f, -0.4f,  0.2f,   0.0f,  0.0f,  1.0f,
				0.2f, -0.4f,  0.2f,   0.0f,  0.0f,  1.0f,
				0.2f,  0.4f,  0.2f,   0.0f,  0.0f,  1.0f,
				-0.2f,  0.4f,  0.2f,   0.0f,  0.0f,  1.0f,
				
				// Back face (normal pointing toward -Z)
				-0.2f, -0.4f, -0.2f,   0.0f,  0.0f, -1.0f,
				0.2f, -0.4f, -0.2f,   0.0f,  0.0f, -1.0f,
				0.2f,  0.4f, -0.2f,   0.0f,  0.0f, -1.0f,
				-0.2f,  0.4f, -0.2f,   0.0f,  0.0f, -1.0f,
				
				// Left face (normal pointing toward -X)
				-0.2f, -0.4f, -0.2f,  -1.0f,  0.0f,  0.0f,
				-0.2f, -0.4f,  0.2f,  -1.0f,  0.0f,  0.0f,
				-0.2f,  0.4f,  0.2f,  -1.0f,  0.0f,  0.0f,
				-0.2f,  0.4f, -0.2f,  -1.0f,  0.0f,  0.0f,
				
				// Right face (normal pointing toward +X)
				0.2f, -0.4f, -0.2f,   1.0f,  0.0f,  0.0f,
				0.2f, -0.4f,  0.2f,   1.0f,  0.0f,  0.0f,
				0.2f,  0.4f,  0.2f,   1.0f,  0.0f,  0.0f,
				0.2f,  0.4f, -0.2f,   1.0f,  0.0f,  0.0f,
				
				// Top face (normal pointing toward +Y)
				-0.2f,  0.4f, -0.2f,   0.0f,  1.0f,  0.0f,
				-0.2f,  0.4f,  0.2f,   0.0f,  1.0f,  0.0f,
				0.2f,  0.4f,  0.2f,   0.0f,  1.0f,  0.0f,
				0.2f,  0.4f, -0.2f,   0.0f,  1.0f,  0.0f,
				
				// Bottom face (normal pointing toward -Y)
				-0.2f, -0.4f, -0.2f,   0.0f, -1.0f,  0.0f,
				-0.2f, -0.4f,  0.2f,   0.0f, -1.0f,  0.0f,
				0.2f, -0.4f,  0.2f,   0.0f, -1.0f,  0.0f,
				0.2f, -0.4f, -0.2f,   0.0f, -1.0f,  0.0f,
			};

			uint[] indices = 
			{
				// Front face
				0, 1, 2,
				2, 3, 0,
				
				// Back face
				4, 5, 6,
				6, 7, 4,
				
				// Left face
				8, 9, 10,
				10, 11, 8,
				
				// Right face
				12, 13, 14,
				14, 15, 12,
				
				// Top face
				16, 17, 18,
				18, 19, 16,
				
				// Bottom face
				20, 21, 22,
				22, 23, 20
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

			GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float,false, 6 * sizeof(float), 0);
			GL.EnableVertexAttribArray(0);

			GL.VertexAttribPointer(1,3, VertexAttribPointerType.Float,false, 6 * sizeof(float), 3 * sizeof(float)); 
			GL.EnableVertexAttribArray(1);
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

	public class Pyramid : IDisposable
	{
		public int _vbo { get; private set; }
		public int _vao { get; private set; }
		public int _ebo { get; private set; }
		public int IndexCount { get; private set; }

		public Vector3 Position { get; set; }
		public Vector3 Scale { get; set; }
		public Vector4 Color { get; set; }

		public Pyramid(Vector3 position, Vector3 scale, Vector4 color)
		{
			Position = position;
			Scale = scale;
			Color = color;

			// Geometry: square base on y = -0.4, apex at y = +0.4
			Vector3 b0 = new Vector3(-0.2f, -0.4f,  0.2f); // front-left
			Vector3 b1 = new Vector3( 0.2f, -0.4f,  0.2f); // front-right
			Vector3 b2 = new Vector3( 0.2f, -0.4f, -0.2f); // back-right
			Vector3 b3 = new Vector3(-0.2f, -0.4f, -0.2f); // back-left
			Vector3 apex = new Vector3(0.0f, 0.4f, 0.0f);

			// Helper to compute a triangle normal (CCW winding)
			static Vector3 FaceNormal(Vector3 a, Vector3 b, Vector3 c)
			{
				var ab = b - a;
				var ac = c - a;
				return Vector3.Normalize(Vector3.Cross(ab, ac));
			}

			// We duplicate vertices so each face gets a constant normal (flat shading)
			Vector3 nFront = FaceNormal(b0, b1, apex);
			Vector3 nRight = FaceNormal(b1, b2, apex);
			Vector3 nBack  = FaceNormal(b2, b3, apex);
			Vector3 nLeft  = FaceNormal(b3, b0, apex);
			Vector3 nDown  = new Vector3(0f, -1f, 0f);

			float[] vertices =
			{
				// Side faces (each is 3 verts)
				// Front (b0, b1, apex)
				b0.X,b0.Y,b0.Z,  nFront.X,nFront.Y,nFront.Z,
				b1.X,b1.Y,b1.Z,  nFront.X,nFront.Y,nFront.Z,
				apex.X,apex.Y,apex.Z, nFront.X,nFront.Y,nFront.Z,

				// Right (b1, b2, apex)
				b1.X,b1.Y,b1.Z,  nRight.X,nRight.Y,nRight.Z,
				b2.X,b2.Y,b2.Z,  nRight.X,nRight.Y,nRight.Z,
				apex.X,apex.Y,apex.Z, nRight.X,nRight.Y,nRight.Z,

				// Back (b2, b3, apex)
				b2.X,b2.Y,b2.Z,  nBack.X,nBack.Y,nBack.Z,
				b3.X,b3.Y,b3.Z,  nBack.X,nBack.Y,nBack.Z,
				apex.X,apex.Y,apex.Z, nBack.X,nBack.Y,nBack.Z,

				// Left (b3, b0, apex)
				b3.X,b3.Y,b3.Z,  nLeft.X,nLeft.Y,nLeft.Z,
				b0.X,b0.Y,b0.Z,  nLeft.X,nLeft.Y,nLeft.Z,
				apex.X,apex.Y,apex.Z, nLeft.X,nLeft.Y,nLeft.Z,

				// Base (2 triangles) - normal down
				// tri 1: b0, b2, b1  (chosen so normal points down)
				b0.X,b0.Y,b0.Z,  nDown.X,nDown.Y,nDown.Z,
				b2.X,b2.Y,b2.Z,  nDown.X,nDown.Y,nDown.Z,
				b1.X,b1.Y,b1.Z,  nDown.X,nDown.Y,nDown.Z,

				// tri 2: b0, b3, b2
				b0.X,b0.Y,b0.Z,  nDown.X,nDown.Y,nDown.Z,
				b3.X,b3.Y,b3.Z,  nDown.X,nDown.Y,nDown.Z,
				b2.X,b2.Y,b2.Z,  nDown.X,nDown.Y,nDown.Z,
			};

			// 6 triangles total = 18 indices, but since vertices are already grouped, we can index 0..17
			uint[] indices =
			{
				0, 1, 2,       // front
				3, 4, 5,       // right
				6, 7, 8,       // back
				9,10,11,       // left
				12,13,14,      // base tri 1
				15,16,17       // base tri 2
			};

			IndexCount = indices.Length;

			_vao = GL.GenVertexArray();
			GL.BindVertexArray(_vao);

			_vbo = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
			GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

			_ebo = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
			GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

			GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
			GL.EnableVertexAttribArray(0);

			GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
			GL.EnableVertexAttribArray(1);
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
