using UnityEngine;

public class ItemSpot : MonoBehaviour
{
    [Header("Elements")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform itemParent;


    [Header("Settings")]
    private Item item;
    public Item Item => item;



    public void Populate(Item item)
    {
        this.item = item;
        //1.Turn the item as a child of the item spot
        item.transform.SetParent(itemParent);

        item.AssignSpot(this);

    }

    public void Clear()
    {
        item = null;
    }

    public void BumpDown()
    {
        animator.Play("Bump", 0, 0);
    }


    public bool IsEmpty() 
        => item == null;


}
