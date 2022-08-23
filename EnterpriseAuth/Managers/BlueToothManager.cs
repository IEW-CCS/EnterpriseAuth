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

namespace EnterpriseAuth.Managers
{
    public class BlueToothManager
    {
        BluetoothLEDevice _bleDevice;
        //BluetoothLEAdvertisementWatcher _watcher = new BluetoothLEAdvertisementWatcher();
        private string bioServiceUUID = "80d27b8a-f1b4-474e-9398-b686f5e71bfb";
        //private string bioServiceUUID = "80D27B8A-F1b4-474E-9398-B686F5E71BFB";

        public void TestScan()
        {
            BluetoothLEAdvertisementWatcher watcher = new BluetoothLEAdvertisementWatcher();
            watcher.ScanningMode = BluetoothLEScanningMode.Active;

            // Only activate the watcher when we're recieving values >= -80
            watcher.SignalStrengthFilter.InRangeThresholdInDBm = -80;

            // Stop watching if the value drops below -90 (user walked away)
            watcher.SignalStrengthFilter.OutOfRangeThresholdInDBm = -90;

            // Register callback for when we see an advertisements
            watcher.Received += OnAdvertisementReceived;
            

            // Wait 5 seconds to make sure the device is really out of range
            watcher.SignalStrengthFilter.OutOfRangeTimeout = TimeSpan.FromMilliseconds(5000);
            watcher.SignalStrengthFilter.SamplingInterval = TimeSpan.FromMilliseconds(2000);

            // Starting watching for advertisements
            watcher.Start();
        }

        private void OnAdvertisementReceived(BluetoothLEAdvertisementWatcher watcher, BluetoothLEAdvertisementReceivedEventArgs eventArgs)
        {
            // Tell the user we see an advertisement and print some properties
            Console.WriteLine(String.Format("Advertisement:"));
            Console.WriteLine(String.Format("  BT_ADDR: {0}", eventArgs.BluetoothAddress));
            Console.WriteLine(String.Format("  FR_NAME: {0}", eventArgs.Advertisement.LocalName));
            if (eventArgs.Advertisement.LocalName == "BioAuth")
            {
                watcher.Received -= OnAdvertisementReceived; 
                watcher.Stop();

                Console.WriteLine("BioAuth device found.");
                ConnectDevice(eventArgs.BluetoothAddress);
                /*
                foreach (Guid id in eventArgs.Advertisement.ServiceUuids)
                {
                    Console.WriteLine(String.Format("  SR_UUID: {0}", id.ToString()));
                }
                */
            }
        }

        async void ConnectDevice(ulong address)
        {
            Console.WriteLine("Start to connect device...");
            try
            {
                BluetoothLEDevice bleDevice = await BluetoothLEDevice.FromBluetoothAddressAsync(address);
                //BluetoothLEHelper bluetoothLEHelper = BluetoothLEHelper.Context;
                if (bleDevice != null)
                {
                    this._bleDevice = bleDevice;
                    Console.WriteLine("Connect to BioAuth successful !");
                    Console.WriteLine(String.Format("  DeviceID: {0}", bleDevice.DeviceId));
                    Console.WriteLine(String.Format("  BluetoothDeviceID: {0}", bleDevice.BluetoothDeviceId));
                    Console.WriteLine(String.Format("  ConnectionStatus: {0}", bleDevice.ConnectionStatus));
                    Console.WriteLine(String.Format("  DeviceInformation ID: {0}", bleDevice.DeviceInformation.Id));
                    Console.WriteLine(String.Format("  DeviceInformation Name: {0}", bleDevice.DeviceInformation.Name));

                    try
                    {
                        GattDeviceServicesResult result = await bleDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached);

                        if (result.Status == GattCommunicationStatus.Success)
                        {
                            var services = result.Services;
                            Console.WriteLine(String.Format("Found {0} services", services.Count));
                            foreach (var service in services)
                            {
                                Console.WriteLine("Service UUID: {0}", service.Uuid);

                                if(service.Uuid.ToString() == this.bioServiceUUID)
                                {
                                    GetCharacteristicsInfo(service);
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
                }
                bleDevice.Dispose();
            }
            catch(Exception ex)
            {
                Console.WriteLine("BLE Connection Exception: " + ex.Message);
            }
            
        }

        public async void GetCharacteristicsInfo(GattDeviceService service)
        {
            IReadOnlyList<GattCharacteristic> characteristics = null;
            try
            {
                // Ensure we have access to the device.
                var accessStatus = await service.RequestAccessAsync();
                if (accessStatus == DeviceAccessStatus.Allowed)
                {
                    // BT_Code: Get all the child characteristics of a service. Use the cache mode to specify uncached characterstics only 
                    // and the new Async functions to get the characteristics of unpaired devices as well. 
                    var chatResult = await service.GetCharacteristicsAsync(BluetoothCacheMode.Uncached);
                    if (chatResult.Status == GattCommunicationStatus.Success)
                    {
                        characteristics = chatResult.Characteristics;
                        foreach(GattCharacteristic characteristic in characteristics)
                        {
                            Console.WriteLine("Characteristicce UUID: {0}", characteristic.Uuid);
                            try
                            {
                                GattReadResult readResult = await characteristic.ReadValueAsync(BluetoothCacheMode.Uncached);
                                if (readResult.Status == GattCommunicationStatus.Success)
                                {
                                    Console.WriteLine("Characteristicce Value: {0}", readResult.Value);
                                }
                                else
                                {
                                    Console.WriteLine("Read failed: {0}", readResult.Status);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Read Characteristicce Value Exception: {0}", ex.Message);
                            }
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
                    // Not granted access
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

        public void DisconnectDevice()
        {
            
            if(this._bleDevice != null)
            {
                //Console.WriteLine("Before Dispose Status: {0}", this._bleDevice.ConnectionStatus.ToString());
                //this._bleDevice.ConnectionStatusChanged -= ConnectionStatusChangeHandlerAsync;
                this._bleDevice.Dispose();
                this._bleDevice = null;
                GC.Collect();
                //Console.WriteLine("After Dispose Status: {0}", this._bleDevice.ConnectionStatus.ToString());
            }
        }
    }
}
