using UnityEngine;

public class ItemHand : MonoBehaviour
{
    public ItemSO itemScriptableObject;
    
    public Transform ikLeftTarget;
    public Transform ikRightTarget;

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }
}
