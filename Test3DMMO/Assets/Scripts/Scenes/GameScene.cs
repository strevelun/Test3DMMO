using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameScene : BaseScene
{
    //UI_GameScene _sceneUI;
    public Text _debugText = GameObject.Find("Debug").transform.GetChild(0).GetComponent<Text>();

    public void Log(string str)
    {
        _debugText.text = str;
    }

    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Game;

        Cursor.visible = true;

        Screen.SetResolution(640, 480, false);
    }

    public override void Clear()
    {
        
    }
}
