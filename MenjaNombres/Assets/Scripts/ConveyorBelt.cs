using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

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

    public bool isTutorialMode = false;
    private Queue<int> queuedTutorialNumbers = new Queue<int>();

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

    public void PlaySmokeParticles()
    {
        if (particles != null)
        {
            particles.Play();
            print("palyer");
        }
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
                    // Tutorial override
                    if (isTutorialMode && queuedTutorialNumbers.Count > 0)
                    {
                        int value = queuedTutorialNumbers.Dequeue();
                        SpawnSpecificNumber(holder, value);

                        // Optionally, keep filling from last value if out
                        if (queuedTutorialNumbers.Count == 0)
                        {
                            queuedTutorialNumbers.Enqueue(value);
                        }
                    }
                    else if (hasPendingBox)
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
            if (numero.TryGetComponent<DragDropNumbers>(out var existingDrag))
            {
                if (!existingDrag.lockedInMachine)
                    Destroy(numero.gameObject);
            }
            else
            {
                Destroy(numero.gameObject);
            }

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
        foreach (Transform numero in holder)
            Destroy(numero.gameObject);
        GameObject num = Instantiate(numberPrefab, holder.position, Quaternion.identity, holder);
        DragDropNumbers drag = num.GetComponent<DragDropNumbers>();

        if(!isTutorialMode)
            num.GetComponentInChildren<TMP_Text>().color = Color.green * Color.gray;

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

    public void SetTutorialNumbers(IEnumerable<int> nums)
    {
        queuedTutorialNumbers.Clear();
        foreach (var n in nums)
            queuedTutorialNumbers.Enqueue(n);
    }

    public void ClearTutorialNumbers()
    {
        queuedTutorialNumbers.Clear();
    }

    public void QueueNumberSpawn(int value)
    {
        pendingNumber = value;
        hasPendingNumber = true;
    }

}