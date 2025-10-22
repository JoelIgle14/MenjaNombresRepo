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

    private Animator animator;

    private string idleAnimation;
    private string appearanceAnimation;
    private string hoverAnimation;
    private string rejectAnimation;


    void Start()
    {
        animator = GetComponent<Animator>();

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
                if (animator != null && !string.IsNullOrEmpty(appearanceAnimation))
                    animator.Play(appearanceAnimation);


                MoveTowards(targetPos);
                if (HasReached(targetPos))
                {
                    currentState = State.Waiting;
                    OnReachedTop?.Invoke();

                    // Cambiar a idle cuando llega arriba
                    if (animator != null && !string.IsNullOrEmpty(idleAnimation))
                        animator.Play(idleAnimation);
                }
                break;

            case State.Waiting:
                // No hace nada más, sólo mantiene idle
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

    public void SetAnimationNames(string idle, string appearance)
    {
        idleAnimation = idle;
        appearanceAnimation = appearance;
    }

    public void SetHoverAnimation(string hover)
    {
        hoverAnimation = hover;
    }

    public void SetRejectAnimation(string reject)
    {
        rejectAnimation = reject;
    }

    public void PlayAnimation(string animationName)
    {
        if (animator != null && !string.IsNullOrEmpty(animationName))
            animator.Play(animationName);
    }


    public void StartFalling()
    {
        currentState = State.Falling;
        if (animator != null && !string.IsNullOrEmpty(hoverAnimation))
            animator.Play(hoverAnimation);
    }


    void MoveTowards(Vector3 target)
    {
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
    }

    bool HasReached(Vector3 target)
    {
        return Vector3.Distance(transform.position, target) < 0.01f;
    }

    public void InitializeRising(Transform assignedTarget)
    {
        // Asignar la línea
        targetLine = assignedTarget;

        // Inicializar la posición de inicio
        startPos = transform.position;
        startPos.y = startY;
        transform.position = startPos;

        // Calcular la posición objetivo
        if (targetLine != null)
            targetPos = new Vector3(startPos.x, targetLine.position.y, startPos.z);
        else
            targetPos = new Vector3(startPos.x, startY + 5f, startPos.z);
    }

}
