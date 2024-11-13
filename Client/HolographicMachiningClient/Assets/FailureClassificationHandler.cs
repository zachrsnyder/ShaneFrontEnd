using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FailureClassificationHandler : MonoBehaviour
{
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
}
