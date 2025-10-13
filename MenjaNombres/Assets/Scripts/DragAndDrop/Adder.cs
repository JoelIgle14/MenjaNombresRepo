using UnityEngine;
public class Adder : MonoBehaviour
{
    [SerializeField] private bool add = true;
    [SerializeField] private NumberDropArea a1;
    [SerializeField] private NumberDropArea a2;
    [SerializeField] private ConveyorBelt belt;
    [SerializeField] private GameManager gameManager;
    public void Add()
    {
        if (a1.CurrentNum == 0 || a2.CurrentNum == 0) { Debug.Log("Need 1 more number"); return; }
        int result = add ? a1.CurrentNum + a2.CurrentNum : Mathf.Abs(a1.CurrentNum - a2.CurrentNum);

        result = Mathf.Clamp(result, 1, gameManager.difficultyLevels[gameManager.currentDifficultyIndex].maxNumValue);
        belt.QueueNumberSpawn(result);

        a1.CurrentNum = 0;
        a2.CurrentNum = 0;

        Destroy(a1.num.gameObject);
        Destroy(a2.num.gameObject);

        a1.num = null;
        a2.num = null;

        Debug.Log($"Adder queued result {result} for next spawn");
    }
}