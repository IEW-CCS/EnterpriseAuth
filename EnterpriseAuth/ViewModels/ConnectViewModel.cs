using System;
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
using System.Net.NetworkInformation;
using System.Diagnostics;

namespace EnterpriseAuth.ViewModels
{
    public class ConnectViewModel : BaseViewModel
    {
        //public event EventHandler ProfileUpdateEventHandler;

        public ServerProfile serverProfile = new ServerProfile();
        private string messageInfo = "";
        private BitmapImage stateImage = new BitmapImage();
        //BlueToothManager btManager = new BlueToothManager();
        BlueToothManager btManager;
        private int eventCount = 0;
        private bool HashPasswordReq = false;


        private string userPassword = "";
        private string passCode = "";
        private string serialNumber = "";
        private string credentialSign = "";
        private string hashPassword = "";
        private bool deviceConnectedFlag = false;
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
        public string ConnectMessageInfo
        {
            get { return this.messageInfo; }
            set
            {
                this.messageInfo = value;
                OnPropertyChanged("ConnectMessageInfo");
            }
        }

        public BitmapImage ConnectStateImage
        {
            get { return this.stateImage; }
            set
            {
                this.stateImage = value;
                OnPropertyChanged("ConnectStateImage");
            }
        }

        public ConnectViewModel(ServerProfile sProfile)
        {
            this.serverProfile = sProfile;
            this.btManager = MainWindow._btm;
        }

