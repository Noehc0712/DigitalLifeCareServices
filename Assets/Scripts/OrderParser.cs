using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class MenuData { public int index; public string name; public string[] keywords; public string[] supportedOptions; }
public class OptionInfo { public string category; public string value; public OptionInfo(string c, string v) { category = c; value = v; } }
public class ParsedOrder { public bool isSuccess; public bool isCancel; public string finalDataFormat; public string displayText; public string menuName; }

// 위치(거리) 기반 매칭을 위한 보조 클래스들
public class MenuMatch { public int start; public int end; public MenuData menu; }
public class OptMatch { public int start; public int end; public string category; public string value; }
public class QtyMatch { public int start; public int end; public int count; }
public class CancelMatch { public int start; public int end; }

public class OrderParser : MonoBehaviour
{
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

    private Dictionary<string, string> defaultOptions = new Dictionary<string, string>()
    {
        { "얼음양", "보통" }, { "샷추가", "기본" }, { "원두", "기본" }, { "설탕", "기본" }
    };

    private Dictionary<string, OptionInfo> optionDictionary = new Dictionary<string, OptionInfo>()
    {
        { "얼음적게", new OptionInfo("얼음양", "적게") }, { "얼음빼", new OptionInfo("얼음양", "적게") },
        { "샷추가", new OptionInfo("샷추가", "1샷추가") }, { "샷하나", new OptionInfo("샷추가", "1샷추가") }, { "추가", new OptionInfo("샷추가", "1샷추가") },
        { "디카페인", new OptionInfo("원두", "디카페인") }, { "논카페인", new OptionInfo("원두", "디카페인") },
        { "설탕빼", new OptionInfo("설탕", "없음") }, { "안달게", new OptionInfo("설탕", "없음") },
        { "안달기", new OptionInfo("설탕", "없음") }, { "설탕없이", new OptionInfo("설탕", "없음") }, { "설탕없", new OptionInfo("설탕", "없음") },
        { "달게", new OptionInfo("설탕", "기본") }, { "달기", new OptionInfo("설탕", "기본") },
        { "설탕넣어", new OptionInfo("설탕", "기본") }, { "설탕기본", new OptionInfo("설탕", "기본") }, { "설탕있", new OptionInfo("설탕", "기본") }
    };

    private Dictionary<string, int> numberDictionary = new Dictionary<string, int>()
    {
        { "한", 1 }, { "하나", 1 }, { "일", 1 }, { "두", 2 }, { "둘", 2 }, { "이", 2 },
        { "세", 3 }, { "셋", 3 }, { "삼", 3 }, { "네", 4 }, { "넷", 4 }, { "사", 4 }, { "다섯", 5 }, { "오", 5 }
    };

    private string[] cancelKeywords = new string[] { "취소", "빼", "제외", "지워", "잘못" };

