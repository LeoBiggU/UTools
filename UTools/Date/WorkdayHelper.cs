using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace UTools.Date
{
    /// <summary>
    /// 工作日帮助类
    /// </summary>
    public static class WorkdayHelper
    {

        /// <summary>
        /// 工作日寻找帮助类
        /// </summary>
        private static IWorkdayFinder WorkdayFinder;

        /// <summary>
        /// 通过工作日寻找帮助类找到的工作日列表
        /// TODO 使用时的判空
        /// </summary>
        private static List<string> WorkdayList;

        /// <summary>
        /// 初始化工作日帮助类和数据
        /// </summary>
        static WorkdayHelper()
        {
            WorkdayFinder = new DefaultWorkdayFinder();
            WorkdayList = new List<string>();
        }

        ///// <summary>
        ///// 判断给定日期是否是工作日
        ///// </summary>
        ///// <param name="dateTime">要判断的日期</param>
        ///// <returns>给定日期是否是工作日</returns>
        //public static bool IsWorkday(this DateTime dateTime)
        //{
        //    var dateString = WorkdayFinder.ParseDateTimeToDateString(dateTime);
        //    return WorkdayList.Any(day => day == dateString);
        //}

        /// <summary>
        /// 以给定日期为基准尝试获取最近的一个工作日
        /// 可能是今天，也可能是之后的一天
        /// </summary>
        /// <param name="dateTime">基准日期</param>
        /// <param name="isConsiderNextYear">是否考虑下一年，默认不考虑，因为不一定能获取到只有年末才能获取到</param>
        /// <returns>如果为 null 说明不考虑下一年时就找不到，否则为下一个工作日的字符串值</returns>
        /// <exception cref="Exception">找不到时报错</exception>
        public static string RecentWorkday(this DateTime dateTime, bool isConsiderNextYear = false)
        {
            var dateString = WorkdayFinder.ParseDateTimeToDateString(dateTime.Date);
            // 尝试次数，最多尝试3次
            var tryCount = 0;
            // 最近的一个工作日
            string recentWorkday = null;
            while (tryCount < 3 && string.IsNullOrWhiteSpace(recentWorkday))
            {
                tryCount++;

                recentWorkday = WorkdayList.FirstOrDefault(day => string.Compare(day, dateString, StringComparison.Ordinal) >= 0);
                if (!string.IsNullOrWhiteSpace(recentWorkday)) break;

                #region 一般都是能找到的，找不到可能就是年份不同了，就要更新一下缓存的工作日再重新找

                var cachedFirstWorkday = WorkdayList.Any() ? WorkdayFinder.ParseDateStringToDateTime(WorkdayList.First()) : default;
                // 如果指定日期和缓存的工作日列表年份相同，说明下一个工作日在下一年；否则先更新到指定的年份去找是不是在那一年
                var updateYear = cachedFirstWorkday.Year == dateTime.Year ? dateTime.Year + 1 : dateTime.Year;
                if (!isConsiderNextYear && updateYear > DateTime.Now.Year) return null;
                WorkdayList = WorkdayFinder.GetWorkdays(updateYear);

                #endregion
            }

            if (string.IsNullOrEmpty(recentWorkday)) 
                throw new Exception("获取下一个工作日失败");

            return recentWorkday;
        }

        /// <summary>
        /// 找从指定日期算第 <paramref name="num"/> 个工作日的日期
        /// </summary>
        /// <param name="dateTime">基准日期</param>
        /// <param name="num">第几个工作日</param>
        /// <param name="isConsiderNextYear">是否考虑下一年，默认不考虑，因为不一定能获取到只有年末才能获取到</param>
        /// <returns>下一个指定工作日的日期，为 null 表示找不到</returns>
        public static DateTime? NextWorkday(this DateTime dateTime, int num = 1, bool isConsiderNextYear = false)
        {
            var dateString = WorkdayFinder.ParseDateTimeToDateString(dateTime.Date);

            var recentWorkday = dateTime.RecentWorkday();
            var recentWorkdayDate = WorkdayFinder.ParseDateStringToDateTime(recentWorkday);

            // 如果不等于，说明最近的一个工作日在指定日期之后，往后找的天数可以减一
            if (dateString != recentWorkday) num--;

            var recentWorkdayIndex = WorkdayList.IndexOf(recentWorkday);
            // 缓存的年份里剩余的工作日数量
            var remainingWorkdayNum = WorkdayList.Count - recentWorkdayIndex - 1;

            if (remainingWorkdayNum >= num) return WorkdayFinder.ParseDateStringToDateTime(WorkdayList[recentWorkdayIndex + num]);


            // 如果剩余的工作日天数少于指定的天数，说明跨年了，到下一年里继续找
            return isConsiderNextYear ? NextWorkday(new DateTime(dateTime.Year + 1, 1, 1), num - remainingWorkdayNum) : null; 

        }
        
    }

    #region 工作日寻找帮助类

    /// <summary>
    /// 寻找工作日帮助类 接口
    /// </summary>
    public interface IWorkdayFinder
    {

        /// <summary>
        /// 获取某年的工作日
        /// </summary>
        /// <param name="year">年份，值为四位数字，如2023（值为空时默认表示当前年份）</param>
        /// <returns>
        /// 返回工作日列表，
        /// 日期大小按List索引大小有效到大递增，字符串类型是方便缓存，
        /// 可以用 <see cref="ParseDateStringToDateTime"/> 方法解析成 <see cref="DateTime"/> 类型
        /// </returns>
        List<string> GetWorkdays(int? year = null);

        /// <summary>
        /// 将日期字符串解析成 <see cref="DateTime"/> 类型
        /// </summary>
        /// <param name="dateString">日期字符串，来源是 <see cref="GetWorkdays"/> 方法 </param>
        /// <returns>解析后的 <see cref="DateTime"/> 类型值</returns>
        DateTime ParseDateStringToDateTime(string dateString);

        /// <summary>
        /// 将日期 <see cref="DateTime"/> 类型值 解析成日期字符串（格式同 <see cref="GetWorkdays"/> 方法返回值）
        /// </summary>
        /// <param name="dateTime">要转换的日期值</param>
        /// <returns>解析后的日期字符串值</returns>
        string ParseDateTimeToDateString(DateTime dateTime);

    }

    /// <summary>
    /// 寻找工作日帮助类 默认实现
    /// 基于接口坞网站 <see cref="http://www.apihubs.cn/#/holiday"/> 提供的接口
    /// </summary>
    public class DefaultWorkdayFinder : IWorkdayFinder
    {
        public List<string> GetWorkdays(int? year = null)
        {
            year = year ?? DateTime.Now.Year;
            var res = new List<string>();

            try
            {
                #region 调用接口直接获取工作日

                var client = new HttpClient();
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri($"https://api.apihubs.cn/holiday/get?year={year}&workday=1&cn=1&size=365"),
                };
                using (var response = client.SendAsync(request).Result)
                {
                    response.EnsureSuccessStatusCode();
                    var body = response.Content.ReadAsStringAsync().Result;
                    var jsonResult = JsonConvert.DeserializeObject<JObject>(body);
                    var total = int.Parse(jsonResult["data"]["total"].ToString());
                    var dayList = jsonResult["data"]["list"].ToList();
                    var workdayArray = new string[total];
                    for (int i = 1; i <= total; i++)
                    {
                        var dayDate = dayList[i - 1];
                        workdayArray[total - i] = dayDate["date"].ToString();
                    }

                    res.AddRange(workdayArray.ToList());
                }

                if (res.Count == 0) throw new Exception($"列表为空");
                #endregion
            }
            catch (Exception ex)
            {
                throw new Exception($"获取{year}年份工作日信息失败：{ex.Message}");
            }

            return res;
        }

        public DateTime ParseDateStringToDateTime(string dateString) =>
            DateTime.ParseExact(dateString, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);

        public string ParseDateTimeToDateString(DateTime dateTime) => $"{dateTime.Year}{dateTime.Month:D2}{dateTime.Day:D2}";

    }

    #endregion

    ///// <summary>
    ///// 日期类型
    ///// </summary>
    //public enum DayType
    //{
    //    /// <summary>
    //    /// 工作日
    //    /// </summary>
    //    Workday,
    //    /// <summary>
    //    /// 补班日
    //    /// </summary>
    //    MakeUpDay,
    //    /// <summary>
    //    /// 周末
    //    /// </summary>
    //    Weekend,
    //    /// <summary>
    //    /// 法定节假日
    //    /// </summary>
    //    LegalHolidays,
    //}
}
