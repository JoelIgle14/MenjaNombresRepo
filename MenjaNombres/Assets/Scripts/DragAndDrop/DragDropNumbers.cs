using UnityEngine;
using TMPro;

public class DragDropNumbers : MonoBehaviour
{
    public GameObject Holder;
    public int value;
    [SerializeField]
    private TMP_Text text;

    private Collider2D col;
    private Vector3 startDragPosition;
    public Vector3 originalMachinePosition; 

    public bool lockedInMachine = false;
    public bool isInMachine = false;

    PlAud aud;
    ScaleAnimator scaleAnimator;

    void Start()
    {
        scaleAnimator = GetComponent<ScaleAnimator>();
        col = GetComponent<Collider2D>();
        aud = GetComponent<PlAud>();
    }

    private void OnMouseDown()
    {
        // Si ya está dentro de la máquina, no lo escalamos ni lo movemos
        if (isInMachine)
            return;

        scaleAnimator.PlayScale(0.3f, 0.2f);
        aud.PlayAud();

        startDragPosition = transform.position;
        transform.position = GetMousePositionInWorldSpace() + new Vector3(0, 0, -3);
    }


    private void OnMouseDrag()
    {
        transform.position = GetMousePositionInWorldSpace() + new Vector3(0, 0, -3);
    }

    private void OnMouseUp()
    {
        aud.PlayAud();
        col.enabled = false;
        Collider2D hitCollider = Physics2D.OverlapPoint(transform.position);
        col.enabled = true;

        if (hitCollider != null)
        {
            // Si se suelta sobre una zona válida de números
            if (hitCollider.TryGetComponent(out NumberDropArea numberDropArea) && !isInMachine)
            {
                numberDropArea.OnNumberDrop(this, hitCollider.transform);
                isInMachine = true;
                originalMachinePosition = transform.position; // Guardamos su nueva posición
                return;
            }
            // Si cae en la papelera
            else if (hitCollider.TryGetComponent(out TrashCanArea trashCan))
            {
                trashCan.OnItemDropped(this);
                return;
            }
        }

        //  Reposicionamiento automático 
        if (isInMachine)
        {
            // Ya pertenece a la máquina  vuelve a su sitio exacto
            transform.position = originalMachinePosition;
        }
        else
        {
            // No cayó en una drop area  volver al holder
            if (Holder != null)
                transform.position = Holder.transform.position;
        }
    }

    public Vector3 GetMousePositionInWorldSpace()
    {
        Vector3 p = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        p.z = 0f;
        return p;
    }

    public void UpdateVisual()
    {
        text.text = value.ToString();
    }
}
