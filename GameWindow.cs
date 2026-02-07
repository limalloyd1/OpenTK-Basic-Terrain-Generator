using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;
using CubeRenderer;
using ErrorCode = OpenTK.Graphics.OpenGL4.ErrorCode;
using System.IO;
using System;

namespace GameSpace
{
	public class Shader : IDisposable 
	{
		int VertexShader;
		int FragmentShader;
		public int Handle;

		public Shader(string vertexPath, string fragmentPath)
		{
			string VertexShaderSource = File.ReadAllText(vertexPath);
			string FragmentShaderSource = File.ReadAllText(fragmentPath);

			VertexShader = GL.CreateShader(ShaderType.VertexShader);
			GL.ShaderSource(VertexShader, VertexShaderSource);

			FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
			GL.ShaderSource(FragmentShader, FragmentShaderSource);

			GL.CompileShader(VertexShader);

			GL.GetShader(VertexShader, ShaderParameter.CompileStatus, out int success);
			if (success == 0)
			{
				string infoLog = GL.GetShaderInfoLog(VertexShader);
				Console.WriteLine(infoLog);
			}

			GL.CompileShader(FragmentShader);

			GL.GetShader(FragmentShader, ShaderParameter.CompileStatus, out int fSuccess);
			if (fSuccess == 0)
			{
				string infoLog = GL.GetShaderInfoLog(FragmentShader);
				Console.WriteLine(infoLog);
			}

			Handle = GL.CreateProgram();

			GL.AttachShader(Handle, VertexShader);
			GL.AttachShader(Handle, FragmentShader);

			GL.LinkProgram(Handle);

			GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int hSuccess);
			if (hSuccess == 0)
			{
				string infoLog = GL.GetProgramInfoLog(Handle);
				Console.WriteLine(infoLog);
			}

			GL.DetachShader(Handle, VertexShader);
			GL.DetachShader(Handle, FragmentShader);
			GL.DeleteShader(FragmentShader);
			GL.DeleteShader(VertexShader);	

		}

		public void SetVector4(string name, float x, float y, float z, float w)
		{
			int location = GL.GetUniformLocation(Handle, name);
			GL.Uniform4(location, x, y, z, w);
		}

		public void SetMatrix4(string name, Matrix4 data)
		{
			int location = GL.GetUniformLocation(Handle, name);
			if (location == -1)
			{
				Console.WriteLine($"Uniform {name} not found in shader!");
			}
			GL.UniformMatrix4(location, false, ref data);
		}

		public void Use()
		{
			GL.UseProgram(Handle);
		}

		private bool disposedValue = false;
		
		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				GL.DeleteProgram(Handle);

