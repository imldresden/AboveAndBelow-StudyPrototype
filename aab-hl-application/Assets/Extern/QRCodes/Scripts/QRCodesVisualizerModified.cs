using System.Collections.Generic;
using UnityEngine;
using MasterNetworking.EventHandling;

namespace QRTracking
{
    public class QRCodesVisualizerModified : MonoBehaviour
    {
        public GameObject ContentRoot;
        public GameObject QRCodeGO;

        private bool locked = false;

        struct ActionData
        {
            public enum Type
            {
                Added,
                Updated,
                Removed
            };
            public Type type;
            public Microsoft.MixedReality.QR.QRCode qrCode;

            public ActionData(Type type, Microsoft.MixedReality.QR.QRCode qRCode) : this()
            {
                this.type = type;
                qrCode = qRCode;
            }
        }

        private System.Collections.Generic.Queue<ActionData> pendingActions = new Queue<ActionData>();
        void Awake()
        {

        }

        // Use this for initialization
        void Start()
        {
            Debug.Log("QRCodesVisualizer start");

            QRCodesManager.Instance.QRCodesTrackingStateChanged += Instance_QRCodesTrackingStateChanged;
            QRCodesManager.Instance.QRCodeAdded += Instance_QRCodeAdded;
            QRCodesManager.Instance.QRCodeUpdated += Instance_QRCodeUpdated;
            QRCodesManager.Instance.QRCodeRemoved += Instance_QRCodeRemoved;

            JsonRpcHandler.Instance.AddNotificationDelegate(rpcEvent: "ToggleQRLock", notification: _onNotification_ToggleQRLock);
        }
        private void Instance_QRCodesTrackingStateChanged(object sender, bool status)
        {
        }

        private void Instance_QRCodeAdded(object sender, QRCodeEventArgs<Microsoft.MixedReality.QR.QRCode> e)
        {
            Debug.Log("QRCodesVisualizer Instance_QRCodeAdded");

            lock (pendingActions)
            {
                pendingActions.Enqueue(new ActionData(ActionData.Type.Added, e.Data));
            }
        }

        private void Instance_QRCodeUpdated(object sender, QRCodeEventArgs<Microsoft.MixedReality.QR.QRCode> e)
        {
            Debug.Log("QRCodesVisualizer Instance_QRCodeUpdated");

            lock (pendingActions)
            {
                pendingActions.Enqueue(new ActionData(ActionData.Type.Updated, e.Data));
            }
        }

        private void Instance_QRCodeRemoved(object sender, QRCodeEventArgs<Microsoft.MixedReality.QR.QRCode> e)
        {
            Debug.Log("QRCodesVisualizer Instance_QRCodeRemoved");

            lock (pendingActions)
            {
                pendingActions.Enqueue(new ActionData(ActionData.Type.Removed, e.Data));
            }
        }

        private void _onNotification_ToggleQRLock(JsonRpcMessage message)
        {
            locked = !locked;
            QRCodeGO.SetActive(!locked);
            ContentRoot.GetComponent<SpatialGraphCoordinateSystemModified>().Locked = locked;

        }

        private void HandleEvents()
        {
            lock (pendingActions)
            {
                while (pendingActions.Count > 0)
                {
                    var action = pendingActions.Dequeue();
                    ContentRoot.GetComponent<SpatialGraphCoordinateSystemModified>().Id = action.qrCode.SpatialGraphNodeId;
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            HandleEvents();
        }
    }

}