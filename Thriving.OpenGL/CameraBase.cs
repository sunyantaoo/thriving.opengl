using Thriving.Geometry;

namespace Thriving.OpenGL
{
    public abstract class CameraBase:EntityBase
    {
        /// <summary>
        /// 相机在视口中显示的图形
        /// </summary>
        private readonly GeometryObject? _geometry;

        protected CameraBase(Point3D position, Vector3D lookat, Vector3D up)
        {
            var basisX = lookat.CrossProduct(up).Normalize();
            var basisY = up.IsNormalize() ? up : up.Normalize();
            var basisZ = lookat.IsNormalize() ? lookat : lookat.Normalize();

            // 左手坐标系
            _transform = new Transform3D(position, basisX, basisY, basisZ);

            ZNear = 1.0;
            ZFar = 1000.0;
        }

        /// <summary>
        /// 相机指向方向，相机局部坐标的Z轴正方向
        /// </summary>
        public Vector3D LookAt { get => _transform.BasisZ; }
        /// <summary>
        /// 相机向上方向，相机局部坐标的Y轴正方向
        /// </summary>
        public Vector3D Up { get => _transform.BasisY; }

        /// <summary>
        /// 左手坐标系X轴方向
        /// </summary>
        public Vector3D Right { get => _transform.BasisX; }

        /// <summary>
        /// 近平面
        /// </summary>
        public double ZNear { get; set; }
        /// <summary>
        /// 远平面
        /// </summary>
        public double ZFar { get; set; }

        /// <summary>
        /// 获取相机的投影矩阵
        /// </summary>
        /// <returns></returns>
        public abstract float[] GetProjectionMatrix();

        public abstract float[] GetTestMatrix();

        /// <summary>
        /// 绕相机X轴方向旋转，相机上下倾角
        /// </summary>
        /// <param name="angle"></param>
        public void Pitch(double angle)
        {
            var rotate = Transform3D.CreateRotation(Right, angle);
            _transform = _transform.Multiply(rotate);
        }

        /// <summary>
        /// 绕相机Up方向旋转，相机左右倾角
        /// </summary>
        /// <param name="angle"></param>
        public void Yaw(double angle)
        {
            var rotate = Transform3D.CreateRotation(Up, angle);
          _transform=_transform.Multiply(rotate);
        }

        /// <summary>
        /// 绕相机LookAt方向旋转，侧滚倾角
        /// </summary>
        /// <param name="angle"></param>
        public void Roll(double angle)
        {
            var rotate = Transform3D.CreateRotation(LookAt, angle);
            _transform = _transform.Multiply(rotate);
        }

        public abstract Vector3D? GetRay(double x, double y);
    }

    public class PerspectiveCamera : CameraBase
    {
        public PerspectiveCamera(Point3D position, Vector3D lookat, Vector3D up, double fov, double aspect) : base(position, lookat, up)
        {
            this.fov = fov;
            this.aspect = aspect;
        }

        /// <summary>
        /// 视野 field of view
        /// </summary>
        public double fov;
        /// <summary>
        /// 宽高比
        /// </summary>
        public double aspect;

        /// <summary>
        /// 获取过视点及屏幕坐标的向量
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public override Vector3D? GetRay(double x, double y)
        {
            var top = ZNear * Math.Tan(Math.PI * fov / 360);
            var right = aspect * top;

            var res = _transform.OfVector(new Vector3D(Position, new Point3D(x * right, y * top, ZNear)));
            return res;
        }

