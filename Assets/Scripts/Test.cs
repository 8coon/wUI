using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyUp("space"))
        {
            var dialog = GetComponent<WDialog>();

            if (dialog && !dialog.IsVisible())
            {
                dialog.Show();
            }
        }
    }
}
