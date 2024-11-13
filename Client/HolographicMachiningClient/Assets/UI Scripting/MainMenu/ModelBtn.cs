using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.UX;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

/* Hopefully the creation of these objects can eventually take care of the entire process of adding an STL to the program
 I.E: Converting to prefab obj, taking thumbnail, setting text of button, etc.*/
public class ModelBtn : PressableButton
{
    private GameObject ModelPreview;

    private MainMenuController MenuController;

    [SerializeField] private TextMeshProUGUI labeltext;
    
    public GameObject Model
    {
        get => ModelPreview;
        set => ModelPreview = value;
    }

    private string gcode_filename;

    public string GcodeFilename
    {
        get => gcode_filename;
        set
        {
            labeltext.text = "<size=8>" + value.Split(".")[0] + "</size>";
            gcode_filename = value;
        }
    }

    void Start()
    {
        this.OnClicked.AddListener(onSelect);
        MenuController = GetComponentInParent<MainMenuController>();
        if (!MenuController) Debug.LogError("No Menu Controller found in Hierarchy! This will break things!");
    }

    void onSelect()
    {
        MenuController.OnModelSelected(this);
    }
}
