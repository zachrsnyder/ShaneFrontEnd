using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;

public class PrintjobControl : MonoBehaviour
{
    [SerializeField] private MoonrakerClient client;
    [SerializeField] private GameObject PreviewParent;
    [SerializeField] private TextMeshProUGUI displayText;
    private bool IsPaused = false;
    
    // Start is called before the first frame update
    void Start()
    {
        client = FindObjectOfType<MoonrakerClient>();
        if (client == null)
        {
            Debug.LogError("NO MOONRAKER CLIENT FOUND!");
        }

        if (client.printer.currentPreview != null)
        {
            Instantiate(client.printer.currentPreview, PreviewParent.transform);
        }

        if (client.printer.currentGCodeName != null)
        {
            Task.Run(async () => await client.StartPrint(client.printer.currentGCodeName));
        }
    }

    public void TogglePrint()
    {
        if (IsPaused)
        {
            Task.Run(client.ResumePrint);
            IsPaused = false;
            displayText.text = "Pause Print";

        }
        else
        {
            Task.Run(client.PausePrint);
            IsPaused = true;
            displayText.text = "Resume Print";
        }
        
    }

    public void OnCancelBtnSelected()
    {
        try
        {
            Destroy(PreviewParent.transform.GetChild(0).gameObject);
            Task.Run(client.CancelPrint);
            IsPaused = true;
        }
        catch (Exception e)
        {
            Debug.Log("No print preview to cancel currently." + e.ToString());
        }
        
    }

    public void OnESTOPBtnSelected()
    {
        Task.Run(client.EmergencyStop);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
