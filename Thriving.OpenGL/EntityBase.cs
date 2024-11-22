using System;
using Thriving.Geometry;

namespace Thriving.OpenGL
{

    public abstract class EntityBase
    {
        /// <summary>
        /// 局部坐标系
        /// </summary>
        protected Transform3D _transform = Transform3D.Identity;

        public float[] GetModelMatrix(bool transpose)
        {
            if (transpose)
            {
                return new float[]
                {
                    (float)_transform.BasisX.X,(float)_transform.BasisX.Y,(float)_transform.BasisX.Z,0f,
                     (float)_transform.BasisY.X, (float)_transform.BasisY.Y,(float)_transform.BasisY.Z,0f,
                     (float)_transform.BasisZ.X, (float)_transform.BasisZ.Y, (float)_transform.BasisZ.Z,0f,
                     (float)_transform.Origin.X,(float)_transform.Origin.Y,(float)_transform.Origin.Z,1f
                };
            }
            else
            {
                return new float[]
                {
                    (float)_transform.BasisX.X,  (float)_transform.BasisY.X,  (float)_transform.BasisZ.X,  (float)_transform.Origin.X,
                    (float)_transform.BasisX.Y,  (float)_transform.BasisY.Y,  (float)_transform.BasisZ.Y  ,(float)_transform.Origin.Y,
                    (float)_transform.BasisX.Z,  (float)_transform.BasisY.Z, (float)_transform.BasisZ.Z  ,(float)_transform.Origin.Z,
                    0f, 0f, 0f ,1f
                };
            }
        }

        public Point3D Position
        {
            get => _transform.Origin;
            set { _transform = new Transform3D(value, _transform.BasisX, _transform.BasisY, _transform.BasisZ); }
        }

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
        private readonly  GeometryObject _geometry;
        private readonly BasicMaterial _material;
        public GeometryEntity(GeometryObject geometry, BasicMaterial material)
        {
            _geometry = geometry;
            _material = material;
        }

        public void Draw(Shader shader, CameraBase camera,bool normalized)
        {
            shader.Use();
            // 更新顶点着色器
            shader.UpdateVertexShader(this, camera);
            // 更新片段着色器
            shader.UpdateFragmentShader(_material);

            _geometry.Draw(normalized);
        }

    }
}
