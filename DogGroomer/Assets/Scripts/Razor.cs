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

    private Hand.AttachmentFlags _attachmentFlags = Hand.defaultAttachmentFlags & (~Hand.AttachmentFlags.SnapOnAttach) & (~Hand.AttachmentFlags.DetachOthers) & (~Hand.AttachmentFlags.VelocityMovement);

    private Hand _hand;
    private bool _isOn;
    private Coroutine _coroutine;

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
        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
            _coroutine = null;
        }

        _hand = null;
        _isOn = false;
    }

    private void Update()
    {
        if (_interactable.attachedToHand)
        {
            SteamVR_Input_Sources hand = _interactable.attachedToHand.handType;
            bool buttonDown = razorAction.GetState(hand);
            if (!_isOn && buttonDown)
            {
                _isOn = true;
                _coroutine = StartCoroutine(OnSequence());
            }
            else if (_isOn && !buttonDown)
            {
                _isOn = false;

                if (_coroutine != null)
                {
                    StopCoroutine(_coroutine);
                    _coroutine = null;
                }
            }
        }
    }

    private IEnumerator OnSequence()
    {
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
}