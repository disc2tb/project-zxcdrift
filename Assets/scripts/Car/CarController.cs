using UnityEngine;

public class CarController : MonoBehaviour
{
    private Rigidbody _body;
    private EngineController _engine;
    private GearboxController _gearbox;
    private DifferentialController _differential;
    private ClutchController _clutch;

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

        _engine = GetComponent<EngineController>();
        _gearbox = GetComponent<GearboxController>();
        _differential = GetComponent<DifferentialController>();
        _clutch = GetComponent<ClutchController>();

        _clutch.Setup(_engine.GetMaxTorque());
    }

    private void LateUpdate()
    {
        _throttle = Input.GetAxis("Throttle");
        _steering = Input.GetAxis("Steering");
        // _brake = Input.GetAxis("Brake");
        // _handbrake = Input.GetAxis("Handbrake");

        if (Input.GetKeyDown(KeyCode.P))
            StartCoroutine(_gearbox.ShiftGearUp());
        else if (Input.GetKeyDown(KeyCode.L))
            StartCoroutine(_gearbox.ShiftGearDown());
    }

    private void FixedUpdate()
    {
        float[] torque = _differential.GetOutputTorque(_gearbox.GetOutputTorque(_clutch.torque));
        Debug.Log(torque[0] + " " + torque[1]);

        for (int i = 0; i < 2; i++)
        {
            wheels[i].transform.localEulerAngles = new Vector3(0, _steering * maxSteering, 0);
            wheels[i].Step(0);
        }

        wheels[2].Step(torque[0]);
        wheels[3].Step(torque[1]);

        float inputShaftVelocity = _gearbox.GetInputShaftVelocity(
            _differential.GetInputShaftVelocity(wheels[2].angularVelocity, wheels[3].angularVelocity)
        );

        _clutch.Step(inputShaftVelocity, _engine.angularVelocity, _gearbox.ratio);
        _engine.Step(_throttle, _clutch.torque);
    }
}