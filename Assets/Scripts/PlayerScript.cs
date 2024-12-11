using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
   
   [Header("Player Movement")]
   public float playerSpeed = 1.9f;
   public float playerSprint = 3f; 

    
    [Header("Player Health Things")]
    private float playerHealth = 120f;
    private float playerPresentHealth;
    public HealthBar healthBar;

     

    [Header("Player Camera Script")]
    public Transform playerCamera;
    public GameObject deathCamera;
    public GameObject EndGameMenuUI;

    [Header("Player animator & gravity")]
    public CharacterController cC;
    public float gravity = -9.81f;
    public Animator animator;


    


    [Header("Player jumping and velocity")]
     public float jumpRange = 1f;
    Vector3 velocity;
    public float turnCalmTime = 0.1f;
    float turnCalmVelocity;
    public Transform surfaceCheck;
    bool onSurface;
    public float surfaceDistance = 0.5f;
    public LayerMask surfaceMask;





    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        playerPresentHealth = playerHealth;
        healthBar.GiveFullHealth(playerHealth);
    }

    // Update is called once per frame
    void Update()
    {
       onSurface = Physics.CheckSphere(surfaceCheck.position, surfaceDistance, surfaceMask);

       if(onSurface && velocity.y < 0){

        velocity.y = -2f;
       }

       //gravity
       velocity.y += gravity * Time.deltaTime;
       cC.Move(velocity * Time.deltaTime);

       PlayerMove();
       Jump();
       PlayerSprint();
    }


    void PlayerMove(){

        float horizontal_axis = Input.GetAxisRaw("Horizontal");
        float vertical_axis = Input.GetAxisRaw("Vertical"); 

        Vector3 direction = new Vector3(horizontal_axis,0f,vertical_axis).normalized;
      
        if(direction.magnitude >= 0.1f){
            animator.SetBool("Walk", true);
            animator.SetBool("Running", false);
            animator.SetBool("Idle", false);
            animator.SetTrigger("Jump");
            animator.SetBool("AimWalk", false);
            animator.SetBool("IdleAim", false);
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + playerCamera.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnCalmVelocity, turnCalmTime);
            transform.rotation = Quaternion.Euler(0f,angle, 0f);
            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            cC.Move(moveDirection.normalized *playerSpeed * Time.deltaTime);
        }
        else
        {
            animator.SetBool("Idle", true);
            animator.SetTrigger("Jump");
            animator.SetBool("Walk", false);
            animator.SetBool("Running", false);
            animator.SetBool("AimWalk", false);

        }

       

    }
     void Jump(){
            if(Input.GetButtonDown("Jump") && onSurface){
                animator.SetBool("Walk", false);
                animator.SetTrigger("Jump");
                velocity.y = Mathf.Sqrt(jumpRange * -2 * gravity);
            }
            else
            {
              animator.ResetTrigger("Jump");
            }
        }

 void PlayerSprint (){
     if(Input.GetButton("Sprint") && Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) && onSurface)
       {
          float horizontal_axis = Input.GetAxisRaw("Horizontal");
          float vertical_axis = Input.GetAxisRaw("Vertical"); 

          Vector3 direction = new Vector3(horizontal_axis,0f,vertical_axis).normalized;
      
          if(direction.magnitude >= 0.1f){
            animator.SetBool("Running", true);
            animator.SetBool("Idle", false);
            animator.SetBool("Walk", false);
            animator.SetBool("IdleAim", false);
            

          float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + playerCamera.eulerAngles.y;
          float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnCalmVelocity, turnCalmTime);
          transform.rotation = Quaternion.Euler(0f,angle, 0f);
          Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            cC.Move(moveDirection.normalized *playerSprint * Time.deltaTime); 
             }
             else
             {
               animator.SetBool("Idle", false);
               animator.SetBool("Walk", false); 
             }
        }

    } 

    public void playerHitDamage(float takeDamage)
    {
      playerPresentHealth -= takeDamage;
      healthBar.SetHealth(playerPresentHealth);
       if(playerPresentHealth <= 0)
       {
        PlayerDie();
       }
    }
     
   private void PlayerDie()
   {
    EndGameMenuUI.SetActive(true);
    Cursor.lockState = CursorLockMode.None;
    animator.SetBool("Die", true);
    Object.Destroy(gameObject, 1.0f);
   }

}
