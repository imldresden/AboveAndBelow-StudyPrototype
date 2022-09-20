using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    public enum InteractionTechnique
    {
        
        gazeAngle,
        distance,
        gazeNoAngle

    }
    [SerializeField]
    private InteractionTechnique _interactionTech;
    
    private bool useDistanceForAngle = false;
    private bool useLookForAngle = false;
    private bool useGazeNoAngle = false;

    [SerializeField]
    private bool clickForHUD = true;
    [SerializeField]
    private float minDistance;
    [SerializeField]
    private float maxDistance;
    [SerializeField]
    private Transform targetCamera;
    [SerializeField]
    private float waitTimeFokusLost = 3;
    [SerializeField]
    private float waitTimeFokusLostWhileAnimate = 1;

    public bool UseLookForAngle { get => useLookForAngle; }
    public bool UseDistanceForAngle { get => useDistanceForAngle; }
    public float MinDistance { get => minDistance; set => minDistance = value; }
    public float MaxDistance { get => maxDistance; set => maxDistance = value; }
    public Transform TargetCamera { get => targetCamera; set => targetCamera = value; }
    public bool ClickForHUD { get => clickForHUD; set => clickForHUD = value; }
    public float WaitTimeFokusLost { get => waitTimeFokusLost; set => waitTimeFokusLost = value; }
    public float WaitTimeFokusLostWhileAnimate { get => waitTimeFokusLostWhileAnimate; set => waitTimeFokusLostWhileAnimate = value; }

    public InteractionTechnique InteractionTech { get => _interactionTech; set => _interactionTech = value; }

    public void Start()
    {
        // #Debug Unnecessary Debug Logs ...
        //Debug.Log(targetCamera);
        if (targetCamera == null)
        {
            //Debug.Log("blubb");
            targetCamera = CameraCache.Main.transform;
            //Debug.Log(targetCamera);
        }
    }

    public void OnEnable()
    {
        switch (_interactionTech)
        {
            case InteractionTechnique.distance:
                useDistanceForAngle = true;
                useLookForAngle = false;
                useGazeNoAngle = false;
                break;
            case InteractionTechnique.gazeAngle:
                useDistanceForAngle = false;
                useLookForAngle = true;
                useGazeNoAngle = false;
                break;
            case InteractionTechnique.gazeNoAngle:
                useDistanceForAngle = false;
                useLookForAngle = false;
                useGazeNoAngle = true;
                break;
            default:
                useDistanceForAngle = false;
                useLookForAngle = false;
                useGazeNoAngle = false;
                break;
        }

    }

    public void SetInteractionTech(InteractionTechnique tech)
    {
        _interactionTech = tech;
        OnEnable();
    }

    public void ToggelDistanceAngle()
    {
        useDistanceForAngle = !useDistanceForAngle;
    }

    public void ToggleLookAngle()
    {
        useLookForAngle = !useLookForAngle;
        Debug.Log(useLookForAngle);
    }

    public void setUseLookForAngle(bool value)
    {
        useLookForAngle = value;
    }

    public void setUseDistanceForAngle(bool value)
    {
        useDistanceForAngle = value;
    }
}
