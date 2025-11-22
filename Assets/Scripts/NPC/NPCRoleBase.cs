using UnityEngine;

public abstract class NPCRoleBase : MonoBehaviour
{
    protected BaseNPC npc;

    protected virtual void Awake()
    {
        npc = GetComponent<BaseNPC>();
        if (npc == null)
            Debug.LogError("NPCRoleBase requires BaseNPC on same GameObject.");
    }

    protected virtual void Update()
    {
        // roles implement their own state updates
    }
}
