using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FarmhandStuff;
using JetBrains.Annotations;
using Microsoft.MixedReality.SampleQRCodes;
using Microsoft.MixedReality.Toolkit.UX;
using UnityEngine;
using NativeWebSocket;
using Unity.VisualScripting;


public class FarmhandManager : MonoBehaviour
{
    [SerializeField] private string server_address_port;

    //Note: If we want to handle more than one websocket instance at a time, we will need to do some modification to this library.
    public WebSocket FarmhandWebsocket;

    [SerializeField] public QRCodesManager codewatcher;
    //Ideally this would be a list of actual printer objects, but the HoloLens does *not* need any extra memory usage right now
    private List<string> discoveredDevices = new List<string>();
    public QRCodesVisualizer QRHandler;

    [SerializeField] private GameObject GraphVisPrefab;
    public List<string> DiscoveredDevices => discoveredDevices;

    public DialogPool dialogPool;
    
    public List<Printer> current_printers = new List<Printer>();

    public List<PrinterAnchor> current_interfaces = new List<PrinterAnchor>();

    
    // Start is called before the first frame update
    async void Start()
    {
        dialogPool = this.GetComponent<DialogPool>();

        if (server_address_port == null)
        {
            Debug.LogError("No server address provided!");
        }

        FarmhandWebsocket = new WebSocket(server_address_port);

        FarmhandWebsocket.OnOpen += () => Debug.Log("Connection Opened!");

        FarmhandWebsocket.OnError += FarmhandWebsocketOnError;

        FarmhandWebsocket.OnMessage += FarmhandWebsocketOnMessage;
        

    await FarmhandWebsocket.Connect();
    }

    [CanBeNull]
    public PrinterAnchor GetAnchorByName(string printer_name)
    {
        return current_interfaces.Where((pa) => { return pa.qr_code.Data.Equals(printer_name); }).FirstOrDefault();
    }
    
    private async void FarmhandWebsocketOnMessage(byte[] data)
    {
        string[] command = System.Text.Encoding.UTF8.GetString(data).Split("~");
        Debug.Log("New message: " + System.Text.Encoding.UTF8.GetString(data));

        switch (command[0])
        {
            case "error":
            {
                Debug.LogError(string.Join('~', command));
                switch (command[1])
                {
                    case "unrecognized":
                    {
                        PrinterAnchor invalid_anchor = GetAnchorByName(command[2]);
                        if (invalid_anchor != null)
                        {
                            Debug.LogWarning("Letting the anchor know that it refers to an invalid machine...");
                            invalid_anchor.Confirm_Machine(null);
                        }

                        break;
                    }
                }
            } 
                break;
            
            case "machine_confirmed":
            {
                PrinterAnchor anchor = GetAnchorByName(command[1]);
                if (anchor != null)
                {
                    Printer new_printer = new Printer(command[1]);
                    current_printers.Add(new_printer);
                    anchor.Confirm_Machine(new_printer);
                }
                else
                {
                    Debug.LogError("Received machine_confirmed message for a machine without a matching PrinterAnchor qr code!");
                }
            }
                break;
            
            case "stat_update":
            {
                Printer target = current_printers.Where((machine) => { return machine.name.Equals(command[1]); }).FirstOrDefault();
                if (target != null)
                {
                    target.current_temp_bed = float.Parse(command[2]);
                    target.current_temp_extruder = float.Parse(command[3]);
                    target.current_extruder_fan_power = float.Parse(command[4]);
                    target.current_state = command[5];
                    PrinterAnchor current_anchor = GetAnchorByName(command[1]);
                    if (current_anchor != null)
                    {
                        //Anything attached to the anchor can subscribe for updates, eg a status window
                        current_anchor.NotifyPrinterUpdate();
                    }    
                }
                else
                {
                    Debug.LogError("Received a stat_update for a printer which we never discovered: " + command[1]);
                }
            }
                break;
            
            case "notification":
            {
                await showNotification(command[1], command[2], ((command.Length == 4) ? command[3] : "Ok, got it!"));
            }
                break;
            
            case "vis_data":
            {
                var qrobj = GetAnchorByName(command[1]);
                if (qrobj != null)
                {
                    //Now we have a position in which to spawn the log, time to process the data
                    VisualizerController vis_graph = Instantiate(GraphVisPrefab, qrobj.transform).GetComponent<VisualizerController>();
                    // vis_graph.gameObject.transform.position = qrobj.transform.position;
                    // vis_graph.transform.rotation = Quaternion.identity;
                    vis_graph.populate(command);
                }
                else
                {
                    Debug.Log("Received a vis_data for a printer anchor " + command[1] + " that we do not have!");
                }
            }
                break;
            
            case "level_info":
            {
                PrinterAnchor anchor = GetAnchorByName(command[1]);
                if (anchor != null)
                {
                    anchor.NotifyBedLevelingComplete(float.Parse(command[2]), float.Parse(command[3]), float.Parse(command[4]), float.Parse(command[5]));
                }
                else
                {
                    Debug.LogError("Received level_info message for a machine without a matching PrinterAnchor!");
                }
            }
                break;
            
            case "available_printables":
            {
                Printer target = current_printers.Where((machine) => { return machine.name.Equals(command[1]); }).FirstOrDefault();
                if (target != null)
                {
                    target.available_prints.Clear();
                    for (int i = 2; i < command.Length; i++)
                    {
                        target.available_prints.Add(new PrintableObject(command[i]));
                    }

                    PrinterAnchor anchor = GetAnchorByName(command[1]);
                    if (anchor != null)
                    {
                        anchor.NotifyPrintablesLoaded(target.available_prints);
                    }
                }
            }
                break;
            
            case "login_state":
            {
                Debug.LogWarning(System.Text.Encoding.UTF8.GetString(data));
                PrinterAnchor anchor = GetAnchorByName(command[1]);
                if (anchor != null)
                {
                    anchor.Update_Interface_State((PrinterAnchor.PRINTER_UI_STATE) (Int32.Parse(command[3])));
                }
                else
                {
                    Debug.LogError("Received login_state message for a machine without a matching PrinterAnchor qr code!");
                }
            }
                break;

            case "digitaltwin":
            {
                Printer target = current_printers.Where((machine) => { return machine.name.Equals(command[1]); })
                    .FirstOrDefault();
                if (target != null)
                {
                    target.update_printer(command[2]);
                    PrinterAnchor anchor  = GetAnchorByName(target.name);
                    if (anchor != null)
                    {
                        anchor.NotifyDigitalTwinUpdated();
                    }
                }
            }
                break;
        }
    }
    
