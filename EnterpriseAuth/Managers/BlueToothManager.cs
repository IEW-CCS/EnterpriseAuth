using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using System.Threading;

namespace EnterpriseAuth.Managers
{
    public class BlueToothManager
    {
        public event EventHandler BiometricsVerifyEvent;
        public event EventHandler BLEMessageEvent;
        public event EventHandler CredentialContentEvent;

        BluetoothLEAdvertisementWatcher _watcher;

        //BLE simulator UUID list
        //private string bioServiceUUID = "80d27b8a-f1b4-474e-9398-b686f5e71bfb";
        //private string bioSwitchFlagUUID = "48fe1431-5edd-4177-9050-77ad1a629d27";
        //private string bioVerifyResultUUID = "c43402ad-a4bd-46c8-9893-d5a78e44c885";

        //LightBlue simulator UUID list
        private string deviceName = "BioAuth";
        private Guid bioDeviceUUID = new Guid("ada5adcc-a829-4b05-bb0e-9ac4a40d2193");
        private Guid bioServiceUUID = new Guid("e593247c-bc00-41a3-93d0-3ad4b64b27cb");
        private Guid bioSwitchFlagUUID = new Guid("97a1c8e5-e399-4124-a363-750d1c7102af");
        private Guid bioVerifyResultUUID = new Guid("4e4a3a1b-fd4a-40a5-a08f-586078499da9");
        private Guid bioCredentialUUID = new Guid("92f59a03-0e61-41eb-b758-64460c72706a");

        //private Guid bioDeviceUUID = new Guid("ADA5ADCC-A829-4B05-BB0E-9AC4A40D2193");
        //private Guid bioServiceUUID = new Guid("E593247C-BC00-41A3-93D0-3AD4B64B27CB");
        //private Guid bioSwitchFlagUUID = new Guid("97A1C8E5-E399-4124-A363-750D1C7102AF");
        //private Guid bioVerifyResultUUID = new Guid("4E4A3A1B-FD4A-40A5-A08F-586078499DA9");
        //private Guid bioCredentialUUID = new Guid("ADA5ADCC-A829-4B05-BB0E-9AC4A40D2193");


        public BluetoothLEDevice BioDevice { get; set; }
        public GattDeviceService BioService { get; set; }
        public GattCharacteristic BioSwitchCharacteristic { get; set; }
        public GattCharacteristic VerifyResultCharacteristic { get; set; }
        public GattCharacteristic CredentialContentCharacteristic { get; set; }

        public void ScanAndConnect()
        {
            this._watcher = new BluetoothLEAdvertisementWatcher();
            this._watcher.ScanningMode = BluetoothLEScanningMode.Active;

            // Only activate the watcher when we're recieving values >= -80
            this._watcher.SignalStrengthFilter.InRangeThresholdInDBm = -80;

            // Stop watching if the value drops below -90 (user walked away)
            this._watcher.SignalStrengthFilter.OutOfRangeThresholdInDBm = -90;

            // Register callback for when we see an advertisements
            this._watcher.Received += OnAdvertisementReceived;

            // Wait 5 seconds to make sure the device is really out of range
            this._watcher.SignalStrengthFilter.OutOfRangeTimeout = TimeSpan.FromMilliseconds(5000);
            this._watcher.SignalStrengthFilter.SamplingInterval = TimeSpan.FromMilliseconds(2000);

            // Starting watching for advertisements
            this._watcher.Start();
        }

        private async void Device_ConnectionStatusChanged(BluetoothLEDevice device, object args)
        {
            if (device == null)
            {
                Console.WriteLine("Device_ConnectionStatusChanged: Device is null");
                return;
            }

            if (device.ConnectionStatus == BluetoothConnectionStatus.Connected)
            {
                Console.WriteLine("Device ConnectionStatus is Connected");
                if (this.VerifyResultCharacteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Notify))
                {
                    Console.WriteLine("Subscribe Value Changed Event");
                    this.VerifyResultCharacteristic.ValueChanged += Characteristic_ValueChanged;

                    // Set the notify enable flag
                    try
                    {
                        Console.WriteLine("WriteClientCharacteristicConfigurationDescriptorAsync()...");
                        await this.VerifyResultCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("WriteClientCharacteristicConfigurationDescriptorAsync Exception: {0}", ex.Message);
                    }
                }

            }

            if (device.ConnectionStatus == BluetoothConnectionStatus.Disconnected)
            {
                Console.WriteLine("Device ConnectionStatus is Disconnected");
                this.VerifyResultCharacteristic.ValueChanged -= Characteristic_ValueChanged;
            }
        }

