using UnityEngine;

public class NumberDropArea : MonoBehaviour
{
    public int CurrentNum = 0;
    [HideInInspector]
    public DragDropNumbers num;

   public  bool IsAdder = false;
    Adder add;

    private void Start()
    {
        add = GetComponentInParent<Adder>();
        if (add != null)
        {
            IsAdder = true;
        }
    }

    public void OnNumberDrop(DragDropNumbers number, Transform transform)
    {
        number.transform.position = transform.position;
        number.transform.parent.transform.DetachChildren();
        num = number;
        CurrentNum = num.value;

        if(IsAdder)
        {
            print("adding");
            add.Add();
        }
    }
}