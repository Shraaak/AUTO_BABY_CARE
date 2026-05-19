using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class ResultManager : MonoBehaviour
{
    public static ResultManager Instance { get; private set; }

    [Header("游戏时长配置")]
    [Tooltip("秒为单位")]
    public float gameDuration = 300f;
    private float remainingTime;

    [Header("健康值区间配置")]
    [Tooltip("<多少为health")]
    public float healthyThreshold = 150f;
    public float goodThreshold = 250f;
    // 差：总和 ≥ 250

    [Header("收入记录")]
    private int finalIncome;

    [Header("结果UI")]
    public TextMeshProUGUI healthText; // 显示健康值文本
    public TextMeshProUGUI timeLeftText; // 显示剩余时间文本
    public TextMeshProUGUI inComeText; // 显示结算结果文本
    public GameObject resultPanel; // 结算面板

    private Baby baby;
    private bool isGameOver; // 游戏是否结束


    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        resultPanel.SetActive(false);
        baby = Baby.Instance;

        // 初始化计时
        remainingTime = gameDuration;
        isGameOver = false;
    }

    void Update()
    {
        if (isGameOver || baby == null) return;

        // 1. 更新剩余时间
        UpdateGameTime();

        // 3. 检测是否进入生病状态（游戏结束）
        CheckSickState();
    }


    /// <summary>
    /// 更新游戏剩余时间
    /// </summary>
    private void UpdateGameTime()
    {
        remainingTime -= Time.deltaTime;
        remainingTime = Mathf.Max(0, remainingTime);

        // 格式化时间显示（分:秒）
        TimeSpan timeSpan = TimeSpan.FromSeconds(remainingTime);
        timeLeftText.text = $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";

        // 时间到且未生病 → 正常结算
        if (remainingTime <= 0)
        {
            GameOver(false);
        }
    }

    /// <summary>
    /// 计算并更新健康值（hunger + sleepy + boredom）
    /// </summary>
    private void UpdateHealthValue()
    {
        float totalHealth = baby.hunger + baby.sleepy + baby.boredom;
        string healthLevel = GetHealthLevel(totalHealth);

        // 更新UI显示
        healthText.text = $"宝宝的健康评价：\n{healthLevel}";
    }

    /// <summary>
    /// 检测是否进入生病状态
    /// </summary>
    private void CheckSickState()
    {
        if (baby.stateMechine.currentState == baby.sickState)
        {
            GameOver(true);
        }
    }

    private void GameOver(bool isSick)
    {
        isGameOver = true;
        resultPanel.SetActive(true);

        if (isSick)
        {
            // 生病结束 → 游戏失败，无收入
            finalIncome = 0;
            healthText.text = $"宝宝的健康评价：\n生病";
        }
        else
        {
            // 时间到结束 → 正常结算收入
            
            finalIncome = MainCanvasUI.Instance.currentMoney;
            //计算并更新健康值
            UpdateHealthValue();
        }
        inComeText.text = $"今日收入：\n" + finalIncome.ToString();

        Time.timeScale = 0;
    }

    /// <summary>
    /// 根据健康值总和判定等级
    /// </summary>
    /// <param name="totalHealth">hunger+sleepy+boredom总和</param>
    /// <returns>健康/良好/差</returns>
    private string GetHealthLevel(float totalHealth)
    {
        if (totalHealth < healthyThreshold)
        {
            return "健康";
        }
        else if (totalHealth < goodThreshold)
        {
            return "良好";
        }
        else
        {
            return "差";
        }
    }
}