using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GreenPlayerPing : MonoBehaviour
{
    public RectTransform questMarkerRectTransform; 

 
    public bool firstAssignment = false;
  
    public List<Image> questMarkerImages;
    public Image arrow1, arrow2;
    public TextMeshProUGUI distanceText;
    public Transform questObjectiveTransform;
    public Transform questObjectiveFixedTransform;
    public Transform playerTransform;

    private float screenWidth, screenHeight;
    private Camera mainCamera;
    private float orbitRadius = 245f; 
    private float secondArrowDistance = 80f; 
    private GameSetupController gsc;



 


    public void UpdateTarget(Transform what)
    {

  
        questObjectiveTransform = what;

        questObjectiveFixedTransform = what;

        if (!firstAssignment)
        {
            firstAssignment = true;
            onWorldChange(0);
        }
    }
    public void UpdateTargetButDontChangeOriginal(Transform what)
    {


        questObjectiveTransform = what;

    }

    public void scrubTracker()
    {


        questObjectiveTransform = null;
        questObjectiveFixedTransform = null;
    }
    public bool disabled = false;


    void Start()
    {
        mainCamera = Camera.main;
        screenWidth = Screen.width;
        screenHeight = Screen.height;
        gsc = REFERENCENEXUS.Instance.gsc;

        arrow1.transform.SetParent(questMarkerRectTransform);
        arrow2.transform.SetParent(questMarkerRectTransform);





    }

    public void setUpReferences()
    {


 
      
    }
    public bool usingSpecialWorld()
    {
        bool ourbool = false;



        return ourbool;
    }

    public int findSpecialWorldID(int whatWorld)
    {

        int ourint = whatWorld;





        return ourint;

    }



    public void onWorldChange(int whatWorld)
    {

        if (questObjectiveTransform == null)
        {

            return;
        }
        if (questObjectiveFixedTransform == null)
        {

            return;
        }

        Transform newObjectiveTarget = null;

   
      

        newObjectiveTarget = questObjectiveFixedTransform;
        if (newObjectiveTarget != null)
        {
            UpdateTargetButDontChangeOriginal(newObjectiveTarget.transform);
        }

    }


    void Update()
    {

        if (playerTransform == null)
        {
            if (gsc.realLocalPlayer != null)
            {
                playerTransform = gsc.realLocalPlayer.transform;
            }
            return;
        }
        if (questObjectiveTransform == null)
        {
            return;
        }
        Vector3 objectivePos = questObjectiveTransform.position;
        Vector3 screenPos = mainCamera.WorldToScreenPoint(objectivePos);

        float distance = Vector3.Distance(mainCamera.transform.position, objectivePos);
        distanceText.text = distance.ToString("F2") + "m";

        Vector3 directionToObjective = (objectivePos - playerTransform.position).normalized;
        float angleToObjective = Mathf.Atan2(directionToObjective.x, directionToObjective.z) * Mathf.Rad2Deg - playerTransform.eulerAngles.y;

        if (angleToObjective < 0) angleToObjective += 360;

        if (screenPos.z > 0 && screenPos.x > 0 && screenPos.x < screenWidth && screenPos.y > 0 && screenPos.y < screenHeight)
        {
            foreach (Image img in questMarkerImages)
            {
                img.rectTransform.position = new Vector3(screenPos.x, screenPos.y, img.rectTransform.position.z);
            }
            arrow1.gameObject.SetActive(false);
            arrow2.gameObject.SetActive(false);
        }
        else
        {
            if (screenPos.z < 0)
            {
                screenPos.x = screenWidth - screenPos.x;
                screenPos.y = screenHeight - screenPos.y;
            }

            Vector3 screenCenter = new Vector3(screenWidth, screenHeight, 0) / 2;

            float angle = Mathf.Atan2(screenPos.y - screenCenter.y, screenPos.x - screenCenter.x);
            float xEdge = screenCenter.x + Mathf.Cos(angle) * (screenWidth / 2);
            float yEdge = screenCenter.y + Mathf.Sin(angle) * (screenHeight / 2);

            Vector3 edgePos = new Vector3(xEdge, yEdge, 0);
            foreach (Image img in questMarkerImages)
            {

                img.rectTransform.position = new Vector3(edgePos.x, edgePos.y, img.rectTransform.position.z);
         
            }


        }

        if (screenPos.z < 0 || screenPos.x < 0 || screenPos.x > screenWidth || screenPos.y < 0 || screenPos.y > screenHeight)
        {
            float angle = Mathf.Atan2(directionToObjective.x, directionToObjective.z) * Mathf.Rad2Deg - playerTransform.eulerAngles.y;
            angle = -angle + 90;  
            if (angle < 0) angle += 360;

            Vector3 arrow1PosLocal = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0) * orbitRadius;
            Vector3 arrow2PosLocal = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0) * (orbitRadius + secondArrowDistance);

            arrow1.rectTransform.anchoredPosition = new Vector3(arrow1PosLocal.x, arrow1PosLocal.y, arrow1.rectTransform.position.z); ;
            arrow2.rectTransform.anchoredPosition = new Vector3(arrow2PosLocal.x, arrow2PosLocal.y, arrow2.rectTransform.position.z); 

       

            arrow1.rectTransform.localEulerAngles = new Vector3(0, 0, angle - 90);
            arrow2.rectTransform.localEulerAngles = new Vector3(0, 0, angle - 90);

            arrow1.gameObject.SetActive(true);
            arrow2.gameObject.SetActive(true);
        }
   
    }
}
