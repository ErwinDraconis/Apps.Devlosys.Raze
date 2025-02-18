using Apps.Devlosys.Infrastructure;
using Apps.Devlosys.Infrastructure.Models;
using Apps.Devlosys.Services.Interfaces;
using com.itac.artes;
using com.itac.mes.imsapi.client.dotnet;
using com.itac.mes.imsapi.domain.container;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using static com.itac.mes.imsapi.client.dotnet.IMSApiDotNetConstants;

namespace Apps.Devlosys.Services
{
    public class IMSApi : IIMSApi
    {
        private readonly string BIN_DATA_STATION_NUMBER = "TG01-SMT-LAS-L01-10";
        private IIMSApiDotNet imsapi = null;
        private IMSApiSessionContextStruct sessionContext = null;

        private static readonly HttpClient httpClient = new HttpClient();

        #region Properties

        public string ItacVersion { get; set; }

        #endregion

        #region Public Methods

        public bool ItacConnection(AppSession session)

        {

            IMSApiDotNetBase.setProperty(ArtesPropertyNames.PROP_ARTES_APPID, "IMSApiDotNetTestClient");

            IMSApiDotNetBase.setProperty(ArtesPropertyNames.PROP_ARTES_CLUSTERNODES, $"http://{session.ItacServer}:8080/mes/");

            Log.Information($"iTAC server at : {session.ItacServer}");



            imsapi = IMSApiDotNet.loadLibrary();

            IMSApiGetLibraryVersion();



            return IMSApiInit() && RegLogin(session.Station);

        }

        public int ItacShutDown()

        {

            try

            {

#if DEBUG

                int result = 0;

#else

                int result = imsapi.imsapiShutdown();

#endif

                return result;

            }

            catch (Exception ex)

            {

                Log.Error($"An error occurred during iTAC shutdown: {ex.Message} - Stack trace {ex.StackTrace}");



                return -1;

            }

        }

        public bool CheckUser(string station, string username, string password)

        {

            IMSApiSessionValidationStruct sessValData = new()

            {

                stationNumber = station,

                stationPassword = "",

                user = username,

                password = password,

                client = "01",

                registrationType = "U",

                systemIdentifier = "01"

            };



            Log.Information($"Trying to connect to iTAC server with following credentials : user name {username} ");



            int result = imsapi.regLogin(sessValData, out IMSApiSessionContextStruct newSessionContext);

            sessionContext = newSessionContext;



            if (result != RES_OK)

            {

                PrintErrorText(result, "CheckUser");

                return false;

            }



            Log.Information("result value: <{result}>", result);

            Log.Information("new session established.");

            Log.Information("===== SessionId: <{sessionId}>", sessionContext.sessionId);

            Log.Information("===== locale: <{locale}>", sessionContext.locale);



            return true;

        }

        public int GetUserLevel(string station, string username)

        {

            string[] attributeCodeArray = { "razeLevel" };

            string[] attributeResultKeys = { "ATTRIBUTE_CODE", "ATTRIBUTE_VALUE", "ERROR_CODE" };



            return imsapi.attribGetAttributeValues(sessionContext, station, 16, username, "-1", attributeCodeArray, 0, attributeResultKeys, out string[] results) != RES_OK

                ? -1

                : int.Parse(results[1]);

        }

        public bool CheckSerialNumberState(string station, string snr, out string state, out string error)

        {

            string[] resultKeys = { "SERIAL_NUMBER_STATE", "ERROR_CODE" };



            int result = imsapi.trCheckSerialNumberState(sessionContext, station, 2, 0, snr, "-1", resultKeys, out string[] outResults);



            state = outResults[0];

            error = outResults[1];



            return result == RES_OK;

        }

        public async Task<(bool Success, string SnState, int ErrorCode)> CheckSerialNumberStateAsync(string station, string snr)

