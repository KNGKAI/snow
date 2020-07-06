
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public static readonly int ChunkSize = 32;

    public static Texture2D gridTexture;

    public static Texture2D GridTexture
    {
        get
        {
            if (gridTexture == null)
            {
                gridTexture = new Texture2D(2, 2) { filterMode = FilterMode.Point };

                gridTexture.SetPixel(0, 0, Color.gray);
                gridTexture.SetPixel(1, 1, Color.gray);

                gridTexture.SetPixel(0, 1, Color.white);
                gridTexture.SetPixel(1, 0, Color.white);

                gridTexture.Apply();
            }
            return (gridTexture);
        }
    }

    public static void Update(float x, float y, int radius = 1)
    {
        float a;
        float b;

        for (int i = -radius; i <= radius; i++)
        {
            for (int j = -radius; j <= radius; j++)
            {
                a = x + i * ChunkSize;
                b = y + j * ChunkSize;
                if (!ContainsChunk(a, b))
                {
                    AddChunk(a, b);
                }
            }
        }
    }

    public static void Destroy()
    {
        for (int i = 0; i < Chunks.Count; i++)
        {
            DestroyImmediate(Chunks[i].gameObject);
        }
        Chunks.Clear();
    }

    public static void AddChunk(float x, float y)
    {
        Chunk chunk;

        if (ContainsChunk(x, y))
        {
            return;
        }
        chunk = new GameObject("chunk").AddComponent<Chunk>();
        chunk.Init(x, y);
        Chunks.Add(chunk);
    }

    public static bool ContainsChunk(float x, float y)
    {
        Vector2Int position;

        position = Snap(x, y);
        for (int i = 0; i < Chunks.Count; i++)
        {
            if (Chunks[i].position == position)
            {
                return (true);
            }
        }
        return (false);
    }

    public static Chunk GetChunk(float x, float y)
    {
        Vector2Int position;

        position = Snap(x, y);
        for (int i = 0; i < Chunks.Count; i++)
        {
            if (Chunks[i].position == position)
            {
                return (Chunks[i]);
            }
        }
        return (null);
    }

    public static Vector2Int Snap(float x, float y)
    {
        return (new Vector2Int((int)(x - x % ChunkSize), (int)(y - y % ChunkSize)));
    }

    public static Chunk[] GetAllChunks()
    {
        return (FindObjectsOfType<Chunk>());
    }

    private static List<Chunk> chunks;

    public static List<Chunk> Chunks
    {
        get
        {
            if (chunks == null)
            {
                chunks = new List<Chunk>();
                chunks.AddRange(GetAllChunks());
            }
            return (chunks);
        }
    }

    private static List<ChunkModifier> modifiers;

    public static List<ChunkModifier> Modifiers
    {
        get
        {
            if (modifiers == null)
            {
                modifiers = new List<ChunkModifier>();
            }
            return (modifiers);
        }
    }

    public static void ClearModifiers()
    {
        Modifiers.Clear();
    }

    public static float GetModifierHeight(float x, float y)
    {
        float h;

        h = 0.0f;
        for (int i = 0; i < Modifiers.Count; i++)
        {
            h += Modifiers[i].GetHeight((int)x, (int)y);
        }
        return (h);
    }

    public static void AddModifier(ChunkModifier modifier)
    {
        Modifiers.Add(modifier);
    }

    private static int lod = 1;

    private static List<int> triangles;

    private static List<int> Triangles
    {
        get
        {
            if (triangles == null)
            {
                triangles = Terrain.GetTriangle(ChunkSize, lod);
            }
            return (triangles);
        }
    }

    private static List<Vector2> uvs;

    private static List<Vector2> UVs
    {
        get
        {
            if (uvs == null)
            {
                uvs = Terrain.GetUVs(ChunkSize, lod);
            }
            return (uvs);
        }
    }

    public Vector2Int position;

    public Vector2Int Position
    {
        get
        {
            return (position);
        }
    }

    private MeshRenderer render;

    public MeshRenderer Render
    {
        get
        {
            if (render == null)
            {
                render = GetComponent<MeshRenderer>();
            }
            return (render);
        }
    }

    private MeshFilter filter;

    public MeshFilter Filter
    {
        get
        {
            if (filter == null)
            {
                filter = GetComponent<MeshFilter>();
            }
            return (filter);
        }
    }

    private static Mesh ModifiedMesh(int x, int y)
    {
        List<Vector3> v;
        Vector3 p;
        Mesh m;

        v = Terrain.GetVertices(x, y, ChunkSize, lod);

        for (int i = 0; i < v.Count; i++)
        {
            p = v[i];
            p.y += GetModifierHeight(v[i].x, v[i].z);
            v[i] = p;
        }

        m = new Mesh();
        m.SetVertices(v);
        m.SetTriangles(Triangles, 0);
        m.SetUVs(0, UVs);
        m.RecalculateNormals();
        m.Optimize();

        return (m);
    }

    public void Init(float x, float y)
    {
        GameObject tree;
        Vector3 position;

        this.position = Snap(x, y);

        this.gameObject.AddComponent<MeshRenderer>();
        this.gameObject.AddComponent<MeshFilter>();

        this.Render.material = new Material(Shader.Find("Standard")) { mainTexture = GridTexture };
        this.Filter.mesh = ModifiedMesh(this.position.x, this.position.y);

        this.gameObject.AddComponent<MeshCollider>();

        //Place Trees
        int step = 2;
        for (int a = 0; a < Chunk.ChunkSize; a += step)
        {
            for (int b = 0; b < Chunk.ChunkSize; b += step)
            {
                if (Random.Range(0, 100) < 10)
                {
                    position.x = this.position.x + a;
                    position.y = 10;
                    position.z = this.position.y + b;
                    if (Physics.Raycast(position, Vector3.down, out RaycastHit hit))
                    {
                        if (Terrain.GetTerrainHeight(position.x, position.z) < hit.point.y)
                        {
                            tree = GameObject.Instantiate<GameObject>(Game.Tree);
                            tree.transform.position = hit.point;
                            tree.transform.parent = transform;
                        }
                    }
                }
            }
        }
    }
}

