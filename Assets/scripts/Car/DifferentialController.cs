using UnityEngine;

public class DifferentialController : MonoBehaviour
{
    // Simmetrical open differential
    public float ratio = 3.9f;

    public float[] GetOutputTorque(float inputTorque)
    {
        float[] torques = new float[2]
        {
            inputTorque * ratio * 0.5f,
            inputTorque * ratio * 0.5f
        };

        return torques;
    }

    public float GetInputShaftVelocity(float outputShaftVelocityLeft, float outputShaftVelocityRight)
    {
        return (outputShaftVelocityLeft + outputShaftVelocityRight) * 0.5f * ratio;
    }
}