        {

            Log.Information("Entered CheckSerialNumberStateAsync");

            string[] resultKeys = ["SERIAL_NUMBER_STATE", "ERROR_CODE"];

            string snState = string.Empty;

            int errorCode = -1;



            bool success = await Task.Run(() =>

            {

                int result = imsapi.trCheckSerialNumberState(sessionContext, station, 2, 0, snr, "-1", resultKeys, out string[] outResults);



                if (outResults.Length >= 2)

                {

                    snState = outResults[0];

                    int.TryParse(outResults[1], out errorCode);

                }



                return result == RES_OK;

            });

            Log.Information($"CheckSerialNumberStateAsync {snr} snState {snState} errorCode {errorCode}");

            return (success, snState, errorCode);

        }

        public bool UploadState(string station, string snr, string[] inKeys, string[] inValues, out string[] results, out int code)

        {

            var result = imsapi.trUploadState(sessionContext, station, 2, snr, "-1", 0, 0, -1L, 0f, inKeys, inValues, out string[] outResults);

            results = outResults;

            code = result;



            return result == RES_OK;

        }

        public bool UploadState(string station, string snr, long bookDate, out int code)

        {

            var result = imsapi.trUploadState(sessionContext, station, 2, snr, "-1", 0, 0, bookDate, -1f, null, null, out _);

            code = result;

            return result == RES_OK;

        }

        public async Task<(bool, string[], int)> UploadStateAsync(string station, string snr, string[] inKeys, string[] inValues)

        {

            return await Task.Run(() =>

            {

                var result = imsapi.trUploadState(sessionContext, station, 2, snr, "-1", 0, 0, -1L, 0f, inKeys, inValues, out string[] outResults);

                Log.Information($"UploadStateAsync SN {snr} RSLT {result}");

                return (result == RES_OK, outResults, result);

            });

        }

        public async Task<(bool, int)> UploadStateAsync(string station, string snr, long bookDate)

        {

            return await Task.Run(() =>

            {

                var result = imsapi.trUploadState(sessionContext, station, 2, snr, "-1", 0, 0, bookDate, -1f, null, null, out _);

                return (result == RES_OK, result);

            });

        }

        public bool GetSerialNumberInfo(string station, string snr, string[] inKeys, out string[] results, out int code)

        {

            var result = imsapi.trGetSerialNumberInfo(sessionContext, station, snr, "-1", inKeys, out string[] outResults);

            results = outResults;

            code = result;



            return result == RES_OK;

        }

        public async Task<(bool, string[], int)> GetSerialNumberInfoAsync(string station, string snr, string[] inKeys)

        {

            return await Task.Run(() =>

            {

                var result = imsapi.trGetSerialNumberInfo(sessionContext, station, snr, "-1", inKeys, out string[] outResults);

                Log.Information($"GetSerialNumberInfoAsync SNR {snr} result {result}");

                return (result == RES_OK, outResults, result);

            });

        }

        public bool GetStateForProductDeclaration(string station, string snr)

        {

            string[] inArgs = new string[2] { "SERIAL_NUMBER_STATE", "WORKORDER_NUMBER" };



            int result = imsapi.trCheckSerialNumberState(sessionContext, station, 2, 0, snr, "-1", inArgs, out _);



            return result == RES_OK;

        }

        public bool GetBookDateMLS(string station, string snr, out string stationNumber, out string date, out int code)

        {

            string[] inArgs = { "BOOK_DATE", "STATION_NUMBER" };



            int result = imsapi.trGetSerialNumberUploadInfo(sessionContext, station, 2, snr, "-1", 0, inArgs, out string[] outArgs);



            date = string.Empty;

            stationNumber = string.Empty;

            code = result;



            if (outArgs.Length > 0)

            {

                date = outArgs[0];

                stationNumber = outArgs[1];

            }



            return result == RES_OK;

        }

        public bool LockSnrItac(string station, string snr, out int code)