				disposedValue = true;
			}
		}
		~Shader()
		{
			if(disposedValue == false)
			{
				Console.WriteLine("GPU Resource Leak! Did you forget to call Dispose()?");
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

	}

	public class Game : GameWindow 
	{


		float[] vertices =
		{
			// Front face
			-0.5f, -0.5f,  0.5f,
			0.5f, -0.5f,  0.5f,
			0.5f,  0.5f,  0.5f,
			-0.5f,  0.5f,  0.5f,
			
			// Back face
			-0.5f, -0.5f, -0.5f,
			-0.5f,  0.5f, -0.5f,
			0.5f,  0.5f, -0.5f,
			0.5f, -0.5f, -0.5f,
			
			// Top face
			-0.5f,  0.5f, -0.5f,
			-0.5f,  0.5f,  0.5f,
			0.5f,  0.5f,  0.5f,
			0.5f,  0.5f, -0.5f,
			
			// Bottom face
			-0.5f, -0.5f, -0.5f,
			0.5f, -0.5f, -0.5f,
			0.5f, -0.5f,  0.5f,
			-0.5f, -0.5f,  0.5f,
			
			// Right face
			0.5f, -0.5f, -0.5f,
			0.5f,  0.5f, -0.5f,
			0.5f,  0.5f,  0.5f,
			0.5f, -0.5f,  0.5f,
			
			// Left face
			-0.5f, -0.5f, -0.5f,
			-0.5f, -0.5f,  0.5f,
			-0.5f,  0.5f,  0.5f,
			-0.5f,  0.5f, -0.5f
		};

		uint[] indices =
		{
			0, 1, 2, 2, 3, 0,       // Front
			4, 5, 6, 6, 7, 4,       // Back
			8, 9, 10, 10, 11, 8,    // Top
			12, 13, 14, 14, 15, 12, // Bottom
			16, 17, 18, 18, 19, 16, // Right
			20, 21, 22, 22, 23, 20  // Left
		};


		Matrix4 _view;
		Matrix4 _projection;
		private Vector3 _cameraPosition = new Vector3(0, 1.7f, 10);
		private Vector3 _cameraFront = new Vector3(0,0,-1);
		private Vector3 _cameraUp = Vector3.UnitY;
		private float _cameraSpeed = 2.5f;

		private bool _firstMove = true;
		private Vector2 _lastPos;
		private float _yaw = -90.0f;
		private float _pitch = 0.0f;
		private float _sensitivity = 0.1f;	

		private float _verticalVelocity = 0.0f;
		private float _gravity = -25.0f;
		private float _jumpStrength = 15.0f;
		private bool _isOnGround = true;
		private float _groundLevel = 1.7f; // eye height

		private int _elementBuffer;

		private Cube _baseBuilding;
		private Cube _building2;


		Shader _shader;
		Shader _buildingShader;

		int _vertexBuffer;
		int VertexArrayObject;

		// Console.Writeline("Starting Game Window");

		public Game(int wWidth, int wHeight, string title) :
		base(GameWindowSettings.Default, new NativeWindowSettings()
		{
			ClientSize = (wWidth, wHeight),
			Title=title
		})
		{
		}

		protected override void OnLoad()
		{
			base.OnLoad();

			
			CursorState = CursorState.Grabbed;
			GL.Enable(EnableCap.DepthTest);
			GL.Disable(EnableCap.CullFace);
			GL.Viewport(0, 0, Size.X, Size.Y); 
			Console.WriteLine("Loading Window...");
			GL.ClearColor(0.1f,0.2f,0.4f,1.0f);

			VertexArrayObject = GL.GenVertexArray(); // generate VAO
			GL.BindVertexArray(VertexArrayObject); // bind variable as VAO object

			_vertexBuffer = GL.GenBuffer(); // Gen Array Buffer
			GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer); // Bind Array Buffer

			GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw); // sets bufffer to memory based on VAO
			
			_elementBuffer = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBuffer);
			GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);
			Console.WriteLine($"Indices count: {indices.Length}");
			Console.WriteLine($"VBO ID: {_vertexBuffer}");
			Console.WriteLine($"EBO ID: {_elementBuffer}");
			Console.WriteLine($"VAO ID: {VertexArrayObject}");

			Console.WriteLine($"First vertex: ({vertices[0]}, {vertices[1]}, {vertices[2]})");
			
			GL.VertexAttribPointer(0,3,VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
			GL.EnableVertexArrayAttrib(VertexArrayObject, 0); // sets VAO attrib to 0?
			
			_baseBuilding = new Cube(
				new Vector3(-4, 5, -3),           // Position
				new Vector3(5, 15, 5),           // Scale
				new Vector4(0.2f, 0.6f, 0.9f, 1.0f)  // Blue color
			);

			_building2 = new Cube(
				// (x,y,z)
				new Vector3(4, 7, -4),           // Position
				new Vector3(7, 20, 7),           // Scale
				new Vector4(0.2f, 0.6f, 0.9f, 1.0f)  // Blue color
			);


			string vertexPath = "shader.vert";
			string fragmentPath = "shader.frag";

			string buildingVertex = "buildingShader.vert";
			string buildingFragment = "buildingShader.frag";


			Console.WriteLine($"Vertex shader path: {Path.GetFullPath(vertexPath)}");
			Console.WriteLine($"Vertex shader exists: {File.Exists(vertexPath)}");
			Console.WriteLine($"Fragment shader path: {Path.GetFullPath(fragmentPath)}");
			Console.WriteLine($"Fragment shader exists: {File.Exists(fragmentPath)}");

			Console.WriteLine($"Building Vertex shader path: {Path.GetFullPath(buildingVertex)}");
			Console.WriteLine($"Building Vertex shader exists: {File.Exists(buildingVertex)}");
			Console.WriteLine($"Buildig Fragment shader path: {Path.GetFullPath(buildingFragment)}");
			Console.WriteLine($"Building Fragment shader exists: {File.Exists(buildingFragment)}");

			if (File.Exists(vertexPath))
			{
				string vertContent = File.ReadAllText(vertexPath);
				Console.WriteLine($"Vertex shader first 50 chars: {vertContent.Substring(0, Math.Min(50, vertContent.Length))}");
			}

			_shader = new Shader(vertexPath, fragmentPath);
			_buildingShader = new Shader(buildingVertex, buildingFragment);
			Console.WriteLine("Shader created - no exceptions thrown");


			_projection = Matrix4.CreatePerspectiveFieldOfView(
					MathHelper.DegreesToRadians(45f), // sets FOV
					Size.X / (float)Size.Y, // Aspect Ratio
					0.1f, // Near Plane
					500f // Far Plane
					);
					
		}

		protected override void OnMouseMove(MouseMoveEventArgs e)
		{
			base.OnMouseMove(e);

			if (_firstMove)
			{
				_lastPos = new Vector2(e.X, e.Y);
				_firstMove = false;
				return; 
			}

			float deltaX = e.X - _lastPos.X;
			float deltaY = e.Y - _lastPos.Y;
			_lastPos = new Vector2(e.X, e.Y);

			_yaw += deltaX * _sensitivity;
			_pitch -= deltaY * _sensitivity; // Inverted Y

			if (_pitch > 89.0f)
				_pitch = 89.0f;

			if (_pitch < -89.0f)
				_pitch = -89.0f;

			_cameraFront.X = (float)Math.Cos(MathHelper.DegreesToRadians(_pitch)) * (float)Math.Cos(MathHelper.DegreesToRadians(_yaw));
			_cameraFront.Y = (float)Math.Sin(MathHelper.DegreesToRadians(_pitch));
			_cameraFront.Z = (float)Math.Cos(MathHelper.DegreesToRadians(_pitch)) * (float)Math.Sin(MathHelper.DegreesToRadians(_yaw));
		       _cameraFront = Vector3.Normalize(_cameraFront);	

		}
		
		bool wireframe = false;
		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			base.OnUpdateFrame(e);

			if(KeyboardState.IsKeyDown(Keys.Escape))
			{
				Console.WriteLine("Closing Window...");
				Close();
			}

			if (KeyboardState.IsKeyDown(Keys.F))
			{
				wireframe = !wireframe;

				if (wireframe)
				{
				
					GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
					
					GL.Disable(EnableCap.CullFace);
					GL.Enable(EnableCap.PolygonOffsetLine);
					GL.PolygonOffset(-1.0f, -1.0f);
				}
				else 
				{
					GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

					GL.Enable(EnableCap.CullFace);
					GL.Disable(EnableCap.PolygonOffsetLine);
					GL.PolygonOffset(0f, 0f);
				}
			}


			float cameraSpeed = _cameraSpeed * (float)e.Time;

			    if (KeyboardState.IsKeyDown(Keys.W))
					_cameraPosition += _cameraFront * cameraSpeed;
				if (KeyboardState.IsKeyDown(Keys.S))
					_cameraPosition -= _cameraFront * cameraSpeed;
				if (KeyboardState.IsKeyDown(Keys.A))
					_cameraPosition -= Vector3.Normalize(Vector3.Cross(_cameraFront, _cameraUp)) * cameraSpeed;
				if (KeyboardState.IsKeyDown(Keys.D))
					_cameraPosition += Vector3.Normalize(Vector3.Cross(_cameraFront, _cameraUp)) * cameraSpeed;
				if (KeyboardState.IsKeyDown(Keys.Space) && _isOnGround)
				{
					_verticalVelocity = _jumpStrength;
					_isOnGround = false;	
				}

				// Apply Gravity
				_verticalVelocity += _gravity * (float)e.Time;
				_cameraPosition.Y += _verticalVelocity * (float)e.Time;

				if (_cameraPosition.Y <= _groundLevel)
				{
					_cameraPosition.Y = _groundLevel;
					_verticalVelocity = 0.0f;
					_isOnGround = true;	
				}
		}

		private int _frameCount = 0;
		protected override void OnRenderFrame(FrameEventArgs e)
		{
			base.OnRenderFrame(e);

			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			_shader.Use();
		


			_view = Matrix4.LookAt(_cameraPosition, _cameraPosition + _cameraFront, _cameraUp);
			_shader.SetMatrix4("view", _view);
    		_shader.SetMatrix4("projection", _projection);


			if (_frameCount == 0)
			{
				Console.WriteLine($"=== FRAME {_frameCount} START ===");
				Console.WriteLine($"Camera pos: {_cameraPosition}");
				Console.WriteLine($"Looking at: (0, 0, 0)");
				
				_shader.Use();
				int modelLoc = GL.GetUniformLocation(_shader.Handle, "model");
				int viewLoc = GL.GetUniformLocation(_shader.Handle, "view");
				int projLoc = GL.GetUniformLocation(_shader.Handle, "projection");
				
				Console.WriteLine($"Shader Handle: {_shader.Handle}");
				Console.WriteLine($"Uniform locations - model: {modelLoc}, view: {viewLoc}, proj: {projLoc}");
			}
			
			_frameCount++;

			// Draw ground
			_shader.SetVector4("color", 0.65f, 0.50f,0.48f, 1.0f); 
			// Matrix4 groundModel = Matrix4.Identity;
			Matrix4 groundModel = Matrix4.CreateScale(100.0f, 0.2f, 100.0f);
			_shader.SetMatrix4("model", groundModel);
			// _shader.SetMatrix4("view", _view);
			// _shader.SetMatrix4("projection", _projection);
			GL.BindVertexArray(VertexArrayObject);
			GL.DrawElements(PrimitiveType.Triangles, 36, DrawElementsType.UnsignedInt, 0);

			_buildingShader.Use();
			_buildingShader.SetMatrix4("view", _view);
    		_buildingShader.SetMatrix4("projection", _projection);
			_baseBuilding.Draw(_buildingShader);
			_building2.Draw(_buildingShader);
			
			SwapBuffers();
		}
		protected override void OnResize(ResizeEventArgs e)
		{
			base.OnResize(e);
			
			GL.Viewport(0, 0, e.Width, e.Height);
			
			// Update projection matrix with new aspect ratio
			_projection = Matrix4.CreatePerspectiveFieldOfView(
				MathHelper.DegreesToRadians(45f),
				e.Width / (float)e.Height,
				0.1f,
				500f
			);
		}

		protected override void OnUnload()
		{
			base.OnUnload();

			_shader.Dispose();
			_buildingShader.Dispose();
			_baseBuilding.Dispose();
			_building2.Dispose();

			GL.DeleteBuffer(_vertexBuffer);
			GL.DeleteVertexArray(VertexArrayObject);
		}

		//protected override void OnFrameBufferResize(FramebufferResizeEventArgs e)
		//{
			//base.OnFrameBufferResize(e);

			//GL.Viewport(0, 0, e.Width, e.Height);
		//}
	}


}

