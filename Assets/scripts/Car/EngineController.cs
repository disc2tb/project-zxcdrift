using System.Diagnostics;
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
    public float angularVelocity = 100;

    [ReadOnly]
    public float _RPM;
    [ReadOnly]
    public float _effectiveTorque;

    public float GetMaxTorque()
    {
        // Works only if highest point of torqueCurve is a key

        float maxTorque = 0;

        for (int i = 0; i < torqueCurve.length; i++)
        {
            if (torqueCurve.keys[i].value > maxTorque)
                maxTorque = torqueCurve.keys[i].value;
        }

        return maxTorque;
    }

    public void Step(float throttle, float loadTorque)
    {
        Acceleration(throttle, loadTorque);
    }

    private void Acceleration(float throttle, float loadTorque)
    {
        float frictionTorque = startFriction + _RPM * frictionCoefficient;

        float maxInitialTorque = torqueCurve.Evaluate(_RPM) + frictionTorque;
        float initialTorque = maxInitialTorque * throttle;

        _effectiveTorque = initialTorque - frictionTorque;

        float angularAcceleration = (_effectiveTorque - loadTorque) / inertia;

        angularVelocity = Mathf.Clamp(
            angularVelocity + angularAcceleration * Time.fixedDeltaTime,
            idleRPM * Helpers.RPM2Rad,
            maxRPM * Helpers.RPM2Rad
        );

        _RPM = angularVelocity * Helpers.Rad2RPM;
    }
}