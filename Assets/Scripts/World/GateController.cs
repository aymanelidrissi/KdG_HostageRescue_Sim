using UnityEngine;

public class GateController : MonoBehaviour
{
    public float openHeight = 2f;
    public float speed = 2f;

    Vector3 closedPos;
    Vector3 openPos;
    Vector3 targetPos;
    bool isOpen;

    void Awake()
    {
        closedPos = transform.position;
        openPos = closedPos + Vector3.up * openHeight;
        targetPos = closedPos;
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
    }

    public void Toggle()
    {
        isOpen = !isOpen;
        targetPos = isOpen ? openPos : closedPos;
    }

    public void Open() { isOpen = true; targetPos = openPos; }
    public void Close() { isOpen = false; targetPos = closedPos; }
}
