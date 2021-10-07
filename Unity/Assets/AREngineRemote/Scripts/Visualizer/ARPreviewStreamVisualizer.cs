using System;
using HuaweiARUnitySDK;
using UnityEngine;

namespace HuaweiAREngineRemote
{
    public class PreviewStreamVisualizer : BaseVisualizer<AREngineImage>
    {
        private Texture2D texY, texUV;
        private BackGroundRenderer render;
        private readonly Vector4 front_uv = new Vector4(0, 1, 1, -1);
        private readonly Vector4 back_uv = new Vector4(1, 1, -1, -1);
        private static readonly int UVSt = Shader.PropertyToID("uv_st");

        protected override TcpHead head
        {
            get { return TcpHead.Preview; }
        }

        protected override void OnInitial()
        {
            render = FindObjectOfType<BackGroundRenderer>();
            var uv_st = sceneState == SceneState.Face ?
                front_uv :
                back_uv;
            render.BackGroundMaterial.SetVector(UVSt, uv_st);
        }

        protected override void OnProcess(byte[] recvBuf, ref int offset)
        {
            ar_data.camPos = RecvVector3(recvBuf, ref offset);
            ar_data.camAngle = RecvVector3(recvBuf, ref offset);
            var width = Bytes2Int(recvBuf, ref offset);
            var heigth = Bytes2Int(recvBuf, ref offset);
            ar_data.Set(width, heigth);
            var len = Bytes2Int(recvBuf, ref offset);
            var y_len = len * 2 / 3;
            var uv_len = len / 3;
            if (ar_data.y_buf == null)
            {
                ar_data.y_buf = new byte[y_len];
                ar_data.uv_buf = new byte[uv_len];
            }
            Array.Copy(recvBuf, offset, ar_data.y_buf, 0, y_len);
            Array.Copy(recvBuf, offset + y_len, ar_data.uv_buf, 0, uv_len);
        }

        protected override void OnUpdateVisual()
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
