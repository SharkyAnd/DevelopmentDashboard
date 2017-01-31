using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Utils.ConfigurationUtils;
using Utils;
using System.ComponentModel;
using DisplayName = Utils.ConfigurationUtils.DisplayNameAttribute;

namespace DevelopmentDashboardCore
{
    public class DevelopmentDashboardConfig :ConfigBase
    {
        private static DevelopmentDashboardConfig instance = null;

        public override string GetPath()
        {
            return FileName;
        }

        public override ConfigBase GetBaseInstance()
        {
            return Instance;
        }

        public override ConfigBase CreateDeepCopy()
        {
            DevelopmentDashboardConfig config = (DevelopmentDashboardConfig)MemberwiseClone();

            return config;
        }

        [XmlIgnore]
        public static DevelopmentDashboardConfig Instance
        {
            get
            {
                lock (lockFlag)
                {
                    if (instance == null)
                        instance = ReadFromFile<DevelopmentDashboardConfig>(FileName);
                    return instance;
                }
            }
        }

        public override void UpdateConfig(ConfigBase cb)
        {
            DevelopmentDashboardConfig c = cb as DevelopmentDashboardConfig;
            if (c != null)
            {
                instance = c;
                return;
            }
            else
                throw new ApplicationException("Argument for Config Update (cb) cannot be casted to DevelopmentDashboardConfig");
        }

        private static readonly string FileName = Utils.FileUtils.GetFullPathForFile("config_developmentdashboard.xml");

        #region Development Dashboard

        #region Database connection string
        //private string _DbConnectionString = "Data Source=(local);Initial Catalog=DevTest; Integrated Security = SSPI;";
        private string _DbConnectionString = "Data Source=SRV-SOHO2;Initial Catalog=DevelopmentDashboard; User ID = DDUser; Password = gffgh3SDF5754swer";

        [Category("Data Storage")]
        [Description("Database Connection String")]
        [DisplayName("Database Connection String")]
        [ChangeProperty(ChangeAction.NoAction)]
        public string ConnectionString
        {
            get { return _DbConnectionString; }
            set { _DbConnectionString = value; }
        }
        #endregion

        #endregion
    }
}
