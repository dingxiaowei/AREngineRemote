using System.Collections.Generic;
using HuaweiARUnitySDK;
using UnityEngine;

namespace HuaweiAREngineRemote
{
    public class ARHandVisualizer : BaseVisualizer<ARHands>
    {
        protected override TcpHead head
        {
            get { return TcpHead.Hand; }
        }

        private GameObject m_handBox;
        private List<LineRenderer> m_boxLineRenderers = new List<LineRenderer>();

        protected override void OnProcess(byte[] recvBuf, ref int offset)
        {
            int cnt = Bytes2Int(recvBuf, ref offset);
            ar_data.hands = new ARHand[cnt];
            for (int i = 0; i < cnt; i++)
            {
                int gestT = Bytes2Int(recvBuf, ref offset);
                int handT = Bytes2Int(recvBuf, ref offset);
                int gestS = Bytes2Int(recvBuf, ref offset);
                int skeleS = Bytes2Int(recvBuf, ref offset);
                int len = Bytes2Int(recvBuf, ref offset);
                ar_data.hands[i] = new ARHand(len);
                ar_data.hands[i].gestureType = gestT;
                ar_data.hands[i].handType = (HuaweiARUnitySDK.ARHand.HandType)handT;
                ar_data.hands[i].coordSystem = (ARCoordinateSystemType)gestS;
                ar_data.hands[i].skeletonSystem =(ARCoordinateSystemType) skeleS;
                for (int j = 0; j < len; j++)
                {
                    var v = RecvVector3(recvBuf, ref offset);
                    ar_data.hands[i].handBox[j] = v;
                }
            }
        }

        public void OnGUI()
        {
            GUIStyle bb = new GUIStyle();
            bb.normal.background = null;
            bb.normal.textColor = new Color(1, 0, 0);
            bb.fontSize = 30;
            if (ar_data?.hands?.Length > 0)
            {
                var hand = ar_data.hands[0];
                GUI.Label(new Rect(0, 0, 200, 200),
                    string.Format("GuestureType:{0}\n HandType:{1}\n" + " GuestureCoord:{2}\n SkeletonCoord:{3}",
                        hand.gestureType, hand.handType, hand.coordSystem,
                        hand.skeletonSystem), bb);
            }
        }

        private LineRenderer AddLineRender()
        {
            m_handBox = new GameObject("HandBox");
            m_handBox.transform.SetParent(transform);
            m_handBox.transform.localScale = Vector3.one;
            m_handBox.transform.localPosition = Vector3.zero;
            m_handBox.SetActive(false);
            var boxLineRenderer = m_handBox.AddComponent<LineRenderer>();
            boxLineRenderer.positionCount = 5;
            boxLineRenderer.startWidth = 0.03f;
            boxLineRenderer.endWidth = 0.03f;
            return boxLineRenderer;
        }

        protected override void OnUpdateVisual()
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
            render.SetPosition(0, TransferGLCoord2UnityWoldCoordWithDepth(glLeftTopCorner));
            render.SetPosition(1, TransferGLCoord2UnityWoldCoordWithDepth(glRightTopCorner));
            render.SetPosition(2, TransferGLCoord2UnityWoldCoordWithDepth(glRightBottomCorner));
            render.SetPosition(3, TransferGLCoord2UnityWoldCoordWithDepth(glLeftBottomCorner));
            render.SetPosition(4, TransferGLCoord2UnityWoldCoordWithDepth(glLeftTopCorner));
            render.gameObject.SetActive(true);
        }

        private Vector3 TransferGLCoord2UnityWoldCoordWithDepth(Vector3 glCoord)
        {
            Vector3 screenCoord = new Vector3((glCoord.x + 1) / 2, (glCoord.y + 1) / 2, 3);
            return MainCamera.ViewportToWorldPoint(screenCoord);
        }
    }
}