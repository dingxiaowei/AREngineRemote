using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace DefaultNamespace
{
    public class ADBExecutor
    {
        public const int ANDROID_PORT = 30000;
        public const int HOST_PORT = 35000;
        public const string adb = @"~/Documents/software/eclipse/android-sdks/platform-tools/adb";

        private static string AdbExec(string command)
        {
            Process pro = new Process();
            pro.StartInfo.FileName = "sh";
            pro.StartInfo.UseShellExecute = false;
            pro.StartInfo.RedirectStandardError = true;
            pro.StartInfo.RedirectStandardInput = true;
            pro.StartInfo.RedirectStandardOutput = true;
            pro.StartInfo.CreateNoWindow = true;
            //pro.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            pro.Start();
            pro.StandardInput.WriteLine(adb + " " + command);
            pro.StandardInput.WriteLine("exit");
            pro.StandardInput.AutoFlush = true;
            string output = pro.StandardOutput.ReadToEnd();
            pro.WaitForExit();
            pro.Close();
            return output;
        }

        public string AdbDevice()
        {
            Debug.Log("adb devices");
            string devices_result = AdbExec("devices");
            string[] device_id_list = devices_result.Split('\n');
            if (device_id_list.Length <= 1)
            {
                Debug.LogError("No Devices Attached.");
                return null;
            }
            string str_device_id = device_id_list[1];
            Debug.Log(str_device_id);
            var str_device_id_parts = str_device_id.Split('\t');
            if (str_device_id_parts[1].Equals("device"))
            {
                var device = str_device_id_parts[0];
                Debug.Log(device);
                return device;
            }
            return null;
        }
        
        public void AdbSingleDevicePortForward(String device_id)
        {
            Debug.Log("adb -s " + device_id + " forward tcp:" + HOST_PORT + " tcp:" + ANDROID_PORT);
            AdbExec("-s " + device_id + " forward tcp:" + HOST_PORT + " tcp:" + ANDROID_PORT);
        }
    }
}