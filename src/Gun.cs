using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public float fireRate;
    public GameObject bullet;

    private bool isFiring;
    private float timeSinceFire;

    void Start()
    {
        timeSinceFire = fireRate;
    }

    void Update()
    {
        Fire();
    }

    void Fire()
    {
        timeSinceFire += Time.deltaTime;

        if (!isFiring)
        {
            return;
        }

        if (timeSinceFire < fireRate)
        {
            return;
        }

        GameObject g = Instantiate(bullet, this.transform.position, this.transform.parent.rotation);
        g.tag = this.transform.parent.parent.CompareTag("Player") ? "PlayerBullet" : "EnemyBullet";
        if(g.CompareTag("EnemyBullet"))
        {
            this.transform.parent.parent.GetComponent<Enemy>().PlayFire();
            g.transform.Rotate(0f, 180f, 0f);
        }
        else
        {
            this.transform.parent.parent.GetComponent<RobotStats>().PlayFire();
        }
        timeSinceFire = 0f;
    }

    public void SetIsFiring(bool firing)
    {
        isFiring = firing;
    }

    public void SetTimeSinceFire(float val)
    {
        timeSinceFire = val;
    }
}
