using System;
using System.Collections;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class Razor : MonoBehaviour
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

    private Hand.AttachmentFlags _attachmentFlags = Hand.defaultAttachmentFlags & (~Hand.AttachmentFlags.SnapOnAttach) & (~Hand.AttachmentFlags.DetachOthers) & (~Hand.AttachmentFlags.VelocityMovement);

    private bool _isOn;
    public bool isOn { get { return _isOn; } }

    private Hand _hand;
    private Coroutine _onCoroutine;
    private Coroutine _cutCoroutine;
    private bool _needsCut = false;

    private void HandHoverUpdate(Hand hand)
    {
        GrabTypes startingGrabType = hand.GetGrabStarting();
        bool isGrabEnding = hand.IsGrabEnding(this.gameObject);

        if (_interactable.attachedToHand == null && startingGrabType != GrabTypes.None)
        {
            hand.HoverLock(_interactable);
            hand.AttachObject(gameObject, startingGrabType, _attachmentFlags);

            _hand = hand;
        }
        else if (isGrabEnding)
        {
            hand.DetachObject(gameObject);
            hand.HoverUnlock(_interactable);
        }
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
        _audioSource.volume = 0.25f;
        _audioSource.Play();

        WaitForSeconds interval = new WaitForSeconds(0.1f);
        while(true)
        {
            if (_hand != null)
            {
                float pulse = 800f;
                _hand.TriggerHapticPulse((ushort)pulse);
            }

            yield return interval;
        }
    }

    public void CutFur()
    {
        _needsCut = true;

        if (_cutCoroutine == null)
        {
            StartCoroutine(CutSequence());
        }
    }

    private IEnumerator CutSequence()
    {
        if (_cutCoroutine != null)
        {
            StopCoroutine(_cutCoroutine);
            _cutCoroutine = null;
        }

        _audioSourceCut.Play();

        float ratio;
        for (int i = 0; i < 10; i++)
        {
            ratio = (float)i / 10f;
            _audioSource.volume = 0.25f * (1f - ratio);
            _audioSourceCut.volume = 0.25f * ratio;

            yield return null;
        }

        while (_needsCut)
        {
            _needsCut = false;

            yield return new WaitForSeconds(0.1f);
        }

        for (int i = 0; i < 10; i++)
        {
            ratio = (float)i / 10f;
            _audioSource.volume = 0.25f * ratio;
            _audioSourceCut.volume = 0.25f * (1f - ratio);

            yield return null;
        }

        _audioSourceCut.Stop();
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

        _audioSource.Stop();
        _audioSourceCut.Stop();

        _hand = null;
        _isOn = false;
    }
}