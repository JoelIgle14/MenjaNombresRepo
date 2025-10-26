using System.Diagnostics;
using UnityEngine;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;

public class ConveyorBelt : MonoBehaviour
{
    public float moveSpeed = 1f;
    public bool moveRight = true;
    public bool canSpawnNumber = true;
    public const int numberHoldersCount = 4;
    public Transform InitialPosition;
    public Transform[] numberHolders;
    public GameObject numberPrefab;
    private Vector3[] startPositions;
    public bool hasPendingNumber = false;
    private int pendingNumber = 0;

    public ParticleSystem particles;

    private bool hasPendingBox = false;
    private GameObject pendingBoxPrefab;
    private GameManager.BoxEffectType pendingBoxEffect;


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
    void FixedUpdate()
    {
        MoveHolders();
    }


    public void QueueBoxSpawn(GameObject prefab, GameManager.BoxEffectType effect)
    {
        pendingBoxPrefab = prefab;
        pendingBoxEffect = effect;
        hasPendingBox = true;
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
                {
                    // Check if Adder requested a specific number
                    if (hasPendingBox)
                    {
                        SpawnSpecialBox(holder, pendingBoxPrefab, pendingBoxEffect);
                        hasPendingBox = false;
                    }
                    else if (hasPendingNumber)
                    {
                        SpawnSpecificNumber(holder, pendingNumber);
                        hasPendingNumber = false;
                    }
                    else
                    {
                        SpawnNumber(holder);
                    }

                }
            }
        }
    }

    void SpawnSpecialBox(Transform holder, GameObject prefab, GameManager.BoxEffectType effect)
    {
        foreach (Transform child in holder)
            Destroy(child.gameObject);

        GameObject box = Instantiate(prefab, holder.position, Quaternion.identity, holder);

        var boxScript = box.GetComponent<SpecialBox>();
        if (boxScript != null)
        {
            boxScript.effectType = effect;
        }

        Debug.Log($"[ConveyorBelt] Spawned special box with effect: {effect}");
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
    void SpawnSpecificNumber(Transform holder, int value)
    {
        if(particles != null)
        {
            particles.Play();
        }
        foreach (Transform numero in holder)
            Destroy(numero.gameObject);
        GameObject num = Instantiate(numberPrefab, holder.position, Quaternion.identity, holder);
        DragDropNumbers drag = num.GetComponent<DragDropNumbers>();
        if (drag != null)
        {
            drag.value = value;
            drag.UpdateVisual();
            drag.Holder = holder.gameObject;
        }
    }
    int GenerateBiasedRandomValue()
    {
        float r = Random.value;
        if (r < 0.3f) return Random.Range(1, 4);
        if (r < 0.7f) return Random.Range(4, 7);
        return Random.Range(7, 10);
    }
    public void QueueNumberSpawn(int value)
    {
        pendingNumber = value;
        hasPendingNumber = true;
    }

}