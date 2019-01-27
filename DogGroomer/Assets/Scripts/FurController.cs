using System.Collections;
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
    private Animator _animator;

    [SerializeField, Range(0f, 1f)]
    private float _hairLength;

    [SerializeField, Range(0f, 100f)]
    private float _minHairSize;

    [SerializeField, Range(0f, 100f)]
    private float _maxHairSize;

    [SerializeField, Range(1, 10)]
    private int _hairNodes;

    [SerializeField]
    private int _cutFurInterval;

    [SerializeField]
    private Transform _tongueBase;

    [SerializeField]
    private Vector3 _tongueInPosition;

    [SerializeField]
    private Vector3 _tongueInRotation;

    [SerializeField]
    private List<Vector3> _tonguePositions;

    [SerializeField]
    private List<Vector3> _tongueRotations;

    [SerializeField]
    private List<Razor> _razors;

    private List<Fur> _fur = new List<Fur>();
    private ParticleSystem.Particle[] _particles;
    private List<ParticleSystem.Particle> _enterParticles = new List<ParticleSystem.Particle>();
    private bool _isReady;
    private int _cutFurCount;

    private void Awake()
    {
        Reset();

        StartCoroutine(IdleSequence());
        StartCoroutine(BlinkSequence());
        StartCoroutine(WagSequence());
        StartCoroutine(TongueSequence());
    }

    public void Reset()
    {
        _isReady = false;

        _cutFurParticleSystem.Clear();
        _particleSystem.Clear();
        _enterParticles.Clear();
        _fur.Clear();
        _particles = null;

        var emitParams = new ParticleSystem.EmitParams();
        emitParams.velocity = Vector3.zero;
        emitParams.startLifetime = 9999999f;

        Mesh mesh = new Mesh();
        _skinnedMeshRenderer.BakeMesh(mesh);

        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;
        Color32[] colors = mesh.colors32;

        int count = mesh.vertexCount;
        for (int i = 0; i < count; i++)
        {
            if (colors[i].r <= 0f)
                continue;

            emitParams.startSize = GetHairSize(colors[i]);

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
            if (colors[i].r > 0f)
            {
                float hairLength = GetHairLength(colors[i]);

                Fur fur = new Fur(transform, _hairNodes);
                fur.UpdateParticles(ref _particles, stride, vertices[i], normals[i], hairLength, _hairNodes, true);

                _fur.Add(fur);

                stride += _hairNodes;
            }
        }

        _particleSystem.SetParticles(_particles, _particles.Length);

        _isReady = true;
    }

    private void OnParticleCollision(GameObject other)
    {
        for (int i = 0; i < _razors.Count; i++)
        {
            if (other == _razors[i].gameObject)
            {
                _razors[i].CutFur();
                return;
            }
        }
    }
    
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

            _cutFurCount++;

            if (_cutFurCount % _cutFurInterval == 0)
            {
                SpawnCutFur(p.position, p.startSize);
            }
        }
        
        _particleSystem.SetTriggerParticles(ParticleSystemTriggerEventType.Enter, _enterParticles);
    }

    private void SpawnCutFur(Vector3 position, float size)
    {
        var emitParams = new ParticleSystem.EmitParams();
        emitParams.velocity = Vector3.zero;
        emitParams.startLifetime = 9999999f;
        emitParams.startSize = size;
        emitParams.position = position;
        emitParams.rotation3D = UnityEngine.Random.rotation.eulerAngles;
        emitParams.velocity = UnityEngine.Random.insideUnitSphere;

        _cutFurParticleSystem.Emit(emitParams, 1);
    }

    private IEnumerator IdleSequence()
    {
        while (true)
        {
            float roll = UnityEngine.Random.value;
            if (roll < 0.5f)
            {
                _animator.SetTrigger("Look");
            }

            yield return new WaitForSeconds(GetRandomIdleSeconds());
        }
    }

    private IEnumerator BlinkSequence()
    {
        while (true)
        {
            _animator.SetTrigger("Blink");

            yield return new WaitForSeconds(GetRandomBlinkSeconds());
        }
    }

    private IEnumerator WagSequence()
    {
        _animator.SetBool("Wagging", true);
        yield break;
    }

    private IEnumerator TongueSequence()
    {
        while (true)
        {
            Vector3 initialPosition = _tongueBase.localPosition;
            Vector3 initialRotation = _tongueBase.localRotation.eulerAngles;

            int index = UnityEngine.Random.Range(0, _tonguePositions.Count);
            for (int i = 0; i < 10; i++)
            {
                float ratio = (float)i / 10f;
                _tongueBase.localPosition = Vector3.Lerp(initialPosition, _tongueInPosition, ratio);
                _tongueBase.localRotation = Quaternion.Euler(Vector3.Lerp(initialRotation, _tongueInRotation, ratio));

                yield return null;
            }

            yield return new WaitForSeconds(0.25f);

            for (int i = 0; i < 10; i++)
            {
                float ratio = (float)i / 10f;
                _tongueBase.localPosition = Vector3.Lerp(_tongueInPosition, _tonguePositions[index], ratio);
                _tongueBase.localRotation = Quaternion.Euler(Vector3.Lerp(_tongueInRotation, _tongueRotations[index], ratio));

                yield return null;
            }

            yield return new WaitForSeconds(GetRandomBlinkSeconds());
        }
    }

    private float GetRandomBlinkSeconds()
    {
        return UnityEngine.Random.Range(3f, 8f);
    }

    private float GetRandomIdleSeconds()
    {
        return UnityEngine.Random.Range(5f, 10f);
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
            _fur[i].UpdateParticles(ref _particles, stride, vertices[i], normals[i], hairLength, _hairNodes);

            stride += _hairNodes;
        }

        _particleSystem.SetParticles(_particles, _particles.Length);
    }

    private float GetHairLength(Color32 color)
    {
        return _hairLength * (color.r / 255f);
    }

    private float GetHairSize(Color32 color)
    {
        return Mathf.Max(_minHairSize, _maxHairSize * (color.r / 255f));
    }
}