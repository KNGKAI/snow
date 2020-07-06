using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    public Level level;
    public Character player;
    public Gun gun;
    public Gun missles;
    [Header("Camera")]
    public new Transform camera;
    public float distance;
    public float height;
    public float speed;
    [Header("Controll")]
    public KeyCode aim;
    public KeyCode fire;
    public KeyCode push;
    public KeyCode brake;
    public KeyCode left;
    public KeyCode right;
    public KeyCode jump;
    public KeyCode reset;
    [Header("Misc")]
    public Vector3 aimOffset;
    public Vector3 aimOffsetRotation;
    public float aimTimeScale;
    public float missleAccuracy;
    public float missleMaxDistance;

    Vector3 mousePosition;
    Vector3 cameraPosition;
    Vector3 resetPos;
    Quaternion resetRot;

    private bool aiming;
    private float x;
    private float y;

    public bool Aiming
    {
        get
        {
            return (aiming);
        }
    }

    private void Start()
    {
        SetupReset();

        Game.StartNewLevel();
    }

    private void Update()
    {
        if (Input.GetKeyDown(reset))
        {
            Reset();
        }

        if (Game.Playing)
        {
            if (Input.GetKeyDown(aim))
            {
                SetupAim();
            }
            aiming = Input.GetKey(aim);
            Time.timeScale = Aiming ? aimTimeScale : 1.0f;
            if (Aiming)
            {
                Aim();
            }
            else
            {
                Follow();
                Controll();
            }

            Vector3 direction;

            direction = cameraPosition - camera.position;
            if (direction.magnitude > 1)
            {
                direction.Normalize();
            }

            camera.Translate(direction * speed * Time.fixedDeltaTime);

            //camera.position = cameraPosition;
            camera.LookAt(player.transform.position + Vector3.up * player.height);
            //camera.position = camera.position + (player.Switch ? -camera.right : camera.right);

            if (Aiming)
            {
                camera.Translate(aimOffset);
                camera.Rotate(aimOffsetRotation);
                gun.Aim(camera.forward);
            }

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                mousePosition = Input.mousePosition;
                if (mousePosition.y > Screen.height / 2)
                {
                    player.Push();
                }
            }
            else if (Input.GetKey(KeyCode.Mouse0))
            {
                player.Rotate((Input.mousePosition.x - mousePosition.x) / Screen.width);
                if (Input.mousePosition.y - mousePosition.y > Screen.height * 0.75f)
                {
                    mousePosition.y = Input.mousePosition.y;
                    Reset();
                }
            }

            /*
            if (Input.GetKeyDown(fire))
            {
                if (Aiming)
                {
                    gun.Fire();
                }
                else
                {
                    if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, missleMaxDistance))
                    {
                        missles.Launch(hit.point, missleAccuracy);
                    }
                }
            }
            */

            //Chunk.Update(player.transform.position.x, player.transform.position.z, 3);
        }
    }

    private void SetupAim()
    {
        Vector3 temp;
        Quaternion rotation;

        temp = camera.position - player.transform.position;
        rotation = Quaternion.LookRotation(-temp);
        temp = rotation.eulerAngles;
        x = temp.x;
        y = temp.y;
    }

    private void Aim()
    {
        cameraPosition = Quaternion.Euler(x, y, 0.0f) * Vector3.back * Mathf.Sqrt(Mathf.Pow(distance, 2.0f) + Mathf.Pow(height, 2.0f));
        x += Input.GetAxis("Mouse Y") * speed;
        y += Input.GetAxis("Mouse X") * speed;
    }

    private void Controll()
    {
        if (Input.GetKeyDown(push))
        {
            player.Push();
        }
        if (Input.GetKeyDown(brake))
        {
            player.Brake();
        }
        if (Input.GetKey(left))
        {
            player.Rotate(false);
        }
        if (Input.GetKey(right))
        {
            player.Rotate(true);
        }
        if (Input.GetKeyDown(jump))
        {
            player.Olie();
        }
    }

    private void Follow()
    {
        cameraPosition = -player.Rig.velocity.normalized;
        cameraPosition.y = 0;
        cameraPosition *= distance;
        cameraPosition.y = height;
        cameraPosition += player.transform.position;
    }

    private void SetupReset()
    {
        resetPos = player.transform.position;
        resetRot = player.transform.rotation;
    }

    private void Reset()
    {
        Game.GameOver();
    }
}
