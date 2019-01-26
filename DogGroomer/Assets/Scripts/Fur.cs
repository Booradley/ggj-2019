using System.Collections.Generic;
using UnityEngine;

public class Fur
{
    private Transform _transform;
    private List<ParticleSystem.Particle> _particles;

    public Fur(Transform transform)
    {
        _transform = transform;
        _particles = new List<ParticleSystem.Particle>();
    }

    public void AddParticle(ParticleSystem.Particle particle)
    {
        _particles.Add(particle);
    }

    public void UpdateParticles(Vector3 vertexPosition, Vector3 vertexNormal)
    {
        for (int i = 0; i < FurController.FUR_NODES; i++)
        {
            ParticleSystem.Particle particle = _particles[i];
            particle.position = _transform.TransformPoint(vertexPosition + (_transform.TransformDirection(vertexNormal * (1f - (float)i / (float)FurController.FUR_NODES) * 0.1f)));
            _particles[i] = particle;
        }
    }
}