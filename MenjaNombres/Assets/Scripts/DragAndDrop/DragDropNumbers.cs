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
    }
    private void OnMouseDrag()
    {
        transform.position = GetMousePositionInWorldSpace();
    }
    private void OnMouseUp()
    {
        col.enabled = false;
        Collider2D hitCollider = Physics2D.OverlapPoint(transform.position);
        col.enabled = true;
        if (hitCollider != null && hitCollider.TryGetComponent(out NumberDropArea numberDropArea))
        {
            numberDropArea.OnNumberDrop(this, hitCollider.transform);
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