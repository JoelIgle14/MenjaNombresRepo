using UnityEngine;
using System;

public class RisingMonster : MonoBehaviour
{
    [Header("Referencias")]
    public Transform targetLine;

    [Header("Configuración (controlado por GameManager)")]
    [HideInInspector] public float startY = -10f;
    [HideInInspector] public float speed = 2f;
    [HideInInspector] public float stayTime = 3f;

    private float timer = 0f;

    private enum State { Rising, Waiting, Falling, Idle }
    private State currentState = State.Rising;

    private Vector3 startPos    ;
    public Vector3 targetPos;

    public event Action OnReachedTop;

    void Start()
    {
        startPos = transform.position;
        startPos.y = startY;
        transform.position = startPos;

        if (targetLine != null)
        {
            targetPos = new Vector3(startPos.x, targetLine.position.y, startPos.z);
        }
        else
        {
            Debug.LogWarning("No se asignó ninguna 'targetLine'. Usa un objeto vacío como referencia en el Inspector.");
            targetPos = new Vector3(startPos.x, startY + 5f, startPos.z);
        }
    }

    void Update()
    {
        switch (currentState)
        {
            case State.Rising:
                MoveTowards(targetPos);
                if (HasReached(targetPos))
                {
                    currentState = State.Waiting;
                    OnReachedTop?.Invoke(); // Avisar al Monster
                }
                break;

            case State.Waiting:
                // Aquí el monstruo se queda quieto hasta que Monster le diga que baje
                break;

            case State.Falling:
                MoveTowards(startPos);
                if (HasReached(startPos))
                {
                    currentState = State.Idle;
                    Destroy(gameObject);
                }
                break;

            case State.Idle:
                break;
        }
    }

    public void StartFalling()
    {
        currentState = State.Falling;
    }

    void MoveTowards(Vector3 target)
    {
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
    }

    bool HasReached(Vector3 target)
    {
        return Vector3.Distance(transform.position, target) < 0.01f;
    }
}
