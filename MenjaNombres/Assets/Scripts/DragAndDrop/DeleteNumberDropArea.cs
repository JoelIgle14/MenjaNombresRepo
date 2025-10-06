using UnityEngine;

public class DeleteNumberDropArea : MonoBehaviour, INumberDropArea
{
    // Por si tiene que aparecer algo al tirar un numero
    //[SerializeField] private GameObject objectToSpawn;

    public void OnNumberDrop(DragDropNumbers number)
    {
        Destroy(number.gameObject);
        //Instantiate(objectToSpawn, transform.position, transform.rotation);
        Debug.Log("Numero esborrat");
    }
}
