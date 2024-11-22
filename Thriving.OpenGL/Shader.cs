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
                var errorInfo = GL.GetProgramInfoLog(_program);
            }
        }

        public void Use()
        {
            GL.UseProgram(_program);
        }

        public void Complie(string src, ShaderType type)
        {
            var shader = GL.CreateShader(type);
            GL.ShaderSource(shader, 1, new string[] { src }, new int[] {src.Length });
            var res = GL.GetShaderiv(shader, ShaderInfo.GL_SHADER_SOURCE_LENGTH);
            if (res == 0)
            {
                var errorInfo = GL.GetShaderInfoLog(shader);
            }
            GL.CompileShader(shader);
            res = GL.GetShaderiv(shader, ShaderInfo.GL_COMPILE_STATUS);
            if (res == 0)
            {
                var errorInfo = GL.GetShaderInfoLog(shader);
            }
            GL.AttachShader(_program, shader);
            res = GL.GetProgramiv(_program, ProgramInfo.GL_ATTACHED_SHADERS);
            if (res == 0)
            {
                var errorInfo = GL.GetProgramInfoLog(_program);
            }
            GL.DeleteShader(shader);
        }


        public void UpdateVertexShader(EntityBase entity,CameraBase camera)
        {
            var location = GL.GetUniformLocation(_program, "model");
            if (location >= 0)
            {
                var tArray = entity.GetModelMatrix(false);
                GL.UniformMatrix4fv(location, 1, true, tArray);
            }

            location = GL.GetUniformLocation(_program, "projection");
            if (location >= 0)
            {
                var tArray = camera.GetProjectionMatrix();
                GL.UniformMatrix4fv(location, 1, false, tArray);
            }
        }

        public void UpdateFragmentShader(BasicMaterial material)
        {

        }
    }

}
