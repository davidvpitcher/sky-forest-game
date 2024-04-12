using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneManagerClient : MonoBehaviour
{
  
    void Start()
    {
        REFERENCENEXUS.Instance.stonemanagerclient = this;

    }

    public List<GameObject> PickupableStones = new List<GameObject>();


    public void ReceiveStoneListFromSkyIsland(List<GameObject> pickupableStones)
    {


        PickupableStones.AddRange(pickupableStones);


    }

    public void ReactivateRockByID(int rockID)
    {

        if (PickupableStones.Count < rockID)
        {
            return;
        }
        if (rockID== -1)
        {

            return;
        }

        GameObject pickUpableStone = PickupableStones[rockID];

        if (pickUpableStone != null)
        {
            pickUpableStone.SetActive(true);

        }


    }
    
    public void DeactivateRockByID(int rockID)
    {

        if (PickupableStones.Count < rockID)
        {
            return;
        }
        if (rockID== -1)
        {

            return;
        }

        GameObject pickUpableStone = PickupableStones[rockID];

        if (pickUpableStone != null)
        {
            pickUpableStone.SetActive(false);

        }


    }

    public void OnPlayerPickedUpStone(GameObject whichStone)
    {


        int whichRock = PickupableStones.IndexOf(whichStone);

        if (whichRock == -1)
        {
            return;
        }

        REFERENCENEXUS.Instance.stonemanagerserver.InformServerStoneWasPickedUp(whichRock);
    }


    public void RequestRockStateFromServerForNewClient()
    {

        REFERENCENEXUS.Instance.stonemanagerserver.RequestRockStateFromServerForNewClient(REFERENCENEXUS.Instance.gsc.realLocalPlayerConnection);
    }

    public void ProcessRockData(bool[] rockData)
    {
        for (int i = 0; i < rockData.Length && i < PickupableStones.Count; i++)
        {
            PickupableStones[i].SetActive(rockData[i]);
        }
    }
}
