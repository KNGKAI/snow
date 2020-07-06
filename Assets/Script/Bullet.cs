using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public GameObject explosion;
    private float speed;
    private bool fire;
    private float t;

    private void FixedUpdate()
    {
        if (fire)
        {
            transform.Translate(Vector3.forward * speed, Space.Self);

            if (t < 0.9f)
            {
                if (Physics.CheckSphere(transform.position, 0.1f))
                {
                    GameObject explos = GameObject.Instantiate<GameObject>(explosion);
                    explos.transform.position = transform.position;
                    Destroy(explos, 5);
                    DestroyImmediate(gameObject);
                }
            }
            if (t > 0)
            {
                t -= Time.fixedDeltaTime;
            }
        }
    }

    public void Fire(float power)
    {
        speed = power * Time.fixedDeltaTime;
        fire = true;
    }

    public void Luanch(Vector3 target, float power, float accurate = 0.1f)
    {
        speed = power * Time.fixedDeltaTime;
        fire = true;
        t = 1.0f;
    }
}
