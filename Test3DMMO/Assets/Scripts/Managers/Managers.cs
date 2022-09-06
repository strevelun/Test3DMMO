using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Managers : MonoBehaviour
{
    static Managers s_inst;
    static Managers Inst { get { Init(); return s_inst; } }

    #region Contents
    InputManager _input = new InputManager();
    ObjectManager _obj = new ObjectManager();
    NetworkManager _network = new NetworkManager();

    public static InputManager Input { get { return Inst._input; } }
    public static ObjectManager Object { get { return Inst._obj; } }
    public static NetworkManager Network { get { return Inst._network; } }
    #endregion

    #region Core
    ResourceManager _resource = new ResourceManager();
    DataManager _data = new DataManager();
    SceneManagerEx _scene = new SceneManagerEx();
    UIManager _ui = new UIManager();

    public static ResourceManager Resource { get { return Inst._resource; } }
    public static DataManager Data { get { return Inst._data; } }
    public static SceneManagerEx Scene { get { return Inst._scene; } }
    public static UIManager UI { get { return Inst._ui; } }
    #endregion

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        //_input.OnUpdate();
        Network.Update();

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


            s_inst._network.Init();
            Data.Init();
        }
    }

    public static void Clear()
    {
        //Scene.Clear();
        //UI.Clear();
    }
}
