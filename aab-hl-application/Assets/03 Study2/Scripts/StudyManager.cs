using MasterNetworking.EventHandling;
using MasterNetworking.WebSocket;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PauseScreenManager;
using static TrialManager;

#if !UNITY_EDITOR
using Windows.Storage;
#endif

public class StudyManager : MonoBehaviour
{    
    [SerializeField]
    private int _participantNumber = 1;
    [SerializeField]
    private TextAsset _randomList;
    [SerializeField]
    private string _loggingPathForValues;
    [SerializeField]
    private TrialManager _trialManager;
    [SerializeField]
    private PauseScreenManager _pauseManager;


    private bool _studyStarted = false;
    private List<string> _partOrder;
    private List<int> _partOrderInt = new List<int>() { 1, 2, 3 };
    private List<List<string>> _allVariables = new List<List<string>>();

    private List<string> _currentVariables = new List<string>();
    private string _currentState = "";
    private int _currentPart = 0;
    private int _currentTrial = 0;
    private InfoToLog _currentTrialLogInfo;

    public struct InfoToLog
    {
        public int part;
        public int trial;
        public LocationMarker.LocationName location;
        public ContentType contentType;
        public float size;
        public float distance;
        public float tilt;
    }

    #region Unity Lyfecycle
    // Start is called before the first frame update
    void Start()
    {
        JsonRpcHandler.Instance.AddNotificationDelegate(rpcEvent: "StartExperiment", notification: _onNotification_StartExperiment);
        JsonRpcHandler.Instance.AddNotificationDelegate(rpcEvent: "FinishTrial", notification: _onNotification_FinishTrials);
        JsonRpcHandler.Instance.AddNotificationDelegate(rpcEvent: "NextState", notification: _onNotification_NextState);
    }

    // Update is called once per frame
    void Update()
    {

    }
    #endregion

    /// <summary>
    /// Reads the file for the study order
    /// </summary>
    [ContextMenu("ReadFile")]
    private void ReadFile()
    {
        List<string> lines = new List<string>(_randomList.text.Split("\n"[0]));
        string line = lines[_participantNumber - 1];
        List<string> split1 = new List<string>(line.Split('|'));
        string[] split2left = split1[0].Split(',');
        List<string> partOrder = new List<string>()
        {
            "Part"+split2left[0],
            "Part"+split2left[1],
            "Part"+split2left[2]
        };
        _partOrder = partOrder;
        _partOrderInt[0] = Int32.Parse(split2left[0]);
        _partOrderInt[1] = Int32.Parse(split2left[1]);
        _partOrderInt[2] = Int32.Parse(split2left[2]);

        List<string> partVariables = new List<string>(split1[1].Split('+'));
        List<List<string>> variables = new List<List<string>>();

        variables.Add(new List<string>(partVariables[0].Split(',')));
        variables.Add(new List<string>(partVariables[1].Split(',')));
        variables.Add(new List<string>(partVariables[2].Split(',')));
        _allVariables = variables;
    }


    #region JSON RPC Notifications
    private void _onNotification_StartExperiment(JsonRpcMessage message)
    {
        //if (_studyStarted)
        //    return;
        _participantNumber = message.Data["subjectId"].ToObject<int>();
        ReadFile();
        _studyStarted = true;
        //_logging.SetLoggingPathAndInitFile(_loggingPathForValues+"Participant_"+_participantNumber);

        // #TODO: Do we need to do something specific here?

        JsonRpcHandler.Instance.SendNotification(
            message: new JsonRpcMessage(
                method: "StateChange",
                data: new JObject(
                    new JProperty("newState", "Training1")
                )
            )
        );

        _currentState = "Training1";
        _pauseManager.StopQRScreen();
        StartTraining1();
    }
    private void _onNotification_FinishTrials(JsonRpcMessage message)
    {
        //update the information
        _trialManager.UpdateCurrentConfig();

        if (_currentState == "Training1" || _currentState == "Training2" || _currentState == "StartScreen" || _currentState == "BreakScreen1" || _currentState == "BreakScreen2")
        {
            _onNotification_NextState(null);
            return;
        }
        if (_currentTrial + 1 >= _currentVariables.Count)
        {
            Debug.Log("EndOfTrialsinThisPart");
            _onNotification_NextState(null);
        }

        JsonRpcHandler.Instance.SendNotification(
            message: new JsonRpcMessage(
                method: "LogTrialData",
                data: new JObject(
                    new JProperty("part", _currentPart),
                    new JProperty("trial", _currentTrial),
                    new JProperty("area", _trialManager.CurrentConfig.location),
                    new JProperty("content", _trialManager.CurrentConfig.contentType),
                    new JProperty("size", _trialManager.CurrentConfig.size),
                    new JProperty("distance", _trialManager.CurrentConfig.distance),
                    new JProperty("tilt", _trialManager.CurrentConfig.tilt)
                )
            ),
            ccType: ClientConnectionType.Server
        );

        StartTrialByIndex(_currentTrial + 1);

    }

