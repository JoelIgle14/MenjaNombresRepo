using UnityEngine;
using TMPro;
using System.Collections;

public class Typingtexteffect : MonoBehaviour
{
    public TMP_Text textComponent;
    public float letterDelay = 0.05f;
    public AudioClip typingSound;

    private AudioSource audioSource;
    private Coroutine typingCoroutine;
    private string currentFullText = "";
    private bool isTyping = false;

    void Awake()
    {
        if (textComponent == null)
            textComponent = GetComponent<TMP_Text>();

        audioSource = GetComponent<AudioSource>();
    }
    public void TypeText(string text)
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        currentFullText = text;
        typingCoroutine = StartCoroutine(TypeTextCoroutine(text));
    }
    public void SkipTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (textComponent != null)
            textComponent.text = currentFullText;

        isTyping = false;
    }
    public void ClearText()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (textComponent != null)
            textComponent.text = "";

        currentFullText = "";
        isTyping = false;
    }

    IEnumerator TypeTextCoroutine(string text)
    {
        isTyping = true;
        textComponent.text = "";

        foreach (char letter in text)
        {
            textComponent.text += letter;

            // Play typing sound (optional)
            if (audioSource != null && typingSound != null && letter != ' ')
            {
                audioSource.PlayOneShot(typingSound, 0.3f);
            }

            yield return new WaitForSecondsRealtime(letterDelay);
        }

        isTyping = false;
        typingCoroutine = null;
    }

    public bool IsTyping()
    {
        return isTyping;
    }

    public void SetLetterDelay(float delay)
    {
        letterDelay = delay;
    }
}