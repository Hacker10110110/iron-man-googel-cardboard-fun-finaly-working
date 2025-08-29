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
