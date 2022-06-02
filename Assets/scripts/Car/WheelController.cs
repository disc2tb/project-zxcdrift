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
    private float _minLength;
    private float _maxLength;

    private float _springForce;
    private float _springVelocity;
    private float _damperForce;
    private Vector3 _suspensionForce;

    [Header("Wheel")]
    public float radius = 0.34f;

    public void Setup(Rigidbody body)
    {
        _body = body;
        _minLength = springRestLength - springTravel;
        _maxLength = springRestLength + springTravel;
    }

    public void Step(float throttle, float brake, float handbrake)
    {
        Raycast();
    }

    private void Raycast()
    {
        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, _maxLength + radius))
        {
            _lastLength = _length;
            _length = Mathf.Clamp(hit.distance - radius, _minLength, _maxLength);
            _springVelocity = (_lastLength - _length) / Time.fixedDeltaTime;
            _springForce = springStiffness * (springRestLength - _length);

            _damperForce = damperStiffness * _springVelocity;

            _suspensionForce = (_springForce + _damperForce) * transform.up;

            _body.AddForceAtPosition(_suspensionForce, transform.position);
        }
        else
        {
            _length = _maxLength;
        }

        Debug.DrawRay(transform.position, -transform.up * (_length + radius), Color.magenta);
    }
}