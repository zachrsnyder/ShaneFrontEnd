using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

//TODO: Parameterize visualizations so new variables can easily be added
public class VisualizerController : MonoBehaviour
{
    [SerializeField] private HolographGraphController extruderTempGraph;
    [SerializeField] private HolographGraphController bedTempGraph;
    [SerializeField] private HolographGraphController fanPowerGraph;
    [SerializeField] private HolographGraphController velocityGraph;

    [SerializeField] private TextMeshProUGUI graphTitle;

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
    }

    public void OnFailureClassified(string reason)
    {
        my_anchor.CancelPrintWithReason(reason);
        Destroy(this.gameObject);
    }

    public void onCloseBtn()
    {
        Destroy(this.gameObject);
    }
    
    public void populate(string[] command)
    {
        graphTitle.text = "Visualization\n" + command[1];
        int num_pnts = command.Length - 2;
        extruderTempGraph.numPoints = num_pnts;
        bedTempGraph.numPoints = num_pnts;
        fanPowerGraph.numPoints = num_pnts;
        velocityGraph.numPoints = num_pnts;
        extruderTempGraph.alloc_pointpool();
        bedTempGraph.alloc_pointpool();
        fanPowerGraph.alloc_pointpool();
        velocityGraph.alloc_pointpool();
        for (int i = 2; i < command.Length; i++)
        {
            string[] nums = command[i].Split(",");
            bedTempGraph.addPoint(Mathf.Round(float.Parse(nums[0])));
            extruderTempGraph.addPoint(Mathf.Round(float.Parse(nums[1])));
            fanPowerGraph.addPoint(Mathf.Round(float.Parse(nums[2]) * 100f));
            //velocityGraph.addPoint(Mathf.Round(float.Parse(nums[3])));
        }
    }
}
