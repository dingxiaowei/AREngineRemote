using System.Diagnostics;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace HuaweiAREngineRemote
{
    public class ADBExecutor
    {
        public static string adb
        {
            get
            {
                return Path.Combine(AndroidSdkRoot, "platform-tools/adb");
            }
        }
        
        /// <summary>
        /// adb configed in environment path
        /// </summary>
        private static string AdbExec(string command)
        {
            Process pro = new Process();
            pro.StartInfo.FileName = "sh";
            pro.StartInfo.UseShellExecute = false;
            pro.StartInfo.RedirectStandardError = true;
            pro.StartInfo.RedirectStandardInput = true;
            pro.StartInfo.RedirectStandardOutput = true;
            pro.StartInfo.CreateNoWindow = true;
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
            string devices_result = AdbExec("devices");
            string[] device_id_list = devices_result.Split('\n');
            if (device_id_list.Length <= 1)
            {
                Debug.LogError("No Devices Attached.");
                return null;
            }
            string str_device_id = device_id_list[1].Trim();
            var str_device_id_parts = str_device_id.Split('\t');
            if (str_device_id_parts[1].StartsWith("device"))
            {
                return str_device_id_parts[0];
            }
            return null;
        }

        public void AdbSingleDevicePortForward(string device_id)
        {
            var p1 = AREngineRemote.HOST_PORT;
            var p2 = AREngineRemote.ANDROID_PORT;
            if (string.IsNullOrEmpty(device_id.Trim()))
            {
                Debug.Log("adb forward tcp:35002 tcp:35001");
                AdbExec(" forward tcp:" + p1 + " tcp:" + p2);
            }
            else
            {
                Debug.Log("adb -s " + device_id + " forward tcp:" + p1 + " tcp:" + p2);
                AdbExec("-s " + device_id + " forward tcp:" + p1 + " tcp:" + p2);
            }
        }

        
        public static string AndroidSdkRoot
        {
            get { return EditorPrefs.GetString("AndroidSdkRoot"); }
            set { EditorPrefs.SetString("AndroidSdkRoot", value); }
        }

        public static string JdkRoot
        {
            get { return EditorPrefs.GetString("JdkPath"); }
            set { EditorPrefs.SetString("JdkPath", value); }
        }
        
        public static string AndroidNdkRoot
        {
            get { return EditorPrefs.GetString("AndroidNdkRoot"); }
            set { EditorPrefs.SetString("AndroidNdkRoot", value); }
        }

    }
}
#endif