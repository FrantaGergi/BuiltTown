using UnityEngine;
using static InteractManager;

public class TreeResource : MonoBehaviour, IInteractable
{
    private bool isChopping;

    private float lastLoopCount = 0f;
    private Animator toolAnimator;

    public void Interact(InteractManager interactor, InteractAction action)
    {
        if (toolAnimator == null && interactor != null)
        {
            toolAnimator = interactor.GetToolAnimator();
        }

        switch (action)
        {
            case InteractAction.E:
                Debug.Log("Krátký úder na strom.");
                break;

            case InteractAction.HoldStart:
                isChopping = true;
                toolAnimator?.SetBool("IsDoing", true);
                Debug.Log("Zaèínáš sekat strom (držení)...");
                break;

            case InteractAction.Hold:
                if (isChopping)
                    Debug.Log("Sekáš strom...");
                break;

            case InteractAction.HoldEnd:
                isChopping = false;
                toolAnimator?.SetBool("IsDoing", false);
                Debug.Log("Pøestal jsi sekat strom.");
                break;
        }
    }

    public void OnHoverEnter() => Debug.Log("Míøíš na strom.");
    public void OnHoverExit() => Debug.Log("Už nemíøíš na strom.");


    private void Update()
    {
        if (isChopping)
        {
           
            if (toolAnimator != null)
            {
                DoCheck(toolAnimator);
            }
        }
    }

    private void DoCheck(Animator toolAnimator)
    {
        AnimatorStateInfo state = toolAnimator.GetCurrentAnimatorStateInfo(0);
        if (state.IsName("Mining"))
        {
            // získáme èíslo aktuální smyèky (0, 1, 2...)
            int loopCount = Mathf.FloorToInt(state.normalizedTime);

            // když se èíslo smyèky zmìnilo od minula
            if (loopCount > lastLoopCount)
            {
                Debug.Log("Jedna tìžební smyèka dokonèena!");
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

}
