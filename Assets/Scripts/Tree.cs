using UnityEngine;
using static InteractManager;

public class Tree : MonoBehaviour, IInteractable
{
    private bool isChopping;

    public void Interact(GameObject interactor, InteractAction action)
    {
        switch (action)
        {
            case InteractAction.E:
                Debug.Log("Krátkı úder na strom.");
                break;

            case InteractAction.HoldStart:
                isChopping = true;
                Debug.Log("Zaèínáš sekat strom (drení)...");
                break;

            case InteractAction.Hold:
                if (isChopping)
                    Debug.Log("Sekáš strom...");
                break;

            case InteractAction.HoldEnd:
                isChopping = false;
                Debug.Log("Pøestal jsi sekat strom.");
                break;
        }
    }

    public void OnHoverEnter() => Debug.Log("Míøíš na strom.");
    public void OnHoverExit() => Debug.Log("U nemíøíš na strom.");
}
