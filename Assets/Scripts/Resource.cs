using System;
using System.Collections.Generic;
using UnityEngine;
using static InteractManager;

public class Resource : MonoBehaviour, IInteractable
{
    protected bool isMining;

    protected float lastLoopCount = 0f;
    protected Animator toolAnimator;
    protected PlayerEquipment playerEquipment;
    protected InteractManager interactor;
    protected InventoryManager inventoryManager;
    protected ResourceMapManager resourceMapManager;
    protected ItemSO ItemSO;

    protected HitShakeEffect hitEffect;

    [Header("Highlight Settings"), SerializeField]
    protected GameObject highlightObject;

    protected AnimationType miningAnimationName;
    [Range(10, 1000),SerializeField]
    public int hitPoints = 10;
    [Range(1, 3), SerializeField]
    public int hitsPerDrop = 1;

    protected int hitsTaken = 0;

    [Header("Animation Hit Track Time")]
    [Range(0f,1f),SerializeField] protected float hitTime= 0.5f; // èas v sekundách, kdy bìhem animace dojde k zásahu
    protected bool hitTriggered = false;


    [SerializeField] private ResourceNode resourceNode;

    protected enum AnimationType
    {
        Chopping,
        Mining
    }

    private  readonly Dictionary<AnimationType, ItemType> requiredTools =
    new Dictionary<AnimationType, ItemType>
    {
        { AnimationType.Chopping, ItemType.Chopp },
        { AnimationType.Mining, ItemType.Mine }
        // sem snadno doplníš další (Fishing, Digging...)
    };

    protected virtual void Start()
    {
        enabled = false; // Update se nebude volat, dokud nezaène tìžba
        if (highlightObject != null)
            highlightObject.SetActive(false);

        hitEffect = GetComponent<HitShakeEffect>();
        if (hitEffect == null)
        {
            Debug.LogError("HitShakeEffect component not found on Resource.");
        }
        resourceNode = GetComponent<ResourceNode>();
        if(resourceNode == null)
        {
            Debug.LogError("ResourceNode component not found on Resource.");
        }
    }

    public virtual void Interact(InteractManager interactor, InteractAction action)
    {
        if(this.interactor == null)
        {
            this.interactor = interactor;
            playerEquipment = interactor.GetPlayerEquipment();
            toolAnimator = interactor.GetToolAnimator();
            inventoryManager = interactor.GetInventoryManager();
            resourceMapManager = interactor.GetResourceMapManager();

            LateStart(); // inicializace, která potøebuje interactor
        }
    }

    public virtual void OnHoverEnter(InteractManager interactor) 
    {

        SetVariablesFromInteractor(interactor);
        if (!HasCorrectTool()) return;

        highlightObject.SetActive(true);

    }
    public virtual void OnHoverExit()
    {
        if(highlightObject != null)
            highlightObject.SetActive(false);
    }

    protected void SetVariablesFromInteractor(InteractManager interactor)
    {
        if (this.interactor == null && interactor != null) { 
            this.interactor = interactor;
            playerEquipment = interactor.GetPlayerEquipment();
            toolAnimator = interactor.GetToolAnimator();
            inventoryManager = interactor.GetInventoryManager();
            resourceMapManager = interactor.GetResourceMapManager();

            LateStart(); // inicializace, která potøebuje interactor
         }

    }

    protected virtual void Update()
    {
        if (isMining)
        {

            if (toolAnimator != null)
            {
                DoCheck(toolAnimator);
            }
        }
    }

    // slouží když potøebuješ nìco inicializovat až po tom, co interactor zavolá Interact()
    protected virtual void LateStart()
    {
    }


    protected void DoCheck(Animator toolAnimator)
    {
        AnimatorStateInfo state = toolAnimator.GetCurrentAnimatorStateInfo(0);
        if (state.IsName(miningAnimationName.ToString()))
        {
            float progress = state.normalizedTime % 1f; // zbytek po 1 => hodnota mezi 0–1

            if (progress >= (hitTime-0.1f) && progress <= hitTime && !hitTriggered) // tøeba kolem poloviny animace
            {
                TakeHit(playerEquipment.CurrentTool.gatherAmount);
                hitTriggered = true;
            }

            // reset flagu, až animace zaène znovu
            if (progress < 0.1f)
                hitTriggered = false;
        }
        else
        {
            hitTriggered = false;
        }
    }

    protected virtual void TakeHit(int damagePoint)
    {
        hitPoints -= damagePoint;
       

        hitsTaken += damagePoint;
        if (hitsTaken >= hitsPerDrop)
        {
            int drops = hitsTaken / hitsPerDrop;
            hitsTaken = hitsTaken % hitsPerDrop;
            GetDrop(drops); // vrátí 1, 2, 3... podle toho, kolikrát se vejde
        }

        if (hitPoints <= 0)
        {
            DestroyResource();
        }
    }

    protected virtual void GetDrop(int ammount)
    {
        hitEffect.Hit();
        SpawnIcon(ammount, ItemSO);
        // inventoryManager.AddResourceToHotbar(ItemSO, ammount);


    }

    public void SpawnIcon(int ammount, ItemSO itemSO)
    {
        Vector3 origin = transform.position + Vector3.up * 1.2f;

        // už ne z isemso, abychom mohli modifikovat podle ruzné scaly resource node
        var a = Instantiate(resourceNode.GroundItemPrefab, origin, UnityEngine.Random.rotation);
        a.GetComponent<SpawnIconBehaviour>().SetAndStart(playerEquipment.Player, inventoryManager, ammount, itemSO);
    }


    protected virtual void DestroyResource()
    {
        StopMining();
        interactor.ClearCurrentTarget(this);
        Destroy(gameObject);
    }

    protected virtual void StartMining()
    {
        if(!HasCorrectTool()) return;


        isMining = true;
        enabled = true; // Aktivuje Update

        toolAnimator?.SetBool("Is" + miningAnimationName, true);
    }
    protected virtual void StopMining()
    {
        isMining = false;
        enabled = false; // Deaktivuje Update

        toolAnimator?.SetBool("Is" + miningAnimationName, false);
    }

    protected bool HasCorrectTool()
    {
        if (playerEquipment == null || playerEquipment.CurrentTool == null)
            return false;

        if (playerEquipment.CurrentTool.itemType == ItemType.None)
            return false;

        if (requiredTools.TryGetValue(miningAnimationName, out ItemType requiredTool))
        {
            return playerEquipment.CurrentTool.itemType == requiredTool;
        }

        return false; // pokud animace nemá pøiøazený žádný tool
    }


}
