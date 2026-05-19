using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RhythmManager : MonoBehaviour
{
    public AudioSource theMusic;
    public bool startPlaying;
    public BeatScoller theBS;
    public static RhythmManager Instance{ get; private set; }

    public int currentScore;
    public int scorePerNote = 100;
    public int scorePerGoodNote = 125;
    public int scorePerPerfectNote = 150;

    public int currentMultiple;// 当前的连击倍数
    public int multipleTracker;// 当前的连击计数器
    public int[] multiplierThresholds;// 升级阈值数组
    public int finalScore = 0;

    public TextMeshProUGUI scoreText;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        scoreText.text = "Score: 0";
        currentMultiple = 1;

        AudioManager.Instance.PlayBGM("BGM");
    }

    void Update()
    {
    }

    public void NoteHit()
    {
        Debug.Log("Hit On Time");

        if(currentMultiple - 1 < multiplierThresholds.Length)
        {

            multipleTracker++;
            if(multiplierThresholds[currentMultiple - 1] <= multipleTracker)
            {
                multipleTracker = 0;
                currentMultiple++;
            }
        }

        scoreText.text = "Score: " + currentScore;
    }

    public void GoodHit()
    {
        currentScore += scorePerGoodNote * currentMultiple;
        NoteHit();
    }

    public void PerfectHit()
    {
        currentScore += scorePerPerfectNote * currentMultiple;
        NoteHit();
    }

    public void NormalHit()
    {
        currentScore += scorePerNote * currentMultiple;
        NoteHit();
    }

    public void NoteMissed()
    {
        Debug.Log("Missed Note");

        currentMultiple = 1;
        multipleTracker = 0;
    }

    
    /// <summary>
    /// 开始游戏：重置状态 + 启动音乐和音符滚动
    /// </summary>
    public void Begin()
    {
        // 重置所有游戏状态（避免上一局残留数据）
        ResetGameState();

        if (!startPlaying)
        {
            startPlaying = true;
            theBS.hasStarted = true;

            // 确保音乐从开头播放
            theMusic.time = 0;
            theMusic.Play();
            
            Debug.Log("Rhythm Game Started!");
        }
    }



    /// <summary>
    /// 结束游戏：停止播放 + 完全重置所有状态
    /// </summary>
    public void Finish()
    {
        if (startPlaying)
        {
            // 停止音乐
            theMusic.Stop();
            
            // 记录最终分数并结算金钱
            finalScore = currentScore;
            if (MainCanvasUI.Instance != null)
            {
                MainCanvasUI.Instance.currentMoney += finalScore / 5;
                Debug.Log($"Game Finished! Final Score: {finalScore}, Added Money: {finalScore / 5}");
            }

            // 完全重置所有游戏状态
            ResetGameState();
            
            Debug.Log("Rhythm Game Reset Complete!");
        }
    }

    private void ResetGameState()
    {
        // 分数重置
        currentScore = 0;
        finalScore = 0;
        scoreText.text = "Score: 0";

        // 连击重置
        currentMultiple = 1;
        multipleTracker = 0;

        // 播放状态
        startPlaying = false;

        if (theBS != null)
        {
            theBS.hasStarted = false;

            // 整个轨道 + 所有音符直接还原位置
            theBS.transform.position = theBS.initialPosition;

            // ========== 新增代码开始 ==========
            // 遍历BeatScoller下所有音符（包含禁用的），重新激活并重置状态
            NoteObject[] allNotes = theBS.GetComponentsInChildren<NoteObject>(true);
            foreach (NoteObject note in allNotes)
            {
                note.gameObject.SetActive(true); // 重新激活音符
                note.canBePressed = false;       // 重置可按下状态（避免残留）
            }
            // ========== 新增代码结束 ==========
        }
    }
}
