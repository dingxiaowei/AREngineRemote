using UnityEngine;

namespace HuaweiAREngineRemote
{
    public class PlaneVisualizer : BaseVisualizer<AREnginePlane>
    {

        public static volatile bool change;

        protected override TcpHead head
        {
            get { return TcpHead.Plane; }
        }

        protected override void OnUpdate()
        {
            int len = ar_data.planes.Length;
//            Debug.Log("plane length: " + len);
            for (int i = 0; i < len; i++)
            {
                // var p = ar_plane.planes[i];
            }
        }
    }
}