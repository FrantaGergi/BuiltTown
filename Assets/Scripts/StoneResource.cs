using UnityEngine;
using static InteractManager;

public class StoneResource : MonoBehaviour, IInteractable
{
    private bool isMining;

    public void Interact(InteractManager interactor, InteractAction action)
    {
        switch (action)
        {
            case InteractAction.E:
                Debug.Log("Krátkı úder na kámen.");
                break;

            case InteractAction.HoldStart:
                isMining = true;
                Debug.Log("Zaèínáš tìit kámen (drení)...");
                break;

            case InteractAction.Hold:
                if (isMining)
                    Debug.Log("Tìíš kámen...");
                break;

            case InteractAction.HoldEnd:
                isMining = false;
                Debug.Log("Pøestal jsi tìit kámen.");
                break;
        }
    }

    public void OnHoverEnter() => Debug.Log("Míøíš na kámen.");
    public void OnHoverExit() => Debug.Log("U nemíøíš na kámen.");
}
