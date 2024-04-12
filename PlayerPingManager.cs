using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Object;
using FishNet.Connection;
using PixelCrushers.DialogueSystem;

public class PlayerPingManager : NetworkBehaviour
{

    public void ResetComponent()
    {

        currentlyPingedTransform = null;


        foreach(GameObject ping in generatedPings)
        {
            Destroy(ping);
        }

        onCooldown = false;
        generatedPings.Clear();
        generatedPingTransforms.Clear();
        currentlyTrackedObject.Clear();
        pingsByConnection.Clear();

    }

    public void PingHome()
    {

        pingLocation(REFERENCENEXUS.Instance.HomeLocation.gameObject);


    }


    private LayerMask groundMask;
    private LayerMask zombieMask;
    private void Awake()
    {
        playerCamera = Camera.main;
        zombieMask = LayerMask.GetMask("zomberskeleton", "obstruction", "Default");
        groundMask = LayerMask.GetMask("Default", "Terrain", "obstruction");
    }

    public void pingLocation(GameObject what)
    {
        placePingAtTransform(what.transform, REFERENCENEXUS.Instance.gsc.realLocalPlayerConnection, null);

    }

    public Transform currentlyPingedTransform;

    public GameObject pingPrefab;

    public void UpdateMarker(Transform what, GreenPlayerPing playerping, GameObject whatmarker)
    {

        if (what == null)
        {

            return;
        }
        if (playerping == null)
        {

            return;
        }
        if (whatmarker == null)
        {
            return;
        }

        playerping.UpdateTarget(what);
        TurnOnMarker(playerping, whatmarker);
    }
    public void TurnOnMarker(GreenPlayerPing playerping, GameObject questObjectiveMarker)
    {

        if (!playerping.disabled)
        {
            questObjectiveMarker.gameObject.SetActive(true);
        }

    }

    public Camera playerCamera; 


    private IEnumerator pingInputCooldown()
    {
        onCooldown = true;

        yield return new WaitForSeconds(pinglength);

        onCooldown = false;
    }
    private float pinglength = 5f;
    private bool onCooldown = false;


    public void ping() 
    {

        NetworkConnection myConn = REFERENCENEXUS.Instance.gsc.realLocalPlayerConnection;

        causeNetworkedPing(myConn);
   

    }

    private Dictionary<NetworkConnection, List<(GameObject ping, float creationTime)>> pingsByConnection = new Dictionary<NetworkConnection, List<(GameObject ping, float creationTime)>>();

