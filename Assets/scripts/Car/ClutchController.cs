using System;
using UnityEngine;

public class ClutchController : MonoBehaviour
{
    public float clutchStiffness = 40;
    public float clutchCapacity = 1.3f;
    [Range(0, 0.9f)]
    public float clutchDamping = 0.7f;

    public float torque;
    [ReadOnly]
    public float _engineMaxTorque;
    [ReadOnly]
    public float _clutchMaxTorque;
    [ReadOnly]
    public float _angularVelocity;

    public void Setup(float engineMaxTorque)
    {
        _engineMaxTorque = engineMaxTorque;
        _clutchMaxTorque = _engineMaxTorque * clutchCapacity;
    }

    public void Step(float outputShaftVelocity, float engineAngularVelocity, float gearboxRatio)
    {
        // Clutch torque
        float clutchSlip = (engineAngularVelocity - _angularVelocity) * Mathf.Sign(Mathf.Abs(gearboxRatio));

        // autoclutch
        float clutchLock = Mathf.Min(
            Helpers.Map(1000, 1300, 0, 1, engineAngularVelocity * Helpers.Rad2RPM)
            + Convert.ToInt32(gearboxRatio == 0), 1
        );

        torque = (Mathf.Clamp(
            clutchSlip * clutchStiffness * clutchLock,
            -_clutchMaxTorque, _clutchMaxTorque
        ) - torque) * clutchDamping;
    }
}