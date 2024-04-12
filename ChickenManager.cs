using FishNet.Connection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChickenManager : MonoBehaviour
{


    public STANDARDCHICKEN standardChickenPrefab;


    public List<STANDARDCHICKEN> activeChickens = new List<STANDARDCHICKEN>();


    public ObjectPooler chickenPooler;

    public ObjectPooler eggPooler;

    public void ResetComponent()
    {

        foreach(STANDARDCHICKEN chicken in activeChickens)
        {

            if (chicken == null)
            {
                continue;
            }
            chicken.ResetCompletely();
            chicken.gameObject.SetActive(false);
        }

        activeChickens.Clear();

    }





    public STANDARDCHICKEN GenerateChicken(int uniqueItemID, string username, Vector3 where, Quaternion how, int hp, int maxhp, LOCALDATABASEMANAGER.AnimalData animalData, bool animatorStartsOn)
    {


        GameObject newChicken = chickenPooler.GetPooledObject();

        STANDARDCHICKEN standardchicken = newChicken.GetComponent<STANDARDCHICKEN>();
        standardchicken.ResetCompletely();
        standardchicken.SetUpChicken(uniqueItemID, username, where, how, hp, maxhp, animalData);
        standardchicken.transform.position = where;
        standardchicken.transform.rotation = how;
        standardchicken.chickenAnimator.enabled = animatorStartsOn;
        newChicken.SetActive(true);
        return standardchicken;

    }




    public void RevealChicken(int uniqueItemID, string username, Vector3 correctPosition, Quaternion correctRotation, int hp, int maxhp, LOCALDATABASEMANAGER.AnimalData animalData)
    {
        STANDARDCHICKEN currentChicken = GetLocalChickenByUniqueItemID(uniqueItemID);

        if (currentChicken != null)
        {
            return;
        }
  
      STANDARDCHICKEN newStandardChicken = GenerateChicken(uniqueItemID, username, correctPosition, correctRotation, hp, maxhp, animalData, true);

        if (newStandardChicken== null)
        {
            return;
        }



        activeChickens.Add(newStandardChicken);

        REFERENCENEXUS.Instance.clientsidesfx.PlayCustomAudioAtLocation(correctPosition, "Chicken calls 4");
    }

    public void LoadEgg(ANIMALBROADCASTER.EggData eggData)
    {



        PlaceEggAtPositionWithID(eggData.EggID, eggData.Position, eggData.ParentChickenID);

    }
    public void LoadChicken(LOCALDATABASEMANAGER.AnimalData animalData, List<ANIMALBROADCASTER.EggData> eggDatas)
    {
        int uniqueItemID = (int)animalData.SummonItemUniqueID;

        STANDARDCHICKEN currentChicken = GetLocalChickenByUniqueItemID(uniqueItemID);

        if (currentChicken != null)
        {
            return;
        }

        STANDARDCHICKEN standardchicken = GenerateChicken((int)animalData.SummonItemUniqueID, animalData.OwnerUsername, new Vector3(animalData.PositionX, animalData.PositionY, animalData.PositionZ), new Quaternion(animalData.RotationX, animalData.RotationY, animalData.RotationZ, animalData.RotationW), animalData.HP, animalData.MaxHP, animalData, false);


        activeChickens.Add(standardchicken);


        ANIMALBROADCASTER.EggData eggData = GetEggDataForChickenByID(eggDatas, animalData.AnimalID);

        if (eggData == null)
        {
            return;
        }



        PlaceEggAtPositionWithIDUsingTransform(eggData.EggID, eggData.Position, standardchicken.transform);



    }

    private ANIMALBROADCASTER.EggData GetEggDataForChickenByID(List<ANIMALBROADCASTER.EggData> eggDatas, int animalID)
    {
        
        foreach(ANIMALBROADCASTER.EggData eggData in eggDatas)
        {
            if (eggData.ParentChickenID == animalID)
            {
                return eggData;

            }

        }
        return null;


    }


    public void HideChicken(int uniqueItemID, string username, Vector3 correctPosition, Quaternion correctRotation)
    {

        STANDARDCHICKEN currentChicken = GetLocalChickenByUniqueItemID(uniqueItemID);

        if (currentChicken == null)
        {
      
            return;
        }

        activeChickens.Remove(currentChicken);

        currentChicken.gameObject.SetActive(false);



        REFERENCENEXUS.Instance.clientsidesfx.PlayCustomAudioAtLocation(correctPosition, "Chicken calls 1");


    }

    public void PlaceEggAtPositionWithID(int eggID, Vector3 position, int chickenUniqueItemID)
    {
        STANDARDCHICKEN thatChicken = GetLocalChickenByUniqueItemID(chickenUniqueItemID);

        if (thatChicken == null)
        {

            return;
        }
        Vector3 adjustedPosition = thatChicken.transform.forward * -0.4f;
        Vector3 spotBehindChicken = position + adjustedPosition;
        Vector3 raycastOriginPosition = spotBehindChicken + new Vector3(0f, 20f, 0f);

        LayerMask mask = REFERENCENEXUS.Instance.anyGroundOrObstructionLayerMask;

        RaycastHit hit;
        if (Physics.Raycast(raycastOriginPosition, Vector3.down, out hit, 100f, mask))
        {
            position = hit.point;
        }
        else
        {
           position = spotBehindChicken;
        }

        Vector3 eggoffset = new Vector3(0f, 0.1f, 0f);
        position += eggoffset;
        GameObject newEgg = eggPooler.GetPooledObject();
        newEgg.transform.position = position; 

        GROUNDEGG groundegg = newEgg.GetComponent<GROUNDEGG>();
        groundegg.eggID = eggID;
        groundegg.eggType = "BASICEGG";

        newEgg.SetActive(true);

        groundEggs.Add(groundegg);
    }

    
    public void PlaceEggAtPositionWithIDUsingTransform(int eggID, Vector3 position, Transform thatChicken)
    {

        Vector3 adjustedPosition = thatChicken.forward * -0.4f;
        Vector3 spotBehindChicken = position + adjustedPosition;
        Vector3 raycastOriginPosition = spotBehindChicken + new Vector3(0f, 20f, 0f);

         LayerMask mask = REFERENCENEXUS.Instance.anyGroundOrObstructionLayerMask;

        RaycastHit hit;
        if (Physics.Raycast(raycastOriginPosition, Vector3.down, out hit, 100f, mask))
        {
           position = hit.point;
        }
        else
        {
           position = spotBehindChicken;
        }

        Vector3 eggoffset = new Vector3(0f, 0.1f, 0f);
        position += eggoffset;
        GameObject newEgg = eggPooler.GetPooledObject();
        newEgg.transform.position = position;

        GROUNDEGG groundegg = newEgg.GetComponent<GROUNDEGG>();
        groundegg.eggID = eggID;
        groundegg.eggType = "BASICEGG";

        newEgg.SetActive(true);

        groundEggs.Add(groundegg);
    }


    public List<GROUNDEGG> groundEggs = new List<GROUNDEGG>();
    
    public void RemoveEggByID(int eggID)
    {


        GROUNDEGG newEgg = GetEggByID(eggID);

        if (newEgg != null)
        {
            groundEggs.Remove(newEgg);
            newEgg.gameObject.SetActive(false);

        }


    }

    public GROUNDEGG GetEggByID(int eggID)
    {
        foreach(GROUNDEGG egg in groundEggs)
        {

            if (egg.eggID == eggID)
            {
                return egg;
            }
        }
        return null;
    }

    public void Start()
    {
        REFERENCENEXUS.Instance.chickenmanager = this;
    }


    public STANDARDCHICKEN GetLocalChickenByUniqueItemID(int itemID)
    {

        foreach(STANDARDCHICKEN standardchicken in activeChickens)
        {

            if (standardchicken.UniqueChickenAssociatedItemID == itemID)
            {

                return standardchicken;
            }


        }


        return null;


    }





    public void ReceiveAnimalUpdate(int uniqueItemID, int healthAfterDamageTaken, bool died, string animalType)
    {
        STANDARDCHICKEN localStandardChicken = GetLocalChickenByUniqueItemID(uniqueItemID);

        if (localStandardChicken == null)
        {
        
            return;
        }


        localStandardChicken.health = healthAfterDamageTaken;

        if (died)
        {


            localStandardChicken.DeathRoutine();

            
        }









    }


    public void RequestTurnOff(STANDARDCHICKEN standardchicken, GameObject whatAnimal)
    {

        StartCoroutine(delayTurnOff(standardchicken, whatAnimal));
    
    }

    private IEnumerator delayTurnOff(STANDARDCHICKEN standardchicken, GameObject whatAnimal)
    {

        yield return new WaitForSeconds(10f);



        whatAnimal.SetActive(false);
        standardchicken.ResetCompletely();

    }


}
