using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Terrain
{
    private static float slope = 45.0f;

    public static float Slope
    {
        get
        {
            return (slope);
        }
    }

    public static void SetSlope(float a)
    {
        slope = Mathf.Clamp(a, 0.0f, 90.0f);
    }

    private static float height = 10.0f;

    public static float Height
    {
        get
        {
            return (height);
        }
    }

    public static void SetHeight(float a)
    {
        height = Mathf.Clamp(a, 0.0f, 20.0f);
    }

    private static float freq = 0.1f;

    public static float Freq
    {
        get
        {
            return (freq);
        }
    }

    public static void SetFreq(float a)
    {
        freq = Mathf.Clamp(a, 0.0f, 1.0f);
    }

    public static float GetValue(float x, float y)
    {
        x -= 1024.0f;
        y -= 1024.0f;
        return (Mathf.PerlinNoise(x * freq, y * freq));
    }

    public static float GetSlope(float y)
    {
        return (y * -Mathf.Sin(Mathf.Deg2Rad * slope));
    }

    public static float GetHeight(float x, float y)
    {
        return (GetValue(x, y) * height);
    }

    public static float GetTerrainHeight(float x, float y)
    {
        return (GetHeight(x, y) + GetSlope(y));
    }

    public static List<Vector3> GetVertices(int x, int y, int size, int lod = 1)
    {
        List<Vector3> v;
        int c, d, s;

        v = new List<Vector3>();

        lod = Mathf.Clamp(lod, 1, size);
        s = size / lod;

        for (int a = 0; a <= s; a++)
        {
            for (int b = 0; b <= s; b++)
            {
                c = x + (a == s ? size : a * lod);
                d = y + (b == s ? size : b * lod);

                v.Add(new Vector3(c, Terrain.GetTerrainHeight(c, d) + 0.1f, d));
            }
        }

        return (v);
    }

    public static List<int> GetTriangle(int size, int lod = 1)
    {
        List<int> t;
        int i;
        int s;

        t = new List<int>();

        lod = Mathf.Clamp(lod, 1, size);
        s = size / lod;

        for (int a = 0; a <= s; a++)
        {
            for (int b = 0; b <= s; b++)
            {
                if (a < s && b < s)
                {
                    i = a + b * (s + 1);

                    t.Add(i);
                    t.Add(i + 1);
                    t.Add(i + 2 + s);

                    t.Add(i);
                    t.Add(i + 2 + s);
                    t.Add(i + 1 + s);
                }
            }
        }

        return (t);
    }

    public static List<Vector2> GetUVs(int size, int lod = 1)
    {
        List<Vector2> u;
        int s;

        u = new List<Vector2>();

        lod = Mathf.Clamp(lod, 1, size);
        s = size / lod;

        for (int a = 0; a <= s; a++)
        {
            for (int b = 0; b <= s; b++)
            {
                u.Add(new Vector2(a / 5, b / 5));
            }
        }
        return (u);
    }

    public static Mesh GetMesh(int x, int y, int size, int lod = 1)
    {
        Mesh mesh;

        mesh = new Mesh();
        mesh.SetVertices(GetVertices(x, y, size, lod));
        mesh.SetTriangles(GetTriangle(size, lod), 0);
        mesh.SetUVs(0, GetUVs(size, lod));
        mesh.RecalculateNormals();
        mesh.Optimize();

        return (mesh);
    }
}