    private void causeNetworkedPing(NetworkConnection conn)
    {
        lastPingedZombie = null;
      

        if (onCooldown) return;
        onCooldown = true;
        StartCoroutine(pingInputCooldown());

        Ray ray = playerCamera.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));
        RaycastHit hit;
        Vector3 pingPosition = Vector3.zero; 
        NetworkObject networkedZombie = null;

        int lastPingedTree = -1;

     
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, zombieMask))
        {
            if (hit.transform.root.gameObject.GetComponent<NetworkObject>())
            {
                networkedZombie = hit.transform.root.gameObject.GetComponent<NetworkObject>();
                lastPingedZombie = networkedZombie;
            }

            if (hit.transform.root.gameObject.CompareTag("tree")){

               
                lastPingedTree = hit.transform.root.gameObject.GetComponent<ConstructibleTree>().TreeID;
                lastPingedTreeID = lastPingedTree;
            }

            if (hit.transform.root.gameObject.CompareTag("Player"))
            {
            
                if (hit.transform.root.gameObject.GetComponent<NETWORKEDWOLF>())
                {

             

                    networkedZombie = hit.transform.root.gameObject.GetComponent<NetworkObject>();
                    lastPingedZombie = networkedZombie;

                }
            }
         
            pingPosition = hit.point;
        }
        else if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundMask))
        {
          
            pingPosition = hit.point;
        }
        else
        {
         
            pingPosition = ray.origin + ray.direction * 50f;
        }

      
        SendPingToClients(pingPosition, conn, networkedZombie, lastPingedTree);
    }

    public NetworkObject lastPingedZombie;

    public void BecomeLastPinged()
    {


        if (lastPingedZombie == null)
        {
            REFERENCENEXUS.Instance.gsc.realLocalInventory.sendLocalMessage("no target");
        } else
        {

            if (!lastPingedZombie.gameObject.GetComponent<WolfController>())
            {
                REFERENCENEXUS.Instance.gsc.realLocalInventory.sendLocalMessage("no wolf");
                return;
            }

            RequestBecomeLastPinged(this, REFERENCENEXUS.Instance.gsc.realLocalPlayerConnection, lastPingedZombie);

        

            REFERENCENEXUS.Instance.gsc.realLocalInventory.sendLocalMessage("become last pinged");
        }


    }
    [ServerRpc(RequireOwnership = false)]
    public void RequestBecomeLastPinged(PLAYERPINGMANAGER ppm, NetworkConnection pinger, NetworkObject lastPingedZombie)
    {
        if (lastPingedZombie.Owner == pinger)
        {

            ppm.InformPinger(pinger, "you already own it", lastPingedZombie, false);
            return;
        }
        lastPingedZombie.GiveOwnership(pinger);


        REFERENCENEXUS.Instance.ValidateConnection(pinger);

        ppm.InformPinger(pinger, "success", lastPingedZombie, true);
    }

    [TargetRpc]
    public void InformPinger(NetworkConnection pinger, string msg, NetworkObject thingBecame, bool success)
    {

        REFERENCENEXUS.Instance.gsc.realLocalInventory.sendLocalMessage(msg);

        if (!success)
        {
            return;
        }

        GameObject thePlayerGO = REFERENCENEXUS.Instance.playerMind;
        PlayerMind thePlayer = thePlayerGO.GetComponent<PlayerMind>();

        WolfController wolfController = thingBecame.gameObject.GetComponent<WolfController>();
        Transform myFollowPoint = wolfController.myFollowPoint;
        thingBecame.GetComponent<KinematicCharacterController.KinematicCharacterMotor>().enabled = true;
        thingBecame.GetComponent<Selector>().enabled = true;
        thingBecame.GetComponent<SelectorUseStandardUIElements>().enabled = true;
        if (wolfController)
        {
            wolfController.enabled = true;
            thePlayer.CameraFollowPoint = myFollowPoint;
            thePlayer.WolfCharacter = wolfController;

            REFERENCENEXUS.Instance.exampleCharacterCamera.SetFollowTransform(myFollowPoint);
            REFERENCENEXUS.Instance.playerMindManager.Character.enabled = false;
            REFERENCENEXUS.Instance.playerMindManager.Character.Motor.enabled = false;


            REFERENCENEXUS.Instance.gsc.realLocalNetworkedAdmiral.disableAnimatorBools();


        }



    }
    public int lastPingedTreeID = -1;

    public GameObject getLastPingedTree()
    {


        return REFERENCENEXUS.Instance.constructionmanager.LocateTreeById(lastPingedTreeID);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendPingToClients(Vector3 pingPosition, NetworkConnection conn, NetworkObject networkedZombie, int lastPingedTree)
    {
    
        CreatePingOnClients(pingPosition, conn, networkedZombie, lastPingedTree);
    }

    [ObserversRpc(ExcludeOwner = false, BufferLast = false)]
    private void CreatePingOnClients(Vector3 pingPosition, NetworkConnection conn, NetworkObject networkedZombie, int lastPingedTree)
    {
     
        if (pingsByConnection.TryGetValue(conn, out var pings))
        {
            var currentTime = Time.time;
            foreach (var (ping, creationTime) in pings)
            {
                if (currentTime - creationTime < pinglength)
                {
                    return;
                }
            }
        }

        Transform currentlyPingedTransform1 = null;

        Ray ray = playerCamera.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));
        RaycastHit hit;
        GameObject pingObject = null; 
        bool hitSomething = false;
     
        if (networkedZombie != null)
        {
          
            currentlyPingedTransform1 = networkedZombie.gameObject.transform;
            hitSomething = true;
        } else if (lastPingedTree != -1)
        {

           ConstructibleTree lastTree = REFERENCENEXUS.Instance.constructionmanager.LocateCTreeById(lastPingedTree);
            currentlyPingedTransform1 = lastTree.myTransform;
                hitSomething = true;
        }
        else
        {
         
            pingObject = new GameObject("PLAYERPINGTRANSFORM");

          
                pingObject.transform.position = pingPosition;
        
            currentlyPingedTransform1 = pingObject.transform;
            hitSomething = true;
        }


        placePingAtTransform(currentlyPingedTransform1, conn, pingObject);


    }

    private IEnumerator DestroyPingAfterDelay(GameObject ping, NetworkConnection conn, GameObject pingObject)
    {
        yield return new WaitForSeconds(pinglength);

      
        if (pingsByConnection.TryGetValue(conn, out var pings))
        {
            pings.RemoveAll(p => p.ping == ping);
            if (pings.Count == 0)
            {
              
                pingsByConnection.Remove(conn);
            }
        }
        Destroy(ping);

     
        if (pingObject != null && pingObject.name == "PLAYERPINGTRANSFORM")
        {
            Destroy(pingObject); 
        }
    }
    public Transform pingCanvas;

    public List<GameObject> generatedPings = new List<GameObject>();
    public List<Transform> generatedPingTransforms = new List<Transform>();
    private void placePingAtTransform(Transform newGameObjectToFollow, NetworkConnection conn, GameObject pingObject)
    {
        if (newGameObjectToFollow == null)
        {
            return;
        }
        generatedPingTransforms.Add(newGameObjectToFollow);


        GameObject newGreenPing = Instantiate(pingPrefab, pingCanvas);
        generatedPings.Add(newGreenPing);


        GreenPlayerPing newGreenPlayerPing = newGreenPing.transform.GetChild(0).GetChild(0).gameObject.GetComponent<GreenPlayerPing>();

        currentlyTrackedObject.Add(newGameObjectToFollow.gameObject);
        UpdateTracker(newGreenPlayerPing, newGreenPing, newGameObjectToFollow.gameObject);


        newGreenPing.transform.GetChild(0).gameObject.SetActive(true);

    
        if (!pingsByConnection.ContainsKey(conn))
        {
            pingsByConnection[conn] = new List<(GameObject ping, float creationTime)>();
        }
        pingsByConnection[conn].Add((newGreenPing, Time.time));


    
        StartCoroutine(DestroyPingAfterDelay(newGreenPing, conn, pingObject));


    }

    public void UpdateTracker(GreenPlayerPing newGreenPlayerPing, GameObject newGreenPing, GameObject newGameObjectToFollow)
    {


            UpdateMarker(newGameObjectToFollow.transform, newGreenPlayerPing, newGreenPing);


    }

    public List<GameObject> currentlyTrackedObject = new List<GameObject>();



}
