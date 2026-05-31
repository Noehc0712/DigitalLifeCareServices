using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class MenuData { public int index; public string name; public int price; public string[] keywords; public string[] supportedOptions; }
public class OptionInfo { public string category; public string value; public OptionInfo(string c, string v) { category = c; value = v; } }
public class ParsedOrder { public bool isSuccess; public bool isCancel; public string finalDataFormat; public string displayText; public string menuName; public int totalPrice; }

public class MenuMatch { public int start; public int end; public MenuData menu; }
public class OptMatch { public int start; public int end; public string category; public string value; }
public class QtyMatch { public int start; public int end; public int count; }
public class CancelMatch { public int start; public int end; }

public class OrderParser : MonoBehaviour
{
    private List<MenuData> menuDatabase = new List<MenuData>()
    {
        // ICE 커피
        new MenuData { index = 101, name = "(ICE)메가리카노", price = 3000, keywords = new string[] { "메가리카노", "아이스메가리카노" }, supportedOptions = new string[] { "얼음양", "샷추가", "원두"} },
        new MenuData { index = 102, name = "(ICE)아메리카노", price = 2000, keywords = new string[] { "아메리카노", "아아", "아이스아메리카노", "아메", "아이스커피", "냉커피" }, supportedOptions = new string[] { "얼음양", "샷추가", "원두"} },
        new MenuData { index = 103, name = "할메가커피", price = 1900, keywords = new string[] { "할메가커피", "할매가커피", "할메가", "할매가" }, supportedOptions = new string[] { "얼음양", "샷추가"} },
        new MenuData { index = 104, name = "(ICE)꿀아메리카노", price = 2700, keywords = new string[] { "꿀아메리카노", "아이스꿀아메리카노", "꿀아메" }, supportedOptions = new string[] { "얼음양", "샷추가", "원두"} },
        new MenuData { index = 105, name = "(ICE)바닐라아메리카노", price = 2700, keywords = new string[] { "바닐라아메리카노", "아이스바닐라아메리카노", "바닐라아메" }, supportedOptions = new string[] { "얼음양", "샷추가", "원두"} },
        new MenuData { index = 106, name = "(ICE)헤이즐넛아메리카노", price = 2700, keywords = new string[] { "헤이즐넛아메리카노", "아이스헤이즐넛아메리카노", "헤이즐넛아메" }, supportedOptions = new string[] { "얼음양", "샷추가", "원두"} },
        // HOT 커피
        new MenuData { index = 107, name = "(HOT)아메리카노", price = 1500, keywords = new string[] { "뜨아", "따뜻한아메리카노", "뜨거운아메리카노", "핫아메리카노", "따뜻한커피" }, supportedOptions = new string[] { "샷추가", "원두" } },
        new MenuData { index = 108, name = "(HOT)꿀아메리카노", price = 2700, keywords = new string[] { "따뜻한꿀아메리카노", "뜨거운꿀아메리카노", "핫꿀아메리카노" }, supportedOptions = new string[] { "샷추가", "원두" } },
        new MenuData { index = 109, name = "(HOT)바닐라아메리카노", price = 2700, keywords = new string[] { "따뜻한바닐라아메리카노", "뜨거운바닐라아메리카노", "핫바닐라아메리카노" }, supportedOptions = new string[] { "샷추가", "원두" } },
        new MenuData { index = 110, name = "(HOT)헤이즐넛아메리카노", price = 2700, keywords = new string[] { "따뜻한헤이즐넛아메리카노", "뜨거운헤이즐넛아메리카노", "핫헤이즐넛아메리카노" }, supportedOptions = new string[] { "샷추가", "원두" } },
        // 에스프레소
        new MenuData { index = 111, name = "에스프레소", price = 1500, keywords = new string[] { "에스프레소", "에스", "애스" }, supportedOptions = new string[] { "설탕" } },
        new MenuData { index = 112, name = "에스프레소 도피오", price = 2000, keywords = new string[] { "에스프레소도피오", "도피오" }, supportedOptions = new string[] { "설탕" } },
        // 라떼
        new MenuData { index = 113, name = "카페라떼", price = 2900, keywords = new string[] { "카페라떼", "라떼" }, supportedOptions = new string[] { "얼음양", "샷추가", "원두" } },
        new MenuData { index = 114, name = "바닐라라떼", price = 3400, keywords = new string[] { "바닐라라떼", "바라" }, supportedOptions = new string[] { "얼음양", "샷추가", "원두" } },
        new MenuData { index = 115, name = "카푸치노", price = 3200, keywords = new string[] { "카푸치노" }, supportedOptions = new string[] { "샷추가", "원두" } },
        new MenuData { index = 116, name = "왕메가카페라떼", price = 4400, keywords = new string[] { "왕메가카페라떼", "왕메가라떼", "왕메가", "왕매가" }, supportedOptions = new string[] { "얼음양", "샷추가", "원두" } },
        // 디카페인
        new MenuData { index = 117, name = "(ICE)디카페인 아메리카노", price = 3400, keywords = new string[] { "아이스디카페인아메리카노", "아이스디카페인", "디아아", "디카페인아아" }, supportedOptions = new string[] { "얼음양", "샷추가" } },
        new MenuData { index = 118, name = "(HOT)디카페인 아메리카노", price = 3400, keywords = new string[] { "따뜻한디카페인아메리카노", "핫디카페인아메리카노", "디카페인뜨아", "뜨디아" }, supportedOptions = new string[] { "샷추가" } },
        new MenuData { index = 119, name = "디카페인 카페라떼", price = 3900, keywords = new string[] { "디카페인카페라떼", "디카페인라떼" }, supportedOptions = new string[] { "얼음양", "샷추가" } },
        // 음료
        new MenuData { index = 120, name = "제로 부스트 에이드", price = 3000, keywords = new string[] { "제로부스트에이드", "제로에이드" }, supportedOptions = new string[] { "얼음양", "당도" } },
        new MenuData { index = 121, name = "블루베리요거트스무디", price = 3900, keywords = new string[] { "블루베리요거트스무디", "블루베리스무디" }, supportedOptions = new string[] { "얼음양", "당도" } },
        new MenuData { index = 122, name = "저당 꿀배 XO요구르트", price = 3900, keywords = new string[] { "저당꿀배엑스오요구르트", "저당꿀배요구르트", "꿀배요구르트", "저당꿀배" }, supportedOptions = new string[] { "얼음양", "당도" } },
        // 티
        new MenuData { index = 123, name = "유자생강차", price = 3300, keywords = new string[] { "유자생강차", "유자차", "생강차" }, supportedOptions = new string[] { "당도"  } },
        new MenuData { index = 124, name = "망고폼 자스민 티플레저", price = 3900, keywords = new string[] { "망고폼자스민티플레저", "망고폼자스민", "자스민티플레저", "망고티플레저" }, supportedOptions = new string[] { "얼음양", "당도" } },
        // 푸드
        new MenuData { index = 125, name = "딸기요거트마카롱", price = 2800, keywords = new string[] { "딸기요거트마카롱", "딸기마카롱", "마카롱" } },
        new MenuData { index = 126, name = "아이스허니와앙슈", price = 3500, keywords = new string[] { "아이스허니와앙슈", "허니와앙슈", "와앙슈", "슈", "크림빵" } },
        new MenuData { index = 127, name = "치즈케익", price = 4200, keywords = new string[] { "치즈케익", "치즈케이크", "치즈" } },
        new MenuData { index = 128, name = "팥빙 젤라또 파르페", price = 4500, keywords = new string[] { "팥빙젤라또파르페", "젤라또파르페", "팥빙파르페", "팥", "빙수" } },
        new MenuData { index = 129, name = "플레인 크로플", price = 3300, keywords = new string[] { "플레인크로플", "크로플", "빵" } },
        // 상품
        new MenuData { index = 130, name = "메가 텀블러", price = 12000, keywords = new string[] { "메가텀블러", "텀블러" } }
    };

