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
using FBXModelLoaderSpace;

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

		public void SetVector3(string name, Vector3 value)
		{
			int location = GL.GetUniformLocation(Handle, name);
			GL.Uniform3(location, value);
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


		List<Cube> _buildings;
		List<Cube> _cityBuildings;
		private FBXModelLoaderPN _fbxLoader;
		private List<GLMeshPN> _importedMeshes;

		float[] groundVertices = 
		{
			// Positions          // Normals (pointing up)
			-50.0f, 0.0f, -50.0f,  0.0f, 1.0f, 0.0f,
			50.0f, 0.0f, -50.0f,  0.0f, 1.0f, 0.0f,
			50.0f, 0.0f,  50.0f,  0.0f, 1.0f, 0.0f,
			-50.0f, 0.0f,  50.0f,  0.0f, 1.0f, 0.0f,
		};

		uint[] groundIndices = 
		{
			0,1,2,
			2,3,0
		};

		int _groundVAO;
		int _groundVBO;
		int _groundEBO;

		int _skyboxVAO;
		int _skyboxVBO;


		Matrix4 _view;
		Matrix4 _projection;
		private Vector3 _cameraPosition = new Vector3(0, 1.7f, 10);
		private Vector3 _cameraFront = new Vector3(0,0,-1);
		private Vector3 _cameraUp = Vector3.UnitY;
		private float _cameraSpeed = 7.5f;

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

		private Cube _baseBuilding = null;
		private Cube _building2 = null;

		


		Shader _shader;
		Shader _buildingShader;
		Shader _cityShader;
		Shader _skyboxShader;

		// int _vertexBuffer;
		// int VertexArrayObject;

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

			// Create skybox shader
			_skyboxShader = new Shader("skybox.vert", "skybox.frag");

			// Skybox cube vertices (large cube around camera)
			float[] skyboxVertices = {
				-1.0f,  1.0f, -1.0f,
				-1.0f, -1.0f, -1.0f,
				1.0f, -1.0f, -1.0f,
				1.0f, -1.0f, -1.0f,
				1.0f,  1.0f, -1.0f,
				-1.0f,  1.0f, -1.0f,

				-1.0f, -1.0f,  1.0f,
				-1.0f, -1.0f, -1.0f,
				-1.0f,  1.0f, -1.0f,
				-1.0f,  1.0f, -1.0f,
				-1.0f,  1.0f,  1.0f,
				-1.0f, -1.0f,  1.0f,

				1.0f, -1.0f, -1.0f,
				1.0f, -1.0f,  1.0f,
				1.0f,  1.0f,  1.0f,
				1.0f,  1.0f,  1.0f,
				1.0f,  1.0f, -1.0f,
				1.0f, -1.0f, -1.0f,

				-1.0f, -1.0f,  1.0f,
				-1.0f,  1.0f,  1.0f,
				1.0f,  1.0f,  1.0f,
				1.0f,  1.0f,  1.0f,
				1.0f, -1.0f,  1.0f,
				-1.0f, -1.0f,  1.0f,

				-1.0f,  1.0f, -1.0f,
				1.0f,  1.0f, -1.0f,
				1.0f,  1.0f,  1.0f,
				1.0f,  1.0f,  1.0f,
				-1.0f,  1.0f,  1.0f,
				-1.0f,  1.0f, -1.0f,

				-1.0f, -1.0f, -1.0f,
				-1.0f, -1.0f,  1.0f,
				1.0f, -1.0f, -1.0f,
				1.0f, -1.0f, -1.0f,
				-1.0f, -1.0f,  1.0f,
				1.0f, -1.0f,  1.0f
			};

			_skyboxVAO = GL.GenVertexArray();
			GL.BindVertexArray(_skyboxVAO);

			_skyboxVBO = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.ArrayBuffer, _skyboxVBO);
			GL.BufferData(BufferTarget.ArrayBuffer, skyboxVertices.Length * sizeof(float), skyboxVertices, BufferUsageHint.StaticDraw);

			GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
			GL.EnableVertexAttribArray(0);
			// Brown Sky
			// GL.ClearColor(0.25f,0.2f,0.23f, 1.0f);

			// Navy Blue Sky
			// GL.ClearColor(0.1f,0.2f,0.4f,1.0f);

			// Gray Sky
			GL.ClearColor(0.17f,0.2f,0.4f, 1.0f);
			

			_groundVAO = GL.GenVertexArray();
			GL.BindVertexArray(_groundVAO);

			_groundVBO = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.ArrayBuffer, _groundVBO);
			GL.BufferData(BufferTarget.ArrayBuffer, groundVertices.Length * sizeof(float), groundVertices, BufferUsageHint.StaticDraw);

			_groundEBO = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, _groundEBO);
			GL.BufferData(BufferTarget.ElementArrayBuffer, groundIndices.Length * sizeof(uint), groundIndices, BufferUsageHint.StaticDraw);
			
			GL.VertexAttribPointer(0,3,VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
			GL.EnableVertexAttribArray(0); // sets VAO attrib to 0?
			
			GL.VertexAttribPointer(1,3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
			GL.EnableVertexAttribArray(1);


			_buildings = new List<Cube>();
			Random random = new Random();
			for (int i = 0; i < 15; i++)
			{
				float x = (float)(random.NextDouble() * 80 - 40); // -40 to 40
				float z = (float)(random.NextDouble() * 80 - 40); //-40 to 40

				// Random scale
				float width = (float)(random.NextDouble() * 8 + 18); // 3 to 6
				float height = (float)(random.NextDouble() * 6 + 10); // 5 to 20
				float depth = (float)(random.NextDouble() * 8 + 18); //3 to 6

				// Set Y Pos to half so building sits on ground
				Vector3 position = new Vector3(x, 2f, z);
				Vector3 scale = new Vector3(width, height, depth);

				Vector4 color = new Vector4(0.50f,0.35f,0.35f,1.0f);

				Cube building = new Cube(position, scale, color);
				_buildings.Add(building);
			}

			_cityBuildings = new List<Cube>();
			Random cityRandom = new Random();

			int rows = 4;
			int cols = 5; // 4*5 = 20 buildings
			float spacing = 15f;

			for (int i = 0; i< 20; i++)
			{
				int row = i / cols; // which row (0-3)
				int col = i % cols; // which column (0-4)

				// Calculate position based on grid
				float x = col * spacing - (cols * spacing/2); // Center grid
				float z = row * spacing - (rows * spacing/2);

				float width = (float)(cityRandom.NextDouble() * 2 + 6);
				float height = (float)(cityRandom.NextDouble() * 2 + 19);
				float depth = (float)(cityRandom.NextDouble() * 2 + 6);

				Vector3 position = new Vector3(x, 7f, z);
				Vector3 scale = new Vector3(width, height, depth);
				Vector4 color = new Vector4(0.45f,0.5f,0.6f,1.0f);

				Cube building = new Cube(position, scale, color);
				_cityBuildings.Add(building);
			}

			
			string vertexPath = "shader.vert";
			string fragmentPath = "shader.frag";

			string buildingVertex = "buildingShader.vert";
			string buildingFragment = "buildingShader.frag";

			string cityBuildingFragment = "cityBuildingShader.frag";



			_shader = new Shader(vertexPath, fragmentPath);
			_buildingShader = new Shader(buildingVertex, buildingFragment);
			// _cityShader = new Shader(buildingVertex, cityBuildingFragment);
			Console.WriteLine("Shader created - no exceptions thrown");

			_fbxLoader = new FBXModelLoaderPN();

			_importedMeshes = _fbxLoader.LoadToGL(
				filePath:"3DModels/shrine1EXP.fbx";
				position: new Vector3(0f, 0f, 5f),
				scale: new Vector3(0.01f, 0.01f, 0.01f); // scaled down for compilatiohn
				color: new Vector4(0.8f,0.8f,0.8f,1.0f)
			);


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

					// GL.Enable(EnableCap.CullFace);
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

			_view = Matrix4.LookAt(_cameraPosition, _cameraPosition + _cameraFront, _cameraUp);

			GL.DepthFunc(DepthFunction.Lequal);
			_skyboxShader.Use();
			_skyboxShader.SetMatrix4("view", _view);
			_skyboxShader.SetMatrix4("projection", _projection);
			GL.BindVertexArray(_skyboxVAO);
			GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
			GL.DepthFunc(DepthFunction.Less);

			_shader.Use();
		
			
			_shader.SetMatrix4("view", _view);
    		_shader.SetMatrix4("projection", _projection);

			_shader.SetVector3("lightPos", new Vector3(50f, 50f, 50f));
			_shader.SetVector3("lightColor", new Vector3(1.0f, 0.9f, 0.7f));
			_shader.SetVector3("viewPos", _cameraPosition);

			// DEBUG
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
			_shader.SetVector4("color", 0.28f,0.55f,0.20f, 1.0f); 
			Matrix4 groundModel = Matrix4.Identity;
			_shader.SetMatrix4("model", groundModel);

			GL.BindVertexArray(_groundVAO);
			GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
			
			// GL.BindVertexArray(VertexArrayObject);
			// GL.DrawElements(PrimitiveType.Triangles, 36, DrawElementsType.UnsignedInt, 0);

			_buildingShader.Use();
			_buildingShader.SetMatrix4("view", _view);
    		_buildingShader.SetMatrix4("projection", _projection);

			_buildingShader.SetVector3("lightPos", new Vector3(50f, 50f, 50f));
			_buildingShader.SetVector3("lightColor", new Vector3(1.0f, 0.85f, 0.6f));
			_buildingShader.SetVector3("viewPos", _cameraPosition);

			// _baseBuilding.Draw(_buildingShader);
			// _building2.Draw(_buildingShader);
			foreach (Cube building in _buildings)
			{
				building.Draw(_buildingShader);				
			}


			foreach (Cube building in _cityBuildings)
			{
				building.Draw(_buildingShader);
			}
			
			if (_importedMeshes != null)
			{
				foreach(var m in _importedMeshes)
				{
					m.Draw(_buildingShader);
				}

			}
			
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
			// _cityShader.Dispose();
			//_baseBuilding.Dispose();
			//_building2.Dispose();

			foreach (Cube building in _buildings)
			{
				building.Dispose();
			}

			foreach (Cube building in _cityBuildings)
			{
				building.Dispose();
			}

			if (_importedMeshes != null)
			{
				foreach(var m in _importedMeshes)
				{
					m.Dispose(); 
				}

			}

			_fbxLoader?.Dispose();

			GL.DeleteBuffer(_groundVBO);
			GL.DeleteVertexArray(_groundVAO);
			GL.DeleteBuffer(_groundEBO);
		}

		//protected override void OnFrameBufferResize(FramebufferResizeEventArgs e)
		//{
			//base.OnFrameBufferResize(e);

			//GL.Viewport(0, 0, e.Width, e.Height);
		//}
	}


}

