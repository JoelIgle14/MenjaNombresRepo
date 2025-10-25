using UnityEngine;
using TMPro;
using System.Diagnostics;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;

public class SpecialBox : MonoBehaviour
{
    public GameManager.BoxEffectType effectType;
    public int value = -1;
    public GameObject attachedNumber; // el n�mero que fue absorbido
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
            // Si se suelta sobre un n�mero: absorberlo
            if (hit.TryGetComponent(out DragDropNumbers number))
            {
                AbsorbNumber(number);
                return;
            }

            // Si se suelta sobre un monstruo (NumberDropArea)
            if (hit.TryGetComponent(out NumberDropArea numberDrop))
            {
                // Si no tiene n�mero absorbido, no hace nada
                if (value != -1)
                {
                    numberDrop.OnNumberDropBox(this, hit.transform);
                    GameManager.Instance.ActivateBoxEffect(effectType);

                }
                else
                {
                    Debug.Log("[SpecialBox] No tiene n�mero todav�a.");
                }
                return;
            }
        }

        // Si no colisiona con nada, vuelve a su posici�n original
        transform.position = startDragPosition;
    }

    void AbsorbNumber(DragDropNumbers number)
    {
        if (value != -1)
        {
            Debug.Log("[SpecialBox] Ya tiene un n�mero asignado.");
            return;
        }

        value = number.value;
        attachedNumber = number.gameObject;

        // Guardar referencia al holder del n�mero
        Transform targetHolder = null;
        if (number.Holder != null)
        {
            targetHolder = number.Holder.transform;
        }

        Destroy(number.gameObject); // eliminamos el n�mero original

        // Reparentar la caja al holder de ese n�mero
        if (targetHolder != null)
        {
            transform.SetParent(targetHolder);
            transform.position = targetHolder.position;
            Debug.Log($"[SpecialBox] Reparentada a holder '{targetHolder.name}' para seguir la cinta correcta.");
        }

        UpdateVisual();
        Debug.Log($"[SpecialBox] Absorbi� el n�mero {value}");
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
