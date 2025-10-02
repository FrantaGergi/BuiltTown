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
    protected Material originalMaterial;
    protected Renderer renderer;


    protected AnimationType miningAnimationName;
    [Range(10, 1000),SerializeField]
    public int hitPoints = 10;
    [Range(3, 100), SerializeField]
    public int hitsPerDrop = 3;

    protected int hitsTaken = 0;

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

        renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            originalMaterial = renderer.material;
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

    }
    public virtual void OnHoverExit() 
    {
    
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
            // získáme èíslo aktuální smyèky (0, 1, 2...)
            int loopCount = Mathf.FloorToInt(state.normalizedTime);
            // když se èíslo smyèky zmìnílo od minula
            if (loopCount > lastLoopCount)
            {
               TakeHit(1);
                // tady spustíš tìžbu (pøidat kámen, zahrát zvuk...)
            }

            lastLoopCount = loopCount;
        }
        else
        {
            // reset, když nejsme v Mining animaci
            lastLoopCount = 0;
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
        //  každá surovina bude mít svùj ItemSO
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


        Debug.Log("StartMining");
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
