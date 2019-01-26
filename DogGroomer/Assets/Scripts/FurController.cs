using System;
using System.Collections.Generic;
using UnityEngine;

public class FurController : MonoBehaviour
{
    [SerializeField]
    private ParticleSystem _particleSystem;

    [SerializeField]
    private ParticleSystem _cutFurParticleSystem;

    [SerializeField]
    private SkinnedMeshRenderer _skinnedMeshRenderer;

    [SerializeField]
    private Razor _razor;

    [SerializeField, Range(0f, 1f)]
    private float _hairLength;

    [SerializeField, Range(1, 10)]
    private int _hairNodes;

    private List<Fur> _fur = new List<Fur>();
    private ParticleSystem.Particle[] _particles;
    List<ParticleSystem.Particle> enter = new List<ParticleSystem.Particle>();
    private bool _isReady;

    private void Awake()
    {
        var emitParams = new ParticleSystem.EmitParams();
        emitParams.velocity = Vector3.zero;
        emitParams.startLifetime = 9999999f;
        emitParams.startSize = 0.025f;

        Mesh mesh = new Mesh();
        _skinnedMeshRenderer.BakeMesh(mesh);

        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;
        Color32[] colors = mesh.colors32;

        int count = mesh.vertexCount;
        for (int i = 0; i < count; i++)
        {
            for (int j = 0; j < _hairNodes; j++)
            {
                emitParams.rotation3D = UnityEngine.Random.rotation.eulerAngles;

                _particleSystem.Emit(emitParams, 1);
            }
        }

        _particleSystem.Play();
        _particles = new ParticleSystem.Particle[_particleSystem.particleCount];
        _particleSystem.GetParticles(_particles);

        int stride = 0;

        for (int i = 0; i < count; i++)
        {
            float hairLength = GetHairLength(colors[i]);
            int hairNodes = GetHairNodes(colors[i]);

            Fur fur = new Fur(transform);
            fur.UpdateParticles(ref _particles, stride, vertices[i], normals[i], hairLength, hairNodes, true);

            _fur.Add(fur);

            stride += _hairNodes;
        }

        _particleSystem.SetParticles(_particles, _particles.Length);

        _isReady = true;
    }

    public void SetActiveRazor(Razor razor)
    {
        _razor = razor;
    }

    private void OnParticleTrigger()
    {
        int numEnter = _particleSystem.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, enter);
        for (int i = 0; i < numEnter; i++)
        {
            ParticleSystem.Particle p = enter[i];

            if (p.remainingLifetime == 0f)
                continue;

            p.remainingLifetime = 0f;
            enter[i] = p;

            SpawnCutFur(p.position);
        }
        
        _particleSystem.SetTriggerParticles(ParticleSystemTriggerEventType.Enter, enter);
    }

    private void SpawnCutFur(Vector3 position)
    {
        var emitParams = new ParticleSystem.EmitParams();
        emitParams.velocity = Vector3.zero;
        emitParams.startLifetime = 9999999f;
        emitParams.startSize = 0.025f;
        emitParams.position = position;
        emitParams.rotation3D = UnityEngine.Random.rotation.eulerAngles;
        emitParams.velocity = UnityEngine.Random.insideUnitSphere;

        _cutFurParticleSystem.Emit(emitParams, 1);
    }

    private void LateUpdate()
    {
        if (!_isReady)
            return;

        Mesh mesh = new Mesh();
        _skinnedMeshRenderer.BakeMesh(mesh);

        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;
        Color32[] colors = mesh.colors32;

        _particleSystem.GetParticles(_particles);

        int count = _fur.Count;
        int stride = 0;
        for (int i = 0; i < count; i++)
        {
            float hairLength = GetHairLength(colors[i]);
            int hairNodes = GetHairNodes(colors[i]);

            _fur[i].UpdateParticles(ref _particles, stride, vertices[i], normals[i], hairLength, hairNodes);

            stride += _hairNodes;
        }

        _particleSystem.SetParticles(_particles, _particles.Length);
    }

    private float GetHairLength(Color32 color)
    {
        return _hairLength * (color.r / 255f);
    }

    private int GetHairNodes(Color32 color)
    {
        return _hairNodes;
    }
}