        public override float[] GetProjectionMatrix()
        {
            // 点到中间平面的距离为z坐标
            double na = LookAt.X, nb = LookAt.Y, nc = LookAt.Z;
            var nd = LookAt.X * Position.X + LookAt.Y * Position.Y + LookAt.Z * Position.Z;
            var np = Math.Sqrt(na * na + nb * nb + nc * nc);

            // 点到XZ平面的距离为y坐标
            double ma = Up.X, mb = Up.Y, mc = Up.Z;
            var md = Up.X * Position.X + Up.Y * Position.Y + Up.Z * Position.Z;
            var mp = Math.Sqrt(ma * ma + mb * mb + mc * mc);

            // 点到YZ平面的距离为x坐标
            var basisX = Up.CrossProduct(LookAt).Normalize();
            double ta = basisX.X, tb = basisX.Y, tc = basisX.Z;
            var td = basisX.X * Position.X + basisX.Y * Position.Y + basisX.Z * Position.Z;
            var tp = Math.Sqrt(ta * ta + tb * tb + tc * tc);

            var top = ZNear * Math.Tan(Math.PI * fov / 360);
            var bottom = -top;
            var right = aspect * top;
            var left = aspect * bottom;

            var sacleX = (2.0) / (right - left);
            var sacleY = (2.0) / (top - bottom);
            var sacleZ = (2.0) / (ZFar - ZNear);

            var m6 = new double[4, 4]
            {
                {sacleX*ZNear*ta/tp, sacleX*ZNear* tb/tp,sacleX*ZNear* tc/tp,-sacleX*ZNear*td/tp },
                {sacleY*ZNear*ma/mp, sacleY*ZNear*mb/mp, sacleY*ZNear*mc/mp,-sacleY*ZNear*md/mp },
                {sacleZ* na/np,sacleZ* nb/np,sacleZ*nc/np,-sacleZ*(nd/np+0.5*(ZNear+ZFar))},
                { na/np, nb/np,nc/np,-nd/np}
            };
            var result = new float[]
             {
                (float)m6[0,0],(float)m6[1,0],(float)m6[2,0],(float)m6[3,0],
                (float)m6[0,1],(float)m6[1,1],(float)m6[2,1],(float)m6[3,1],
                (float)m6[0,2],(float)m6[1,2],(float)m6[2,2],(float)m6[3,2],
                (float)m6[0,3],(float)m6[1,3],(float)m6[2,3],(float)m6[3,3],
             };
            return result;
        }

        public override float[] GetTestMatrix()
        {
            // 相机位于原点处的矩阵

            // 点到中间平面的距离为z坐标
            double na = LookAt.X, nb = LookAt.Y, nc = LookAt.Z;
            var nd = 0;
            var np = Math.Sqrt(na * na + nb * nb + nc * nc);

            // 点到XZ平面的距离为y坐标
            double ma = Up.X, mb = Up.Y, mc = Up.Z;
            var md = 0;
            var mp = Math.Sqrt(ma * ma + mb * mb + mc * mc);

            // 点到YZ平面的距离为x坐标
            var basisX = Up.CrossProduct(LookAt).Normalize();
            double ta = basisX.X, tb = basisX.Y, tc = basisX.Z;
            var td = 0;
            var tp = Math.Sqrt(ta * ta + tb * tb + tc * tc);

            var top = ZNear * Math.Tan(Math.PI * fov / 360);
            var bottom = -top;
            var right = aspect * top;
            var left = aspect * bottom;

            var sacleX = (2.0) / (right - left);
            var sacleY = (2.0) / (top - bottom);
            var sacleZ = (2.0) / (ZFar - ZNear);

            var m6 = new double[4, 4]
            {
                {sacleX*ZNear*ta/tp, sacleX*ZNear* tb/tp,sacleX*ZNear* tc/tp,-sacleX*ZNear*td/tp },
                {sacleY*ZNear*ma/mp, sacleY*ZNear*mb/mp, sacleY*ZNear*mc/mp,-sacleY*ZNear*md/mp },
                {na/np, nb/np,nc/np,-nd/np},
                { na/np, nb/np,nc/np,-nd/np}
            };
            var result = new float[]
             {
                (float)m6[0,0],(float)m6[1,0],(float)m6[2,0],(float)m6[3,0],
                (float)m6[0,1],(float)m6[1,1],(float)m6[2,1],(float)m6[3,1],
                (float)m6[0,2],(float)m6[1,2],(float)m6[2,2],(float)m6[3,2],
                (float)m6[0,3],(float)m6[1,3],(float)m6[2,3],(float)m6[3,3],
             };
            return result;
        }
    }

    public class OrthographicCamera : CameraBase
    {
        public OrthographicCamera(Point3D position, Vector3D lookat, Vector3D up) : base(position, lookat, up)
        {

        }

        public double left;
        public double right;
        public double top;
        public double bottom;

