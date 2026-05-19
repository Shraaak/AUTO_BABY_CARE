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
    // ===================== 这里粘贴你刚复制的完整API Key =====================
    private const string API_KEY = "sk-46da5cdebd324c64993713092d06a5e8";
    // 固定用qwen-turbo，适配你的宝宝对话场景
    private const string API_URL = "https://dashscope.aliyuncs.com/api/v1/services/aigc/text-generation/generation";
    private const string TARGET_MODEL = "qwen-turbo";
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
                string reply = result.output.choices[0].message.content;
                
                // 按场景动态判断正确回复
                bool isCorrect = state == "bored" 
                    ? reply.Contains("开心") || reply.Contains("嘻嘻") || reply.Contains("玩")
                    : reply.Contains("睡") || reply.Contains("觉") || reply.Contains("乖") || reply.Contains("安静") || reply.Contains("睡啦");
                
                callback?.Invoke(reply, isCorrect);
            }
        }
    }

    // 宝宝对话专用Prompt，固定返回软萌语气+符合意图的回复
    private static string BuildBabyPrompt(string input, string state)
    {
        string scene = state == "bored" 
            ? "宝宝现在很无聊，想要玩家陪玩。如果玩家的话是想陪你玩、哄你，就是正确的；如果玩家拒绝陪你、让你自己玩，就是错误的。" 
            : "宝宝现在很困，想要睡觉。如果玩家的话是哄你睡觉、让你安静休息，就是正确的；如果玩家让你起来玩、吵你，就是错误的。";

        return $@"
            你是一个软萌的6岁小宝宝，说话简短符合婴儿，2-8个字。
            规则：
            1. 当前场景：{scene}
            2. 正确的话，你要回复开心、乖巧的话；错误的话，你要回复委屈、闹脾气的话。
            3. 只回复宝宝说的话，不要加其他内容。

            玩家对你说：{input}
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
}