using System.Collections.Generic;
using UnityEngine;

public class Floor : MonoBehaviour
{
    [SerializeField]
    private ParticleSystem _particleSystem;

    private List<ParticleSystem.Particle> _enterParticles = new List<ParticleSystem.Particle>();

    private void OnParticleTrigger()
    {
        int numEnter = _particleSystem.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, _enterParticles);
        for (int i = 0; i < numEnter; i++)
        {
            ParticleSystem.Particle p = _enterParticles[i];

            if (p.remainingLifetime == 0f)
                continue;

            p.remainingLifetime = 0f;
            _enterParticles[i] = p;
        }

        _particleSystem.SetTriggerParticles(ParticleSystemTriggerEventType.Enter, _enterParticles);
    }
}