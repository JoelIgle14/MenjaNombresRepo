using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("UI References")]
    public GameObject tutorialPanel;
    public TMP_Text explanationText;
    public TMP_Text actionText;
    public GameObject actionPanel;
    public Button nextButton;
    public GameObject highlightOverlay;
    public Image fadeOverlay;

    [Header("Tutorial Settings")]
    public float letterDelay = 0.05f;
    public bool tutorialActive = false;

    private int currentPhase = 0;
    private bool phaseCompleted = false;
    private bool isShowingText = false;

    // Tutorial phase tracking
    private bool hasDroppedNumberToMonster = false;
    private bool hasUsedAdder = false;
    private bool hasUsedTrashCan = false;
    private bool hasUsedSpecialBox = false;

    [System.Serializable]
    public class TutorialPhase
    {
        public string phaseName;
        public string explanationText;
        public string actionText;
        public PhaseType type;
        public GameObject highlightTarget;
        public float slowdownMultiplier = 0.3f;
    }

    public enum PhaseType
    {
        Explanation,
        DragDropBasic,
        UseAdder,
        UseTrashCan,
        UseSpecialBox,
        Complete
    }

    [Header("Tutorial Phases")]
    public TutorialPhase[] phases = new TutorialPhase[]
    {
        new TutorialPhase
        {
            phaseName = "Welcome",
            explanationText = "Benvingut a MenjaNombres! Els monstres tenen gana de nombres.",
            actionText = "",
            type = PhaseType.Explanation,
            slowdownMultiplier = 0f
        },
        new TutorialPhase
        {
            phaseName = "Belts Explanation",
            explanationText = "Els nombres es mouen per les cintes transportadores. Agafa'ls abans que desapareguin!",
            actionText = "",
            type = PhaseType.Explanation,
            slowdownMultiplier = 0.2f
        },
        new TutorialPhase
        {
            phaseName = "Basic Customer",
            explanationText = "Els monstres demanen nombres específics. Arrossega el nombre correcte a la seva comanda!",
            actionText = "Arrossega el nombre demanat al monstre",
            type = PhaseType.DragDropBasic,
            slowdownMultiplier = 0.3f
        },
        new TutorialPhase
        {
            phaseName = "Adder Machine",
            explanationText = "Utilitza les màquines per sumar o restar nombres. El resultat apareixerà a la cinta!",
            actionText = "Combina dos nombres a la màquina per crear un resultat",
            type = PhaseType.UseAdder,
            slowdownMultiplier = 0.3f
        },
        new TutorialPhase
        {
            phaseName = "Trash Can",
            explanationText = "Si no necessites un nombre, llença'l a la paperera per netejar la màquina.",
            actionText = "Arrossega un nombre a la paperera",
            type = PhaseType.UseTrashCan,
            slowdownMultiplier = 0.3f
        },
        new TutorialPhase
        {
            phaseName = "Special Boxes",
            explanationText = "Les caixes especials donen poders! Combina-les amb nombres per activar efectes únics.",
            actionText = "Agafa una caixa especial i combina-la amb un nombre",
            type = PhaseType.UseSpecialBox,
            slowdownMultiplier = 0.3f
        },
        new TutorialPhase
        {
            phaseName = "Tutorial Complete",
            explanationText = "Perfecte! Ara estàs preparat per servir monstres. Molta sort!",
            actionText = "",
            type = PhaseType.Complete,
            slowdownMultiplier = 1f
        }
    };

    private void LateUpdate()
    {
        if (PlayerPrefs.GetInt("TutorialCompleted", 0) == 1)
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        tutorialActive = true;
        StartTutorial();

        if (nextButton != null)
            nextButton.onClick.AddListener(OnNextButtonClicked);
    }

    public void StartTutorial()
    {
        tutorialActive = true;
        currentPhase = 0;


        StartCoroutine(ShowPhase(currentPhase));
    }

    IEnumerator ShowPhase(int phaseIndex)
    {
        if (phaseIndex >= phases.Length)
        {
            CompleteTutorial();
            yield break;
        }

        phaseCompleted = false;
        TutorialPhase phase = phases[phaseIndex];

        Debug.Log($"[Tutorial] Starting phase: {phase.phaseName}");

        switch (phase.type)
        {
            case PhaseType.DragDropBasic:
                GameManager.Instance.ForceBasicCustomerPhase();
                break;
            case PhaseType.UseAdder:
                GameManager.Instance.ForceAdditionPhase();
                break;
            case PhaseType.UseSpecialBox:
                GameManager.Instance.ForceSpecialBoxPhase();
                break;
        }


        // Show tutorial panel
        if (tutorialPanel != null)
            tutorialPanel.SetActive(true);

        // Show explanation text with typing effect
        if (explanationText != null && !string.IsNullOrEmpty(phase.explanationText))
        {
            yield return StartCoroutine(TypeText(explanationText, phase.explanationText));
        }

        // Wait for player to click next on explanation phases
        if (phase.type == PhaseType.Explanation)
        {
            if (nextButton != null)
                nextButton.gameObject.SetActive(true);

            yield return new WaitUntil(() => phaseCompleted);

            if (nextButton != null)
                nextButton.gameObject.SetActive(false);
        }
        else
        {
            // Hide next button for action phases
            if (nextButton != null)
                nextButton.gameObject.SetActive(false);

            // Small delay before showing action
            yield return new WaitForSecondsRealtime(1f);

            // Hide explanation panel
            if (tutorialPanel != null)
                tutorialPanel.SetActive(false);

            // Show action panel with requirement
            if (actionPanel != null && !string.IsNullOrEmpty(phase.actionText))
            {
                actionPanel.SetActive(true);
                if (actionText != null)
                {
                    yield return StartCoroutine(TypeText(actionText, phase.actionText));
                }
            }

            // Highlight target if specified
            if (phase.highlightTarget != null && highlightOverlay != null)
            {
                HighlightObject(phase.highlightTarget);
            }

            // Set game speed for this phase
            Time.timeScale = phase.slowdownMultiplier;

            // Wait for phase completion
            yield return new WaitUntil(() => phaseCompleted);

            // Hide action panel
            if (actionPanel != null)
                actionPanel.SetActive(false);

            // Remove highlight
            if (highlightOverlay != null)
                highlightOverlay.SetActive(false);
        }

        // Move to next phase
        currentPhase++;
        yield return new WaitForSecondsRealtime(0.5f);
        StartCoroutine(ShowPhase(currentPhase));
    }

    IEnumerator TypeText(TMP_Text textComponent, string fullText)
    {
        isShowingText = true;
        textComponent.text = "";

        foreach (char c in fullText)
        {
            textComponent.text += c;
            yield return new WaitForSecondsRealtime(letterDelay);
        }

        isShowingText = false;
    }

    void HighlightObject(GameObject target)
    {
        if (highlightOverlay == null) return;

        highlightOverlay.SetActive(true);
        // Position highlight overlay over the target
        // You can customize this based on your UI setup
        highlightOverlay.transform.position = target.transform.position;
    }

    void OnNextButtonClicked()
    {
        if (isShowingText)
        {
            // Skip typing animation
            StopAllCoroutines();
            if (currentPhase < phases.Length)
            {
                explanationText.text = phases[currentPhase].explanationText;
            }
            isShowingText = false;
        }
        else
        {
            phaseCompleted = true;
        }
    }

    // Methods called by game events to track tutorial progress
    public void OnNumberDroppedToMonster()
    {
        if (!tutorialActive) return;

        if (phases[currentPhase].type == PhaseType.DragDropBasic && !hasDroppedNumberToMonster)
        {
            hasDroppedNumberToMonster = true;
            phaseCompleted = true;
            Debug.Log("[Tutorial] Player completed drag & drop!");
        }
    }

    public void OnAdderUsed()
    {
        if (!tutorialActive) return;
        if (phases[currentPhase].type == PhaseType.UseAdder && !hasUsedAdder)
        {
            GetComponent<GameManager>().RemoveAllMonsters();
            hasUsedAdder = true;
            phaseCompleted = true;
            Debug.Log("[Tutorial] Player used adder!");
        }
    }
    public void OnTrashCanUsed()
    {
        if (!tutorialActive) return;

        if (phases[currentPhase].type == PhaseType.UseTrashCan && !hasUsedTrashCan)
        {
            hasUsedTrashCan = true;
            phaseCompleted = true;
            Debug.Log("[Tutorial] Player used trash can!");
        }
    }

    public void OnSpecialBoxUsed()
    {
        if (!tutorialActive) return;

        if (phases[currentPhase].type == PhaseType.UseSpecialBox && !hasUsedSpecialBox)
        {
            hasUsedSpecialBox = true;
            phaseCompleted = true;
            Debug.Log("[Tutorial] Player used special box!");
        }
    }

    void CompleteTutorial()
    {
        Debug.Log("[Tutorial] Tutorial completed!");
        tutorialActive = false;

        // Save completion
        PlayerPrefs.SetInt("TutorialCompleted", 1);
        PlayerPrefs.Save();

        // Hide all UI
        if (tutorialPanel != null) tutorialPanel.SetActive(false);
        if (actionPanel != null) actionPanel.SetActive(false);
        if (highlightOverlay != null) highlightOverlay.SetActive(false);

    }

    public void SkipTutorial()
    {
        PlayerPrefs.SetInt("TutorialCompleted", 1);
        CompleteTutorial();
    }

    public bool IsTutorialActive()
    {
        return tutorialActive;
    }

    public PhaseType GetCurrentPhaseType()
    {
        if (currentPhase < phases.Length)
            return phases[currentPhase].type;
        return PhaseType.Complete;
    }
}