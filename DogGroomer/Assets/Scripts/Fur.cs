using UnityEngine;

public class Fur
{
    private Transform _transform;

    public Fur(Transform transform)
    {
        _transform = transform;
    }

    public void UpdateParticles(ref ParticleSystem.Particle[] particles, int stride, Vector3 vertexPosition, Vector3 vertexNormal, float hairLength, bool forcePosition = false)
    {
        for (int j = 0; j < FurController.FUR_NODES; j++)
        {
            ParticleSystem.Particle particle = particles[stride + j];

            Vector3 normalOffset = _transform.TransformDirection(vertexNormal) * (1f - (float)(j + 1) / (float)FurController.FUR_NODES) * hairLength;

            float ratio = ((float)j / (float)(FurController.FUR_NODES - 1) * 0.5f);

            if (forcePosition)
            {
                particle.position = _transform.TransformPoint(vertexPosition + normalOffset);
            }
            else
            {
                particle.position = _transform.TransformPoint(Vector3.Lerp(vertexPosition + normalOffset, _transform.InverseTransformPoint(particle.position), ratio));
            }

            particles[stride + j] = particle;
        }
    }
}