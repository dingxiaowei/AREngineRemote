using HuaweiARUnitySDK;
using UnityEngine;

namespace HuaweiAREngineRemote
{
    public class PreviewStreamVisualizer : BaseVisualizer<AREngineImage>
    {
        public static volatile bool change;

        private Texture2D texY, texUV;
        private BackGroundRenderer render;

        protected override TcpHead head
        {
            get { return TcpHead.Preview; }
        }

        protected override void Init()
        {
            render = FindObjectOfType<BackGroundRenderer>();
        }
        
        protected override void OnUpdate()
        {
            if (texY == null)
            {
                int w = ar_data.width;
                int h = ar_data.height;
                texY = new Texture2D(w, h, TextureFormat.Alpha8, false);
                texUV = new Texture2D(w >> 1, h >> 1, TextureFormat.RG16, false);
            }
            texY.LoadRawTextureData(ar_data.y_buf);
            texUV.LoadRawTextureData(ar_data.uv_buf);
            texY.Apply();
            texUV.Apply();
            render.UpdateEditor(texY, texUV);
            
            var tf = MainCamera.transform;
            tf.position = ar_data.camPos;
            tf.eulerAngles = ar_data.camAngle;
        }
    }
}
