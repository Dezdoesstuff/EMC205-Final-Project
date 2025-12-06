using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform player;

    [Header("Combat Parameters")]
    public int damagePerHit = 10;
    public float attackRange = 2f; 
    public float attackCooldown = 1f;   
    private float lastAttackTime;

    [Header("Thrown Object")]
    public int weaponLayer = 10;

    private bool dead = false;

    void Start()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        if (dead || player == null) return;

        // Move toward player
        agent.SetDestination(player.position);

        // Damage the player if in range
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance <= attackRange)
            {
                PlayerHealth ph = player.GetComponent<PlayerHealth>();
                if (ph != null)
                {
                    ph.TakeDamage(damagePerHit);
                    lastAttackTime = Time.time;
                }
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (dead) return;

        // Enemy dies if hit by a thrown object
        if (collision.collider.gameObject.layer == weaponLayer)
        {
            ThrowableObject t = collision.collider.GetComponent<ThrowableObject>();
            if (t != null && t.isThrown)
            {
                Die(collision);
            }
        }
    }

    void Die(Collision hit)
    {
        dead = true;
        agent.enabled = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.AddForce(hit.impulse, ForceMode.Impulse);
        }
        Destroy(gameObject, 3f);
    }
}