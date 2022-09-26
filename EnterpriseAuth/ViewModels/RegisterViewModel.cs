using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Newtonsoft.Json;
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
using EnterpriseAuth.Transactions;
using EnterpriseAuth.Managers;
using EnterpriseAuth.Security;
using System.Net.NetworkInformation;

namespace EnterpriseAuth.ViewModels
{

    public class RegisterViewModel : BaseViewModel
    {
        //public event EventHandler ProfileUpdateEventHandler;

        private ServerProfile serverProfile = new ServerProfile();
        private string messageInfo = "";
        private BitmapImage stateImage = new BitmapImage();
        private string userPassword = "";

        public string clientID = "";
        public string serviceUUID = "";
        public WebSocket webSocket4Net = null;

        private string credentialContent = "";
        BlueToothManager btManager = new BlueToothManager();

        /*
        public ServerProfile ProfileObject
        {
            get { return serverProfile; }
            set { serverProfile = value; }
        }
        */

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
            set { this.messageInfo = value;
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
        public RegisterViewModel(ServerProfile sProfile)
        {
            this.serverProfile = sProfile;
        }

        public void TestRegisterComplete()
        {
            this.MessageInfo = "Test Register Complete and Change to ConnectView";
            this.serverProfile.strProfileState = GlobalVaraible.PROFILE_STATE_REGISTER;
            OnProfileUpdated();
        }

        public async void RegisterServer(/*RegisterReplyDelegate del*/)
        {
            this.DisplayConnectingStatus();



            ARREGREQ registerRequest = new ARREGREQ();
            string ServerURL = "http://" + serverProfile.strServerURL + ":" + serverProfile.strServerPort + "/" + GlobalVaraible.REGISTER_LOGIN;
            //registerRequest.PassWord = serverProfile.strPassword;
            registerRequest.PassWord = this.userPassword;
            registerRequest.ClientRSAPublicKey = MainWindow.ObjectSecutiry.GetRSASecurity(serverProfile.strUserName, serverProfile.strProfileName).PublicKey;
            registerRequest.DeviceMacAddress = GetMacAddress();
            registerRequest.APPGuid = System.Reflection.Assembly.GetEntryAssembly().GetName().Name.ToString();
            registerRequest.APPVersion = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
            registerRequest.OSEnv = "Windows";
    

            string registerRequestJson = JsonConvert.SerializeObject(registerRequest);
            AuthBaseDES BaseDES = new AuthBaseDES();

            string registerRequestDES = BaseDES.EncryptDES(registerRequestJson);

            HttpTrx HttpSend = new HttpTrx();
            HttpSend.username = this.UserName;
            HttpSend.devicetype = EnumList.DeviceType.CONSOLE.ToString();
            HttpSend.procstep = EnumList.ProcessStep.ARREGREQ.ToString();
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
                // httpClient.DefaultRequestHeaders.Add("authorization", "token {api token}");

                try
                {
                    //this.serverProfile.strProfileProcessStep = ProcessStep
                    HttpResponseMessage response =await httpClient.PostAsync(ServerURL, requestContent);
                    //response.EnsureSuccessStatusCode();
                    if (response.IsSuccessStatusCode)
                    {
                        HttpContent content = response.Content;
                        //string jsonContent = content.ReadAsStringAsync().Result.Replace("\\", "").Trim(new char[1] { '"' }); ;
                        string jsonContent = await content.ReadAsStringAsync();

                        ARREGPLY apregply = null;
                        string returnMsg = string.Empty;

                        if (Handle_APREGPLY(jsonContent, out apregply, out returnMsg) == false)
                        {
                            Application.Current.Dispatcher.Invoke(() => {
                                this.MessageInfo = "Handle APREGPLY  Error: " + returnMsg;
                            }, DispatcherPriority.Background);
                        }
                        else
                        {
                             this.clientID = apregply.ServerName;
                            //     this.serverProfile.strMyPublicKey = apregply.serverPublicKey;
                            //   this.serverProfile.strCredential = apregply.credentialHash;
                            serverProfile.strServerPublicKey = apregply.ServerRSAPublicKey;
                          //  this.OpenWebSocket();

                            this.serverProfile.strTokenID = apregply.HttpToken;
                            this.clientID = serverProfile.strUserName;
                            //this.serverProfile.strCredential = string.Empty;
                            this.serverProfile.strServerPublicKey = apregply.ServerRSAPublicKey;
                            this.serverProfile.strHttpServiceURL = apregply.HttpServiceURL;
                            this.serverProfile.strWSServiceURL = apregply.WSServiceURL;
        
                            MainWindow.ObjectSecutiry.GetRSASecurity(serverProfile.strUserName, serverProfile.strProfileName).ClientID = serverProfile.strUserName;
                            MainWindow.ObjectSecutiry.GetRSASecurity(serverProfile.strUserName, serverProfile.strProfileName).ClientPublicKey = apregply.ServerRSAPublicKey;
                            
                            this.OpenWebSocket(apregply.WSServiceURL, apregply.HttpToken);


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

        public async void RegisterFinish()
        {
            string ServerURL = "http://" + serverProfile.strServerURL + ":" + serverProfile.strServerPort + "/" + serverProfile.strHttpServiceURL;

            ARREGCMP compRequest = new ARREGCMP();
            compRequest.Result = "OK";

            string compRequestJson = JsonConvert.SerializeObject(compRequest);

            //------ 20220916 Not Base DES use AuthDES 加密方法
            AuthDES DES = new AuthDES();
            string compRequestDES = DES.EncryptDES(compRequestJson);

            ECS HECS = new ECS();
            HECS.Algo = "DES";
            HECS.Key = DES.GetKey();
            HECS.IV = DES.GetIV();

            string ECSEncryptRetMsg = string.Empty;
            string HECSJsonStr = JsonConvert.SerializeObject(HECS);

            string rtnMsg = string.Empty;
            string SignStr = string.Empty;
            string ECSEncryptStr = string.Empty;

            if (MainWindow.ObjectSecutiry.GetRSASecurity(serverProfile.strUserName, serverProfile.strProfileName).Encrypt_Sign(HECSJsonStr, out ECSEncryptStr, out SignStr, out rtnMsg) != 0)
            {

                // Show Error Message 

            }
            else
            {
                HttpTrx HttpSend = new HttpTrx();
                HttpSend.username = serverProfile.strUserName;
                HttpSend.devicetype = EnumList.DeviceType.CONSOLE.ToString();
                HttpSend.procstep = EnumList.ProcessStep.ARREGCMP.ToString();
                HttpSend.returncode = 0;
                HttpSend.returnmsg = string.Empty;
                HttpSend.datacontent = compRequestDES;
                HttpSend.ecs = ECSEncryptStr;
                HttpSend.ecssign = SignStr;

                var modifiedAssetJSON = JsonConvert.SerializeObject(HttpSend);
                StringContent requestContent = null;
                requestContent = new StringContent(modifiedAssetJSON, Encoding.UTF8, "application/json");

                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(ServerURL);

                    httpClient.DefaultRequestHeaders.Accept.Clear();
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    httpClient.DefaultRequestHeaders.Add("authorization", "bearer " + this.serverProfile.strTokenID);

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

                            ARREGFIN arregfin = null;
                            string returnMsg = string.Empty;

                            if (Handle_ARREGFIN(jsonContent, out arregfin, out returnMsg) == false)
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    this.MessageInfo = "Handle ARREGFIN  Error: " + returnMsg;
                                }, DispatcherPriority.Background);
                            }
                            else
                            {
                                //this.clientID = aregfin.ServerName;
                                //this.serverProfile.strMyPublicKey = apregply.serverPublicKey;
                                //this.serverProfile.strCredential = apregply.credentialHash;
                                serverProfile.strAuthenticationTokenID = arregfin.AuthenticationToken;
                                serverProfile.strAuthenticationURL = arregfin.AuthenticationURL;
                                // 20220926 不是第一次連線不需要改ObjectSecurity
                                //MainWindow.ObjectSecutiry.GetRSASecurity(serverProfile.strUserName, serverProfile.strProfileName).ClientID = serverProfile.strUserName;
                                //MainWindow.ObjectSecutiry.GetRSASecurity(serverProfile.strUserName, serverProfile.strProfileName).ClientPublicKey = serverProfile.strMyPublicKey;
                                this.serverProfile.strProfileState = GlobalVaraible.PROFILE_STATE_REGISTER;
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    this.MessageInfo = "Register Complete";
                                    OnProfileUpdated();
                                }, DispatcherPriority.Background);

                            }
                        }
                        else
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                this.MessageInfo = "HTTP Response Error: " + response.StatusCode;
                            }, DispatcherPriority.Background);
                        }
                    }
                    catch (Exception ex)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            this.MessageInfo = "HTTP Post Exception: " + ex.Message;
                        }, DispatcherPriority.Background);
                    }

                }
            }
        }

        public void WebSocketTest()
        {
            this.DisplayConnectingStatus();
            this.OpenWebSocket();
        }

        public void DisplayConnectingStatus()
        {
            Application.Current.Dispatcher.Invoke(() =>            {
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

        public void OpenWebSocket()
        {

            //this.webSocket4Net = new WebSocket("ws://localhost:23538/CLIENT_VERIFY");
            this.webSocket4Net = new WebSocket("ws://" + serverProfile.strServerURL + ":" + serverProfile.strServerPort + "/CLIENT_VERIFY");

            this.webSocket4Net.Error += WebSocket4Net_Error;

            this.webSocket4Net.Opened += WebSocket4Net_Opened;
            this.webSocket4Net.Closed += WebSocket4Net_Closed;
            this.webSocket4Net.MessageReceived += WebSocketNotifyMessage;
            this.webSocket4Net.Open();
        }

        public void OpenWebSocket(string WS_URL, string TokenID)
        {

            //this.webSocket4Net = new WebSocket("ws://localhost:23538/CLIENT_VERIFY");
            string WSURL = "ws://" + serverProfile.strServerURL + ":" + serverProfile.strServerPort + "/" + WS_URL + "?access_token= " + TokenID;
            this.webSocket4Net = new WebSocket(WSURL);
            this.webSocket4Net.Error += WebSocket4Net_Error;

            this.webSocket4Net.Opened += WebSocket4Net_Opened;
            this.webSocket4Net.Closed += WebSocket4Net_Closed;
            this.webSocket4Net.MessageReceived += WebSocketNotifyMessage;
            this.webSocket4Net.Open();
         }

            private void WebSocket4Net_Opened(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() => {
                this.MessageInfo = "Verifying Client...";
            }, DispatcherPriority.Background);

            //this.WebSocket4Net_SendVerifyMessage();
        }

        private void WebSocket4Net_Closed(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() => {
                this.MessageInfo = "WebSocket Connection Closed.";
            }, DispatcherPriority.Background);
        }

        private void WebSocket4Net_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            string str = e.Exception.Message;

            Application.Current.Dispatcher.Invoke(() => {
                this.MessageInfo = str;
            }, DispatcherPriority.Background);
        }

        public void WebSocket4Net_SendVerifyMessage()
        {
            ClientVerifyRequest clientRequest = new ClientVerifyRequest();
            clientRequest.clientID = "client_james";
            clientRequest.tokenID = "234923bju3ahsd3acsd3gg68vsdf9";
            string strRequestMessage = JsonConvert.SerializeObject(clientRequest);
            this.webSocket4Net.Send(strRequestMessage);
        }

        public void WebSocketNotifyMessage(object sender, MessageReceivedEventArgs e)
        {

            WSTrx wstrx = JsonConvert.DeserializeObject<WSTrx>(e.Message);
            if (wstrx.ProcStep == EnumList.ProcessStep.ARWSCPLY.ToString())
            {
                if (wstrx.ReturnCode == 0)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        this.MessageInfo = "WebSocket Client Verify OK. Server. Reply Msg = " + wstrx.ReturnMsg;
                    }, DispatcherPriority.Background);

                    this.GenerateQRCode();

                }
                else
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        this.MessageInfo = "WebSocket Client Verify Failed, Reply Msg = " + wstrx.ReturnMsg;
                    }, DispatcherPriority.Background);
                }
            }
            else if (wstrx.ProcStep == EnumList.ProcessStep.ARWSCANN.ToString())
            {
                if (wstrx.ReturnCode == 0)
                {
                    Console.WriteLine("Data Content: " + wstrx.DataContent);

                    //this.serviceUUID = notifyData.serviceUUID;
                    //Console.WriteLine($"Servier Reply Data: {e.Message}！");

                    /* 20220926 Chris - Receive Data */
                    if (Handle_ARWSCANN(wstrx.DataContent, out ARWSCANN arwscann, out string ReturnMsg))
                    {
                        this.DisplayBlueToothStatus();
                        this.btManager.ScanAndConnect();
                        this.btManager.BLEMessageEvent += BLEMessage_Received;
                        this.btManager.BiometricsVerifyEvent += BiometricsVerify_Received;
                        this.btManager.CredentialContentEvent += CredentialContent_Received;
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            this.MessageInfo = "Handle ARWSCANN Error  " ;
                        }, DispatcherPriority.Background);

                    }
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        this.MessageInfo = "WebSocket Notify Error, Reply Msg = " + wstrx.ReturnMsg;
                    }, DispatcherPriority.Background);
                }

            }

            /*
            ClientNotify notifyData = new ClientNotify();
            notifyData = JsonConvert.DeserializeObject<ClientNotify>(e.Message);
            if(notifyData.processStep == "VERIFY")
            {
                if(notifyData.result == "OK")
                {
                    Application.Current.Dispatcher.Invoke(() => {
                        this.MessageInfo = "WebSocket Client Verify OK.";
                    }, DispatcherPriority.Background);

                    this.GenerateQRCode();
                } else
                {
                    Application.Current.Dispatcher.Invoke(() => {
                        this.MessageInfo = "WebSocket Client Verify Failed";
                    }, DispatcherPriority.Background);
                }
            }
            else if(notifyData.processStep == "UUID")
            {
                this.serviceUUID = notifyData.serviceUUID;
                Console.WriteLine($"Servier Reply Data: {e.Message}！");
                
                this.DisplayBlueToothStatus();
                this.btManager.ScanAndConnect();
                this.btManager.BLEMessageEvent += BLEMessage_Received;
                this.btManager.BiometricsVerifyEvent += BiometricsVerify_Received;
                this.btManager.CredentialContentEvent += CredentialContent_Received;


            }
            else
            {
                Application.Current.Dispatcher.Invoke(() => {
                    this.MessageInfo = "WebSocket Notify Error";
                }, DispatcherPriority.Background);
            }*/
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

            Application.Current.Dispatcher.Invoke(() => {
                this.MessageInfo = "Receive and Compare Credential Content OK";
            }, DispatcherPriority.Background);

            RegisterFinish();
            //this.serverProfile.strProfileState = GlobalVaraible.PROFILE_STATE_REGISTER;
            //OnProfileUpdated();
        }

        public void GenerateQRCode()
        {
            QRCodeContent qrCodeContent = new QRCodeContent();
            qrCodeContent.strServerURL = "http://" + serverProfile.strServerURL + ":" + serverProfile.strServerPort + "/" + serverProfile.strHttpServiceURL;
            qrCodeContent.strTokenID = serverProfile.strTokenID;
            qrCodeContent.strUserName = serverProfile.strUserName;
            var strQRCodeText = JsonConvert.SerializeObject(qrCodeContent);

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(strQRCodeText, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);

            Application.Current.Dispatcher.Invoke(() => {
                this.StateImage = BitmapToImageSource(qrCodeImage);
                this.MessageInfo = "Use BioAuthentication App to Scan This QR Code";
            }, DispatcherPriority.Background);


        }
        
        BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        private bool Handle_APREGPLY(string DataContent, out ARREGPLY apregply, out string ReturnMsg)
        {
            bool ReturnStatus = true;
            HttpTrx httptrx = DeserializeObj._HttpTrx(DataContent);
            if (httptrx == null)
            {
                ReturnStatus = false;
                ReturnMsg = "Deserialize Http Trx Error";
                apregply = null;
                return ReturnStatus;
            }
            else
            {
                if (httptrx.returncode != 0)
                {
                    ReturnStatus = false;
                    ReturnMsg = "Http Return Error, Message = " + httptrx.returnmsg;
                    apregply = null;
                    return ReturnStatus;
                }
                else
                {
                    string APREGPLY_DecryptContent = string.Empty;
                    string APREGPLY_DecryptReturnMsg = string.Empty;
                    int APREGPLY_DecryptReturnCode = 0;
                    APREGPLY_DecryptReturnCode = MainWindow.ObjectSecutiry.GetRSASecurity(serverProfile.strUserName, serverProfile.strProfileName).DecryptByPrivateKey(httptrx.ecs, out APREGPLY_DecryptContent, out APREGPLY_DecryptReturnMsg);
                    if (APREGPLY_DecryptReturnCode != 0)
                    {
                        ReturnStatus = false;
                        ReturnMsg = "Decrypt By PrivateKey Error";
                        apregply = null;
                        return ReturnStatus;
                    }
                    else
                    {
                        ECS HESC = DeserializeObj._ECS(APREGPLY_DecryptContent);
                        if (HESC == null)
                        {
                            ReturnStatus = false;
                            ReturnMsg = "Deserialize ECS Object Error";
                            apregply = null;
                            return ReturnStatus;

                        }
                        else
                        {
                            string DecrypStr = this.DecryptDESData(HESC.Key, HESC.IV, httptrx.datacontent);
                            if (DecrypStr == string.Empty)
                            {
                                ReturnStatus = false;
                                ReturnMsg = "Decrypt by DES Object Error";
                                apregply = null;
                                return ReturnStatus;
                            }
                            else
                            {
                                apregply = DeserializeObj._APREGPLY(DecrypStr);
                                if (apregply == null)
                                {
                                    ReturnStatus = false;
                                    ReturnMsg = "Deserialize APREGPLY  Object Error";
                                    apregply = null;
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


        private bool Handle_ARWSCANN(string DataContent, out ARWSCANN arwscann, out string ReturnMsg)
        {

            bool ReturnStatus = false;
            arwscann = DeserializeObj._ARWSCANN(DataContent);
            if (arwscann == null)
            {
                ReturnStatus = false;
                ReturnMsg = "Deserialize ARWSCANN Object Error";
                arwscann = null;
                return ReturnStatus;
            }
            else
            {
                ReturnStatus = true;
                ReturnMsg = string.Empty;
                return ReturnStatus;
            }

        }

        private bool Handle_ARREGFIN(string DataContent, out ARREGFIN aregfin, out string ReturnMsg)
        {
            bool ReturnStatus = true;
            HttpTrx httptrx = DeserializeObj._HttpTrx(DataContent);
            if (httptrx == null)
            {
                ReturnStatus = false;
                ReturnMsg = "Deserialize Http Trx Error";
                aregfin = null;
                return ReturnStatus;
            }
            else
            {
                if (httptrx.returncode != 0)
                {
                    ReturnStatus = false;
                    ReturnMsg = "Http Return Error, Message = " + httptrx.returnmsg;
                    aregfin = null;
                    return ReturnStatus;
                }
                else
                {
                    string AREGFIN_DecryptContent = string.Empty;
                    string AREGFIN_DecryptReturnMsg = string.Empty;
                    int AREGFIN_DecryptReturnCode = 0;
                    AREGFIN_DecryptReturnCode = MainWindow.ObjectSecutiry.GetRSASecurity(serverProfile.strUserName, serverProfile.strProfileName).DecryptByPrivateKey(httptrx.ecs, out AREGFIN_DecryptContent, out AREGFIN_DecryptReturnMsg);
                    if (AREGFIN_DecryptReturnCode != 0)
                    {
                        ReturnStatus = false;
                        ReturnMsg = "Decrypt By PrivateKey Error";
                        aregfin = null;
                        return ReturnStatus;
                    }
                    else
                    {
                        ECS HESC = DeserializeObj._ECS(AREGFIN_DecryptContent);
                        if (HESC == null)
                        {
                            ReturnStatus = false;
                            ReturnMsg = "Deserialize ECS Object Error";
                            aregfin = null;
                            return ReturnStatus;

                        }
                        else
                        {
                            string DecrypStr = this.DecryptDESData(HESC.Key, HESC.IV, httptrx.datacontent);
                            if (DecrypStr == string.Empty)
                            {
                                ReturnStatus = false;
                                ReturnMsg = "Decrypt by DES Object Error";
                                aregfin = null;
                                return ReturnStatus;
                            }
                            else
                            {
                                aregfin = DeserializeObj._AREGFIN(DecrypStr);
                                if (aregfin == null)
                                {
                                    ReturnStatus = false;
                                    ReturnMsg = "Deserialize APREGPLY  Object Error";
                                    aregfin = null;
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




        private string DecryptDESData(string key, string iv, string DataContent)
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


        private string GetMacAddress()
        {
            string FirstEthernetMacAddress = string.Empty;
            var EthernetInfo = NetworkInterface.GetAllNetworkInterfaces().Where(E => E.NetworkInterfaceType == NetworkInterfaceType.Ethernet).FirstOrDefault();
            if(EthernetInfo != null)
            {
                FirstEthernetMacAddress = EthernetInfo.GetPhysicalAddress().ToString();
            }

            return FirstEthernetMacAddress;
        }

    }
}
