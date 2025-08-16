using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager I;
    public enum State { Playing, Win, Lose }
    public State state = State.Playing;

    public float elapsed;
    public UnityEvent onWin;
    public UnityEvent onLose;

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (state == State.Playing) elapsed += Time.deltaTime;
    }

    public void Win()
    {
        if (state != State.Playing) return;
        state = State.Win;
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
}
