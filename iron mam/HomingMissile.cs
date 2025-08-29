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
