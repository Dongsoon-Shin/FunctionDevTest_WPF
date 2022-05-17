using NModbus;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace FunctionDevTest_WPF.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _Device1;
        public string Device1
        {
            get { return _Device1; }
            set { SetProperty(ref _Device1, value); }
        }
        private string _Device2;
        public string Device2
        {
            get { return _Device2; }
            set { SetProperty(ref _Device2, value); }
        }
        private string _Device3;
        public string Device3
        {
            get { return _Device3; }
            set { SetProperty(ref _Device3, value); }
        }

        private DelegateCommand<string> _runCommand;
        public DelegateCommand<string> DeviceRun =>
            _runCommand ?? (_runCommand = new DelegateCommand<string>(ExecuteRunCommand));
        private DelegateCommand<string> _stopCommand;
        public DelegateCommand<string> DeviceStop =>
            _stopCommand ?? (_stopCommand = new DelegateCommand<string>(ExecuteDeviceStop));

        bool isContinue = true;

        void ExecuteDeviceStop(string parameter)
        {
            isContinue = false;
        }

        void ExecuteRunCommand(string parameter)
        {
            if (string.IsNullOrEmpty(parameter)) throw new ArgumentNullException();
            runDevice(parameter);
        }

        private void runDevice(string parameter)
        {
            isContinue = true;
            Task.Run(() =>
            {
                while (isContinue)
                {
                    WrappingFunction(parameter);
                    Thread.Sleep(1000);
                }
            });
        }

        public MainWindowViewModel()
        {

        }

        private async void WrappingFunction(string parameter)
        {
            var r = await GetHoldingRegisterData(parameter);
            // r[0] == first register
            // parameter == port number
            switch (parameter)
            {
                case "5000":
                    Device1 = r[0].ToString();
                    break;
                case "5001":
                    Device2 = r[0].ToString();
                    break;
                case "5002":
                    Device3 = r[0].ToString();
                    break;
                default:
                    break;
            }
        }

        public async Task<ushort[]> GetHoldingRegisterData(string parameter)
        {
            using TcpClient client = new TcpClient("127.0.0.1", int.Parse(parameter));
            var factory = new ModbusFactory();
            IModbusMaster master = factory.CreateMaster(client);

            ushort startAddress = 0;
            ushort numInputs = 1;

            var data = await master.ReadHoldingRegistersAsync(0, startAddress, numInputs);

            return data;
        }
    }
}
