using UnityEngine;

public interface INumberDropArea
{
    void OnNumberDrop(DragDropNumbers number, Transform transform)
    {
        number.transform.position = transform.position;
        number.transform.parent.transform.DetachChildren();
    }

}
