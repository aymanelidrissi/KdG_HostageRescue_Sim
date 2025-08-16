using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class EndGameActions : MonoBehaviour
{
    public Behaviour[] disableOnEnd;
    public GameObject winObject;
    public GameObject loseObject;

    public void OnWin()
    {
        foreach (var b in disableOnEnd) if (b) b.enabled = false;
        if (winObject) winObject.SetActive(true);
    }

    public void OnLose()
    {
        foreach (var b in disableOnEnd) if (b) b.enabled = false;
        if (loseObject) loseObject.SetActive(true);
    }
}
