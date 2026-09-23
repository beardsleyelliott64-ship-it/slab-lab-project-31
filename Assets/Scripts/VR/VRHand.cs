using UnityEngine;
using UnityEngine.XR;

public class VRHand : MonoBehaviour
{
    [SerializeField] private float velocitySmoothing = 18f;
    [SerializeField] private float collisionRadius = 0.11f;

    private XRNode node;
    private SlapLabRuntime runtime;
    private InputDevice device;

    private Vector3 previousPosition;
    private Vector3 velocity;
    private bool initialized;

    public InputDevice Device => device;
    public Vector3 Velocity => velocity;
    public float Speed => velocity.magnitude;

    public void Initialize(XRNode handNode, SlapLabRuntime owner)
    {
        node = handNode;
        runtime = owner;
        device = InputDevices.GetDeviceAtXRNode(node);

        if (!device.isValid)
            device = InputDevices.GetDeviceAtXRNode(node);

        previousPosition = transform.position;
        initialized = true;

        Collider existing = GetComponent<Collider>();
        if (existing != null)
        {
            existing.isTrigger = true;
        }
    }

    private void Awake()
    {
        if (!initialized)
            previousPosition = transform.position;
    }

    private void Update()
    {
        if (!device.isValid)
            device = InputDevices.GetDeviceAtXRNode(node);

        if (device.isValid)
        {
            if (device.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 position))
                transform.position = position;

            if (device.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rotation))
                transform.rotation = rotation;
        }

        if (!initialized)
        {
            previousPosition = transform.position;
            initialized = true;
            return;
        }

        float dt = Mathf.Max(Time.deltaTime, 0.0001f);
        Vector3 rawVelocity = (transform.position - previousPosition) / dt;
        float blend = 1f - Mathf.Exp(-velocitySmoothing * dt);
        velocity = Vector3.Lerp(velocity, rawVelocity, blend);
        previousPosition = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        RegisterHit(other, transform.position);
    }

    private void OnCollisionEnter(Collision collision)
    {
        RegisterHit(collision.collider, collision.GetContact(0).point);
    }

    private void RegisterHit(Collider other, Vector3 hitPoint)
    {
        SlapTarget target = other.GetComponentInParent<SlapTarget>();
        if (target == null)
            return;

        if (Speed < 1.25f)
            return;

        Vector3 normal = (hitPoint - target.transform.position).normalized;
        if (normal.sqrMagnitude < 0.001f)
            normal = -transform.forward;

        target.RegisterHit(Speed, hitPoint, normal);
    }

    private void OnValidate()
    {
        collisionRadius = Mathf.Max(0.03f, collisionRadius);
    }
}
