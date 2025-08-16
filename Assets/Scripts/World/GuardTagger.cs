using UnityEngine;
using UnityEngine.Events;

public class GuardTagger : MonoBehaviour
{
    public string playerTag = "Player";
    public UnityEvent onTagged;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag)) onTagged?.Invoke();
    }
}
