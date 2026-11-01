using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class StoneProjectile : MonoBehaviour
{
    [SerializeField] private Explosion explosion;
    [SerializeField] private float lifeTime = 8f;

    private Rigidbody rb;

    private Collider[] projectileColliders;
    private bool launched;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        projectileColliders = GetComponentsInChildren<Collider>();

        if (explosion == null)
            explosion = GetComponent<Explosion>();
    }

    public void Prepare(Transform owner)
    {
        launched = false;

        rb.isKinematic = true;
        rb.useGravity = false;
        rb.velocity = Vector3.zero;

        Collider[] ownerColliders = owner.GetComponentsInChildren<Collider>();

        foreach (Collider projectileCol in projectileColliders)
        {
            projectileCol.enabled = false;

            foreach (Collider ownerCol in ownerColliders)
            {
                Physics.IgnoreCollision(
                    projectileCol,
                    ownerCol,
                    true
                );
            }
        }
    }

    public void Launch(Vector3 velocity)
    {
        transform.SetParent(null, true);

        foreach (Collider col in projectileColliders)
            col.enabled = true;

        rb.isKinematic = false;
        rb.useGravity = true;

        rb.velocity = velocity;

        launched = true;

        Destroy(gameObject, lifeTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!launched)
            return;

        launched = false;

        if (explosion != null)
            explosion.Explode();
        else
            Destroy(gameObject);
    }
}