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
using System.Text;
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
#if DEBUG
            int result = 0;
#else
            int result = imsapi.imsapiShutdown();
#endif

            return result;
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

            Log.Information($"Trying to connect to iTAC server with following credentials : user name {username} , pass word {password} ");

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

        public async Task<(bool, string state, string error)> CheckSerialNumberStateAsync(string station, string snr)
        {
            return await Task.Run(() =>
            {
                string[] resultKeys = { "SERIAL_NUMBER_STATE", "ERROR_CODE" };

                int result = imsapi.trCheckSerialNumberState(sessionContext, station, 2, 0, snr, "-1", resultKeys, out string[] outResults);

                string state = outResults[0];
                string error = outResults[1];

                return (result == RES_OK, state, error);
            });
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
            var fictiveNumberOfBoards = new Random().Next(10, 50);
            Random random = new Random();
            for (int i = 0; i < fictiveNumberOfBoards; i++)
            {
                panelRslt.Add(new PanelPositions 
                    {
                        PositionNumber = i + 1,
                        SerialNumber = "99999_99999_9999", 
                        Status = random.Next(0, 7),
                    });
            }
            return panelRslt;
#endif
            int processLayer       = 2;
            int checkMultiBoard    = 1;
            string serialNumberPos = "-1";
            string[] SnStateResultKeys  = new[] { "SERIAL_NUMBER_POS", "SERIAL_NUMBER", "SERIAL_NUMBER_STATE" };
            string[] SnStateResultValues;

            await Task.Run(() =>
            {
                int result = imsapi.trCheckSerialNumberState(sessionContext, station, processLayer, checkMultiBoard, snr, serialNumberPos,
                                                                         SnStateResultKeys, out SnStateResultValues);

                if (!string.IsNullOrEmpty(SnStateResultValues[0]) && SnStateResultValues.Length > 0)
                {
                    for (int i = 0; i < SnStateResultValues.Length; i += SnStateResultKeys.Length)
                    {
                        panelRslt.Add(new PanelPositions
                        {
                            PositionNumber = int.Parse(SnStateResultValues[i]),
                            SerialNumber = SnStateResultValues[i + 1],
                            Status = int.Parse(SnStateResultValues[i + 2]),
                        });
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
            string[] attributeUploadKeys = new string[3] { "ATTRIBUTE_CODE", "ATTRIBUTE_VALUE", "ERROR_CODE" };
            string[] attributeUploadValues = new string[3] { "razeUser", $"{username} on : {DateTime.Now}", "0" };

            return await Task.Run(() =>
            {
                int result = imsapi.attribAppendAttributeValues(sessionContext, station, 0, srn, "-1", -1L, 0, attributeUploadKeys, attributeUploadValues, out string[] results);

                // Check the result and return the appropriate value
                return result == RES_OK ? 0 : int.Parse(results[1]);
            });
        }


        /*public void StartMES(string WorkCenter, string productNumber, string eventDateTime, string serialNumber, string Qte, string CycleTime, AppSession _session)
        {
            string motherForm = $"<?xml version=\"1.0\"?><FSA_INT_FlatFileManager xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:noNamespaceSchemaLocation=\"C:/Inetpub/wwwroot/SchemaRepository/XMLSchemas/FlexNet/FSA_INT_FlatFileManager.xsd\" Version=\"1.0\"><FIInvocationSynchronousEvent NodeType=\"FIInvocation\"><StandardOperation><OperationResolutionMethod>ByOperationCode</OperationResolutionMethod><OperationCode>SVC_MES_MI_ProductionDeclaration</OperationCode></StandardOperation><Parameters><Inputs><InputName>WorkCenter</InputName><InputValue>{WorkCenter}</InputValue></Inputs><Inputs><InputName>ProductNo</InputName><InputValue>{productNumber}</InputValue></Inputs><Inputs><InputName>EventDateTime</InputName><InputValue>{eventDateTime}</InputValue></Inputs><Inputs><InputName>SerialNo</InputName><InputValue>{serialNumber}</InputValue></Inputs><Inputs><InputName>Quantity</InputName><InputValue>{Qte}</InputValue></Inputs><Inputs><InputName>CycleTime</InputName><InputValue>{CycleTime}</InputValue></Inputs></Parameters></FIInvocationSynchronousEvent></FSA_INT_FlatFileManager>";
            string beutyXML = PrettyXml(motherForm);

            if (_session.UploadType == Infrastructure.UploadMethodEnum.API)
            {
                SendApi(beutyXML, _session.BarFlowServer);
            }
            else
            {
                UploadFile(beutyXML, _session.FtpUsername, _session.FtpPassword);
            }

            if (_session.IsItacInterlock)
            {
                LockSerial(_session.Station, serialNumber);
            }
        }*/

        public async Task<(string status, string reason)> StartMESAsync(string WorkCenter, string productNumber, string eventDateTime, string serialNumber, string Qte, string CycleTime, AppSession _session)
        {
            string motherForm = $"<?xml version=\"1.0\"?><FSA_INT_FlatFileManager xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:noNamespaceSchemaLocation=\"C:/Inetpub/wwwroot/SchemaRepository/XMLSchemas/FlexNet/FSA_INT_FlatFileManager.xsd\" Version=\"1.0\"><FIInvocationSynchronousEvent NodeType=\"FIInvocation\"><StandardOperation><OperationResolutionMethod>ByOperationCode</OperationResolutionMethod><OperationCode>SVC_MES_MI_ProductionDeclaration</OperationCode></StandardOperation><Parameters><Inputs><InputName>WorkCenter</InputName><InputValue>{WorkCenter}</InputValue></Inputs><Inputs><InputName>ProductNo</InputName><InputValue>{productNumber}</InputValue></Inputs><Inputs><InputName>EventDateTime</InputName><InputValue>{eventDateTime}</InputValue></Inputs><Inputs><InputName>SerialNo</InputName><InputValue>{serialNumber}</InputValue></Inputs><Inputs><InputName>Quantity</InputName><InputValue>{Qte}</InputValue></Inputs><Inputs><InputName>CycleTime</InputName><InputValue>{CycleTime}</InputValue></Inputs></Parameters></FIInvocationSynchronousEvent></FSA_INT_FlatFileManager>";
            string prettyXML = PrettyXml(motherForm);

            try
            {
                if (_session.UploadType == Infrastructure.UploadMethodEnum.API)
                {
                    var apiResponse = await SendApiAsync(prettyXML, _session.BarFlowServer);

                    if (apiResponse.status == "fail")
                    {
                        Log.Error("API call failed : " + apiResponse.reason);
                        return ("fail", apiResponse.reason);
                    }
                    Log.Error("API call Ok : " + apiResponse.status);
                }
                else
                {
                    UploadFileAsync(prettyXML, _session.FtpUsername, _session.FtpPassword);
                }

                if (_session.IsItacInterlock)
                {
                    await Task.Run(()=> LockSerial(_session.Station, serialNumber));
                }

                return ("ok", null);
            }
            catch (Exception ex)
            {
                return ("fail", $"Error in StartMES: {ex.Message}");
            }
        }


        public int VerifyMESAttr(string station,string serialNumber)
        {
            string[] attributeCodeArray  = { "MES_Booking" };
            string[] attributeResultKeys = { "ATTRIBUTE_CODE", "ATTRIBUTE_VALUE", "ERROR_CODE" };

            return imsapi.attribGetAttributeValues(sessionContext, station, 0, serialNumber,null, attributeCodeArray, 0, attributeResultKeys, out string[] results) != RES_OK
                ? -1
                : int.Parse(results[1]);
        }

        public async Task<int> VerifyMESAttrAsync(string station, string serialNumber)
        {
            string[] attributeCodeArray = { "MES_Booking" };
            string[] attributeResultKeys = { "ATTRIBUTE_CODE", "ATTRIBUTE_VALUE", "ERROR_CODE" };

            return await Task.Run(() =>
            {
                int result = imsapi.attribGetAttributeValues(sessionContext, station, 0, serialNumber, null, attributeCodeArray, 0, attributeResultKeys, out string[] results);

                return result != RES_OK ? -1 : int.Parse(results[1]);
            });
        }


        public int AppendMESAttr(string station, string serialNumber)
        {
            string[] attributeResultKeys = new string[3] { "ATTRIBUTE_CODE", "ATTRIBUTE_VALUE", "ERROR_CODE" };
            string[] attributeUploadValues = new string[3] { "MES_Booking", "1", "0" };

            //attribAppendAttributeValues((SessionContext, stationNumber, 0, SN, null, -1, 1, attributeResultKeys{"ATTRIBUTE_CODE","ATTRIBUTE_VALUE","ERROR_CODE"}, attributeUploadValues{"MES_Booking","1",0})
            return imsapi.attribAppendAttributeValues(sessionContext, station, 0, serialNumber, null, -1L, 1, attributeResultKeys, attributeUploadValues, out string[] results) == RES_OK
                ? 0
                : int.Parse(results[1]);
        }

        public async Task<int> AppendMESAttrAsync(string station, string serialNumber)
        {
            string[] attributeResultKeys = new string[3] { "ATTRIBUTE_CODE", "ATTRIBUTE_VALUE", "ERROR_CODE" };
            string[] attributeUploadValues = new string[3] { "MES_Booking", "1", "0" };

            return await Task.Run(() =>
            {
                int result = imsapi.attribAppendAttributeValues(sessionContext, station, 0, serialNumber, null, -1L, 1, attributeResultKeys, attributeUploadValues, out string[] results);

                // Check the result and return the appropriate value
                return result == RES_OK ? 0 : int.Parse(results[1]);
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

        public async void UploadFileAsync(string beutyXML, string username, string password)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "xml");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string filePath = Path.Combine(path, $"{DateTime.Now:yyyyMMddHHmm}_fileToUpload.xml");

            using StreamWriter fsWrite = new(filePath);
            fsWrite.WriteLine(beutyXML);
            await UploadFileToFtpAsync("ftp://10.172.4.117/FLEXNET/FLATFILES/TODO/", filePath, username, password);
        }

        private static async Task<(string status, string reason)> SendApiAsync(string body, string barflow)
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
                        return ("ok", null);
                    }
                    else
                    {
                        return ("fail", $"Unexpected response code: {(int)httpResponse.StatusCode} {httpResponse.StatusDescription}");
                    }
                }
            }
            catch (WebException webEx)
            {
                if (webEx.Response is HttpWebResponse errorResponse)
                {
                    return ("fail", $"Web exception: {(int)errorResponse.StatusCode} {errorResponse.StatusDescription}");
                }
                return ("fail", $"Web exception: {webEx.Message}");
            }
            catch (Exception ex)
            {
                return ("fail", $"General exception: {ex.Message}");
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

        public async Task UploadFileToFtpAsync(string url, string filePath, string username, string password)
        {
            try
            {
                string fileName = Path.GetFileName(filePath);
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url + fileName);
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.Credentials = new NetworkCredential(username, password);
                request.UsePassive = true;
                request.UseBinary = true;
                request.KeepAlive = false;

                using (FileStream fileStream = File.OpenRead(filePath))
                {
                    using (Stream requestStream = await request.GetRequestStreamAsync())
                    {
                        await fileStream.CopyToAsync(requestStream);
                        requestStream.Close(); 
                    }
                }

                using FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error uploading file to FTP", ex);
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
