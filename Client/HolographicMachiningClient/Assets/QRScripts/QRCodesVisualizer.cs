﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using FarmhandStuff;
using UnityEngine;

namespace Microsoft.MixedReality.SampleQRCodes
{
    public class QRCodesVisualizer : MonoBehaviour
    {
        public GameObject qrCodePrefab;
        [SerializeField] private FarmhandManager client;
        public SortedDictionary<System.Guid, GameObject> qrCodesObjectsList;
        private bool clearExisting = false;
        private DateTimeOffset startTime;

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

        private Queue<ActionData> pendingActions = new Queue<ActionData>();

        // Use this for initialization
        void Start()
        {
            startTime = DateTimeOffset.Now;
            Debug.Log("QRCodesVisualizer start");
            qrCodesObjectsList = new SortedDictionary<System.Guid, GameObject>();

            QRCodesManager.Instance.QRCodesTrackingStateChanged += Instance_QRCodesTrackingStateChanged;
            QRCodesManager.Instance.QRCodeAdded += Instance_QRCodeAdded;
            QRCodesManager.Instance.QRCodeUpdated += Instance_QRCodeUpdated;
            QRCodesManager.Instance.QRCodeRemoved += Instance_QRCodeRemoved;
            if (qrCodePrefab == null)
            {
                throw new System.Exception("Prefab not assigned");
            }
        }
        private void Instance_QRCodesTrackingStateChanged(object sender, bool status)
        {
            //Debug.Log("Update: Status = " + status.ToString());
            if (!status)
            {
                clearExisting = true;
            }
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
            //Debug.Log("QRCodesVisualizer Instance_QRCodeUpdated");

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

        private void HandleEvents()
        {
            lock (pendingActions)
            {
                while (pendingActions.Count > 0)
                {
                    
                    var action = pendingActions.Dequeue();
                    //Debug.Log("Pending Action: " + action.type.ToString());
                    if (action.type == ActionData.Type.Added && action.qrCode.LastDetectedTime >= startTime)
                    {
                        //var index = client.DiscoveredDevices.FindIndex((s => action.qrCode.Data == s));
                        // if (index != -1)
                        // {
                            //Instantiate an object which can: a) figure out whether to show status screen or login button and b) has option to turn QR code following on or off (in case its too shaky).
                        GameObject qrCodeObject = Instantiate(qrCodePrefab, new Vector3(0, 0, 0), Quaternion.identity);
                        qrCodeObject.GetComponent<SpatialGraphNodeTracker>().Id = action.qrCode.SpatialGraphNodeId;
                        PrinterAnchor printerAnchor = qrCodeObject.GetComponent<PrinterAnchor>();
                        printerAnchor.farmhand_client = client;
                        printerAnchor.qr_code = action.qrCode;

                        client.DiscoverMachine(action.qrCode.Data, printerAnchor);
                        qrCodesObjectsList.Add(action.qrCode.Id, qrCodeObject);
                        //}

                        // else
                        // {
                        //     Debug.Log("No device matching QRCode data=" + action.qrCode.Data + "!");
                        // }

                    }
                    else if (action.type == ActionData.Type.Updated)
                    {
                        //var index = client.DiscoveredDevices.FindIndex((s => action.qrCode.Data == s));
                        if (!qrCodesObjectsList.ContainsKey(action.qrCode.Id))
                        {
                            Debug.Log("Updating qr code not in list?");
                            GameObject qrCodeObject = Instantiate(qrCodePrefab, new Vector3(0, 0, 0), Quaternion.identity);
                            qrCodeObject.GetComponent<SpatialGraphNodeTracker>().Id = action.qrCode.SpatialGraphNodeId;
                            PrinterAnchor printerAnchor = qrCodeObject.GetComponent<PrinterAnchor>();
                            printerAnchor.farmhand_client = client;
                            printerAnchor.qr_code = action.qrCode;

                            client.DiscoverMachine(action.qrCode.Data, printerAnchor);
                            qrCodesObjectsList.Add(action.qrCode.Id, qrCodeObject);
                        }
                    }
                    else if (action.type == ActionData.Type.Removed)
                    {
                        if (qrCodesObjectsList.ContainsKey(action.qrCode.Id))
                        {
                            Destroy(qrCodesObjectsList[action.qrCode.Id]);
                            qrCodesObjectsList.Remove(action.qrCode.Id);
                        }
                    }
                }
            }
            if (clearExisting)
            {
                clearExisting = false;
                foreach (var obj in qrCodesObjectsList)
                {
                    Destroy(obj.Value);
                }
                qrCodesObjectsList.Clear();

            }
        }

        // Update is called once per frame
        void Update()
        {
            HandleEvents();
        }
    }
}
