using System.Resources;
using UnityEngine;

public class GameServices : MonoBehaviour
{
    public static GameServices I { get; private set; }

    public Transform Player;
    public ResourceMapManager resourceMapManager;
    public InventoryManager playerInventory;
    public NPCManager NPCManager;
    //public NPCManager npcManager;

    private void Awake()
    {
        if (I != null) { Destroy(gameObject); return; }
        I = this;

        DontDestroyOnLoad(gameObject);
       if( Player == null)
        Player = GameObject.FindGameObjectWithTag("Player").transform;
    }
}