        public async void StartAuthentication()
        {
            Console.WriteLine("Start Authentication Process......");
            HashPasswordReq = true;

            this.DisplayConnectingStatus();

            AACONREQ AuthConnectRequest = new AACONREQ();
            string ServerURL = "http://" + serverProfile.strServerURL + ":" + serverProfile.strServerPort + "/" + serverProfile.strAuthenticationURL;
            AuthConnectRequest.DeviceCode = GetMacAddress();
            string AuthConnectRequestJson = JsonConvert.SerializeObject(AuthConnectRequest);


            //------ 20220916 Not Base DES use AuthDES 加密方法
            AuthDES DES = new AuthDES();
            string AuthConnectRequestDES = DES.EncryptDES(AuthConnectRequestJson);

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
                HttpSend.procstep = EnumList.ProcessStep.AACONREQ.ToString();
                HttpSend.returncode = 0;
                HttpSend.returnmsg = string.Empty;
                HttpSend.datacontent = AuthConnectRequestDES;
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
                    httpClient.DefaultRequestHeaders.Add("Authorization", "bearer " + this.serverProfile.strAuthenticationTokenID);

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

                            AACONPLY vconply = null;
                            string returnMsg = string.Empty;

                            if (Handle_AACONPLY(jsonContent, out vconply, out returnMsg) == false)
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    this.ConnectMessageInfo = "Handle AACONPLY  Error: " + returnMsg;
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
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                this.ConnectMessageInfo = "HTTP Response Error: " + response.StatusCode;
                            }, DispatcherPriority.Background);
                        }
                    }
                    catch (Exception ex)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            this.ConnectMessageInfo = "HTTP Post Exception: " + ex.Message;
                        }, DispatcherPriority.Background);
                    }
                }
            }
        }

        public async void VerifyUser()
        {
            AAUTHREQ verifyRequest = new AAUTHREQ();
            string ServerURL = "http://" + serverProfile.strServerURL + ":" + serverProfile.strServerPort + "/" + serverProfile.strAuthenticationURL;
            verifyRequest.PassWord = this.userPassword;
            verifyRequest.PassCode = this.passCode;

            string verifyRequestJson = JsonConvert.SerializeObject(verifyRequest);


            //------ 20220916 Not Base DES use AuthDES 加密方法
            AuthDES DES = new AuthDES();
            string verifyRequestDES = DES.EncryptDES(verifyRequestJson);

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
                HttpSend.procstep = EnumList.ProcessStep.AAUTHREQ.ToString();
                HttpSend.returncode = 0;
                HttpSend.returnmsg = string.Empty;
                HttpSend.datacontent = verifyRequestDES;
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
                    httpClient.DefaultRequestHeaders.Add("Authorization", "bearer " + this.serverProfile.strAuthenticationTokenID);

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

                            AAUTHPLY apvryply = null;
                            string returnMsg = string.Empty;

                            if (Handle_AAUTHPLY(jsonContent, out apvryply, out returnMsg) == false)
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    this.ConnectMessageInfo = "Handle APVRYPLY  Error: " + returnMsg;
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
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                this.ConnectMessageInfo = "HTTP Response Error: " + response.StatusCode;
                            }, DispatcherPriority.Background);
                        }
                    }
                    catch (Exception ex)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            this.ConnectMessageInfo = "HTTP Post Exception: " + ex.Message;
                        }, DispatcherPriority.Background);
                    }

                }
            }
        }

        public async void HashPasswordRequest()
        {
            AAPSWREQ pwdRequest = new AAPSWREQ();
            string ServerURL = "http://" + serverProfile.strServerURL + ":" + serverProfile.strServerPort + "/" + serverProfile.strAuthenticationURL;
            pwdRequest.BiometricsResult = "OK";
            pwdRequest.SerialNumber = this.serialNumber;
            pwdRequest.CredentialSign = this.credentialSign;

            string pwdRequestJson = JsonConvert.SerializeObject(pwdRequest);
            //  AuthBaseDES BaseDES = new AuthBaseDES();
            //  string pwdRequestDES = BaseDES.EncryptDES(pwdRequestJson);

            //------ 20220916 Not Base DES use AuthDES 加密方法
            AuthDES DES = new AuthDES();
            string pwdRequestDES = DES.EncryptDES(pwdRequestJson);

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
                HttpSend.procstep = EnumList.ProcessStep.AAPSWREQ.ToString();
                HttpSend.returncode = 0;
                HttpSend.returnmsg = string.Empty;
                HttpSend.datacontent = pwdRequestDES;
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
                    httpClient.DefaultRequestHeaders.Add("Authorization", "bearer " + this.serverProfile.strAuthenticationTokenID);

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

                            AAPSWPLY ahpwply = null;
                            string returnMsg = string.Empty;

                            if (Handle_AAPSWPLY(jsonContent, out ahpwply, out returnMsg) == false)
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    this.ConnectMessageInfo = "Handle AAPSWREQ  Error: " + returnMsg;
                                }, DispatcherPriority.Background);
                            }
                            else
                            {
                                //Connection reply successful
                                this.hashPassword = ahpwply.PasswordData;
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    this.ConnectMessageInfo = "Hash PWD:  " + this.hashPassword;
                                }, DispatcherPriority.Background);

                                //Connect to VPN or Citrix
                                ConnectRemoteServer();

                                //Share Desktop Screen to Remore
                                ShareDesktop();
                            }
                        }
                        else
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                this.ConnectMessageInfo = "HTTP Response Error: " + response.StatusCode;
                            }, DispatcherPriority.Background);
                        }
                    }
                    catch (Exception ex)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            this.ConnectMessageInfo = "HTTP Post Exception: " + ex.Message;
                        }, DispatcherPriority.Background);
                    }
                }
            }
        }

        public void ConnectRemoteServer()
        {
            //this.btManager.DisconnectDevice();
            // string exeString = "\"C:\\Program Files\\OpenVPN\\bin\\openvpn.exe\" --config +\"C:\\Program Files\\OpenVPN\\bin\\config-pass.ovpn\"";
            //string passString = @"""C:\\Program Files\\OpenVPN\\bin\\config-pass.ovpn""";
            //WritePasswordInfo("james", "james");
            WritePasswordInfo(this.serverProfile.strUserName, this.hashPassword);
            System.Diagnostics.Process.Start("CMD.exe", "/K C:\\StartOpenVpnConnect.bat");

            /*
            string fullPath = @"C:\\Program Files\\OpenVPN\\bin\\openvpn.exe";
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.WindowStyle = ProcessWindowStyle.Normal;
            psi.FileName = "CMD.exe"; //Path.GetFileName(fullPath);
            psi.WorkingDirectory = Path.GetDirectoryName(fullPath);
            psi.Arguments = fullPath + " --config C:\\Program Files\\OpenVPN\\bin\\config-pass.ovpn";
            Process.Start(psi);
            */
        }
        
        public void ShareDesktop()
        {
            System.Diagnostics.Process.Start("CMD.exe", "/C C:\\StartChrome.bat");
        }

        private bool Handle_AACONPLY(string DataContent, out AACONPLY vconply, out string ReturnMsg)
        {
            bool ReturnStatus = true;
            //HttpTrx httptrx = DeserializeObj._HttpTrx(DataContent);
            if (!DeserializeObj.TryParseJson(DataContent, out HttpTrx httptrx))
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

                    string DecryptECS = string.Empty;
                    string returnMsg = string.Empty;
                    int returnCode = MainWindow.ObjectSecutiry.GetRSASecurity(serverProfile.strUserName, serverProfile.strProfileName).Decrypt_Check(httptrx.ecs, httptrx.ecssign, out DecryptECS, out returnMsg);
                    if (returnCode != 0)
                    {
                        ReturnStatus = false;
                        ReturnMsg = "Http RSA Decrypt Error, Message = " + returnMsg;
                        vconply = null;
                        return ReturnStatus;
                    }
                    else
                    {
                        //ECS HESC = DeserializeObj._ECS(DecryptECS);
                        if (!DeserializeObj.TryParseJson(DecryptECS, out ECS HESC))
                        {
                            ReturnStatus = false;
                            ReturnMsg = "Deserialize ECS Object Error";
                            vconply = null;
                            return ReturnStatus;

                        }
                        else
                        {
                            string DecrypStr = this.DecryptDESData(HESC.Key, HESC.IV, httptrx.datacontent);
                            if (DecrypStr == string.Empty)
                            {
                                ReturnStatus = false;
                                ReturnMsg = "Decrypt by DES Object Error";
                                vconply = null;
                                return ReturnStatus;
                            }
                            else
                            {
                                //vconply = DeserializeObj._AACONPLY(DecrypStr);

                                if (!DeserializeObj.TryParseJson(DecrypStr, out vconply))
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

        private bool Handle_AAUTHPLY(string DataContent, out AAUTHPLY apvryply, out string ReturnMsg)
        {
            bool ReturnStatus = true;
            //HttpTrx httptrx = DeserializeObj._HttpTrx(DataContent);
            if (!DeserializeObj.TryParseJson(DataContent, out HttpTrx httptrx))
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
                    string APVRYPLY_DecryptContent = string.Empty;
                    string APVRYPLY_DecryptReturnMsg = string.Empty;
                    int APVRYPLY_DecryptReturnCode = 0;
                    APVRYPLY_DecryptReturnCode = MainWindow.ObjectSecutiry.GetRSASecurity(serverProfile.strUserName, serverProfile.strProfileName).DecryptByPrivateKey(httptrx.ecs, out APVRYPLY_DecryptContent, out APVRYPLY_DecryptReturnMsg);
                    if (APVRYPLY_DecryptReturnCode != 0)
                    {
                        ReturnStatus = false;
                        ReturnMsg = "Decrypt By PrivateKey Error";
                        apvryply = null;
                        return ReturnStatus;
                    }
                    else
                    {
                        //ECS HESC = DeserializeObj._ECS(APVRYPLY_DecryptContent);
                        if (!DeserializeObj.TryParseJson(APVRYPLY_DecryptContent, out ECS HESC))
                        {
                            ReturnStatus = false;
                            ReturnMsg = "Deserialize ECS Object Error";
                            apvryply = null;
                            return ReturnStatus;

                        }
                        else
                        {
                            string DecrypStr = this.DecryptDESData(HESC.Key, HESC.IV, httptrx.datacontent);
                            if (DecrypStr == string.Empty)
                            {
                                ReturnStatus = false;
                                ReturnMsg = "Decrypt by DES Object Error";
                                apvryply = null;
                                return ReturnStatus;
                            }
                            else
                            {
                                //apvryply = DeserializeObj._APVRYPLY(DecrypStr);
                                if (!DeserializeObj.TryParseJson(DecrypStr, out apvryply))
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

        private bool Handle_AAPSWPLY(string DataContent, out AAPSWPLY ahpwply, out string ReturnMsg)
        {
            bool ReturnStatus = true;
            //HttpTrx httptrx = DeserializeObj._HttpTrx(DataContent);
            if (!DeserializeObj.TryParseJson(DataContent, out HttpTrx httptrx))
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
                        //ECS HESC = DeserializeObj._ECS(AHPWPLY_DecryptContent);
                        if (!DeserializeObj.TryParseJson(AHPWPLY_DecryptContent, out ECS HESC))
                        {
                            ReturnStatus = false;
                            ReturnMsg = "Deserialize ECS Object Error";
                            ahpwply = null;
                            return ReturnStatus;

                        }
                        else
                        {
                            string DecrypStr = this.DecryptDESData(HESC.Key, HESC.IV, httptrx.datacontent);
                            if (DecrypStr == string.Empty)
                            {
                                ReturnStatus = false;
                                ReturnMsg = "Decrypt by DES Object Error";
                                ahpwply = null;
                                return ReturnStatus;
                            }
                            else
                            {
                                //ahpwply = DeserializeObj._AAPSWPLY(DecrypStr);
                                if (!DeserializeObj.TryParseJson(DecrypStr, out ahpwply))
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

        public void  StartBLEAuthentication()
        {
            this.DisplayBlueToothStatus();
            this.btManager.ScanAndConnect();

            if (!this.deviceConnectedFlag)
            {
                this.btManager.BLEMessageEvent += this.BLEMessage_Received;
                this.btManager.BiometricsVerifyEvent += this.BiometricsVerify_Received;
                this.btManager.CredentialContentEvent += this.CredentialContent_Received;
                this.eventCount++;
                Console.WriteLine("this.btManager.CredentialContentEvent Count: {0}", this.eventCount);


                this.deviceConnectedFlag = true;
            }
        }

        public void DisplayConnectingStatus()
        {
            Application.Current.Dispatcher.Invoke(() => {
                this.ConnectStateImage = new BitmapImage(new Uri("/images/connect.jpeg", UriKind.Relative));
                this.ConnectMessageInfo = "Connecting to Server...";
            }, DispatcherPriority.Background);
        }

        public void DisplayBlueToothStatus()
        {
            Application.Current.Dispatcher.Invoke(() => {
                this.ConnectStateImage = new BitmapImage(new Uri("/images/bluetooth.jpeg", UriKind.Relative));
                this.ConnectMessageInfo = "Connecting to BlueTooth Device...";
            }, DispatcherPriority.Background);
        }

        private void BLEMessage_Received(object sender, EventArgs e)
        {
            BLEMessageEventArgs pe = e as BLEMessageEventArgs;
            Console.WriteLine("BLEMessage_Received: " + pe.bleMessage);

            Application.Current.Dispatcher.Invoke(() => {
                this.ConnectMessageInfo = pe.bleMessage;
            }, DispatcherPriority.Background);
        }

        private void BiometricsVerify_Received(object sender, EventArgs e)
        {
            BLEMessageEventArgs pe = e as BLEMessageEventArgs;
            Console.WriteLine("Biometrics Verify " + pe.bleMessage);

            Application.Current.Dispatcher.Invoke(() => {
                this.ConnectMessageInfo = "Biometrics Verify " + pe.bleMessage;
            }, DispatcherPriority.Background);
        }

        private void CredentialContent_Received(object sender, EventArgs e)
        {
            BLEMessageEventArgs pe = e as BLEMessageEventArgs;
            Console.WriteLine("Received Credential Content: " + pe.bleMessage);

            this.credentialSign = pe.bleMessage;

            Application.Current.Dispatcher.Invoke(() => {
                this.ConnectMessageInfo = "Receive and Compare Credential Content OK";
            }, DispatcherPriority.Background);

        //    if (HashPasswordReq == true)
         //   {
                HashPasswordReq = false;
                this.HashPasswordRequest();
     //       }
        }

        private string GetMacAddress()
        {
            string FirstEthernetMacAddress = string.Empty;
            var EthernetInfo = NetworkInterface.GetAllNetworkInterfaces().Where(E => E.NetworkInterfaceType == NetworkInterfaceType.Ethernet).FirstOrDefault();
            if (EthernetInfo != null)
            {
                FirstEthernetMacAddress = EthernetInfo.GetPhysicalAddress().ToString();
            }

            return FirstEthernetMacAddress;
        }

        public void TestBLE()
        {
            Console.WriteLine("Start to Test BLE...");

            this.DisplayBlueToothStatus();
            
            this.btManager.ScanAndConnect();
            if(!this.deviceConnectedFlag)
            {
                this.btManager.BLEMessageEvent += this.BLEMessage_Received;
                this.btManager.BiometricsVerifyEvent += this.BiometricsVerify_Received;
                this.btManager.CredentialContentEvent += this.CredentialContent_Received;
                this.eventCount++;
                Console.WriteLine("this.btManager.CredentialContentEvent Count: {0}", this.eventCount);

                this.deviceConnectedFlag = true;
            }
        }

        public static void WritePasswordInfo(string user_name, string password)
        {
            using (StreamWriter sw = new StreamWriter("pwdinfo.txt"))
            {
                // Add some text to the file.
                sw.WriteLine(user_name);
                sw.WriteLine(password);
            }
        }

    }
}
