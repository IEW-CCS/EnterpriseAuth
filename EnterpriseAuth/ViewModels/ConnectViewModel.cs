﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Newtonsoft.Json;
using EnterpriseAuth.Transactions;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Security.Policy;
using QRCoder;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WebSocket4Net;
using EnterpriseAuth.Managers;
using EnterpriseAuth.Security;

namespace EnterpriseAuth.ViewModels
{
    public class ConnectViewModel : BaseViewModel
    {
        //public event EventHandler ProfileUpdateEventHandler;

        public ServerProfile serverProfile = new ServerProfile();
        private string messageInfo = "";
        private BitmapImage stateImage = new BitmapImage();
        BlueToothManager btManager = new BlueToothManager();

        private string userPassword = "";
        private string passCode = "";
        private string serialNumber = "";
        private string credentialSign = "";
        private string hashPassword = "";

        public string ProfileName
        {
            get { return serverProfile.strProfileName; }
            set { serverProfile.strProfileName = value; }
        }
        public string ServerIP
        {
            get { return serverProfile.strServerURL; }
            set { serverProfile.strServerURL = value; }
        }
        public string ServerPort
        {
            get { return serverProfile.strServerPort; }
            set { serverProfile.strServerPort = value; }
        }
        public string UserName
        {
            get { return serverProfile.strUserName; }
            set { serverProfile.strUserName = value; }
        }

        public string UserPassword
        {
            //get { return serverProfile.strPassword; }
            set { this.userPassword = value; }
        }
        public string MessageInfo
        {
            get { return this.messageInfo; }
            set
            {
                this.messageInfo = value;
                OnPropertyChanged("MessageInfo");
            }
        }

        public BitmapImage StateImage
        {
            get { return this.stateImage; }
            set
            {
                this.stateImage = value;
                OnPropertyChanged("StateImage");
            }
        }

        public ConnectViewModel(ServerProfile sProfile)
        {
            this.serverProfile = sProfile;
        }

        public async void StartAuthentication()
        {
            this.DisplayConnectingStatus();

            VCONREQ vconRequest = new VCONREQ();
            string ServerURL = "http://" + serverProfile.strServerURL + ":" + serverProfile.strServerPort + "/" +serverProfile.strAuthenticationURL;
            vconRequest.DeviceCode = "00-1C-42-5E-F1-72";

            string registerRequestJson = JsonConvert.SerializeObject(vconRequest);
            AuthBaseDES BaseDES = new AuthBaseDES();

            string registerRequestDES = BaseDES.EncryptDES(registerRequestJson);

            HttpTrx HttpSend = new HttpTrx();
            HttpSend.username = serverProfile.strUserName;
            HttpSend.devicetype = EnumList.DeviceType.CONSOLE.ToString();
            HttpSend.procstep = EnumList.ProcessStep.VCON_REQ.ToString();
            HttpSend.returncode = 0;
            HttpSend.returnmsg = string.Empty;
            HttpSend.datacontent = registerRequestDES;
            HttpSend.ecs = string.Empty;
            HttpSend.ecssign = string.Empty;

            var modifiedAssetJSON = JsonConvert.SerializeObject(HttpSend);
            StringContent requestContent = null;
            requestContent = new StringContent(modifiedAssetJSON, Encoding.UTF8, "application/json");

            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(ServerURL);

                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("authorization", "Bearer " + this.serverProfile.strTokenID);

                try
                {
                    //this.serverProfile.strProfileProcessStep = ProcessStep
                    HttpResponseMessage response = await httpClient.PostAsync(ServerURL, requestContent);
                    //response.EnsureSuccessStatusCode();
                    if (response.IsSuccessStatusCode)
                    {
                        HttpContent content = response.Content;
                        //string jsonContent = content.ReadAsStringAsync().Result.Replace("\\", "").Trim(new char[1] { '"' }); ;
                        string jsonContent = await content.ReadAsStringAsync();

                        VCONPLY vconply = null;
                        string returnMsg = string.Empty;

                        if (Handle_VCONPLY(jsonContent, out vconply, out returnMsg) == false)
                        {
                            Application.Current.Dispatcher.Invoke(() => {
                                this.MessageInfo = "Handle VCONPLY  Error: " + returnMsg;
                            }, DispatcherPriority.Background);
                        }
                        else
                        {
                            //Connection reply successful
                            this.passCode = vconply.PassCode;
                            this.VerifyUser();
                        }
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(() => {
                            this.MessageInfo = "HTTP Response Error: " + response.StatusCode;
                        }, DispatcherPriority.Background);
                    }
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.Invoke(() => {
                        this.MessageInfo = "HTTP Post Exception: " + ex.Message;
                    }, DispatcherPriority.Background);
                }

            }
        }

