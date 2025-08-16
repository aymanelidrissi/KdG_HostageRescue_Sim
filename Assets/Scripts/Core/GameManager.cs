using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager I;
    public enum State { Playing, Win, Lose }
    public State state = State.Playing;

    public float elapsed;
    public float bestTime = float.PositiveInfinity;

    public UnityEvent onWin;
    public UnityEvent onLose;

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
        bestTime = PlayerPrefs.GetFloat("bestTime", float.PositiveInfinity);
    }

    void Update()
    {
        if (state == State.Playing) elapsed += Time.deltaTime;
    }

    public void Win()
    {
        if (state != State.Playing) return;
        state = State.Win;
        if (elapsed < bestTime)
        {
            bestTime = elapsed;
            PlayerPrefs.SetFloat("bestTime", bestTime);
            PlayerPrefs.Save();
        }
        onWin?.Invoke();
    }

    public void Lose()
    {
        if (state != State.Playing) return;
        state = State.Lose;
        onLose?.Invoke();
    }

    public void ResetRun()
    {
        state = State.Playing;
        elapsed = 0f;
    }

    public void ClearBestTime()
    {
        bestTime = float.PositiveInfinity;
        PlayerPrefs.DeleteKey("bestTime");
    }
}
