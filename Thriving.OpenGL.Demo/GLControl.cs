using System.Runtime.InteropServices;
using System.Windows.Interop;
using Thriving.Geometry;
using Thriving.Win32Tools;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace Thriving.OpenGL.Demo
{
    public class GLControl : System.Windows.FrameworkElement
    {
        private readonly WindowProc _wndProc;
        private readonly IntPtr _hInstance;
        private readonly string _className;

        private IntPtr _hwnd;
        /// <summary>
        /// Device Context
        /// </summary>
        private IntPtr _hdc;
        /// <summary>
        /// GL Render Context
        /// </summary>
        private IntPtr _hglrc;

        public bool PolygonMode
        {
            get { return (bool)GetValue(PolygonModeProperty); }
            set { SetValue(PolygonModeProperty, value); }
        }
        public static readonly DependencyProperty PolygonModeProperty =
            DependencyProperty.Register("PolygonMode", typeof(bool), typeof(GLControl), new PropertyMetadata(false, OnPropertyChanged));



        public bool ColorMode
        {
            get { return (bool)GetValue(ColorModeProperty); }
            set { SetValue(ColorModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ColorMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColorModeProperty =
            DependencyProperty.Register("ColorMode", typeof(bool), typeof(GLControl), new PropertyMetadata(false, OnPropertyChanged));




        private static void OnPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is GLControl ctl)
            {
                if (e.Property == ColorModeProperty)
                {
                    ctl.RefreshShader();
                }

                ctl.Refresh();
            }
        }

        public GLControl()
        {
            _wndProc = new WindowProc(GLWndProc);
            _hInstance = Marshal.GetHINSTANCE(typeof(GLControl).Module);
            _className = $"opengl_host_{Guid.NewGuid()}";
            var wndClass = new WNDCLASSEX()
            {
                hInstance = _hInstance,
                lpfnWndProc = _wndProc,
                lpszClassName = _className,
                cbSize = Marshal.SizeOf<WNDCLASSEX>(),
                style = WindowClassStyle.CS_HREDRAW | WindowClassStyle.CS_VREDRAW | WindowClassStyle.CS_DBLCLKS | WindowClassStyle.CS_OWNDC,
            };
            var res = WindowHelper.RegisterClassEx(ref wndClass);

            this.Loaded += (s, e) =>
            {
                if (HwndSource.FromVisual(this) is HwndSource hwndParent)
                {
                    var p1 = this.PointToScreen((Point)this.VisualOffset);
                    var p0 = this.PointFromScreen(p1);

                    _hwnd = WindowHelper.CreateWindowEx(0, _className, "View",
                        Thriving.Win32Tools.WindowStyle.WS_CHILD | Thriving.Win32Tools.WindowStyle.WS_VISIBLE,
                         (int)p0.X, (int)p0.Y,
                       (int)this.ActualWidth, (int)this.ActualHeight,
                        hwndParent.Handle,
                        IntPtr.Zero,
                        _hInstance,
                        IntPtr.Zero);
                }
            };

            this.SizeChanged += (s, e) =>
            {
                if (_hwnd != IntPtr.Zero)
                {
                    var p1 = this.PointToScreen((Point)this.VisualOffset);
                    var p0 = this.PointFromScreen(p1);
                    WindowHelper.SetWindowPos(_hwnd, IntPtr.Zero, (int)p0.X, (int)p0.Y, (int)e.NewSize.Width, (int)e.NewSize.Height, WindowPosStyle.SWP_SHOWWINDOW);
                }
            };
        }

        ~GLControl()
        {
            if (_hInstance != IntPtr.Zero) WindowHelper.UnregisterClass(_className, _hInstance);
            if (_hglrc != IntPtr.Zero) GL.DeleteContext(_hglrc);
            if (_hwnd != IntPtr.Zero)
            {
                if (_hdc != IntPtr.Zero) { WindowHelper.ReleaseDC(_hwnd, _hdc); }
                WindowHelper.DestroyWindow(_hwnd);
            }
        }


        private void Refresh()
        {
            if (_hwnd == IntPtr.Zero) return;
            WindowHelper.SendMessage(_hwnd, WindowMessage.WM_PAINT, IntPtr.Zero, IntPtr.Zero);
        }

        private void RefreshShader()
        {
            var shader = new Shader();
            if (ColorMode)
            {
                var colorSource = "#version 440 core\r\n" +
                    "in vec2 TexCoord;\r\n" +
                    "in vec3 TexColor;\r\n" +
                    "void main()\r\n" +
                    "{\r\n" +
                    "gl_FragColor = vec4(TexColor,0.6);\r\n" +
                    "}";

                shader.Complie(vertexShaderSource, ShaderType.GL_VERTEX_SHADER);
                shader.Complie(colorSource, ShaderType.GL_FRAGMENT_SHADER);
                shader.Link();
            }
            else
            {
                shader.Complie(vertexShaderSource, ShaderType.GL_VERTEX_SHADER);
                shader.Complie(fragmentShaderSource, ShaderType.GL_FRAGMENT_SHADER);
                shader.Link();
            }
            _shader = shader;
        }

        private const string vertexShaderSource = "#version 440 core\r\n" +
           "layout (location = 0) in vec3 aPos;\r\n" +
           "layout (location = 1) in vec3 aNormal;\r\n" +
           "layout (location = 2) in vec2 aTexCoord;\r\n" +
           "layout (location = 3) in vec3 aColor;\r\n" +
           "out vec2 TexCoord;\r\n" +
           "out vec3 TexColor;\r\n" +
           "uniform mat4 model;\r\n" +
           "uniform mat4 projection;\r\n" +
           "void main()\r\n" +
           "{\r\n" +
           "gl_Position = projection * model * vec4(aPos, 1.0f);\r\n" +
           "TexCoord = aTexCoord;\r\n" +
           "TexColor = aColor;\r\n" +
           "}";
        private const string fragmentShaderSource = "#version 440 core\r\n" +
            "in vec2 TexCoord;\r\n" +
            "in vec3 TexColor;\r\n" +
            "uniform sampler2D texture1;\r\n" +
            "uniform sampler2D texture2;\r\n" +
            "void main()\r\n" +
            "{\r\n" +
            "gl_FragColor = mix(texture(texture1, TexCoord), texture(texture2, TexCoord), 0.2);\r\n" +
            "}";

        protected Shader _shader;
        protected CameraBase _camera;
        protected Texture _texture1;
        protected Texture _texture2;

        protected Shader _skyboxShader;
        protected Texture _skyboxTexture;
        protected uint _skyboxVAO;

        protected ICollection<GeometryEntity> _entities;

        private bool _firstMouse = true;
        private short _lastX;
        private short _lastY;
        private DateTime _lastTime;

        protected virtual IntPtr GLWndProc(IntPtr hwnd, WindowMessage msg, IntPtr wParam, IntPtr lParam)
        {
            Debug.WriteLine(msg);
            if (msg == WindowMessage.WM_MOUSEACTIVATE)
            {
                WindowHelper.SetFocus(hwnd);
            }

            if (msg == WindowMessage.WM_CREATE)
            {
                _hdc = WindowHelper.GetDC(hwnd);

                var pfd = new PixelFormatDescriptor()
                {
                    nSize = (short)Marshal.SizeOf<PixelFormatDescriptor>(),
                    nVersion = 1,
                    dwFlags = (int)(PixelBufferProperty.PFD_DRAW_TO_WINDOW | PixelBufferProperty.PFD_SUPPORT_OPENGL | PixelBufferProperty.PFD_DOUBLEBUFFER),
                    iPixelType = PixelType.PFD_TYPE_RGBA,
                    cColorBits = 8,
                    cDepthBits = 16,
                };

                _hglrc = GL.CreateContext(_hdc, ref pfd);
                var sr = GL.MakeCurrent(_hdc, _hglrc);

                //_camera = new Thriving.OpenGL.OrthographicCamera()
                //{
                //    Position = new Thriving.Geometry.Point3D(0, 0, 3),
                //    LookAt = new Vector3D(0, 0, -1),
                //    Up = new Vector3D(0, 1, 0),
                //    ZNear = 1.0,
                //    ZFar = 100,
                //};

                _camera = new Thriving.OpenGL.PerspectiveCamera(

                   new Thriving.Geometry.Point3D(0, 0, 5),
                    Thriving.Geometry.Vector3D.BasisZ.Negate(),
                    Thriving.Geometry.Vector3D.BasisY,
                    45,
                    3 / 4);

                _shader = new Shader();
                _shader.Complie(vertexShaderSource, ShaderType.GL_VERTEX_SHADER);
                _shader.Complie(fragmentShaderSource, ShaderType.GL_FRAGMENT_SHADER);
                _shader.Link();

                _skyboxShader = new Shader();
                _skyboxShader.Complie("#version 440 core\r\n" +
                    "layout (location = 0) in vec3 aPos;\r\n\r\n" +
                    "out vec3 TexCoords;\r\n\r\n" +
                    "uniform mat4 projection;\r\n" +
                    "void main()\r\n{\r\n    TexCoords = aPos;\r\n    " +
                    "gl_Position =projection * vec4(aPos, 1.0);\r\n }", ShaderType.GL_VERTEX_SHADER);
                _skyboxShader.Complie("#version 440 core\r\n" +
                    "in vec3 TexCoords;\r\n\r\n" +
                    "uniform samplerCube skybox;\r\n\r\n" +
                    "void main()\r\n{    \r\n    gl_FragColor = texture(skybox, TexCoords);\r\n}", ShaderType.GL_FRAGMENT_SHADER);
                _skyboxShader.Link();

                var vertices = new Vertex[]{

 // 底面
                      new Vertex(){Position =new Point3D(-0.5f, -0.5f, -0.5f),UV=new Vector2D( 0.0f, 0.0f),Color=new Vector3D(1,0.2,0.8)},
                      new Vertex(){Position =new Point3D( 0.5f, -0.5f, -0.5f),UV=new Vector2D(1.0f, 0.0f),Color=new Vector3D(1,0.2,0.8)},
                      new Vertex(){Position =new Point3D( 0.5f,  0.5f, -0.5f),UV=new Vector2D(1.0f, 1.0f),Color=new Vector3D(1,0.2,0.8)},
                      new Vertex(){Position =new Point3D( 0.5f,  0.5f, -0.5f),UV=new Vector2D(1.0f, 1.0f),Color=new Vector3D(1,0.2,0.8)},
                      new Vertex(){Position =new Point3D(-0.5f,  0.5f, -0.5f),UV=new Vector2D(0.0f, 1.0f),Color=new Vector3D(1,0.2,0.8)},
                      new Vertex(){Position =new Point3D(-0.5f, -0.5f, -0.5f),UV=new Vector2D(0.0f, 0.0f),Color=new Vector3D(1,0.2,0.8)},
// 顶面
                      new Vertex(){Position =new Point3D(-0.5f, -0.5f,  0.5f),UV=new Vector2D(0.0f, 0.0f),Color=new Vector3D(1,0.2,0.8)},
                      new Vertex(){Position =new Point3D( 0.5f, -0.5f,  0.5f),UV=new Vector2D(1.0f, 0.0f),Color=new Vector3D(1,0.2,0.8)},
                      new Vertex(){Position =new Point3D( 0.5f,  0.5f,  0.5f),UV=new Vector2D(1.0f, 1.0f),Color=new Vector3D(1,0.2,0.8)},
                      new Vertex(){Position =new Point3D( 0.5f,  0.5f,  0.5f),UV=new Vector2D(1.0f, 1.0f),Color=new Vector3D(1,0.2,0.8)},
                      new Vertex(){Position =new Point3D(-0.5f,  0.5f,  0.5f),UV=new Vector2D(0.0f, 1.0f),Color=new Vector3D(1,0.2,0.8)},
                      new Vertex(){Position =new Point3D(-0.5f, -0.5f,  0.5f),UV=new Vector2D(0.0f, 0.0f),Color=new Vector3D(1,0.2,0.8)},
// 左侧面
                      new Vertex(){Position =new Point3D(-0.5f,  0.5f,  0.5f),UV=new Vector2D(1.0f, 0.0f),Color=new Vector3D(1,0.2,0.8)},
                      new Vertex(){Position =new Point3D(-0.5f,  0.5f, -0.5f),UV=new Vector2D(1.0f, 1.0f),Color=new Vector3D(1,0.2,0.8)},
                      new Vertex(){Position =new Point3D(-0.5f, -0.5f, -0.5f),UV=new Vector2D(0.0f, 1.0f),Color=new Vector3D(1,0.2,0.8)},
                      new Vertex(){Position =new Point3D(-0.5f, -0.5f, -0.5f),UV=new Vector2D(0.0f, 1.0f),Color=new Vector3D(1,0.2,0.8)},
                      new Vertex(){Position =new Point3D(-0.5f, -0.5f,  0.5f),UV=new Vector2D(0.0f, 0.0f),Color=new Vector3D(1,0.2,0.8)},
                      new Vertex(){Position =new Point3D(-0.5f,  0.5f,  0.5f),UV=new Vector2D(1.0f, 0.0f),Color=new Vector3D(1,0.2,0.8)},
// 右侧面
                       new Vertex(){Position =new Point3D(0.5f,  0.5f,  0.5f),UV=new Vector2D(1.0f, 0.0f),Color=new Vector3D(1,0.2,0.8)},
                       new Vertex(){Position =new Point3D(0.5f,  0.5f, -0.5f),UV=new Vector2D(1.0f, 1.0f),Color=new Vector3D(1,0.2,0.8)},
                       new Vertex(){Position =new Point3D(0.5f, -0.5f, -0.5f),UV=new Vector2D(0.0f, 1.0f),Color=new Vector3D(1,0.2,0.8)},
                       new Vertex(){Position =new Point3D(0.5f, -0.5f, -0.5f),UV=new Vector2D(0.0f, 1.0f),Color=new Vector3D(1,0.2,0.8)},
                       new Vertex(){Position =new Point3D(0.5f, -0.5f,  0.5f),UV=new Vector2D(0.0f, 0.0f),Color=new Vector3D(1,0.2,0.8)},
                       new Vertex(){Position =new Point3D(0.5f,  0.5f,  0.5f),UV=new Vector2D(1.0f, 0.0f),Color=new Vector3D(1,0.2,0.8)},
// 近面
                       new Vertex(){Position =new Point3D(-0.5f, -0.5f, -0.5f),UV=new Vector2D(0.0f, 1.0f),Color=new Vector3D(1,0.2,0.8)},
                       new Vertex(){Position =new Point3D( 0.5f, -0.5f, -0.5f),UV=new Vector2D(1.0f, 1.0f),Color=new Vector3D(1,0.2,0.8)},
                       new Vertex(){Position =new Point3D( 0.5f, -0.5f,  0.5f),UV=new Vector2D(1.0f, 0.0f),Color=new Vector3D(1,0.2,0.8)},
                       new Vertex(){Position =new Point3D( 0.5f, -0.5f,  0.5f),UV=new Vector2D(1.0f, 0.0f),Color=new Vector3D(1,0.2,0.8)},
                       new Vertex(){Position =new Point3D(-0.5f, -0.5f,  0.5f),UV=new Vector2D(0.0f, 0.0f),Color=new Vector3D(1,0.2,0.8)},
                       new Vertex(){Position =new Point3D(-0.5f, -0.5f, -0.5f),UV=new Vector2D(0.0f, 1.0f),Color=new Vector3D(1,0.2,0.8)},
// 远面
                       new Vertex(){Position =new Point3D(-0.5f,  0.5f, -0.5f),UV=new Vector2D(0.0f, 1.0f),Color=new Vector3D(1,0.2,0.8)},
                       new Vertex(){Position =new Point3D( 0.5f,  0.5f, -0.5f),UV=new Vector2D(1.0f, 1.0f),Color=new Vector3D(0.7,0.2,0.0)},
                       new Vertex(){Position =new Point3D( 0.5f,  0.5f,  0.5f),UV=new Vector2D(1.0f, 0.0f),Color=new Vector3D(1,0.2,0.8)},
                       new Vertex(){Position =new Point3D( 0.5f,  0.5f,  0.5f),UV=new Vector2D(1.0f, 0.0f),Color=new Vector3D(1,0.2,0.8)},
                       new Vertex(){Position =new Point3D(-0.5f,  0.5f,  0.5f),UV=new Vector2D(0.0f, 0.0f),Color=new Vector3D(0.6,0.2,0.8)},
                       new Vertex(){Position =new Point3D(-0.5f,  0.5f, -0.5f),UV=new Vector2D(0.0f, 1.0f),Color=new Vector3D(1,0.2,0.3)}
                };
                var indices = new uint[] {
                        0, 1, 3,
                        1, 2, 3
                    };



                var geo = new GeometryObject();
                geo.SetupVertexBuffer(vertices);
                geo.SetupIndexBuffer(indices);
                //    geo.EnableVertexAttribute();

                var material = new BasicMaterial();

                Point3D[] cubePositions = {
                    new Point3D(0.0f, 0.0f, 0.0f),
                    new Point3D(2.0f, 5.0f, -15.0f),
                    new Point3D(-1.5f, -2.2f, -2.5f),
                    new Point3D(-3.8f, -2.0f, -12.3f),
                    new Point3D(2.4f, -0.4f, -3.5f),
                    new Point3D(-1.7f, 3.0f, -7.5f),
                    new Point3D(1.3f, -2.0f, -2.5f),
                    new Point3D(1.5f, 2.0f, -2.5f),
                    new Point3D(1.5f, 0.2f, -1.5f),
                    new Point3D(-1.3f, 1.0f, -1.5f)
                };
                _entities = new List<GeometryEntity>();
                foreach (var vertex in cubePositions)
                {
                    var entity = new GeometryEntity(geo, material);
                    entity.Position = (vertex);
                    _entities.Add(entity);
                }

                _texture1 = new Texture(TextureTarget.GL_TEXTURE_2D);
                _texture1.BindImage2D(@"C:\Users\Master\Desktop\PixPin_2024-07-26_23-41-47.jpg");

                _texture2 = new Texture(TextureTarget.GL_TEXTURE_2D);
                _texture2.BindImage2D(@"C:\Users\Master\Desktop\PixPin_2024-07-26_23-42-03.jpg");

                _shader.Use();
                var location = GL.GetUniformLocation(_shader.Id, "texture1");
                GL.Uniform1i(location, 0);

                var location4 = GL.GetUniformLocation(_shader.Id, "texture2");
                GL.Uniform1i(location4, 1);


                var skybox = new float[]
                {
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

         -1.0f,  1.0f, -1.0f ,
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


                _skyboxVAO = GL.GenVertexArrays(1)[0];
                var skyboxVBO = GL.GenBuffers(1);
                GL.BindVertexArray(_skyboxVAO);

                GL.BindBuffer(BufferTarget.GL_ARRAY_BUFFER, skyboxVBO[0]);
                GL.BufferData(BufferTarget.GL_ARRAY_BUFFER, skybox, BufferUsage.GL_STATIC_DRAW);

                /////顶点着色器position属性 layout (location = 0)
                GL.EnableVertexAttribArray(0);
                GL.VertexAttribPointer(0, 3, DataType.GL_FLOAT, false, 3 * Marshal.SizeOf<float>(), 0);

                _skyboxTexture = new Texture(TextureTarget.GL_TEXTURE_CUBE_MAP);
                _skyboxTexture.BindCubeMap(
                @"C:\Users\Master\Desktop\LearnC\LearnOpenGL\resources/textures/skybox/right.jpg",
              @"C:\Users\Master\Desktop\LearnC\LearnOpenGL\resources/textures/skybox/left.jpg",
               @"C:\Users\Master\Desktop\LearnC\LearnOpenGL\resources/textures/skybox/top.jpg",
              @"C:\Users\Master\Desktop\LearnC\LearnOpenGL\resources/textures/skybox/bottom.jpg",
              @"C:\Users\Master\Desktop\LearnC\LearnOpenGL\resources/textures/skybox/front.jpg",
               @"C:\Users\Master\Desktop\LearnC\LearnOpenGL\resources/textures/skybox/back.jpg");

                _skyboxShader.Use();
                var location5 = GL.GetUniformLocation(_skyboxShader.Id, "skybox");
                GL.Uniform1i(location5, 0);
            }

            if ((WindowMessage?)msg == WindowMessage.WM_PAINT)
            {
                //var now = DateTime.Now;
                //if ((now - _lastTime).Milliseconds < 150)
                //{
                //    return IntPtr.Zero;
                //}
                //_lastTime = now;

                if (PolygonMode)
                {
                    GL.PolygonMode(Thriving.OpenGL.PolygonMode.GL_LINE);
                }
                else
                {
                    GL.PolygonMode(Thriving.OpenGL.PolygonMode.GL_FILL);
                }

                if (!GL.IsEnabled(Capability.GL_DEPTH_TEST))
                {
                    GL.Enable(Capability.GL_DEPTH_TEST);
                }

                GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
                GL.Clear(BufferBitType.GL_COLOR_BUFFER_BIT | BufferBitType.GL_DEPTH_BUFFER_BIT);

                _shader.Use();

                GL.ActiveTexture(TextureLayer.GL_TEXTURE0);
                _texture1.Bind();
                GL.ActiveTexture(TextureLayer.GL_TEXTURE1);
                _texture2.Bind();

                foreach (var cube in _entities)
                {
                    cube.Rotate(Vector3D.BasisZ, (float)(10 * Math.PI / 180));
                    cube.Draw(_shader, _camera, false);
                }

                if (PolygonMode) GL.PolygonMode(Thriving.OpenGL.PolygonMode.GL_FILL);

                GL.DepthFunc(DepthFunc.GL_LEQUAL);
                _skyboxShader.Use();

                var location = GL.GetUniformLocation(_skyboxShader.Id, "projection");
                if (location >= 0)
                {
                    var tArray = _camera.GetTestMatrix();
                    GL.UniformMatrix4fv(location, 1, false, tArray);
                }

                GL.BindVertexArray(_skyboxVAO);
                GL.ActiveTexture(TextureLayer.GL_TEXTURE0);
                _skyboxTexture.Bind();

                GL.DrawArrays(DrawMode.GL_TRIANGLES, 0, 36);
                GL.DepthFunc(DepthFunc.GL_LESS);

                GL.SwapBuffers(_hdc);
            }


            if ((WindowMessage?)msg == WindowMessage.WM_SIZE)
            {
                if (ActualWidth > 0 && ActualHeight > 0)
                {
                    GL.Viewport(0, 0, (int)ActualWidth, (int)ActualHeight);

                    if (_camera is PerspectiveCamera pers)
                    {
                        pers.aspect = ActualWidth / ActualHeight;
                    }

                    if (_camera is OrthographicCamera orth)
                    {
                        orth.left = -0.5 * ActualWidth;
                        orth.right = 0.5 * ActualWidth;
                        orth.top = 0.5 * ActualHeight;
                        orth.bottom = -0.5 * ActualHeight;
                    }
                }
            }

            if (msg == WindowMessage.WM_KEYDOWN)
            {
                var key = (VirtualKeyCode?)wParam;
                if (key == VirtualKeyCode.VK_DOWN)
                {
                    _camera.Translate(0.1 * _camera.Up);
                }
                if (key == VirtualKeyCode.VK_UP)
                {
                    _camera.Translate(-0.1 * _camera.Up);
                }
                if (key == VirtualKeyCode.VK_LEFT)
                {
                    _camera.Translate(-0.1 * _camera.Right);
                }
                if (key == VirtualKeyCode.VK_RIGHT)
                {
                    _camera.Translate(0.1 * _camera.Right);
                }
                Refresh();
            }

            if (msg == WindowMessage.WM_MOUSEWHEEL)
            {
                var offset = Win32Helper.High16Bits(wParam);
                if (_camera is PerspectiveCamera pers)
                {
                    _camera.Translate((offset / Mouse.MouseWheelDeltaForOneLine) * _camera.LookAt);
                }
                Refresh();
            }

            if (msg == WindowMessage.WM_MOUSEMOVE)
            {
                var mState = (MouseState)wParam;

                var yPos = Win32Helper.High16Bits(lParam);
                var xPos = Win32Helper.Low16Bits(lParam);

                if (mState == MouseState.MK_SHIFT)
                {
                    if (_firstMouse)
                    {
                        _firstMouse = false;
                    }
                    else
                    {
                        var xoffset = xPos - _lastX;
                        var yoffset = yPos - _lastY;

                        if (xoffset != 0)
                        {
                            _camera.Yaw(-Math.PI * xoffset / 1800);
                        }
                        if (yoffset != 0)
                        {
                            _camera.Pitch(Math.PI * yoffset / 1800);
                        }
                        Refresh();
                    }
                    _lastX = xPos;
                    _lastY = yPos;
                }
                else
                {
                    _firstMouse = true;
                }
            }

            if (msg == WindowMessage.WM_LBUTTONDOWN)
            {
                var yPos = Win32Helper.High16Bits(lParam);
                var xPos = Win32Helper.Low16Bits(lParam);

                // 裁剪空间坐标
                var x = 2 * (xPos / this.ActualWidth) - 1;
                var y = 1 - 2 * (yPos / this.ActualHeight);

                // 空间坐标
                var p = _camera.GetRay(x, y);
            }

            return WindowHelper.DefWindowProc(hwnd, msg, wParam, lParam);
        }
    }
}
