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

namespace EnterpriseAuth.ViewModels
{

    public class RegisterViewModel : BaseViewModel
    {
        private ServerProfile serverProfile = new ServerProfile();
        private string messageInfo = "";
        private BitmapImage stateImage = new BitmapImage();
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

        public async void RegisterServer(/*RegisterReplyDelegate del*/)
        {
            this.DisplayConnectingStatus();

            RegisterRequest registerRequest = new RegisterRequest();
            registerRequest.serverURL = "http://" + serverProfile.strServerURL + ":" + serverProfile.strServerPort + "/registerrequest";
            registerRequest.userName = serverProfile.strUserName;
            registerRequest.userPassword = serverProfile.strPassword;
            registerRequest.publicKey = serverProfile.strPublicKey;
            registerRequest.processStep = "REGISTER_REQUEST";

            var modifiedAssetJSON = JsonConvert.SerializeObject(registerRequest);
            StringContent requestContent = null;
            requestContent = new StringContent(modifiedAssetJSON, Encoding.UTF8, "application/json");

            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(registerRequest.serverURL);

                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                // httpClient.DefaultRequestHeaders.Add("authorization", "token {api token}");

                try
                {
                    HttpResponseMessage response =await httpClient.PostAsync(registerRequest.serverURL, requestContent);
                    //response.EnsureSuccessStatusCode();
                    if (response.IsSuccessStatusCode)
                    {
                        HttpContent content = response.Content;
                        //string jsonContent = content.ReadAsStringAsync().Result.Replace("\\", "").Trim(new char[1] { '"' }); ;
                        string jsonContent = await content.ReadAsStringAsync();
                        RegisterReply registerReply = JsonConvert.DeserializeObject<RegisterReply>(jsonContent);
                        this.clientID = registerReply.clientID;
                        this.serverProfile.strPublicKey = registerReply.serverPublicKey;
                        this.serverProfile.strCredential = registerReply.credentialHash;
                        this.OpenWebSocket();
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

        private void WebSocket4Net_Opened(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() => {
                this.MessageInfo = "Verifying Client...";
            }, DispatcherPriority.Background);

            this.WebSocket4Net_SendVerifyMessage();
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
            }
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
        }

        public void GenerateQRCode()
        {
            string strQRCodeText = this.serverProfile.strServerURL + this.serverProfile.strPublicKey;
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

    }
}
