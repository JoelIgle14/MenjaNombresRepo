using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    public float moveSpeed = 1f;
    public bool moveRight = true;

    public bool canSpawnNumber = true; //For later use

    public const int numberHoldersCount = 4;

    public Transform InitialPosition;
    public Transform[] numberHolders;
    public GameObject numberPrefab;

    private Vector3[] startPositions;

    void Start()
    {
        startPositions = new Vector3[numberHolders.Length];

        for (int i = 0; i < numberHolders.Length; i++)
        {
            startPositions[i] = numberHolders[i].position;
            if (canSpawnNumber)
                SpawnNumber(numberHolders[i]);
        }
    }

    //fixed bc OnMouseDown() uses raycast under the hood
    void FixedUpdate()
    {
        MoveHolders();
    }

    void MoveHolders()
    {
        float direction = moveRight ? 1f : -1f;

        for (int i = 0; i < numberHolders.Length; i++)
        {
            Transform holder = numberHolders[i];
            holder.Translate(Vector3.right * direction * moveSpeed * Time.deltaTime);

            if (IsOutOfScreen(holder.position))
            {
                holder.position = InitialPosition.position;
                if (canSpawnNumber)
                    SpawnNumber(holder);
            }
        }
    }

    bool IsOutOfScreen(Vector3 position)
    {
        Vector3 screenPoint = Camera.main.WorldToViewportPoint(position);

        return screenPoint.x < -0.1f || screenPoint.x > 1.1f;
    }

    void SpawnNumber(Transform holder)
    {
        foreach (Transform numero in holder)
            Destroy(numero.gameObject);

        GameObject num = Instantiate(numberPrefab, holder.position, Quaternion.identity, holder);

        DragDropNumbers drag = num.GetComponent<DragDropNumbers>();
        if (drag != null)
        {
            drag.value = GenerateBiasedRandomValue();
            drag.UpdateVisual();
            drag.Holder = holder.gameObject;
        }
    }


    //queremos que los valores bajos seam más prominientes
    int GenerateBiasedRandomValue()
    {
        float r = Random.value;
        if (r < 0.3f) return Random.Range(1, 4);  
        if (r < 0.7f) return Random.Range(4, 7);  
        return Random.Range(7, 10);               
    }
}