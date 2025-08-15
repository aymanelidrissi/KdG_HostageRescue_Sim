using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ButtonBumper : MonoBehaviour
{
    public string requiredTag = "KeyItem";
    public bool requireHeld = true;
    public bool openOnce = true;
    public UnityEvent onBumped;

    bool used;

    void OnTriggerEnter(Collider other)
    {
        if (openOnce && used) return;
        if (!other.CompareTag(requiredTag)) return;

        if (requireHeld)
        {
            var grab = other.GetComponent<XRGrabInteractable>();
            if (grab == null || !grab.isSelected) return;
        }

        used = true;
        onBumped?.Invoke();
    }
}
