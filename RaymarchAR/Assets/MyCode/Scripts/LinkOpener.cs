using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkOpener : MonoBehaviour
{

    public string link = "https://github.com/vbelovitsky/RaymarchAR/tree/master/QR-codes";

    public void OpenLink()
    {
        if(link != null)
        {
            Application.OpenURL(link);
        }
       
    }
}
