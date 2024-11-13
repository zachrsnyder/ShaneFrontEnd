using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using FarmhandStuff;
using Microsoft.MixedReality.Toolkit.UX;
using TMPro;
using UnityEngine;

public class StatusOverlayController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI pauseBtnTxt;
    [SerializeField] private PressableButton eStopBtn;
    [SerializeField] private PressableButton cancelBtn;
    [SerializeField] private TextMeshProUGUI timeEstimate;
    [SerializeField] private TextMeshProUGUI currentGCodeName;
    
    [SerializeField] private HolographGraphController extruderTempGraph;
    [SerializeField] private HolographGraphController bedTempGraph;


    [SerializeField] private GameObject CancellationClassificationPrefab;

    private GameObject current_menu = null;

    private PrinterAnchor my_anchor;
    private FarmhandManager client;

    
    // Start is called before the first frame update
    void Start()
    {
        // client = FindObjectOfType<FarmhandManager>();
        my_anchor = GetComponentInParent<PrinterAnchor>();
        if (my_anchor == null)
        {
            Debug.LogError("NO PRINTER ANCHOR FOUND");
        }

        client = my_anchor.farmhand_client;
        
        //What if we switch states while the overlay is open?
        switch (my_anchor.Attached_Printer.current_state.ToLower())
        {
            case "paused":
                pauseBtnTxt.text = "Resume";
                currentGCodeName.text = "Current Print Job:\n" + my_anchor.Attached_Printer.current_gcode_name;
                break;
            case "printing":
                pauseBtnTxt.text = "Pause";
                currentGCodeName.text = "Current Print Job:\n" + my_anchor.Attached_Printer.current_gcode_name;
                break;
            //These last two are edge cases, sometimes it takes a sec for the print to start after it is submitted
            case "standby":
                pauseBtnTxt.text = "Login";
                currentGCodeName.text = "Standby";
                break;
            case "cancelled":
                pauseBtnTxt.text = "Login";
                currentGCodeName.text = "Standby";
                break;
            default:
                //This case should not happen...
                pauseBtnTxt.text = my_anchor.Attached_Printer.current_state.ToUpper();
                break;
        }

        my_anchor.onAttachedPrinterUpdated += onPrinterUpdateReceived;

    }

    private void onPrinterUpdateReceived()
    {
        switch (my_anchor.Attached_Printer.current_state.ToLower())
        {
            case "paused":
                pauseBtnTxt.text = "Resume";
                currentGCodeName.text = "Current Print Job:\n" + my_anchor.Attached_Printer.current_gcode_name;
                break;
            case "printing":
                pauseBtnTxt.text = "Pause";
                currentGCodeName.text = "Current Print Job:\n" + my_anchor.Attached_Printer.current_gcode_name;
                break;
            //These last two are edge cases, sometimes it takes a sec for the print to start after it is submitted
            case "standby":
                pauseBtnTxt.text = "Login";
                currentGCodeName.text = "Standby";
                break;
            case "cancelled":
                pauseBtnTxt.text = "Login";
                currentGCodeName.text = "Standby";
                break;
            default:
                Debug.LogError("Printer state unexpected: " + my_anchor.Attached_Printer.current_state);
                //This case should not happen...
                pauseBtnTxt.text = my_anchor.Attached_Printer.current_state.ToUpper();
                break;
        }
        extruderTempGraph.addPoint(my_anchor.Attached_Printer.current_temp_extruder);
        bedTempGraph.addPoint(my_anchor.Attached_Printer.current_temp_bed);
    }

    public void onPauseBtnPressed()
    {
        if (pauseBtnTxt.text == "Login")
        {
            my_anchor.Advance_Interface();
        }
        else if (pauseBtnTxt.text == "Pause")
        {
            my_anchor.PauseResume();
            //Should be taken care of by update
            // pauseBtnTxt.text = "Resume";
        }
        else if (pauseBtnTxt.text == "Resume")
        {
            my_anchor.PauseResume();
            // my_anchor.Attached_Printer.current_state = "printing";
            // pauseBtnTxt.text = "Pause";
        }
    }

    public void onEStopBtnPressed()
    {
        my_anchor.Estop();
        client.showNotification("WARNING", "After an estop is initiated, not only will the printer likely need to be hard reset, it also will not function with the holographic interface until the next restart :(", "Ok, got it!");
    }
    
    public void onCancelBtnPressed()
    {
        if (current_menu == null)
        {
            current_menu = Instantiate(CancellationClassificationPrefab, this.transform); 
        }
        else
        {
            my_anchor.CancelPrint();
            Destroy(current_menu);
        }
        
    }
    
    private void OnDestroy()
    {
        my_anchor.onAttachedPrinterUpdated -= onPrinterUpdateReceived;
    }
}
