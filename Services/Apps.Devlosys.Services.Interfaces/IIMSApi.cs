using Apps.Devlosys.Infrastructure.Models;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Apps.Devlosys.Services.Interfaces
{
    public interface IIMSApi
    {
        bool ItacConnection(AppSession session);

        int ItacShutDown();

        bool CheckUser(string station, string username, string password);

        int GetUserLevel(string station, string username);

        bool CheckSerialNumberState(string station, string snr, out string state, out string error);

        Task<(bool, string state, string error)> CheckSerialNumberStateAsync(string station, string snr);

        bool UploadState(string station, string snr, string[] inKeys, string[] inValues, out string[] results, out int code);

        bool UploadState(string station, string snr, long bookDate, out int code);

        Task<(bool, string[], int)> UploadStateAsync(string station, string snr, string[] inKeys, string[] inValues);

        Task<(bool, int)> UploadStateAsync(string station, string snr, long bookDate);

        bool GetSerialNumberInfo(string station, string snr, string[] inKeys, out string[] results, out int code);

        Task<(bool, string[], int)> GetSerialNumberInfoAsync(string station, string snr, string[] inKeys);

        bool GetBinData(string bin, out string[] data, out int code);

        bool GetStateForProductDeclaration(string station, string snr);

        bool GetBookDateMLS(string station, string snr, out string stationNumber, out string date, out int code);

        bool LockSnrItac(string station, string snr, out int code);

        Task LockSerialAsync(string station, string srn);

        bool UnlockSnrItac(string station, string snr, out int code);

        Task<List<PanelPositions>> GetPanelSNStateAsync(string station, string snr);

        bool LunchBooking(string station, string snr, out int code);

        int SetUserWhoMan(string station, string srn, string username);

        Task<int> SetUserWhoManAsync(string station, string srn, string username);

        //void StartMES(string WorkCenter, string productNumber, string eventDateTime, string serialNumber, string Qte, string CycleTime, AppSession _session);
        Task<(string status, string reason)> StartMESAsync(string WorkCenter, string productNumber, string eventDateTime,string serialNumber, 
              string Qte, string CycleTime, AppSession _session);

        public int VerifyMESAttr(string station, string serialNumber);

        Task<int> VerifyMESAttrAsync(string station, string serialNumber);

        public int AppendMESAttr(string station, string serialNumber);

        Task<int> AppendMESAttrAsync(string station, string serialNumber);

        string GetErrorText(int result);

        Task<string> GetErrorTextAsync(int result);

        string[] GetGroups();
    }
}
