using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShapePanelOpener : MonoBehaviour
{
    public GameObject ShapePanel;
    public int shapeIndex;

    public void TogglePanel()
    {
        if(ShapePanel != null)
        {
            ShapePanelManager spm = ShapePanel.GetComponent<ShapePanelManager>();
            bool isActive = ShapePanel.activeSelf;

            if(!isActive)
            {
                ShapePanel.SetActive(!isActive);
                spm.SetData(shapeIndex);
            }
            else
            {
                if(spm.Index != shapeIndex)
                {
                    spm.SetData(shapeIndex);
                    isActive = false;
                }
                ShapePanel.SetActive(!isActive);
            }
        }
    }
}
