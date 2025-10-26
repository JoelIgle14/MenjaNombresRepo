using UnityEngine;

public class PlAud : MonoBehaviour
{
    [SerializeField]
    private AudioClip[] clips;
    [SerializeField]
    private AudioSource source;

    public void PlayAud()
    {
        source.clip = clips[Random.RandomRange(0, clips.Length)];
        source.Play();
    }
}
