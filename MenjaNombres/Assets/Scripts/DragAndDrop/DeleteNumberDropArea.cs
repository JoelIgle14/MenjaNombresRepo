using UnityEngine;

public class DeleteNumberDropArea : MonoBehaviour
{
    public void OnNumberDrop(DragDropNumbers number)
    {
        Destroy(number.gameObject);
        //Instantiate(objectToSpawn, transform.position, transform.rotation);
        Debug.Log("Numero esborrat");
    }
}
