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

    private Vector3 _force;

    [Header("Wheel")]
    public float radius = 0.34f;
    public float tireRelaxationLength = 1;
    public float slipAnglePeak = 8;
    public float inertia = 1.5f;

    [ReadOnly]
    public float _angularVelocity;

    [ReadOnly]
    private Vector3 _slip; // up vector is unused, vector3 just because x and z components
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

            SlipZ(torque);
            SlipX();
            // SlipC(); :smirk_cat:
            TireForce(torque);

            Debug.DrawRay(transform.position, transform.right * _slip.x * 2, Color.red);
            Debug.DrawRay(transform.position, transform.up * _force.y * 0.0001f, Color.green);
            Debug.DrawRay(transform.position, transform.forward * _slip.z, Color.blue);
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

            _force.y = _springForce + _damperForce;
            _suspensionForce = _force.y * _hit.normal.normalized;

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
        float totalTorque = torque - _force.z * radius; // torque - frictionTorque
        float angularAcceleration = totalTorque / inertia;

        _angularVelocity = Mathf.Clamp(_angularVelocity + angularAcceleration * Time.fixedDeltaTime, -120, 120);
    }

    private void SlipZ(float torque)
    {
        float targetAngularVelocity = _linearVelocityLocal.z / radius;
        float targetAngularAcceleration = (_angularVelocity - targetAngularVelocity) / Time.fixedDeltaTime;
        float targetFrictionTorque = targetAngularAcceleration * inertia;
        float maxFrictionTorque = _force.y * radius;

        if (_force.y == 0)
            _slip.z = 0;
        else
            _slip.z = Mathf.Clamp(targetFrictionTorque / maxFrictionTorque, -1, 1); // TODO: remove clamp
    }

    private void SlipX()
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

        _slip.x = Mathf.Clamp(_slipAngleDynamic / slipAnglePeak, -1, 1); // TODO: remove clamp
    }

    private void TireForce(float torque)
    {
        _force.x = Mathf.Max(_force.y, 0) * _slip.x;
        _force.z = Mathf.Max(_force.y, 0) * _slip.z;

        Vector3 force =
            Vector3.ProjectOnPlane(transform.right, _hit.normal).normalized * _force.x
            + Vector3.ProjectOnPlane(transform.forward, _hit.normal).normalized * _force.z;

        _body.AddForceAtPosition(force, _hit.point);
    }

    private void UpdateModel()
    {
        _model.localPosition = new Vector3(0, -_length, 0);
        _model.Rotate(_angularVelocity * Mathf.Rad2Deg * Time.fixedDeltaTime, 0, 0);
    }
}