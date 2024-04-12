using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Object;
using FishNet.Connection;


public class StoneManagerServer :NetworkBehaviour
{


    void Start()
    {
        REFERENCENEXUS.Instance.stonemanagerserver = this;
        InitializeRockStates();
    }

    private void InitializeRockStates()
    {
        for (int i = 0; i < 11; i++)
        {
            rockStates[i] = true;
        }
    }

    public void InformServerStoneWasPickedUp(int rockId)
    {

       InformServerStoneWasPickedUpSERVER(rockId, REFERENCENEXUS.Instance.gsc.realLocalPlayerConnection, DBManager.username);



    }

    [ServerRpc(RequireOwnership = false)]
    private void InformServerStoneWasPickedUpSERVER(int rockId, NetworkConnection requester, string username)
    {
        REFERENCENEXUS.Instance.networkbroadcaster.ValidateConnection(requester);
        if (!rockStates.ContainsKey(rockId) || !rockStates[rockId])
        {
            REFERENCENEXUS.Instance.networkbroadcaster.SendArbitraryTargetMessage(requester, "Invalid rock");
            return;
        }

      rockStates[rockId] = false;
        StartCoroutine(RespawnRock(rockId, 130.0f));
        InformClientRockPickedUp(requester, rockId);
        InformObserversRockPickedUp(rockId);
       REFERENCENEXUS.Instance.networkbroadcaster.StoreItemForPlayerServerAuthoritatively(requester, username, "STONE", 1, 1, true, false); // update the database item
    }


    [ObserversRpc(BufferLast = false, ExcludeOwner = false)]
    public void InformObserversRockPickedUp(int rockId)
    {

        REFERENCENEXUS.Instance.stonemanagerclient.DeactivateRockByID(rockId);



    }

    [TargetRpc]
    public void InformClientRockPickedUp(NetworkConnection requester, int rockId)
    {

  

    }

    private IEnumerator RespawnRock(int rockId, float delay)
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(delay);

        rockStates[rockId] = true;
       ReactivateRock(rockId);
    }

    private void ReactivateRock(int rockId)
    {

        ReactivateRockFORALL(rockId);
    }

    [ObserversRpc(BufferLast = false, ExcludeOwner = false)]
    public void ReactivateRockFORALL(int rockID)
    {

        REFERENCENEXUS.Instance.stonemanagerclient.ReactivateRockByID(rockID);
    }



    public void RequestRockStateFromServerForNewClient(NetworkConnection requester)
    {
        RequestRockStateFromServerForNewClientSERVER(requester);


    }
    [ServerRpc(RequireOwnership = false)]
    public void RequestRockStateFromServerForNewClientSERVER(NetworkConnection requester)
    {

        REFERENCENEXUS.Instance.networkbroadcaster.ValidateConnection(requester);


        bool[] rockData = new bool[rockStates.Count];
        rockStates.Values.CopyTo(rockData, 0);
        ReceiveRockDataAsNewClient(requester, rockData);
    }
    private Dictionary<int, bool> rockStates = new Dictionary<int, bool>();


    [TargetRpc]
    public void ReceiveRockDataAsNewClient(NetworkConnection requester, bool[] rockData)
    {
        REFERENCENEXUS.Instance.stonemanagerclient.ProcessRockData(rockData);

    }

}
