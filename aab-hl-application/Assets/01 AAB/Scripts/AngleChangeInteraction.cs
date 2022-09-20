using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class AngleChangeInteraction : MonoBehaviour
{
    public enum Axis
    {
        x,
        y,
        z
    }

    [SerializeField]
    private Axis _angleAxis;
    [SerializeField]
    private Transform _parentWithMaximumRotation;
    [SerializeField]
    private Transform _objectToAngle;
    [SerializeField]
    private BaseWrapper _wrapper;

    private Transform parent;
    private bool useDistanceForAngle = false;
    private bool useLookForAngle = false;

    private bool animate = false;
    private bool forwards = true;
    private float minAngle = 0f;
    private bool onFocus = false;
    private float currentAngle=0;
    private Transform targetCamera;
    private bool lookedAt = false;

    public Axis AngleAxis { get => _angleAxis; set => _angleAxis = value; }
    public Transform ParentWithMaximumRotation { get => _parentWithMaximumRotation; set => _parentWithMaximumRotation = value; }
    public Transform ObjectToAngle { get => _objectToAngle; set => _objectToAngle = value; }
    public BaseWrapper Wrapper { get => _wrapper; set => _wrapper = value; }
    public float MinAngle { get => minAngle; }
    public float CurrentAngle { get => currentAngle;}

    // Start is called before the first frame update
    void Start()
    {
        parent = _parentWithMaximumRotation;
        targetCamera = _wrapper.InteractionManager.TargetCamera;
        
    }

    private void OnEnable()
    {
        animate = false;
    }

    private void OnDisable()
    {
        SetAngle(0);
    }
    // Update is called once per frame
    void Update()
    {
        InteractionManager _interactionManager = _wrapper.InteractionManager;
        useDistanceForAngle = _interactionManager.UseDistanceForAngle;
        useLookForAngle = _interactionManager.UseLookForAngle;
        switch (_angleAxis)
        {
            case Axis.x:
                minAngle = parent.localRotation.eulerAngles.x * -1f * Mathf.Sign(parent.localRotation.eulerAngles.x);
                if (minAngle < -90)
                {
                    minAngle = -minAngle % 360;
                    minAngle = 360 - minAngle;
                }
                break;
            case Axis.y:
                minAngle = parent.localRotation.eulerAngles.y * -1f * Mathf.Sign(parent.localRotation.eulerAngles.y);
                if (minAngle < -90)
                {
                    minAngle = -minAngle % 360;
                    minAngle = 360 - minAngle;
                }
                break;
            case Axis.z:
                minAngle = parent.localRotation.eulerAngles.z * -1f * Mathf.Sign(parent.localRotation.eulerAngles.z);
                if (minAngle < -90)
                {
                    minAngle = -minAngle % 360;
                    minAngle = 360 - minAngle;
                }
                break;
            default:
                break;
        }

        // #Check Is this appleciable for all instances/usages of this script?
        // Get the child right direct under the 
        BaseWrapper parentWrapper = GetComponentInParent<BaseWrapper>();
        Vector3 parentPos = parentWrapper.transform.GetChild(0).position;
        float offset = parentWrapper.transform.GetChild(0).localPosition.z;
        // Get the correct position of the parent which has to be altered based on the location.
        Vector2 pos = new Vector2(
            parentPos.x, 
            parentPos.z
        );
        Vector2 camPos = new Vector2(targetCamera.position.x, targetCamera.position.z);
        float distance = Mathf.Abs(Vector2.Distance(camPos, pos));

        if (!onFocus && useLookForAngle && !animate && !useDistanceForAngle)
        {
           // Debug.Log(useLookForAngle + " | " + minAngle);
            switch (_angleAxis)
            {
                case Axis.x:
                    _objectToAngle.localRotation = Quaternion.Euler(minAngle, _objectToAngle.localRotation.eulerAngles.y, _objectToAngle.localRotation.eulerAngles.z);
                    currentAngle = minAngle;
                    break;
                case Axis.y:
                    _objectToAngle.localRotation = Quaternion.Euler(_objectToAngle.localRotation.eulerAngles.x, minAngle, _objectToAngle.localRotation.eulerAngles.z);
                    currentAngle = minAngle;
                    break;
                case Axis.z:
                    _objectToAngle.localRotation = Quaternion.Euler(_objectToAngle.localRotation.eulerAngles.x, _objectToAngle.localRotation.eulerAngles.y, minAngle);
                    currentAngle = minAngle;
                    break;
                default:
                    break;
            }
            
        }

        if (animate)
            Animate(forwards);

        if (!useLookForAngle && useDistanceForAngle)
        {
            if (distance <= _interactionManager.MinDistance)
            {
                SetAngle(0);
            }
            if (distance > _interactionManager.MinDistance && distance <= _interactionManager.MaxDistance)
            {
                float multiplier = (1 / (_interactionManager.MaxDistance - _interactionManager.MinDistance)) * (distance - _interactionManager.MinDistance);
              //  Debug.Log("Multiplier: " + multiplier);
                SetAngle(multiplier * minAngle);
            }
            if (distance> _interactionManager.MaxDistance)
            {
                SetAngle(minAngle);
            }
        }
        if (currentAngle == 0 && (useDistanceForAngle || useLookForAngle))
        {
            _wrapper.StateOfView = BaseWrapper.State.Preview;
        }
        if (currentAngle == minAngle && (useDistanceForAngle || useLookForAngle))
        {
            _wrapper.StateOfView = BaseWrapper.State.Icon;
        }

    }

    private void Animate(bool forward)
    {
        //Debug.Log(forward);
        float nextAngle =0;
        float sign = Mathf.Sign(minAngle);
        //Debug.Log(sign + " | " + currentAngle);
        if (forward)
        {
            if ((sign == -1 && currentAngle < 0f) || (sign == 1 && currentAngle > 0f))
            {
                nextAngle = currentAngle + 0.5f * sign * -1f;
             //   Debug.Log("nextAngle: " + nextAngle);
                if ((sign == -1 && nextAngle >= 0f) || (sign == 1 && nextAngle <= 0f))
                {
                   // Debug.Log("last if");
                    nextAngle = 0;
                    animate = false;
                    onFocus = true;
                }
            }
        }
        else
        {
           // Debug.Log("backwards: "+currentAngle.ToString() + " | " + sign.ToString() + " | " + (currentAngle + 0.005f * sign).ToString());
            if ((sign == -1 && currentAngle > minAngle) || (sign == 1 && currentAngle < minAngle))
            {
                nextAngle = currentAngle + 0.5f * sign;
                if ((sign == -1 && nextAngle <= minAngle) || (sign == 1 && nextAngle >= minAngle))
                {
                    nextAngle = minAngle;
                    animate = false;
                    onFocus = false;
                }
            }
        }

        //Debug.Log(nextAngle);
        SetAngle(nextAngle);
        currentAngle = nextAngle;
    }

    public void OnLookAway()
    {
        lookedAt = false;
        if (useLookForAngle)
            StartCoroutine(LookAwayWaiter(_wrapper.InteractionManager.WaitTimeFokusLostWhileAnimate, _wrapper.InteractionManager.WaitTimeFokusLost));

    }

    private IEnumerator LookAwayWaiter(float waitTimeWhileAnimate, float waitTime)
    {
        if (currentAngle != 0)
        {
            animate = false;
            forwards = false;
            yield return new WaitForSeconds(waitTimeWhileAnimate);
            if(!lookedAt)
                animate = true;
        }
        else
        {
            yield return new WaitForSeconds(waitTime);
            if (currentAngle != minAngle && !lookedAt)
            {
                animate = true;
                forwards = false;
            }
        }
        Debug.Log("OnLookAway| animate: " + animate + " | forwards: " + forwards);
    }
    public void OnLookAt()
    {
        lookedAt = true;
        if (useLookForAngle)
        {
            animate = true;
            onFocus = true;
            forwards = true;
        }
        Debug.Log("OnLookAt| animate: " + animate + " | forwards: " + forwards);
    }
    public void SetAngle(float angle)
    {
        switch (_angleAxis)
        {
            case Axis.x:
                _objectToAngle.localRotation = Quaternion.Euler(angle, _objectToAngle.localRotation.eulerAngles.y, _objectToAngle.localRotation.eulerAngles.z);
                break;
            case Axis.y:
                _objectToAngle.localRotation = Quaternion.Euler(_objectToAngle.localRotation.eulerAngles.x, angle, _objectToAngle.localRotation.eulerAngles.z);
                break;
            case Axis.z:
                _objectToAngle.localRotation = Quaternion.Euler(_objectToAngle.localRotation.eulerAngles.x, _objectToAngle.localRotation.eulerAngles.y, angle);
                break;
            default:
                break;
        }
        currentAngle = angle;
    }

    public void toggleAnimate()
    {
        animate = !animate;
    }

    public void setForwards(bool value)
    {
        forwards = value;
    }

    public void setFocus(bool isOnFocus)
    {
        onFocus = isOnFocus;
    }
}