        public override float[] GetProjectionMatrix()
        {
            var nOrigin = Position.Add(0.5 * (ZFar + ZNear) * LookAt);
            // 点到中间平面的距离为z坐标
            double na = LookAt.X, nb = LookAt.Y, nc = LookAt.Z;
            var nd = LookAt.X * Position.X + LookAt.Y * Position.Y + LookAt.Z * Position.Z;
            var np = Math.Sqrt(na * na + nb * nb + nc * nc);

            // 点到XZ平面的距离为y坐标
            double ma = Up.X, mb = Up.Y, mc = Up.Z;
            var md = Up.X * nOrigin.X + Up.Y * nOrigin.Y + Up.Z * nOrigin.Z;
            var mp = Math.Sqrt(ma * ma + mb * mb + mc * mc);

            // 点到YZ平面的距离为x坐标
            var basisX = Up.CrossProduct(LookAt).Normalize();
            double ta = basisX.X, tb = basisX.Y, tc = basisX.Z;
            var td = basisX.X * nOrigin.X + basisX.Y * nOrigin.Y + basisX.Z * nOrigin.Z;
            var tp = Math.Sqrt(ta * ta + tb * tb + tc * tc);

            var matrix = new double[4, 4]
            {
                {ta/tp, tb/tp, tc/tp,-td/tp },
                {ma/mp,mb/mp, mc/mp,-md/mp },
                {na/ np, nb / np, nc / np,-nd/np },
                { 0,0,0,1}
            };
            var mat2 = new double[4, 4]
            {
                {2.0/(right-left) ,0,0,0},
                {0 ,2.0/(top-bottom),0,0},
                {0 ,0,2.0 /(ZFar-ZNear),0},
                {0 ,0,0,1},
            };

            var m6 = new Matrix(mat2) * new Matrix(matrix);
            var result = new float[]
             {
                (float)m6[0,0],(float)m6[1,0],(float)m6[2,0],(float)m6[3,0],
                (float)m6[0,1],(float)m6[1,1],(float)m6[2,1],(float)m6[3,1],
                (float)m6[0,2],(float)m6[1,2],(float)m6[2,2],(float)m6[3,2],
                (float)m6[0,3],(float)m6[1,3],(float)m6[2,3],(float)m6[3,3],
             };
            return result;
        }

        public override Vector3D? GetRay(double x, double y)
        {
            throw new NotImplementedException();
        }

        public override float[] GetTestMatrix()
        {
            var nOrigin = Position.Add(0.5 * (ZFar + ZNear) * LookAt);
            // 点到中间平面的距离为z坐标
            double na = LookAt.X, nb = LookAt.Y, nc = LookAt.Z;
            var nd = LookAt.X * Position.X + LookAt.Y * Position.Y + LookAt.Z * Position.Z;
            var np = Math.Sqrt(na * na + nb * nb + nc * nc);

            // 点到XZ平面的距离为y坐标
            double ma = Up.X, mb = Up.Y, mc = Up.Z;
            var md = Up.X * nOrigin.X + Up.Y * nOrigin.Y + Up.Z * nOrigin.Z;
            var mp = Math.Sqrt(ma * ma + mb * mb + mc * mc);

            // 点到YZ平面的距离为x坐标
            var basisX = Up.CrossProduct(LookAt).Normalize();
            double ta = basisX.X, tb = basisX.Y, tc = basisX.Z;
            var td = basisX.X * nOrigin.X + basisX.Y * nOrigin.Y + basisX.Z * nOrigin.Z;
            var tp = Math.Sqrt(ta * ta + tb * tb + tc * tc);

            var matrix = new double[4, 4]
            {
                {ta/tp, tb/tp, tc/tp,-td/tp },
                {ma/mp,mb/mp, mc/mp,-md/mp },
                {na/ np, nb / np, nc / np,-nd/np },
                { 0,0,0,1}
            };
            var mat2 = new double[4, 4]
            {
                {2.0/(right-left) ,0,0,0},
                {0 ,2.0/(top-bottom),0,0},
                {0 ,0,2.0 /(ZFar-ZNear),0},
                {0 ,0,0,1},
            };

            var m6 = new Matrix(mat2) * new Matrix(matrix);
            var result = new float[]
             {
                (float)m6[0,0],(float)m6[1,0],(float)m6[2,0],(float)m6[3,0],
                (float)m6[0,1],(float)m6[1,1],(float)m6[2,1],(float)m6[3,1],
                (float)m6[0,2],(float)m6[1,2],(float)m6[2,2],(float)m6[3,2],
                (float)m6[0,3],(float)m6[1,3],(float)m6[2,3],(float)m6[3,3],
             };
            return result;
        }
    }
}
