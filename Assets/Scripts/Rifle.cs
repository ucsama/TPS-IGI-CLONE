using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rifle : MonoBehaviour
{
    [Header("Rifle Things")]
     public Camera cammera;
    public float giveDamageOf = 10f;
    public float shootingRange = 100f;
    public float fireCharge = 15f;
    public Animator animator;
    public PlayerScript player;


    [Header("Rifle Animation & Shooting")]
    private int maximumAmmunation = 30;
    private int mag = 25;
    private int presentAmmunation;
    public float reloadingTime = 1.3f;
    private bool setReloading = false;
    private float nextTimeToShoot = 0f;



    [Header("Rifle Effetcs")]
    public ParticleSystem muzzleSpark;
    public GameObject impactEffect;
    public GameObject goreEffect;

private void Awake()
{
    presentAmmunation = maximumAmmunation; 
}


    // [Header(" Sounds and UI")]

    // Update is called once per frame
    void Update()
    {
        if(setReloading)
        return;
        if(presentAmmunation <=0)
        {
          StartCoroutine(Reload());
          return;
        }

        if(Input.GetButton("Fire1") && Time.time >= nextTimeToShoot){
            animator.SetBool("Fire", true);
            animator.SetBool("Idle", false);
            nextTimeToShoot =  Time.time + 1f/fireCharge;
            Shoot();
        }
        else if(Input.GetButton("Fire1") && Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            animator.SetBool("Idle", false);
            animator.SetBool("IdleAim", true);
            animator.SetBool("FireWalk", true);
            animator.SetBool("Walk", true);
            animator.SetBool("Reloading", false);
        }
        else if(Input.GetButton("Fire2") && Input.GetButton("Fire1"))
        {
            animator.SetBool("Idle", false);
            animator.SetBool("IdleAim", true);
            animator.SetBool("FireWalk", true);
            animator.SetBool("Walk", true);
            animator.SetBool("Reloading", false);
        }
        else
        {
            animator.SetBool("Fire", false);
            animator.SetBool("Idle", true);
            animator.SetBool("FireWalk", false);
            animator.SetBool("Reloading", false);

        }
    }
    void Shoot(){
        if (mag == 0)
        {
          return;
            //show amo text
        }
        presentAmmunation--;

        if(presentAmmunation == 0)
        {

            mag--;
        }

        //updating UI
        AmmoCount.occurance.UpdateAmmoText(presentAmmunation);
        AmmoCount.occurance.UpdateMagText(mag);

     muzzleSpark.Play();
     RaycastHit hitinfo;
     if(Physics.Raycast(cammera.transform.position, cammera.transform.forward, out hitinfo, shootingRange))
     {
       Debug.Log(hitinfo.transform.name);
       Objects objects = hitinfo.transform.GetComponent<Objects>();
       Enemy enemy = hitinfo.transform.GetComponent<Enemy>();

       if(objects != null)
       {
        objects.objectHitDamage(giveDamageOf);
        GameObject impactGo = Instantiate(impactEffect, hitinfo.point, Quaternion.LookRotation(hitinfo.normal));
        Destroy(impactGo, 2f);
        
       }
       else if(enemy != null)
       {
        enemy.enemtHitDamage(giveDamageOf);
         GameObject impactGO = Instantiate(goreEffect, hitinfo.point, Quaternion.LookRotation(hitinfo.normal));
       Destroy(impactGO, 2f); 
       }
      
     }
    }
    IEnumerator Reload()
    {
     player.playerSpeed = 0f;
     player.playerSprint = 0f;
     setReloading = true;
     Debug.Log("Reloading..");
     animator.SetBool("Reloading", true);
     yield return new WaitForSeconds(reloadingTime);
     animator.SetBool("Reloading", false);
     presentAmmunation = maximumAmmunation;
     player.playerSpeed = 1.9f;
     player.playerSprint = 3f;
     setReloading = false;
    }
}
