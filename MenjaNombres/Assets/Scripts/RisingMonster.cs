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
                    // Esperamos un frame antes de destruir para que la animación termine
                    StartCoroutine(DestroyAfterReachBottom());
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
        if (animator == null || string.IsNullOrEmpty(animationName))
            return;

        //  No permitir hover si no está esperando arriba
        if (animationName == hoverAnimation && currentState != State.Waiting)
            return;

        animator.Play(animationName);

        //  Mantener la corrección de posición
        if (animationName == idleAnimation ||
            animationName == hoverAnimation ||
            animationName == rejectAnimation)
        {
            StartCoroutine(ResetToTopPositionNextFrame());
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

    public void PlayHappyAndDescend(string happyAnimation)
    {
        if (animator != null && !string.IsNullOrEmpty(happyAnimation))
            animator.Play(happyAnimation);

        currentState = State.Falling;
    }

    private System.Collections.IEnumerator DestroyAfterReachBottom()
    {
        yield return new WaitForSeconds(0.1f);
        Destroy(gameObject);
    }

}
