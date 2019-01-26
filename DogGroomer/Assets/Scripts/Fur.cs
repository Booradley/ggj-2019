using UnityEngine;

public class Fur
{
    private Transform _transform;

    public Fur(Transform transform)
    {
        _transform = transform;
    }

    public void UpdateParticles(ref ParticleSystem.Particle[] particles, int stride, Vector3 vertexPosition, Vector3 vertexNormal, float hairLength, int hairNodes, bool forcePosition = false)
    {
        for (int j = 0; j < hairNodes; j++)
        {
            ParticleSystem.Particle particle = particles[stride + j];

            Vector3 normalOffset = vertexNormal * (1f - (float)(j + 1) / (float)hairNodes) * hairLength;

            float ratio = ((float)j / (float)(hairNodes - 1) * 0.5f);

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