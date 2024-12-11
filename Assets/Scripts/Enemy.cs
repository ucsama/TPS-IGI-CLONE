using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    private Vector3 initialPosition;
    private Rigidbody rb;
    
    [Header("Enemy Health and Damage")]
    private float enemyHealth = 120f;
    private float presentHealth;
    public float giveDamage = 5f;
    public HealthBar healthBar;

    [Header("Enemy Things")]
    public NavMeshAgent enemyAgent;
    public Transform LookPoint;
    public Camera ShootingRaycastArea;
    public Transform playerBody;
    public LayerMask PlayerLayer;

    [Header("Enemy Guarding Var")]
    public GameObject[] walkPoints;
    int currentEnemyPosition = 0;
    public float enemySpeed;
    float walkingPointRadius = 2;

    [Header("Enemy Shooting Var")]
    public float timebtwShoot;
    bool previouslyShoot;

    [Header("Enemy Animation and Muzzle effect")]
    public Animator anim;
    public ParticleSystem muzzleSpark;

    [Header("Enemy Mood/Situations")]
    public float visionRadius;
    public float shootingRadius;
    public bool playerInvisionRadius;
    public bool playerInshootingRadius;

    private void Awake()
{
    presentHealth = enemyHealth;
    healthBar.GiveFullHealth(enemyHealth);
    playerBody = GameObject.Find("Player").transform;

    enemyAgent = GetComponent<NavMeshAgent>();
    rb = GetComponent<Rigidbody>();  // Initialize rb here
    
    initialPosition = transform.position;
}

    private void Update()
{
    if (enemyAgent.enabled && enemyAgent.isOnNavMesh) // Check if the agent is active and on NavMesh
    {
        playerInvisionRadius = Physics.CheckSphere(transform.position, visionRadius, PlayerLayer);
        playerInshootingRadius = Physics.CheckSphere(transform.position, shootingRadius, PlayerLayer);

        if (!playerInvisionRadius && !playerInshootingRadius) 
        {
            Guard();
        }
        else if (playerInvisionRadius && !playerInshootingRadius) 
        {
            enemyAgent.SetDestination(playerBody.position); // Set destination here
            pursuePlayer(); // Then pursue the player
        }
        else if (playerInvisionRadius && playerInshootingRadius) 
        {
            ShootPlayer();
        }
    }
}


    private void Guard()
    {
        if (Vector3.Distance(walkPoints[currentEnemyPosition].transform.position, transform.position) < walkingPointRadius)
        {
            currentEnemyPosition = Random.Range(0, walkPoints.Length);
            if (currentEnemyPosition >= walkPoints.Length)
            {
                currentEnemyPosition = 0;
            }
        }
        transform.position = Vector3.MoveTowards(transform.position, walkPoints[currentEnemyPosition].transform.position, Time.deltaTime * enemySpeed);

        transform.LookAt(walkPoints[currentEnemyPosition].transform.position);
    }

    private void pursuePlayer()
{
    if (enemyAgent.enabled && enemyAgent.isOnNavMesh)
    {
        //enemyAgent.stoppingDistance = 1.0f; // Set the stopping distance
        
        enemyAgent.SetDestination(playerBody.position);

        visionRadius = 25f;
        shootingRadius = 9f;
       
        // Animations
        anim.SetBool("Walk", false);
        anim.SetBool("AimRun", true);
        anim.SetBool("Shoot", false);
        anim.SetBool("Die", false);

        
    }
    else
    {
        Debug.LogWarning("NavMeshAgent is not active or not on the NavMesh.");
    }
}


    private void ShootPlayer()
    {
        if (enemyAgent.enabled && enemyAgent.isOnNavMesh)
        {
            enemyAgent.SetDestination(transform.position);
            transform.LookAt(LookPoint);

            if (!previouslyShoot)
            {
                muzzleSpark.Play();
                RaycastHit hit;
                Vector3 directionToPlayer = (playerBody.position - ShootingRaycastArea.transform.position).normalized;
                if (Physics.Raycast(ShootingRaycastArea.transform.position, directionToPlayer, out hit, shootingRadius, PlayerLayer))
                {
                    Debug.Log("Shooting" + hit.transform.name);
                    PlayerScript playerBody = hit.transform.GetComponent<PlayerScript>();

                    if (playerBody != null)
                    {
                        playerBody.playerHitDamage(giveDamage);
                    }

                    anim.SetBool("Shoot", true);
                    anim.SetBool("Walk", false);
                    anim.SetBool("AimRun", false);
                    anim.SetBool("Die", false);
                }

                previouslyShoot = true;
                Invoke(nameof(ActiveShooting), timebtwShoot);
            }
        }
    }

    private void ActiveShooting()
    {
        previouslyShoot = false;
    }

    public void enemtHitDamage(float takeDamage)
    {
        presentHealth -= takeDamage;
        healthBar.SetHealth(presentHealth);

        if (presentHealth <= 0)
        {
            anim.SetBool("Walk", false);
            anim.SetBool("Shoot", false);
            anim.SetBool("AimRun", false);
            anim.SetBool("Die", true);
            enemyDie();
        }
    }

   private void enemyDie()
{
    Debug.Log("Enemy died, preparing to respawn...");
    
    
    enemyAgent.SetDestination(transform.position);
    enemyAgent.enabled = false;  

    // Disabling colliders to prevent any physics-related shifts after enemy death
    Collider[] colliders = GetComponentsInChildren<Collider>();
    foreach (Collider collider in colliders)
    {
        collider.enabled = false;
    }

    // Ensuring rigidbody is not affected by physics
    Rigidbody rb = GetComponent<Rigidbody>();
    if (rb != null)
    {
        rb.isKinematic = true;  // Prevents physics interactions
    }

    // Keep enemy in the current position and play death animation
    transform.position = transform.position;  // Fix position explicitly to avoid shifts
    anim.SetBool("Die", true);

    // Start the respawn coroutine
    StartCoroutine(RespawnEnemy());
}


    private IEnumerator RespawnEnemy()
{
    yield return new WaitForSeconds(3.0f); // delay Adjustment 

    Debug.Log("Respawning enemy...");

    // Reset health and other properties
    presentHealth = enemyHealth;
    healthBar.GiveFullHealth(enemyHealth);

    // Reset position and rotation
    transform.position = initialPosition;
    transform.rotation = Quaternion.identity;

    // Re-enable components for the enemy to work properly
    enemyAgent.enabled = true;
    enemySpeed = 3.5f;
    shootingRadius = 6f;
    visionRadius = 13f;

    // Reactivate colliders
    foreach (Collider collider in GetComponentsInChildren<Collider>())
    {
        collider.enabled = true;
    }

    // Reset Rigidbody
    if (rb != null)
    {
        rb.isKinematic = false;
    }

    // Reset animator
    anim.Rebind();
    anim.SetBool("Die", false);
    anim.SetBool("Walk", true);

    Debug.Log("Enemy respawned successfully.");
}

}
