using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotController : MonoBehaviour
{
    [Header("Movement")]
    [Range(5f, 20f)]
    public float movementSpeed = 10f;
    [Range(0.1f, 1f)]
    public float acceleratingTime = 0.3f;
    [Range(0.1f, 1f)]
    public float acceleratingOnAirTime = 0.6f;
    [Range(0.01f, 1f)]
    public float deceleratingTime = 0.1f;
    [Range(0.1f, 1f)]
    public float deceleratingOnAirTime = 0.3f;

    [Header("Jumping")]
    [Range(5f, 50f)]
    public float jumpPower = 15f;
    [Range(0.01f, 0.5f)]
    public float jumpCooldown = 0.2f;
    [Range(0.8f, 0.9999f)]
    public float jumpDeceleration = 0.95f;
    [Range(0.1f, 0.5f)]
    public float jumpPressDuration = 0.3f;
    [Range(0.01f, 0.2f)]
    public float jumpExtendDuration = 0.05f;

    [Header("Gravity")]
    [Range(1f, 10f)]
    public float gravityMin = 5f;
    [Range(1f, 30f)]
    public float gravityMax = 30f;
    [Range(1.001f, 2f)]
    public float gravityMultiplier = 1.05f;

    private Vector3 groundSnapPoint;
    private Vector3 velocity;
    private Vector3 jumpVelocity;
    private Collider col;
    private Animator anim;
    private RobotStats stats;
    private Gun gun;
    private float gravityForce;
    private float jumpDuration;
    private float lastJumpState;
    private float stopJumpDuration;
    private float currentJumpPower;
    private float lastXAxis;
    private float xAxis;
    private float lastDir;
    private float jump;
    private float velMultiplier;
    private float currentSpeed;
    private float timeSinceGrounded;
    private float lookingRot;
    private bool shoot;
    private bool isGrounded;
    private bool isAscending;
    private bool snapped;
    
    void Start()
    {
        anim = this.transform.GetChild(0).GetChild(1).GetComponent<Animator>();
        stats = this.GetComponent<RobotStats>();
        lookingRot = 1f;
        gun = this.transform.GetComponentInChildren<Gun>();
        col = this.GetComponent<Collider>();
        velMultiplier = 0f;
        gravityForce = gravityMin;
        currentSpeed = 0f;
        xAxis = 0f;
        lastDir = 0f;
        jumpDuration = 0f;
        currentJumpPower = jumpPower;
        stopJumpDuration = 0f;
        timeSinceGrounded = 0f;
    }

    void Update()
    {
        ParseInput();
    }

    void FixedUpdate()
    {
        DetectGrounded();
        HandleMovement();
        Jumping();
        ApplyGravity();
        HandleCollisions();
    }

    void Jumping()
    {
        bool jmpAnim = anim.GetBool("jump");
        if (jmpAnim)
        {
            if(isGrounded)
            {
                anim.SetBool("jump", false);
            }
        }
        else if (isAscending)
        {
            anim.SetBool("jump", true);
        }

        isAscending = false;
        
        if (stopJumpDuration == 0f || stopJumpDuration > jumpExtendDuration)
        {
            stopJumpDuration = 0f;

            if (jump == 0f)
            {
                jumpDuration = 0f;
                currentJumpPower = jumpPower;
                return;
            }
            if (jumpDuration > jumpPressDuration)
            {
                return;
            }
            if (!isGrounded && jumpDuration == 0f)
            {
                return;
            }
        }

        isAscending = true;
        jumpVelocity = currentJumpPower * Time.fixedDeltaTime * Vector3.up;
        currentJumpPower *= jumpDeceleration;
        jumpDuration += Time.fixedDeltaTime;
        stopJumpDuration += Time.fixedDeltaTime;

        this.transform.Translate(jumpVelocity);
    }

    void HandleMovement()
    {
        float timeToReach;

        if(xAxis != 0f)
        {
            anim.SetBool("moving", true);
            timeToReach = isGrounded ? acceleratingTime : acceleratingOnAirTime;
            currentSpeed = Mathf.SmoothDamp(currentSpeed, movementSpeed, ref velMultiplier, timeToReach);
            velocity = new Vector3(xAxis * currentSpeed * Time.fixedDeltaTime, 0f, 0f);
            lastDir = xAxis;
        }
        else
        {
            anim.SetBool("moving", false);
            timeToReach = isGrounded ? deceleratingTime : deceleratingOnAirTime;
            currentSpeed = Mathf.SmoothDamp(currentSpeed, 0f, ref velMultiplier, timeToReach);
            velocity = new Vector3(lastDir * currentSpeed * Time.fixedDeltaTime, 0f, 0f);
        }
        
        this.transform.Translate(velocity, Space.World);
    }

    void ParseInput()
    {
        lastXAxis = xAxis;
        lastJumpState = jump;
        xAxis = Input.GetAxisRaw("Horizontal");
        if(xAxis > 0.6f)
        {
            xAxis = 1f;
        }
        else if(xAxis < -0.6f)
        {
            xAxis = -1f;
        }
        else
        {
            xAxis = 0f;
        }

        if (lookingRot * xAxis == -1f)
        {
            StopAllCoroutines();
            StartCoroutine(ChangeDirection(lookingRot));
        }

        jump = Input.GetAxisRaw("Jump");
        shoot = Input.GetAxisRaw("Fire1") != 0f;
        gun.SetIsFiring(shoot);

        if (jump == 0 && jumpDuration > jumpPressDuration)
        {
            jumpDuration = 0f;
        }

        if(timeSinceGrounded < jumpCooldown && !isAscending)
        {
            jumpDuration = 0.5f;
        }

        if(lastJumpState == 0f && isAscending)
        {
            jumpDuration = 0.5f;
        }

        if(lastXAxis != xAxis)
        {
            if(isGrounded)
            {
                currentSpeed *= 0.2f;
            }
            else
            {
                currentSpeed *= 0.5f;
            }
        }

        if (jump == 0f && lastJumpState > 0f && isAscending)
        {
            stopJumpDuration = 0.0001f;
        }
    }
    
    IEnumerator ChangeDirection(float lookRotation)
    {
        float targetRot = lookRotation == -1f ? 0f : 180f;
        float rotVel = 0f;
        Transform meshTransform = this.transform.GetChild(0);

        while (Mathf.Abs(meshTransform.eulerAngles.y - targetRot) > 5f)
        {
            float newRotY = Mathf.SmoothDampAngle(meshTransform.eulerAngles.y, targetRot, ref rotVel, 0.03f);
            meshTransform.eulerAngles = new Vector3(meshTransform.eulerAngles.x, newRotY, meshTransform.eulerAngles.z);
            yield return null;
        }

        meshTransform.eulerAngles = new Vector3(meshTransform.eulerAngles.x, targetRot, meshTransform.eulerAngles.z);
        lookingRot = -lookRotation;
    }

    void ApplyGravity()
    {
        if (isAscending)
        {
            return;
        }
        if(isGrounded)
        {
            if (!snapped)
            {
                this.transform.position = new Vector3(this.transform.position.x,
                                                    groundSnapPoint.y + col.bounds.extents.y + 0.01f,
                                                    this.transform.position.z);
                snapped = true;
            }
            
            return;
        }

        this.transform.Translate(Vector3.down * gravityForce * Time.fixedDeltaTime);
        gravityForce *= gravityMultiplier;
        gravityForce = Mathf.Clamp(gravityForce, gravityMin, gravityMax);
    }

    void HandleCollisions()
    {
        Collider[] hits = Physics.OverlapBox(col.bounds.center, col.bounds.extents);
        Vector3 dir;
        float dist;

        foreach(Collider c in hits)
        {
            if(c == col)
            {
                continue;
            }

            if(c.CompareTag("PlayerBullet"))
            {
                continue;
            }

            if(c.CompareTag("EnemyBullet"))
            {
                stats.TakeDamage(c.GetComponent<Bullet>().damage);
                Destroy(c.gameObject);
                continue;
            }

            if(c.CompareTag("PowerupH"))
            {
                stats.TakeDamage(-(0.3f * stats.maximumHealth));
                stats.PlayPowerUp();
                Destroy(c.gameObject);
                continue;
            }

            if(c.CompareTag("PowerupF"))
            {
                stats.AddFuel(0.3f * stats.maximumFuel);
                stats.PlayPowerUp();
                Destroy(c.gameObject);
                continue;
            }

            bool overlap = Physics.ComputePenetration(col, this.transform.position, this.transform.rotation,
                                    c, c.transform.position, c.transform.rotation,
                                    out dir, out dist);
            if(overlap)
            {
                if(dir == Vector3.down)
                {
                    jumpDuration = 0.5f;
                }
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
                gravityForce = 1f;
                groundSnapPoint = hit.point;
                if (timeSinceGrounded == 0f)
                {
                    currentSpeed *= 0.6f;
                }
                timeSinceGrounded += Time.fixedDeltaTime;

                return;
            }
        }

        timeSinceGrounded = 0f;
        snapped = false;
    }
}
