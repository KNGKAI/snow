using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missle : MonoBehaviour
{
    public GameObject explosion;
    public AnimationCurve accuracyCurve;

    private Vector3 position;
    private float speed;
    private float accuracy;
    private bool fire;

    private float t;

    private void FixedUpdate()
    {
        if (fire)
        {
            Vector3 direction = transform.position - position;
            transform.rotation = Quaternion.LookRotation(Vector3.Slerp(transform.forward, -direction.normalized, accuracy), transform.up);
            transform.Translate(transform.forward * speed);

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
            else
            {
                t = 0;
            }
        }
    }

    public void Luanch(Vector3 target, float power, float accurate = 0.1f)
    {
        speed = power * Time.fixedDeltaTime;
        position = target;
        fire = true;
        accuracy = accurate;
        t = 1.0f;
    }
}
