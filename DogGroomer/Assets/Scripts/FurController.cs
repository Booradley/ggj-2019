using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

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
    private List<FurInteractable> _furInteractables;

    private List<Fur> _fur = new List<Fur>();
    private Mesh _bakedMesh;
    private List<Vector3> _vertices = new List<Vector3>();
    private List<Vector3> _normals = new List<Vector3>();
    private List<Color32> _colors = new List<Color32>();
    private ParticleSystem.Particle[] _particles;
    private List<ParticleSystem.Particle> _enterParticles = new List<ParticleSystem.Particle>();
    private bool _isReady;

    private void Awake()
    {
        _bakedMesh = new Mesh();

        Reset();
    }

    public IEnumerator Start()
    {
        StartCoroutine(IdleSequence());
        StartCoroutine(BlinkSequence());
        StartCoroutine(WagSequence());
        StartCoroutine(TongueSequence());

        SteamVR_Fade.Start(Color.clear, 1.0f);

        yield return new WaitForSeconds(1.0f);
    }

    public void Reset()
    {
        _isReady = false;

        for (int i = 0; i < _fur.Count; i++)
        {
            _fur[i].onCut -= SpawnCutFur;
            _fur[i].Cleanup();
        }

        _fur.Clear();
        
        _cutFurParticleSystem.Clear();
        _particleSystem.Clear();
        _enterParticles.Clear();
        _particles = null;

        var emitParams = new ParticleSystem.EmitParams();
        emitParams.velocity = Vector3.zero;
        emitParams.startLifetime = 9999999f;
        
        _skinnedMeshRenderer.BakeMesh(_bakedMesh);

        Vector3[] vertices = _bakedMesh.vertices;
        Vector3[] normals = _bakedMesh.normals;
        Color32[] colors = _bakedMesh.colors32;

        int count = _bakedMesh.vertexCount;
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
                fur.onCut += SpawnCutFur;
                fur.UpdateParticles(ref _particles, stride, vertices[i], normals[i], hairLength, _hairNodes, true);

                _fur.Add(fur);

                stride += _hairNodes;
            }
        }

        _particleSystem.SetParticles(_particles, _particles.Length);

        _isReady = true;
    }
    
    private void OnParticleTrigger()
    {
        int numEnter = _particleSystem.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, _enterParticles);

        for (int i = 0; i < numEnter; i++)
        {
            ParticleSystem.Particle p = _enterParticles[i];

            FurInteractable closestInteractable = null;
            float smallestDistance = float.MaxValue;
            for (int j = 0; j < _furInteractables.Count; j++)
            {
                float distance = Vector3.Distance(_furInteractables[j].transform.position, p.position);
                if (distance < smallestDistance)
                {
                    smallestDistance = distance;
                    closestInteractable = _furInteractables[j];
                }
            }

            closestInteractable.Interact();
            
            if (closestInteractable is Razor)
            {
                if (p.remainingLifetime > 0f)
                    SpawnCutFur(p.position, p.startSize, p.startColor);
                    
                p.remainingLifetime = 0f;
            }
            else if (closestInteractable is Brush)
            {
                p.startColor = (closestInteractable as Brush).color;
            }

            _enterParticles[i] = p;
        }
        
        _particleSystem.SetTriggerParticles(ParticleSystemTriggerEventType.Enter, _enterParticles);
    }

    private void SpawnCutFur(Vector3 position, float size, Color color)
    {
        var emitParams = new ParticleSystem.EmitParams();
        emitParams.velocity = Vector3.zero;
        emitParams.startLifetime = 9999999f;
        emitParams.startSize = size;
        emitParams.startColor = color;
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
            if (roll < 0.25f)
            {
                _animator.SetTrigger("Shake");
            }
            else if (roll < 0.5f)
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
        
        _skinnedMeshRenderer.BakeMesh(_bakedMesh);

        _bakedMesh.GetVertices(_vertices);

        if (_normals.Count == 0)
            _bakedMesh.GetNormals(_normals);

        if (_colors.Count == 0)
            _bakedMesh.GetColors(_colors);

        _particleSystem.GetParticles(_particles);

        int count = _fur.Count;
        int stride = 0;
        for (int i = 0; i < count; i++)
        {
            _fur[i].UpdateParticles(ref _particles, stride, _vertices[i], _normals[i], GetHairLength(_colors[i]), _hairNodes, false);
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