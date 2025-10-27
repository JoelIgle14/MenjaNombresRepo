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
    private Vector3 originalMachinePosition; 
    private bool isInMachine = false;        

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
        scaleAnimator.PlayScale(0.3f, 0.2f);
        aud.PlayAud();
        // Guardamos la posición al empezar a arrastrar
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
            if (hitCollider.TryGetComponent(out NumberDropArea numberDropArea))
            {
                numberDropArea.OnNumberDrop(this, hitCollider.transform);
                isInMachine = true;
                originalMachinePosition = transform.position; // guardamos su nueva posición
            }
            // Si se suelta sobre la papelera
            else if (hitCollider.TryGetComponent(out TrashCanArea trashCan))
            {
                trashCan.OnItemDropped(this);
                return; // se destruye, no necesitamos moverlo
            }
            else
            {
                // Si estaba en la máquina, vuelve a su posición anterior en la máquina
                if (isInMachine)
                {
                    transform.position = originalMachinePosition;
                }
                else
                {
                    // Si estaba en el holder, vuelve al holder
                    transform.position = Holder.transform.position;
                }
            }
        }
        else
        {
            // Igual: si estaba en máquina, vuelve ahí; si no, al holder
            if (isInMachine)
            {
                transform.position = originalMachinePosition;
            }
            else
            {
                transform.position = Holder.transform.position;
            }
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
