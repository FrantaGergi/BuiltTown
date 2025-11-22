using UnityEngine;

public class ResourceMapManager : MonoBehaviour
{
    [Header("Resource SO Settings")]
    [field: SerializeField]
    public ItemSO WoodSO { get; private set; }
    [field: SerializeField]
    public ItemSO StoneSO { get;set; }
    [field: SerializeField]
    public ItemSO OreSO { get; set; }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (WoodSO == null || StoneSO == null || OreSO == null)
        {
            Debug.LogError("One or more  references are not set in ResourceMapManager.");
        }
    }


    public ItemSO GetResourceSO(ItemType itemType)
    {
        return itemType switch
        {
            ItemType.Wood => WoodSO,
            ItemType.Stone => StoneSO,
            ItemType.Ore => OreSO,
            _ => null,
        };
    }

}
