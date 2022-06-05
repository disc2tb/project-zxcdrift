using System.Collections;
using UnityEngine;

public class GearboxController : MonoBehaviour
{
    public float mainRatio = 3.900f;

    public float[] ratios = { 0, 3.636f, 2.375f, 1.761f, 1.346f, 1.062f, 0.842f };
    public float reverseRatio = -3.545f;

    [ReadOnly]
    public float ratio;

    private int _gear;
    private bool _inGear;

    public float gearShiftTime = 0.1f;

    private void SetGear(int gear)
    {
        _gear = gear;

        if (gear == 0)
            _inGear = false;
        else
            _inGear = true;

        if (gear == -1)
            ratio = reverseRatio * mainRatio;
        else
            ratio = ratios[gear] * mainRatio;
    }

    public IEnumerator ShiftGearUp()
    {
        if (_gear < ratios.Length - 1 && _inGear)
        {
            int prevGear = _gear;
            SetGear(0);
            yield return new WaitForSeconds(gearShiftTime);
            SetGear(prevGear + 1);
        }
        else if (!_inGear && _gear == 0)
        {
            SetGear(_gear + 1);
        }
    }

    public IEnumerator ShiftGearDown()
    {
        if (_gear > -1 && _inGear)
        {
            int prevGear = _gear;
            SetGear(0);
            yield return new WaitForSeconds(gearShiftTime);
            SetGear(prevGear - 1);
        }
        else if (!_inGear && _gear == 0)
        {
            SetGear(_gear - 1);
        }
    }

    public float GetOutputTorque(float inputTorque)
    {
        return inputTorque * ratio;
    }

    public float GetInputShaftVelocity(float outputShaftVelocity)
    {
        return outputShaftVelocity * ratio;
    }
}