    public List<ParsedOrder> AnalyzeOrderText(string sttText)
    {
        List<ParsedOrder> results = new List<ParsedOrder>();
        string rawText = sttText.Replace(" ", "");

        List<MenuMatch> menuMatches = new List<MenuMatch>();
        foreach (var menu in menuDatabase)
        {
            foreach (var kw in menu.keywords)
            {
                int idx = rawText.IndexOf(kw);
                while (idx != -1)
                {
                    menuMatches.Add(new MenuMatch { start = idx, end = idx + kw.Length, menu = menu });
                    idx = rawText.IndexOf(kw, idx + kw.Length);
                }
            }
        }
        menuMatches = menuMatches.OrderBy(m => m.start).ThenByDescending(m => m.end - m.start).ToList();
        List<MenuMatch> validMenus = new List<MenuMatch>();
        int lastMenuEnd = -1;
        foreach (var m in menuMatches) { if (m.start >= lastMenuEnd) { validMenus.Add(m); lastMenuEnd = m.end; } }

        if (validMenus.Count == 0) return results;

        List<OptMatch> optMatches = new List<OptMatch>();
        foreach (var opt in optionDictionary)
        {
            int idx = rawText.IndexOf(opt.Key);
            while (idx != -1) { optMatches.Add(new OptMatch { start = idx, end = idx + opt.Key.Length, category = opt.Value.category, value = opt.Value.value }); idx = rawText.IndexOf(opt.Key, idx + opt.Key.Length); }
        }
        optMatches = optMatches.OrderBy(o => o.start).ThenByDescending(o => o.end - o.start).ToList();
        List<OptMatch> validOpts = new List<OptMatch>();
        int lastOptEnd = -1;
        foreach (var o in optMatches) { if (o.start >= lastOptEnd) { validOpts.Add(o); lastOptEnd = o.end; } }

        List<QtyMatch> qtyMatches = new List<QtyMatch>();
        foreach (var num in numberDictionary)
        {
            int idx = rawText.IndexOf(num.Key);
            while (idx != -1) { qtyMatches.Add(new QtyMatch { start = idx, end = idx + num.Key.Length, count = num.Value }); idx = rawText.IndexOf(num.Key, idx + num.Key.Length); }
        }
        MatchCollection matches = Regex.Matches(rawText, @"\d+");
        foreach (Match m in matches) { qtyMatches.Add(new QtyMatch { start = m.Index, end = m.Index + m.Length, count = int.Parse(m.Value) }); }

        qtyMatches = qtyMatches.OrderBy(q => q.start).ThenByDescending(q => q.end - q.start).ToList();
        List<QtyMatch> validQtys = new List<QtyMatch>();
        int lastQtyEnd = -1;
        foreach (var q in qtyMatches) { if (q.start >= lastQtyEnd) { validQtys.Add(q); lastQtyEnd = q.end; } }

        List<CancelMatch> cancelMatches = new List<CancelMatch>();
        foreach (string c in cancelKeywords)
        {
            int idx = rawText.IndexOf(c);
            while (idx != -1) { cancelMatches.Add(new CancelMatch { start = idx, end = idx + c.Length }); idx = rawText.IndexOf(c, idx + c.Length); }
        }

        var menuOptMap = new Dictionary<MenuMatch, Dictionary<string, string>>();
        var menuQtyMap = new Dictionary<MenuMatch, int>();
        var menuCancelMap = new Dictionary<MenuMatch, bool>();

        foreach (var m in validMenus)
        {
            menuOptMap[m] = new Dictionary<string, string>();
            if (m.menu.supportedOptions != null) foreach (var cat in m.menu.supportedOptions) menuOptMap[m][cat] = defaultOptions[cat];
            menuQtyMap[m] = 1;
            menuCancelMap[m] = false;
        }

        foreach (var opt in validOpts)
        {
            var closestMenu = GetClosestMenu(opt.start, opt.end, validMenus);
            if (closestMenu != null && closestMenu.menu.supportedOptions != null && closestMenu.menu.supportedOptions.Contains(opt.category)) menuOptMap[closestMenu][opt.category] = opt.value;
        }
        foreach (var qty in validQtys)
        {
            var closestMenu = GetClosestMenu(qty.start, qty.end, validMenus);
            if (closestMenu != null) menuQtyMap[closestMenu] = qty.count;
        }
        foreach (var cancel in cancelMatches)
        {
            var closestMenu = GetClosestMenu(cancel.start, cancel.end, validMenus);
            if (closestMenu != null) menuCancelMap[closestMenu] = true;
        }

        foreach (var m in validMenus)
        {
            ParsedOrder po = new ParsedOrder();
            po.isSuccess = true;
            po.menuName = m.menu.name;
            po.isCancel = menuCancelMap[m];
            int count = menuQtyMap[m];

            List<string> finalOptionValues = new List<string>();
            if (m.menu.supportedOptions != null && m.menu.supportedOptions.Length > 0)
                foreach (string category in m.menu.supportedOptions) finalOptionValues.Add(menuOptMap[m][category]);
            else finalOptionValues.Add("옵션없음");

            string optionString = string.Join(",", finalOptionValues);
            po.finalDataFormat = $"{m.menu.index},{m.menu.name},{count},{optionString}";
            po.displayText = $"{m.menu.name} {count}잔  [{optionString}]";

            results.Add(po);
        }

        return results;
    }

    private MenuMatch GetClosestMenu(int start, int end, List<MenuMatch> menus)
    {
        MenuMatch closest = null;
        int minDistance = int.MaxValue;

        foreach (var m in menus)
        {
            int dist = 0;
            if (end <= m.start) dist = m.start - end;
            else if (start >= m.end) dist = start - m.end;

            if (dist < minDistance) { minDistance = dist; closest = m; }
        }
        return closest;
    }
}