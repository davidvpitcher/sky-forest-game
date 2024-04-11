using FishNet.Connection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDummy : MonoBehaviour, IVulnerable
{

    public string creatureName; 

    public string GetCreatureType()
    {

        return "Test Dummy";
    }

    public void GetStruck()
    {

        PlayHitTaken();
        REFERENCENEXUS.Instance.clientsidesfx.PlayCustomAudioAtLocation(myTransform.position, "Chopping wood 13");
    }

    public void Start()
    {


        if (creatureName == "First Dummy")
        {

            REFERENCENEXUS.Instance.firstdummy = this;
        }

        originalLocation = myTransform.position;
        originalRotation = myTransform.rotation;

        health = 100;
        maxhealth = 100;
    }

    public Vector3 GetDamageNumberLocation()
    {

        return damageNumberSpot.position;
    }
    public Transform damageNumberSpot; //
    public int GetUniqueID()
    {
        return -1;
    }

    public void TakeDamage(int damage, NetworkConnection conn, string parthit, bool setOnFire, GameObject shooter, bool staggers, Vector3 hitLocation, int uniquecreatureID)
    {

        string currentWeapon = "";

   

        currentWeapon = REFERENCENEXUS.Instance.gsc.realLocalInventory.GetCurrentDamageType();


        if (string.IsNullOrEmpty(currentWeapon))
        {
    

            return;
        }

            REFERENCENEXUS.Instance.networkbroadcaster.UniqueCreatureTakesDamage(damage, conn, parthit, setOnFire, shooter, staggers, hitLocation, creatureName, DBManager.username, currentWeapon);


     

    }


    public int health;
    public int maxhealth;

    public int GetCurrentHealth()
    {


        return health;
    }

    public int GetMaxHealth()
    {
        return maxhealth;

    }

    public bool IsBelowHalfHealth()
    {

        return (health / 2) > 0.5;
    }

    public void ResetCompletely()
    {


        dummyActive = false;

    }

    public bool dummyActive = false;


    public Animator creatureAnimator;





    public Vector3 originalLocation;

    public Quaternion originalRotation;

    public void PlayHitTaken()
    {

      

        if (!creatureAnimator.enabled)
        {
            return;
        }
        creatureAnimator.Play("Take Damage", 0);

    }

    private void Awake()
    {
        myTransform = transform;
    }



 
 

    private Transform myTransform;



}
