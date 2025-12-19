using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class PowerupManager : MonoBehaviour
{
    //added cgpt
    public static PowerupManager instance;

    [Header("Vaccum Elements")]
    [SerializeField] private Vaccum vaccum;
    [SerializeField] private Transform vaccumSuckPosition;

    [Header("Vaccum Elements")]
    [SerializeField] private float fanMagnitude;




    [Header("Settings")]
    private bool isBusy;
    private int vaccumItemsToCollect;
    private int vaccumCounter;

    [Header("Actions")]
    public static Action<Item> itemPickedUp;
    public static Action<Item> itemBackToGame;

    [Header("Data")]
    [SerializeField] private int initialPUCount;
    private int vaccumPUCount;

    private void Awake()
    {
        LoadData();

        Vaccum.started += OnVaccumStarted;
        InputManager.powerupClicked += OnPowerupClicked;
    }

    private void OnDestroy()
    {
        Vaccum.started -= OnVaccumStarted;
        InputManager.powerupClicked -= OnPowerupClicked;
    }

    private void OnPowerupClicked(Powerup powerup)
    {
        if (isBusy)
            return;

        switch(powerup.Type)
        {
            case EPowerupType.Vaccum:
                HandleVaccumClicked();
                UpdateVaccumVisuals();
                break;
        }
    }

    private void HandleVaccumClicked()
    {
        if (vaccumPUCount <=0)
        {
            vaccumPUCount = 3;
            SaveData();
        }
        else
        {
            isBusy = true;

            vaccumPUCount--;
            SaveData();

            vaccum.Play();
        }

    }

    private void OnVaccumStarted()
    {
        VaccumPowerup();
    }


    #region Vaccum


    [Button]
    private void VaccumPowerup()
    {
        //Collect 3 target/goal items

        //Grab the items

        //Grab the goal items

        //Grab the giak that has the greatest amount
        //Grab 3 items

        Item[] items = LevelManager.instance.Items;
        ItemLevelData[] goals = GoalManager.instance.Goals;

        ItemLevelData? greatestGoal = GetGreatestGoal(goals);

        if (greatestGoal == null)
            return;

        ItemLevelData goal = (ItemLevelData)greatestGoal;

        vaccumCounter = 0;

        List<Item> itemsToCollect = new List<Item>();

        for (int i = 0; i < items.Length; i++)
        {
            if(items[i] == null)
                continue;


            if (items[i].ItemName == goal.itemPrefab.ItemName)
            {
                itemsToCollect.Add(items[i]);

                if (itemsToCollect.Count >= 3)
                    break;
            }
        }

        vaccumItemsToCollect = itemsToCollect.Count;

        for (int i = 0; i < itemsToCollect.Count; i++)
        {
            itemsToCollect[i].DisablePhysics();

            Item itemToCollect = itemsToCollect[i];

            List<Vector3> points = new List<Vector3>();

            points.Add(itemsToCollect[i].transform.position);
            points.Add(itemsToCollect[i].transform.position);

            points.Add(itemsToCollect[i].transform.position + Vector3.up * 2);
            points.Add(vaccumSuckPosition.transform.position + Vector3.up * 2);


            points.Add(vaccumSuckPosition.position);
            points.Add(vaccumSuckPosition.position);

            LeanTween.moveSpline(itemsToCollect[i].gameObject, points.ToArray(), .75f)
                 .setOnComplete(() => ItemReachedVaccum(itemToCollect));

            /*LeanTween.move(itemsToCollect[i].gameObject, vaccumSuckPosition.position, .5f)
                .setEase(LeanTweenType.easeInCubic)
                .setOnComplete(() => ItemReachedVaccum(itemToCollect));*/

            LeanTween.scale(itemsToCollect[i].gameObject, Vector3.zero, .75f);
        
        }


        for (int i = itemsToCollect.Count-1; i >= 0 ; i--)
        {
            itemPickedUp?.Invoke(itemsToCollect[i]);
            //Destroy(itemsToCollect[i].gameObject);
        }
    }

    private void ItemReachedVaccum(Item item)
    {
        vaccumCounter++;

        if (vaccumCounter >= vaccumItemsToCollect)
            isBusy = false;

        Destroy(item.gameObject);
    }



    private ItemLevelData? GetGreatestGoal(ItemLevelData[] goals)
    {
        int max = 0;
        int goalIndex = -1;

        for (int i = 0; i < goals.Length; i++)
        {
            if (goals[i].amount >= max)
            {
                max = goals[i].amount;
                goalIndex = i;

            }
        }

        if (goalIndex <= -1)
            return null;

        return goals[goalIndex];

        //return goals.OrderByDescending(g => g.amount).FirstOrDefault();
    }



    private void UpdateVaccumVisuals()
    {
        vaccum.UpdateVisuals(vaccumPUCount);
    }


    #endregion


    #region Spring

    [Button]
    private void SpringPowerup()
    {

        ItemSpot spot = ItemSpotsManager.instance.GetRandomOccupiedSpot();

        if (spot == null)
            return;

        isBusy = true;

        Item itemToRelease = spot.Item;

        spot.Clear();

        itemToRelease.UnassignSpot();
        itemToRelease.EnablePhysics();


        itemToRelease.transform.parent = LevelManager.instance.ItemParent;
        itemToRelease.transform.localPosition = Vector3.up * 3;
        itemToRelease.transform.localScale = Vector3.one;

        itemBackToGame?.Invoke(itemToRelease);

    }


    #endregion



    #region Fan

    [Button]
    public void FanPowerup()
    {
        Item[] items = LevelManager.instance.Items;

        foreach (Item item in items)
            item.ApplyRandomForce(fanMagnitude);

    }

    #endregion


    #region Freeze

    [Button]
    public void FreezeGunPowerup()
    {
        TimerManager.instance.FreezeTimer();
    }

    #endregion


    private void LoadData()
    {
        vaccumPUCount = PlayerPrefs.GetInt("VaccumCount", initialPUCount);

        UpdateVaccumVisuals();
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt("VaccumCount", vaccumPUCount);
    }



}
