using System;
using System.Collections.Generic;
using UnityEngine;

public class Fur
{
    public event Action<Vector3, float, Color> onCut;

    private Transform _transform;
    private List<Vector3> _offsets;

    public Fur(Transform transform, int hairNodes)
    {
        _transform = transform;
        _offsets = new List<Vector3>();
        for (int i = 0; i < hairNodes; i++)
        {
            _offsets.Add(UnityEngine.Random.insideUnitSphere * 0.02f);
        }
    }

    public void Cleanup()
    {
        _transform = null;
        _offsets.Clear();
    }

    public void UpdateParticles(ref ParticleSystem.Particle[] particles, int stride, Vector3 vertexPosition, Vector3 vertexNormal, float hairLength, int hairNodes, bool forcePosition, Action<Vector3, float> spawnFunc = null)
    {
        bool foundCutNode = false;
        for (int j = hairNodes - 1; j >= 0; j--)
        {
            ParticleSystem.Particle particle = particles[stride + j];

            if (particle.remainingLifetime <= 0f)
            {
                if (!foundCutNode)
                    foundCutNode = true;

                particles[stride + j] = particle;

                continue;
            }

            if (foundCutNode && particle.remainingLifetime > 0f)
            {
                particle.remainingLifetime = 0f;

                if (onCut != null)
                    onCut(particle.position, particle.startSize, particle.startColor);

                particles[stride + j] = particle;

                continue;
            }

            Vector3 normalOffset = vertexNormal * (1f - (float)(j + 1) / (float)hairNodes) * hairLength;

            float ratio = 1f - ((float)j / (float)(hairNodes - 1) + 0.2f);

            if (forcePosition)
            {
                particle.position = _transform.TransformPoint(vertexPosition + normalOffset + _offsets[j]);
            }
            else
            {
                particle.position = _transform.TransformPoint(Vector3.Lerp(vertexPosition + normalOffset + _offsets[j], _transform.InverseTransformPoint(particle.position), ratio));
            }

            particles[stride + j] = particle;
        }
    }
}