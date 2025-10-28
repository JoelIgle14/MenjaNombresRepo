using UnityEngine;
using System.Collections;

public class Adder : MonoBehaviour
{
    [SerializeField] private bool add = true;
    [SerializeField] private NumberDropArea a1;
    [SerializeField] private NumberDropArea a2;
    [SerializeField] private ConveyorBelt belt;
    [SerializeField] private GameManager gameManager;

    [Header("Animaciones")]
    [SerializeField] private Animator animator;
    [SerializeField] private string combineAnimationName;
    [SerializeField] private string cookAnimationName;
    [SerializeField] private string idleAnimationName = "Idle";

    private bool resultSpawned = false;
    PlAud aud;

    private void Start()
    {
        aud = GetComponent<PlAud>();
    }

    public void Add()
    {
        if (a1.CurrentNum == 0 || a2.CurrentNum == 0)
        {
            Debug.Log("Need 1 more number");
            return;
        }

        int result = add ? a1.CurrentNum + a2.CurrentNum : Mathf.Abs(a1.CurrentNum - a2.CurrentNum);

        // Notify tutorial system
        if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive())
        {
            TutorialManager.Instance.OnAdderUsed();
        }

        // Clean inputs
        Destroy(a1.num.gameObject);
        Destroy(a2.num.gameObject);
        a1.CurrentNum = 0;
        a2.CurrentNum = 0;
        a1.num = null;
        a2.num = null;

        resultSpawned = false;

        StartCoroutine(PlayAnimationsThenSpawn(result));
    }

    private IEnumerator PlayAnimationsThenSpawn(int result)
    {
        if (animator != null)
        {
            // Combine animation
            if (!string.IsNullOrEmpty(combineAnimationName))
            {
                animator.Play(combineAnimationName);
                yield return new WaitForSeconds(GetAnimationLength(combineAnimationName));
            }

            // Cooking animation loop
            if (!string.IsNullOrEmpty(cookAnimationName))
            {
                animator.Play(cookAnimationName);
                StartCoroutine(LoopCookAnimationUntilResult());
            }
        }

        aud.PlayAud();
        belt.PlaySmokeParticles();

        yield return new WaitForSeconds(1.1f);
        belt.QueueNumberSpawn(result);
        resultSpawned = true;

        yield return new WaitForSeconds(0.5f);
        if (!string.IsNullOrEmpty(idleAnimationName))
            animator.Play(idleAnimationName);

        Debug.Log($"Adder queued result {result} after cooking animation");
    }

    private IEnumerator LoopCookAnimationUntilResult()
    {
        float cookLength = GetAnimationLength(cookAnimationName);

        while (!resultSpawned)
        {
            animator.Play(cookAnimationName);
            yield return new WaitForSeconds(cookLength);
        }
    }

    private float GetAnimationLength(string animName)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
            return 0f;

        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == animName)
                return clip.length;
        }

        Debug.LogWarning($"No se encontr� la animaci�n {animName} en el Animator");
        return 0f;
    }
}