using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Managers : MonoBehaviour
{
    static Managers s_inst;
    static Managers Inst { get { Init(); return s_inst; } }

    #region
    InputManager _input = new InputManager();

    #endregion

    public static InputManager Input { get { return Inst._input; } }

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        //_input.OnUpdate();

    }

    static void Init()
    {
        if (s_inst == null)
        {
            GameObject go = GameObject.Find("@Managers");
            if (go == null)
            {
                go = new GameObject { name = "@Managers" };
                go.AddComponent<Managers>();
            }

            DontDestroyOnLoad(go);
            s_inst = go.GetComponent<Managers>();
        }
    }
}
