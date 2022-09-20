using MasterNetworking.EventHandling;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdjustFloorAndCeilingPos : MonoBehaviour
{
    [SerializeField]
    private float _ceilingHeight;
    [SerializeField]
    private float _floorHeight;
    [SerializeField]
    private bool _alwaysUpdate;
    [SerializeField]
    private bool _usePositionGetter;
    [SerializeField]
    private Transform _ceiling;
    [SerializeField]
    private Transform _floor;
    // Start is called before the first frame update
    void Start()
    {
        JsonRpcHandler.Instance.AddNotificationDelegate("SetCoordinateSystem", _onNotification_SetCoordinateSystem);
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_alwaysUpdate)
        {
            UpdatePos();
        }
    }

    private void _onNotification_SetCoordinateSystem(JsonRpcMessage message)
    {
        _floorHeight = message.Data["floorHeight"].ToObject<float>();
        _ceilingHeight = message.Data["ceilingHeight"].ToObject<float>();
    }

    public void UpdatePos()
    {
        _ceiling.localPosition = new Vector3(_ceiling.localPosition.x, _ceilingHeight, _ceiling.localPosition.z); //add heigth to y here or set y of contentroot to zero always
        _floor.localPosition = new Vector3(_floor.localPosition.x, _floorHeight, _floor.localPosition.z);
    }
}
