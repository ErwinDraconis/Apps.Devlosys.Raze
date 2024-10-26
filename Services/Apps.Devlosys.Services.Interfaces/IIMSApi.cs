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

        bool UploadState(string station, string snr, string[] inKeys, string[] inValues, out string[] results, out int code);

        bool UploadState(string station, string snr, long bookDate, out int code);

        bool GetSerialNumberInfo(string station, string snr, string[] inKeys, out string[] results, out int code);

        bool GetBinData(string bin, out string[] data, out int code);

        bool GetStateForProductDeclaration(string station, string snr);

        bool GetBookDateMLS(string station, string snr, out string stationNumber, out string date, out int code);

        bool LockSnrItac(string station, string snr, out int code);

        bool UnlockSnrItac(string station, string snr, out int code);

        Task<List<PanelPositions>> GetPanelSNStateAsync(string station, string snr);

        bool LunchBooking(string station, string snr, out int code);

        int SetUserWhoMan(string station, string srn, string username);

        //void StartMES(string WorkCenter, string productNumber, string eventDateTime, string serialNumber, string Qte, string CycleTime, AppSession _session);
        Task<(string status, string reason)> StartMESAsync(string WorkCenter, string productNumber, string eventDateTime,string serialNumber, 
              string Qte, string CycleTime, AppSession _session);

        public int VerifyMESAttr(string station, string serialNumber);

        public int AppendMESAttr(string station, string serialNumber);

        string GetErrorText(int result);

        string[] GetGroups();
    }
}
