using System.Drawing;

namespace Thriving.OpenGL
{
    /// <summary>
    /// 基础材质
    /// </summary>
    public class BasicMaterial
    {
        /// <summary>
        /// 环境光光强系数[0,1]
        /// </summary>
        public float AmbientFactor { get; set; }
        /// <summary>
        ///  环境光颜色
        /// </summary>
        public Color AmbientColor { get; set; }
        /// <summary>
        /// 环境光贴图
        /// </summary>
        public Texture? AmbientMap { get; set; }
        /// <summary>
        ///  漫反射系数[0,1]
        /// </summary>
        public float DiffuseFactor { get; set; }
        ///  <summary>
        ///  漫反射颜色
        /// </summary>
        public Color DiffuseColor { get; set; }
        /// <summary>
        /// 漫反射贴图
        /// </summary>
        public Texture? DiffuseMap { get; set; }
        /// <summary>
        ///  镜面反射系数
        /// </summary>
        public float SpecularFactor { get; set; }
        /// <summary>
        ///  反射光颜色
        /// </summary>
        public Color SpecularColor { get; set; }
        /// <summary>
        /// 镜面反射贴图
        /// </summary>
        public Texture? SpecularMap { get; set; }
        ///  <summary>
        ///  物体光泽度，即镜面反射指数
        /// </summary>
        public float Shininess { get; set; }
    }
}
