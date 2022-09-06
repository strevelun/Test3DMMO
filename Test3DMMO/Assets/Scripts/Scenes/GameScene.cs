using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameScene : BaseScene
{
    public UI_GameScene SceneUI { get; private set; }


    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Game;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Locked;

        Screen.SetResolution(640, 480, false);

        SceneUI = Managers.UI.ShowSceneUI<UI_GameScene>();
    }



    public override void Clear()
    {
        
    }
}
