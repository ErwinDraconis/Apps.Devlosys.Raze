using Apps.Devlosys.Infrastructure.Converters;
using Apps.Devlosys.Infrastructure.Helpers;
using Apps.Devlosys.Resources.I18N;
using System.ComponentModel;

namespace Apps.Devlosys.Infrastructure
{
    #region Keypad Enum

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum KeypadEnum
    {
        [Description("0")]
        pad0,
        [Description("1")]
        pad1,
        [Description("2")]
        pad2,
        [Description("3")]
        pad3,
        [Description("4")]
        pad4,
        [Description("5")]
        pad5,
        [Description("6")]
        pad6,
        [Description("7")]
        pad7,
        [Description("8")]
        pad8,
        [Description("9")]
        pad9,
        [Description(".")]
        DecimalSeparator,
        [Description("back")]
        padBack,
        [Description("entre")]
        padEnter
    }

    #endregion

    #region Alpha Keypad Enum

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum AlphaKeypadEnum
    {
        [Description("0")]
        padNum0,
        [Description("1")]
        padNum1,
        [Description("2")]
        padNum2,
        [Description("3")]
        padNum3,
        [Description("4")]
        padNum4,
        [Description("5")]
        padNum5,
        [Description("6")]
        padNum6,
        [Description("7")]
        padNum7,
        [Description("8")]
        padNum8,
        [Description("9")]
        padNum9,

        [Description("a")]
        padA,
        [Description("z")]
        padZ,
        [Description("e")]
        padE,
        [Description("r")]
        padR,
        [Description("t")]
        padT,
        [Description("y")]
        padY,
        [Description("u")]
        padU,
        [Description("i")]
        padI,
        [Description("o")]
        padO,
        [Description("p")]
        padP,
        [Description("q")]
        padQ,
        [Description("s")]
        padS,
        [Description("d")]
        padD,
        [Description("f")]
        padF,
        [Description("g")]
        padG,
        [Description("h")]
        padH,
        [Description("j")]
        padJ,
        [Description("k")]
        padK,
        [Description("l")]
        padL,
        [Description("m")]
        padM,
        [Description("w")]
        padW,
        [Description("x")]
        padX,
        [Description("c")]
        padC,
        [Description("v")]
        padV,
        [Description("b")]
        padB,
        [Description("n")]
        padN,
        [Description("é")]
        padE1,
        [Description("è")]
        padE2,
        [Description("à")]
        padA1,

        [Description("A")]
        padCapA,
        [Description("Z")]
        padCapZ,
        [Description("E")]
        padCapE,
        [Description("R")]
        padCapR,
        [Description("T")]
        padCapT,
        [Description("Y")]
        padCapY,
        [Description("U")]
        padCapU,
        [Description("I")]
        padCapI,
        [Description("O")]
        padCapO,
        [Description("P")]
        padCapP,
        [Description("Q")]
        padCapQ,
        [Description("S")]
        padCapS,
        [Description("D")]
        padCapD,
        [Description("F")]
        padCapF,
        [Description("G")]
        padCapG,
        [Description("H")]
        padCapH,
        [Description("J")]
        padCapJ,
        [Description("K")]
        padCapK,
        [Description("L")]
        padCapL,
        [Description("M")]
        padCapM,
        [Description("W")]
        padCapW,
        [Description("X")]
        padCapX,
        [Description("C")]
        padCapC,
        [Description("V")]
        padCapV,
        [Description("B")]
        padCapB,
        [Description("N")]
        padCapN,
        [Description("É")]
        padCapE1,
        [Description("È")]
        padCapE2,
        [Description("À")]
        padCapA1,

        [Description("ـ")]
        pad01,
        [Description("'")]
        pad02,
        [Description(":")]
        pad03,
        [Description("-")]
        pad04,
        [Description("/")]
        pad05,
        [Description(".")]
        pad06,
        [Description("@")]
        pad07,
        [Description(" ")]
        padSpace,
        [Description("back")]
        padBack,
        [Description("enter")]
        padEnter,
    }

    #endregion

    #region Data Size Unit Enum

    public enum DataSizeUnit { B, KB, MB, GB, TB, PB, EB }

    #endregion

    #region Traitement Enum

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum TraitementEnum
    {
        [LocalizedDescriptionAttribute("CheckBookingText", typeof(TraitmentResource))] BOOKING,
        [LocalizedDescriptionAttribute("CheckLablingText", typeof(TraitmentResource))] LABLING,
        [LocalizedDescriptionAttribute("CheckBothText", typeof(TraitmentResource))] BOTH,
        [LocalizedDescriptionAttribute("CheckMesText", typeof(TraitmentResource))] MES,
        [LocalizedDescriptionAttribute("CheckBinText", typeof(TraitmentResource))] BIN,
    }

    #endregion

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum ProjectEnum
    {
        [LocalizedDescriptionAttribute("ProjectVisualText", typeof(EnumsResource))] VISUAL,
        [LocalizedDescriptionAttribute("ProjectInvisibleText", typeof(EnumsResource))] INVISIBLE,
        [LocalizedDescriptionAttribute("ProjectFVTText", typeof(EnumsResource))] FVT,
        [LocalizedDescriptionAttribute("ProjectMLSText", typeof(EnumsResource))] MLS,
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum LabelTypeEnum
    {
        [LocalizedDescriptionAttribute("LabelTypeTG01Text", typeof(EnumsResource))] TG01,
        [LocalizedDescriptionAttribute("LabelTypeTSText", typeof(EnumsResource))] TS,
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum PrintModeEnum
    {
        [LocalizedDescriptionAttribute("PrintModeNetworkText", typeof(EnumsResource))] NET,
        [LocalizedDescriptionAttribute("PrintModeSRText", typeof(EnumsResource))] SR,
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum UploadMethodEnum
    {
        [LocalizedDescriptionAttribute("UploadMethodAPIText", typeof(EnumsResource))] API,
        [LocalizedDescriptionAttribute("UploadMethodFTPText", typeof(EnumsResource))] FTP
    }

    public enum iTAC_Check_SN_RSLT_ENUM
    {
        PART_OK    = 0,
        PART_FAIL  = 1,
        PART_SCRAP = 2,
        Unknown    = -1,
    }

    #region App States Enum

    public enum AppStates { BLOCKED, NORMAL, }

    #endregion
}
