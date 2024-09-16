using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TipPanel : BasePanel
{
    //提示文本
    private TextMeshProUGUI text;
    //确定按钮
    private Button okBtn;

    private UnityAction okAction;

    //初始化
    public override void OnInit()
    {
        skinPath = "TipPanel";
        layer = PanelManager.Layer.Tip;
    }

    //显示
    public override void OnShow(params object[] para)
    {
        //寻找组件
        text = skin.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        okBtn = skin.transform.Find("OkBtn").GetComponent<Button>();
        //监听
        okBtn.onClick.AddListener(OnOkClick);
        //提示语
        if(para.Length == 1)
        {
            text.text = (string)para[0];
           
        }
        else if(para.Length == 2)
        {
            text.text = (string)para[0];
            okAction = (UnityAction)para[1];
        }
        
    }

    //关闭 
    public override void OnClose()
    {

    }

    //当按下确定按钮
    private void OnOkClick()
    {
        Close();
        if(okAction != null)
        {
            okAction();
        }
    }
}
