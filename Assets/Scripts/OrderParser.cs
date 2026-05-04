using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class MenuData
{
    public int index;
    public string name;
    public string[] keywords;
    public string[] validOptions;
}

public class OrderParser : MonoBehaviour
{
    // 메뉴 사전 - 메뉴 인덱스, 메뉴명, 인식 키워드
    private List<MenuData> menuDatabase = new List<MenuData>()
    {
        new MenuData {
            index = 101, name = "(ICE)아메리카노",
            keywords = new string[] { "아메", "아매", "아메리카노", "아아", "아이스커피", "블랙커피", "시원한커피", "차가운커피", "냉커피" },
            validOptions = new string[] { "얼음적게", "샷추가", "디카페인"} },
        new MenuData {
            index = 102, name = "(HOT)아메리카노",
            keywords = new string[] { "뜨아", "뜨거운커피", "핫커피", "따뜻한커피", "온커피" },
            validOptions = new string[] { "얼음적게", "샷추가", "디카페인" } },
        new MenuData {
            index = 103, name = "에스프레소",
            keywords = new string[] { "에스", "애스", "에스프레소", "커피원액", "진한커피", "커피진한" },
            validOptions = new string[] { "달게" } },
    };

    // 옵션 키워드 사전 (키워드 : 변환될 표준 옵션명)
    private Dictionary<string, string> optionDictionary = new Dictionary<string, string>()
    {
        { "얼음적게", "얼음적게" }, { "얼음빼", "얼음적게" },
        { "샷추가", "1샷추가" },{ "샷하나", "1샷추가" }, { "추가", "1샷추가" },
        { "디카페인", "원두디카페인" },{ "논카페인", "원두디카페인" },
        { "설탕빼", "설탕없음" }, { "안달게", "설탕없음" }, { "안달기", "설탕없음" }, { "설탕없이", "설탕없음"}, { "설탕없", "설탕없음"},
        { "달게", "설탕기본" }, { "달기", "설탕기본" }, { "설탕넣어", "설탕기본" }, { "설탕기본", "설탕기본" }, { "설탕있", "설탕기본" }
    };

    // 수량 변환 사전
    private Dictionary<string, int> numberDictionary = new Dictionary<string, int>()
    {
        { "한", 1 }, { "하나", 1 }, { "일", 1 }, { "두", 2 }, { "둘", 2 }, { "이", 2 },
        { "세", 3 }, { "셋", 3 }, { "삼", 3 }, { "네", 4 }, { "넷", 4 }, { "사", 4 },
        { "다섯", 5 }, { "오", 5 }
    };

    public string AnalyzeOrderText(string sttText)
    {
        string rawText = sttText.Replace(" ", "");

        MenuData detectedMenuData = null;
        int detectedCount = 1;

        foreach (var menu in menuDatabase)
        {
            foreach (var keyword in menu.keywords)
            {
                if (rawText.Contains(keyword))
                {
                    detectedMenuData = menu;
                    break;
                }
            }
            if (detectedMenuData != null) break;
        }

        if (detectedMenuData == null)
        {
            return "0,메뉴인식실패,0,기본";
        }

        List<string> extractedOptions = new List<string>();
        foreach (var opt in optionDictionary)
        {
            if (rawText.Contains(opt.Key))
            {
                if (!extractedOptions.Contains(opt.Value))
                {
                    extractedOptions.Add(opt.Value);
                }
            }
        }

        List<string> finalValidOptions = new List<string>();
        if (detectedMenuData.validOptions != null)
        {
            foreach (string opt in extractedOptions)
            {
                if (detectedMenuData.validOptions.Contains(opt))
                {
                    finalValidOptions.Add(opt);
                }
            }
        }

        if (finalValidOptions.Count == 0)
        {
            finalValidOptions.Add("기본");
        }

        Match numberMatch = Regex.Match(rawText, @"\d+");
        if (numberMatch.Success)
        {
            detectedCount = int.Parse(numberMatch.Value);
        }
        else
        {
            foreach (var num in numberDictionary)
            {
                if (rawText.Contains(num.Key))
                {
                    detectedCount = num.Value;
                    break;
                }
            }
        }

        string optionString = string.Join(",", finalValidOptions);
        string finalDataFormat = $"{detectedMenuData.index},{detectedMenuData.name},{detectedCount},{optionString}";

        return finalDataFormat;
    }
}