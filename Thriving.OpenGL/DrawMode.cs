﻿namespace Thriving.OpenGL
{
    public enum DrawMode
    {
        GL_POINTS = 0x0000,
        GL_LINES = 0x0001,
        GL_LINE_LOOP = 0x0002,
        GL_LINE_STRIP = 0x0003,
        GL_TRIANGLES = 0x0004,
        GL_TRIANGLE_STRIP = 0x0005,
        GL_TRIANGLE_FAN = 0x0006,
        GL_LINES_ADJACENCY = 0x000A,
        GL_LINE_STRIP_ADJACENCY = 0x000B,
        GL_TRIANGLES_ADJACENCY = 0x000C,
        GL_TRIANGLE_STRIP_ADJACENCY = 0x000D,
        GL_PATCHES = 0x000E,
    }
}
