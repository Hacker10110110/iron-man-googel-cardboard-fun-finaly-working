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
