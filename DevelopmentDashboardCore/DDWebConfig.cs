using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;
using System.Data;
using System.Reflection;
using System.ComponentModel;
using ConfigurationUtils;
namespace DevelopmentDashboardCore.WebConfigurations
{
    public class DDWebConfig : DatabaseConfigBase
    {
        private DDWebConfig() { }

        private static object lockFlag;

        private static DDWebConfig instance = null;

        public static DDWebConfig Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DDWebConfig();
                    FillProperties(instance, DevelopmentDashboardConfig.Instance.ConnectionString);
                }
                return instance;
            }
        }

        public void Reset()
        {
            instance = null;
        }

        #region DD Sessions
        #region Check Active Status Time In Minutes
        private int _checkActiveStatusTimeInMinutes = 5;
        [Description("Time period in minutes that user must be active in system to get status 'Active now'. Otherwise when user inactive longer than value of parameter, he gets status 'Not active now'")]
        public int CheckActiveStatusTimeInMinutes
        {
            get { return _checkActiveStatusTimeInMinutes; }
            set { _checkActiveStatusTimeInMinutes = value; }
        }
        #endregion

        #region Page Auto Update Period In Minutes
        private int _pageAutoUpdatePeriodInMinutes = 5;
        [Description("When elapsed, pages that uses this timer will automatically refresh")]
        public int PageAutoUpdatePeriodInMinutes
        {
            get { return _pageAutoUpdatePeriodInMinutes; }
            set { _pageAutoUpdatePeriodInMinutes = value; }
        }
        #endregion

        #region Yellow Status User Absence Period In Days
        private int _yellowStatusUserAbsencePeriodInDays = 2;
        [Description("Time period in days that user must be inactive to get 'yellow' status in the '10 last sessions' widget of the main page")]
        public int YellowStatusUserAbsencePeriodInDays
        {
            get { return _yellowStatusUserAbsencePeriodInDays; }
            set { _yellowStatusUserAbsencePeriodInDays = value; }
        }
        #endregion

        #region Red Status User Absence Period In Days
        private int _redStatusUserAbsencePeriodInDays = 4;
        [Description("Time period ins days that user must be inactive to get 'red' status in the '10 last sessions' widget of the main page")]
        public int RedStatusUserAbsencePeriodInDays
        {
            get { return _redStatusUserAbsencePeriodInDays; }
            set { _redStatusUserAbsencePeriodInDays = value; }
        }
        #endregion

        #region Token Life Time In hours
        private int _tokenLifeTimeInHours = 24;
        [Description("Life time of token that creates when users login the application")]
        public int TokenLifeTimeInHours
        {
            get { return _tokenLifeTimeInHours; }
            set { _tokenLifeTimeInHours = value; }
        }
        #endregion

        #endregion
    }
}
