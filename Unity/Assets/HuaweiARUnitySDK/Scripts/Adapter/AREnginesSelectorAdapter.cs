namespace HuaweiARInternal
{
    using System;
    using System.Runtime.InteropServices;
    using HuaweiARUnitySDK;

    internal class AREnginesSelectorAdapter
    {

        public AREnginesAvaliblity CheckDeviceExecuteAbility()
        {
#if UNITY_EDITOR
            return AREnginesAvaliblity.NONE_SUPPORTED;
#endif
            return NDKAPI.HwArEnginesSelector_checkAllAvailableEngines(ARUnityHelper.Instance.GetJEnv(),
                ARUnityHelper.Instance.GetActivityHandle());
        }

        public AREnginesType SetAREngine(AREnginesType executor)
        {
            return NDKAPI.HwArEnginesSelector_setAREngine(executor);
        }
        public AREnginesType GetCreatedEngine()
        {
            return NDKAPI.HwArEnginesSelector_getCreatedEngine();
        }


        private struct NDKAPI
        {
            [DllImport(AdapterConstants.HuaweiARNativeApi)]
            public static extern AREnginesAvaliblity HwArEnginesSelector_checkAllAvailableEngines(IntPtr envHandle, IntPtr applicationContextHandle);
            [DllImport(AdapterConstants.HuaweiARNativeApi)]
            public static extern AREnginesType HwArEnginesSelector_setAREngine(AREnginesType executerType);
            [DllImport(AdapterConstants.HuaweiARNativeApi)]
            public static extern AREnginesType HwArEnginesSelector_getCreatedEngine();
        }
    }
}
