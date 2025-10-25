using UnityEngine;
using TMPro;
using System.Diagnostics;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;

public class SpecialBox : MonoBehaviour
{
    public GameManager.BoxEffectType effectType;
    public int value = -1;
    public GameObject attachedNumber; // el número que fue absorbido
    private TMP_Text text;

    private Collider2D col;
    private Vector3 startDragPosition;
    private Vector3 originalPosition;
    private bool isDragging = false;

    void Start()
    {
        col = GetComponent<Collider2D>();
        text = GetComponentInChildren<TMP_Text>();
        UpdateVisual();
    }

    private void OnMouseDown()
    {
        startDragPosition = transform.position;
        isDragging = true;
    }

    private void OnMouseDrag()
    {
        if (!isDragging) return;
        transform.position = GetMousePositionInWorldSpace() + new Vector3(0, 0, -3);
    }

    private void OnMouseUp()
    {
        isDragging = false;
        col.enabled = false;
        Collider2D hit = Physics2D.OverlapPoint(transform.position);
        col.enabled = true;

        if (hit != null)
        {
            // Si se suelta sobre un número: absorberlo
            if (hit.TryGetComponent(out DragDropNumbers number))
            {
                AbsorbNumber(number);
                return;
            }

            // Si se suelta sobre un monstruo (NumberDropArea)
            if (hit.TryGetComponent(out NumberDropArea numberDrop))
            {
                // Si no tiene número absorbido, no hace nada
                if (value != -1)
                {
                    numberDrop.OnNumberDropBox(this, hit.transform);
                    GameManager.Instance.ActivateBoxEffect(effectType);

                }
                else
                {
                    Debug.Log("[SpecialBox] No tiene número todavía.");
                }
                return;
            }
        }

        // Si no colisiona con nada, vuelve a su posición original
        transform.position = startDragPosition;
    }

    void AbsorbNumber(DragDropNumbers number)
    {
        if (value != -1)
        {
            Debug.Log("[SpecialBox] Ya tiene un número asignado.");
            return;
        }

        value = number.value;
        attachedNumber = number.gameObject;

        // Guardar referencia al holder del número
        Transform targetHolder = null;
        if (number.Holder != null)
        {
            targetHolder = number.Holder.transform;
        }

        Destroy(number.gameObject); // eliminamos el número original

        // Reparentar la caja al holder de ese número
        if (targetHolder != null)
        {
            transform.SetParent(targetHolder);
            transform.position = targetHolder.position;
            Debug.Log($"[SpecialBox] Reparentada a holder '{targetHolder.name}' para seguir la cinta correcta.");
        }

        UpdateVisual();
        Debug.Log($"[SpecialBox] Absorbió el número {value}");
    }


    public void UpdateVisual()
    {
        if (text != null)
        {
            text.text = (value == -1) ? "?" : value.ToString();
        }
    }

    Vector3 GetMousePositionInWorldSpace()
    {
        Vector3 p = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        p.z = 0f;
        return p;
    }
}
