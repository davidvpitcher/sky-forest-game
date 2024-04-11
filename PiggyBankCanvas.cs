using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PiggyBankCanvas : MonoBehaviour
{
 
    void Start()
    {
        REFERENCENEXUS.Instance.piggybankcanvas = this;
    }


    public void ResetCompletely()
    {
        requestOut = false;
        piggybankbag.SetActive(false);

        bankFundsTextGO.SetActive(false);
        fundsInHandTextGO.SetActive(false);
        endfix1graceful();

    }

    private Coroutine fix1;

    public void fix1go()
    {

        endfix1graceful();
        fix1 = StartCoroutine(waitdelayfix());
    }

    private IEnumerator waitdelayfix()
    {

        yield return new WaitForSeconds(30f);

        requestOut = false;
    }
    private void endfix1graceful()
    {
        if (fix1 != null)
        {
            StopCoroutine(fix1);
            fix1 = null;
        }
    }
    public void OpenPiggyBankPanel()
    {
        receivedFunds = false;
        REFERENCENEXUS.Instance.menuManager.CloseInventoryIfNecessary();

        REFERENCENEXUS.Instance.menuManager.OnOpenedMenu(piggybankbag);

        REFERENCENEXUS.Instance.EnableCursor();
        RequestFundsStatus();
    }


    public void RequestFundsStatus()
    {
        requestOut = true;
        REFERENCENEXUS.Instance.networkbroadcaster.advancednetworkfunctions.RequestFundsStatus();
        fix1go();
    }

    public bool requestOut = false;

    public bool receivedFunds = false;
    public void ReceiveFundsStatus(int fundsinhand, int bankfunds, bool sfx)
    {


        fundsInHandText.text = fundsinhand.ToString();
        bankFundsText.text = bankfunds.ToString();
        DBManager.funds = fundsinhand;
        DBManager.bankfunds = bankfunds;
        requestOut = false;
        bankFundsTextGO.SetActive(true);
        fundsInHandTextGO.SetActive(true);
        receivedFunds = true;
        if (sfx)
        {

            REFERENCENEXUS.Instance.clientsidesfx.PlaySoundEffectByName("Item purchase 13");

        }
        endfix1graceful();
    }


    public GameObject fundsInHandTextGO;
    public GameObject bankFundsTextGO;
    
    public void PlayerPressedDeposit()
    {
        if (!receivedFunds)
        {
            REFERENCENEXUS.Instance.SendLocalMessage("Please wait as funds load");
            return;
        }
        if (requestOut)
        {
            REFERENCENEXUS.Instance.SendLocalMessage("Please wait and try again");
            return;
        }
        if (!piggybankbag.activeInHierarchy)
        {
            return;
        }

        REFERENCENEXUS.Instance.numberpickerpiggybank.pickingForDeposit = true;
        REFERENCENEXUS.Instance.numberpickerpiggybank.pickingForWithdraw = false;
        REFERENCENEXUS.Instance.numberpickerpiggybank.OpenNumberPicker();
    }

    public void OnPressedOkayForMultiple(string what)
    {
        if (!receivedFunds)
        {
            REFERENCENEXUS.Instance.SendLocalMessage("Please wait as funds load");
            return;
        }
        if (requestOut)
        {
            REFERENCENEXUS.Instance.SendLocalMessage("Please wait and try again");
            return;
        }
        if (what.Length > 5)
        {
            Debug.LogError("Length too long");
            REFERENCENEXUS.Instance.SendLocalMessage("Invalid input. Please enter a shorter number.");
            return;
        }
        if (!int.TryParse(what, out int textAsInt))
        {
            REFERENCENEXUS.Instance.SendLocalMessage("Invalid input. Please enter a valid number.");
            return;
        }
   
        if (REFERENCENEXUS.Instance.numberpickerpiggybank.pickingForWithdraw)
        {
            requestOut = true;
            REFERENCENEXUS.Instance.networkbroadcaster.advancednetworkfunctions.RequestWithdraw(textAsInt);
        }
        }
        public void OnPressedOkayForMultipleSell(string what)
    {

        if (!receivedFunds)
        {
            REFERENCENEXUS.Instance.SendLocalMessage("Please wait as funds load");
            return;
        }
        if (requestOut)
        {
            REFERENCENEXUS.Instance.SendLocalMessage("Please wait and try again");
            return;
        }
        if (what.Length > 5)
        {
            Debug.LogError("Length too long");
            REFERENCENEXUS.Instance.SendLocalMessage("Invalid input. Please enter a shorter number.");
            return;
        }
        if (!int.TryParse(what, out int textAsInt))
        {
            REFERENCENEXUS.Instance.SendLocalMessage("Invalid input. Please enter a valid number.");
            return;
        }

        if (REFERENCENEXUS.Instance.numberpickerpiggybank.pickingForDeposit)
        {
            requestOut = true;
            REFERENCENEXUS.Instance.networkbroadcaster.advancednetworkfunctions.RequestDeposit(textAsInt);
        }
     
    }


    public void PlayerPressedWithdraw()
    {
        if (!receivedFunds)
        {
            REFERENCENEXUS.Instance.SendLocalMessage("Please wait as funds load");
            return;
        }

        if (requestOut)
        {
            REFERENCENEXUS.Instance.SendLocalMessage("Please wait and try again");
            return;
        }
        if (!piggybankbag.activeInHierarchy)
        {
            return;
        }

        if (!piggybankbag.activeInHierarchy)
        {
            return;
        }

        REFERENCENEXUS.Instance.numberpickerpiggybank.pickingForDeposit = false;
        REFERENCENEXUS.Instance.numberpickerpiggybank.pickingForWithdraw = true;
        REFERENCENEXUS.Instance.numberpickerpiggybank.OpenNumberPicker();



    }
    public void ClosePiggyBankPanel()
    {
        receivedFunds = false;
      
        REFERENCENEXUS.Instance.menuManager.OnClosedMenu(piggybankbag);
        REFERENCENEXUS.Instance.menuManager.DisableCursorIfNoMenus();

        bankFundsTextGO.SetActive(false);
        fundsInHandTextGO.SetActive(false);

        if (REFERENCENEXUS.Instance.numberpickerpiggybank.numberpickerBag.activeInHierarchy)
        {

            REFERENCENEXUS.Instance.numberpickerpiggybank.CloseNumberPicker();
        }

    }

    public GameObject piggybankbag;

    public TextMeshProUGUI fundsInHandText;
    public TextMeshProUGUI bankFundsText;


}
