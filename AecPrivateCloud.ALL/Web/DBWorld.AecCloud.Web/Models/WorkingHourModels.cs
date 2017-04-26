using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AecCloud.MfilesServices;

namespace DBWorld.AecCloud.Web.Models
{
    public class ProjectHour
    {
        public string ProjName { get; set; }
        public IList<UserHour> UserList { get; set; }
        public double BudgetHours { get; set; }
        public double ActualHours { get; set; }
        public IList<string> TimeSpans { get; set; } 
    }

    public class UserHour
    {
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 预算工时
        /// </summary>
        public IList<UnitHour> BudgetHours { get; set; }
        /// <summary>
        /// 实际工时
        /// </summary>
        public IList<UnitHour> ActualHours { get; set; }


        private double _budgetTotal;
        public Double BudgetTotal
        {
            get
            {
                _budgetTotal = 0.0;
                if (BudgetHours == null || BudgetHours.Count == 0)
                {
                    return _budgetTotal;
                }
                foreach (UnitHour u in BudgetHours)
                {
                    _budgetTotal += u.Hours;
                }
                return _budgetTotal;
            }
            set { _budgetTotal = value; } 
        }


        private double _actualTotal;
        public Double ActualTotal
        {
            get
            {
                _actualTotal = 0.0;
                if (ActualHours == null || ActualHours.Count == 0)
                {
                    return _actualTotal;
                }
                foreach (UnitHour u in ActualHours)
                {
                    _actualTotal += u.Hours;
                }
                return _actualTotal;
            }
            set { _actualTotal = value; }
        }
    }
    public class UnitHour
    {
        public string Title { get; set; }
        public double Hours { get; set; }
    }

    public class MonthBudget
    {
        public DateTime Month { get; set; }
        public double Hours { get; set; }
    }
}