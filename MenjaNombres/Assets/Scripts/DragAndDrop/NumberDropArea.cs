using UnityEngine;
using System;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;

public class NumberDropArea : MonoBehaviour
{
    public int CurrentNum = 0;
    [HideInInspector] public DragDropNumbers num;
    public bool IsAdder = false;

    // Event for when a number is dropped (for Monster script)
    public event Action<int> OnNumberDropped;

    private Adder add;

    // Guardamos el GameObject que está actualmente en la zona
    private GameObject currentNumberObject = null;

    private void Start()
    {
        add = GetComponentInParent<Adder>();
        if (add != null)
        {
            IsAdder = true;
        }
    }

    // Nota: renombrado 'transform' a 'dropPoint' para evitar shadowing
    public void OnNumberDrop(DragDropNumbers number, Transform dropPoint)
    {
        if(currentNumberObject != null){ return ; }
        // Mover el número a la posición de drop
        number.transform.position = dropPoint.position;


        // Posicionar en el punto correcto de la máquina
        number.transform.position = dropPoint.position;
        number.transform.SetParent(null); // separarlo visualmente

        // Bloquearlo para que la cinta no lo toque nunca más
        number.lockedInMachine = true;
        number.isInMachine = true;
        number.Holder = null;

        // Guardar la posición exacta para que no se mueva nunca más
        number.originalMachinePosition = dropPoint.position;


        // Si el número estaba en un holder como child, separamos hijos para evitar referencias rotas
        if (number.transform.parent != null)
        {
            number.transform.parent.DetachChildren();
        }

        // Guardamos referencias específicas
        num = number;
        currentNumberObject = number.gameObject;
        CurrentNum = num.value;

        // Trigger event for Monster
        OnNumberDropped?.Invoke(CurrentNum);

        if (IsAdder)
        {
            // Si esta zona es parte de un "Adder", avisamos
            add.Add();
        }
    }

    // Method to clear the current number (funciona tanto para DragDropNumbers como para SpecialBox)
    public void ClearNumber()
    {
        // Si hay un DragDropNumbers guardado, destruirlo y limpiar referencias
        if (num != null)
        {
            Destroy(num.gameObject);
            num = null;
        }
        else if (currentNumberObject != null)
        {
            // Si no hay `num` pero existe un objeto (por ejemplo una caja especial), destruirlo también
            Destroy(currentNumberObject);
        }

        currentNumberObject = null;
        CurrentNum = 0;
    }

    // Ahora OnNumberDropBox usa la misma lógica pero con SpecialBox
    public void OnNumberDropBox(SpecialBox box, Transform dropPoint)
    {
        // Si ya hay algo, limpiarlo
        if (currentNumberObject != null || num != null)
            ClearNumber();

        // Guardamos la caja como objeto actual
        currentNumberObject = box.gameObject;

        // Posicionarla en el punto de drop
        currentNumberObject.transform.position = dropPoint.position;

        // Actualizamos CurrentNum con el valor que tenga la caja
        CurrentNum = box.value;

        // Avisamos al suscriptor (Monster)
        OnNumberDropped?.Invoke(box.value);

        if (IsAdder)
        {
            add.Add();
        }
    }
}
