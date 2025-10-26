using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CameraEffects : MonoBehaviour
{
    [Header("Shake Settings")]
    public float shakeDuration = 0.2f;
    public AnimationCurve shakeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    [Header("Damage Vignette")]
    public Image vignetteImage;
    public AnimationCurve vignetteCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float vignetteDuration = 1.5f;

    private Vector3 _originalPosition;

    void Awake()
    {
        _originalPosition = transform.localPosition;
        if (vignetteImage)
            vignetteImage.color = new Color(1, 0, 0, 0); // start transparent red
    }

    public void CameraShake(float force, bool damage)
    {
        StopAllCoroutines();
        StartCoroutine(Shake(force));

        if (damage && vignetteImage)
            StartCoroutine(DamageVignette());
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.X))
        {
            CameraShake(0.1f,true);
        }
    }

    public void Damage()
    {
        if (vignetteImage)
            StartCoroutine(DamageVignette());
    }

    private IEnumerator Shake(float force)
    {
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float strength = shakeCurve.Evaluate(elapsed / shakeDuration) * force;
            transform.localPosition = _originalPosition + (Vector3)Random.insideUnitCircle * strength;
            yield return null;
        }
        transform.localPosition = _originalPosition;
    }

    private IEnumerator DamageVignette()
    {
        float elapsed = 0f;

        // Fade in
        while (elapsed < vignetteDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = vignetteCurve.Evaluate(elapsed / (vignetteDuration / 2f));
            vignetteImage.color = new Color(1, 0, 0, t);
            yield return null;
        }

        // Fade out
        elapsed = 0f;
        while (elapsed < vignetteDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = vignetteCurve.Evaluate(1 - (elapsed / (vignetteDuration / 2f)));
            vignetteImage.color = new Color(1, 0, 0, t);
            yield return null;
        }

        vignetteImage.color = new Color(1, 0, 0, 0);
    }
}
