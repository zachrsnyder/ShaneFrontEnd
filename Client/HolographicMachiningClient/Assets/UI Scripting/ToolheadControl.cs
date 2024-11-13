using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.UX;
using TMPro;
using UnityEngine;

public class ToolheadControl : MonoBehaviour
{
    [SerializeField] private MRTKTMPInputField setx;
    [SerializeField] private MRTKTMPInputField sety;
    [SerializeField] private MRTKTMPInputField setz;
    [SerializeField] private MoonrakerClient client;
    
    // Start is called before the first frame update
    private void Start()
    {
        client = FindObjectOfType<MoonrakerClient>();
        if (client == null)
        {
            Debug.LogError("NO MOONRAKER CLIENT FOUND!");
        }
        else
        {
            client.toolhead_moved += ClientOntoolhead_moved;
        }
    }
    

    private void ClientOntoolhead_moved(float[] new_pos)
    {
        
        var placeholder = (TextMeshProUGUI)setx.placeholder;
        placeholder.text = new_pos[0].ToString();
        
        placeholder = (TextMeshProUGUI)sety.placeholder;
        placeholder.text = new_pos[1].ToString();
        
        placeholder = (TextMeshProUGUI)setz.placeholder;
        placeholder.text = new_pos[2].ToString();
    }

    // Update is called once per frame
    void Update()
    {
            
    }
}
