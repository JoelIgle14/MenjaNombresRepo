using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class PlateFollow : MonoBehaviour
{
    public float YOffset = 0;

    [SerializeField] private Transform Plate;

    // Update is called once per frame
    void FixedUpdate()
    {
        Plate.position = transform.position + new Vector3(0,YOffset,0);
    }
}
