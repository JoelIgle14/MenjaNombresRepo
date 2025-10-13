using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class Monster : MonoBehaviour
{
    [Header("Visual References")]
    public GameObject speechBubblePrefab;
    public Transform speechBubbleSpawnPoint;
    private TMP_Text operationText;

    [Header("Drop Area")]
    public Transform dropAreaSpawnPoint;
    public GameObject dropAreaPrefab;

    [Header("Order Data")]
    [HideInInspector] public string operation;
    [HideInInspector] public int requiredResult;
    [HideInInspector] public int pointValue;
    [HideInInspector] public GameManager.OperationType orderType;

    [Header("Sequential Order Tracking")]
    private List<int> sequentialValues = new List<int>();
    private int currentSequenceIndex = 0;

    [Header("Timer")]
    public float orderTime = 10f;
    private float timer;
    public Image timerBar;
    public Color timerStartColor = Color.green;
    public Color timerEndColor = Color.red;

    private GameObject speechBubbleInstance;
    private NumberDropArea dropArea;
    private bool orderCompleted = false;

    public void Initialize(string op, int result, int points, GameManager.OperationType type)
    {
        operation = op;
        requiredResult = result;
        pointValue = points;
        orderType = type;
        timer = orderTime;

        // Parse sequential order if needed
        if (orderType == GameManager.OperationType.SequentialCustomer)
        {
            ParseSequentialOrder(op);
        }

        // Spawn speech bubble
        if (speechBubblePrefab != null && speechBubbleSpawnPoint != null)
        {
            speechBubbleInstance = Instantiate(speechBubblePrefab, new Vector3(speechBubbleSpawnPoint.position.x, GetComponent<RisingMonster>().targetPos.y + 4, 0), Quaternion.identity);
            operationText = speechBubbleInstance.GetComponentInChildren<TMP_Text>();
            TMP_Text bubbleText = speechBubbleInstance.GetComponentInChildren<TMP_Text>();
            if (bubbleText != null)
            {
                bubbleText.text = operation;
            }
        }
        else if (operationText != null)
        {
            operationText.text = operation;
        }

        // Spawn drop area
        if (dropAreaPrefab != null && dropAreaSpawnPoint != null)
        {
            GameObject dropAreaObj = Instantiate(dropAreaPrefab, new Vector3(dropAreaSpawnPoint.position.x, GetComponent<RisingMonster>().targetPos.y + 4, dropAreaSpawnPoint.position.z), Quaternion.identity);
            dropArea = dropAreaObj.GetComponent<NumberDropArea>();


            if (dropArea != null)
            {
                dropArea.OnNumberDropped += OnNumberReceived;
            }
        }
        Destroy(speechBubbleInstance, orderTime);
        Destroy(dropArea.gameObject, orderTime);
    }

    void Update()
    {
        if (orderCompleted) return;

        // Update timer
        timer -= Time.deltaTime;
        UpdateTimerVisual();

        if (timer <= 0)
        {
            OnOrderFailed();
        }
    }

    void UpdateTimerVisual()
    {
        float timePercent = timer / orderTime;
        timerBar.fillAmount = timePercent;
        if (timerBar != null)
        {
            timerBar.color = Color.Lerp(timerEndColor, timerStartColor, timePercent);

            // Scale effect when time is running low
            if (timePercent < 0.5f)
            {
                float scaleAmount = 1f + Mathf.Sin(Time.time * 10f) * 0.1f;
                timerBar.transform.localScale = new Vector3(scaleAmount, scaleAmount, 1f);
            }
        }
    }

    void ParseSequentialOrder(string op)
    {
        // Parse "1-2-3" format
        string[] parts = op.Split('-');
        sequentialValues.Clear();

        foreach (string part in parts)
        {
            if (int.TryParse(part, out int value))
            {
                sequentialValues.Add(value);
            }
        }
    }

    public void OnNumberReceived(int value)
    {
        if (orderCompleted) return;

        bool isCorrect = false;

        switch (orderType)
        {
            case GameManager.OperationType.BasicClient:
                isCorrect = (value == requiredResult);
                break;

            case GameManager.OperationType.SophisticatedCustomer:
                isCorrect = (value == requiredResult);
                break;

            case GameManager.OperationType.SequentialCustomer:
                if (currentSequenceIndex < sequentialValues.Count)
                {
                    if (value == sequentialValues[currentSequenceIndex])
                    {
                        currentSequenceIndex++;

                        // Update display to show progress
                        if (operationText != null)
                        {
                            operationText.text = $"{currentSequenceIndex}/{sequentialValues.Count}";
                        }

                        // Check if sequence complete
                        if (currentSequenceIndex >= sequentialValues.Count)
                        {
                            isCorrect = true;
                        }
                        else
                        {
                            // Play positive feedback but don't complete order yet
                            PlayPartialSuccessAnimation();
                            return;
                        }
                    }
                    else
                    {
                        // Wrong number in sequence - reset
                        currentSequenceIndex = 0;
                        PlayFailAnimation();
                        return;
                    }
                }
                break;

            case GameManager.OperationType.PickyCustomer:
                bool isEven = (value % 2 == 0);
                bool needsEven = operation.Contains("Even");
                isCorrect = (isEven == needsEven);
                break;

            case GameManager.OperationType.IntellectualCustomer:
                // Extract range from operation string
                isCorrect = CheckIntellectualOrder(value);
                break;

            case GameManager.OperationType.MultiplierCustomer:
                isCorrect = (value == requiredResult);
                break;
        }

        if (isCorrect)
        {
            OnOrderCompleted();
        }
        else
        {
            OnWrongNumber();
        }
    }

    bool CheckIntellectualOrder(int value)
    {
        // Parse "Find even between 3 and 9"
        bool needsEven = operation.Contains("even");
        bool isEven = (value % 2 == 0);

        if (isEven != needsEven)
            return false;

        // Extract range (simple parsing)
        string[] words = operation.Split(' ');
        for (int i = 0; i < words.Length - 2; i++)
        {
            if (words[i] == "between" && int.TryParse(words[i + 1], out int min))
            {
                if (int.TryParse(words[i + 3], out int max))
                {
                    return value >= min && value <= max;
                }
            }
        }

        return value == requiredResult;
    }

    void OnOrderCompleted()
    {
        orderCompleted = true;

        // Calculate points based on time remaining
        float timePercent = timer / orderTime;
        int finalPoints = pointValue;

        if (timePercent < 0.25f)
        {
            finalPoints = Mathf.RoundToInt(pointValue * 0.5f);
        }
        else if (timePercent < 0.5f)
        {
            finalPoints = Mathf.RoundToInt(pointValue * 0.75f);
        }

        GameManager.Instance.OnMonsterServed(finalPoints);

        PlaySuccessAnimation();

        dropArea.ClearNumber();

        // Destroy after animation
        Destroy(gameObject, 0.5f);
    }

    void OnWrongNumber()
    {
        // Reduce timer as penalty
        timer -= 2f;
        dropArea.ClearNumber();
        PlayFailAnimation();
    }

    void OnOrderFailed()
    {
        if (orderCompleted) return;

        orderCompleted = true;
        GameManager.Instance.OnMonsterFailed();

        PlayLeaveAnimation();

        // Destroy immediately
        Destroy(gameObject, 0.3f);
        dropArea.ClearNumber();
    }

    void PlaySuccessAnimation()
    {
        Debug.Log($"{gameObject.name} - Order completed! Happy animation");
        // Add your success animation/particles here
    }

    void PlayFailAnimation()
    {
        Debug.Log($"{gameObject.name} - Wrong number! Angry animation");
        // Add your fail animation here
    }

    void PlayPartialSuccessAnimation()
    {
        Debug.Log($"{gameObject.name} - Correct sequential number!");
        // Add feedback for correct sequential number
    }

    void PlayLeaveAnimation()
    {
        Debug.Log($"{gameObject.name} - Time's up! Leaving...");
        // Add your leave animation here
    }

    void OnDestroy()
    {
        if (dropArea != null)
        {
            dropArea.OnNumberDropped -= OnNumberReceived;
        }
    }
}