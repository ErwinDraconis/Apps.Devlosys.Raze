using System;
using System.Configuration;

namespace Apps.Devlosys.Infrastructure.Models
{
    public class AppSession
    {
        private readonly Configuration configuration;

        private const string APP_STATE_NAME = "appState";
        private const string STATION_NAME = "station";
        private const string LABLING_STATION_NAME = "lablingStation";
        private const string PROJECT_TYPE_NAME = "PRO";
        private const string LABEL_TYPE_NAME = "DTC";
        private const string ITAC_SERVER_NAME = "ItacIp";
        private const string SHIPPING_PRINTER_NAME = "zebraip";
        private const string PRINT_MODE_NAME = "printmode";
        private const string PORT_NAME = "comport";
        private const string DOUBLE_CHECK_NAME = "DBC";
        private const string QUALITY_VALIDATION_NAME = "QLV";
        private const string FVT_INTERLOCK_NAME = "FLI";
        private const string PRINT_MANUELLE_LABEL_NAME = "MLSL";
        private const string WORK_CENTER_NAME = "WorkCenter";
        private const string MES_ACTIVE_NAME = "MES";
        private const string ITAC_INTERLOCK_NAME = "itacInterlock";
        private const string UPLOAD_TYPE_NAME = "uptype";
        private const string FTP_USERNAME_NAME = "ftpCredentialUsername";
        private const string FTP_PASSWORD_NAME = "ftpCredentialPassword";
        private const string BARFLOW_SERVER_NAME = "barflowServer";
        private const string BIN_NAME = "BIN";
        private const string LEAK_HOURS_NAME = "LeakHours";
        private const string DATE_FORMAT_NAME = "dateFormat";
        private const string TRAITEMENT_NAME = "TRAIT";
        private const string PortIL           = "PortIL";
        private const string BaudRatesIL      = "BaudRatesIL";
        private const string StopBitsIL       = "StopBitsIL";
        private const string ParitiesIL       = "ParitiesIL";
        private const string DataBitsIL       = "DataBitsIL";

        public AppSession()
        {
            configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        }

        #region Properties

        public string UserName { get; set; }

        public string Password { get; set; }

        public int Level { get; set; }

        public AppStates AppState
        {
            get => configuration.AppSettings.Settings[APP_STATE_NAME].Value == "1" ? AppStates.NORMAL : AppStates.BLOCKED;
            set => configuration.AppSettings.Settings[APP_STATE_NAME].Value = value == AppStates.NORMAL ? "1" : "0";
        }

        public string Station
        {
            get => configuration.AppSettings.Settings[STATION_NAME].Value.ToUpper();
            set => configuration.AppSettings.Settings[STATION_NAME].Value = value;
        }

        public string LablingStation
        {
            get => configuration.AppSettings.Settings[LABLING_STATION_NAME].Value.ToUpper();
            set => configuration.AppSettings.Settings[LABLING_STATION_NAME].Value = value;
        }

        public ProjectEnum ProjectType
        {
            get => (ProjectEnum)Enum.Parse(typeof(ProjectEnum), configuration.AppSettings.Settings[PROJECT_TYPE_NAME].Value);
            set => configuration.AppSettings.Settings[PROJECT_TYPE_NAME].Value = value.GetEnum();
        }

        public LabelTypeEnum LabelType
        {
            get => (LabelTypeEnum)Enum.Parse(typeof(LabelTypeEnum), configuration.AppSettings.Settings[LABEL_TYPE_NAME].Value.ToUpper());
            set => configuration.AppSettings.Settings[LABEL_TYPE_NAME].Value = value.GetEnum();
        }

        public string ItacServer
        {
            get => configuration.AppSettings.Settings[ITAC_SERVER_NAME].Value;
            set => configuration.AppSettings.Settings[ITAC_SERVER_NAME].Value = value;
        }

        public string ShippingPrinter
        {
            get => configuration.AppSettings.Settings[SHIPPING_PRINTER_NAME].Value;
            set => configuration.AppSettings.Settings[SHIPPING_PRINTER_NAME].Value = value;
        }

        public PrintModeEnum PrintMode
        {
            get => (PrintModeEnum)Enum.Parse(typeof(PrintModeEnum), configuration.AppSettings.Settings[PRINT_MODE_NAME].Value.ToUpper());
            set => configuration.AppSettings.Settings[PRINT_MODE_NAME].Value = value.GetEnum();
        }

        public string Port
        {
            get => configuration.AppSettings.Settings[PORT_NAME].Value.ToUpper();
            set => configuration.AppSettings.Settings[PORT_NAME].Value = value;
        }

        public bool IsDoubleCheck
        {
            get => configuration.AppSettings.Settings[DOUBLE_CHECK_NAME].Value.ToUpper() == "ON";
            set => configuration.AppSettings.Settings[DOUBLE_CHECK_NAME].Value = value ? "ON" : "OFF";
        }