        private void OnAdvertisementReceived(BluetoothLEAdvertisementWatcher watcher, BluetoothLEAdvertisementReceivedEventArgs eventArgs)
        {
            Console.WriteLine(String.Format("Advertisement:"));
            Console.WriteLine(String.Format("  BT_ADDR: {0}", eventArgs.BluetoothAddress));
            Console.WriteLine(String.Format("  FR_NAME: {0}", eventArgs.Advertisement.LocalName));
            if (eventArgs.Advertisement.LocalName == this.deviceName)
            {
                Console.WriteLine("BioAuth device found.");
                BLEMessageEventArgs arg = new BLEMessageEventArgs();
                arg.bleMessage = "BioAuth device found.";
                this.BLEMessageEvent(this, arg);

                this._watcher.Received -= OnAdvertisementReceived;
                this._watcher.Stop();

                ConnectDevice(eventArgs.BluetoothAddress);
            }
        }

        async void ConnectDevice(ulong address)
        {
            Console.WriteLine("Start to connect device...");
            try
            {
                //BluetoothLEDevice bleDevice = await BluetoothLEDevice.FromBluetoothAddressAsync(address);
                this.BioDevice = await BluetoothLEDevice.FromBluetoothAddressAsync(address);
                if (this.BioDevice != null)
                {
                    //this._bleDevice = bleDevice;
                    Console.WriteLine("Connect to BioAuth successful !");
                    Console.WriteLine(String.Format("  DeviceID: {0}", this.BioDevice.DeviceId));
                    Console.WriteLine(String.Format("  BluetoothDeviceID: {0}", this.BioDevice.BluetoothDeviceId));
                    Console.WriteLine(String.Format("  ConnectionStatus: {0}", this.BioDevice.ConnectionStatus));
                    Console.WriteLine(String.Format("  DeviceInformation ID: {0}", this.BioDevice.DeviceInformation.Id));
                    Console.WriteLine(String.Format("  DeviceInformation Name: {0}", this.BioDevice.DeviceInformation.Name));

                    try
                    {
                        GattDeviceServicesResult result = await this.BioDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached);

                        if (result.Status == GattCommunicationStatus.Success)
                        {
                            var services = result.Services;
                            //Console.WriteLine(String.Format("Found {0} services", services.Count));
                            foreach (var service in services)
                            {
                                Console.WriteLine("Service UUID: {0}", service.Uuid);

                                if(service.Uuid.ToString() == this.bioServiceUUID.ToString().ToLower())
                                {
                                    Console.WriteLine("Found Specific Service");
                                    Console.WriteLine(String.Format("Found {0} services", services.Count));
                                    this.BioService = service;
                                    await GetCharacteristicsInfo(this.BioService);
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("Device unreachable");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("GetGattServices Exception: " + ex.Message);
                    }
                    this.BioDevice.ConnectionStatusChanged += Device_ConnectionStatusChanged;
                    Console.WriteLine();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("BLE Connection Exception: " + ex.Message);
            }
        }

        public async Task GetCharacteristicsInfo(GattDeviceService service)
        {
            IReadOnlyList<GattCharacteristic> characteristics = null;
            try
            {
                var accessStatus = await service.RequestAccessAsync();
                if (accessStatus == DeviceAccessStatus.Allowed)
                {
                    var charResult = await service.GetCharacteristicsAsync(BluetoothCacheMode.Uncached);
                    if (charResult.Status == GattCommunicationStatus.Success)
                    {
                        characteristics = charResult.Characteristics;
                        foreach(GattCharacteristic characteristic in characteristics)
                        {
                            Console.WriteLine("**** Characteristic ID: [ {0} ] ****", characteristic.Uuid.ToString());

                            if (characteristic.Uuid.ToString().ToLower() == this.bioSwitchFlagUUID.ToString().ToLower())
                            {
                                Console.WriteLine("Found BioSwitchFlag Characteristic!!");
                                this.BioSwitchCharacteristic = characteristic;

                                //Console.WriteLine("Read Value from BioSwitchFlag Characteristic");
                                //await ReadCharacteristicValue(characteristic);

                                Console.WriteLine("Turn ON BioSwitchFlag Characteristic");
                                var writeBuffer = CryptographicBuffer.ConvertStringToBinary("ON", BinaryStringEncoding.Utf8);
                                var writeSuccessful = await WriteValueToCharacteristic(characteristic, writeBuffer);
                                if(writeSuccessful)
                                {
                                    Console.WriteLine("Turn ON Biometrics Successful, wait for verify result");
                                    //Trigger Event Handler (Delegate) to Inform RegisterViewModel
                                    BLEMessageEventArgs arg = new BLEMessageEventArgs();
                                    arg.bleMessage = "Waiting for Biometrics Verify";
                                    this.BLEMessageEvent(this, arg);
                                }
                            }

                            if (characteristic.Uuid.ToString().ToLower() == this.bioVerifyResultUUID.ToString().ToLower())
                            {
                                Console.WriteLine("Found BioVerifyResult Characteristic!!");
                                Console.WriteLine("Subscribe Notification for BioVerifyResult Characteristic");
                                this.VerifyResultCharacteristic = characteristic;

                                var clientResult = await this.VerifyResultCharacteristic.ReadClientCharacteristicConfigurationDescriptorAsync();

                                this.VerifyResultCharacteristic.ProtectionLevel = GattProtectionLevel.Plain;

                                await VerifyResultNotification(this.VerifyResultCharacteristic);
                            }

                            if(characteristic.Uuid.ToString().ToLower() == this.bioCredentialUUID.ToString().ToLower())
                            {
                                Console.WriteLine("Found CredentialContent Characteristic!!");
                                this.CredentialContentCharacteristic = characteristic;
                            }
                            Console.WriteLine();
                        }
                    }
                    else
                    {
                        Console.WriteLine("Error to access service");
                        characteristics = new List<GattCharacteristic>();
                    }
                }
                else
                {
                    Console.WriteLine("No granted access for service");
                    characteristics = new List<GattCharacteristic>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Restricted service. Can't read characteristics: " + ex.Message);
                characteristics = new List<GattCharacteristic>();
            }

        }

        private async Task ReadCharacteristicValue(GattCharacteristic characteristic)
        {
            GattReadResult result = await characteristic.ReadValueAsync(BluetoothCacheMode.Uncached);
            if (result.Status == GattCommunicationStatus.Success)
            {
                byte[] data;
                CryptographicBuffer.CopyToByteArray(result.Value, out data);
                Console.WriteLine($"Read Value result: {Encoding.UTF8.GetString(data)}");
            }
            else
            {
                Console.WriteLine($"Read Value failed: {result.Status}");
            }
        }

        private async Task<bool> WriteValueToCharacteristic(GattCharacteristic characteristic, IBuffer buffer)
        {
            try
            {
                var result = await characteristic.WriteValueWithResultAsync(buffer);

                if (result.Status == GattCommunicationStatus.Success)
                {
                    Console.WriteLine("Write Value to Characteristic({0}) successful", characteristic.Uuid.ToString());
                    return true;
                }
                else
                {
                    Console.WriteLine("Write Value to Characteristic({0}) failed", characteristic.Uuid.ToString());
                    return false;
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine("Write Value to Characteristic({0}) Exception: {1}", characteristic.Uuid.ToString(), ex.Message);
                return false;
            }
        }

        private async Task VerifyResultNotification(GattCharacteristic characteristic)
        {

            var cccdValue = GattClientCharacteristicConfigurationDescriptorValue.Notify;
            GattCommunicationStatus status = GattCommunicationStatus.Unreachable;
            try
            {
                status = await characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(cccdValue);
                if (status == GattCommunicationStatus.Success)
                {
                    characteristic.ValueChanged += Characteristic_ValueChanged;
                    Console.WriteLine("Successfully subscribed for value changes");
                }
                else
                {
                    Console.WriteLine("Error registering for value changes");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("WriteClientCharacteristicConfigurationDescriptorAsync Exception: {0}", ex.Message);
            }
            
        }

        private async void Characteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            byte[] data;
            CryptographicBuffer.CopyToByteArray(args.CharacteristicValue, out data);
            Console.WriteLine($"Received Changed Value: {Encoding.UTF8.GetString(data)}");

            if(Encoding.UTF8.GetString(data) == "OK")
            {
                Console.WriteLine("Start to read credential");
                BLEMessageEventArgs arg = new BLEMessageEventArgs();
                arg.bleMessage = "Biometrics Verify OK";
                this.BLEMessageEvent(this, arg);

                arg.bleMessage = "OK";
                this.BiometricsVerifyEvent(this, arg);

                GattReadResult result = await this.CredentialContentCharacteristic.ReadValueAsync(BluetoothCacheMode.Uncached);
                if (result.Status == GattCommunicationStatus.Success)
                {
                    byte[] credentialData;
                    CryptographicBuffer.CopyToByteArray(result.Value, out credentialData);
                    Console.WriteLine($"Read Credential Content: {Encoding.UTF8.GetString(credentialData)}");

                    //Trigger Event Handler (Delegate) to Inform RegisterViewModel for the Credential Content
                    //BLEMessageEventArgs arg2 = new BLEMessageEventArgs();
                    arg.bleMessage = Encoding.UTF8.GetString(credentialData);
                    this.CredentialContentEvent(this, arg);
                }
                else
                {
                    Console.WriteLine($"Read Value failed: {result.Status}");
                }

            }
            else
            {
                BLEMessageEventArgs arg = new BLEMessageEventArgs();
                arg.bleMessage = "Biometrics Verify NG";
                this.BLEMessageEvent(this, arg);

                arg.bleMessage = "NG";
                this.BiometricsVerifyEvent(this, arg);

            }

        }

        public void DisconnectDevice()
        {
            if (this.BioDevice != null)
            {
                //Console.WriteLine("Before Dispose Status: {0}", this._bleDevice.ConnectionStatus.ToString());
                //this._bleDevice.ConnectionStatusChanged -= ConnectionStatusChangeHandlerAsync;
                this.BioDevice.Dispose();
                this.BioDevice = null;
                GC.Collect();
                //Console.WriteLine("After Dispose Status: {0}", this._bleDevice.ConnectionStatus.ToString());
            }
        }
    }
}
