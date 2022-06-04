using UnityEngine;

public class CarController : MonoBehaviour
{
    private Rigidbody _body;

    [Header("Settings")]
    public WheelController[] wheels;
    public float maxSteering = 35;
    public Transform centerOfMass;

    [Header("Debugging")]
    [ReadOnly]
    public float _throttle;
    [ReadOnly]
    public float _brake;
    [ReadOnly]
    public float _handbrake;
    [ReadOnly]
    public float _steering;
    [ReadOnly]
    public float _downForce;
    [ReadOnly]
    public float _downForceKG;
    [ReadOnly]
    public float _downForceToCarMass;

    private void Start()
    {
        _body = GetComponent<Rigidbody>();
        _body.centerOfMass = centerOfMass.localPosition;

        foreach (WheelController wheel in wheels)
        {
            wheel.Setup(_body);
        }
    }

    private void LateUpdate()
    {
        _throttle = Input.GetAxis("Throttle");
        _steering = Input.GetAxis("Steering");
        // _brake = Input.GetAxis("Brake");
        // _handbrake = Input.GetAxis("Handbrake");
    }

    private void FixedUpdate()
    {
        // DownForce();

        for (int i = 0; i < 2; i++)
        {
            wheels[i].transform.localEulerAngles = new Vector3(0, _steering * maxSteering, 0);
        }

        foreach (WheelController wheel in wheels)
        {
            wheel.Step(_throttle);
        }
    }

    private void DownForce()
    {
        Vector3 frontAxis = (wheels[0].transform.position + wheels[1].transform.position) / 2;
        Vector3 rearAxis = (wheels[3].transform.position + wheels[2].transform.position) / 2;

        _downForce = 0;

        for (int i = 0; i < 2; i++)
        {
            Vector3 velocityVector;

            if (i == 0)
                velocityVector = _body.GetPointVelocity(frontAxis);
            else
                velocityVector = _body.GetPointVelocity(rearAxis);

            float velocity = Mathf.Max((Quaternion.Inverse(_body.transform.rotation) * velocityVector).z, 0);

            float downForce = 0.5f * 1.22f * Mathf.Sqrt(velocity) * 5.0f / 2;
            downForce *= 100; // N to Unity force
            _downForce += downForce;

            _body.AddForceAtPosition(-transform.up * downForce, velocityVector);
        }

        _downForceKG = _downForce / 9.8f;
        _downForceToCarMass = _downForceKG / _body.mass;
    }
}