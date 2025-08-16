using UnityEngine;
using TMPro;

public class HUDController : MonoBehaviour
{
    public TMP_Text timerText;
    public TMP_Text bestText;

    void Update()
    {
        if (!GameManager.I) return;

        if (timerText)
        {
            var t = GameManager.I.elapsed;
            timerText.text = t.ToString("F2") + " s";
        }

        if (bestText)
        {
            var b = GameManager.I.bestTime;
            bestText.text = float.IsPositiveInfinity(b) ? "--" : b.ToString("F2") + " s";
        }
    }
}
