using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapePanelManager : MonoBehaviour
{

    GameObject Panel;
    public int Index { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        Panel = this.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void SetData(int index)
    {
        Index = index;
        // Устанавливаем параметры формы
    }

    
}
