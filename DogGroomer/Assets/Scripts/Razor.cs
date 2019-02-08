using System.Collections;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class Razor : FurInteractable
{
    [SerializeField]
    private Interactable _interactable;

    [SteamVR_DefaultAction("Razor")]
    public SteamVR_Action_Boolean razorAction;

    [SerializeField]
    private AudioSource _audioSource;

    [SerializeField]
    private AudioSource _audioSourceCut;

    [SerializeField]
    private AudioClip _onAudioClip;

    [SerializeField]
    private AudioClip _cutAudioClip;

    [SerializeField]
    private SteamVR_Action_Vibration _hapticAction;

    private Hand.AttachmentFlags _attachmentFlags = Hand.defaultAttachmentFlags & (~Hand.AttachmentFlags.SnapOnAttach) & (~Hand.AttachmentFlags.DetachOthers) & (~Hand.AttachmentFlags.VelocityMovement);

    private bool _isOn;
    public bool isOn { get { return _isOn; } }

    private Quaternion _initialRotation;
    private Vector3 _initialPosition;
    private Hand _hand;
    private Coroutine _onCoroutine;
    private Coroutine _cutCoroutine;
    private Coroutine _cutEndCoroutine;
    private bool _needsCut = false;
    private float _hapticAmplitude = 0.02f;

    private void Start()
    {
        _initialPosition = transform.position;
        _initialRotation = transform.rotation;
    }

    public void Reset()
    {
        if (_interactable.attachedToHand != null)
        {
            _interactable.attachedToHand.DetachObject(gameObject);
            _interactable.attachedToHand.HoverUnlock(_interactable);
        }

        transform.position = _initialPosition;
        transform.rotation = _initialRotation;
    }

    private void OnAttachedToHand(Hand hand)
    {
        _hand = hand;
    }

    private void OnDetachedFromHand(Hand hand)
    {
        TurnOff();
    }

    private void Update()
    {
        if (_interactable.attachedToHand)
        {
            //SteamVR_Input_Sources hand = _interactable.attachedToHand.handType;
            bool buttonDown = true;// razorAction.GetState(hand);
            if (!_isOn && buttonDown)
            {
                _isOn = true;
                _onCoroutine = StartCoroutine(OnSequence());
            }
            else if (_isOn && !buttonDown)
            {
                TurnOff();
            }
        }
    }

    private IEnumerator OnSequence()
    {
        _audioSource.loop = true;
        _audioSource.clip = _onAudioClip;
        _audioSource.volume = 0.05f;
        _audioSource.Play();

        WaitForSeconds interval = new WaitForSeconds(0.1f);
        while(true)
        {
            if (_hand != null)
            {
                _hapticAction.Execute(0, 0.05f, 50, _hapticAmplitude, _hand.handType);
            }

            yield return interval;
        }
    }

    public override void Interact()
    {
        _needsCut = true;

        if (_cutEndCoroutine != null)
        {
            StopCoroutine(_cutEndCoroutine);
            _cutEndCoroutine = null;
        }

        _cutEndCoroutine = StartCoroutine(CutEndSequence());

        if (_cutCoroutine == null)
        {
            _cutCoroutine = StartCoroutine(CutSequence());
        }
    }

    private IEnumerator CutEndSequence()
    {
        yield return new WaitForSeconds(0.15f);

        _needsCut = false;
        _cutEndCoroutine = null;
    }

    private IEnumerator CutSequence()
    {
        _audioSourceCut.volume = 0f;
        _audioSourceCut.clip = _cutAudioClip;
        _audioSourceCut.Play();

        float ratio;
        for (int i = 0; i < 10; i++)
        {
            ratio = (float)(i + 1) / 10f;
            _audioSource.volume = 0.05f * (1f - ratio);
            _audioSourceCut.volume = 0.10f * ratio;

            yield return null;
        }

        _hapticAmplitude = 1.0f;

        while (_needsCut)
        {
            yield return null;
        }

        _hapticAmplitude = 0.02f;

        for (int i = 0; i < 10; i++)
        {
            ratio = (float)(i + 1) / 10f;
            _audioSource.volume = 0.05f * ratio;
            _audioSourceCut.volume = 0.10f * (1f - ratio);

            yield return null;
        }

        _audioSourceCut.Stop();
        _cutCoroutine = null;
    }

    private void TurnOff()
    {
        if (_onCoroutine != null)
        {
            StopCoroutine(_onCoroutine);
            _onCoroutine = null;
        }

        if (_cutCoroutine != null)
        {
            StopCoroutine(_cutCoroutine);
            _cutCoroutine = null;
        }

        if (_cutEndCoroutine != null)
        {
            StopCoroutine(_cutEndCoroutine);
            _cutEndCoroutine = null;
        }

        _audioSource.Stop();
        _audioSourceCut.Stop();

        _hand = null;
        _isOn = false;
        _needsCut = false;
    }
}