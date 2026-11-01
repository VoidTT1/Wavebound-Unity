using Code.Global.Units.Interfaces;
using UnityEngine;
using UnityEngine.UI;

public class BlockedNPCHealth : MonoBehaviour, IDamageable
{
    public static float RoundSpawnMaxHealth = -1f;

    [Header("Health")]
    [SerializeField] private float baseMaxHealth = 100f;
    [SerializeField] private float _baseShieldMaxHealth = 200f;


    [Header("Drop")]
    [SerializeField, Range(0f, 1f)] private float dropChance = 0.5f;
    [SerializeField] private Transform dropPoint;
    [SerializeField] private GameObject dropItemA;
    [SerializeField] private GameObject dropItemB;

    [Header("Destroy")]
    [SerializeField] private GameObject Shield;

    [Header("Money")]
    [SerializeField] private int money = 5;
    [SerializeField] private Inventory inventory;


    private float currentHealth;
    private float _shieldCurrentHealth;
    private float maxHealth;
    private float _shieldMaxHealth;
    private bool isDead;

    private GameObject inventoryOBJ;
    public float CurrentHealth => currentHealth;
    public float ShieldCurrentHealth => _shieldCurrentHealth;
    public float MaxHealth => maxHealth;
    public float ShieldMaxHealth => _shieldMaxHealth;

    private bool isShield = true;
    public bool IsShield => isShield;

    private void Awake()
    {
        maxHealth = RoundSpawnMaxHealth > 0f ? RoundSpawnMaxHealth : baseMaxHealth;
        currentHealth = maxHealth;

        _shieldMaxHealth = RoundSpawnMaxHealth > 0f ? RoundSpawnMaxHealth : _baseShieldMaxHealth;
        _shieldCurrentHealth = _shieldMaxHealth;

        inventoryOBJ = GameObject.FindGameObjectWithTag("Inventory");
        inventory = inventoryOBJ.GetComponent<Inventory>();

        isShield = true;
    }


    public void ApplyDamage(float amount)
    {
        if (isDead) return;

        if (isShield == true)
        {
            _shieldCurrentHealth -= amount;
            _shieldCurrentHealth = Mathf.Clamp(_shieldCurrentHealth, 0f, _shieldMaxHealth);
            Debug.Log($"<color=blue>{name} took {amount} damage, {_shieldCurrentHealth} HP left</color>");
        }
        else if(!isShield) 
        {
            currentHealth -= amount;
            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
            Debug.Log($"<color=red>{name} took {amount} damage, {currentHealth} HP left</color>");
        }


        if(_shieldCurrentHealth <= 0f)
        {
            isShield = false;
            Destroy(Shield);
        }

        if (currentHealth <= 0f)
            Die();
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        AddMoney();
        //TryDropItem();

        Debug.Log($"<color=green>{name} died</color>");

        GameObject target = transform.parent != null ? transform.parent.gameObject : gameObject;
        Destroy(target);
    }

    private void TryDropItem()
    {
        if (Random.value > dropChance)
            return;

        GameObject prefabToSpawn = Random.value < 0.5f ? dropItemA : dropItemB;
        if (prefabToSpawn == null)
            return;

        Transform spawnTransform = dropPoint != null ? dropPoint : transform;

        Instantiate(prefabToSpawn, spawnTransform.position, spawnTransform.rotation);
    }
    private void AddMoney()
    {
        if (inventory == null)
            return;

        inventory.AddMoney(money);

    }
}