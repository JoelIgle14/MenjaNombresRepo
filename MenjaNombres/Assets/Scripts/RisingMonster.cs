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

    public enum State { Rising, Waiting, Falling, Idle }
    public State currentState = State.Rising;

    private Vector3 startPos;
    public Vector3 targetPos;
    private Vector3 topPos; //  guardamos la posición exacta al llegar arriba

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
                    topPos = transform.position; //  guardamos la posición exacta arriba
                    OnReachedTop?.Invoke();

                    if (animator != null && !string.IsNullOrEmpty(idleAnimation))
                        animator.Play(idleAnimation);
                }
                break;

            case State.Waiting:
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
        {
            animator.Play(animationName);

            // Cuando termina hover o reject, restablecer posición exacta
            if (animationName == idleAnimation ||
                animationName == hoverAnimation ||
                animationName == rejectAnimation)
            {
                // Esperar un pequeño frame para que el Animator actualice
                StartCoroutine(ResetToTopPositionNextFrame());
            }
        }
    }

    private System.Collections.IEnumerator ResetToTopPositionNextFrame()
    {
        yield return null; // espera 1 frame
        transform.position = topPos;
    }

    public void StartFalling()
    {
        currentState = State.Falling;

        // Puedes poner animación de caer si la tienes
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
        targetLine = assignedTarget;

        startPos = transform.position;
        startPos.y = startY;
        transform.position = startPos;

        if (targetLine != null)
            targetPos = new Vector3(startPos.x, targetLine.position.y, startPos.z);
        else
            targetPos = new Vector3(startPos.x, startY + 5f, startPos.z);
    }
}
