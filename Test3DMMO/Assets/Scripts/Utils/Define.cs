using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Define
{
    public enum Scene
    {
        Unknown,
        Login,
        Lobby,
        Game,
    }

    public enum Layers
    {
        Ground = 6,
        Monster,
    }

    public enum UIEvent
    {
        Click,
        Drag,
    }
}