    private void _onNotification_NextState(JsonRpcMessage message)
    {
        // #MethodDescription
        // 1. Check if a next State is available, if not, do nothing
        // 2. Load new content
        // 3. Send "StateChange" to the experimenter client
        // 3. Send "NextTrial" to all client devices.

        string nextState = "";
        // "StateMachine"
        if      (_currentState == "Training1")      nextState = "Training2";
        else if (_currentState == "Training2")      nextState = "StartScreen";
        else if (_currentState == "StartScreen")    nextState = _partOrder[0];
        else if (_currentState == _partOrder[0])    nextState = "BreakScreen1";
        else if (_currentState == "BreakScreen1")   nextState = _partOrder[1];
        else if (_currentState == _partOrder[1])    nextState = "BreakScreen2";
        else if (_currentState == "BreakScreen2")   nextState = _partOrder[2];
        else if (_currentState == _partOrder[2])    nextState = "EndScreen";

        // If no new state was found, cancel the next actions.
        if (nextState == "")
            return;

        if (nextState == "StartScreen" || nextState == "BreakScreen1" || nextState == "BreakScreen2" || nextState == "EndScreen")
        {
            _pauseManager.StartPause(nextState);
            JsonRpcHandler.Instance.SendNotification(
                message: new JsonRpcMessage(
                    method: "ToggleBlockUserInput",
                    data: new JObject()
                )
            );
        }
        //if state is a type of pause, end that pause
        if (_currentState == "StartScreen" || _currentState == "BreakScreen1" || _currentState == "BreakScreen2")
        {
            _pauseManager.StopPause();
            JsonRpcHandler.Instance.SendNotification(
                message: new JsonRpcMessage(
                    method: "ToggleBlockUserInput",
                    data: new JObject()
                )
            );
        }

        JsonRpcHandler.Instance.SendNotification(
            message: new JsonRpcMessage(
                method: "StateChange",
                data: new JObject(
                    new JProperty("newState", nextState)
                )
            )
        );

        _currentState = nextState;
        if (_currentState == "Training1")
            StartTraining1();
        if (_currentState == "Training2")
            StartTraining2();
        if (_partOrder.Contains(_currentState))
            StartPartByIndex(_partOrder.IndexOf(_currentState));

    }
    #endregion

    #region StartDifferentStatesOrTrials

    [ContextMenu("Test Break1")]
    public void TestPause()
    {
        StartPause("BreakScreen1");
    }
    private void StartPause(string pauseType)
    {
        _pauseManager.StartPause(pauseType);
    }

    [ContextMenu("StartTraining1")]
    private void StartTraining1()
    {
        LocationMarker.LocationName location = LocationMarker.LocationName.Ceiling;//hardcoded traininglocation
        _trialManager.SetupTraining(0, location);

        //send notification next trial
        JsonRpcHandler.Instance.SendNotification(
            message: new JsonRpcMessage(
                method: "NextTrial",
                data: new JObject(
                    new JProperty("part", _currentPart),
                    new JProperty("trial", -1),
                    new JProperty("area", location),
                    new JProperty("content", "Dog"),
                    new JProperty("size", _trialManager.CurrentConfig.size),
                    new JProperty("distance", _trialManager.CurrentConfig.distance),
                    new JProperty("tilt", _trialManager.CurrentConfig.tilt)
                )
            )
        );
    }

    [ContextMenu("StartTraining2")]
    private void StartTraining2()
    {
        LocationMarker.LocationName location = LocationMarker.LocationName.Floor; //hardcoded training location
        _trialManager.SetupTraining(1, location);


        //send notification next trial
        JsonRpcHandler.Instance.SendNotification(
            message: new JsonRpcMessage(
                method: "NextTrial",
                data: new JObject(
                    new JProperty("part", _currentPart),
                    new JProperty("trial", -1),
                    new JProperty("area", location),
                    new JProperty("content", "Dog"),
                    new JProperty("size", _trialManager.CurrentConfig.size),
                    new JProperty("distance", _trialManager.CurrentConfig.distance),
                    new JProperty("tilt", _trialManager.CurrentConfig.tilt)
                )
            )
        );
    }

    //starts the part according to the index in the part order
    private void StartPartByIndex(int index)
    {
        _currentVariables = _allVariables[index];
        _currentPart = _partOrderInt[index];
        StartTrialByIndex(0);
    }

    //starts the trial by trial index
    private void StartTrialByIndex(int index)
    {        
        List<string> trialVariables = new List<string>(_currentVariables[index].Split('-'));
        LocationMarker.LocationName location = LocationMarker.LocationName.Ceiling;
        ContentType contentType = ContentType.Icon;
        int fIndex = Int32.Parse(trialVariables[2])-1;
        if (trialVariables[0] != "C")
            location = LocationMarker.LocationName.Floor;
        if (trialVariables[3] != "I")
            contentType = ContentType.Picture;

        _trialManager.SetupTrial(location,_currentPart, fIndex, contentType, index);

        _currentTrial = index;

        //send notification next trial
        JsonRpcHandler.Instance.SendNotification(
            message: new JsonRpcMessage(
                method: "NextTrial",
                data: new JObject(
                    new JProperty("part", _currentPart),
                    new JProperty("trial", index),
                    new JProperty("area",location),
                    new JProperty("content",contentType),
                    new JProperty("size",_trialManager.CurrentConfig.size),
                    new JProperty("distance",_trialManager.CurrentConfig.distance),
                    new JProperty("tilt",_trialManager.CurrentConfig.tilt),
                    new JProperty("contentIndex", _trialManager.CurrentConfig.contentIndex)
                ),
                loggingMethod: $"Logging_TrialStarts-{_participantNumber}"
            )
        );
    }
    #endregion

    #region more testers
    [ContextMenu("TestStart")]
    private void Tester()
    {
        ReadFile();
        StartPartByIndex(0);
    }

    [ContextMenu("TestNextTrial")]
    private void nextTrialTester()
    {
        if (_currentTrial < _currentVariables.Count-1)
        {
            StartTrialByIndex(_currentTrial + 1);
            
        }
    }
    #endregion
}
