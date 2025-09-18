using UnityEngine;
using UnityEngine.InputSystem;

public class MiniManager : MonoBehaviour
{
    [Header("Tree Settings")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("Wood Settings")]
    public GameObject woodPrefab;
    public Transform spawnPoint;
    public float healthPerWood = 20f; // kolik HP ubere každý spawn døeva

    [Header("Mining Settings")]
    public bool isBeingMined;
    public float miningSpeed = 10f;  // HP za sekundu pøi držení tlaèítka

    private float accumulatedDamage = 0f;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (isBeingMined)
        {
            MineTree();
        }
    }

    private void MineTree()
    {
        float damage = miningSpeed * Time.deltaTime;
        currentHealth -= damage;
        accumulatedDamage += damage;

        // spawn døeva za každých healthPerWood
        while (accumulatedDamage >= healthPerWood)
        {
            Debug.Log("Padá do inv");
            accumulatedDamage -= healthPerWood;
        }

        if (currentHealth <= 0f)
        {
            Destroy(gameObject);
        }
    }


    public void StartMining(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return; //reaguj jen na performed

        isBeingMined = true;
    }
    public void StopMining(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return; //reaguj jen na performed

        isBeingMined = false;
    }
}