using UnityEngine;

public class TrashCanArea : MonoBehaviour
{
    public CameraEffects effects;
    // Puedes poner efectos visuales o sonidos si quieres cuando se destruye algo
    public void OnItemDropped(DragDropNumbers number)
    {
        effects.CameraShake(0.1f, false);
        this.GetComponent<PlAud>().PlayAud();
        Destroy(number.gameObject);
    }
}
