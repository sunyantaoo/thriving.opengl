using Thriving.OpenCV;
using static System.Net.Mime.MediaTypeNames;

namespace Thriving.OpenGL
{
    public enum TextureLayer
    {
        GL_TEXTURE0 = 0x84C0,
        GL_TEXTURE1,
        GL_TEXTURE2,
        GL_TEXTURE3,
        GL_TEXTURE4,
        GL_TEXTURE5,
        GL_TEXTURE6,
        GL_TEXTURE7,
        GL_TEXTURE8,
        GL_TEXTURE9,
        GL_TEXTURE10,
        GL_TEXTURE11,
        GL_TEXTURE12,
        GL_TEXTURE13,
        GL_TEXTURE14,
        GL_TEXTURE15,
        GL_TEXTURE16,
        GL_TEXTURE17,
        GL_TEXTURE18,
        GL_TEXTURE19,
        GL_TEXTURE20,
        GL_TEXTURE21,
        GL_TEXTURE22,
        GL_TEXTURE23,
        GL_TEXTURE24,
        GL_TEXTURE25,
        GL_TEXTURE26,
        GL_TEXTURE27,
        GL_TEXTURE28,
        GL_TEXTURE29,
        GL_TEXTURE30,
        GL_TEXTURE31,
    }

    public class Texture
    {
        /// <summary>
        /// 值为 深度组件 GL_DEPTH_COMPONENT or 模板索引 GL_STENCIL_INDEX
        /// </summary>
        private const int GL_DEPTH_STENCIL_TEXTURE_MODE = 0x90EA;
        /// <summary>
        /// 默认值为0
        /// </summary>
        private const int GL_TEXTURE_BASE_LEVEL = 0x813C;
        /// <summary>
        /// 默认值为1000
        /// </summary>
        private const int GL_TEXTURE_MAX_LEVEL = 0x813D;
        /// <summary>
        /// GL_COMPARE_REF_TO_TEXTURE   GL_NONE
        /// </summary>
        private const int GL_TEXTURE_COMPARE_MODE = 0x884C;
        /// <summary>
        /// GL_LEQUAL  GL_LEQUAL  GL_LEQUAL GL_LEQUAL  GL_LEQUAL  GL_LEQUAL  GL_LEQUAL  GL_LEQUAL
        /// </summary>
        private const int GL_TEXTURE_COMPARE_FUNC = 0x884D;
        /// <summary>
        /// 默认值为0.0
        /// </summary>
        private const int GL_TEXTURE_LOD_BIAS = 0x8501;
        /// <summary>
        /// GL_NEAREST  GL_LINEAR    GL_NEAREST_MIPMAP_NEAREST   GL_LINEAR_MIPMAP_NEAREST  GL_NEAREST_MIPMAP_LINEAR  GL_LINEAR_MIPMAP_LINEAR
        /// </summary>
        private const int GL_TEXTURE_MIN_FILTER = 0x2801;
        /// <summary>
        /// GL_NEAREST  GL_LINEAR
        /// </summary>
        private const int GL_TEXTURE_MAG_FILTER = 0x2800;
        /// <summary>
        /// 默认值为-1000
        /// </summary>
        private const int GL_TEXTURE_MIN_LOD = 0x813A;
        /// <summary>
        /// 默认值为1000
        /// </summary>
        private const int GL_TEXTURE_MAX_LOD = 0x813B;

        private const int GL_TEXTURE_SWIZZLE_R = 0x8E42;
        private const int GL_TEXTURE_SWIZZLE_G = 0x8E43;
        private const int GL_TEXTURE_SWIZZLE_B = 0x8E44;
        private const int GL_TEXTURE_SWIZZLE_A = 0x8E45;
        private const int GL_TEXTURE_WRAP_S = 0x2802;
        private const int GL_TEXTURE_WRAP_T = 0x2803;
        private const int GL_TEXTURE_WRAP_R = 0x8072;

        /// <summary>
        /// 默认值为(0.0f,0.0f,0.0f,0.0f)
        /// </summary>
        private const int GL_TEXTURE_BORDER_COLOR = 0x1004;

        private const int GL_TEXTURE_SWIZZLE_RGBA = 0x8E46;

        private readonly uint _texId;
        private readonly TextureTarget _target;
        public Texture(TextureTarget target)
        {
            _texId = GL.GenTextures(1)[0];
            _target = target;
        }

        public uint Id { get => _texId; }

        ~Texture()
        {
            GL.DeleteTextures(1, [_texId]);
        }

        public void Bind()
        {
            GL.BindTexture(_target, Id);
        }

        /// <summary>
        /// X轴环绕方式
        /// </summary>
        /// <param name="wrapType"></param>
        public void WrapX(WrapType wrapType)
        {
            GL.TexParameteri(_target, GL_TEXTURE_WRAP_S, (int)wrapType);
        }
        /// <summary>
        /// Y轴环绕方式
        /// </summary>
        /// <param name="wrapType"></param>
        public void WrapY(WrapType wrapType)
        {
            GL.TexParameteri(_target, GL_TEXTURE_WRAP_T, (int)wrapType);
        }
        /// <summary>
        /// Z轴环绕方式
        /// </summary>
        /// <param name="wrapType"></param>
        public void WrapZ(WrapType wrapType)
        {
            GL.TexParameteri(_target, GL_TEXTURE_WRAP_R, (int)wrapType);
        }

