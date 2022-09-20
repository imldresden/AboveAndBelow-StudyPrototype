using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalAxisBlocker : MonoBehaviour
{
    public enum AxisToBlock
    {
        x,
        y,
        z,
        xy,
        xz,
        yz
    }
    [SerializeField]
    private AxisToBlock _axisToBlock;
    [SerializeField]
    private bool compensateForWeirdParentObj = false;
    [SerializeField]
    private BaseWrapper _baseWrapper;
    private Quaternion startLocalRotation;

    public float maxAngle;


    private float angle;
    // Start is called before the first frame update
    void Start()
    {
        maxAngle = transform.localRotation.eulerAngles.x;
        startLocalRotation = this.transform.localRotation;
    }


    // Update is called once per frame
    void LateUpdate()
    {
        float subtractor = 0f;
        float multiplicator = 1f;
        if (compensateForWeirdParentObj)
        {
            subtractor = 180f;
            multiplicator = -1;
        }

        Vector3 localRotation = transform.localRotation.eulerAngles;
        Quaternion newRotation = new Quaternion();
        switch(_axisToBlock)
        {
            case AxisToBlock.x:
                newRotation = Quaternion.Euler(startLocalRotation.x, (localRotation.y-subtractor)*multiplicator, (localRotation.z - subtractor)*multiplicator);
                break;
            case AxisToBlock.y:
                newRotation = Quaternion.Euler((localRotation.x - subtractor)*multiplicator, startLocalRotation.y, (localRotation.z - subtractor)*multiplicator);
                break;
            case AxisToBlock.z:
                newRotation = Quaternion.Euler((localRotation.x - subtractor)*multiplicator, (localRotation.y - subtractor)*multiplicator, startLocalRotation.z);
                break;
            case AxisToBlock.xy:
                newRotation = Quaternion.Euler(startLocalRotation.x, startLocalRotation.y, (localRotation.z - subtractor)*multiplicator);
                break;
            case AxisToBlock.xz:
                newRotation = Quaternion.Euler(startLocalRotation.x, (localRotation.y - subtractor)*multiplicator, startLocalRotation.z);
                break;
            case AxisToBlock.yz:
                newRotation = Quaternion.Euler((localRotation.x - subtractor)*multiplicator, startLocalRotation.y, startLocalRotation.z);
                break;
            default:
                newRotation = Quaternion.Euler(localRotation);
                break;

        }
        if (_baseWrapper.Location == LocationMarker.LocationName.Ceiling)
        {
            newRotation = Quaternion.Euler(Math.Abs(maxAngle), newRotation.eulerAngles.y, newRotation.eulerAngles.z);
            this.transform.localRotation = newRotation;
        }
        if (_baseWrapper.Location == LocationMarker.LocationName.Floor)
        {
            //Math.Abs(_baseWrapper.AngleChangeInteractionScript.MinAngle) % 360

            // #Debug Unnecessary Debug Logs ...
            //Debug.Log(360- Math.Abs(_baseWrapper.AngleChangeInteractionScript.MinAngle));
            newRotation = Quaternion.Euler((360 - Math.Abs(maxAngle))-180, newRotation.eulerAngles.y, newRotation.eulerAngles.z);
            newRotation.Normalize();
            this.transform.localRotation=newRotation;



        }
        //Debug.Log(newRotation.eulerAngles);
        
    }
}
