﻿using Microsoft.Win32;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text.RegularExpressions;

namespace Apps.Devlosys.Infrastructure.Helpers
{
    public static class SystemInfo
    {
        private const string ExtractionFailed = "<Extraction Failed>";

        public enum DotNetFramework
        {
            Unknown,
            V45,
            V451,
            V452,
            V46,
            V461,
            V462,
            V47,
            V471,
            V472OrLater
        }

        public static readonly string Caption;
        public static readonly string CSName;
        public static readonly string InstallDate;
        public static readonly string OSArchitecture;
        public static readonly string SerialNumber;
        public static readonly string Version;
        public static readonly long TotalVisibleMemorySize;
        public static readonly string CPUName;
        public static readonly DotNetFramework DotNetFrameworkVersion;

        static SystemInfo()
        {
            var extractionRegex = new Regex("(\\w+?) ?= ?\"(.+?)\";", RegexOptions.Compiled | RegexOptions.Multiline);

            var systemInfo = GetManagementInfo("SELECT * FROM Win32_OperatingSystem", extractionRegex);
            var processorInfo = GetManagementInfo("SELECT * FROM Win32_Processor", extractionRegex);

            Caption = systemInfo.GetValue("Caption");
            CSName = systemInfo.GetValue("CSName");
            InstallDate = systemInfo.GetValue("InstallDate");
            OSArchitecture = systemInfo.GetValue("OSArchitecture");
            SerialNumber = systemInfo.GetValue("SerialNumber");
            Version = systemInfo.GetValue("Version");
            TotalVisibleMemorySize = systemInfo.GetValue("TotalVisibleMemorySize").CastTo<long>();

            CPUName = processorInfo.GetValue("Name");

            DotNetFrameworkVersion = Get45PlusFromRegistry();
        }

        private static IReadOnlyDictionary<string, string> GetManagementInfo(string query, Regex regex)
        {
            try
            {
                var infoString = string.Empty;
                var result = new ManagementObjectSearcher(new ObjectQuery(query));

                using (var collection = result.Get())
                {
                    foreach (var item in collection)
                    {
                        var managementObject = item as ManagementObject;

                        infoString = managementObject?.GetText(TextFormat.Mof);
                        if (!string.IsNullOrEmpty(infoString)) break;
                    }
                }

                if (string.IsNullOrEmpty(infoString)) return null;

                return regex.Matches(infoString).OfType<Match>()
                    .Where(item => item.Success)
                    .ToDictionary(item => item.Groups[1].Value, item => item.Groups[2].Value);
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                return null;
            }
        }

        private static string GetValue(this IReadOnlyDictionary<string, string> info, string key)
        {
            return info != null && info.TryGetValue(key, out var value) ? value : ExtractionFailed;
        }

        private static DotNetFramework Get45PlusFromRegistry()
        {
            const string subKey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";

            using (RegistryKey ndpKey = RegistryKey
                .OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)
                .OpenSubKey(subKey))
            {
                return ndpKey?.GetValue("Release") != null
                    ? CheckFor45PlusVersion((int)ndpKey.GetValue("Release"))
                    : DotNetFramework.Unknown;
            }
        }

        private static DotNetFramework CheckFor45PlusVersion(int releaseKey)
        {
            if (releaseKey >= 461808)
                return DotNetFramework.V472OrLater;
            if (releaseKey >= 461308)
                return DotNetFramework.V471;
            if (releaseKey >= 460798)
                return DotNetFramework.V47;
            if (releaseKey >= 394802)
                return DotNetFramework.V462;
            if (releaseKey >= 394254)
                return DotNetFramework.V461;
            if (releaseKey >= 393295)
                return DotNetFramework.V46;
            if (releaseKey >= 379893)
                return DotNetFramework.V452;
            if (releaseKey >= 378675)
                return DotNetFramework.V451;
            if (releaseKey >= 378389)
                return DotNetFramework.V45;

            // This code should never execute. A non-null release key should mean
            // that 4.5 or later is installed.
            return DotNetFramework.V472OrLater;
        }
    }
}
