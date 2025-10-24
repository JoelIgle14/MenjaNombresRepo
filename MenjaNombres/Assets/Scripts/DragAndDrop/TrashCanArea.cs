using UnityEngine;

public class TrashCanArea : MonoBehaviour
{
    // Puedes poner efectos visuales o sonidos si quieres cuando se destruye algo
    public void OnItemDropped(DragDropNumbers number)
    {
        Destroy(number.gameObject);
    }
}
