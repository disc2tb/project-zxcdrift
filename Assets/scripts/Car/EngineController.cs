using UnityEngine;

public class EngineController : MonoBehaviour
{
    public AnimationCurve torqueCurve;
    public float startFriction = 50;
    public float frictionCoefficient = 0.02f;
    public float inertia = 0.2f;

    public float idleRPM = 900;
    public float maxRPM = 7500;

    [ReadOnly]
    public float _RPM;
    [ReadOnly]
    public float _angularVelocity = 100;
    [ReadOnly]
    public float _effectiveTorque;

    public void Step(float throttle)
    {
        Acceleration(throttle);
    }

    private void Acceleration(float throttle)
    {
        float frictionTorque = startFriction + _RPM * frictionCoefficient;

        float maxInitialTorque = torqueCurve.Evaluate(_RPM) + frictionTorque;
        float initialTorque = maxInitialTorque * throttle;

        _effectiveTorque = initialTorque - frictionTorque;

        float angularAcceleration = _effectiveTorque / inertia;

        _angularVelocity = Mathf.Clamp(
            _angularVelocity + angularAcceleration * Time.fixedDeltaTime,
            idleRPM * Helpers.RPM2Rad,
            maxRPM * Helpers.RPM2Rad
        );

        _RPM = _angularVelocity * Helpers.Rad2RPM;
    }
}