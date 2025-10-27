using UnityEngine;
using System.Collections;

public class ScaleAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    private Vector3 baseScale;

    private Coroutine currentRoutine;
    private void OnEnable()
    {
        baseScale = transform.localScale;
    }
    public void PlayScale(float strength, float duration)
    {
        // Restart if an animation is already playing
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(AnimateScale(strength, duration));
    }

    private IEnumerator AnimateScale(float strength, float duration)
    {
        float time = 0f;
        while (time < duration)
        {
            float normalizedTime = time / duration;
            float curveValue = scaleCurve.Evaluate(normalizedTime);

            // Apply the curve with strength
            transform.localScale = baseScale * (1f + curveValue * strength);

            time += Time.deltaTime;
            yield return null;
        }

        // Reset to base scale at the end
        transform.localScale = baseScale;
        currentRoutine = null;
    }

    private void Reset()
    {
        baseScale = transform.localScale;
    }
}
