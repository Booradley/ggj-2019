using UnityEngine;
using Valve.VR.InteractionSystem;

public class LazySusan : MonoBehaviour
{
    [SerializeField]
    private Interactable _interactable;

    private Hand.AttachmentFlags _attachmentFlags = Hand.defaultAttachmentFlags & (~Hand.AttachmentFlags.SnapOnAttach) & (~Hand.AttachmentFlags.DetachOthers) & (~Hand.AttachmentFlags.VelocityMovement);

    private void HandHoverUpdate(Hand hand)
    {
        GrabTypes startingGrabType = hand.GetGrabStarting();
        bool isGrabEnding = hand.IsGrabEnding(this.gameObject);

        if (_interactable.attachedToHand == null && startingGrabType != GrabTypes.None)
        {
            hand.HoverLock(_interactable);
            hand.AttachObject(gameObject, startingGrabType, _attachmentFlags);
        }
        else if (isGrabEnding)
        {
            hand.DetachObject(gameObject);
            hand.HoverUnlock(_interactable);
        }
    }
}