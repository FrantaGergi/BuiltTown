using UnityEngine;

public class ResourceNode : MonoBehaviour, IResourceSource
{
    [SerializeField] private ItemType type = ItemType.Wood;
    [SerializeField] private GameObject groundItemPrefab;
    [SerializeField] private int stock = 10;

    public ItemType Type => type;

    public bool CanMine()
    {
        return stock > 0;
    }

    public void MineOnce()
    {
        if (!CanMine()) return;

        stock--;

        if (groundItemPrefab != null)
        {
            var a = Instantiate(groundItemPrefab, transform.position + Vector3.up * 1.2f, Quaternion.identity);
            var gi = a.GetComponent<GroundItem>();
            var sib = a.GetComponent<SpawnIconBehaviour>();
            if (gi != null && sib != null)
            {
                ItemSO iso = GameServices.I.resourceMapManager.GetResourceSO(type);
                sib.SetAndStart(GameServices.I.Player,GameServices.I.playerInventory,1,iso);
                // set quantity/type via inspector on prefab or here if needed
            }else
                Debug.LogError("GroundItem or SpawnIconBehaviour component missing on groundItemPrefab");
        }
    }
}
