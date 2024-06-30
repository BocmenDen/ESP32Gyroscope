using Android.Bluetooth;
using ESP32GyroscopeApp.Platforms.Android.Bluetooth;
using System.Globalization;
using System.Numerics;
using static ESP32GyroscopeApp.Platforms.Android.Bluetooth.BluetoothConnector;

namespace ESP32GyroscopeApp
{
    public partial class MainPage : ContentPage
    {
        private const string PartNameDevice = "Gyroscope";
        private const float MaxAngle = 90;
        private const int SizePoint = 15;
        private readonly BluetoothConnector _connector;
        private DeviceHandler? _deviceModel;
        private readonly List<char> _bufferPart = new(20);

        public MainPage()
        {
            InitializeComponent();
            _connector = new BluetoothConnector();
        }

        private void RefreshButtonClicked(object? sender, EventArgs? e)
            => viewsDevices.ItemsSource = _connector.GetConnectedDevices().Where(x => x.Name.Contains(PartNameDevice)).ToList();

        private void DeviceSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem is ModelDevice modelDevice)
            {
                OpenSocet(modelDevice);
                viewsDevices.SelectedItem = null;
                popupSelectDevice.IsVisible = false;
            }
        }

        private void OpenSocet(ModelDevice modelDevice)
        {
            var socet = _connector.Connect(modelDevice.Address);
            if (socet == null) return;
            _deviceModel?.Close();
            _deviceModel = new DeviceHandler(socet, AppendPackagePart, (message) =>
            {
                _ = _deviceModel?.Close();
                _deviceModel = null;

                Dispatcher.Dispatch(() =>
                {
                    popupSelectDevice.IsVisible = true;
                    _ = DisplayAlert("Проблема", message ?? "Похоже что соединение было разорванно", "ОК");
                });
            });
            _deviceModel.Start();
            popupSelectDevice.IsVisible = false;
        }

        private void AppendPackagePart(byte value)
        {
            if (value == '\n')
            {
                string line = string.Join(string.Empty, _bufferPart);
                var parts = line.Split(';');
                if (parts.Length == 3 &&
                    float.TryParse(parts[0], CultureInfo.InvariantCulture, out float x) &&
                    float.TryParse(parts[1], CultureInfo.InvariantCulture, out float y) &&
                    float.TryParse(parts[2], CultureInfo.InvariantCulture, out float z))
                {
                    ApplayPackageData(x, y, z);
                }
                return;
            }
            if (value == 'S') { _bufferPart.Clear(); return; }
            _bufferPart.Add((char)value);
        }

        private void ApplayPackageData(float x, float y, float z)
        {
            Dispatcher.DispatchAsync(() =>
            {
                layoutViewGyroscope.Padding = SizePoint / 2;
                double size = Math.Min(parentViewGyroscope.Width, parentViewGyroscope.Height);
                double center = size / 2 - SizePoint;
                double scale = (center + SizePoint / 2) / MaxAngle;

                var angle = Math.Min(Math.Min(Math.Abs(x), MaxAngle) + Math.Min(Math.Abs(y), MaxAngle), MaxAngle);

                Vector2 pointPos = Vector2.Normalize(new Vector2(x, y)) * angle;

                x = float.IsNaN(pointPos.X) ? 0 : pointPos.X;
                y = float.IsNaN(pointPos.Y) ? 0 : pointPos.Y;

                layoutViewGyroscope.WidthRequest = size;
                layoutViewGyroscope.HeightRequest = size;
                layoutViewGyroscope.Margin = SizePoint;

                double angleToOffset(double angle) => angle * scale + center;

                AbsoluteLayout.SetLayoutBounds(pointGyroscope, new Rect(angleToOffset(x), angleToOffset(y), SizePoint, SizePoint));
                testTest.Text = $"{angle:f1}°";
            });
        }

        private void ResetClicked(object sender, EventArgs e) => _deviceModel?.Send(1);

        private async void CalibrationClicked(object sender, EventArgs e)
        {
            if (_deviceModel == null) return;
            const string Yes = "Да";
            var result = await DisplayActionSheet("Вы уверены? Возможно придётся заново подключаться", "Нет", Yes);
            if (result == Yes) _deviceModel.Send(2);
        }

        private void SwitchDeviceClicked(object sender, EventArgs e) => popupSelectDevice.IsVisible = true;

        private void popupSelectDevice_Loaded(object sender, EventArgs e) => RefreshButtonClicked(null, null);
    }

    public class DeviceHandler
    {
        private readonly BluetoothSocket _socet;
        private readonly CancellationTokenSource _readerToken = new CancellationTokenSource();
        private readonly byte[] _data = new byte[32];
        private readonly System.Timers.Timer _timer = new System.Timers.Timer()
        {
            Interval = 1000
        };
        private Task? _task;
        private Action<byte>? _dataReaded;
        private Action<string>? _error;

        public DeviceHandler(BluetoothSocket socet, Action<byte>? dataReaded, Action<string> error)
        {
            _socet=socet;
            _dataReaded=dataReaded;
            _error=error;
            _timer.Elapsed += Elapsed;
        }

        private void Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            _error?.Invoke("Вышло время ожидания данных");
        }

        public void Start()
        {
            _task = Task.Run(async () =>
            {
                try
                {
                    await _socet.ConnectAsync();
                    while (!_readerToken.IsCancellationRequested)
                    {
                        try
                        {
                            _timer.Start();
                            var countBytes = await _socet.InputStream!.ReadAtLeastAsync(_data, 1, false, _readerToken.Token);
                            _timer.Stop();
                            for (int i = 0; i < countBytes; i++)
                                _dataReaded?.Invoke(_data[i]);
                        }
                        catch (Exception e)
                        {
                            _error?.Invoke($"Произошла ошибка: {e.Message}");
                        }
                    }
                }
                catch
                {
                    _error?.Invoke("Не удалось подключиться");
                }
            });
        }

        public void Send(byte data)
        {
            if (_task == null) return;
            _socet.OutputStream?.WriteByte(data);
        }

        public async Task Close()
        {
            _dataReaded = null;
            _error = null;
            _readerToken.Cancel();
            if(_task != null) await _task;
            _socet.Close();
            _socet.Dispose();
            _readerToken.Dispose();
            _timer.Stop();
            _timer.Dispose();
        }
    }
}
