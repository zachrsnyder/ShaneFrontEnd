using System;
using System.Collections;
using System.Collections.Generic;
using FarmhandStuff;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using Microsoft.MixedReality.Toolkit.UX;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.XR.Interaction.Toolkit;

public class PrinterConfigMenuController : MonoBehaviour
{
    [SerializeField] private PressableButton continueBtn;
    [SerializeField] private PressableButton pinBtn;

    [SerializeField] private MRTKUGUIInputField bed_temp_input;
    [SerializeField] private MRTKUGUIInputField first_layer_temp_input;
    [SerializeField] private MRTKUGUIInputField printing_temp_input;
    [SerializeField] private MRTKUGUIInputField filament_color_input;
    [SerializeField] private MRTKUGUIInputField material_name_input;
    [SerializeField] private MRTKUGUIInputField filament_nozzle_size_input;
    [SerializeField] private MRTKUGUIInputField equipped_nozzle_size_input;
    [SerializeField] private PressableButton filament_hardened_nozzle_toggle;
    [SerializeField] private PressableButton printer_hardened_nozzle_toggle;

    private PrinterAnchor my_anchor;
    private FarmhandManager client;
    
    // Start is called before the first frame update
    void Start()
    {
        my_anchor = GetComponentInParent<PrinterAnchor>();
        if (my_anchor == null)
        {
            Debug.LogError("NO PRINTER ANCHOR FOUND");
        }

        client = my_anchor.farmhand_client;
        my_anchor.RefreshPrinterDigitalTwin();
        
        my_anchor.onAttachedDigitalTwinUpdated += update_menu_items;
    }
    
    //TODO: Possibly add refresh button so user can call update_menu_items manually
    private void update_menu_items()
    {
        bed_temp_input.text = my_anchor.Attached_Printer.Current_Filament.Bed_Temp.ToString();
        first_layer_temp_input.text = my_anchor.Attached_Printer.Current_Filament.First_Layer_Temp.ToString();
        printing_temp_input.text = my_anchor.Attached_Printer.Current_Filament.Printing_Temp.ToString();
        filament_color_input.text = my_anchor.Attached_Printer.Current_Filament.Color;
        material_name_input.text = my_anchor.Attached_Printer.Current_Filament.Material_Name;
        filament_nozzle_size_input.text = my_anchor.Attached_Printer.Current_Filament.Required_Nozzle_Size.ToString();

        equipped_nozzle_size_input.text = my_anchor.Attached_Printer.Nozzle_Size.ToString();
        printer_hardened_nozzle_toggle.IsToggled.Initialize(my_anchor.Attached_Printer.Has_Hardened_Nozzle);
        filament_hardened_nozzle_toggle.IsToggled.Initialize(my_anchor.Attached_Printer.Current_Filament.Needs_Hardened_Nozzle);
        
        
        //If we have printer config info, the user should be allowed to continue:
        continueBtn.enabled = true;
        
    }
    
    private void onPrinterSwitch()
    {
        Destroy(this.gameObject);
    }

    public void displayHelpDialog()
    {
        client.showNotification(
            "Help",
            "This menu allows you to update any of the information about your printer or filament in case it has changed since the last use! If nothing has changed, or you aren't sure what these settings even mean, you can just leave them at what was previously set and hit continue!",
            "Ok, got it!"
            );
    }

    public void OnRefreshButtonSelected()
    {
        my_anchor.RefreshPrinterDigitalTwin();
    }
    
    public void OnBackButtonSelected()
    {
        my_anchor.Reverse_Interface();
    }
    
    public void OnContinueBtnSelected(GameObject nextMenu)
    {
        //Check locally for conflicts and alert the user if they cannot continue:
        if (equipped_nozzle_size_input.text != filament_nozzle_size_input.text)
        {
            client.showNotification(
                "Bad configuration!",
                "The nozzle size that you have equipped on your printer does not match the size requested by your current filament! This must be fixed in order to prevent printer damage!",
                "Ok, got it!"
                );
            return;
        }

        else if (filament_hardened_nozzle_toggle.IsToggled.Active && !(printer_hardened_nozzle_toggle.IsToggled.Active))
        {
            client.showNotification(
                "Bad configuration!",
                "The type of nozzle that you have equipped on your printer is not compatible with your abrasive filament! This must be fixed in order to prevent printer damage!",
                "Ok, got it!"
            );
            return;
        }
        
        else
        {
            if (bed_temp_input.text != my_anchor.Attached_Printer.Current_Filament.Bed_Temp.ToString())
            {
                my_anchor.Attached_Printer.Current_Filament.Bed_Temp = float.Parse(bed_temp_input.text);
            }
            if (first_layer_temp_input.text != my_anchor.Attached_Printer.Current_Filament.First_Layer_Temp.ToString())
            {
                my_anchor.Attached_Printer.Current_Filament.First_Layer_Temp = float.Parse(first_layer_temp_input.text);
            }
            if (printing_temp_input.text != my_anchor.Attached_Printer.Current_Filament.Printing_Temp.ToString())
            {
                my_anchor.Attached_Printer.Current_Filament.Printing_Temp = float.Parse(printing_temp_input.text);
            }
            if (filament_color_input.text != my_anchor.Attached_Printer.Current_Filament.Color)
            {
                my_anchor.Attached_Printer.Current_Filament.Color = filament_color_input.text;
            }
            if (material_name_input.text != my_anchor.Attached_Printer.Current_Filament.Material_Name)
            {
                my_anchor.Attached_Printer.Current_Filament.Material_Name = material_name_input.text;
            }
            if (filament_nozzle_size_input.text != my_anchor.Attached_Printer.Current_Filament.Required_Nozzle_Size.ToString())
            {
                my_anchor.Attached_Printer.Current_Filament.Required_Nozzle_Size = float.Parse(filament_nozzle_size_input.text);
            }
            if (equipped_nozzle_size_input.text != my_anchor.Attached_Printer.Nozzle_Size.ToString())
            {
                my_anchor.Attached_Printer.Nozzle_Size = float.Parse(equipped_nozzle_size_input.text);
            }

            if (printer_hardened_nozzle_toggle.IsToggled.Active != my_anchor.Attached_Printer.Has_Hardened_Nozzle)
            {
                my_anchor.Attached_Printer.Has_Hardened_Nozzle = printer_hardened_nozzle_toggle.IsToggled.Active;
            }

            if (filament_hardened_nozzle_toggle.IsToggled.Active !=
                my_anchor.Attached_Printer.Current_Filament.Needs_Hardened_Nozzle)
            {
                my_anchor.Attached_Printer.Current_Filament.Needs_Hardened_Nozzle =
                    filament_hardened_nozzle_toggle.IsToggled.Active;
            }

            
            //Send command containing all of the new printer info to the farmhand server
            my_anchor.UpdatePrinterDigitalTwin();
            my_anchor.Advance_Interface();
        }
        
    }

    public void OnDestroy()
    {
        my_anchor.onAttachedDigitalTwinUpdated -= update_menu_items;
    }
}