        /// <summary>
        /// 放大时过滤方式
        /// </summary>
        /// <param name="filterType"></param>
        public void MagnifyFilter(FilterType filterType)
        {
            GL.TexParameteri(_target, GL_TEXTURE_MAG_FILTER, (int)filterType);
        }
        /// <summary>
        /// 缩小时过滤方式
        /// </summary>
        /// <param name="filterType"></param>
        public void MinifyFilter(FilterType filterType)
        {
            GL.TexParameteri(_target, GL_TEXTURE_MIN_FILTER, (int)filterType);
        }

        public void SetBorderColor(float red, float green, float blue, float alpha)
        {
            var array = new float[4] { red, green, blue, alpha };
            GL.TexParameterfv(_target, GL_TEXTURE_BORDER_COLOR, array);
        }

        public void BindImage2D(string imagePath)
        {
            if (!File.Exists(imagePath)) throw new FileNotFoundException(string.Empty, imagePath);
            CVImage image = CVImage.Read(imagePath);
            if (image.Width % 2 != 0)
            {
                // image.Resize(new CVSize() { });
            }
            GL.BindTexture(_target, Id);
            GL.TexImage2D(_target, 0, TextureFormat.GL_RGB, image.Width, image.Height, TextureFormat.GL_BGR, DataType.GL_UNSIGNED_BYTE, image.Data);
            GL.GenerateMipmap(_target);
        }

        public void BindCubeMap(string right, string left, string top, string bottom, string front, string back)
        {
            if (!File.Exists(right)) throw new FileNotFoundException(string.Empty, right);
            if (!File.Exists(left)) throw new FileNotFoundException(string.Empty, left);
            if (!File.Exists(top)) throw new FileNotFoundException(string.Empty, top);
            if (!File.Exists(bottom)) throw new FileNotFoundException(string.Empty, bottom);
            if (!File.Exists(front)) throw new FileNotFoundException(string.Empty, front);
            if (!File.Exists(back)) throw new FileNotFoundException(string.Empty, back);

            CVImage rightImg = CVImage.Read(right);
            CVImage leftImg = CVImage.Read(left);
            CVImage topImg = CVImage.Read(top);
            CVImage bottomImg = CVImage.Read(bottom);
            CVImage frontImg = CVImage.Read(front);
            CVImage backImg = CVImage.Read(back);

            GL.BindTexture(_target, Id);
            GL.TexImage2D(TextureTarget.GL_TEXTURE_CUBE_MAP_POSITIVE_X, 0, TextureFormat.GL_RGB, rightImg.Width, rightImg.Height, TextureFormat.GL_BGR, DataType.GL_UNSIGNED_BYTE, rightImg.Data);
            GL.TexImage2D(TextureTarget.GL_TEXTURE_CUBE_MAP_NEGATIVE_X, 0, TextureFormat.GL_RGB, leftImg.Width, leftImg.Height, TextureFormat.GL_BGR, DataType.GL_UNSIGNED_BYTE, leftImg.Data);
            GL.TexImage2D(TextureTarget.GL_TEXTURE_CUBE_MAP_POSITIVE_Y, 0, TextureFormat.GL_RGB, topImg.Width, topImg.Height, TextureFormat.GL_BGR, DataType.GL_UNSIGNED_BYTE, topImg.Data);
            GL.TexImage2D(TextureTarget.GL_TEXTURE_CUBE_MAP_NEGATIVE_Y, 0, TextureFormat.GL_RGB, bottomImg.Width, bottomImg.Height, TextureFormat.GL_BGR, DataType.GL_UNSIGNED_BYTE, bottomImg.Data);
            GL.TexImage2D(TextureTarget.GL_TEXTURE_CUBE_MAP_POSITIVE_Z, 0, TextureFormat.GL_RGB, frontImg.Width, frontImg.Height, TextureFormat.GL_BGR, DataType.GL_UNSIGNED_BYTE, frontImg.Data);
            GL.TexImage2D(TextureTarget.GL_TEXTURE_CUBE_MAP_NEGATIVE_Z, 0, TextureFormat.GL_RGB, backImg.Width, backImg.Height, TextureFormat.GL_BGR, DataType.GL_UNSIGNED_BYTE, backImg.Data);


           GL.TexParameteri(_target, GL_TEXTURE_MIN_FILTER, (int)FilterType.GL_LINEAR);
           GL.TexParameteri(_target, GL_TEXTURE_MAG_FILTER, (int)FilterType.GL_LINEAR);
           GL.TexParameteri(_target, GL_TEXTURE_WRAP_S, (int)WrapType. GL_CLAMP_TO_EDGE);
           GL.TexParameteri(_target, GL_TEXTURE_WRAP_T, (int)WrapType.GL_CLAMP_TO_EDGE);
           GL.TexParameteri(_target, GL_TEXTURE_WRAP_R, (int)WrapType.GL_CLAMP_TO_EDGE);
            // GL.GenerateMipmap(_target);
        }
    }

    /// <summary>
    /// 环绕方式
    /// </summary>
    public enum WrapType
    {
        GL_REPEAT = 0x2901,
        GL_MIRRORED_REPEAT = 0x8370,
        GL_CLAMP_TO_EDGE = 0x812F,
        GL_CLAMP_TO_BORDER = 0x812D,
    }

    ///  <summary>
    /// 纹理过滤方式
    /// </summary>
    public enum FilterType
    {
        GL_NEAREST = 0x2600,
        GL_LINEAR = 0x2601,
        GL_NEAREST_MIPMAP_NEAREST = 0x2700,
        GL_LINEAR_MIPMAP_NEAREST = 0x2701,
        GL_NEAREST_MIPMAP_LINEAR = 0x2702,
        GL_LINEAR_MIPMAP_LINEAR = 0x2703,
    };
}
