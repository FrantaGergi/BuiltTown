using UnityEngine;

public class PlayerCollisionDetector : MonoBehaviour
{
    // Nastav si vrstvu Player v Inspectoru, nebo zde použij LayerMask
    public LayerMask playerLayer;

    // Pokud je tento objekt collider s IsTrigger = false
    private void OnCollisionEnter(Collision collision)
    {
        // Kontrola, zda je kolidující objekt ve vrstvì Player
        if (((1 << collision.gameObject.layer) & playerLayer) != 0)
        {
            Debug.Log("Player collided with this object!");
            GameServices.I.ShowToolSwitchTutorial();
            // Sem vlož akci, napø. spustit tutoriál
        }
    }

    // Pokud používáš trigger (IsTrigger = true)
    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            Debug.Log("Player entered trigger area!");
            GameServices.I.ShowToolSwitchTutorial();

            // Sem vlož akci
        }
    }
}
