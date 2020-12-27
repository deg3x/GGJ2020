using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum enemyType
{
    minionDefault,
    minionSinoid,
    minionFollowing,
    tankCharge
};

public class Enemy : MonoBehaviour
{
    public float health;
    public float movementSpeed;

    public enemyType type;
    
    public Image healthUI;
    public GameObject playerTarget;
    public GameManager gm;
    public AudioClip fire;

    private Vector3 groundSnapPoint;
    private AudioSource audio;
    private Collider col;
    private Gun gun;
    private float maximumHealth;
    private float lookDirection;
    private float currentLookDirection;
    private float moveDirection;
    private float currentSpeed;
    private float vel;
    private float tankFireRate;
    private float tankTimeSinceSpecial;
    private float tankSpecialDuration;
    private bool isGrounded;
    private bool snapped;

    void Start()
    {
        gun = this.transform.GetComponentInChildren<Gun>();

        tankTimeSinceSpecial = -2f;
        tankSpecialDuration = 5f;
        tankFireRate = gun.fireRate;

        audio = this.GetComponent<AudioSource>();

        moveDirection = -1;
        currentSpeed = 0f;
        lookDirection = -1;
        currentLookDirection = lookDirection;
        maximumHealth = health;
        col = this.GetComponent<Collider>();

        gun.SetTimeSinceFire(-1f);
        if(type != enemyType.tankCharge)
        {
            gun.SetIsFiring(true);
        }
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        UpdateHealthUI();
    }

    public void PlayFire()
    {
        if (audio.isPlaying)
        {
            audio.Stop();
        }
        audio.clip = fire;
        audio.Play();
    }

    void UpdateHealthUI()
    {
        healthUI.fillAmount = health / maximumHealth;
    }

    void Update()
    {
        Rotation();
        CheckForDeath();

        if(type == enemyType.tankCharge)
        {
            tankTimeSinceSpecial += Time.deltaTime;
            if (tankTimeSinceSpecial > tankFireRate)
            {
                StartCoroutine(TankSpecialMove());
                tankTimeSinceSpecial = -tankSpecialDuration;
            }
        }
    }

    void FixedUpdate()
    {
        Movement();
        HandleCollisions();
    }

    void CheckForDeath()
    {
        if (health <= 0f)
        {
            gm.SpawnNextWave();
            Destroy(this.gameObject);
        }
    }

    void Movement()
    {
        GetMoveDirection();
        Vector3 velocity;

        currentSpeed = Mathf.SmoothDamp(currentSpeed, movementSpeed, ref vel, 0.5f);
        velocity = moveDirection * Vector3.right * Time.fixedDeltaTime * currentSpeed;
        this.transform.Translate(velocity);
    }

    IEnumerator TankSpecialMove()
    {
        float timeElapsed = 0f;

        float targetRot = (this.transform.eulerAngles.y + 180f) % 360f;
        float rotVel = 0f;
        Transform meshTransform = this.transform.GetChild(0);

        yield return null;

        while (timeElapsed < tankSpecialDuration)
        {
            float newRotY = Mathf.SmoothDampAngle(meshTransform.eulerAngles.y, targetRot, ref rotVel, 0.1f);
            meshTransform.eulerAngles = new Vector3(meshTransform.eulerAngles.x, newRotY, meshTransform.eulerAngles.z);
            targetRot = (newRotY + 180f) % 360f;
            timeElapsed += Time.deltaTime;

            if (timeElapsed > 0.5f)
            {
                gun.fireRate = 0.05f;
                gun.SetIsFiring(true);
            }

            yield return null;
        }

        gun.SetIsFiring(false);
        gun.fireRate = tankFireRate;
        targetRot = currentLookDirection == -1 ? 0f : 180f;
        meshTransform.eulerAngles = new Vector3(meshTransform.eulerAngles.x, targetRot, meshTransform.eulerAngles.z);
    }

    void Rotation()
    {
        GetLookDirection();
        if (lookDirection == -currentLookDirection)
        {
            StartCoroutine(ChangeDirection(lookDirection));
        }
    }

    IEnumerator ChangeDirection(float lookRotation)
    {
        float targetRot = lookRotation == -1f ? 0f : 180f;
        float rotVel = 0f;
        Transform meshTransform = this.transform.GetChild(0);

        while (Mathf.Abs(meshTransform.eulerAngles.y - targetRot) > 5f)
        {
            float newRotY = Mathf.SmoothDampAngle(meshTransform.eulerAngles.y, targetRot, ref rotVel, 0.5f);
            meshTransform.eulerAngles = new Vector3(meshTransform.eulerAngles.x, newRotY, meshTransform.eulerAngles.z);
            yield return null;
        }

        meshTransform.eulerAngles = new Vector3(meshTransform.eulerAngles.x, targetRot, meshTransform.eulerAngles.z);
        currentLookDirection = lookRotation;
    }

    void GetLookDirection()
    {
        lookDirection = this.transform.position.x - playerTarget.transform.position.x > 0f ? -1f : 1f;
    }

    void GetMoveDirection()
    {
        float distX = Mathf.Abs(this.transform.position.x - playerTarget.transform.position.x);
        if(distX < 3f && moveDirection != -currentLookDirection)
        {
            moveDirection = -currentLookDirection;
            currentSpeed *= 0.3f;
        }
        else if (distX > 8f && moveDirection != currentLookDirection)
        {
            moveDirection = currentLookDirection;
            currentSpeed *= 0.3f;
        }
    }

    void HandleCollisions()
    {
        Collider[] hits = Physics.OverlapBox(col.bounds.center, col.bounds.extents);
        Vector3 dir;
        float dist;

        foreach (Collider c in hits)
        {
            if (c == col)
            {
                continue;
            }

            if (c.CompareTag("EnemyBullet") || c.CompareTag("Player"))
            {
                continue;
            }

            if (c.CompareTag("PlayerBullet"))
            {
                TakeDamage(c.GetComponent<Bullet>().damage);
                Destroy(c.gameObject);
                continue;
            }

            bool overlap = Physics.ComputePenetration(col, this.transform.position, this.transform.rotation,
                                    c, c.transform.position, c.transform.rotation,
                                    out dir, out dist);
            if (overlap)
            {
                this.transform.Translate(dir * dist);
            }
        }
    }

    void DetectGrounded()
    {
        Vector3 rayStart = col.bounds.center - new Vector3(0f, col.bounds.extents.y - 0.1f, 0f);
        Vector3 rayDir = Vector3.down * 0.1f;
        Ray r = new Ray(rayStart, rayDir);
        RaycastHit hit;
        int layerMask = ~(1 << 8);

        isGrounded = false;

        if (Physics.Raycast(r, out hit, 0.3f, layerMask))
        {
            if (hit.transform.CompareTag("Ground"))
            {
                isGrounded = true;
                groundSnapPoint = hit.point;

                return;
            }
        }

        snapped = false;
    }
}
