using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class BabyUIManager : MonoBehaviour
{
    #region UI 组件
    [Header("UI组件")]
    [Tooltip("冒泡UI")]
    public GameObject bubbleUI;
    public Image bubbleImage;  

    [Header("状态图片配置")]
    public Sprite hungerBubble;         
    public Sprite sleepyBubble;         
    public Sprite boredBubble;          
    public Sprite poopBubble;           
    public Sprite sickBubble; 

    [Header("对话面板")]
    public GameObject chatPanel;
    [Tooltip("玩家输入框")]
    public TMP_InputField userInput; 
    [Tooltip("发送按钮")]
    public Button sendBtn;   

    [Header("婴儿说话面板")]   
    public GameObject talkUI;
    [Tooltip("婴儿（LLM）的对话文字")]
    public TextMeshProUGUI babyChatText; 
    [Tooltip("关联Baby实例")]
    public Baby baby;
    [Tooltip("冒泡几秒后隐藏")]
    public float hideTime = 5f;
    #endregion

    void Start()
    {
        // 订阅Baby的事件
        baby.OnNeedInteract += ShowBubbleUI;
        baby.OnLLMMessage += HandleLLMMessage;

        // 绑定按钮事件
        sendBtn.onClick.AddListener(SendMsg);

        // 初始隐藏UI
        bubbleUI.SetActive(false);
        talkUI.SetActive(false);
        chatPanel.SetActive(false);

        
    }

    /// <summary>
    /// 显示冒泡UI
    /// </summary>
    /// <param name="type"></param>
    private void ShowBubbleUI(string type)
    {
        bubbleUI.SetActive(true);
        talkUI.SetActive(false); // 说话UI隐藏
        chatPanel.SetActive(false);
        
        switch (type)
        {
            case "walk":
                print("walk");
                HideBubbleUI();
                break;
            case "hunger":
                print("hunger");
                bubbleImage.sprite = hungerBubble;
                break;
            case "sleepy":
                print("sleepy");
                bubbleImage.sprite = sleepyBubble;
                break;
            case "bored":
                print("bored");
                bubbleImage.sprite = boredBubble;
                break;
            case "poop":
                print("poop");
                bubbleImage.sprite = poopBubble;
                break;
            case "sick":
                print("sick");
                bubbleImage.sprite = sickBubble;
                break;
        }
    }

    /// <summary>
    /// 隐藏冒泡UI
    /// </summary>
    private void HideBubbleUI()
    {
        bubbleUI.SetActive(false);
    }

    private void HandleLLMMessage(string msgType)
    {
        // ============= 对话开启 =============
        if(msgType.StartsWith("dialog_"))
        {
            chatPanel.SetActive(true); // 显示输入框
            talkUI.SetActive(false); // 隐藏说话框
            bubbleUI.SetActive(false); // 隐藏心情冒泡
            userInput.ActivateInputField();
            return;
        }

        // ============= LLM回复婴儿说的话 =============
        talkUI.SetActive(true);
        babyChatText.text = msgType;

        // 自动隐藏
        CancelInvoke(nameof(HideTalkUI));
        Invoke(nameof(HideTalkUI), hideTime);
    }

    private void HideTalkUI()
    {
        talkUI.SetActive(false);
    }

    // 发送玩家输入 → 调用LLM
    private void SendMsg()
    {
        string input = userInput.text.Trim();
        if (string.IsNullOrEmpty(input) || !baby.isDialog) return;

        // 调用LLM
        LLMClient.SendChat(input, baby.currentTalkState, this, (reply, isCorrect) =>
        {
            // 显示宝宝回复
            babyChatText.text = reply;
            talkUI.SetActive(true);
            chatPanel.SetActive(false);

            // 修改宝宝数值
            baby.OnDialogReply(reply, isCorrect);
        });

        userInput.text = "";
    }
}
