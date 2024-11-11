using System.Runtime.InteropServices;
using Thriving.Win32Tools;

namespace Thriving.OpenGL
{
    public unsafe class GL
    {
        private static readonly IntPtr _glModule;
        static GL()
        {
            _glModule = KernelHelper.LoadLibraryEx("opengl32.dll", IntPtr.Zero, 0);
        }

        ~GL()
        {
            KernelHelper.FreeLibrary(_glModule);
        }

        private static TDelegate? GetProcAddress<TDelegate>()
        {
            // 委托名称与gl函数名称相同
            var procName = typeof(TDelegate).Name;

            var proc = wglGetProcAddress(procName);
            if (proc == IntPtr.Zero)
            {
                proc = KernelHelper.GetProcAddress(_glModule, procName);
            }

            TDelegate? result = default;
            if (proc != IntPtr.Zero)
            {
                result = Marshal.GetDelegateForFunctionPointer<TDelegate>(proc);
            }
            return result;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="hdc"></param>
        /// <param name="pfd"></param>
        /// <returns></returns>
        public static IntPtr CreateContext(IntPtr hdc, ref PixelFormatDescriptor pfd)
        {
            var pixelFormat = GDIHelper.ChoosePixelFormat(hdc, ref pfd);
            if (pixelFormat == 0) return IntPtr.Zero;

            var res = GDIHelper.SetPixelFormat(hdc, pixelFormat, ref pfd);
            if (res == false) return IntPtr.Zero;

            var hglrc = wglCreateContext(hdc);
            return hglrc;
        }

        public static bool DeleteContext(IntPtr hglrc)
        {
            return wglDeleteContext(hglrc);
        }

        public static bool MakeCurrent(IntPtr hdc, IntPtr hglrc)
        {
            return wglMakeCurrent(hdc, hglrc);
        }

        public static bool SwapBuffers(IntPtr hdc)
        {
            return GDIHelper.SwapBuffers(hdc);
        }


        private delegate void glViewport(int x, int y, int width, int height);
        public static void Viewport(int x, int y, int width, int height)
        {
            var func = GetProcAddress<glViewport>();
            func?.Invoke(x, y, width, height);
        }

        private delegate void glEnable(Capability capability);
        public static void Enable(Capability capability)
        {
            var func = GetProcAddress<glEnable>();
            func?.Invoke(capability);
        }

        private delegate void glDisable(Capability capability);
        public static void Disable(Capability capability)
        {
            var func = GetProcAddress<glDisable>();
            func?.Invoke(capability);
        }

        private delegate bool glIsEnabled(Capability capability);
        public static bool IsEnabled(Capability capability)
        {
            var result = false;
            var func = GetProcAddress<glIsEnabled>();
            if (func != null) result = func.Invoke(capability);
            return result;
        }

        private delegate void glClear(BufferBitType bufferType);
        public static void Clear(BufferBitType bufferType)
        {
            var func = GetProcAddress<glClear>();
            func?.Invoke(bufferType);
        }

        private delegate void glClearColor(float red, float green, float blue, float alpha);
        public static void ClearColor(float red, float green, float blue, float alpha)
        {
            var func = GetProcAddress<glClearColor>();
            func?.Invoke(red, green, blue, alpha);
        }

        private delegate void glGenTextures(int n, uint[] textures);
        public static uint[] GenTextures(int n)
        {
            var array = new uint[n];
            var func = GetProcAddress<glGenTextures>();
            func?.Invoke(n, array);
            return array;
        }

        private delegate void glDeleteTextures(int n, uint[] textures);
        public static void DeleteTextures(int n, uint[] textures)
        {
            var func = GetProcAddress<glDeleteTextures>();
            func?.Invoke(n, textures);
        }

        private delegate void glBindTexture(TextureTarget target, uint texture);
        public static void BindTexture(TextureTarget target, uint texture)
        {
            var func = GetProcAddress<glBindTexture>();
            func?.Invoke(target, texture);
        }

        private delegate void glActiveTexture(TextureLayer texture);
        public static void ActiveTexture(TextureLayer layer)
        {
            var func = GetProcAddress<glActiveTexture>();
            func?.Invoke(layer);
        }

        private delegate void glTexParameteri(TextureTarget target, int pname, int param);
        public static void TexParameteri(TextureTarget target, int pname, int param)
        {
            var func = GetProcAddress<glTexParameteri>();
            func?.Invoke(target, pname, param);
        }

        private delegate void glTexParameterf(TextureTarget target, int pname, float param);
        public static void TexParameterf(TextureTarget target, int pname, float param)
        {
            var func = GetProcAddress<glTexParameterf>();
            func?.Invoke(target, pname, param);
        }

        private delegate void glTexParameteriv(TextureTarget target, int pname, int[] param);
        public static void TexParameteriv(TextureTarget target, int pname, int[] param)
        {
            var func = GetProcAddress<glTexParameteriv>();
            func?.Invoke(target, pname, param);
        }

        private delegate void glTexParameterfv(TextureTarget target, int pname, float[] param);
        public static void TexParameterfv(TextureTarget target, int pname, float[] param)
        {
            var func = GetProcAddress<glTexParameterfv>();
            func?.Invoke(target, pname, param);
        }


        private delegate void glTexImage1D(TextureTarget target, int level, TextureFormat internalformat, int width, int border, TextureFormat formate, DataType type, IntPtr data);
        public static void TexImage1D(TextureTarget target, int level, TextureFormat internalformat, int width, TextureFormat formate, DataType type, IntPtr data)
        {
            var func = GetProcAddress<glTexImage1D>();
            func?.Invoke(target, level, internalformat, width, 0, formate, type, data);
        }

        private delegate void glTexImage2D(TextureTarget target, int level, TextureFormat internalformat, int width, int height, int border, TextureFormat formate, DataType type, IntPtr data);
        public static void TexImage2D(TextureTarget target, int level, TextureFormat internalformat, int width, int height, TextureFormat formate, DataType type, IntPtr data)
        {
            var func = GetProcAddress<glTexImage2D>();
            func?.Invoke(target, level, internalformat, width, height, 0, formate, type, data);
        }

        private delegate void glTexImage3D(TextureTarget target, int level, TextureFormat internalformat, int width, int height, int depth, int border, TextureFormat formate, DataType type, IntPtr data);
        public static void TexImage3D(TextureTarget target, int level, TextureFormat internalformat, int width, int height, int depth, TextureFormat formate, DataType type, IntPtr data)
        {
            var func = GetProcAddress<glTexImage3D>();
            func?.Invoke(target, level, internalformat, width, height, depth, 0, formate, type, data);
        }


        private delegate void glGenerateMipmap(TextureTarget target);
        public static void GenerateMipmap(TextureTarget target)
        {
            var func = GetProcAddress<glGenerateMipmap>();
            func?.Invoke(target);
        }

        private delegate void glGenVertexArrays(int n, uint[] array);
        public static uint[] GenVertexArrays(int n)
        {
            var func = GetProcAddress<glGenVertexArrays>();
            var array = new uint[n];
            func?.Invoke(n, array);
            return array;
        }

        private delegate void glDeleteVertexArrays(int n, uint[] array);
        public static void DeleteVertexArrays(int n, uint[] array)
        {
            var func = GetProcAddress<glDeleteVertexArrays>();
            func?.Invoke(n, array);
        }

        private delegate void glGenBuffers(int n, uint[] array);
        public static uint[] GenBuffers(int n)
        {
            var result = new uint[n];
            var func = GetProcAddress<glGenBuffers>();
            func?.Invoke(n, result);
            return result;
        }

        private delegate void glDeleteBuffers(int n, uint[] array);
        public static void DeleteBuffers(int n, uint[] array)
        {
            var func = GetProcAddress<glDeleteBuffers>();
            func?.Invoke(n, array);
        }

        private delegate void glBindVertexArray(uint[] array);
        public static void BindVertexArray(uint[] array)
        {
            var func = GetProcAddress<glBindVertexArray>();
            func?.Invoke(array);
        }

        private delegate void glBindBuffer(BufferType bufferType, uint buffer);
        public static void BindBuffer(BufferType bufferType, uint buffer)
        {
            var func = GetProcAddress<glBindBuffer>();
            func?.Invoke(bufferType, buffer);
        }

        private delegate void glBufferData(BufferType bufferType, long size, IntPtr data, BufferUsage usage);
        public static void BufferData(BufferType bufferType, long size, IntPtr data, BufferUsage usage)
        {
            var func = GetProcAddress<glBufferData>();
            func?.Invoke(bufferType, size, data, usage);
        }

        private delegate void glVertexAttribPointer(int index, int size, DataType type, bool normalized, int stride, void* pointer);
        public static void VertexAttribPointer(int index, int size, DataType type, bool normalized, int stride, void* pointer)
        {
            var func = GetProcAddress<glVertexAttribPointer>();
            func?.Invoke(index, size, type, normalized, stride, pointer);
        }

        private delegate void glEnableVertexAttribArray(int index);
        public static void EnableVertexAttribArray(int index)
        {
            var func = GetProcAddress<glEnableVertexAttribArray>();
            func?.Invoke(index);
        }

        private delegate uint glCreateShader(ShaderType shaderType);
        public static uint CreateShader(ShaderType shaderType)
        {
            var func = GetProcAddress<glCreateShader>();
            var result = func != null ? func.Invoke(shaderType) : 0;
            return result;
        }

        private delegate int glShaderSource(uint shader, int count, string[] src, int* length);
        public static void ShaderSource(uint shader, int count, string[] src, int[] length)
        {
            var func = GetProcAddress<glShaderSource>();
            func?.Invoke(shader, count, src, null);
        }

        private delegate int glCompileShader(uint shader);
        public static void CompileShader(uint shader)
        {
            var func = GetProcAddress<glCompileShader>();
            func?.Invoke(shader);
        }

        private delegate uint glCreateProgram();
        public static uint CreateProgram()
        {
            var func = GetProcAddress<glCreateProgram>();
            var result = func != null ? func.Invoke() : 0;
            return result;
        }

        private delegate void glAttachShader(uint program, uint shader);
        public static void AttachShader(uint program, uint shader)
        {
            var func = GetProcAddress<glAttachShader>();
            func?.Invoke(program, shader);
        }

        private delegate void glLinkProgram(uint program);
        public static void LinkProgram(uint program)
        {
            var func = GetProcAddress<glLinkProgram>();
            func?.Invoke(program);
        }

        private delegate void glUseProgram(uint program);
        public static void UseProgram(uint program)
        {
            var func = GetProcAddress<glUseProgram>();
            func?.Invoke(program);
        }

        private delegate void glDeleteProgram(uint program);
        public static void DeleteProgram(uint program)
        {
            var func = GetProcAddress<glDeleteProgram>();
            func?.Invoke(program);
        }

        private delegate void glDeleteShader(uint shader);
        public static void DeleteShader(uint shader)
        {
            var func = GetProcAddress<glDeleteShader>();
            func?.Invoke(shader);
        }

        private delegate void glGetShaderiv(uint shader, ShaderInfo pName, int* param);
        public static int GetShaderiv(uint shader, ShaderInfo pName)
        {
            var func = GetProcAddress<glGetShaderiv>();
            int result = 0;
            func?.Invoke(shader, pName, &result);
            return result;
        }

        private delegate void glGetShaderInfoLog(uint shader, int bufSize, int* length, char[] infoLog);
        public static string GetShaderInfoLog(uint shader, int bufSize)
        {
            var func = GetProcAddress<glGetShaderInfoLog>();
            var result = new char[bufSize];
            func?.Invoke(shader, bufSize, null, result);
            return new string(result);
        }

        private delegate void glGetProgramiv(uint program, ProgramInfo pName, int* param);
        public static int GetProgramiv(uint program, ProgramInfo pName)
        {
            int result = 0;
            var func = GetProcAddress<glGetProgramiv>();
            func?.Invoke(program, pName, &result);
            return result;
        }

        private delegate void glGetProgramInfoLog(uint program, int bufSize, int* length, char[] infoLog);
        public static string GetProgramInfoLog(uint program, int bufSize)
        {
            var func = GetProcAddress<glGetProgramInfoLog>();
            int length = 0;
            var result = new char[bufSize];
            func?.Invoke(program, bufSize, &length, result);
            return length > 0 ? new string(result) : string.Empty;
        }

        private delegate int glGetUniformLocation(uint program, string param);
        public static int GetUniformLocation(uint program, string param)
        {
            var func = GetProcAddress<glGetUniformLocation>();
            var result = func != null ? func.Invoke(program, param) : 0;
            return result;
        }

        private delegate void glUniform1f(int location, float v0);
        public static void Uniform1f(int location, float v0)
        {
            var func = GetProcAddress<glUniform1f>();
            func?.Invoke(location, v0);
        }

        private delegate void glUniform2f(int location, float v0, float v1);
        public static void Uniform2f(int location, float v0, float v1)
        {
            var func = GetProcAddress<glUniform2f>();
            func?.Invoke(location, v0, v1);
        }

        private delegate void glUniform3f(int location, float v0, float v1, float v2);
        public static void Uniform3f(int location, float v0, float v1, float v2)
        {
            var func = GetProcAddress<glUniform3f>();
            func?.Invoke(location, v0, v1, v2);
        }

        private delegate void glUniform4f(int location, float v0, float v1, float v2, float v3);
        public static void Uniform4f(int location, float v0, float v1, float v2, float v3)
        {
            var func = GetProcAddress<glUniform4f>();
            func?.Invoke(location, v0, v1, v2, v3);
        }

        private delegate void glUniform1i(int location, int v0);
        public static void Uniform1i(int location, int v0)
        {
            var func = GetProcAddress<glUniform1i>();
            func?.Invoke(location, v0);
        }

        private delegate void glUniform2i(int location, int v0, int v1);
        public static void Uniform2i(int location, int v0, int v1)
        {
            var func = GetProcAddress<glUniform2i>();
            func?.Invoke(location, v0, v1);
        }

        private delegate void glUniform3i(int location, int v0, int v1, int v2);
        public static void Uniform3i(int location, int v0, int v1, int v2)
        {
            var func = GetProcAddress<glUniform3i>();
            func?.Invoke(location, v0, v1, v2);
        }

        private delegate void glUniform4i(int location, int v0, int v1, int v2, int v3);
        public static void Uniform4i(int location, int v0, int v1, int v2, int v3)
        {
            var func = GetProcAddress<glUniform4i>();
            func?.Invoke(location, v0, v1, v2, v3);
        }

        private delegate void glUniform1ui(int location, uint v0);
        public static void Uniform1ui(int location, uint v0)
        {
            var func = GetProcAddress<glUniform1ui>();
            func?.Invoke(location, v0);
        }

        private delegate void glUniform2ui(int location, uint v0, uint v1);
        public static void Uniform2ui(int location, uint v0, uint v1)
        {
            var func = GetProcAddress<glUniform2ui>();
            func?.Invoke(location, v0, v1);
        }

        private delegate void glUniform3ui(int location, uint v0, uint v1, uint v2);
        public static void Uniform3ui(int location, uint v0, uint v1, uint v2)
        {
            var func = GetProcAddress<glUniform3ui>();
            func?.Invoke(location, v0, v1, v2);
        }

        private delegate void glUniform4ui(int location, uint v0, uint v1, uint v2, uint v3);
        public static void Uniform4ui(int location, uint v0, uint v1, uint v2, uint v3)
        {
            var func = GetProcAddress<glUniform4ui>();
            func?.Invoke(location, v0, v1, v2, v3);
        }

        private delegate void glUniform1fv(int location, int count, float[] data);
        public static void Uniform1fv(int location, int count, float[] data)
        {
            var func = GetProcAddress<glUniform1fv>();
            func?.Invoke(location, count, data);
        }

        private delegate void glUniform2fv(int location, int count, float[] data);
        public static void Uniform2fv(int location, int count, float[] data)
        {
            var func = GetProcAddress<glUniform2fv>();
            func?.Invoke(location, count, data);
        }

        private delegate void glUniform3fv(int location, int count, float[] data);
        public static void Uniform3fv(int location, int count, float[] data)
        {
            var func = GetProcAddress<glUniform3fv>();
            func?.Invoke(location, count, data);
        }

        private delegate void glUniform4fv(int location, int count, float[] data);
        public static void Uniform4fv(int location, int count, float[] data)
        {
            var func = GetProcAddress<glUniform4fv>();
            func?.Invoke(location, count, data);
        }

        private delegate void glUniform1iv(int location, int count, int[] data);
        public static void Uniform1iv(int location, int count, int[] data)
        {
            var func = GetProcAddress<glUniform1iv>();
            func?.Invoke(location, count, data);
        }

        private delegate void glUniform2iv(int location, int count, int[] data);
        public static void Uniform2iv(int location, int count, int[] data)
        {
            var func = GetProcAddress<glUniform2iv>();
            func?.Invoke(location, count, data);
        }

        private delegate void glUniform3iv(int location, int count, int[] data);
        public static void Uniform3iv(int location, int count, int[] data)
        {
            var func = GetProcAddress<glUniform3iv>();
            func?.Invoke(location, count, data);
        }

        private delegate void glUniform4iv(int location, int count, int[] data);
        public static void Uniform4iv(int location, int count, int[] data)
        {
            var func = GetProcAddress<glUniform4iv>();
            func?.Invoke(location, count, data);
        }

        private delegate void glUniform1uiv(int location, int count, uint[] data);
        public static void Uniform1uiv(int location, int count, uint[] data)
        {
            var func = GetProcAddress<glUniform1uiv>();
            func?.Invoke(location, count, data);
        }

        private delegate void glUniform2uiv(int location, int count, uint[] data);
        public static void Uniform2uiv(int location, int count, uint[] data)
        {
            var func = GetProcAddress<glUniform2uiv>();
            func?.Invoke(location, count, data);
        }

        private delegate void glUniform3uiv(int location, int count, uint[] data);
        public static void Uniform3uiv(int location, int count, uint[] data)
        {
            var func = GetProcAddress<glUniform3uiv>();
            func?.Invoke(location, count, data);
        }

        private delegate void glUniform4uiv(int location, int count, uint[] data);
        public static void Uniform4uiv(int location, int count, uint[] data)
        {
            var func = GetProcAddress<glUniform4uiv>();
            func?.Invoke(location, count, data);
        }

        private delegate void glUniformMatrix2fv(int location, int count, bool transpose, float[] data);
        public static void UniformMatrix2fv(int location, int count, bool transpose, float[] data)
        {
            var func = GetProcAddress<glUniformMatrix2fv>();
            func?.Invoke(location, count, transpose, data);
        }

        private delegate void glUniformMatrix3fv(int location, int count, bool transpose, float[] data);
        public static void UniformMatrix3fv(int location, int count, bool transpose, float[] data)
        {
            var func = GetProcAddress<glUniformMatrix3fv>();
            func?.Invoke(location, count, transpose, data);
        }

        private delegate void glUniformMatrix4fv(int location, int count, bool transpose, float[] data);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="location"></param>
        /// <param name="count"></param>
        /// <param name="transpose">为true时以行顺序提供，false时以列顺序提供</param>
        /// <param name="data"></param>
        public static void UniformMatrix4fv(int location, int count, bool transpose, float[] data)
        {
            var func = GetProcAddress<glUniformMatrix4fv>();
            func?.Invoke(location, count, transpose, data);
        }

        private delegate void glUniformMatrix2x3fv(int location, int count, bool transpose, float[] data);
        public static void UniformMatrix2x3fv(int location, int count, bool transpose, float[] data)
        {
            var func = GetProcAddress<glUniformMatrix2x3fv>();
            func?.Invoke(location, count, transpose, data);
        }

        private delegate void glUniformMatrix2x4fv(int location, int count, bool transpose, float[] data);
        public static void UniformMatrix2x4fv(int location, int count, bool transpose, float[] data)
        {
            var func = GetProcAddress<glUniformMatrix2x4fv>();
            func?.Invoke(location, count, transpose, data);
        }

        private delegate void glUniformMatrix3x2fv(int location, int count, bool transpose, float[] data);
        public static void UniformMatrix3x2fv(int location, int count, bool transpose, float[] data)
        {
            var func = GetProcAddress<glUniformMatrix3x2fv>();
            func?.Invoke(location, count, transpose, data);
        }

        private delegate void glUniformMatrix4x2fv(int location, int count, bool transpose, float[] data);
        public static void UniformMatrix4x2fv(int location, int count, bool transpose, float[] data)
        {
            var func = GetProcAddress<glUniformMatrix4x2fv>();
            func?.Invoke(location, count, transpose, data);
        }

        private delegate void glUniformMatrix3x4fv(int location, int count, bool transpose, float[] data);
        public static void UniformMatrix3x4fv(int location, int count, bool transpose, float[] data)
        {
            var func = GetProcAddress<glUniformMatrix3x4fv>();
            func?.Invoke(location, count, transpose, data);
        }

        private delegate void glUniformMatrix4x3fv(int location, int count, bool transpose, float[] data);
        public static void UniformMatrix4x3fv(int location, int count, bool transpose, float[] data)
        {
            var func = GetProcAddress<glUniformMatrix4x3fv>();
            func?.Invoke(location, count, transpose, data);
        }

        private delegate void glDrawArrays(DrawMode mode, int first, int count);
        public static void DrawArrays(DrawMode mode, int first, int count)
        {
            var func = GetProcAddress<glDrawArrays>();
            func?.Invoke(mode, first, count);
        }

        private delegate void glDrawElements(DrawMode mode, int count, DataType type, IntPtr indices);
        public static void DrawElements(DrawMode mode, int count, uint[] indices)
        {
            var func = GetProcAddress<glDrawElements>();
            var data = Marshal.UnsafeAddrOfPinnedArrayElement<uint>(indices, 0);
            func?.Invoke(mode, count, DataType.GL_UNSIGNED_INT, data);
        }

        private delegate int glGetError();
        public static int GetError()
        {
            var func = GetProcAddress<glGetError>();
            var result = func != null ? func.Invoke() : 0;
            return result;
        }

        private delegate void glGenFramebuffers(int n, uint[] array);
        public static uint[] GenFramebuffers(int n)
        {
            var array = new uint[n];
            var func = GetProcAddress<glGenFramebuffers>();
            func?.Invoke(n, array);
            return array;
        }

        private delegate void glBindFramebuffer(FrameBufferTarget target, uint frameBuffer);
        public static void BindFramebuffer(FrameBufferTarget target, uint frameBuffer)
        {
            var func = GetProcAddress<glBindFramebuffer>();
            func?.Invoke(target, frameBuffer);
        }

        private delegate void glFramebufferTexture2D(FrameBufferTarget target, FrameBufferAttachment attachment, TextureTarget texTarget, uint texture, int level);
        public static void FramebufferTexture2D(FrameBufferTarget target, FrameBufferAttachment attachment, TextureTarget texTarget, uint texture, int level)
        {
            var func = GetProcAddress<glFramebufferTexture2D>();
            func?.Invoke(target, attachment, texTarget, texture, level);
        }

        private delegate void glGenRenderbuffers(int n, uint[] array);
        public static uint[] GenRenderbuffers(int n)
        {
            var array = new uint[n];
            var func = GetProcAddress<glGenRenderbuffers>();
            func?.Invoke(n, array);
            return array;
        }

        private delegate void glBindRenderbuffer(uint target, uint renderbuffer);
        public static void BindRenderbuffer(uint renderbuffer)
        {
            uint GL_RENDERBUFFER = 0x8D41;
            var func = GetProcAddress<glBindRenderbuffer>();
            func?.Invoke(GL_RENDERBUFFER, renderbuffer);
        }

        private delegate void glRenderbufferStorage(uint target, TextureFormat format, int width, int height);
        public static void RenderbufferStorage(TextureFormat format, int width, int height)
        {
            uint GL_RENDERBUFFER = 0x8D41;
            var func = GetProcAddress<glRenderbufferStorage>();
            func?.Invoke(GL_RENDERBUFFER, format, width, height);
        }

        private delegate void glFramebufferRenderbuffer(FrameBufferTarget target, FrameBufferAttachment attachment, uint renderbuffertarget, uint renderbuffer);
        public static void FramebufferRenderbuffer(FrameBufferTarget target, FrameBufferAttachment attachment, uint renderbuffer)
        {
            uint GL_RENDERBUFFER = 0x8D41;
            var func = GetProcAddress<glFramebufferRenderbuffer>();
            func?.Invoke(target, attachment, GL_RENDERBUFFER, renderbuffer);
        }

        private delegate FrameBufferStatus glCheckFramebufferStatus(FrameBufferTarget target);
        public static FrameBufferStatus CheckFramebufferStatus(FrameBufferTarget target)
        {
            var func = GetProcAddress<glCheckFramebufferStatus>();
            var result = FrameBufferStatus.GL_FRAMEBUFFER_UNDEFINED;
            if (func != null)
            {
                result = func.Invoke(target);
            }
            return result;
        }


        private delegate void glDeleteRenderbuffers(int n, uint[] renderbuffers);
        public static void DeleteRenderbuffers(int n, uint[] renderbuffers)
        {
            var func = GetProcAddress<glDeleteRenderbuffers>();
            func?.Invoke(n, renderbuffers);
        }

        private delegate void glDeleteFramebuffers(int n, uint[] framebuffers);
        public static void DeleteFramebuffers(int n, uint[] framebuffers)
        {
            var func = GetProcAddress<glDeleteFramebuffers>();
            func?.Invoke(n, framebuffers);
        }

        private delegate void glPolygonMode(PolygonFace face, PolygonMode mode);
        public static void PolygonMode(PolygonMode mode)
        {
            var func = GetProcAddress<glPolygonMode>();
            func?.Invoke(PolygonFace.GL_FRONT_AND_BACK, mode);
        }


        [DllImport("opengl32.dll", EntryPoint = "wglCreateContext")]
        private static extern IntPtr wglCreateContext(IntPtr hDC);

        [DllImport("opengl32.dll", EntryPoint = "wglCreateLayerContext")]
        private static extern IntPtr wglCreateLayerContext(IntPtr hDC, int layer);

        [DllImport("opengl32.dll", EntryPoint = "wglDeleteContext")]
        private static extern bool wglDeleteContext(IntPtr HGLRC);

        [DllImport("opengl32.dll", EntryPoint = "wglMakeCurrent")]
        private static extern bool wglMakeCurrent(IntPtr hDC, IntPtr hGLRC);

        [DllImport("opengl32.dll", EntryPoint = "wglGetCurrentDC")]
        private static extern IntPtr wglGetCurrentDC();

        [DllImport("opengl32.dll", EntryPoint = "wglGetCurrentContext")]
        private static extern IntPtr wglGetCurrentContext();

        [DllImport("opengl32.dll", EntryPoint = "wglGetProcAddress")]
        private static extern IntPtr wglGetProcAddress([MarshalAs(UnmanagedType.LPStr)]string LPCSTR);
    }

    public enum PolygonMode
    {
        GL_POINT = 0x1B00,
        GL_LINE = 0x1B01,
        GL_FILL = 0x1B02
    }

    public enum PolygonFace
    {
        GL_FRONT = 0x040,
        GL_BACK = 0x0405,
        GL_FRONT_AND_BACK= 0x0408,
    }

}
