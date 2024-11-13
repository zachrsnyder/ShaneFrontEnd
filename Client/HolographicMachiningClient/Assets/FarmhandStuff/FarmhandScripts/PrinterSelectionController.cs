using System;
using System.Collections;
using System.Collections.Generic;
using FarmhandStuff;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using Microsoft.MixedReality.Toolkit.UX;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PrinterSelectionController : MonoBehaviour
{
    /*
    [SerializeField] private PressableButton continueBtn;
    [SerializeField] private PressableButton pinBtn;
    [SerializeField] private GameObject menuParent;
    [SerializeField] private GameObject listButton;
    [SerializeField] private GameObject statusMenu;
    private PrinterBtn currentSelection;
    
    [SerializeField] private FarmhandManager client;
    
    // Start is called before the first frame update
    void Start()
    {
        client = FindObjectOfType<FarmhandManager>();
        if (client == null)
        {
            Debug.LogError("NO FARMHAND CONNECTION FOUND!");
        }
        else
        {
            client.onPrinterDiscovered += ClientOnPrinterDiscovered;
        }
        continueBtn.enabled = false;
    }

    private void ClientOnPrinterDiscovered(string s)
    {
        
        PrinterBtn new_btn = Instantiate(listButton, menuParent.transform).GetComponent<PrinterBtn>();
        new_btn.AssociatedPrinter = new Printer(name=s);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPrinterSelected(PrinterBtn button)
    {
        currentSelection = button;
        client.currentPrinter = currentSelection.AssociatedPrinter;
        client.requestPrinterUpdate(client.currentPrinter.name);
        continueBtn.enabled = true;
    }
    private void OnDestroy()
    {
        client.onPrinterDiscovered -= ClientOnPrinterDiscovered;
    }

    public void OnContinueBtnSelected(GameObject nextMenu)
    {
        if (client.currentPrinter.current_state == "standby" || client.currentPrinter.current_state == "cancelled" ||
            client.currentPrinter.current_state == "complete")
        {
            Instantiate(nextMenu);
        }
        else
        {
            Instantiate(statusMenu);
        }
        
        Destroy(this.gameObject);
    }
    */
}
