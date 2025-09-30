using UnityEngine;

public class ResourceMapManager : MonoBehaviour
{
    [Header("Resource Map Settings"), SerializeField]
    private ItemSO wood;
    [SerializeField]
    private ItemSO stone;
    [SerializeField]
    private ItemSO ore;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (wood == null || stone == null || ore == null)
        {
            Debug.LogError("One or more ItemSO references are not set in ResourceMapManager.");
        }
    }

    public ItemSO GetWoodSO()
    {
       return wood;
    }
    public ItemSO GetStoneSO()
    {
       return stone;
    }
    public ItemSO GetOreSO()
    {
       return ore;
    }
}
