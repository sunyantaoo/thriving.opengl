using System.Runtime.InteropServices;
using System.Windows.Interop;
using Thriving.OpenGL;
using Thriving.Geometry;
using Thriving.Win32Tools;
using System.Diagnostics;
using System.Windows;

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

        public static void OnPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            (obj as GLControl)?.Refresh();
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
                    var p1 = this.PointToScreen(new Point(this.VisualOffset.X, this.VisualOffset.Y));
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
                    var p1 = this.PointToScreen(new Point(this.VisualOffset.X, this.VisualOffset.Y));
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

        protected void Refresh()
        {
            if (_hwnd != IntPtr.Zero) 
            {
                WindowHelper.SendMessage(_hwnd, WindowMessage.WM_PAINT, IntPtr.Zero, IntPtr.Zero);
            }
        }


        private const string vertexShaderSource = "#version 330 core\r\n" +
           "layout (location = 0) in vec3 aPos;\r\n" +
           "layout (location = 1) in vec3 aNormal;\r\n" +
           "layout (location = 2) in vec2 aTexCoord;\r\n" +
           "layout (location = 3) in vec3 aColor;\r\n" +
           "out vec2 TexCoord;\r\n" +
           "uniform mat4 model;\r\n" +
           "uniform mat4 projection;\r\n" +
           "void main()\r\n" +
           "{\r\n" +
           "gl_Position = projection * model * vec4(aPos, 1.0f);\r\n" +
           "TexCoord = vec2(aTexCoord.x, aTexCoord.y);\r\n" +
           "}";
        private const string fragmentShaderSource = "#version 330 core\r\n" +
            "out vec4 FragColor;\r\n" +
            "in vec2 TexCoord;\r\n" +
            "uniform sampler2D texture1;\r\n" +
             "uniform sampler2D texture2;\r\n" +
            "void main()\r\n" +
            "{\r\n" +
            "FragColor = mix(texture(texture1, TexCoord), texture(texture2, TexCoord), 0.2);\r\n" +
            "}";

        protected Shader _shader;
        protected CameraBase _camera;
        protected Texture _texture1;
        protected Texture _texture2;
        protected GeometryObject _geo;

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

                _camera = new Thriving.OpenGL.PerspectiveCamera()
                {
                    Position = new Thriving.Geometry.Point3D(0, 0, 5),
                    LookAt = Thriving.Geometry.Vector3D.BasisZ.Negate(),
                    Up = Thriving.Geometry.Vector3D.BasisY,
                    ZNear = 0.1,
                    ZFar = 100,
                    fov = 45,
                };

                _shader = new Shader();
                _shader.Complie(vertexShaderSource, ShaderType.GL_VERTEX_SHADER);
                _shader.Complie(fragmentShaderSource, ShaderType.GL_FRAGMENT_SHADER);
                _shader.Link();
                _shader.Use();

                var vertices = new Vertex[]{

 // 底面
                      new Vertex(){Position =new Point3D(-0.5f, -0.5f, -0.5f),UV=new Vector2D( 0.0f, 0.0f)},
                      new Vertex(){Position =new Point3D( 0.5f, -0.5f, -0.5f),UV=new Vector2D(1.0f, 0.0f)},
                      new Vertex(){Position =new Point3D( 0.5f,  0.5f, -0.5f),UV=new Vector2D(1.0f, 1.0f)},
                      new Vertex(){Position =new Point3D( 0.5f,  0.5f, -0.5f),UV=new Vector2D(1.0f, 1.0f)},
                      new Vertex(){Position =new Point3D(-0.5f,  0.5f, -0.5f),UV=new Vector2D(0.0f, 1.0f)},
                      new Vertex(){Position =new Point3D(-0.5f, -0.5f, -0.5f),UV=new Vector2D(0.0f, 0.0f)},
// 顶面
                      new Vertex(){Position =new Point3D(-0.5f, -0.5f,  0.5f),UV=new Vector2D(0.0f, 0.0f)},
                      new Vertex(){Position =new Point3D( 0.5f, -0.5f,  0.5f),UV=new Vector2D(1.0f, 0.0f)},
                      new Vertex(){Position =new Point3D( 0.5f,  0.5f,  0.5f),UV=new Vector2D(1.0f, 1.0f)},
                      new Vertex(){Position =new Point3D( 0.5f,  0.5f,  0.5f),UV=new Vector2D(1.0f, 1.0f)},
                      new Vertex(){Position =new Point3D(-0.5f,  0.5f,  0.5f),UV=new Vector2D(0.0f, 1.0f)},
                      new Vertex(){Position =new Point3D(-0.5f, -0.5f,  0.5f),UV=new Vector2D(0.0f, 0.0f)},
// 左侧面
                      new Vertex(){Position =new Point3D(-0.5f,  0.5f,  0.5f),UV=new Vector2D(1.0f, 0.0f)},
                      new Vertex(){Position =new Point3D(-0.5f,  0.5f, -0.5f),UV=new Vector2D(1.0f, 1.0f)},
                      new Vertex(){Position =new Point3D(-0.5f, -0.5f, -0.5f),UV=new Vector2D( 0.0f, 1.0f)},
                      new Vertex(){Position =new Point3D(-0.5f, -0.5f, -0.5f),UV=new Vector2D( 0.0f, 1.0f)},
                      new Vertex(){Position =new Point3D(-0.5f, -0.5f,  0.5f),UV=new Vector2D(0.0f, 0.0f)},
                      new Vertex(){Position =new Point3D(-0.5f,  0.5f,  0.5f),UV=new Vector2D(1.0f, 0.0f)},
// 右侧面
                       new Vertex(){Position =new Point3D(0.5f,  0.5f,  0.5f),UV=new Vector2D(1.0f, 0.0f)},
                       new Vertex(){Position =new Point3D(0.5f,  0.5f, -0.5f),UV=new Vector2D(1.0f, 1.0f)},
                       new Vertex(){Position =new Point3D(0.5f, -0.5f, -0.5f),UV=new Vector2D( 0.0f, 1.0f)},
                       new Vertex(){Position =new Point3D(0.5f, -0.5f, -0.5f),UV=new Vector2D( 0.0f, 1.0f)},
                       new Vertex(){Position =new Point3D(0.5f, -0.5f,  0.5f),UV=new Vector2D(0.0f, 0.0f)},
                       new Vertex(){Position =new Point3D(0.5f,  0.5f,  0.5f),UV=new Vector2D(1.0f, 0.0f)},
// 近面
                       new Vertex(){Position =new Point3D(-0.5f, -0.5f, -0.5f),UV=new Vector2D(0.0f, 1.0f)},
                       new Vertex(){Position =new Point3D( 0.5f, -0.5f, -0.5f),UV=new Vector2D(1.0f, 1.0f)},
                       new Vertex(){Position =new Point3D( 0.5f, -0.5f,  0.5f),UV=new Vector2D(1.0f, 0.0f)},
                       new Vertex(){Position =new Point3D( 0.5f, -0.5f,  0.5f),UV=new Vector2D(1.0f, 0.0f)},
                       new Vertex(){Position =new Point3D(-0.5f, -0.5f,  0.5f),UV=new Vector2D(0.0f, 0.0f)},
                       new Vertex(){Position =new Point3D(-0.5f, -0.5f, -0.5f),UV=new Vector2D(0.0f, 1.0f)},
// 远面
                       new Vertex(){Position =new Point3D(-0.5f,  0.5f, -0.5f),UV=new Vector2D(0.0f, 1.0f)},
                       new Vertex(){Position =new Point3D( 0.5f,  0.5f, -0.5f),UV=new Vector2D(1.0f, 1.0f)},
                       new Vertex(){Position =new Point3D( 0.5f,  0.5f,  0.5f),UV=new Vector2D(1.0f, 0.0f)},
                       new Vertex(){Position =new Point3D( 0.5f,  0.5f,  0.5f),UV=new Vector2D(1.0f, 0.0f)},
                       new Vertex(){Position =new Point3D(-0.5f,  0.5f,  0.5f),UV=new Vector2D(0.0f, 0.0f)},
                       new Vertex(){Position =new Point3D(-0.5f,  0.5f, -0.5f),UV=new Vector2D(0.0f, 1.0f)}
                };
                var indices = new uint[] {
                        0, 1, 3,
                        1, 2, 3
                    };

                _geo = new GeometryObject();
                _geo.SetupVertexBuffer(vertices);
                _geo.SetupIndexBuffer(indices);

                _texture1 = new Texture(TextureTarget.GL_TEXTURE_2D);
                _texture1.BindImage2D(@"C:\Users\Master\Desktop\PixPin_2024-07-26_23-41-47.jpg");

                _texture2 = new Texture(TextureTarget.GL_TEXTURE_2D);
                _texture2.BindImage2D(@"C:\Users\Master\Desktop\PixPin_2024-07-26_23-42-03.jpg");

                var location = GL.GetUniformLocation(_shader.Id, "texture1");
                GL.Uniform1i(location, 0);

                var location4 = GL.GetUniformLocation(_shader.Id, "texture2");
                GL.Uniform1i(location4, 1);
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

                GL.ActiveTexture(TextureLayer.GL_TEXTURE0);
                _texture1.Bind();
                GL.ActiveTexture(TextureLayer.GL_TEXTURE1);
                _texture2.Bind();
                _shader.Use();

                 var modelTrans = Transform3D.CreateRotation(Vector3D.BasisY, DateTime.Now.Millisecond * Math.PI / 999);
               // var modelTrans = Transform3D.Identity;
                var tArray1 = new float[]
                {
                            (float)modelTrans.BasisX.X,  (float)modelTrans.BasisY.X,  (float)modelTrans.BasisZ.X,  (float)modelTrans.Origin.X,
                            (float)modelTrans.BasisX.Y,  (float)modelTrans.BasisY.Y,  (float)modelTrans.BasisZ.Y  ,(float)modelTrans.Origin.Y,
                            (float)modelTrans.BasisX.Z,  (float)modelTrans.BasisY.Z, (float)modelTrans.BasisZ.Z  ,(float)modelTrans.Origin.Z,
                            (float)0,  (float)0, (float)0  ,(float)1,
                };


                var tArray3 = _camera.GetProjectionMatrix();

                var location1 = GL.GetUniformLocation(_shader.Id, "model");
                GL.UniformMatrix4fv(location1, 1, true, tArray1);

                var location3 = GL.GetUniformLocation(_shader.Id, "projection");
                GL.UniformMatrix4fv(location3, 1, false, tArray3);

                var indices = new uint[] {
                        0, 1, 3,
                        1, 2, 3
                    };

                _geo.Bind();
                GL.DrawArrays(DrawMode.GL_TRIANGLES, 0, 36);
                //GL.DrawElements(DrawMode.GL_TRIANGLES, 6, indices);

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
                    _camera.Position = _camera.Position.Add(0.1 * _camera.Up);
                }
                if (key == VirtualKeyCode.VK_UP)
                {
                    _camera.Position = _camera.Position.Subtract(0.1 * _camera.Up);
                }
                if (key == VirtualKeyCode.VK_LEFT)
                {
                  
                    _camera.Position = _camera.Position.Subtract(0.1 * _camera.Right);
                }
                if (key == VirtualKeyCode.VK_RIGHT)
                {
                    _camera.Position = _camera.Position.Add(0.1 * _camera.Right);
                }
                WindowHelper.SendMessage(_hwnd, WindowMessage.WM_PAINT, IntPtr.Zero, IntPtr.Zero);
            }

            if (msg == WindowMessage.WM_MOUSEWHEEL)
            {
                // 120 的倍数
                var offset = Win32Helper.High16Bits(wParam);
                if (_camera is PerspectiveCamera pers)
                {
                    _camera.Position = _camera.Position.Add((offset / 120) * _camera.LookAt);
                }
                WindowHelper.SendMessage(_hwnd, WindowMessage.WM_PAINT, IntPtr.Zero, IntPtr.Zero);
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
                        WindowHelper.SendMessage(_hwnd, WindowMessage.WM_PAINT, IntPtr.Zero, IntPtr.Zero);
                    }
                    _lastX = xPos;
                    _lastY = yPos;
                }
                else
                {
                    _firstMouse = true;
                }
            }
            return WindowHelper.DefWindowProc(hwnd, msg, wParam, lParam);
        }


    }
}