public struct ChunkModifier
{
    private Vector2Int position;

    private int size;

    public int Size
    {
        get
        {
            return (size);
        }
    }

    private float[] nodes;

    public float[] Nodes
    {
        get
        {
            if (nodes == null)
            {
                nodes = new float[size * size];
            }
            return (nodes);
        }
    }

    public ChunkModifier(Vector2Int position, int size)
    {
        this.position = position;
        this.size = size;
        this.nodes = null;
    }

    private int Index(int x, int y, bool local)
    {
        if (local)
        {
            return (x + y * size);
        }

        x -= position.x;
        y -= position.y;
        return (x + y * size);
    }

    public bool Inside(int x, int y, bool local = false)
    {
        if (local)
        {
            return (x >= 0 && y >= 0 && x < size && y < size);
        }
        return (x >= position.x && y >= position.y && x < position.x + size && y < position.y + size);
    }

    public float GetHeight(int x, int y, bool local = false)
    {
        if (Inside(x, y, local))
        {
            return (Nodes[Index(x, y, local)]);
        }
        return (0);
    }

    public void SetHeight(int x, int y, float value, bool local = false)
    {
        if (Inside(x, y, local))
        {
            Nodes[Index(x, y, local)] = value;
        }
    }

    public void AddHeight(int x, int y, float value, bool local = false)
    {
        if (Inside(x, y, local))
        {
            Nodes[Index(x, y, local)] += value;
        }
    }

    public void SetRange(int x, int y, int size, float[] a, bool local = false)
    {
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                SetHeight(x + i, y + j, a[i + j * size], local);
            }
        }
    }

    public void AddRange(int x, int y, int size, float[] a, bool local = false)
    {
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                AddHeight(x + i, y + j, a[i + j * size], local);
            }
        }
    }

    public void DrawDot(int x, int y, int radius, float hardness, float value)
    {
        int x0;
        int y0;
        float t;

        x0 = x - radius;
        y0 = y - radius;
        for (int a = -radius; a <= radius; a++)
        {
            for (int b = -radius; b <= radius; b++)
            {
                t = 1 - (float)(Mathf.Abs(a) + Mathf.Abs(b)) / 2.0f / radius;
                AddHeight(x0 + a, y0 + b, value * t);
            }
        }
    }

    public void DrawInvertHeight(int x, int y, int radius)
    {
        Vector2 v;
        int x0;
        int y0;
        float t;

        v = Vector2.zero;
        x0 = x;
        y0 = y;
        for (int a = -radius; a <= radius; a++)
        {
            for (int b = -radius; b <= radius; b++)
            {
                v.x = a;
                v.y = b;
                t = 1 - Mathf.Clamp(v.magnitude / radius, 0.0f, 1.0f);
                SetHeight(x + a, y + b, Mathf.Lerp(GetHeight(x0 + a, y0 + b), -Terrain.GetHeight(x0 + a, y0 + b), t));// * (v.magnitude / radius));// Mathf.Lerp(Terrain.GetSlope(y0 + b), Terrain.GetTerrainHeight(x0 + a, y0 + b), t + 0.1f));
            }
        }
    }

    public void DrawLevelPath(Level level, int step = 1)
    {
        Vector2Int p;

        for (int i = 0; i < level.Path.Lenght; i += step)
        {
            p = level.Path.Evaluate(i);
            DrawInvertHeight(p.x, p.y, level.Radius);
        }
    }
}