using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MenuNavigation : MonoBehaviour
{
    [SerializeField] private GameObject previousMenu;

    [SerializeField] private GameObject nextMenu;

    public void OnNextBtnSelected()
    {
        Instantiate(nextMenu);
        Destroy(this.gameObject);
    }
    
    public void OnPreviousBtnSelected()
    {
        Instantiate(previousMenu);
        var client = FindObjectOfType<MoonrakerClient>();
        if (client == null)
        {
            Debug.LogError("NO MOONRAKER CLIENT FOUND!");
        }
        else
        {
            if (client.printer.currentGCodeName != null)
            {
                Task.Run(client.CancelPrint);
            }
        }
        Destroy(this.gameObject);
    }
    
    // Start is called before the first frame update
    void Start()
    {
        this.GetComponent<Canvas>().worldCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
