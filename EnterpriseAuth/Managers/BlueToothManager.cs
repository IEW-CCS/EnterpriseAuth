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
        BluetoothLEAdvertisementWatcher _watcher;
        BluetoothLEDevice _bleDevice;
        //BluetoothLEAdvertisementWatcher _watcher = new BluetoothLEAdvertisementWatcher();
        private string bioServiceUUID = "80d27b8a-f1b4-474e-9398-b686f5e71bfb";
        private string bioCharacteristicUUID = "48fe1431-5edd-4177-9050-77ad1a629d27";
        //private string bioServiceUUID = "80D27B8A-F1b4-474E-9398-B686F5E71BFB";

        public void TestScan()
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

        private void OnAdvertisementReceived(BluetoothLEAdvertisementWatcher watcher, BluetoothLEAdvertisementReceivedEventArgs eventArgs)
        {
            // Tell the user we see an advertisement and print some properties
            Console.WriteLine(String.Format("Advertisement:"));
            Console.WriteLine(String.Format("  BT_ADDR: {0}", eventArgs.BluetoothAddress));
            Console.WriteLine(String.Format("  FR_NAME: {0}", eventArgs.Advertisement.LocalName));
            if (eventArgs.Advertisement.LocalName == "BioAuth")
            {
                Console.WriteLine("BioAuth device found.");
                this._watcher.Received -= OnAdvertisementReceived;
                this._watcher.Stop();

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
                //BluetoothLEDevice bleDevice = await BluetoothLEDevice.FromBluetoothAddressAsync(address);
                this._bleDevice = await BluetoothLEDevice.FromBluetoothAddressAsync(address);
                if (this._bleDevice != null)
                {
                    //this._bleDevice = bleDevice;
                    Console.WriteLine("Connect to BioAuth successful !");
                    Console.WriteLine(String.Format("  DeviceID: {0}", this._bleDevice.DeviceId));
                    Console.WriteLine(String.Format("  BluetoothDeviceID: {0}", this._bleDevice.BluetoothDeviceId));
                    Console.WriteLine(String.Format("  ConnectionStatus: {0}", this._bleDevice.ConnectionStatus));
                    Console.WriteLine(String.Format("  DeviceInformation ID: {0}", this._bleDevice.DeviceInformation.Id));
                    Console.WriteLine(String.Format("  DeviceInformation Name: {0}", this._bleDevice.DeviceInformation.Name));

                    try
                    {
                        GattDeviceServicesResult result = await this._bleDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached);

                        if (result.Status == GattCommunicationStatus.Success)
                        {
                            var services = result.Services;
                            //Console.WriteLine(String.Format("Found {0} services", services.Count));
                            foreach (var service in services)
                            {
                                Console.WriteLine("Service UUID: {0}", service.Uuid);

                                if(service.Uuid.ToString() == this.bioServiceUUID)
                                {
                                    Console.WriteLine("Found Specific Service");
                                    Console.WriteLine(String.Format("Found {0} services", services.Count));
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
                            if (characteristic.Uuid.ToString() == this.bioCharacteristicUUID)
                            {
                                Console.WriteLine("Found Specific Characteristic!!");
                                Console.WriteLine("Characteristic UUID: {0}", characteristic.Uuid);
                                Console.WriteLine("Characteristic UserDescription: {0}", characteristic.UserDescription);

                                var result = await characteristic.GetDescriptorsAsync(BluetoothCacheMode.Uncached);
                                if (result.Status == GattCommunicationStatus.Success)
                                {
                                    var descriptors = result.Descriptors;
                                    foreach (GattDescriptor descriptor in descriptors)
                                    {
                                        Console.WriteLine("Descriptor UUID: {0}", descriptor.Uuid);
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("GetDescriptorAsync failed");
                                }

                                Console.WriteLine("Read Value from Characteristic");
                                ReadCharacteristicValue(characteristic);

                                Console.WriteLine("Write Value(ON) to Characteristic");
                                var writeBuffer = CryptographicBuffer.ConvertStringToBinary("ON", BinaryStringEncoding.Utf8);
                                var writeSuccessful = await WriteBioSwitchFlagToCharacteristicAsync(characteristic, writeBuffer);

                                Thread.Sleep(3000);

                                Console.WriteLine("Read Value from Characteristic Again");
                                ReadCharacteristicValue(characteristic);
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

        private async void ReadCharacteristicValue(GattCharacteristic characteristic)
        {
            // BT_Code: Read the actual value from the device by using Uncached.
            GattPresentationFormat presentationFormat = null;
            if (characteristic.PresentationFormats.Count > 0)
            {

                if (characteristic.PresentationFormats.Count.Equals(1))
                {
                    // Get the presentation format since there's only one way of presenting it
                    presentationFormat = characteristic.PresentationFormats[0];
                }
                else
                {
                    Console.WriteLine("Characteristic PresentationFormats is null, just return ReadCharacteristicValue funtion");
                    return;
                    // It's difficult to figure out how to split up a characteristic and encode its different parts properly.
                    // In this case, we'll just encode the whole thing to a string to make it easy to print out.
                }
            }

            GattReadResult result = await characteristic.ReadValueAsync(BluetoothCacheMode.Uncached);
            if (result.Status == GattCommunicationStatus.Success)
            {
                
                string formattedResult = FormatValueByPresentation(result.Value, presentationFormat);
                //Console.WriteLine($"Read Value result: {formattedResult}");
                byte[] data;
                CryptographicBuffer.CopyToByteArray(result.Value, out data);
                Console.WriteLine($"Read Value result: {Encoding.UTF8.GetString(data)}");
            }
            else
            {
                Console.WriteLine($"Read Value failed: {result.Status}");
            }
        }

        private async Task<bool> WriteBioSwitchFlagToCharacteristicAsync(GattCharacteristic characteristic, IBuffer buffer)
        {
            try
            {
                // BT_Code: Writes the value from the buffer to the characteristic.
                //await characteristic.WriteValueAsync(buffer);
                //return true;
                
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

        private string FormatValueByPresentation(IBuffer buffer, GattPresentationFormat format)
        {
            // BT_Code: For the purpose of this sample, this function converts only UInt32 and
            // UTF-8 buffers to readable text. It can be extended to support other formats if your app needs them.
            byte[] data;
            CryptographicBuffer.CopyToByteArray(buffer, out data);
            if (format != null)
            {
                if (format.FormatType == GattPresentationFormatTypes.UInt32 && data.Length >= 4)
                {
                    return BitConverter.ToInt32(data, 0).ToString();
                }
                else if (format.FormatType == GattPresentationFormatTypes.Utf8)
                {
                    try
                    {
                        return Encoding.UTF8.GetString(data);
                    }
                    catch (ArgumentException)
                    {
                        return "(error: Invalid UTF-8 string)";
                    }
                }
                else
                {
                    // Add support for other format types as needed.
                    return "Unsupported format: " + CryptographicBuffer.EncodeToHexString(buffer);
                }
            }
            return "Unknown format";
        }


        public void DisconnectDevice()
        {
            if (this._bleDevice != null)
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
