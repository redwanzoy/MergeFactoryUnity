using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UIElements;

using Random = UnityEngine.Random;

public class ItemSpotsManager : MonoBehaviour
{
    public static ItemSpotsManager instance;


    [Header("Elements")]
    [SerializeField] private Transform itemSpotsParent;
    private ItemSpot[] spots;


    [Header("Settings")]
    [SerializeField] private Vector3 itemLocalPositionOnSpot;
    [SerializeField] private Vector3 itemLocalScaleOnSpot;
    private bool isBusy;

    [Header ("Data")]
    private Dictionary<EItemName, ItemMergeData> itemMergeDataDictionnary = new Dictionary<EItemName, ItemMergeData>();


    [Header("Animation Settings")]
    [SerializeField] private float animationDuration;
    [SerializeField] private LeanTweenType animationEasing;

    [Header("Actions")]
    public static Action<List<Item>> mergeStarted;
    public static Action<Item> itemPickedUp;


    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        InputManager.itemClicked += OnItemClicked;
        PowerupManager.itemBackToGame += OnItemBackToGame;

        StoreSpots();
    }

    private void OnDestroy()
    {
        InputManager.itemClicked -= OnItemClicked;
        PowerupManager.itemBackToGame -= OnItemBackToGame;
    }

    private void OnItemBackToGame(Item releasedItem)
    {
        if (!itemMergeDataDictionnary.ContainsKey(releasedItem.ItemName))
            return;

        //remove the item from the dictionary
        itemMergeDataDictionnary[releasedItem.ItemName].Remove(releasedItem);

        //check if we have more items with the same key
        //if not, remove the dictionary entry
        if (itemMergeDataDictionnary[releasedItem.ItemName].items.Count <= 0)
            itemMergeDataDictionnary.Remove(releasedItem.ItemName);
    }

    private void OnItemClicked(Item item)
    {
        if (isBusy)
        {
            Debug.LogError("ItemSpotsManager is busy...");
            return;
        }

        if (!IsFreeSpotAvailable())
        {
            Debug.LogWarning("No Free Spots Available");
            return;
        }

        //we are now busy
        isBusy = true;

        itemPickedUp?.Invoke(item);

        HandleItemClicked(item);

    }


    private void HandleItemClicked(Item item)
    {

        if (itemMergeDataDictionnary.ContainsKey(item.ItemName))
        {
            HandleItemMergeDataFound(item);
        }
        else
        {
            MoveItemToFirstFreeSpot(item);
        }

    }

    private void HandleItemMergeDataFound(Item item)
    {
        ItemSpot idealSpot = GetIdealSpotFor(item);

        itemMergeDataDictionnary[item.ItemName].Add(item);

        TryMoveItemToIdealSpot(item, idealSpot);

    }

    private ItemSpot GetIdealSpotFor(Item item)
    {
        List<Item> items = itemMergeDataDictionnary[item.ItemName].items;
        List<ItemSpot> itemSpots = new List<ItemSpot>();

        for (int i = 0; i < items.Count; i++)
            itemSpots.Add(items[i].Spot);
        //we have a list of occupied spots by the items similar to item

        //if we only have one spot, we should grab the spot next to it 
        if(itemSpots.Count >= 2)
            itemSpots.Sort((a,b) => b.transform.GetSiblingIndex().CompareTo(a.transform.GetSiblingIndex()));

        int idealSpotIndex = itemSpots[0].transform.GetSiblingIndex()+1;

        return spots[idealSpotIndex];

    }


    private void TryMoveItemToIdealSpot(Item item, ItemSpot idealSpot)
    {
        if(!idealSpot.IsEmpty())
        {
            HandleIdealSpotFull(item, idealSpot);
            return;
        }

        MoveItemToSpot(item, idealSpot, () => HandleItemReachedSpot(item));
    }


    private void MoveItemToSpot(Item item, ItemSpot targetSpot, Action completeCallback)
    {
        targetSpot.Populate(item);

        //2. Scale the item down, set its local position
        //item.transform.localPosition = itemLocalPositionOnSpot;
        //item.transform.localScale = itemLocalScaleOnSpot;
        //item.transform.localRotation = Quaternion.identity;

        LeanTween.moveLocal(item.gameObject, itemLocalPositionOnSpot, animationDuration)
            .setEase(animationEasing);
        LeanTween.scale(item.gameObject, itemLocalScaleOnSpot, animationDuration)
            .setEase(animationEasing);
        LeanTween.rotateLocal(item.gameObject, Vector3.zero, animationDuration)
            .setOnComplete(completeCallback);


        //3. Disable its shadow
        item.DisableShadows();

        //4. Disable its collider/physics
        item.DisablePhysics();

       

    }

    private void HandleItemReachedSpot(Item item, bool checkForMerge = true)
    {
        item.Spot.BumpDown(); 

        if (!checkForMerge)
            return;
        if (itemMergeDataDictionnary[item.ItemName].CanMergeItems())
            MergeItems(itemMergeDataDictionnary[item.ItemName]);
        else
            CheckForGameover();
    }

    private void MergeItems(ItemMergeData itemMergeData)
    {
        List<Item> items = itemMergeData.items;

        //remove the item merge data from the dictionary
        itemMergeDataDictionnary.Remove(itemMergeData.itemName);

        for (int i = 0; i < items.Count; i++)
            items[i].Spot.Clear();
        

        if (itemMergeDataDictionnary.Count <= 0)
            isBusy = false;
        else
            MoveAllItemsToTheLeft(HandleAllItemsMovedToTheLeft);


        mergeStarted?.Invoke(items);

    }

    private void MoveAllItemsToTheLeft(Action completeCallback)
    {
        bool callbackTriggered = false;

        for (int i = 3; i< spots.Length; i++)
        {
            ItemSpot spot = spots[i];

            if (spot.IsEmpty())
                continue;

            Item item = spot.Item;

            ItemSpot targetSpot = spots[i - 3];

            if (!targetSpot.IsEmpty())
            {
                Debug.LogWarning(targetSpot.name + "is Full");
                isBusy = false;
                return;
            }

            spot.Clear();

            completeCallback += () => HandleItemReachedSpot(item, false); 
            MoveItemToSpot(item, targetSpot, completeCallback);

            callbackTriggered = true;
        }

        if (!callbackTriggered)
        {
            completeCallback?.Invoke();
        }

    }

    private void HandleAllItemsMovedToTheLeft()
    {
        isBusy = false;
    }


    private void HandleIdealSpotFull(Item item, ItemSpot idealSpot)
    {
        MoveAllItemsToTheRightFrom(idealSpot, item);
    }

    private void MoveAllItemsToTheRightFrom(ItemSpot idealSpot, Item itemToPlace)
    {
        int spotIndex = idealSpot.transform.GetSiblingIndex();

        for (int i = spots.Length - 2; i >= spotIndex; i--)
        {
            ItemSpot spot = spots[i];

            //Double check this case
            if (spot.IsEmpty())
                continue;

            Item item = spot.Item;

            spot.Clear();

            ItemSpot targetSpot = spots[i + 1];

            if (!targetSpot.IsEmpty())
            {
                Debug.LogError("Error, this should not happen");
                isBusy = false;
                return;
            }

            MoveItemToSpot(item, targetSpot, ()=> HandleItemReachedSpot(item, false));
        }

        MoveItemToSpot(itemToPlace, idealSpot, () => HandleItemReachedSpot(itemToPlace));

    }


    private void MoveItemToFirstFreeSpot(Item item)
    {
        ItemSpot targetSpot = GetFreeSpot();

        if (targetSpot == null)
        {
            Debug.LogError("Target spot is null, this should not happen");
            return;
        }

        CreateItemMergeData(item);

        MoveItemToSpot(item, targetSpot, () => HandleFirstItemReachedSpot(item));

    }






    private void HandleFirstItemReachedSpot(Item item)
    {
        item.Spot.BumpDown();

        CheckForGameover();
    }


    private void CheckForGameover()
    {
        if(GetFreeSpot() == null)
        {
            GameManager.instance.SetGameState(EGameState.GAMEOVER);
        }
        else
        {
            isBusy = false;
        }
    }

    private void CreateItemMergeData(Item item)
    {
        itemMergeDataDictionnary.Add(item.ItemName, new ItemMergeData(item));
    }


    private void StoreSpots()
    {
        spots = new ItemSpot[itemSpotsParent.childCount];

        for (int i = 0; i < itemSpotsParent.childCount; i++)
        {
            spots[i] = itemSpotsParent.GetChild(i).GetComponent<ItemSpot>();
        }

    }

    private ItemSpot GetFreeSpot()
    {
        for (int i = 0; i < spots.Length; i++)
        {
            if (spots[i].IsEmpty())
            {
                return spots[i];
            }
        }
        return null;
    }


    private bool IsFreeSpotAvailable()
    {
        for (int i = 0; i < spots.Length; i++)
        {
            if (spots[i].IsEmpty()) 
            {
                return true;
            }
        }
        return false;
    }

    public ItemSpot GetRandomOccupiedSpot()
    {
        List<ItemSpot> occupiedSpots = new List<ItemSpot>();

        for (int i = 0; i < spots.Length; i++)
        {
            if (spots[i].IsEmpty())
                continue;

            occupiedSpots.Add(spots[i]);
        }
        if (occupiedSpots.Count <= 0)
            return null;

        return occupiedSpots[Random.Range(0, occupiedSpots.Count)];

    }


}
