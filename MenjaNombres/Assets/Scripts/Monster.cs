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
    public float ResultPointOffset;
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
    private bool timerActive = false;
    private RisingMonster risingMonster;
    public float orderTime = 10f;
    private float timer;
    public Image timerBar;
    public GameObject timerSprite;
    public Color timerStartColor = Color.green;
    public Color timerEndColor = Color.red;

    [Header("Animation")]
    public string idleAnimationName;
    public string apperanceAnimationName;
    public string hoverAnimationName;
    public string rejectAnimationName;

    private GameObject speechBubbleInstance;
    private GameObject DropInstance;
    private NumberDropArea dropArea;
    private bool orderCompleted = false;

    GameObject Ui;

    public void Initialize(string op, int result, int points, GameManager.OperationType type)
    {
        operation = op;
        requiredResult = result;
        pointValue = points;
        orderType = type;
        timer = orderTime;

        risingMonster = GetComponent<RisingMonster>();
        if (risingMonster != null)
        {
            risingMonster.OnReachedTop += StartOrderTimer;

            // pasar los nombres de animación al RisingMonster
            risingMonster.SetAnimationNames(idleAnimationName, apperanceAnimationName);
            risingMonster.SetHoverAnimation(hoverAnimationName);

        }

        // Ocultar el timer visual al principio
        if (timerBar != null && timerSprite != null)
        {
            timerBar.gameObject.SetActive(false);
            timerSprite.SetActive(false);
        }
    }


    void Update()
    {
        if (orderCompleted || !timerActive) return;

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
                float scaleAmountSprite = timerSprite.transform.localScale.x + Mathf.Sin(Time.time * 10f) * 0.02f;
                timerSprite.transform.localScale = new Vector3(scaleAmountSprite, scaleAmountSprite, 1f);
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

    private void ShowOrderUI()
    {
        // Crear el globo de texto
        if (speechBubblePrefab != null && speechBubbleSpawnPoint != null)
        {
            speechBubbleInstance = Instantiate(  
                speechBubblePrefab,
                new Vector3(speechBubbleSpawnPoint.position.x, GetComponent<RisingMonster>().targetPos.y + speechBubbleSpawnPoint.position.y, 0),
                Quaternion.identity
            );

            Ui = speechBubbleInstance;

            operationText = speechBubbleInstance.GetComponentInChildren<TMP_Text>();
            TMP_Text bubbleText = speechBubbleInstance.GetComponentInChildren<TMP_Text>();
            if (bubbleText != null)
            {
                bubbleText.text = operation;
            }
            Destroy(speechBubbleInstance, orderTime);
        }

        // Crear el área de drop
        if (dropAreaPrefab != null && dropAreaSpawnPoint != null)
        {
            GameObject dropAreaObj = Instantiate(
                dropAreaPrefab,
                new Vector3(dropAreaSpawnPoint.position.x, GetComponent<RisingMonster>().targetPos.y + ResultPointOffset, dropAreaSpawnPoint.position.z),
                Quaternion.identity
            );
            DropInstance = dropAreaObj;

            dropArea = dropAreaObj.GetComponent<NumberDropArea>();
            if (dropArea != null)
            {
                dropArea.OnNumberDropped += OnNumberReceived;
            }
        }

        // Mostrar el timer visual
        if (timerBar != null && timerSprite != null)
        {
            timerSprite.SetActive(true);
            timerBar.gameObject.SetActive(true);
        }

        // Activar el temporizador
        timerActive = true;
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
                string opLower = operation.ToLower();
                bool needsEven = opLower.Contains("parells") && !opLower.Contains("imparells");

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
    string opLower = operation.ToLower();

    // Detectar explícitament si vol parell o imparell
    bool wantsOdd = opLower.Contains("imparell");
    bool wantsEven = opLower.Contains("parell") && !wantsOdd;

    bool isEven = (value % 2 == 0);

    // Si vol imparell i el valor és parell  incorrecte
    // Si vol parell i el valor és imparell  incorrecte
    if (wantsEven && !isEven) return false;
    if (wantsOdd && isEven) return false;

    // Extreure el rang ("entre X i Y")
    int min = int.MinValue;
    int max = int.MaxValue;

    string[] words = opLower.Split(' ');
    for (int i = 0; i < words.Length - 3; i++)
    {
        if (words[i] == "entre" && int.TryParse(words[i + 1], out min) && int.TryParse(words[i + 3], out max))
        {
            break;
        }
    }

    bool inRange = (value >= min && value <= max);


    return inRange;
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

        if(DropInstance != null)
            Destroy(DropInstance);

        // Destroy after animation
        Destroy(gameObject, 0.5f);
        if(Ui != null)Destroy(Ui);
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

        // decirle que empiece a bajar
        if (risingMonster != null)
        {
            risingMonster.StartFalling();
        }

        // eliminar UI visual del pedido y timer
        if (speechBubbleInstance != null) Destroy(speechBubbleInstance);
        if (dropArea != null && dropArea.gameObject != null) Destroy(dropArea.gameObject);
        if (timerBar != null) timerBar.gameObject.SetActive(false);
        if (timerSprite != null) timerSprite.gameObject.SetActive(false);

        Destroy(gameObject, 0.5f);
        if (Ui != null) Destroy(Ui);
        Destroy(gameObject, 1f);
    }

    void PlaySuccessAnimation()
    {
        Debug.Log($"{gameObject.name} - Order completed! Happy animation");
        // Add your success animation/particles here
    }

    void PlayFailAnimation()
    {
        Debug.Log($"{gameObject.name} - Wrong number! Angry animation");

        if (risingMonster != null && !string.IsNullOrEmpty(rejectAnimationName))
            risingMonster.PlayAnimation(rejectAnimationName);
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

    private void StartOrderTimer()
    {
        ShowOrderUI(); // cuando llega arriba, mostrar todo y empezar el timer
    }

    void OnMouseEnter()
    {
        if (risingMonster != null)
            risingMonster.PlayAnimation(hoverAnimationName);
    }

    void OnMouseExit()
    {
        if (risingMonster != null)
            risingMonster.PlayAnimation(idleAnimationName);
    }




    void OnDestroy()
    {
        if (dropArea != null)
        {
            dropArea.OnNumberDropped -= OnNumberReceived;
        }
    }
}