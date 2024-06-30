using Android.Bluetooth;
using Java.Util;

namespace ESP32GyroscopeApp.Platforms.Android.Bluetooth
{
    public class BluetoothConnector
    {
        /// <inheritdoc />
        public List<ModelDevice> GetConnectedDevices()
        {
            _adapter = BluetoothAdapter.DefaultAdapter;

            if (_adapter == null)
                throw new Exception("No Bluetooth adapter found.");

            if (_adapter.IsEnabled)
            {
                if (_adapter.BondedDevices?.Count > 0)
                {
                    return _adapter.BondedDevices.Select(d => new ModelDevice(d.Name, d.Address)).ToList();
                }
            }
            else
            {
                Console.Write("Bluetooth is not enabled on device");
            }
            return new List<ModelDevice>();
        }

        /// <inheritdoc />
        public BluetoothSocket? Connect(string address)
        {
            var device = _adapter.BondedDevices?.FirstOrDefault(d => d.Address == address);
            var _socket = device?.CreateRfcommSocketToServiceRecord(UUID.FromString(SspUdid));
            return _socket;

            //_socket.Connect();
            //var buffer = new byte[] { 49, 49, 49, 49, 49 };

            //// Write data to the device to trigger LED
            //_socket.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// The standard UDID for SSP
        /// </summary>
        private const string SspUdid = "00001101-0000-1000-8000-00805f9b34fb";
        private BluetoothAdapter? _adapter;


        public record class ModelDevice(string Name, string Address);
    }
}
