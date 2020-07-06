using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Game : MonoBehaviour
{
    private static Game instance;

    private static Game Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<Game>();
            }
            return (instance);
        }
    }

    private static Character player;

    public static Character Player
    {
        get
        {
            if (player == null)
            {
                player = MonoBehaviour.FindObjectOfType<Character>();
            }
            return (player);
        }
    }

    private static Camera camera;

    public static Camera Camera
    {
        get
        {
            if (camera == null)
            {
                camera = MonoBehaviour.FindObjectOfType<Camera>();
            }
            return (camera);
        }
    }

    private static Level currentLevel;

    public static Level CurrentLevel
    {
        get
        {
            return (currentLevel);
        }
    }

    private static void SetLevel(Level level)
    {
        currentLevel = level;
        currentLevel.Apply();
    }

    private static float levelTime;

    public static float LevelTime
    {
        get
        {
            return (levelTime);
        }
    }

    private static int playing;

    public static bool Playing
    {
        get
        {
            return (playing > 0);
        }
    }

    public static void GameOver()
    {
        if (playing == 1)
        {
            for (int x = 0; x < animatedLevelPreview.width; x++)
            {
                for (int y = 0; y < animatedLevelPreview.height; y++)
                {
                    animatedLevelPreview.SetPixel(x, y, Color.white);
                }
            }
            animatedLevelPreview.Apply();
            Instance.StartCoroutine(EndLevelEvent());
        }
    }

    private static IEnumerator EndLevelEvent()
    {
        playing = 2;
        levelTime = 0;
        yield return new WaitForSeconds(1);
        StartNewLevel();
        yield return null;
    }

    private static Texture2D animatedLevelPreview;

    private static Texture2D AnimatedLevelPreview
    {
        get
        {
            if (animatedLevelPreview == null)
            {
                animatedLevelPreview = new Texture2D(1, 1);
            }
            return (animatedLevelPreview);
        }
    }

    private static int loadingProcces;

    private static int loadingProccesLimit;

    public static int LoadingProcess
    {
        get
        {
            return (loadingProcces);
        }
    }

    public static float LoadingProgress
    {
        get
        {
            return ((float)loadingProcces / loadingProccesLimit);
        }
    }

    private static Texture2D loadingBar;

    private static Texture2D LoadingBar
    {
        get
        {
            if (loadingBar == null)
            {
                loadingBar = new Texture2D(1, 1);
                loadingBar.SetPixel(0, 0, Color.cyan);
                loadingBar.Apply();
            }
            return (loadingBar);
        }
    }

    public static void StartNewLevel()
    {
        SetLevel(Level.RandomLevel(GameMode.Collect, 5, 50, 100, Random.Range(0, 1024)));
        Instance.GenerateLevel();
    }

    private void GenerateLevel()
    {
        StopAllCoroutines();
        StartCoroutine(GenerateLevelProccess());
    }

    private static IEnumerator GenerateLevelProccess()
    {
        ChunkModifier m;
        Vector2Int a;
        Vector2Int b;
        Vector3 position;
        int r;
        
        playing = 0;
        levelTime = 0;

        r = CurrentLevel.Radius;
        loadingProcces = 0;
        loadingProccesLimit = ((int)CurrentLevel.Path.Lenght / r) + CurrentLevel.Path.Nodes.Count;

        animatedLevelPreview = CurrentLevel.Preview;
        for (int x = 0; x < animatedLevelPreview.width; x++)
        {
            for (int y = 0; y < animatedLevelPreview.height; y++)
            {
                animatedLevelPreview.SetPixel(x, y, Color.white);
            }
        }
        animatedLevelPreview.Apply();

        Chunk.Destroy();
        Waypoint.Destroy();

        //SetupModifiers()
        a = CurrentLevel.Path.Nodes[0];
        b = Vector2Int.zero;
        for (int i = 0; i < CurrentLevel.Path.Nodes.Count; i++)
        {
            if (CurrentLevel.Path.Nodes[i].x < a.x)
            {
                a.x = CurrentLevel.Path.Nodes[i].x;
            }
            if (CurrentLevel.Path.Nodes[i].x > b.x)
            {
                b.x = CurrentLevel.Path.Nodes[i].x;
            }
            if (CurrentLevel.Path.Nodes[i].y > b.y)
            {
                b.y = CurrentLevel.Path.Nodes[i].y;
            }
        }

        a -= Vector2Int.one * CurrentLevel.Radius * 2;
        b += Vector2Int.one * CurrentLevel.Radius * 2;

        m = new ChunkModifier(a, (int)Mathf.Max(b.x - a.x, b.y - a.y));
        m.DrawLevelPath(CurrentLevel, 4);
        yield return null;
        loadingProcces++;

        Chunk.ClearModifiers();
        Chunk.AddModifier(m);

        //GenerateChunks()
        for (int i = 1; i < CurrentLevel.Path.Lenght; i += r)
        {
            position = (Vector2)CurrentLevel.Path.Evaluate(i);
            for (int x = -r; x <= r; x += r)
            {
                for (int y = -r; y <= r; y += r)
                {
                    Chunk.Update(position.x + x, position.y + y, 2);
                }
            }

            for (int j = 0; j < r; j++)
            {
                CurrentLevel.Path.Paint(ref animatedLevelPreview, j + i);
            }
            yield return null;
            loadingProcces++;
        }

        //PlaceWaypoints()
        for (int i = 2; i < CurrentLevel.Path.Nodes.Count; i++)
        {
            position = (Vector2)CurrentLevel.Path.Nodes[i];
            position.z = position.y;
            position.y = Terrain.GetSlope(position.z);

            Waypoint.Create(position, CurrentLevel.Radius * 2);
            yield return null;
            loadingProcces++;
        }

        Player.transform.position = Vector3.forward;
        Player.transform.rotation = Quaternion.LookRotation(Vector3.forward);

        Camera.transform.position = Vector3.up;

        yield return null;
        loadingProcces++;

        playing = 1;
        levelTime = 0;
    }

    public GameObject tree;

    private static GameObject _t;

    public static GameObject Tree
    {
        get
        {
            if (_t == null)
            {
                _t = Instance.tree;
            }
            return (_t);
        }
    }

    private void FixedUpdate()
    {
        if (Playing)
        {
            levelTime += Time.fixedDeltaTime;

            if (Waypoint.Waypoints.Count == 0)
            {
                GameOver();
            }
        }
        else
        {
            Player.Freeze();
        }
    }

    private void OnGUI()
    {
        if (!Playing)
        {
            GUI.color = new Color(1, 1, 1, 1 - levelTime);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), AnimatedLevelPreview);
            GUI.color = Color.white;

            GUI.DrawTexture(new Rect(0, 0, Screen.width * LoadingProgress, 30), LoadingBar);
        }
        else if (playing == 1)
        {
            if (levelTime <= 1)
            {
                GUI.color = new Color(1, 1, 1, 1 - levelTime);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), AnimatedLevelPreview);
                GUI.color = Color.white;
            }
            GUI.Label(new Rect(0, 0, 100, 100), LevelTime.ToString());
        }
        else if (playing == 2)
        {
            GUI.color = new Color(1, 1, 1, levelTime);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), AnimatedLevelPreview);
            GUI.color = Color.white;
        }
    }
}
