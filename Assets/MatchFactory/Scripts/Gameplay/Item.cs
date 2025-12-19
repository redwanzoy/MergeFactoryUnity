using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Item : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private EItemName itemName;    
    public EItemName ItemName => itemName;

    private ItemSpot spot;
    public ItemSpot Spot => spot;

    [SerializeField] private Sprite icon;
    public Sprite Icon => icon;



    [Header("Elements")]
    [SerializeField] private Renderer renderer;
    [SerializeField] private Collider collider;
    private Material baseMaterial;

    private void Awake()
    {
        baseMaterial = renderer.material;
    }

    public void AssignSpot(ItemSpot spot)
        => this.spot = spot;


    public void UnassignSpot()
        => spot = null;


    public void DisableShadows()
    {
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    public void DisablePhysics()
    {
        GetComponent<Rigidbody>().isKinematic = true;
        collider.enabled = false;
    }


    public void EnableShadows()
    {
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
    }

    public void EnablePhysics()
    {
        GetComponent<Rigidbody>().isKinematic = false;
        collider.enabled = true;
    }

    public void Select(Material outlineMaterial)
    {
        renderer.materials = new Material[] { baseMaterial, outlineMaterial };
    }

    public void Deselect()
    {
        renderer.materials = new Material[] {baseMaterial};
    }


    public void ApplyRandomForce(float magnitude)
    {
        GetComponent<Rigidbody>().AddForce(Random.onUnitSphere * magnitude, ForceMode.VelocityChange);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, .02f);
    }

}