        public bool IsQualityValidation
        {
            get => configuration.AppSettings.Settings[QUALITY_VALIDATION_NAME].Value.ToUpper() == "T";
            set => configuration.AppSettings.Settings[QUALITY_VALIDATION_NAME].Value = value ? "T" : "F";
        }

        public bool IsFVTInterlock
        {
            get => configuration.AppSettings.Settings[FVT_INTERLOCK_NAME].Value.ToUpper() == "T";
            set => configuration.AppSettings.Settings[FVT_INTERLOCK_NAME].Value = value ? "T" : "F";
        }

        public bool IsPrintManuelleLabel
        {
            get => configuration.AppSettings.Settings[PRINT_MANUELLE_LABEL_NAME].Value.ToUpper() == "T";
            set => configuration.AppSettings.Settings[PRINT_MANUELLE_LABEL_NAME].Value = value ? "T" : "F";
        }

        public string WorkCenter
        {
            get => configuration.AppSettings.Settings[WORK_CENTER_NAME].Value;
            set => configuration.AppSettings.Settings[WORK_CENTER_NAME].Value = value;
        }

        public bool IsMESActive
        {
            get => configuration.AppSettings.Settings[MES_ACTIVE_NAME].Value.ToUpper() == "ON";
            set => configuration.AppSettings.Settings[MES_ACTIVE_NAME].Value = value ? "ON" : "OFF";
        }

        public bool IsItacInterlock
        {
            get => configuration.AppSettings.Settings[ITAC_INTERLOCK_NAME].Value.ToUpper() == "ON";
            set => configuration.AppSettings.Settings[ITAC_INTERLOCK_NAME].Value = value ? "ON" : "OFF";
        }

        public UploadMethodEnum UploadType
        {
            get => (UploadMethodEnum)Enum.Parse(typeof(UploadMethodEnum), configuration.AppSettings.Settings[UPLOAD_TYPE_NAME].Value.ToUpper());
            set => configuration.AppSettings.Settings[UPLOAD_TYPE_NAME].Value = value.GetEnum();
        }

        public string FtpUsername
        {
            get => configuration.AppSettings.Settings[FTP_USERNAME_NAME].Value;
            set => configuration.AppSettings.Settings[FTP_USERNAME_NAME].Value = value;
        }

        public string FtpPassword
        {
            get => configuration.AppSettings.Settings[FTP_PASSWORD_NAME].Value;
            set => configuration.AppSettings.Settings[FTP_PASSWORD_NAME].Value = value;
        }

        public string BarFlowServer
        {
            get => configuration.AppSettings.Settings[BARFLOW_SERVER_NAME].Value;
            set => configuration.AppSettings.Settings[BARFLOW_SERVER_NAME].Value = value;
        }

        public string BIN
        {
            get => configuration.AppSettings.Settings[BIN_NAME].Value.ToUpper();
            set => configuration.AppSettings.Settings[BIN_NAME].Value = value;
        }

        public string LeakHours
        {
            get => configuration.AppSettings.Settings[LEAK_HOURS_NAME].Value.ToUpper();
            set => configuration.AppSettings.Settings[LEAK_HOURS_NAME].Value = value;
        }

        public string DateFormat
        {
            get => configuration.AppSettings.Settings[DATE_FORMAT_NAME].Value;
            set => configuration.AppSettings.Settings[DATE_FORMAT_NAME].Value = value;
        }

        public TraitementEnum Traitement
        {
            get => (TraitementEnum)Enum.Parse(typeof(TraitementEnum), configuration.AppSettings.Settings[TRAITEMENT_NAME].Value.ToUpper());
            set => configuration.AppSettings.Settings[TRAITEMENT_NAME].Value = value.GetEnum();
        }


        public string PortCOMInterlock
        {
            get => configuration.AppSettings.Settings[PortIL].Value;
            set => configuration.AppSettings.Settings[PortIL].Value = value;
        }

        public string BaudRatesInterLock
        {
            get => configuration.AppSettings.Settings[BaudRatesIL].Value;
            set => configuration.AppSettings.Settings[BaudRatesIL].Value = value;
        }

        public string StopBitsInterLock
        {
            get => configuration.AppSettings.Settings[StopBitsIL].Value;
            set => configuration.AppSettings.Settings[StopBitsIL].Value = value;
        }

        public string ParitiesInterLock
        {
            get => configuration.AppSettings.Settings[ParitiesIL].Value;
            set => configuration.AppSettings.Settings[ParitiesIL].Value = value;
        }

        public string DataBitsInterLock
        {
            get => configuration.AppSettings.Settings[DataBitsIL].Value;
            set => configuration.AppSettings.Settings[DataBitsIL].Value = value;
        }
        #endregion

        public void Save()
        {
            configuration.Save(ConfigurationSaveMode.Modified);
        }
    }
}