    private Dictionary<string, string> defaultOptions = new Dictionary<string, string>()
    {
        { "얼음양", "보통" }, { "샷추가", "기본" }, { "원두", "기본" }, { "설탕", "기본" }, {"당도", "기본"}
    };

    private Dictionary<string, int> optionPriceDB = new Dictionary<string, int>()
    {
        { "보통", 0 }, { "적게", 0 },
        { "기본", 0 }, { "1샷추가", 500 },
        { "디카페인", 500 },
        { "없음", 0 }, { "덜 달게", 0 }
    };

    private Dictionary<string, OptionInfo> optionDictionary = new Dictionary<string, OptionInfo>()
    {
        // 얼음양
        { "얼음적게", new OptionInfo("얼음양", "적게") }, { "얼음빼", new OptionInfo("얼음양", "적게") },
        // 샷추가
        { "샷추가", new OptionInfo("샷추가", "1샷추가") }, { "샷하나", new OptionInfo("샷추가", "1샷추가") }, { "추가", new OptionInfo("샷추가", "1샷추가") },
        // 원두
        { "디카페인", new OptionInfo("원두", "디카페인") }, { "논카페인", new OptionInfo("원두", "디카페인") },
        // 설탕
        { "설탕빼", new OptionInfo("설탕", "없음") }, { "설탕없이", new OptionInfo("설탕", "없음") }, { "설탕없", new OptionInfo("설탕", "없음") },
        { "설탕넣어", new OptionInfo("설탕", "기본") }, { "설탕기본", new OptionInfo("설탕", "기본") }, { "설탕있", new OptionInfo("설탕", "기본") }, 
        // 당도
        { "안달게", new OptionInfo("당도", "덜 달게") }, { "덜달게", new OptionInfo("당도", "덜 달게") }, { "당도없", new OptionInfo("당도", "덜 달게") },
        { "달게", new OptionInfo("당도", "기본") }, { "달기", new OptionInfo("당도", "기본") }, { "당도있", new OptionInfo("당도", "기본") }
    };

    private Dictionary<string, int> numberDictionary = new Dictionary<string, int>()
    {
        { "한", 1 }, { "하나", 1 }, { "두", 2 }, { "둘", 2 },
        { "세", 3 }, { "셋", 3 }, { "네", 4 }, { "넷", 4 }, { "다섯", 5 },
    };

    private string[] cancelKeywords = new string[] { "취소", "빼", "제외", "지워", "잘못", "제거" };

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
            int optionExtraCost = 0;

            if (m.menu.supportedOptions != null && m.menu.supportedOptions.Length > 0)
            {
                foreach (string category in m.menu.supportedOptions)
                {
                    string selectedOptionValue = menuOptMap[m][category];
                    finalOptionValues.Add(selectedOptionValue);

                    if (optionPriceDB.ContainsKey(selectedOptionValue))
                    {
                        optionExtraCost += optionPriceDB[selectedOptionValue];
                    }
                }
            }
            else finalOptionValues.Add("옵션없음");

            int unitPrice = m.menu.price + optionExtraCost;
            po.totalPrice = unitPrice * count;

            string optionString = string.Join(",", finalOptionValues);

            po.finalDataFormat = $"{m.menu.index},{m.menu.name},{count},{po.totalPrice},{optionString}";

            string formattedPrice = string.Format("{0:#,0}", po.totalPrice);
            po.displayText = $"{m.menu.name} {count}개  <color=#008800><b>({formattedPrice}원)</b></color>  [{optionString}]";

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