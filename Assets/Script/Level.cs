using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Level
{
    public readonly static int MinLevelLength = 4;
    public readonly static int MaxLevelLength = 100;

    public readonly static int MinLevelSlope = 15;
    public readonly static int MaxLevelSlope = 65;

    public static System.Random r;

    public static Level defaultLevel;

    public static Level Default
    {
        get
        {
            if (defaultLevel == null)
            {
                defaultLevel = new Level();

                r = new System.Random(0);
                for (int i = 1; i < 6; i++)
                {
                    defaultLevel.Path.Nodes.Add(new Vector2Int(r.Next(-50, 50), i * 50));
                }

                defaultLevel.slope = 45;
                defaultLevel.radius = 10;
                defaultLevel.freq = 0.05f;
                defaultLevel.amplitude = 15;
            }
            return (defaultLevel);
        }
    }

    // Level.RandomLevel(GameMode.Collect, 10, 50, 100, Random.Range(0, 1024)));
    public static Level RandomLevel(GameMode mode, int maxLen, int stride, int range, int seed)
    {
        Level level;
        int len;

        r = new System.Random(seed);

        level = new Level();
        len = r.Next(MinLevelLength, Mathf.Clamp(maxLen, MinLevelLength, MaxLevelLength));
        for (int i = 1; i < len; i++)
        {
            level.Path.Nodes.Add(new Vector2Int(r.Next(-range, range), i * stride));
        }
        level.slope = r.Next(MinLevelSlope, MaxLevelSlope);
        level.radius = 10;
        level.freq = 0.05f;
        level.amplitude = 15;
        return (level);
    }

    private float slope;

    public float Slope
    {
        get
        {
            return (slope);
        }
    }

    private float freq;

    public float Freq
    {
        get
        {
            return (freq);
        }
    }

    private int amplitude;

    public int Amplitude
    {
        get
        {
            return (amplitude);
        }
    }

    private int radius;

    public int Radius
    {
        get
        {
            return (radius);
        }
    }

    private LevelPath path;

    public LevelPath Path
    {
        get
        {
            if (path == null)
            {
                path = new LevelPath();
            }
            return (path);
        }
    }

    private Texture2D texture;

    public Texture2D Preview
    {
        get
        {
            if (texture == null)
            {
                texture = Path.Preview();
            }
            return (texture);
        }
    }

    public void Apply()
    {
        Terrain.SetSlope(slope);
        Terrain.SetFreq(freq);
        Terrain.SetHeight(amplitude);
    }
}

public enum GameMode
{
    Race,
    TimeTrail,
    Collect
}

public class LevelPath
{
    private List<Vector2Int> nodes;

    public List<Vector2Int> Nodes
    {
        get
        {
            if (nodes == null)
            {
                nodes = new List<Vector2Int>() { Vector2Int.zero, Vector2Int.up * 10 };
            }
            return (nodes);
        }
    }

    private float length = -1;

    public float Lenght
    {
        get
        {
            if (length < 0)
            {
                float d;

                d = 0;
                for (int i = 0; i < Nodes.Count - 1; i++)
                {
                    d += Vector2Int.Distance(Nodes[i], Nodes[i + 1]);
                }
                length = d;
            }
            return (length);
        }
    }

    private Vector2Int offset;

    public Vector2Int Offset
    {
        get
        {
            if (offset.x == 0)
            {
                for (int i = 0; i < Nodes.Count; i++)
                {
                    if (Nodes[i].x < offset.x)
                    {
                        offset.x = Nodes[i].x;
                    }
                }
            }
            return (offset);
        }
    }

    private Vector2Int size;

    public Vector2Int Size
    {
        get
        {
            if (size.x == 0)
            {
                for (int i = 0; i < Nodes.Count; i++)
                {
                    if (Nodes[i].x > size.x)
                    {
                        size.x = Nodes[i].x;
                    }
                    if (Nodes[i].y > size.y)
                    {
                        size.y = Nodes[i].y;
                    }
                }
            }
            return (size);
        }
    }

    public Vector2Int Evaluate(float t)
    {
        Vector2Int a, b, c, d;
        float dis;
        int i;

        if (t <= 0 || Nodes.Count == 1)
        {
            return (Nodes[0]);
        }

        if (t >= Lenght)
        {
            return (Nodes[Nodes.Count - 1]);
        }

        for (i = 0; i < Nodes.Count - 1; i++)
        {
            dis = Vector2Int.Distance(Nodes[i], Nodes[i + 1]);
            if (t > dis)
            {
                t -= dis;
            }
            else
            {
                t /= dis;
                break;
            }
        }

        a = Nodes[i];
        b = Nodes[i + 1];
        c = new Vector2Int(a.x, (a.y + b.y) / 2);
        d = new Vector2Int(b.x, c.y);

        return (BezierCurve(new int[4] { a.x, c.x, d.x, b.x }, new int[4] { a.y, c.y, d.y, b.y }, t));
    }

    public Texture2D Preview()
    {
        Texture2D texture;
        int x;
        int a;

        x = Offset.x;

        texture = new Texture2D(Size.x - Offset.x + previewBorder * 2 , Size.y + previewBorder * 2) { filterMode = FilterMode.Point };
        for (int i = 0; i < Lenght; i++)
        {
            Paint(ref texture, i);
        }
        texture.Apply();

        return (texture);
    }

    public void Paint(ref Texture2D texture, int t)
    {
        Vector2Int point;

        point = Evaluate(t);
        point.x += previewBorder - Offset.x;
        point.y += previewBorder;

        texture.SetPixel(point.x, point.y, Color.black);
        texture.SetPixel(point.x + 1, point.y, Color.black);
        texture.SetPixel(point.x, point.y + 1, Color.black);
        texture.SetPixel(point.x + 1, point.y + 1, Color.black);

        texture.Apply();
    }

    private readonly static int previewBorder = 100;

    private static Vector2Int BezierCurve(int[] x, int[] y, float t)
    {
        return (
            new Vector2Int
            (
            (int)(Mathf.Pow(1 - t, 3) * x[0] + 3 * t * Mathf.Pow(1 - t, 2) * x[1] + 3 * Mathf.Pow(t, 2) * (1 - t) * x[2] + Mathf.Pow(t, 3) * x[3]),
            (int)(Mathf.Pow(1 - t, 3) * y[0] + 3 * t * Mathf.Pow(1 - t, 2) * y[1] + 3 * Mathf.Pow(t, 2) * (1 - t) * y[2] + Mathf.Pow(t, 3) * y[3])
            )
       );
    }
}

