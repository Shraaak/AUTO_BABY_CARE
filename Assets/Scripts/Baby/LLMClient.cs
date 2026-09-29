using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

public static class LLMClient
{
    #region API 配置
    //完整API Key 
    public const string API_KEY = "sk-46da5cdebd324c64993713092d06a5e8";
    // 固定用qwen-turbo，适配你的宝宝对话场景
    public const string API_URL = "https://dashscope.aliyuncs.com/api/v1/services/aigc/text-generation/generation";
    public const string TARGET_MODEL = "qwen-turbo";
    #endregion



    #region 本地关键词降级逻辑 
    // 保留本地关键词降级逻辑，API调用失败时兜底
    private static readonly HashSet<string> BoredCorrect = new(){"玩","陪你","抱抱","玩具"};
    private static readonly HashSet<string> BoredWrong = new(){"不玩","走开","忙"};
    private static readonly HashSet<string> SleepyCorrect = new(){"睡","觉觉","安静","关灯","困"};
    private static readonly HashSet<string> SleepyWrong = new(){"不睡","吵闹","玩"};
    #endregion


    /// <summary>
    /// Unity里调用LLM的入口，直接在UI的发送按钮里调用
    /// </summary>
    public static void SendChat(string playerInput, string babyState, MonoBehaviour mono, Action<string, bool> onResult)
    {
        mono.StartCoroutine(RequestCoroutine(playerInput, babyState, onResult));
    }

    private static IEnumerator RequestCoroutine(string input, string state, Action<string, bool> callback)
    {
        // 构造宝宝对话专用Prompt
        string prompt = BuildBabyPrompt(input, state);
        var requestBody = new
        {
            model = TARGET_MODEL,
            input = new
            {
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            },
            parameters = new { result_format = "message" }
        };

        using (HttpClient client = new HttpClient())
        {
            // 配置请求头
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {API_KEY}");
            string json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // 发送请求（Unity协程不卡主线程）
            var requestTask = client.PostAsync(API_URL, content);
            while (!requestTask.IsCompleted) yield return null;

            // 处理结果
            var response = requestTask.Result;
            if (response.IsSuccessStatusCode)
            {
                var readTask = response.Content.ReadAsStringAsync();
                while (!readTask.IsCompleted) yield return null;

                var result = JsonConvert.DeserializeObject<DashScopeResponse>(readTask.Result);
                string cont = result.output.choices[0].message.content;

                BabyLLMResult babyResult = JsonConvert.DeserializeObject<BabyLLMResult>(cont);
                if (babyResult == null ||
                    string.IsNullOrWhiteSpace(babyResult.reply))
                {
                    FallbackLocalCheck(input, state, callback);
                    yield break;
                }
                callback?.Invoke(
                    babyResult.reply,
                    babyResult.isCorrect
                );
            }
            else
            {
                FallbackLocalCheck(input, state, callback);
            }
        }
    }

    // 宝宝对话专用Prompt，固定返回软萌语气+符合意图的回复
    private static string BuildBabyPrompt(string input, string state)
    {
        string scene = state == "bored"
            ? "宝宝很无聊。玩家愿意陪玩、安慰或抱抱属于正确；拒绝、赶走宝宝属于错误。"
            : "宝宝很困。玩家哄睡、保持安静属于正确；让宝宝继续玩、制造吵闹属于错误。";

        return $@"
        你是一个6到8岁的小宝宝。

        当前场景：
        {scene}

        请判断玩家的话是否满足宝宝的需求，并生成2到8个字的宝宝回复。

        只返回以下JSON，不要解释，不要使用Markdown：

        {{
            ""reply"": ""宝宝说的话"",
            ""isCorrect"": true
        }}

        玩家说：
        {input}
        ";
    }

    // 本地兜底逻辑
    private static void FallbackLocalCheck(string input, string state, Action<string, bool> callback)
    {
        input = input.ToLower();
        bool isCorrect = state == "bored" 
            ? CheckKeyword(input, BoredCorrect, BoredWrong) 
            : CheckKeyword(input, SleepyCorrect, SleepyWrong);

        string reply = isCorrect 
            ? (state == "bored" ? "嘻嘻好开心" : "我睡啦") 
            : (state == "bored" ? "呜呜没人陪" : "哼睡不着");
        
        callback?.Invoke(reply, isCorrect);
    }

    private static bool CheckKeyword(string input, HashSet<string> ok, HashSet<string> no)
    {
        foreach (var w in no) if (input.Contains(w)) return false;
        foreach (var k in ok) if (input.Contains(k)) return true;
        return false;
    }

    // API返回结构
    private class DashScopeResponse
    {
        public DashScopeOutput output { get; set; }
    }
    private class DashScopeOutput
    {
        public List<DashScopeChoice> choices { get; set; }
    }
    private class DashScopeChoice
    {
        public DashScopeMessage message { get; set; }
    }
    private class DashScopeMessage
    {
        public string content { get; set; }
    }

    private class BabyLLMResult
    {
        public string reply { get; set; }
        public bool isCorrect { get; set; }
    }    
}