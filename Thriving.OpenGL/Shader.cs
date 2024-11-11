namespace Thriving.OpenGL
{
    public class Shader
    {
        private readonly uint _program;
        public Shader()
        {
            _program = GL.CreateProgram();
        }

        ~Shader()
        {
            GL.DeleteProgram(_program);
        }

        public uint Id { get => _program; }

        public void Link()
        {
            GL.LinkProgram(_program);
            var res = GL.GetProgramiv(_program, ProgramInfo.GL_LINK_STATUS);
            if (res == 0)
            {
                var errorInfo = GL.GetProgramInfoLog(_program, 512);
            }
        }

        public void Use()
        {
            GL.UseProgram(_program);
        }

        public void Complie(string src, ShaderType type)
        {
            var shader = GL.CreateShader(type);
            GL.ShaderSource(shader, 1, new string[] { src }, new int[] { });
            var res = GL.GetShaderiv(shader, ShaderInfo.GL_SHADER_SOURCE_LENGTH);
            if (res == 0)
            {
                var errorInfo = GL.GetShaderInfoLog(shader, 512);
            }
            GL.CompileShader(shader);
            res = GL.GetShaderiv(shader, ShaderInfo.GL_COMPILE_STATUS);
            if (res == 0)
            {
                var errorInfo = GL.GetShaderInfoLog(shader, 512);
            }
            GL.AttachShader(_program, shader);
            res = GL.GetProgramiv(_program, ProgramInfo.GL_ATTACHED_SHADERS);
            if (res == 0)
            {
                var errorInfo = GL.GetProgramInfoLog(_program, 512);
            }
            GL.DeleteShader(shader);
        }
    }

}
