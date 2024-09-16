using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

        //界面 
        PanelManager.Init();
        PanelManager.Open<LoginPanel>();

        PanelManager.Open<TipPanel>("用户名或密码错误！");



        //GameObject tankObj = new GameObject("myTank");
        //CtrlTank ctrlTank = tankObj.AddComponent<CtrlTank>();
        //ctrlTank.Init("tankPrefab");
        //tankObj.AddComponent<CameraFollow>();


        ////被打的坦克 
        //GameObject tankObj2 = new GameObject("enemyTank");
        //BaseTank baseTank = tankObj2.AddComponent<BaseTank>();
        //baseTank.Init("tankPrefab");
        //baseTank.transform.position = new Vector3(0, 10, 30);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
