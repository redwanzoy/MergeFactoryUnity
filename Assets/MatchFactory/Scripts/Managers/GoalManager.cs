using System;
using UnityEngine;
using System.Collections.Generic;

public class GoalManager : MonoBehaviour
{
    public static GoalManager instance;

    [Header("Elements")]
    [SerializeField] private Transform goalCardsParent;
    [SerializeField] private GoalCard goalCardPrefab;

    [Header("Data")]
    private ItemLevelData[] goals;
    private List<GoalCard>goalCards = new List<GoalCard>();

    public ItemLevelData[] Goals => goals; 


    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
        
        
        LevelManager.levelSpawned += OnLevelSpawned;
        ItemSpotsManager.itemPickedUp += OnItemPickedUp;

        PowerupManager.itemPickedUp += OnItemPickedUp;
        PowerupManager.itemBackToGame += OnItemBackToGame;

    }


    private void OnDestroy()
    {
        LevelManager.levelSpawned -= OnLevelSpawned;
        ItemSpotsManager.itemPickedUp -= OnItemPickedUp;

        PowerupManager.itemPickedUp -= OnItemPickedUp;
        PowerupManager.itemBackToGame -= OnItemBackToGame;
    }

    private void OnItemBackToGame(Item releasedItem)
    {
        //Loop throgh our goals
        //if this item is a goal
        //Increase the goal amount
        //Update the cards


        for (int i = 0; i < goals.Length; i++)
        {
            if (goals[i].itemPrefab.ItemName != releasedItem.ItemName)
                continue;

            goals[i].amount++;
            goalCards[i].UpdateAmount(goals[i].amount);
        }
    }

    private void OnLevelSpawned(Level level)
    {
        goals = level.GetGoals();

        GenerateGoalCards();

    }

    private void GenerateGoalCards()
    {
        for (int i = 0; i < goals.Length; i++)
            GenerateGoalCard(goals[i]);
        
    }

    private void GenerateGoalCard(ItemLevelData goal)
    {
        GoalCard cardInstance = Instantiate(goalCardPrefab, goalCardsParent);

        cardInstance.Configure(goal.amount, goal.itemPrefab.Icon);

        goalCards.Add(cardInstance);


    }




    private void OnItemPickedUp(Item item)
    {
        for (int i = 0; i < goals.Length; i++)
        {
            if (!goals[i].itemPrefab.ItemName.Equals(item.ItemName))
                continue;

            goals[i].amount--;

            if (goals[i].amount <= 0)
                CompleteGoals(i);
            else
                goalCards[i].UpdateAmount(goals[i].amount);

                break;
        }
    }

    private void CompleteGoals(int goalIndex)
    {
        Debug.Log("Goal Complete : " + goals[goalIndex].itemPrefab.ItemName);

        goalCards[goalIndex].Complete();

        CheckForLevelComplete();
        
    }

    private void CheckForLevelComplete()
    {
       for (int i = 0; i < goals.Length; i++)
        {
            if (goals[i].amount > 0)
                return;
        }

        GameManager.instance.SetGameState(EGameState.LEVELCOMPLETE);
        //Debug.Log("Level Complete");
    }
}