    private async void FarmhandWebsocketOnError(string errormsg)
    {
        Debug.LogError(errormsg);
    }
    
    // Update is called once per frame
    void Update()
    {
        #if !UNITY_WEBGL || UNITY_EDITOR
            FarmhandWebsocket.DispatchMessageQueue();
        #endif
    }

    private void OnDestroy()
    {
        FarmhandWebsocket.Close();
    }

    public async Task<DialogDismissedEventArgs> showNotification(string header, string body, string button_text)
    {
        return await dialogPool.Get()
            .SetHeader(header)
            .SetBody(body)
            .SetNeutral(button_text)
            .ShowAsync();
    }
    
    //TODO: Need to implement rancher side with filament/digital twin state etc
    public void requestPrinterStateUpload(Printer printer)
    {
        Task.Run(async () => await FarmhandWebsocket.SendText("upload_state~" + printer.name + "~" + JsonUtility.ToJson(printer)));
    }
    
    public void requestPrinterStateRefresh(string printer_name)
    {
        Task.Run(async () => await FarmhandWebsocket.SendText("request_digitaltwin~" + printer_name));
    }
    
    public void requestPrint(string printer_name, string gcode_name)
    {
        Task.Run(async () => await FarmhandWebsocket.SendText("request_print~" + printer_name + "~" + gcode_name));
    }
    public void requestPrint(string printer_name, PrintableObject target_print)
    {
        Task.Run(async () => await FarmhandWebsocket.SendText("request_print~" + printer_name + "~" + target_print.Filename));
    }
    
    public void requestPausePlay(string printer_name)
    {
        Task.Run(async () => await FarmhandWebsocket.SendText("toggle_printing~" + printer_name));
    }

    public void request_loginstate(string printer_name)
    {
        Task.Run(async () => await FarmhandWebsocket.SendText("login~" + printer_name));
    }
    
    public void requestEStop(string printer_name)
    {
        Task.Run(async () => await FarmhandWebsocket.SendText("estop~" + printer_name));
    }
    
    public void requestCancel(string printer_name)
    {
        Task.Run(async () => await FarmhandWebsocket.SendText("cancel_print~" + printer_name));
    }
    
    public void requestPrinterInterfaceReverse(string printer_name)
    {
        Task.Run(async () => await FarmhandWebsocket.SendText("reverse~" + printer_name));
    }
    
    public void requestPrinterInterfaceAdvance(string printer_name)
    {
        Task.Run(async () => await FarmhandWebsocket.SendText("advance~" + printer_name));
    }
    
    public void requestPrinterBedLevel(string printer_name)
    {
        Task.Run(async () => await FarmhandWebsocket.SendText("start_leveling~" + printer_name));
    }
    
    public void requestAvailablePrintables(string printer_name)
    {
        Task.Run(async () => await FarmhandWebsocket.SendText("retrieve_printables~" + printer_name));
    }

    public void DiscoverMachine(string qrCodeData, PrinterAnchor new_anchor)
    {
        FarmhandWebsocket.SendText("discovered_machine~" + qrCodeData);
        current_interfaces.Add(new_anchor);
        
    }


    public void requestCancelWithReason(string printer_name, string reason)
    {
        Task.Run(async () => await FarmhandWebsocket.SendText("cancel_print~" + printer_name + "~" + reason));
    }
}
