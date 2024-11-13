using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.UX;
using TMPro;
using UnityEngine;

public class BedLevelWizard : MonoBehaviour
{
    [SerializeField] private PressableButton continueBtn;
    [SerializeField] private TextMeshProUGUI wizardText;
    
    
    //This should be a prefab, with a child above each screw (labeled appropriately, ie: front_left, front_right, back_left, back_right)
    [SerializeField] private GameObject bed_prefab;
    [SerializeField] private Transform bed_diagram_target_location;
    
    private int step = 0;
    private PrinterAnchor my_anchor;
    private FarmhandManager client;
    
    //TODO: ADD WAY TO RESTART/SKIP BED LEVELING IF IT TIMES OUT
    
    // Start is called before the first frame update
    void Start()
    {
        my_anchor = GetComponentInParent<PrinterAnchor>();
        if (my_anchor == null)
        {
            Debug.LogError("NO PRINTER ANCHOR FOUND");
        }

        client = my_anchor.farmhand_client;
        
    }

    //Continue btn will be tied to this function so that it can go to the next step in leveling whenever it is pressed.
    public void advanceWizard()
    {
        if (step == 0)
        {
            my_anchor.BeginLeveling();
            my_anchor.onBedLevelInfoReceived += displayBedLevelInfo;
            continueBtn.enabled = false;
            wizardText.text =
                "Please wait patiently for the bed leveling measurements to be taken. When they are finished, a diagram will appear along with the amounts by which each screw needs to be turned.";
            
        }

        if (step == 1)
        {
            my_anchor.Advance_Interface();
        }
        
    }
    
    public void OnRefreshButtonSelected()
    {
        my_anchor.ReloadPage();
    }
    
    public void OnBackButtonSelected()
    {
        my_anchor.Reverse_Interface();
    }

    private void displayBedLevelInfo(float front_left, float front_right, float back_left, float back_right)
    {
        step = 1;
        // client.onBedLevelInfoReceived -= displayBedLevelInfo;
        BedLevelDisplayController controller = Instantiate(bed_prefab, bed_diagram_target_location).GetComponent<BedLevelDisplayController>();
        
        controller.front_left_val = front_left;
        controller.front_right_val = front_right;
        controller.back_left_val = back_left;
        controller.back_right_val = back_right;

        wizardText.text =
            "Please adjust each of the bed screws by the amounts directed. When you are done, press continue!";
        continueBtn.enabled = true;
    }
    
    public void displayHelpDialog()
    {
        client.showNotification("Help", "Bed Leveling is the important process of making sure that the extruder is the same distance away from the bed, no matter what part of the bed it is over! If the bed is not level to the extruder, poor quality and print failures are almost always a consequence.", "Ok, got it!");
    }

    private void OnDestroy()
    {
        my_anchor.onBedLevelInfoReceived -= displayBedLevelInfo;
    }

}
