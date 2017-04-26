using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimulaDesign.RevitBimInfo
{
    /// <summary>
    /// Unit conversion - convert values from standard units to given units.
    /// </summary>
    public class UnitConversionFactors
    {
        public static UnitConversionFactors Nmm
        {
            get { return new UnitConversionFactors("mm", "N"); }
        }

        #region Class Member Variables

        /// <summary>
        /// Standard units
        /// </summary>
        private string m_FromUnits;

        /// <summary>
        /// Given units
        /// </summary>
        private string m_ToUnits;

        /// <summary>
        /// true: conversion available for given units
        /// </summary>
        private bool m_RatioSetUpSucceeded;

        /// <summary>
        /// To store length ratio
        /// </summary>
        private double m_LengthRatio;

        /// <summary>
        /// To store point load ratio
        /// </summary>
        private double m_PointLoadRatio;

        /// <summary>
        /// To store point load moment ratio
        /// </summary>
        private double m_PointLoadMomentRatio;

        /// <summary>
        /// To store line load ratio
        /// </summary>
        private double m_LineLoadRatio;

        /// <summary>
        /// To store line moment ratio
        /// </summary>
        private double m_LineMomentRatio;

        /// <summary>
        /// To store area load ratio
        /// </summary>
        private double m_AreaLoadRatio;

        /// <summary>
        /// To store stress ratio
        /// </summary>
        private double m_StressRatio;

        /// <summary>
        /// To store unit weight ratio
        /// </summary>
        private double m_UnitWeightRatio;

        /// <summary>
        /// To store point spring ratio
        /// </summary>
        private double m_PointSpringRatio;

        /// <summary>
        /// To store rotational point spring ratio
        /// </summary>
        private double m_RotationalPointSpringRatio;

        #endregion


        #region Class Public Properties
        /// <summary>
        /// Get FromUnits
        /// </summary>
        public string FromUnits
        {
            get
            {
                return m_FromUnits;
            }
        }

        /// <summary>
        /// Get ToUnits
        /// </summary>
        public string ToUnits
        {
            get
            {
                return m_ToUnits;
            }
        }

        /// <summary>
        /// Get RatioSetUpSucceeded
        /// </summary>
        public bool RatioSetUpSucceeded
        {
            get
            {
                return m_RatioSetUpSucceeded;
            }
        }

        /// <summary>
        /// Get length ratio 长度系数
        /// </summary>
        public double LengthRatio
        {
            get
            {
                return m_LengthRatio;
            }
        }

        /// <summary>
        /// Get point load ratio 负荷系数
        /// </summary>
        public double PointLoadRatio
        {
            get
            {
                return m_PointLoadRatio;
            }
        }

        /// <summary>
        /// Get point load moment ratio 点负载力矩
        /// </summary>
        public double PointLoadMomentRatio
        {
            get
            {
                return m_PointLoadMomentRatio;
            }
        }

        /// <summary>
        /// Get line load force ratio 线负载力
        /// </summary>
        public double LineLoadForceRatio
        {
            get
            {
                return m_LineLoadRatio;
            }
        }

        /// <summary>
        /// Get line load moment ratio 线负载力矩
        /// </summary>
        public double LineLoadMomentRatio
        {
            get
            {
                return m_LineMomentRatio;
            }
        }

        /// <summary>
        /// Get area load ratio 面负载
        /// </summary>
        public double AreaLoadRatio
        {
            get
            {
                return m_AreaLoadRatio;
            }
        }

        /// <summary>
        /// Get stress ratio 应力
        /// </summary>
        public double StressRatio
        {
            get
            {
                return m_StressRatio;
            }
        }

        /// <summary>
        /// Get unit weight ratio 重量
        /// </summary>
        public double UnitWeightRatio
        {
            get
            {
                return m_UnitWeightRatio;
            }
        }

        /// <summary>
        /// Get point sprint ratio
        /// </summary>
        public double PointSpringRatio
        {
            get
            {
                return m_PointSpringRatio;
            }
        }

        /// <summary>
        /// Get rotational point spring ratio 旋转
        /// </summary>
        public double RotationalPointSpringRatio
        {
            get
            {
                return m_RotationalPointSpringRatio;
            }
        }
        #endregion


        #region Class Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="lengthUnit">length unit</param>
        /// <param name="forceUnit">forth unit</param>
        public UnitConversionFactors(string lengthUnit, string forceUnit)
        {
            // Initialize standard factors
            InitializeConversionFactors();

            // Get length conversion factor
            double lenFactor = GetLengthConversionFactor(lengthUnit);
            if (!m_RatioSetUpSucceeded)
            {
                return;
            }

            // Get Force conversion factor
            double forceFactor = GetForceConversionFactor(forceUnit);
            if (!m_RatioSetUpSucceeded)
            {
                return;
            }

            // Set up all the conversion factors
            m_ToUnits = lengthUnit + '-' + forceUnit;
            SetUnitConversionFactors(lenFactor, forceFactor);
        }
        #endregion


        #region Class Implementation
        /// <summary>
        /// Initialize factor to convert internal units to standard ft, kips.
        /// </summary>
        private void InitializeConversionFactors()
        {
            // Internally Revit stores length in feet and other quantities in metric units.
            // Thus the derived unit for force is stored in a non-standard unit: kg-ft/s**2.
            // For example, m_PointLoadRatio below equals 1 (kip) / 14593.90 (kg-ft/s**2)
            m_FromUnits = "ft-kips";
            m_ToUnits = "";
            m_LengthRatio = 1;
            m_PointLoadRatio = 0.00006852176585679176;
            m_PointLoadMomentRatio = 0.00006852176585679176;
            m_LineLoadRatio = 0.00006852176585679176;
            m_LineMomentRatio = 0.00006852176585679176;
            m_AreaLoadRatio = 0.00006852176585679176;
            m_StressRatio = 0.00006852176585679176;
            m_UnitWeightRatio = 0.00006852176585679176;
            m_PointSpringRatio = 0.00006852176585679176;
            m_RotationalPointSpringRatio = 0.00006852176585679176 * 180 / Math.PI;   // Revit uses degrees, Midas uses radians
            m_RatioSetUpSucceeded = false;
        }

        /// <summary>
        /// Get length conversation factor
        /// </summary>
        /// <param name="lengthUnit">length unit type</param>
        /// <returns>length conversation factor</returns>
        private double GetLengthConversionFactor(string lengthUnit)
        {
            bool unitAvailable = true;
            double lenFac = 0;

            switch (lengthUnit)
            {
                case "ft":
                    lenFac = 1;
                    break;
                case "in":
                    lenFac = 12;
                    break;
                case "m":
                    lenFac = 0.3048;
                    break;
                case "cm":
                    lenFac = 30.48;
                    break;
                case "mm":
                    lenFac = 304.8;
                    break;
                default:
                    unitAvailable = false;
                    break;
            }
            m_RatioSetUpSucceeded = unitAvailable;
            return lenFac;
        }

        /// <summary>
        /// Get force conversation factor
        /// </summary>
        /// <param name="forceUnit">force unit</param>
        /// <returns>force conversation factor</returns>
        private double GetForceConversionFactor(string forceUnit)
        {
            bool unitAvailable = true;
            double forceFac = 0;

            switch (forceUnit)
            {
                case "kips":
                    forceFac = 1;
                    break;
                case "lbf":
                    forceFac = 1000;
                    break;
                case "kN":
                    forceFac = 4.4482216152605;
                    break;
                case "N":
                    forceFac = 4448.2216152605;
                    break;
                case "tonf": // metric tonne
                    forceFac = 4.4482216152605 * 0.101971999794;
                    break;
                case "kgf":
                    forceFac = 4448.2216152605 * 0.101971999794;
                    break;
                default:
                    unitAvailable = false;
                    break;
            }
            m_RatioSetUpSucceeded = unitAvailable;
            return forceFac;
        }

        /// <summary>
        /// Set length and force unit conversation factor
        /// </summary>
        /// <param name="lenFac">length unit factor</param>
        /// <param name="forceFac">force unit factor</param>
        private void SetUnitConversionFactors(double lenFac, double forceFac)
        {
            m_LengthRatio *= (lenFac);
            m_PointLoadRatio *= (forceFac);
            m_PointLoadMomentRatio *= (forceFac * lenFac);
            m_LineLoadRatio *= (forceFac / lenFac);
            m_LineMomentRatio *= (forceFac);
            m_AreaLoadRatio *= (forceFac / lenFac / lenFac);
            m_StressRatio *= (forceFac / lenFac / lenFac);
            m_UnitWeightRatio *= (forceFac / lenFac / lenFac / lenFac);
            m_PointSpringRatio *= (forceFac / lenFac);
            m_RotationalPointSpringRatio *= (forceFac * lenFac);
        }
        #endregion
    }
}