        public async void VerifyUser()
        {
            APVRYREQ verifyRequest = new APVRYREQ();
            string ServerURL = "http://" + serverProfile.strServerURL + ":" + serverProfile.strServerPort + "/" + serverProfile.strAuthenticationURL;
            verifyRequest.PassWord = this.userPassword;
            verifyRequest.PassCode = this.passCode;

            string verifyRequestJson = JsonConvert.SerializeObject(verifyRequest);
            AuthBaseDES BaseDES = new AuthBaseDES();

            string verifyRequestDES = BaseDES.EncryptDES(verifyRequestJson);

            HttpTrx HttpSend = new HttpTrx();
            HttpSend.username = serverProfile.strUserName;
            HttpSend.devicetype = EnumList.DeviceType.CONSOLE.ToString();
            HttpSend.procstep = EnumList.ProcessStep.VCON_REQ.ToString();
            HttpSend.returncode = 0;
            HttpSend.returnmsg = string.Empty;
            HttpSend.datacontent = verifyRequestDES;
            HttpSend.ecs = string.Empty;
            HttpSend.ecssign = string.Empty;

            var modifiedAssetJSON = JsonConvert.SerializeObject(HttpSend);
            StringContent requestContent = null;
            requestContent = new StringContent(modifiedAssetJSON, Encoding.UTF8, "application/json");

            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(ServerURL);

                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("authorization", "Bearer " + this.serverProfile.strTokenID);

                try
                {
                    //this.serverProfile.strProfileProcessStep = ProcessStep
                    HttpResponseMessage response = await httpClient.PostAsync(ServerURL, requestContent);
                    //response.EnsureSuccessStatusCode();
                    if (response.IsSuccessStatusCode)
                    {
                        HttpContent content = response.Content;
                        //string jsonContent = content.ReadAsStringAsync().Result.Replace("\\", "").Trim(new char[1] { '"' }); ;
                        string jsonContent = await content.ReadAsStringAsync();

                        APVRYPLY apvryply = null;
                        string returnMsg = string.Empty;

                        if (Handle_APVRYPLY(jsonContent, out apvryply, out returnMsg) == false)
                        {
                            Application.Current.Dispatcher.Invoke(() => {
                                this.MessageInfo = "Handle APVRYPLY  Error: " + returnMsg;
                            }, DispatcherPriority.Background);
                        }
                        else
                        {
                            //Connection reply successful
                            this.serialNumber = apvryply.SerialNumber;

                            //Start to connect BLE device and process Authentication
                            StartBLEAuthentication();
                        }
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(() => {
                            this.MessageInfo = "HTTP Response Error: " + response.StatusCode;
                        }, DispatcherPriority.Background);
                    }
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.Invoke(() => {
                        this.MessageInfo = "HTTP Post Exception: " + ex.Message;
                    }, DispatcherPriority.Background);
                }

            }
        }

        public async void HashPasswordRequest()
        {
            AHPWREQ pwdRequest = new AHPWREQ();
            string ServerURL = "http://" + serverProfile.strServerURL + ":" + serverProfile.strServerPort + "/" + serverProfile.strAuthenticationURL;
            pwdRequest.BiometricsResult = "OK";
            pwdRequest.SerialNumber = this.serialNumber;
            pwdRequest.CredentialSign = this.credentialSign;

            string pwdRequestJson = JsonConvert.SerializeObject(pwdRequest);
            AuthBaseDES BaseDES = new AuthBaseDES();

            string pwdRequestDES = BaseDES.EncryptDES(pwdRequestJson);

            HttpTrx HttpSend = new HttpTrx();
            HttpSend.username = serverProfile.strUserName;
            HttpSend.devicetype = EnumList.DeviceType.CONSOLE.ToString();
            HttpSend.procstep = EnumList.ProcessStep.AHPW_REQ.ToString();
            HttpSend.returncode = 0;
            HttpSend.returnmsg = string.Empty;
            HttpSend.datacontent = pwdRequestDES;
            HttpSend.ecs = string.Empty;
            HttpSend.ecssign = string.Empty;

            var modifiedAssetJSON = JsonConvert.SerializeObject(HttpSend);
            StringContent requestContent = null;
            requestContent = new StringContent(modifiedAssetJSON, Encoding.UTF8, "application/json");

            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(ServerURL);

                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("authorization", "Bearer " + this.serverProfile.strTokenID);

                try
                {
                    //this.serverProfile.strProfileProcessStep = ProcessStep
                    HttpResponseMessage response = await httpClient.PostAsync(ServerURL, requestContent);
                    //response.EnsureSuccessStatusCode();
                    if (response.IsSuccessStatusCode)
                    {
                        HttpContent content = response.Content;
                        //string jsonContent = content.ReadAsStringAsync().Result.Replace("\\", "").Trim(new char[1] { '"' }); ;
                        string jsonContent = await content.ReadAsStringAsync();

                        AHPWPLY ahpwply = null;
                        string returnMsg = string.Empty;

                        if (Handle_AHPWPLY(jsonContent, out ahpwply, out returnMsg) == false)
                        {
                            Application.Current.Dispatcher.Invoke(() => {
                                this.MessageInfo = "Handle AHPWPLY  Error: " + returnMsg;
                            }, DispatcherPriority.Background);
                        }
                        else
                        {
                            //Connection reply successful
                            this.hashPassword = ahpwply.PasswordData;

                            //Connect to VPN or Citrix
                            ConnectRemoteServer();
                        }
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(() => {
                            this.MessageInfo = "HTTP Response Error: " + response.StatusCode;
                        }, DispatcherPriority.Background);
                    }
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.Invoke(() => {
                        this.MessageInfo = "HTTP Post Exception: " + ex.Message;
                    }, DispatcherPriority.Background);
                }

            }
        }

        public void ConnectRemoteServer()
        {

        }

        private bool Handle_VCONPLY(string DataContent, out VCONPLY vconply, out string ReturnMsg)
        {
            bool ReturnStatus = true;
            HttpTrx httptrx = DeserializeObj._HttpTrx(DataContent);
            if (httptrx == null)
            {
                ReturnStatus = false;
                ReturnMsg = "Deserialize Http Trx Error";
                vconply = null;
                return ReturnStatus;
            }
            else
            {
                if (httptrx.returncode != 0)
                {
                    ReturnStatus = false;
                    ReturnMsg = "Http Return Error, Message = " + httptrx.returnmsg;
                    vconply = null;
                    return ReturnStatus;
                }
                else
                {
                    string VCONPLY_DecryptContent = string.Empty;
                    string VCONPLY_DecryptReturnMsg = string.Empty;
                    int VCONPLY_DecryptReturnCode = 0;
                    VCONPLY_DecryptReturnCode = MainWindow.ObjectSecutiry.GetRSASecurity(serverProfile.strUserName, serverProfile.strProfileName).DecryptByPrivateKey(httptrx.ecs, out VCONPLY_DecryptContent, out VCONPLY_DecryptReturnMsg);
                    if (VCONPLY_DecryptReturnCode != 0)
                    {
                        ReturnStatus = false;
                        ReturnMsg = "Decrypt By PrivateKey Error";
                        vconply = null;
                        return ReturnStatus;
                    }
                    else
                    {
                        ECS HESC = DeserializeObj._ECS(VCONPLY_DecryptContent);
                        if (HESC == null)
                        {
                            ReturnStatus = false;
                            ReturnMsg = "Deserialize ECS Object Error";
                            vconply = null;
                            return ReturnStatus;

                        }
                        else
                        {
                            string DecrypStr = this.CheckDESData(HESC.Key, HESC.IV, httptrx.datacontent);
                            if (DecrypStr == string.Empty)
                            {
                                ReturnStatus = false;
                                ReturnMsg = "Decrypt by DES Object Error";
                                vconply = null;
                                return ReturnStatus;
                            }
                            else
                            {
                                vconply = DeserializeObj._VCONPLY(DecrypStr);
                                if (vconply == null)
                                {
                                    ReturnStatus = false;
                                    ReturnMsg = "Deserialize APREGPLY  Object Error";
                                    vconply = null;
                                    return ReturnStatus;
                                }
                                else
                                {
                                    ReturnStatus = true;
                                    ReturnMsg = string.Empty;
                                    return ReturnStatus;

                                }
                            }
                        }
                    }
                }
            }
        }

        private bool Handle_APVRYPLY(string DataContent, out APVRYPLY apvryply, out string ReturnMsg)
        {
            bool ReturnStatus = true;
            HttpTrx httptrx = DeserializeObj._HttpTrx(DataContent);
            if (httptrx == null)
            {
                ReturnStatus = false;
                ReturnMsg = "Deserialize Http Trx Error";
                apvryply = null;
                return ReturnStatus;
            }
            else
            {
                if (httptrx.returncode != 0)
                {
                    ReturnStatus = false;
                    ReturnMsg = "Http Return Error, Message = " + httptrx.returnmsg;
                    apvryply = null;
                    return ReturnStatus;
                }
                else
                {
                    string VCONPLY_DecryptContent = string.Empty;
                    string VCONPLY_DecryptReturnMsg = string.Empty;
                    int VCONPLY_DecryptReturnCode = 0;
                    VCONPLY_DecryptReturnCode = MainWindow.ObjectSecutiry.GetRSASecurity(serverProfile.strUserName, serverProfile.strProfileName).DecryptByPrivateKey(httptrx.ecs, out VCONPLY_DecryptContent, out VCONPLY_DecryptReturnMsg);
                    if (VCONPLY_DecryptReturnCode != 0)
                    {
                        ReturnStatus = false;
                        ReturnMsg = "Decrypt By PrivateKey Error";
                        apvryply = null;
                        return ReturnStatus;
                    }
                    else
                    {
                        ECS HESC = DeserializeObj._ECS(VCONPLY_DecryptContent);
                        if (HESC == null)
                        {
                            ReturnStatus = false;
                            ReturnMsg = "Deserialize ECS Object Error";
                            apvryply = null;
                            return ReturnStatus;

                        }
                        else
                        {
                            string DecrypStr = this.CheckDESData(HESC.Key, HESC.IV, httptrx.datacontent);
                            if (DecrypStr == string.Empty)
                            {
                                ReturnStatus = false;
                                ReturnMsg = "Decrypt by DES Object Error";
                                apvryply = null;
                                return ReturnStatus;
                            }
                            else
                            {
                                apvryply = DeserializeObj._APVRYPLY(DecrypStr);
                                if (apvryply == null)
                                {
                                    ReturnStatus = false;
                                    ReturnMsg = "Deserialize APREGPLY  Object Error";
                                    apvryply = null;
                                    return ReturnStatus;
                                }
                                else
                                {
                                    ReturnStatus = true;
                                    ReturnMsg = string.Empty;
                                    return ReturnStatus;

                                }
                            }
                        }
                    }
                }
            }
        }

        private bool Handle_AHPWPLY(string DataContent, out AHPWPLY ahpwply, out string ReturnMsg)
        {
            bool ReturnStatus = true;
            HttpTrx httptrx = DeserializeObj._HttpTrx(DataContent);
            if (httptrx == null)
            {
                ReturnStatus = false;
                ReturnMsg = "Deserialize Http Trx Error";
                ahpwply = null;
                return ReturnStatus;
            }
            else
            {
                if (httptrx.returncode != 0)
                {
                    ReturnStatus = false;
                    ReturnMsg = "Http Return Error, Message = " + httptrx.returnmsg;
                    ahpwply = null;
                    return ReturnStatus;
                }
                else
                {
                    string AHPWPLY_DecryptContent = string.Empty;
                    string AHPWPLY_DecryptReturnMsg = string.Empty;
                    int AHPWPLY_DecryptReturnCode = 0;
                    AHPWPLY_DecryptReturnCode = MainWindow.ObjectSecutiry.GetRSASecurity(serverProfile.strUserName, serverProfile.strProfileName).DecryptByPrivateKey(httptrx.ecs, out AHPWPLY_DecryptContent, out AHPWPLY_DecryptReturnMsg);
                    if (AHPWPLY_DecryptReturnCode != 0)
                    {
                        ReturnStatus = false;
                        ReturnMsg = "Decrypt By PrivateKey Error";
                        ahpwply = null;
                        return ReturnStatus;
                    }
                    else
                    {
                        ECS HESC = DeserializeObj._ECS(AHPWPLY_DecryptContent);
                        if (HESC == null)
                        {
                            ReturnStatus = false;
                            ReturnMsg = "Deserialize ECS Object Error";
                            ahpwply = null;
                            return ReturnStatus;

                        }
                        else
                        {
                            string DecrypStr = this.CheckDESData(HESC.Key, HESC.IV, httptrx.datacontent);
                            if (DecrypStr == string.Empty)
                            {
                                ReturnStatus = false;
                                ReturnMsg = "Decrypt by DES Object Error";
                                ahpwply = null;
                                return ReturnStatus;
                            }
                            else
                            {
                                ahpwply = DeserializeObj._AHPWPLY(DecrypStr);
                                if (ahpwply == null)
                                {
                                    ReturnStatus = false;
                                    ReturnMsg = "Deserialize APREGPLY  Object Error";
                                    ahpwply = null;
                                    return ReturnStatus;
                                }
                                else
                                {
                                    ReturnStatus = true;
                                    ReturnMsg = string.Empty;
                                    return ReturnStatus;

                                }
                            }
                        }
                    }
                }
            }
        }

        private string CheckDESData(string key, string iv, string DataContent)
        {
            AuthDES objDes = new AuthDES(key, iv);
            string DES_DecryptStr = string.Empty;
            try
            {
                DES_DecryptStr = objDes.DecryptDES(DataContent);
            }
            catch
            {
                DES_DecryptStr = string.Empty;
            }
            return DES_DecryptStr;
        }

        public void  StartBLEAuthentication()
        {
            this.DisplayBlueToothStatus();
            this.btManager.ScanAndConnect();
            this.btManager.BLEMessageEvent += BLEMessage_Received;
            this.btManager.BiometricsVerifyEvent += BiometricsVerify_Received;
            this.btManager.CredentialContentEvent += CredentialContent_Received;
        }

        public void DisplayConnectingStatus()
        {
            Application.Current.Dispatcher.Invoke(() => {
                this.StateImage = new BitmapImage(new Uri("/images/connect.jpeg", UriKind.Relative));
                this.MessageInfo = "Connecting to Server...";
            }, DispatcherPriority.Background);
        }

        public void DisplayBlueToothStatus()
        {
            Application.Current.Dispatcher.Invoke(() => {
                this.StateImage = new BitmapImage(new Uri("/images/bluetooth.jpeg", UriKind.Relative));
                this.MessageInfo = "Connecting to BlueTooth Device...";
            }, DispatcherPriority.Background);
        }

        private void BLEMessage_Received(object sender, EventArgs e)
        {
            BLEMessageEventArgs pe = e as BLEMessageEventArgs;
            Console.WriteLine("BLEMessage_Received: " + pe.bleMessage);

            Application.Current.Dispatcher.Invoke(() => {
                this.MessageInfo = pe.bleMessage;
            }, DispatcherPriority.Background);
        }

        private void BiometricsVerify_Received(object sender, EventArgs e)
        {
            BLEMessageEventArgs pe = e as BLEMessageEventArgs;
            Console.WriteLine("Biometrics Verify " + pe.bleMessage);

            //Application.Current.Dispatcher.Invoke(() => {
            //    this.MessageInfo = "Biometrics Verify " + pe.bleMessage;
            //}, DispatcherPriority.Background);
        }

        private void CredentialContent_Received(object sender, EventArgs e)
        {
            BLEMessageEventArgs pe = e as BLEMessageEventArgs;
            Console.WriteLine("Received Credential Content: " + pe.bleMessage);

            this.credentialSign = pe.bleMessage;

            Application.Current.Dispatcher.Invoke(() => {
                this.MessageInfo = "Receive and Compare Credential Content OK";
            }, DispatcherPriority.Background);

            this.HashPasswordRequest();
        }

    }
}