        {

            string[] inArgs = { "ERROR_CODE", "SERIAL_NUMBER" };

            string[] inValues = { "0", snr };



            int result = imsapi.lockObjects(sessionContext, station, 0, "-1", "-1", -1L, 0, inArgs, inValues, out var _);

            code = result;



            return result == RES_OK;

        }

        public bool UnlockSnrItac(string station, string snr, out int code)

        {

            string[] inArgs = { "ERROR_CODE", "SERIAL_NUMBER" };

            string[] inValues = { "0", snr };



            int result = imsapi.lockUnlockObjects(sessionContext, station, 0, "-1", "-1", 0, -1L, 0, inArgs, inValues, out var _);

            code = result;



            return result == RES_OK;

        }

        public bool LunchBooking(string station, string snr, out int code)

        {

            string[] inArgs = { "SERIAL_NUMBER_STATE" };



            int result = imsapi.trUploadState(sessionContext, station, 2, snr, "-1", 0, 0, -1L, 0f, inArgs, null, out _);

            code = result;



            return result == RES_OK;

        }

        public async Task<List<PanelPositions>> GetPanelSNStateAsync(string station, string snr)
        {
            List<PanelPositions> panelRslt = new List<PanelPositions>();

#if DEBUG
            var fictiveNumberOfBoards = new Random().Next(5, 16);
            Random random = new Random();
            for (int i = 0; i < fictiveNumberOfBoards; i++)
            {
                panelRslt.Add(new PanelPositions

                {

                    PositionNumber = i + 1,

                    SerialNumber = "99999_99999_99999" + random.Next(0, 1000).ToString(),

                    Status = random.Next(0, 4),
                    DisplayStatus = (int)iTAC_Check_SN_RSLT_ENUM.PART_PENDING,

                });
                await Task.Delay(100);
            }

            return panelRslt;

#endif
            int processLayer       = 2;
            int checkMultiBoard    = 1;
            string serialNumberPos = "-1";
            string[] SnStateResultKeys = new[] { "SERIAL_NUMBER_POS", "SERIAL_NUMBER", "SERIAL_NUMBER_STATE" };
            string[] SnStateResultValues;

            await Task.Run(() =>
            {
                int result = imsapi.trCheckSerialNumberState(sessionContext, station, processLayer, checkMultiBoard, snr, serialNumberPos,
                                                                             SnStateResultKeys, out SnStateResultValues);

                if (!string.IsNullOrEmpty(SnStateResultValues[0]) && SnStateResultValues.Length > 0)
                {
                    for (int i = 0; i < SnStateResultValues.Length; i += SnStateResultKeys.Length)
                    {
                        try
                        {
                            panelRslt.Add(new PanelPositions
                            {
                                PositionNumber = int.Parse(SnStateResultValues[i]),
                                SerialNumber   = SnStateResultValues[i + 1],
                                Status = int.Parse(SnStateResultValues[i + 2]),
                                DisplayStatus = (int)iTAC_Check_SN_RSLT_ENUM.PART_PENDING,
                            });
                        }
                        catch (Exception ex)
                        {
                            Log.Error($"Error processing SN {SnStateResultValues[i + 1]}, item at index {i}, Function {nameof(GetPanelSNStateAsync)} , error: {ex.Message}");
                            continue; 
                        }
                    }
                }
            });

            return panelRslt;

        }

        public int SetUserWhoMan(string station, string srn, string username)

        {

            string[] attributeUploadKeys = new string[3] { "ATTRIBUTE_CODE", "ATTRIBUTE_VALUE", "ERROR_CODE" };

            string[] attributeUploadValues = new string[3] { "razeUser", $"{username} on : {DateTime.Now}", "0" };



            return imsapi.attribAppendAttributeValues(sessionContext, station, 0, srn, "-1", -1L, 0, attributeUploadKeys, attributeUploadValues, out string[] results) == RES_OK

                ? 0

                : int.Parse(results[1]);

        }

