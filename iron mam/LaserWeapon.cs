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
