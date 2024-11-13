using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using FarmhandStuff;
using Microsoft.MixedReality.OpenXR;
using Microsoft.MixedReality.SampleQRCodes;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using UnityEngine;
using QRCode = Microsoft.MixedReality.QR.QRCode;

public class PrinterAnchor : MonoBehaviour
{
    /// <summary>
    /// List of prefabs that should be spawned when the printer's state is updated to be the corresponding ui prefab's index
    /// INDEX MATTERS
    /// </summary>
    [SerializeField] 
    private List<GameObject> UIStatePrefabs = new List<GameObject>();
    
    private SpatialGraphNodeTracker tracker;
    
    [NonSerialized]
    public FarmhandManager farmhand_client;
    public QRCode qr_code;
    
    //TODO: Look at moving these events to the printer class rather than in the anchors, to enable future switching of printers among one anchor?
    public delegate void PrinterUpdateHandler();
    public event PrinterUpdateHandler onAttachedPrinterUpdated;

    public delegate void BedLevelInfoDisplay(float front_left, float front_right, float back_left, float back_right);

    public event BedLevelInfoDisplay onBedLevelInfoReceived;
    
    public delegate void PrintableListHandler(List<PrintableObject> printableObjects);
    public event PrintableListHandler onPrintablesLoaded;
    
    public delegate void DigitalTwinUpdateHandler();
    public event DigitalTwinUpdateHandler onAttachedDigitalTwinUpdated;

    
    public Printer Attached_Printer { get => current_printer; }
    private Printer current_printer;

    private GameObject current_interface = null;
    
    public enum PRINTER_UI_STATE
    {
        Status,
        Leveling,
        Confirmation,
        Print_Menu
    };
        
    public PRINTER_UI_STATE Interface_State { get; }
    private PRINTER_UI_STATE interface_state = 0;
    
    private void Awake()
    {
        tracker = this.GetComponent<SpatialGraphNodeTracker>();
        farmhand_client = FindObjectOfType<FarmhandManager>();
        if (farmhand_client == null)
        {
            Debug.LogError("NO FARMHAND CLIENT FOUND!");
        }
    }
    
    public void Confirm_Machine(Printer printer)
    {
        if (printer != null)
        {
            current_printer = printer;
            this.enabled = true;
            farmhand_client.request_loginstate(printer.name);
        }
        else
        {
            Debug.LogError("Couldn't confirm machine for this anchor.");
        }
        
    }

    public void PauseResume()
    {
        farmhand_client.requestPausePlay(Attached_Printer.name);
    }
    
    public void Advance_Interface()
    {
        farmhand_client.requestPrinterInterfaceAdvance(Attached_Printer.name);
    }

    public void Reverse_Interface()
    {
        farmhand_client.requestPrinterInterfaceReverse(Attached_Printer.name);
    }

    public void BeginLeveling()
    {
        farmhand_client.requestPrinterBedLevel(Attached_Printer.name);
    }
    
    /// <summary>
    /// Remark: This should not be called directly. Rather, "reverse/advance" commands should be sent to the rancher plugin, which will then reply with the proper state to farmhandManager, which will then call this function.
    /// </summary>
    /// <param name="new_state"></param>
    public void Update_Interface_State(PRINTER_UI_STATE new_state)
    {
        (Vector3, Quaternion) target_pos;
        if (current_interface != null)
        {
            current_interface.transform.GetPositionAndRotation(out target_pos.Item1, out target_pos.Item2);
            Destroy(current_interface);
        }
        else
        {
            this.transform.GetPositionAndRotation(out target_pos.Item1, out target_pos.Item2);
        }
        
        current_interface = Instantiate(UIStatePrefabs[(int)(new_state)], this.transform);
        
        current_interface.transform.SetPositionAndRotation(target_pos.Item1, target_pos.Item2);
        //SolverHandler handler = current_interface.GetComponent<SolverHandler>();
        // handler.TrackedTargetType = TrackedObjectType.CustomOverride;
        // handler.TransformOverride = this.transform;
        // handler.enabled = true;
        interface_state = new_state;
    }
    
    public void NotifyPrinterUpdate()
    {
        onAttachedPrinterUpdated?.Invoke();
    }

    public void NotifyBedLevelingComplete(float front_left, float front_right, float back_left, float back_right)
    {
        onBedLevelInfoReceived?.Invoke(front_left, front_right, back_left, back_right);
    }
    
    public void Estop()
    {
        farmhand_client.requestEStop(Attached_Printer.name);
    }

    public void CancelPrint()
    {
        farmhand_client.requestCancel(Attached_Printer.name);
    }
    
    public void CancelPrintWithReason(string reason)
    {
        farmhand_client.requestCancelWithReason(Attached_Printer.name, reason);
    }

    public void RetrievePrintables()
    {
        farmhand_client.requestAvailablePrintables(Attached_Printer.name);
    }

    public void NotifyPrintablesLoaded(List<PrintableObject> AvailablePrints)
    {
        onPrintablesLoaded?.Invoke(AvailablePrints);
    }

    public void NotifyDigitalTwinUpdated()
    {
        onAttachedDigitalTwinUpdated?.Invoke();
    }

    public void TryPrint(string gcode_name)
    {
        farmhand_client.requestPrint(Attached_Printer.name, gcode_name);
    }

    public void UpdatePrinterDigitalTwin()
    {
        farmhand_client.requestPrinterStateUpload(Attached_Printer);
    }

    public void RefreshPrinterDigitalTwin()
    {
        farmhand_client.requestPrinterStateRefresh(Attached_Printer.name);
    }

    public void ReloadPage()
    {
        //Workaround for now due to some mismatches when directly setting state with Update_Interface_State
        Reverse_Interface();
        
        Advance_Interface();
    }
}
