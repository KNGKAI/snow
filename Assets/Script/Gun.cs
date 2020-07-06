using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public float power;
    public float rate;
    public GameObject bullet;

    private float t;
    private int check;

    private void Update()
    {
        if (t > 0)
        {
            t -= Time.deltaTime;
        }
        if (check > 0)
        {
            check--;
        }
        else
        {
            transform.localRotation = Quaternion.identity;
        }
    }

    public void Aim(Vector3 direction)
    {
        transform.rotation = Quaternion.LookRotation(direction);
        check = 3;
    }

    public void Fire()
    {
        GameObject b;

        if (t > 0) { return; }

        t = rate;

        b = GameObject.Instantiate<GameObject>(bullet);
        b.transform.position = transform.position;
        b.transform.forward = transform.forward;
        b.GetComponent<Bullet>().Fire(power);
        Destroy(b, 10.0f);
    }

    public void Launch(Vector3 target, float accurate = 0.1f)
    {
        GameObject b;

        if (t > 0) { return; }

        t = rate;

        b = GameObject.Instantiate<GameObject>(bullet);
        b.transform.position = transform.position;
        b.transform.forward = transform.up;
        b.GetComponent<Missle>().Luanch(target, power, accurate);
    }
}