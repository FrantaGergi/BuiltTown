using UnityEngine;

public class ResourceNode : MonoBehaviour, IResourceSource
{
    [SerializeField] private ItemType type = ItemType.Wood;
    [SerializeField] private GameObject groundItemPrefab;
    [Header ("Dont assign, will be set automatically")]
    [SerializeField] private Resource resource;

    public ItemType Type => type;


    private void Start()
    {
        if (resource == null)
        {
            resource = GetComponent<Resource>();
            if (resource == null)
            {
                Debug.LogError("Resource component missing on ResourceNode.");
            }
        }
    }

    public bool CanMine()
    {
        return resource.hitPoints > 0;
    }

    public void MineOnce(int quantity)
    {
        if (!CanMine()) return;

        resource.hitPoints--;

        if (groundItemPrefab != null)
        {
            var a = Instantiate(groundItemPrefab, transform.position + Vector3.up * 1.2f, Quaternion.identity);
            var gi = a.GetComponent<GroundItem>();
            var sib = a.GetComponent<SpawnIconBehaviour>();
            if (gi != null && sib != null)
            {
                gi.SetQuantity(quantity);
                ItemSO iso = GameServices.I.resourceMapManager.GetResourceSO(type);
                sib.SetAndStart(GameServices.I.Player,GameServices.I.playerInventory,quantity,iso);
                // set quantity/type via inspector on prefab or here if needed
            }else
                Debug.LogError("GroundItem or SpawnIconBehaviour component missing on groundItemPrefab");
        }
    }
}