        public async Task<int> SetUserWhoManAsync(string station, string srn, string username)

        {

            string[] attributeUploadKeys = ["ATTRIBUTE_CODE", "ATTRIBUTE_VALUE", "ERROR_CODE"];

            string[] attributeUploadValues = ["razeUser", $"{username} on : {DateTime.Now}", "0"];



            return await Task.Run(() =>

            {

                int result = imsapi.attribAppendAttributeValues(sessionContext, station, 0, srn, "-1", -1L, 0, attributeUploadKeys, attributeUploadValues, out string[] results);



                Log.Information($"SetUserWhoManAsync : SNR {srn} API result {result}");

                /*return result == RES_OK

                ? 0

                : (results.Length > 1 && int.TryParse(results[2], out int parsedResult)

                ? parsedResult

                : -1);//throw new InvalidOperationException("Failed to parse error code."));*/

                return 0;

            });

        }

        public async Task<(bool status, string reason)> StartMESAsync(string WorkCenter, string productNumber, string eventDateTime, string serialNumber, string Qte, string CycleTime, AppSession _session)
        {
            string motherForm = $"<?xml version=\"1.0\"?><FSA_INT_FlatFileManager xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:noNamespaceSchemaLocation=\"C:/Inetpub/wwwroot/SchemaRepository/XMLSchemas/FlexNet/FSA_INT_FlatFileManager.xsd\" Version=\"1.0\"><FIInvocationSynchronousEvent NodeType=\"FIInvocation\"><StandardOperation><OperationResolutionMethod>ByOperationCode</OperationResolutionMethod><OperationCode>SVC_MES_MI_ProductionDeclaration</OperationCode></StandardOperation><Parameters><Inputs><InputName>WorkCenter</InputName><InputValue>{WorkCenter}</InputValue></Inputs><Inputs><InputName>ProductNo</InputName><InputValue>{productNumber}</InputValue></Inputs><Inputs><InputName>EventDateTime</InputName><InputValue>{eventDateTime}</InputValue></Inputs><Inputs><InputName>SerialNo</InputName><InputValue>{serialNumber}</InputValue></Inputs><Inputs><InputName>Quantity</InputName><InputValue>{Qte}</InputValue></Inputs><Inputs><InputName>CycleTime</InputName><InputValue>{CycleTime}</InputValue></Inputs></Parameters></FIInvocationSynchronousEvent></FSA_INT_FlatFileManager>";
            string prettyXML = PrettyXml(motherForm);

            try
            {
                if (_session.UploadType == Infrastructure.UploadMethodEnum.API)
                {
                    // save XML file
                    await SaveXmlToFileAsync(prettyXML, productNumber, serialNumber);

                    var apiResponse = await SendApiAsync(prettyXML, _session.BarFlowServer);

                    if (apiResponse.status == false)
                    {
                        Log.Error($"API call for SN [{serialNumber}] failed : {apiResponse.reason}");

                        // Save XML to file only if MES has failed
                        //if (_session.IsMESXMLActive)
                        //{
                        //    await SaveXmlToFileAsync(prettyXML, productNumber, serialNumber);
                        //}

                        return (false, apiResponse.reason);
                    }
                    else
                    {
                        Log.Information($"API call for SN [{serialNumber}] Ok : " + apiResponse.status);
                        return (true, "PASS");
                    }
                }
                else
                {
                    return await UploadFileAsync(prettyXML, _session.FtpUsername, _session.FtpPassword);
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error in StartMES: {ex.Message}");
            }
        }

        private async Task SaveXmlToFileAsync(string xmlContent, string productNumber, string serialNumber)
        {
            Log.Information($"Saving MES xml file after MES booking has failed for SN {serialNumber}, has started.");

            if (string.IsNullOrWhiteSpace(xmlContent))
            {
                Log.Warning("XML content is empty; skipping file save.");
                return;
            }

            try
            {
                // Remove special characters
                string SanitizeInput(string input)
                {
                    return Regex.Replace(input, @"[^a-zA-Z0-9_]", "_");
                }

                productNumber = SanitizeInput(productNumber);
                serialNumber  = SanitizeInput(serialNumber);

                string baseDirectory  = AppDomain.CurrentDomain.BaseDirectory;
                string errorDirectory = Path.Combine(baseDirectory, "MesXml");

                if (!Directory.Exists(errorDirectory))
                {
                    Directory.CreateDirectory(errorDirectory);
                }

                // Format the file name with sanitized inputs and current date/time
                string sanitizedDateTime = DateTime.Now.ToString("ddHHmmss");
                string fileName = $"{serialNumber}_{sanitizedDateTime}.xml";
                string filePath = Path.Combine(errorDirectory, fileName);

                // Ensure the file path is valid
                if (filePath.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
                {
                    Log.Error($"Invalid file path: {filePath}");
                    return;
                }

                using (var writer = new StreamWriter(filePath, false))
                {
                    await writer.WriteAsync(xmlContent);
                    await writer.FlushAsync();
                }

                Log.Information("XML file generated and saved successfully.");
            }
            catch (Exception ex)
            {
                Log.Error($"MES failed for SN {serialNumber}, Failed to save XML to file: {ex.Message}");
            }
        }

        public int VerifyMESAttr(string station, string serialNumber)

        {

            string[] attributeCodeArray = { "MES_Booking" };

            string[] attributeResultKeys = { "ATTRIBUTE_CODE", "ATTRIBUTE_VALUE", "ERROR_CODE" };



            return imsapi.attribGetAttributeValues(sessionContext, station, 0, serialNumber, null, attributeCodeArray, 0, attributeResultKeys, out string[] results) != RES_OK

                ? -1

                : int.Parse(results[1]);

        }

        public async Task<int> VerifyMESAttrAsync(string station, string serialNumber)
        {
            string[] attributeCodeArray = ["MES_Booking"];

            string[] attributeResultKeys = ["ATTRIBUTE_CODE", "ATTRIBUTE_VALUE", "ERROR_CODE"];

            return await Task.Run(() =>

            {
                int result = imsapi.attribGetAttributeValues(sessionContext, station, 0, serialNumber, null, attributeCodeArray, 0, attributeResultKeys, out string[] results);

                return result;

                /*if (results.Length > 1)
                {
                    return int.TryParse(results[1], out int parsedResult) ? parsedResult : -1;

                }
                else
                {
                    // -911 mean no attribute foud
                    return result;
                }*/

            });

        }

        public int AppendMESAttr(string station, string serialNumber)
        {

            string[] attributeResultKeys = new string[3] { "ATTRIBUTE_CODE", "ATTRIBUTE_VALUE", "ERROR_CODE" };

            string[] attributeUploadValues = new string[3] { "MES_Booking", "1", "0" };

            return imsapi.attribAppendAttributeValues(sessionContext, station, 0, serialNumber, null, -1L, 1, attributeResultKeys, attributeUploadValues, out string[] results) == RES_OK
                   ? 0  : int.Parse(results[1]);

        }

        public async Task<int> AppendMESAttrAsync(string station, string serialNumber)
        {
            string[] attributeResultKeys = ["ATTRIBUTE_CODE", "ATTRIBUTE_VALUE", "ERROR_CODE"];

            string[] attributeUploadValues = ["MES_Booking", "1", "0"];

            return await Task.Run(() =>

            {

                int result = imsapi.attribAppendAttributeValues(sessionContext, station, 0, serialNumber, null, -1L, 1, attributeResultKeys, attributeUploadValues, out string[] results);

                Log.Information($"AppendMESAttrAsync SNR : {serialNumber} API Result {result}");

                return result == RES_OK  ? 0  : -1; 

            });

        }

        public bool GetBinData(string bin, out string[] data, out int code)

        {

            KeyValue[] materialBinFilters = new KeyValue[2]

            {

                new KeyValue("MATERIAL_BIN_NUMBER", bin),

                new KeyValue("MAX_ROWS", "100")

            };

            string[] materialBinResultKeys = new string[7]

            {

                "MATERIAL_BIN_PART_NUMBER",

                "MATERIAL_BIN_QTY_ACTUAL",

                "MATERIAL_BIN_QTY_TOTAL",

                "MATERIAL_BIN_NUMBER",

                "SUPPLIER_NUMBER",

                "PART_DESC",

                "SUPPLIER_NAME"

            };



            code = imsapi.mlGetMaterialBinData(sessionContext, BIN_DATA_STATION_NUMBER, materialBinFilters, new AttributeInfo[0], materialBinResultKeys, out string[] output);

            if (code != RES_OK)

            {

                data = new string[] { };

                return false;

            }



            data = output;



            WriteToFile(output[3].ToString() + "|" + output[2].ToString() + "|" + output[1].ToString() + "|" + output[0].ToString() + "|" + output[5].ToString());



            return true;

        }

        public string GetErrorText(int result)

        {

            imsapi.imsapiGetErrorText(sessionContext, result, out string errorText);



            return errorText;

        }

        public async Task<string> GetErrorTextAsync(int result)

        {

            return await Task.Run(() =>

            {

                imsapi.imsapiGetErrorText(sessionContext, result, out string errorText);

                return errorText;

            });

        }

        public string[] GetGroups()

        {

            imsapi.imsapiGetGroups(sessionContext, out ImsApiGroupStruct[] groups);



            return groups.Select(a => $"{a.groupName} - {a.groupDescr}").ToArray();

        }

        #endregion

        #region Private Methods

        private void IMSApiGetLibraryVersion()

        {

            _ = imsapi.imsapiGetLibraryVersion(out string version);



            ItacVersion = version;

            Log.Information(version);

        }

        private bool IMSApiInit()

        {

            int result = imsapi.imsapiInit();



            if (result != RES_OK)

            {

                string message = result switch

                {

                    RES_ERR_IMSAPI_ALREADY_INITIALIZED => "IMSApi already initialized",

                    RES_ERR_IMSAPI_LOCATOR_INIT_FAILED => "IMSApi locator inti faild",

                    RES_ERR_IMSAPI_SERVICE_LOOKUP => "IMSApi service lookup",

                    _ => "UNKNOW",

                };



                Log.Error("Error Code {result} : {message}", result, message);



                return false;

            }



            return true;

        }

        private bool RegLogin(string station)
        {

            IMSApiSessionValidationStruct sessValData = new()

            {

                stationNumber = station,

                stationPassword = "",

                user = "",

                password = "",

                client = "01",

                registrationType = "S",

                systemIdentifier = "01"

            };



            int result = imsapi.regLogin(sessValData, out IMSApiSessionContextStruct newSessionContext);

            sessionContext = newSessionContext;



            if (result != RES_OK)

            {

                PrintErrorText(result, "RegisterLogin");

                return false;

            }



            Log.Information("result value: <{result}>", result);

            Log.Information("new session established.");

            Log.Information("===== SessionId: <{sessionId}>", sessionContext.sessionId);

            Log.Information("===== locale: <{locale}>", sessionContext.locale);



            return true;

        }

        private void LockSerial(string station, string srn)

        {

            imsapi.attribRemoveAttributeValue(sessionContext, station, 0, srn, "-1", "MesLock", "MesLock");



            string[] attributeUploadKeys = new string[3] { "ATTRIBUTE_CODE", "ATTRIBUTE_VALUE", "ERROR_CODE" };

            string[] attributeUploadValues = new string[3] { "MesLock", "1", "0" };



            imsapi.attribAppendAttributeValues(sessionContext, station, 0, srn, "-1", -1L, 0, attributeUploadKeys, attributeUploadValues, out _);

        }

        public async Task LockSerialAsync(string station, string srn)

        {

            await Task.Run(() =>

                imsapi.attribRemoveAttributeValue(sessionContext, station, 0, srn, "-1", "MesLock", "MesLock")

            );



            string[] attributeUploadKeys = ["ATTRIBUTE_CODE", "ATTRIBUTE_VALUE", "ERROR_CODE"];

            string[] attributeUploadValues = ["MesLock", "1", "0"];



            var result = await Task.Run(() =>

                imsapi.attribAppendAttributeValues(sessionContext, station, 0, srn, "-1", -1L, 0, attributeUploadKeys, attributeUploadValues, out _)

            );

            Log.Information($"LockSerialAsync SNR {srn} RSLT {result}");

        }

        public async Task<bool> SplitSnFromPanelAsync(string station, string snr)

        {

            string[] splitPanelKeys = { "ERROR_CODE", "SERIAL_NUMBER_REF", "SERIAL_NUMBER_REF_POS" };

            string[] splitPanelValues = { "" };



            var result = await Task.Run(() =>

            {

                return imsapi.trSplitPanel(sessionContext, station, snr, "-1", -1, splitPanelKeys, splitPanelValues, out string[] splitPanelResults);

            });


            if (!string.IsNullOrEmpty(splitPanelValues[0]))
            Log.Information($"SplitSnFromPanel SNR {snr}, function result {result}, RSLT {splitPanelValues[0]}");



            return result == RES_OK;

        }

        public async Task<(bool Success, string Message)> UploadFileAsync(string beautyXML, string username, string password)
        {

            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "xml");

            if (!Directory.Exists(path))

            {

                Directory.CreateDirectory(path);

            }



            string filePath = Path.Combine(path, $"{DateTime.Now:yyyyMMddHHmm}_fileToUpload.xml");



            try

            {

                using StreamWriter fsWrite = new(filePath);

                await fsWrite.WriteLineAsync(beautyXML);



                // Upload the file to FTP server and capture the result

                var (rslt, message) = await UploadFileToFtpAsync("ftp://10.172.4.117/FLEXNET/FLATFILES/TODO/", filePath, username, password);

                Log.Information($"UploadFileAsync : {rslt} {message}");

                return (rslt, message);

            }

            catch (Exception ex)

            {

                return (false, $"Failed to write or upload file: {ex.Message}");

            }

        }

        /*private static async Task<(bool status, string reason)> SendApiAsync(string body, string barflow)
        {
            try
            {
                string URL = "http://" + barflow + "/ReceiveXML/Receive.aspx";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
                request.Method = "POST";
                request.ContentType = "text/xml";
                request.ContentLength = body.Length;

                using (StreamWriter requestWriter = new StreamWriter(await request.GetRequestStreamAsync(), Encoding.ASCII))
                {
                    await requestWriter.WriteAsync(body);
                }

                using (HttpWebResponse httpResponse = (HttpWebResponse)await request.GetResponseAsync())
                {
                    if (httpResponse.StatusCode == HttpStatusCode.OK)
                    {
                        return (true, null);
                    }

                    else
                    {
                        return (false, $"Unexpected response code: {(int)httpResponse.StatusCode} {httpResponse.StatusDescription}");
                    }
                }

            }

            catch (WebException webEx)
            {
                if (webEx.Response is HttpWebResponse errorResponse)
                {
                    return (false, $"Web exception: {(int)errorResponse.StatusCode} {errorResponse.StatusDescription}");
                }

                return (false, $"Web exception: {webEx.Message}");
            }

            catch (Exception ex)
            {
                return (false, $"General exception: {ex.Message}");
            }

        }*/

        private static async Task<(bool status, string reason)> SendApiAsync(string body, string barflow)
        {
            try
            {
                string URL  = $"http://{barflow}/ReceiveXML/Receive.aspx";
                var content = new StringContent(body, Encoding.ASCII, "text/xml");

                using (HttpResponseMessage response = await httpClient.PostAsync(URL, content))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        return (true, null);
                    }
                    else
                    {
                        return (false, $"Unexpected response code: {(int)response.StatusCode} {response.ReasonPhrase}");
                    }
                }
            }
            catch (HttpRequestException httpEx)
            {
                return (false, $"HTTP request exception: {httpEx.Message}");
            }
            catch (Exception ex)
            {
                return (false, $"General exception: {ex.Message}");
            }
        }

