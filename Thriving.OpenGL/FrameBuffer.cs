namespace Thriving.OpenGL
{

    public enum FrameBufferTarget
    {
        GL_READ_FRAMEBUFFER = 0x8CA8,
        GL_DRAW_FRAMEBUFFER = 0x8CA9,
        GL_FRAMEBUFFER = 0x8D40,
    }

    public enum FrameBufferAttachment
    {
        GL_COLOR_ATTACHMENT0 = 0x8CE0,
        GL_COLOR_ATTACHMENT1,
        GL_COLOR_ATTACHMENT2,
        GL_COLOR_ATTACHMENT3,
        GL_COLOR_ATTACHMENT4,
        GL_COLOR_ATTACHMENT5,
        GL_COLOR_ATTACHMENT6,
        GL_COLOR_ATTACHMENT7,
        GL_COLOR_ATTACHMENT8,
        GL_COLOR_ATTACHMENT9,
        GL_COLOR_ATTACHMENT10,
        GL_COLOR_ATTACHMENT11,
        GL_COLOR_ATTACHMENT12,
        GL_COLOR_ATTACHMENT13,
        GL_COLOR_ATTACHMENT14,
        GL_COLOR_ATTACHMENT15,
        GL_COLOR_ATTACHMENT16,
        GL_COLOR_ATTACHMENT17,
        GL_COLOR_ATTACHMENT18,
        GL_COLOR_ATTACHMENT19,
        GL_COLOR_ATTACHMENT20,
        GL_COLOR_ATTACHMENT21,
        GL_COLOR_ATTACHMENT22,
        GL_COLOR_ATTACHMENT23,
        GL_COLOR_ATTACHMENT24,
        GL_COLOR_ATTACHMENT25,
        GL_COLOR_ATTACHMENT26,
        GL_COLOR_ATTACHMENT27,
        GL_COLOR_ATTACHMENT28,
        GL_COLOR_ATTACHMENT29,
        GL_COLOR_ATTACHMENT30,
        GL_COLOR_ATTACHMENT31,
        GL_DEPTH_ATTACHMENT = 0x8D00,
        GL_STENCIL_ATTACHMENT = 0x8D20,
        GL_DEPTH_STENCIL_ATTACHMENT = 0x821A,
    }


    public enum FrameBufferStatus
    {
        GL_FRAMEBUFFER_COMPLETE = 0x8CD5,
        GL_FRAMEBUFFER_UNDEFINED = 0x8219,
        GL_FRAMEBUFFER_INCOMPLETE_ATTACHMENT = 0x8CD6,
        GL_FRAMEBUFFER_INCOMPLETE_MISSING_ATTACHMENT = 0x8CD7,
        GL_FRAMEBUFFER_INCOMPLETE_DRAW_BUFFER = 0x8CDB,
        GL_FRAMEBUFFER_INCOMPLETE_READ_BUFFER = 0x8CDC,
        GL_FRAMEBUFFER_UNSUPPORTED = 0x8CDD,
        GL_FRAMEBUFFER_INCOMPLETE_MULTISAMPLE = 0x8D56,
        GL_FRAMEBUFFER_INCOMPLETE_LAYER_TARGETS = 0x8DA8,
    }


    public class FrameBuffer
    {
        private readonly uint[] fbo;
        private readonly uint[] rbo;
        private readonly uint[] tex;
        public FrameBuffer()
        {
            fbo = GL.GenFramebuffers(1);
            tex = GL.GenTextures(1);
            rbo = GL.GenRenderbuffers(1);
        }

        ~FrameBuffer()
        {
            GL.DeleteTextures(1, tex);
            GL.DeleteRenderbuffers(1, rbo);
            GL.DeleteFramebuffers(1, fbo);
        }


        public void Bind(int width,int height)
        {
            GL.BindFramebuffer(FrameBufferTarget.GL_FRAMEBUFFER, fbo[0]);

            GL.BindTexture(TextureTarget.GL_TEXTURE_2D, tex[0]);
            GL.TexImage2D(TextureTarget.GL_TEXTURE_2D, 0, TextureFormat.GL_RGB, width, height, TextureFormat.GL_RGB, DataType.GL_UNSIGNED_BYTE, IntPtr.Zero);

            GL.FramebufferTexture2D(FrameBufferTarget.GL_FRAMEBUFFER, FrameBufferAttachment.GL_COLOR_ATTACHMENT0, TextureTarget.GL_TEXTURE_2D, tex[0], 0);

            GL.BindRenderbuffer(rbo[0]);
            GL.RenderbufferStorage(TextureFormat.GL_DEPTH_STENCIL, width, height);
            GL.FramebufferRenderbuffer(FrameBufferTarget.GL_FRAMEBUFFER, FrameBufferAttachment.GL_DEPTH_STENCIL_ATTACHMENT, rbo[0]);
        }


    }
}
