using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BulletType
{
    DefaultBullet,
    SinoidBullet,
    SinBullet
};

public class Bullet : MonoBehaviour
{
    public float damage;
    public float speed;
    public BulletType type;

    private Vector3 velocity;
    private Vector3 startPoint;
    private SphereCollider col;
    
    void Start()
    {
        startPoint = this.transform.position;
        col = this.GetComponent<SphereCollider>();
    }

    void FixedUpdate()
    {
        BulletMovement();
        BulletCollisions();
    }

    void SetBulletVelocity()
    {
        switch (type)
        {
            case BulletType.DefaultBullet:
                velocity = speed * Vector3.right;
                break;
            case BulletType.SinoidBullet:
                velocity = speed * Vector3.right 
                    + Mathf.Sin(Vector3.Distance(startPoint, this.transform.position) * 3f) * Vector3.up * 20f
                    + Mathf.Cos(Vector3.Distance(startPoint, this.transform.position) * 3f) * Vector3.forward * 20f;
                break;
            case BulletType.SinBullet:
                velocity = speed * Vector3.right
                    + (Mathf.Sin(Mathf.Abs(startPoint.x - this.transform.position.x) * 1f)) * 12f * Vector3.up
                    + 1f * Vector3.right;
                break;
            default:
                break;
        }
    }

    void BulletMovement()
    {
        SetBulletVelocity();

        this.transform.Translate(velocity * Time.fixedDeltaTime);
    }

    void BulletCollisions()
    {
        Collider[] hits = Physics.OverlapSphere(this.transform.position, col.radius);

        foreach (Collider c in hits)
        {
            if (c == col)
            {
                continue;
            }

            if (!c.CompareTag("Player") && 
                !c.CompareTag("EnemyBullet") && 
                !c.CompareTag("PlayerBullet") && 
                !c.CompareTag("Enemy"))
            {
                Destroy(this.gameObject);
            }
        }
    }
}
