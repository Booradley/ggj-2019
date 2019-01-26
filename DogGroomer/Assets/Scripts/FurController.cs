using System.Collections.Generic;
using UnityEngine;

public class FurController : MonoBehaviour
{
    public const int FUR_NODES = 3;

    [SerializeField]
    private ParticleSystem _particleSystem;

    [SerializeField]
    private MeshFilter _meshFilter;

    private List<Fur> _fur = new List<Fur>();
    private ParticleSystem.Particle[] _particles;
    private bool _isReady;

    private void Awake()
    {
        var emitParams = new ParticleSystem.EmitParams();
        emitParams.velocity = Vector3.zero;
        emitParams.startLifetime = 99999;

        Vector3[] vertices = _meshFilter.mesh.vertices;
        int count = _meshFilter.mesh.vertexCount;
        for (int i = 0; i < count; i++)
        {
            vertices[i] = Vector3.Scale(vertices[i], transform.localScale);
            for (int j = 0; j < FUR_NODES; j++)
            {
                emitParams.rotation3D = Random.rotation.eulerAngles;

                _particleSystem.Emit(emitParams, 1);
            }
        }

        _particleSystem.Play();

        _particles = new ParticleSystem.Particle[_particleSystem.particleCount];
        _particleSystem.GetParticles(_particles);

        for (int i = 0; i < count; i++)
        {
            Fur fur = new Fur(transform);
            for (int j = 0; j < FUR_NODES; j++)
            {
                fur.AddParticle(_particles[i + j]);
            }

            _fur.Add(fur);
        }

        _isReady = true;
    }

    private void Update()
    {
        if (!_isReady)
            return;

        Vector3[] vertices = _meshFilter.mesh.vertices;
        Vector3[] normals = _meshFilter.mesh.normals;

        _particleSystem.GetParticles(_particles);

        int count = _fur.Count;
        int stride = 0;
        for (int i = 0; i < count; i++)
        {
            for (int j = 0; j < FurController.FUR_NODES; j++)
            {
                ParticleSystem.Particle particle = _particles[stride + j];
                particle.position = transform.TransformPoint(vertices[i] + (transform.TransformDirection(normals[i])));
                _particles[stride + j] = particle;
            }

            stride += FUR_NODES;

            //_fur[i].UpdateParticles(vertices[i], particleDirection[i]);
        }

        _particleSystem.SetParticles(_particles, _particles.Length);
    }
}