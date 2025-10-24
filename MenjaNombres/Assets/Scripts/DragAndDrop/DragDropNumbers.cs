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
    void Start()
    {
        col = GetComponent<Collider2D>();
    }
    private void OnMouseDown()
    {
        transform.position = GetMousePositionInWorldSpace();
        transform.position += new Vector3(0,0,-3);
    }
    private void OnMouseDrag()
    {
        transform.position = GetMousePositionInWorldSpace();
        transform.position += new Vector3(0, 0, -3);
    }
    private void OnMouseUp()
    {
        col.enabled = false;
        Collider2D hitCollider = Physics2D.OverlapPoint(transform.position);
        col.enabled = true;

        if (hitCollider != null)
        {
            // Si se suelta sobre una zona válida de números
            if (hitCollider.TryGetComponent(out NumberDropArea numberDropArea))
            {
                numberDropArea.OnNumberDrop(this, hitCollider.transform);
            }
            // Si se suelta sobre la papelera
            else if (hitCollider.TryGetComponent(out TrashCanArea trashCan))
            {
                trashCan.OnItemDropped(this);
                return; // se destruye, no necesitamos moverlo
            }
            else
            {
                transform.position = Holder.transform.position;
            }
        }
        else
        {
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