        public void UploadFileToFtp(string url, string filePath, string username, string password)

        {

            try

            {

                string fileName = Path.GetFileName(filePath);

                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url + fileName);

                request.Method = "STOR";

                request.Credentials = new NetworkCredential(username, password);

                request.UsePassive = true;

                request.UseBinary = true;

                request.KeepAlive = false;

                using (FileStream fileStream = File.OpenRead(filePath))

                {

                    using Stream requestStream = request.GetRequestStream();

                    fileStream.CopyTo(requestStream);

                    requestStream.Close();

                }

                FtpWebResponse response = (FtpWebResponse)request.GetResponse();



                response.Close();

            }

            catch (Exception ex)

            {

                throw ex;

            }

        }

        public async Task<(bool Success, string Message)> UploadFileToFtpAsync(string url, string filePath, string username, string password)

        {

            try

            {

                string fileName = Path.GetFileName(filePath);

                var request = (FtpWebRequest)WebRequest.Create(url + fileName);

                request.Method = WebRequestMethods.Ftp.UploadFile;

                request.Credentials = new NetworkCredential(username, password);

                request.UsePassive = true;

                request.UseBinary = true;

                request.KeepAlive = false;



                using (FileStream fileStream = File.OpenRead(filePath))

                using (Stream requestStream = await request.GetRequestStreamAsync())

                {

                    await fileStream.CopyToAsync(requestStream);

                }



                using FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync();

                return (true, $"File uploaded successfully. Status: {response.StatusDescription}");

            }

            catch (Exception ex)

            {

                return (false, $"Error uploading file to FTP: {ex.Message}");

            }

        }

        private void WriteToFile(string Message)

        {

            try

            {

                string path = @"C:\inventory";

                if (!Directory.Exists(path))

                {

                    Directory.CreateDirectory(path);

                }



                string filepath = @$"{path}\inventory.csv";

                if (!File.Exists(filepath))

                {

                    using StreamWriter streamWriter = File.CreateText(filepath);

                    streamWriter.WriteLine(Message);

                }

                else

                {

                    using StreamWriter streamWriter = File.AppendText(filepath);

                    streamWriter.WriteLine(Message);

                }

            }

            catch (Exception ex)

            {

                throw ex;

            }

        }

        private static string PrettyXml(string xml)

        {

            StringBuilder stringBuilder = new();

            XmlWriterSettings settings = new();



            XElement element = XElement.Parse(xml);



            settings.OmitXmlDeclaration = true;

            settings.Indent = true;

            settings.NewLineOnAttributes = true;

            settings.Encoding = Encoding.UTF8;



            using (XmlWriter xmlWriter = XmlWriter.Create(stringBuilder, settings))

            {

                element.Save(xmlWriter);

            }

            return stringBuilder.ToString();

        }

        private void PrintErrorText(int resultValue, string function)

        {

            try

            {

                int result = imsapi.imsapiGetErrorText(sessionContext, resultValue, out string errorText);



                if (result != RES_OK)

                {

                    errorText = "Unable to get the error text.";

                }



                Log.Error("Result value : <{result}> from {function}", resultValue, function);

                Log.Error("Error text : <{error}> from {function}", errorText, function);

            }

            catch (System.Exception ex)

            {

                Log.Error(ex, "Exception : {ex} in PrintErrorText", ex.Message);

            }

        }

        #endregion

    }

}