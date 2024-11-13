using System.Drawing;

namespace Thriving.OpenGL
{
    public abstract class LightBase
    {
        /// <summary>
        /// 环境光光强系数[0,1]
        /// </summary>
        public float AmbientFactor { get; set; }
        /// <summary>
        /// 环境光颜色
        /// </summary>
        public Color AmbientColor { get; set; }

        /// <summary>
        /// 漫反射系数[0,1]
        /// </summary>
        public float DiffuseFactor { get; set; }
        /// <summary>
        /// 漫反射颜色
        /// </summary>
        public Color DiffuseColor { get; set; }

        ///  <summary>
        /// 镜面反射系数
        /// </summary>
        public float SpecularFactor { get; set; }
        ///  <summary>
        /// 反射光颜色
        /// </summary>
        public Color SpecularColor { get; set; }

        /// <summary>
        /// 配置shader光源参数
        /// </summary>
        /// <param name="shader"></param>
        public abstract void Configure(Shader shader);

    }

    public class PointLight : LightBase
    {
        public override void Configure(Shader shader)
        {
            throw new NotImplementedException();
        }
    }

    public class SpotLight : LightBase
    {
        public override void Configure(Shader shader)
        {
            throw new NotImplementedException();
        }
    }

    public class DirectionalLight : LightBase
    {
        public override void Configure(Shader shader)
        {
            throw new NotImplementedException();
        }
    }
}
