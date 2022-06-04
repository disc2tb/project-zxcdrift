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

    private float _forceX;
    private float _forceY;
    private float _forceZ;

    [Header("Wheel")]
    public float radius = 0.34f;
    public float tireRelaxationLength = 1;
    public float slipAnglePeak = 8;
    public float inertia = 1.5f;

    [ReadOnly]
    public float _angularVelocity;

    [ReadOnly]
    private float _slipX;
    private float _slipY;
    private float _slipAngle;
    private float _slipAngleDynamic;

    public void Setup(Rigidbody body)
    {
        _body = body;
        _model = transform.GetChild(0).GetComponent<Transform>();
    }

    public void Step(float torque)
    {
        Acceleration(torque);

        if (SuspensionForce())
        {
            _linearVelocityLocal = transform.InverseTransformDirection(_body.GetPointVelocity(_hit.point));

            SlipX(torque);
            SlipY();
            TireForce(torque);

            Debug.DrawRay(transform.position, transform.right * _slipY * 2, Color.red);
            Debug.DrawRay(transform.position, transform.up * _forceY * 0.0001f, Color.green);
            Debug.DrawRay(transform.position, transform.forward * _slipX, Color.blue);
        }

        UpdateModel();
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

    private void Acceleration(float torque)
    {
        float totalTorque = torque - _forceZ * radius; // torque - frictionTorque
        float angularAcceleration = totalTorque / inertia;

        _angularVelocity = Mathf.Clamp(_angularVelocity + angularAcceleration * Time.fixedDeltaTime, -120, 120);
    }

    private void SlipX(float torque)
    {
        float targetAngularVelocity = _linearVelocityLocal.z / radius;
        float targetAngularAcceleration = (_angularVelocity - targetAngularVelocity) / Time.fixedDeltaTime;
        float targetFrictionTorque = targetAngularAcceleration * inertia;
        float maxFrictionTorque = _forceY * radius;

        if (_forceY == 0)
            _slipX = 0;
        else
            _slipX = Mathf.Clamp(targetFrictionTorque / maxFrictionTorque, -1, 1); // TODO: remove clamp
    }

    private void SlipY()
    {
        if (_linearVelocityLocal.z != 0)
        {
            _slipAngle = Mathf.Rad2Deg * Mathf.Atan(-_linearVelocityLocal.x / Mathf.Abs(_linearVelocityLocal.z));
        }
        else
        {
            _slipAngle = 0;
        }

        float slipAngleLerp = Mathf.Lerp(slipAnglePeak * Mathf.Sign(-_linearVelocityLocal.x), _slipAngle,
            Helpers.Map(3, 6, 0, 1, _linearVelocityLocal.magnitude)
        );

        float coeff = Mathf.Abs(_linearVelocityLocal.x) / tireRelaxationLength/* * Time.fixedDeltaTime*/;

        _slipAngleDynamic = Mathf.Clamp(
            _slipAngleDynamic + (slipAngleLerp - _slipAngleDynamic) * Mathf.Clamp(coeff, 0, 1),
            -90, 90
        );

        _slipY = Mathf.Clamp(_slipAngleDynamic / slipAnglePeak, -1, 1); // TODO: remove clamp
    }

    private void TireForce(float torque)
    {
        _forceX = Mathf.Max(_forceY, 0) * _slipY;
        _forceZ = Mathf.Max(_forceY, 0) * _slipX;

        Vector3 force =
            Vector3.ProjectOnPlane(transform.right, _hit.normal).normalized * _forceX
            + Vector3.ProjectOnPlane(transform.forward, _hit.normal).normalized * _forceZ;

        _body.AddForceAtPosition(force, _hit.point);
    }

    private void UpdateModel()
    {
        _model.localPosition = new Vector3(0, -_length, 0);
        _model.Rotate(_angularVelocity * Mathf.Rad2Deg * Time.fixedDeltaTime, 0, 0);
    }
}