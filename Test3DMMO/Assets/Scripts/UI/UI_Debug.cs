using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Debug : UI_Base
{
    GameObject _debug;


    public override void Init()
    {
    }

    public void Log(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        _debug.transform.GetChild(0).GetComponent<Text>().text = text;
    }
}
