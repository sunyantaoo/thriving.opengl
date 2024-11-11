using Thriving.Geometry;

namespace Thriving.OpenGL
{

    public abstract class EntityBase
    {
        /// <summary>
        /// 局部坐标系
        /// </summary>
        protected Transform3D _transform = Transform3D.Identity;

        /// <summary>
        /// 移动
        /// </summary>
        /// <param name="offset"></param>
        public void Translate(Vector3D offset)
        {
            var target = Transform3D.CreateTranslation(offset);
            _transform = _transform.Multiply(target);
        }

        /// <summary>
        /// 旋转
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="angle"></param>
        public void Rotate(Vector3D axis, float angle)
        {
            var target = Transform3D.CreateRotation(axis, angle);
            _transform = _transform.Multiply(target);
        }

        /// <summary>
        /// 缩放
        /// </summary>
        /// <param name="scale"></param>
        public void Scale(double x, double y, double z)
        {
            var target = Transform3D.CreateScale(x, y, z);
            _transform = _transform.Multiply(target);
        }

    }



    public class GeometryEntity : EntityBase
    {
        private readonly GeometryObject _geometry;
        private readonly BasicMaterial _material;
        public GeometryEntity(GeometryObject geometry, BasicMaterial material)
        {
            _geometry = geometry;
            _material = material;
        }

        public void Draw()
        {
            GL.BindVertexArray(new uint[] { _geometry.Id });


            GL.BindVertexArray(Array.Empty<uint>());
        }

    }
}
