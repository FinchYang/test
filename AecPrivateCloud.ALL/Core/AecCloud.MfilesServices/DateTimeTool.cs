using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AecCloud.MfilesServices
{
    public class DateTimeTool
    {
        /// <summary>
        /// 按照指定方式，获取时间区间列表
        /// </summary>
        /// <param name="beginDate">起点</param>
        /// <param name="endDate">终点</param>
        /// <param name="displayType">显示方式：季度、月、周</param>
        /// <returns></returns>
        public static IEnumerable<TimeArea> GetTimespanList(DateTime beginDate, DateTime endDate, int displayType)
        {
            var timeAreas = new List<TimeArea>();
            switch (displayType)
            {
                case 0://按季度
                    int len0 = (endDate.Year - beginDate.Year) * 4 + QuarterOfYear(endDate) - QuarterOfYear(beginDate);
                    for (int i = 0; i <= len0; i++)
                    {
                        var date0 = beginDate.AddMonths(i * 3);
                        var timeArea = new TimeArea
                        {
                            Title = date0.Year + "年" + QuarterOfYear(date0) + "季度",
                            BeginDate = GetFirstDayOfQuarter(date0),
                            EndDate = GetLastDayOfQuarter(date0)
                        };
                        timeAreas.Add(timeArea);
                    }
                    break;
                case 1://按月
                    int len1 = (endDate.Year - beginDate.Year) * 12 + MonthOfYear(endDate) - MonthOfYear(beginDate);
                    for (int l = 0; l <= len1; l++)
                    {
                        var date0 = beginDate.AddMonths(l);
                        var timeArea = new TimeArea
                        {
                            Title = date0.Year + "年" + MonthOfYear(date0) + "月",
                            BeginDate = GetFirstDayOfMonth(date0),
                            EndDate = GetLastDayOfMonth(date0)
                        };
                        timeAreas.Add(timeArea);
                    }
                    break;
                case 2://按周
                    var week0 = GetFirstDayOfWeek(beginDate);
                    var endWeek0 = GetFirstDayOfWeek(endDate);
                    while (week0 <= endWeek0)
                    {
                        var timeArea = new TimeArea
                        {
                            Title = week0.Year + "年" + WeekOfYear(week0) + "周",
                            BeginDate = GetFirstDayOfWeek(week0),
                            EndDate = GetLastDayOfWeek(week0)
                        };
                        timeAreas.Add(timeArea);

                        week0 = week0.AddDays(7);
                    }
                    break;
            }
            return timeAreas;
        }

        //取指定日期是一年中的第几季度
        public static int QuarterOfYear(DateTime date)
        {
            return (date.Month - 1) / 3 + 1;
        }
        //取指定日期是一年中的第几月
        public static int MonthOfYear(DateTime date)
        {
            return date.Month;
        }
        //取指定日期是一年中的第几周   
        public static int WeekOfYear(DateTime dateTime)
        {
            int firstdayofweek = Convert.ToDateTime(dateTime.Year + "- " + "1-1 ").DayOfWeek.GetHashCode();
            int days = dateTime.DayOfYear;
            int daysOutOneWeek = days - (7 - firstdayofweek);
            if (daysOutOneWeek <= 0)
            {
                return 1;
            }
            else
            {
                int weeks = daysOutOneWeek / 7;
                if (daysOutOneWeek % 7 != 0)
                {
                    weeks++;
                }
                return weeks + 1;
            }
        }

        //周一
        public static DateTime GetFirstDayOfWeek(DateTime dt)
        {
            DateTime startWeek = dt.AddDays(1 - Convert.ToInt32(dt.DayOfWeek.ToString("d")));  //本周周一 
            return startWeek.Date;
        }
        //周日
        public static DateTime GetLastDayOfWeek(DateTime dt)
        {
            DateTime startWeek = dt.AddDays(1 - Convert.ToInt32(dt.DayOfWeek.ToString("d")));  //本周周一  
            DateTime endWeek = startWeek.AddDays(6);  //本周周日
            return endWeek.Date;
        }
        //月初
        public static DateTime GetFirstDayOfMonth(DateTime dt)
        {
            DateTime startMonth = dt.AddDays(1 - dt.Day);  //本月月初 
            return startMonth;
        }
        //月末
        public static DateTime GetLastDayOfMonth(DateTime dt)
        {
            DateTime startMonth = dt.AddDays(1 - dt.Day);  //本月月初  
            DateTime endMonth = startMonth.AddMonths(1).AddDays(-1);  //本月月末  
            return endMonth;
        }
        //季度初
        public static DateTime GetFirstDayOfQuarter(DateTime dt)
        {
            return dt.AddMonths(0 - (dt.Month - 1) % 3).AddDays(1 - dt.Day).Date;
        }
        //季度末
        public static DateTime GetLastDayOfQuarter(DateTime dt)
        {
            var startQuarter = dt.AddMonths(0 - (dt.Month - 1) % 3).AddDays(1 - dt.Day);
            return startQuarter.AddMonths(3).AddDays(-1).Date;
        }

        // 获取指定日期所在周的第一天，星期天为第一天
        public static DateTime GetDateTimeWeekFirstDaySun(DateTime dateTime)
        {
            //得到是星期几，然后从当前日期减去相应天数 
            int weeknow = Convert.ToInt32(dateTime.DayOfWeek);
            int daydiff = (-1) * weeknow;
            DateTime firstWeekDay = dateTime.AddDays(daydiff);
            return firstWeekDay;
        }
        // 获取指定日期所在周的最后一天，星期六为最后一天
        public static DateTime GetDateTimeWeekLastDaySat(DateTime dateTime)
        {
            int weeknow = Convert.ToInt32(dateTime.DayOfWeek);
            int daydiff = (7 - weeknow) - 1;
            DateTime lastWeekDay = dateTime.AddDays(daydiff);
            return lastWeekDay;
        }
    }

    /// <summary>
    /// 时间区间（闭区间），如：季度、月、周
    /// </summary>
    public class TimeArea
    {
        /// <summary>
        /// 区间名称，如2015年7月
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 起始日期
        /// </summary>
        public DateTime BeginDate { get; set; }
        /// <summary>
        /// 截止日期
        /// </summary>
        public DateTime EndDate { get; set; }
    }
}
