﻿using UnityEngine;

public class CarController : MonoBehaviour
{
    private Rigidbody _body;
    private EngineController _engine;
    private GearboxController _gearbox;
    private DifferentialController _differential;

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
        _engine.Step(_throttle);

        for (int i = 0; i < 2; i++)
        {
            wheels[i].transform.localEulerAngles = new Vector3(0, _steering * maxSteering, 0);
            wheels[i].Step(0);
        }

        for (int i = 2; i < 4; i++)
            wheels[i].Step(_throttle * /*100*/0);
    }
}