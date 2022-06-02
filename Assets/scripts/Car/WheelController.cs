using UnityEngine;

public class WheelController : MonoBehaviour
{
    private Rigidbody _body;

    [Header("Suspesion")]
    public float springRestLength = 0.5f;
    public float springTravel = 0.2f;
    public float springStiffness = 30000;
    public float damperStiffness = 4000;

    private float _length;
    private float _lastLength;

    private float _springForce;
    private float _springVelocity;
    private float _damperForce;
    private Vector3 _suspensionForce;

    private float _forceY;

    [Header("Wheel")]
    public float radius = 0.34f;

    public void Setup(Rigidbody body)
    {
        _body = body;
    }

    public void Step(float throttle, float brake, float handbrake)
    {
        RaycastHit hit = SuspensionForce();
        TireForce(hit, throttle);
    }

    private RaycastHit SuspensionForce()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, -transform.up, out hit, springRestLength + springTravel + radius))
        {
            _lastLength = _length;
            _length = (transform.position - (hit.point + (transform.up * radius))).magnitude;
            _springVelocity = (_lastLength - _length) / Time.fixedDeltaTime;
            _springForce = (springRestLength - _length) * springStiffness;

            _damperForce = damperStiffness * _springVelocity;

            _forceY = _springForce + _damperForce;
            _suspensionForce = _forceY * hit.normal.normalized;

            _body.AddForceAtPosition(_suspensionForce, transform.position);
        }
        else
        {
            _length = springRestLength + springTravel;
        }

        Debug.DrawRay(transform.position, -transform.up * (_length + radius), Color.magenta);
        return hit;
    }

    private void TireForce(RaycastHit hit, float torque)
    {
        Vector3 linearVelocityLocal = transform.InverseTransformDirection(_body.GetPointVelocity(hit.point));

        Vector3 force =
            Vector3.ProjectOnPlane(transform.right, hit.normal).normalized *
                (Mathf.Max(_forceY, 0) * Mathf.Clamp(linearVelocityLocal.x / -1, -1, 1)) +
            Vector3.ProjectOnPlane(transform.forward, hit.normal).normalized *
                (Mathf.Max(_forceY, 0) * torque);

        _body.AddForceAtPosition(force, hit.point);
    }
}