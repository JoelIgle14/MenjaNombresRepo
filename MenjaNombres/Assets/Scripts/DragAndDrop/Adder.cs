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
    [SerializeField] private string combineAnimationName; // se escribe desde el inspector
    [SerializeField] private string cookAnimationName;    // se escribe desde el inspector
    [SerializeField] private string idleAnimationName = "Idle"; // volver al idle opcional

    private bool resultSpawned = false;

    public void Add()
    {
        if (a1.CurrentNum == 0 || a2.CurrentNum == 0)
        {
            Debug.Log("Need 1 more number");
            return;
        }

        int result = add ? a1.CurrentNum + a2.CurrentNum : Mathf.Abs(a1.CurrentNum - a2.CurrentNum);

        // limpiar entradas
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
            // Animación de combinar (una vez)
            if (!string.IsNullOrEmpty(combineAnimationName))
            {
                animator.Play(combineAnimationName);
                yield return new WaitForSeconds(GetAnimationLength(combineAnimationName));
            }

            //  Empieza la animación de cocinar en bucle
            if (!string.IsNullOrEmpty(cookAnimationName))
            {
                animator.Play(cookAnimationName);

                // Iniciamos una corrutina que se repetirá hasta que el número aparezca
                StartCoroutine(LoopCookAnimationUntilResult());
            }
        }

        // Esperar un poco y luego mostrar el resultado en la cinta
        yield return new WaitForSeconds(2f); // puedes ajustar el tiempo de "cocción"
        belt.QueueNumberSpawn(result);
        resultSpawned = true;

        //  Esperar un poco más antes de volver a Idle
        yield return new WaitForSeconds(0.5f);
        if (!string.IsNullOrEmpty(idleAnimationName))
            animator.Play(idleAnimationName);

        Debug.Log($"Adder queued result {result} after cooking animation");
    }

    private IEnumerator LoopCookAnimationUntilResult()
    {
        float cookLength = GetAnimationLength(cookAnimationName);

        // se repite hasta que el número haya aparecido
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

        Debug.LogWarning($"No se encontró la animación {animName} en el Animator");
        return 0f;
    }
}
