using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public void OnShow()
    {
        Debug.Log("Dialog was showed!");
    }

    public void OnLabelReached()
    {
        var dialog = GetComponent<WDialog>();
        Debug.Log(dialog.GetCurrentLabel() + " was reached!");
    }

    public void OnHide()
    {
        Debug.Log("Dialog was hidden!");
    }

    void Update()
    {
        var dialog = GetComponent<WDialog>();

        if (Input.GetKeyUp(KeyCode.D))
        {
            if (!dialog.IsVisible())
            {
                dialog.Show();
            }
        }
    }
}
