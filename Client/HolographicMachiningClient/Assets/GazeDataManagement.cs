using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;
using UnityEngine.InputSystem;

public class GazeDataManagement : MonoBehaviour
{
    //private EyeTrackingWebSocket _webSocketClient;  // Add WebSocket client
    private Camera _cam;
    public InputActionAsset eyeActionAsset;
    private InputActionMap _eyeActionMap;
    private Dictionary<string, InputAction> _eyeVectorActions;
    private Dictionary<string, InputAction> _eyeRotationActions;

    private FuzzyGazeInteractor _fuzzyGazeInteractor;

    private EyeData _prevTime;

    //private Vector3 _headPos;
    //private Vector3 _headDir;

    void Start()
    {
        _cam = Camera.main;
        //_webSocketClient = FindObjectOfType<EyeTrackingWebSocket>(); // Initialize WebSocket client
        //if (_webSocketClient != null)
        //{
        //    Debug.Log("Websocket Client found!");
        //}
        //else
        //{
        //    Debug.Log("Websocket Client not found!");
        //}


        //_headPos = _cam.transform.position; // Initialize head position
        //_headDir = _cam.transform.forward;  // Initialize head direction


        _fuzzyGazeInteractor = FindObjectOfType<FuzzyGazeInteractor>(); // Grab gaze interactor
        if (_fuzzyGazeInteractor != null)
        {
            Debug.Log("Fuzzy Gaze Interactor found!");
        }
        else
        {
            Debug.Log("No Fuzzy Gaze Interactor found!");
        }

        _eyeActionMap = eyeActionAsset.FindActionMap("EyeActions", true);
        _eyeActionMap.Enable();

        _eyeVectorActions = new Dictionary<string, InputAction>()
        {
            {"position",_eyeActionMap.FindAction("pose/Position")},
            {"velocity",_eyeActionMap.FindAction("pose/Velocity")},
            {"rightEyePosition",_eyeActionMap.FindAction("RightEyePosition")},
            {"leftEyePosition",_eyeActionMap.FindAction("LeftEyePosition")},
            {"centerEyePosition",_eyeActionMap.FindAction("CenterEyePosition")},
        };

        _eyeRotationActions = new Dictionary<string, InputAction>()
        {
            { "centerEyeRotation",_eyeActionMap.FindAction("CenterEyeRotation") },
            { "leftEyeRotation",_eyeActionMap.FindAction("LeftEyeRotation") },
            { "rightEyeRotation",_eyeActionMap.FindAction("RightEyeRotation") },
        };

        _prevTime = new EyeData(Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Quaternion.identity, Quaternion.identity, Quaternion.identity, Time.time, _fuzzyGazeInteractor, _cam.transform);
    }

    void Update()
    {

        if (_fuzzyGazeInteractor.rayEndTransform != null)
        {
            var currentData = new EyeData(Time.time, _eyeVectorActions, _eyeRotationActions, _fuzzyGazeInteractor, _cam.transform, _prevTime);
            // Convert eye data to JSON and send through WebSocket
            string jsonData = currentData.ToJson();
            //_webSocketClient.SendEyeTrackingData(jsonData);  // Send data to middleman server

            Debug.Log(jsonData);
        }
        else
        {
            Debug.Log($"Not Looking at anything? {_fuzzyGazeInteractor.coneCastAngle}");
        }



    }

    /*private bool ChangePosition(Vector3 contender)
    {
        var difference = contender - _headPos;
        if (difference.magnitude > 0.5f)
        {
            _headPos = contender;
            return true;
        }
        return false;
    }*/
}



