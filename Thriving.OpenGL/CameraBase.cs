using System;
using System.Linq;
using Thriving.Geometry;

namespace Thriving.OpenGL
{
    public abstract class CameraBase
    {
        /// <summary>
        /// 相机位置
        /// </summary>
        public Point3D Position { get; set; }
        /// <summary>
        /// 相机指向方向，相机局部坐标的Z轴正方向
        /// </summary>
        public Vector3D LookAt { get; set; }
        /// <summary>
        /// 相机向上方向，相机局部坐标的Y轴正方向
        /// </summary>
        public Vector3D Up { get; set; }

        /// <summary>
        /// 左手坐标系X轴方向
        /// </summary>
        public Vector3D Right { get => LookAt.CrossProduct(Up).Normalize(); }

        /// <summary>
        /// 近平面
        /// </summary>
        public double ZNear { get; set; }
        /// <summary>
        /// 远平面
        /// </summary>
        public double ZFar { get; set; }

        /// <summary>
        /// 投影平面局部坐标系
        /// <code>以近平面为投影平面，相机向上方向为Y轴方向，相机指向方向的逆方向为Z轴方向</code>
        /// </summary>
        /// <returns></returns>
        public abstract float[] GetProjectionMatrix();

        /// <summary>
        /// 绕相机X轴方向旋转，相机上下倾角
        /// </summary>
        /// <param name="angle"></param>
        public void Pitch(double angle)
        {
            var rotate = Transform3D.CreateRotation(Right, angle);
            this.LookAt = rotate.OfVector(LookAt);
            this.Up = rotate.OfVector(Up);
        }

        /// <summary>
        /// 绕相机Up方向旋转，相机左右倾角
        /// </summary>
        /// <param name="angle"></param>
        public void Yaw(double angle)
        {
            var rotate = Transform3D.CreateRotation(Up, angle);
            this.LookAt = rotate.OfVector(LookAt);
        }

        /// <summary>
        /// 绕相机LookAt方向旋转，侧滚倾角
        /// </summary>
        /// <param name="angle"></param>
        public void Roll(double angle)
        {
            var rotate = Transform3D.CreateRotation(LookAt, angle);
            this.Up = rotate.OfVector(Up);
        }


    }

    public class PerspectiveCamera : CameraBase
    {
        /// <summary>
        /// 视野 field of view
        /// </summary>
        public double fov;
        /// <summary>
        /// 宽高比
        /// </summary>
        public double aspect;


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

            var top = ZNear * Math.Tan(Math.PI * 45 / 360);
            var bottom = -top;
            var right = aspect * top;
            var left = aspect * bottom;

            var matrix = new double[4, 4]
            {
                {ZNear*ta/tp,  ZNear*tb/tp,ZNear*tc/tp,-ZNear*td/tp },
                {ZNear*ma/mp, ZNear*mb/mp, ZNear*mc/mp,-ZNear*md/mp },
                { na/np, nb/np,nc/np,-nd/np-0.5*(ZFar+ZNear)},
                { na/np, nb/np,nc/np,-nd/np}
            };
            var mat2 = new double[4, 4]
            {
               {(2.0)/(right-left),0,0,0 },
               {0,(2.0)/(top-bottom),0,0 },
               {0,0,(2.0)/(ZFar-ZNear),0},
               { 0, 0,0,1}
            };
            var m6 = new Matrix(mat2) * new Matrix(matrix);
            var result = new float[]
             {
                (float)m6[0,0],(float)m6[1,0],(float)m6[2,0],(float)m6[3,0],
                (float)m6[0,1],(float)m6[1,1],(float)m6[2,1],(float)m6[3,1],
                (float)m6[0,2],(float)m6[1,2],(float)m6[2,2],(float)m6[3,2],
                (float)m6[0,3],(float)m6[1,3],(float)m6[2,3],(float)m6[3,3],
             };

            //var tanHalfFovy = (float)Math.Tan(Math.PI * fov / 360);

            //var result = new float[16];
            //result[0] = (float)(1.0f / (aspect * tanHalfFovy));
            //result[5] = 1.0f / (tanHalfFovy);
            //result[10] = (float)(-(ZFar + ZNear) / (ZFar - ZNear));
            //result[11] = -1.0f;
            //result[14] = (float)(-(2.0f * ZFar * ZNear) / (ZFar - ZNear));
            //result[15] = 1.0f;
            return result;
        }
    }

    public class OrthographicCamera : CameraBase
    {
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

            //var result = new float[16];
            //result[0] = (float)((2.0f) / (right - left));
            //result[5] = (float)((2.0f) / (top - bottom));
            //result[10] = (float)(-(2.0f) / (ZFar - ZNear));
            //result[12] = (float)(-(right + left) / (right - left));
            //result[13] = (float)(-(top + bottom) / (top - bottom));
            //result[14] = (float)(-(ZFar + ZNear) / (ZFar - ZNear));
            //result[15] = 1.0f;
            return result;
        }
    }
}
