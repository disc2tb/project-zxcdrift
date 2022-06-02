using UnityEngine;

public class CarController : MonoBehaviour
{
    private Rigidbody _body;
    public WheelController[] wheels;

    private float _throttle;
    private float _brake;
    private float _handbrake;

    private float _steering;
    public float maxSteering = 35;

    private void Start()
    {
        _body = GetComponent<Rigidbody>();

        foreach (WheelController wheel in wheels)
        {
            wheel.Setup(_body);
        }
    }

    private void LateUpdate()
    {
        _throttle = Input.GetAxis("Throttle");
        // _steering = Input.GetAxis("Steering");
        // _brake = Input.GetAxis("Brake");
        // _handbrake = Input.GetAxis("Handbrake");
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < 2; i++)
        {
            wheels[i].transform.localEulerAngles = new Vector3(0, _steering * maxSteering, 0);
        }

        foreach (WheelController wheel in wheels)
        {
            wheel.Step(_throttle, _brake, _handbrake);
        }
    }
}