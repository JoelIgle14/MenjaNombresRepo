using UnityEngine;
using System;

public class NumberDropArea : MonoBehaviour
{
    public int CurrentNum = 0;
    [HideInInspector] public DragDropNumbers num;
    public bool IsAdder = false;

    // Event for when a number is dropped (for Monster script)
    public event Action<int> OnNumberDropped;

    private Adder add;

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

        // Trigger event for Monster
        OnNumberDropped?.Invoke(CurrentNum);

        if (IsAdder)
        {
            print("adding");
            add.Add();
        }
    }

    // Method to clear the current number
    public void ClearNumber()
    {
        if (num != null)
        {
            Destroy(num.gameObject);
            num = null;
            CurrentNum = 0;
        }
    }
}