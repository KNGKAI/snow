using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour
{
    private static List<Waypoint> waypoints;

    public static List<Waypoint> Waypoints
    {
        get
        {
            if (waypoints == null)
            {
                waypoints = new List<Waypoint>();
            }
            return (waypoints);
        }
    }

    public static void Destroy()
    {
        for (int i = 0; i < Waypoints.Count; i++)
        {
            DestroyImmediate(Waypoints[i].gameObject);
        }
        Waypoints.Clear();
    }

    public static Waypoint Create(Vector3 position, int size)
    {
        GameObject o;
        Waypoint w;

        o = new GameObject("waypoint");

        o.transform.position = position;
        o.transform.localScale = Vector3.one * size;

        o.AddComponent<BoxCollider>().isTrigger = true;
        o.AddComponent<MeshFilter>().mesh = Cube;
        o.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Standard")) { color = new Color(1.0f, 1.0f, 0.0f, 0.1f) };

        w = o.AddComponent<Waypoint>();
        Waypoints.Add(w);
        return (w);
    }
    private static Mesh cube;

    public static Mesh Cube
    {
        get
        {
            if (cube == null)
            {
                Mesh mesh;
                List<Vector3> v;
                List<int> t;

                Vector3[] vertices = {
                    new Vector3 (-0.5f, -0.5f, -0.5f),
                    new Vector3 (0.5f, -0.5f, -0.5f),
                    new Vector3 (0.5f, 0.5f, -0.5f),
                    new Vector3 (-0.5f, 0.5f, -0.5f),
                    new Vector3 (-0.5f, 0.5f, 0.5f),
                    new Vector3 (0.5f, 0.5f, 0.5f),
                    new Vector3 (0.5f, -0.5f, 0.5f),
                    new Vector3 (-0.5f, -0.5f, 1),
                };
                v = new List<Vector3>();
                v.AddRange(vertices);

                int[] triangles = {
                    0, 2, 1, //face front
	                0, 3, 2,
                    2, 3, 4, //face top
	                2, 4, 5,
                    1, 2, 5, //face right
	                1, 5, 6,
                    0, 7, 4, //face left
	                0, 4, 3,
                    5, 4, 7, //face back
	                5, 7, 6,
                    0, 6, 7, //face bottom
	                0, 1, 6
                };
                t = new List<int>();
                t.AddRange(triangles);

                mesh = new Mesh();
                mesh.SetVertices(v);
                mesh.SetTriangles(t, 0);
                cube = mesh;
            }
            return (cube);
        }
    }

    private Character _player;

    private Character Player
    {
        get
        {
            if (_player == null)
            {
                _player = Game.Player;
            }
            return (_player);
        }
    }

    private int Radius
    {
        get
        {
            return (Game.CurrentLevel.Radius);
        }
    }

    private void Update()
    {
        if (Player.transform.position.z > transform.position.z + Radius)
        {
            Game.GameOver();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (Player.gameObject == other.gameObject)
        {
            Waypoints.RemoveAt(0);
            Destroy(gameObject);
        }
    } 
}