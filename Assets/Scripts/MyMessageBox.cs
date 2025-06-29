using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MyMessageBox : MonoBehaviour
{
    public GameObject dialogBox; // 对话框 GameObject
    public TMP_Text messageText;    // 消息文本
    public Button okButton;     // 确定按钮
    private System.Action onOkCallback;

    void Start()
    {
        dialogBox.SetActive(false); // 默认隐藏
        okButton.onClick.AddListener(OnOK);
    }

    // 显示对话框
    public void Show(string message, System.Action callback = null)
    {
        messageText.text = message;
        onOkCallback = callback;
        dialogBox.SetActive(true);
    }

    // 确定按钮回调
    private void OnOK()
    {
        dialogBox.SetActive(false);
        Debug.Log("用户点击了确定");
        onOkCallback?.Invoke();
    }
}
