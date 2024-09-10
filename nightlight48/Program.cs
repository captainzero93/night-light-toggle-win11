using System;
using Microsoft.Win32;

namespace NightLightController
{
    public class NightLight : IDisposable
    {
        private string _key =
            "Software\\Microsoft\\Windows\\CurrentVersion\\CloudStore\\Store\\DefaultAccount\\Current\\default$windows.data.bluelightreduction.bluelightreductionstate\\windows.data.bluelightreduction.bluelightreductionstate";

        private RegistryKey _registryKey;

        public bool Enabled
        {
            get
            {
                if (!Supported) return false;
                var data = _registryKey?.GetValue("Data") as byte[];
                if (data == null) return false;
                return data[18] == 0x15;
            }
            set
            {
                if (Supported && Enabled != value) Toggle();
            }
        }

        public bool Supported => _registryKey != null;

        public NightLight()
        {
            _registryKey = Registry.CurrentUser.OpenSubKey(_key, true);
        }

        private void Toggle()
        {
            var data = _registryKey.GetValue("Data") as byte[];
            var newData = new byte[43];

            if (Enabled)
            {
                newData = new byte[41];
                Array.Copy(data, 0, newData, 0, 22);
                Array.Copy(data, 25, newData, 23, 43 - 25);
                newData[18] = 0x13;
            }
            else
            {
                Array.Copy(data, 0, newData, 0, 22);
                Array.Copy(data, 23, newData, 25, 41 - 23);
                newData[18] = 0x15;
                newData[23] = 0x10;
                newData[24] = 0x00;
            }

            for (int i = 10; i < 15; i++)
            {
                if (newData[i] != 0xff)
                {
                    newData[i]++;
                    break;
                }
            }

            _registryKey.SetValue("Data", newData, RegistryValueKind.Binary);
            _registryKey.Flush();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _registryKey?.Close();
                _registryKey = null;
            }
        }

        ~NightLight()
        {
            Dispose(false);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            using (var nightLight = new NightLight())
            {
                if (nightLight.Supported)
                {
                    Console.WriteLine("Night Light is supported on this system.");

                    // Toggle the Night Light
                    nightLight.Enabled = !nightLight.Enabled;

                    if (nightLight.Enabled)
                        Console.WriteLine("Night Light turned ON.");
                    else
                        Console.WriteLine("Night Light turned OFF.");
                }
                else
                {
                    Console.WriteLine("Night Light is not supported on this system.");
                }
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
