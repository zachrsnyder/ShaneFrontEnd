using System;
using System.Collections;
using System.Collections.Generic;
using FarmhandStuff;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using Microsoft.MixedReality.Toolkit.UX;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ModelSelectionController : MonoBehaviour
{
    /*
    [SerializeField] private GameObject PreviewParent;
    [SerializeField] private PressableButton continueBtn;
    [SerializeField] private PressableButton pinBtn;
    
    private GameObject currentPreviewModel;
    private ModelBtn currentSelection;
    
    [SerializeField] private FarmhandManager client;
    
    // Start is called before the first frame update
    void Start()
    {
        client = FindObjectOfType<FarmhandManager>();
        if (client == null)
        {
            Debug.LogError("NO FARMHAND CLIENT FOUND!");
        }
        client.requestAvailablePrintables();
        //client.onModelsLoaded += onModelsLoaded;
        continueBtn.enabled = false;
    }

    private void onModelsLoaded(PrintableObject[] discoveredmodels)
    {
        throw new System.NotImplementedException();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // public void ResetModel()
    // {
    //     if (currentSelection && currentPreviewModel)
    //     {
    //         OnModelSelected(currentSelection);
    //     }
    //
    //     else
    //     {
    //         Debug.Log("Tried to respawn model without there already being one to respawn.");
    //     }
    // }
    
    // public void OnModelSelected(ModelBtn selectedBtn)
    // {
    //     //Did this so that we have a reference to the current STL in the menu's state
    //     currentSelection = selectedBtn;
    //     client.printer.currentPreview = selectedBtn.Model;
    //     client.printer.currentGCodeName = selectedBtn.gcode_filename;
    //     if (currentPreviewModel)
    //     {
    //         Destroy(currentPreviewModel);
    //     }
    //     
    //     currentPreviewModel = Instantiate(selectedBtn.Model, PreviewParent.transform);
    //     
    //     //The next line is necessary due to bug where gameobjects do not always follow canvases they are childed to
    //     currentPreviewModel.GetComponent<BoundsControl>().ManipulationStarted
    //         .AddListener(arg0 => pinBtn.ForceSetToggled(true));
    //     
    //     continueBtn.enabled = true;
    // }

    private void OnDestroy()
    {
        //client.onModelsLoaded -= onModelsLoaded;
    }

    public void OnContinueBtnSelected(GameObject nextMenu)
    {
        //Home all axes
        Instantiate(nextMenu);
        Destroy(this.gameObject);
    }
    */
}
