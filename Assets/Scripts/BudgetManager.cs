using UnityEngine;

public class BudgetManager : MonoBehaviour
{
    [SerializeField] int budgetRound;

    [SerializeField] BudgetCounter enemyCounter;
    [SerializeField] BudgetCounter playerCounter;

    [SerializeField] RoundData data;

    public void NextRound()
    {
        data.playerBudget += data.playerCount + budgetRound;
        data.enemyBudget += data.enemyCount + budgetRound;
        data.playerBudget = Mathf.Clamp(data.playerBudget, 0, 10);
        data.enemyBudget = Mathf.Clamp(data.enemyBudget, 0, 10);
        playerCounter.Change_Budget(data.playerBudget);
        enemyCounter.Change_Budget(data.enemyBudget);
    }
}
