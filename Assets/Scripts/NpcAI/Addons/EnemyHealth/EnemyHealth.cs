using Code.Global.Units.Interfaces;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    public static float RoundSpawnMaxHealth = -1f;

    [Header("Health")]
    [SerializeField] private float baseMaxHealth = 500f;
    [SerializeField] private Image hpBar;

    [Header("Drop")]
    [SerializeField, Range(0f, 1f)] private float dropChance = 0.5f;
    [SerializeField] private Transform dropPoint;
    [SerializeField] private GameObject dropItemA;
    [SerializeField] private GameObject dropItemB;

    [Header("Money")]
    [SerializeField] private int money = 5;
    [SerializeField] private Inventory inventory;

    private float currentHealth;
    private float maxHealth;
    private GameObject inventoryOBJ;

    private bool isDead;
    Explosion _explosion;
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    private void Awake()
    {
        maxHealth = RoundSpawnMaxHealth > 0f ? RoundSpawnMaxHealth : baseMaxHealth;
        currentHealth = maxHealth;
        _explosion = GetComponent<Explosion>();


        if (hpBar != null)
            hpBar.fillAmount = 1f;

        inventoryOBJ = GameObject.FindGameObjectWithTag("Inventory");
        if (inventoryOBJ!=null)
            inventory = inventoryOBJ.GetComponent<Inventory>();
    }

    public void ApplyDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        if (hpBar != null)
            hpBar.fillAmount = currentHealth / maxHealth;

        Debug.Log($"<color=red>{name} took {amount} damage, {currentHealth} HP left</color>");

        if (currentHealth <= 0f)
            Die();
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        AddMoney();
        if (_explosion != null)
            _explosion.Explode();
        //TryDropItem();

        Debug.Log($"<color=green>{name} died</color>");
        GameObject target = transform.parent != null ? transform.parent.gameObject : gameObject;
        Destroy(target);
    }

    //¬озможность дропать после смерти предметы, на данный момент не используетс€
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