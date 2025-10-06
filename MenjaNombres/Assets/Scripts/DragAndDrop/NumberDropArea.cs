using UnityEngine;

public class NumberDropArea : MonoBehaviour, INumberDropArea
{
    public void OnNumberDrop(DragDropNumbers number)
    {
        number.transform.position = transform.position;
        Debug.Log("Numero soltat aqui");
    
    }
}
