I've split your Iron Man VR code into 5 separate .cs files ready to upload to GitHub:

---

**1. VRIronManController.cs**
```csharp
using UnityEngine;

public class VRIronManController : MonoBehaviour
{
    public Transform headTransform;
    public float thrustForce = 20f;
    public float maxSpeed = 25f;
    public float turnSpeed = 3f;
    public float liftDamping = 2f;
    public bool useGyroForPitchRoll = true;

    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
    }

    void FixedUpdate()
    {
        if (headTransform == null) return;

        bool thrust = Input.GetButton("Fire1") || Input.GetMouseButton(0);
        if (thrust)
            rb.AddForce(transform.forward * thrustForce, ForceMode.Acceleration);

        if (rb.velocity.magnitude > maxSpeed)
            rb.velocity = rb.velocity.normalized * maxSpeed;

        Vector3 headForward = headTransform.forward;
        headForward.y = 0;
        if (headForward.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(headForward, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.fixedDeltaTime * turnSpeed);
        }

        if (useGyroForPitchRoll)
        {
            float headPitch = headTransform.eulerAngles.x;
            if (headPitch > 180) headPitch -= 360f;
            float pitchFactor = Mathf.Clamp(headPitch / 40f, -1f, 1f);
            rb.AddForce(Vector3.up * pitchFactor * thrustForce / liftDamping, ForceMode.Acceleration);
        }

        rb.velocity = new Vector3(rb.velocity.x, Mathf.Lerp(rb.velocity.y, 0, Time.fixedDeltaTime * 0.5f), rb.velocity.z);
    }
}
```

---

**2. LaserWeapon.cs**
```csharp
using UnityEngine;

public class LaserWeapon : MonoBehaviour
{
    public Transform headTransform;
    public float range = 200f;
    public float fireRate = 0.15f;
    public float damage = 25f;
    public GameObject laserBeamPrefab;
    public AudioClip fireSfx;
    public LayerMask hitLayers = ~0;

    float lastFireTime = -999f;
    AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        if ((Input.GetButton("Fire1") || Input.GetMouseButton(0)) && Time.time - lastFireTime >= fireRate)
        {
            Fire();
            lastFireTime = Time.time;
        }
    }

    void Fire()
    {
        Vector3 origin = headTransform.position;
        Vector3 dir = headTransform.forward;

        RaycastHit hit;
        Vector3 end = origin + dir * range;
        if (Physics.Raycast(origin, dir, out hit, range, hitLayers))
        {
            end = hit.point;
            var enemy = hit.collider.GetComponent<EnemySimple>();
            if (enemy != null) enemy.TakeDamage(damage);
        }

        if (laserBeamPrefab != null)
        {
            GameObject beam = GameObject.Instantiate(laserBeamPrefab, origin, Quaternion.identity);
            var lr = beam.GetComponent<LineRenderer>();
            if (lr != null)
            {
                lr.SetPosition(0, origin);
                lr.SetPosition(1, end);
            }
            GameObject.Destroy(beam, 0.07f);
        }
        else
        {
            Debug.DrawLine(origin, end, Color.red, 0.07f);
        }

        if (fireSfx != null) audioSource.PlayOneShot(fireSfx);
    }
}
```

---

**3. MissileLauncher.cs**
```csharp
using UnityEngine;

public class MissileLauncher : MonoBehaviour
{
    public Transform headTransform;
    public GameObject missilePrefab;
    public Transform missileSpawnPoint;
    public float missileCooldown = 0.8f;
    float lastLaunch = -999f;

    void Update()
    {
        if ((Input.GetButtonDown("Fire2") || Input.GetMouseButtonDown(1)) && Time.time - lastLaunch >= missileCooldown)
        {
            LaunchMissile();
            lastLaunch = Time.time;
        }
    }

    void LaunchMissile()
    {
        Vector3 spawnPos = missileSpawnPoint != null ? missileSpawnPoint.position : headTransform.position + headTransform.forward * 0.6f;
        Quaternion spawnRot = Quaternion.LookRotation(headTransform.forward, Vector3.up);
        GameObject missile = GameObject.Instantiate(missilePrefab, spawnPos, spawnRot);
        Rigidbody mr = missile.GetComponent<Rigidbody>();
        if (mr != null) mr.velocity = headTransform.forward * 6f;
    }
}
```

---

**4. HomingMissile.cs**
```csharp
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HomingMissile : MonoBehaviour
{
    public float speed = 10f;
    public float rotateSpeed = 120f;
    public float lifeTime = 8f;
    public float explosionRadius = 2f;
    public float explosionDamage = 60f;
    public LayerMask damageMask;
    public GameObject explosionPrefab;

    Rigidbody rb;
    Transform target;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        Destroy(gameObject, lifeTime);
        AcquireTarget();
    }

    void AcquireTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float bestDist = Mathf.Infinity;
        Transform bestT = null;
        foreach (var e in enemies)
        {
            float d = Vector3.Distance(transform.position, e.transform.position);
            if (d < bestDist) { bestDist = d; bestT = e.transform; }
        }
        target = bestT;
    }

    void FixedUpdate()
    {
        if (target == null)
        {
            if (Random.value < 0.02f) AcquireTarget();
            rb.velocity = transform.forward * speed;
            return;
        }

        Vector3 dir = (target.position - transform.position).normalized;
        Quaternion lookRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRot, rotateSpeed * Time.fixedDeltaTime);
        rb.velocity = transform.forward * speed;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") || other.gameObject.layer == LayerMask.NameToLayer("Default"))
            Explode();
    }

    void Explode()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius, damageMask);
        foreach (var h in hits)
        {
            var enemy = h.GetComponent<EnemySimple>();
            if (enemy != null) enemy.TakeDamage(explosionDamage);
        }
        if (explosionPrefab != null) Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
```

---

**5. EnemySimple.cs**
```csharp
using UnityEngine;

public class EnemySimple : MonoBehaviour
{
    public float maxHealth = 100f;
    float hp;

    void Start()
    {
        hp = maxHealth;
    }

    public void TakeDamage(float d)
    {
        hp -= d;
        if (hp <= 0) Die();
    }

    void Die()
    {
        Destroy(gameObject);
    }
}
```

---

These 5 files are now ready to be **uploaded to GitHub** and will work properly in Unity when attached to the correct GameObjects.
