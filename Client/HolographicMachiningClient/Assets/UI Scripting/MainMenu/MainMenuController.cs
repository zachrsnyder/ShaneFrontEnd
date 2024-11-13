using System;
using System.Collections;
using System.Collections.Generic;
using FarmhandStuff;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using Microsoft.MixedReality.Toolkit.UX;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private GameObject PreviewParent;
    [SerializeField] private PressableButton continueBtn;
    [SerializeField] private PressableButton pinBtn;
    [SerializeField] private GameObject scrollParent;
    [SerializeField] private GameObject modelBtnPrefab;
    
    private GameObject currentPreviewModel;
    private ModelBtn currentSelection;

    private List<ModelBtn> current_buttons;
    
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

        current_buttons = new List<ModelBtn>();
        client = my_anchor.farmhand_client;
        
        continueBtn.enabled = false;
        my_anchor.Attached_Printer.current_gcode_name = String.Empty;
        my_anchor.onPrintablesLoaded += LoadPrintables;

        my_anchor.RetrievePrintables();

    }
    
    private void LoadPrintables(List<PrintableObject> discoveredprintables)
    {
        current_buttons.ForEach(x => Destroy(x.gameObject));
        for (int i = 0; i < discoveredprintables.Count; i++)
        {
            ModelBtn newbtn = Instantiate(modelBtnPrefab, scrollParent.transform).GetComponent<ModelBtn>();
            current_buttons.Add(newbtn);
            newbtn.GcodeFilename = discoveredprintables[i].Filename;
            newbtn.Model = discoveredprintables[i].PreviewModel;
            
        }
    }
    
    public void ResetModel()
    {
        if (currentSelection && currentPreviewModel)
        {
            OnModelSelected(currentSelection);
        }

        else
        {
            Debug.Log("Tried to respawn model without there already being one to respawn.");
        }
    }
    
    public void OnModelSelected(ModelBtn selectedBtn)
    {
        //Did this so that we have a reference to the current STL in the menu's state
        currentSelection = selectedBtn;
        my_anchor.Attached_Printer.current_gcode_name = selectedBtn.GcodeFilename;
        if (currentPreviewModel)
        {
            Destroy(currentPreviewModel);
        }

        if (selectedBtn.Model != null)
        {
            currentPreviewModel = Instantiate(selectedBtn.Model, PreviewParent.transform);

            //The next line is necessary due to bug where gameobjects do not always follow canvases they are childed to
            currentPreviewModel.GetComponent<BoundsControl>().ManipulationStarted
                .AddListener(arg0 => pinBtn.ForceSetToggled(true));
        }

        else
        {
            client.showNotification("No preview available!", "There is no preview available for this file, but you can still print it by selecting continue as normal.", "Ok, got it!");
        }

        continueBtn.enabled = true;
    }
    
    public void OnContinueBtnSelected(GameObject nextMenu)
    {
        continueBtn.enabled = false;
        my_anchor.TryPrint(my_anchor.Attached_Printer.current_gcode_name);
    }
    
    public void OnRefreshButtonSelected()
    {
        my_anchor.RetrievePrintables();
    }
    
    public void OnBackButtonSelected()
    {
        my_anchor.Reverse_Interface();
    }

    private void OnDestroy()
    {
        my_anchor.onPrintablesLoaded -= LoadPrintables;
    }
}
