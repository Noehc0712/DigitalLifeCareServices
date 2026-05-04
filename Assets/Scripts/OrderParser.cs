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
    public string[] supportedOptions;
}
public class OptionInfo
{
    public string category;
    public string value;
    public OptionInfo(string c, string v) { category = c; value = v; }
}

public class OrderParser : MonoBehaviour
{
    // 메뉴 리스트 - 메뉴 인덱스, 메뉴명, 인식 키워드, 옵션 목록
    private List<MenuData> menuDatabase = new List<MenuData>()
    {
        new MenuData {
            index = 101, name = "(ICE)아메리카노",
            keywords = new string[] { "아메", "아매", "아메리카노", "아아", "아이스커피", "블랙커피", "시원한커피", "차가운커피", "냉커피" },
            supportedOptions = new string[] { "얼음양", "샷추가", "원두"} },
        new MenuData {
            index = 102, name = "(HOT)아메리카노",
            keywords = new string[] { "뜨아", "뜨거운커피", "핫커피", "따뜻한커피", "온커피" },
            supportedOptions = new string[] { "샷추가", "원두" } },
        new MenuData {
            index = 103, name = "에스프레소",
            keywords = new string[] { "에스", "애스", "에스프레소", "커피원액", "진한커피", "커피진한" },
            supportedOptions = new string[] { "설탕" } },
    };

    // 각 카테고리별 [기본값] 설정
    private Dictionary<string, string> defaultOptions = new Dictionary<string, string>()
    {
        { "얼음양", "보통" },
        { "샷추가", "기본" },
        { "원두", "기본" },
        { "설탕", "기본" }
    };

    // 옵션 키워드 사전 (키워드 : (카테고리, 변환될 표준 세부옵션명))
    private Dictionary<string, OptionInfo> optionDictionary = new Dictionary<string, OptionInfo>()
    {
        { "얼음적게", new OptionInfo("얼음양", "적게") },
        { "얼음빼", new OptionInfo("얼음양", "적게") },

        { "샷추가", new OptionInfo("샷추가", "1샷추가") },
        { "샷하나", new OptionInfo("샷추가", "1샷추가") },
        { "추가", new OptionInfo("샷추가", "1샷추가") },

        { "디카페인", new OptionInfo("원두", "디카페인") },
        { "논카페인", new OptionInfo("원두", "디카페인") },

        { "설탕빼", new OptionInfo("설탕", "없음") },
        { "안달게", new OptionInfo("설탕", "없음") },
        { "안달기", new OptionInfo("설탕", "없음") },
        { "설탕없이", new OptionInfo("설탕", "없음") },
        { "설탕없", new OptionInfo("설탕", "없음") },
        { "달게", new OptionInfo("설탕", "기본") },
        { "달기", new OptionInfo("설탕", "기본") },
        { "설탕넣어", new OptionInfo("설탕", "기본") },
        { "설탕기본", new OptionInfo("설탕", "기본") },
        { "설탕있", new OptionInfo("설탕", "기본") }
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

        Dictionary<string, string> finalOptionsMap = new Dictionary<string, string>();
        if (detectedMenuData.supportedOptions != null)
        {
            foreach (string category in detectedMenuData.supportedOptions)
            {
                finalOptionsMap[category] = defaultOptions[category];
            }
        }

        foreach (var opt in optionDictionary)
        {
            if (rawText.Contains(opt.Key))
            {
                string category = opt.Value.category;
                string value = opt.Value.value;

                if (finalOptionsMap.ContainsKey(category))
                {
                    finalOptionsMap[category] = value;
                }
            }
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

        List<string> finalOptionValues = new List<string>();
        if (detectedMenuData.supportedOptions != null && detectedMenuData.supportedOptions.Length > 0)
        {
            foreach (string category in detectedMenuData.supportedOptions)
            {
                finalOptionValues.Add(finalOptionsMap[category]);
            }
        }
        else
        {
            finalOptionValues.Add("옵션없음");
        }

        string optionString = string.Join(",", finalOptionValues);
        string finalDataFormat = $"{detectedMenuData.index},{detectedMenuData.name},{detectedCount},{optionString}";

        return finalDataFormat;
    }
}