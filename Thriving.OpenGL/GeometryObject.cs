using System.Runtime.InteropServices;
using Thriving.Geometry;

namespace Thriving.OpenGL
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Color
    {
        private byte _red, _green, _blue, _alpha;

        public byte R
        {
            readonly get => _red ;
            set { _red = value; }
        }

        public byte G
        {
            readonly get => _green ;
            set { _green = value ; }
        }

        public byte B
        {
            readonly get => _blue ;
            set { _blue = value ; }
        }

        public byte A
        {
            readonly get => _alpha ;
            set { _alpha = value ; }
        }

        public Color Mix(Color other)
        {
            // 颜色混合 换算成[0-1]后相乘
            return new Color()
            {
                _red = (byte)(this._red * other._red / 255),
                _green = (byte)(this._green * other._green / 255),
                _blue = (byte)(this._blue * other._blue / 255),
            };
        }

    }

    [StructLayout(LayoutKind.Explicit)]
    public struct Vertex
    {
        [FieldOffset(0)] internal Point3D _position;
        [FieldOffset(24)] internal Vector3D _normal;
        [FieldOffset(48)] internal Vector2D _uv;
        [FieldOffset(64)] internal Vector3D _color;

        public Point3D Position
        {
            readonly get => _position;
            set { _position = value; }
        }
        public Vector3D Normal
        {
            readonly get => _normal;
            set { _normal = value; }
        }
        public Vector2D UV
        {
            readonly get => _uv;
            set { _uv = value; }
        }
        public Vector3D Color
        {
            readonly get => _color;
            set { _color = value; }
        }
    }

    /// <summary>
    /// 图形对象
    /// </summary>
    public class GeometryObject
    {
        /// <summary>
        /// 顶点数组
        /// </summary>
        private readonly uint[] _vaos;
        public GeometryObject()
        {
            _vaos = GL.GenVertexArrays(1);
        }

        ~GeometryObject()
        {
            GL.DeleteVertexArrays(_vaos);
            if (_vbos != null) GL.DeleteBuffers(_vbos);
            if (_ebos != null) GL.DeleteBuffers(_ebos);
        }

        /// <summary>
        /// 顶点缓存
        /// </summary>
        private uint[]? _vbos;
        /// <summary>
        /// 索引缓存
        /// </summary>
        private uint[]? _ebos;

        public uint Id { get => _vaos[0]; }

        /// <summary>
        /// 将顶点数据写入GPU
        /// </summary>
        /// <param name="vertices"></param>
        public unsafe void SetupVertexBuffer(Vertex[] vertices)
        {
            GL.BindVertexArray(_vaos);
            {
                _vbos = GL.GenBuffers(1);
                GL.BindBuffer(BufferType.GL_ARRAY_BUFFER, _vbos[0]);
                GL.BufferData(BufferType.GL_ARRAY_BUFFER, vertices, BufferUsage.GL_STATIC_DRAW);

                //顶点着色器position属性 layout (location = 0)
                GL.VertexAttribPointer(0, 3, DataType.GL_DOUBLE, false, Marshal.SizeOf<Vertex>(), 0);
                GL.EnableVertexAttribArray(0);

                // 顶点着色器normal属性 layout (location = 1)
                GL.VertexAttribPointer(1, 3, DataType.GL_DOUBLE, false, Marshal.SizeOf<Vertex>(), (Marshal.OffsetOf<Vertex>(nameof(Vertex._normal)).ToInt32()));
                GL.EnableVertexAttribArray(1);

                // 顶点着色器uv属性 layout (location = 2)
                GL.VertexAttribPointer(2, 2, DataType.GL_DOUBLE, false, Marshal.SizeOf<Vertex>(), (Marshal.OffsetOf<Vertex>(nameof(Vertex._uv)).ToInt32()));
                GL.EnableVertexAttribArray(2);

                // 顶点着色器color属性 layout (location = 3)
                GL.VertexAttribPointer(3, 3, DataType.GL_DOUBLE, false, Marshal.SizeOf<Vertex>(), (Marshal.OffsetOf<Vertex>(nameof(Vertex._color)).ToInt32()));
                GL.EnableVertexAttribArray(3);
            }
            GL.BindVertexArray(Array.Empty<uint>());
        }

        /// <summary>
        /// 将索引数据写入GPU
        /// </summary>
        /// <param name="indices"></param>
        public unsafe void SetupIndexBuffer(uint[] indices)
        {
            GL.BindVertexArray(_vaos);
            {
                _ebos = GL.GenBuffers(1);
                GL.BindBuffer(BufferType.GL_ELEMENT_ARRAY_BUFFER, _ebos[0]);
                GL.BufferData(BufferType.GL_ELEMENT_ARRAY_BUFFER, indices, BufferUsage.GL_STATIC_DRAW);
            }
            GL.BindVertexArray(Array.Empty<uint>());
        }


        internal void Draw()
        {
            GL.BindVertexArray(_vaos);
            {
                // 根据图形的不同特性绘制
                GL.DrawArrays(DrawMode.GL_TRIANGLES, 0, 36);
                //GL.DrawElements(DrawMode.GL_TRIANGLES, 6, indices);
            }
            GL.BindVertexArray(Array.Empty<uint>());
        }

    }

    /// <summary>
    /// 四面体，四个顶点，四个三角面，
    /// </summary>
    public class Tetrahedron : GeometryObject
    {

    }

}
