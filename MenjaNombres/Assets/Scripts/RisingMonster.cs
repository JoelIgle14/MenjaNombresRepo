using UnityEngine;

public class RisingMonster : MonoBehaviour
{
    [Header("Referencias")]
    public Transform targetLine;   // referencia a la línea o punto hasta donde subirá

    [Header("Configuración (controlado por GameManager)")]
    [HideInInspector] public float startY = -10f;
    [HideInInspector] public float speed = 2f;
    [HideInInspector] public float stayTime = 3f;

    private float timer = 0f;

    private enum State { Rising, Waiting, Falling, Idle }
    private State currentState = State.Rising;

    private Vector3 startPos;
    public Vector3 targetPos;

    void Start()
    {
        // Guardar la posición inicial del objeto
        startPos = transform.position;
        startPos.y = startY;
        transform.position = startPos;

        // Si tiene línea asignada, usar su posición Y como destino
        if (targetLine != null)
        {
            targetPos = new Vector3(startPos.x, targetLine.position.y, startPos.z);
        }
        else
        {
            Debug.LogWarning("No se asignó ninguna 'targetLine'. Usa un objeto vacío como referencia en el Inspector.");
            targetPos = new Vector3(startPos.x, startY + 5f, startPos.z); // fallback
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
                    timer = 0f;
                }
                break;

            case State.Waiting:
                timer += Time.deltaTime;
                if (timer >= stayTime)
                {
                    currentState = State.Falling;
                }
                break;

            case State.Falling:
                MoveTowards(startPos);
                if (HasReached(startPos))
                {
                    currentState = State.Idle;
                    Destroy(gameObject); // destruye el monstruo cuando termina
                }
                break;




            case State.Idle:
                // El objeto ya terminó su ciclo
                break;
        }
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
