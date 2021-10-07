using System.Collections.Generic;
using UnityEngine;

namespace HuaweiAREngineRemote
{
    public class ARHandVisualizer : BaseVisualizer<ARHands>
    {
        protected override TcpHead head
        {
            get { return TcpHead.Hand; }
        }

        private List<LineRenderer> m_boxLineRenderers = new List<LineRenderer>();

        protected override void OnProcess(byte[] recvBuf, ref int offset)
        {
            int cnt = Bytes2Int(recvBuf, ref offset);
            ar_data.hands = new ARHand[cnt];
            for (int i = 0; i < cnt; i++)
            {
                int len = Bytes2Int(recvBuf, ref offset);
                ar_data.hands[i] = new ARHand(len);
                for (int j = 0; j < len; j++)
                {
                    var v = RecvVector3(recvBuf, ref offset);
                    ar_data.hands[i].handBox[j] = v;
                }
            }
        }

        private LineRenderer AddLineRender()
        {
            var handBox = new GameObject("HandBox");
            handBox.transform.SetParent(transform);
            handBox.transform.localScale = Vector3.one;
            handBox.transform.localPosition = Vector3.zero;
            handBox.SetActive(false);
            var boxLineRenderer = handBox.AddComponent<LineRenderer>();
            boxLineRenderer.positionCount = 5;
            boxLineRenderer.startWidth = 0.03f;
            boxLineRenderer.endWidth = 0.03f;
            return boxLineRenderer;
        }

        protected override void OnUpdate()
        {
            while (ar_data.hands.Length > m_boxLineRenderers.Count)
            {
                var render = AddLineRender();
                m_boxLineRenderers.Add(render);
            }
            for (int i = 0; i < ar_data.hands.Length; i++)
            {
                UpdateBox(ar_data.hands[i], m_boxLineRenderers[i]);
            }
        }

        private void UpdateBox(ARHand hand, LineRenderer render)
        {
            Vector3 glLeftTopCorner = hand.handBox[0];
            Vector3 glRightBottomCorner = hand.handBox[1];
            Vector3 glLeftBottomCorner = new Vector3(glLeftTopCorner.x, glRightBottomCorner.y);
            Vector3 glRightTopCorner = new Vector3(glRightBottomCorner.x, glLeftTopCorner.y);
            render.SetPosition(0, _TransferGLCoord2UnityWoldCoordWithDepth(glLeftTopCorner));
            render.SetPosition(1, _TransferGLCoord2UnityWoldCoordWithDepth(glRightTopCorner));
            render.SetPosition(2, _TransferGLCoord2UnityWoldCoordWithDepth(glRightBottomCorner));
            render.SetPosition(3, _TransferGLCoord2UnityWoldCoordWithDepth(glLeftBottomCorner));
            render.SetPosition(4, _TransferGLCoord2UnityWoldCoordWithDepth(glLeftTopCorner));
            render.gameObject.SetActive(true);
        }

        private Vector3 _TransferGLCoord2UnityWoldCoordWithDepth(Vector3 glCoord)
        {
            Vector3 screenCoord = new Vector3 {x = (glCoord.x + 1) / 2, y = (glCoord.y + 1) / 2, z = 3,};
            return MainCamera.ViewportToWorldPoint(screenCoord);
        }
    }
}