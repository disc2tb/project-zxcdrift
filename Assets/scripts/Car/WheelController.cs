using UnityEngine;

public class WheelController : MonoBehaviour
{
    private Rigidbody _body;
    private RaycastHit _hit;
    private Vector3 _linearVelocityLocal;
    private Transform _model;

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
    [ReadOnly]
    public float _slipX;
    [ReadOnly]
    public float _slipY;

    public void Setup(Rigidbody body)
    {
        _body = body;
        _model = transform.GetChild(0).GetComponent<Transform>();
    }

    public void Step(float throttle)
    {
        if (SuspensionForce())
        {
            _linearVelocityLocal = transform.InverseTransformDirection(_body.GetPointVelocity(_hit.point));

            SlipX(throttle);
            SlipY();
            TireForce(throttle);
        }

        UpdateModel();

        // Debug.DrawRay(transform.position, transform.right * _forceX, Color.red);
        Debug.DrawRay(transform.position, transform.up * _forceY * 0.0001f, Color.green);
        // Debug.DrawRay(transform.position, transform.forward * _forceZ, Color.blue);
    }

    private bool SuspensionForce()
    {
        if (Physics.Raycast(transform.position, -transform.up, out _hit, springRestLength + springTravel + radius))
        {
            _lastLength = _length;
            _length = (transform.position - (_hit.point + (transform.up * radius))).magnitude;
            _springVelocity = (_lastLength - _length) / Time.fixedDeltaTime;
            _springForce = (springRestLength - _length) * springStiffness;

            _damperForce = damperStiffness * _springVelocity;

            _forceY = _springForce + _damperForce;
            _suspensionForce = _forceY * _hit.normal.normalized;

            _body.AddForceAtPosition(_suspensionForce, transform.position);
            return true;
        }
        else
        {
            _length = springRestLength + springTravel;
            return false;
        }
    }

    private void SlipX(float throttle)
    {
        _slipX = throttle;
    }

    private void SlipY()
    {
        _slipY = Mathf.Clamp(_linearVelocityLocal.x / -1, -1, 1);
    }

    private void TireForce(float torque)
    {
        Vector3 force =
            Vector3.ProjectOnPlane(transform.right, _hit.normal).normalized *
                (Mathf.Max(_forceY, 0) * _slipY) +
            Vector3.ProjectOnPlane(transform.forward, _hit.normal).normalized *
                (Mathf.Max(_forceY, 0) * _slipX);

        _body.AddForceAtPosition(force, _hit.point);
    }

    private void UpdateModel()
    {
        _model.localPosition = new Vector3(0, -_length, 0);
    }
}