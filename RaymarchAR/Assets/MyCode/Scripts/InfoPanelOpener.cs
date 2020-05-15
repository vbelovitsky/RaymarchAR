using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoPanelOpener : MonoBehaviour
{
    public GameObject InfoPanel;
    
    public void TogglePanel()
    {
        if(InfoPanel != null)
        {
            InfoPanel.SetActive(!InfoPanel.activeSelf);
        }
    }

}
