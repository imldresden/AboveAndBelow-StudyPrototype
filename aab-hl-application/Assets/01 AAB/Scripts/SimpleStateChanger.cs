using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleStateChanger : MonoBehaviour
{
    [SerializeField]
    private BaseWrapper _wrapper;
    [SerializeField]
    [Range(0, 360)] private float iconAngle;
    [SerializeField]
    [Range(0, 360)] private float previewAngle;
    [SerializeField]
    [Range(0, 360)] private float fullViewAngle;
    [SerializeField]
    private AngleChangeInteraction _angleChangeInteraction;
    [SerializeField]
    private bool useSetAnglesForIconAndPreview = true;

    private bool firstTime=true;

    void OnEnable()
    {
        if (CheckForGazeNoAngle())
        {
            _angleChangeInteraction.SetAngle(iconAngle);
        }

    }

    private void LateUpdate()
    {
        if (firstTime)
        {
            if (useSetAnglesForIconAndPreview)
            {
                StartCoroutine(Waiter());
                iconAngle = _angleChangeInteraction.MinAngle;
                previewAngle = 0f;
                _angleChangeInteraction.SetAngle(iconAngle);
            }
            firstTime = false;
        }
    }

    IEnumerator Waiter()
    {
        yield return new WaitForSeconds(0.1f);
    }
    public void setIconState()
    {
        if (CheckForGazeNoAngle())
        {
            _wrapper.StateOfView = BaseWrapper.State.Icon;
            _angleChangeInteraction.SetAngle(iconAngle);
        }
    }

    public void setPreviewState()
    {
        if (CheckForGazeNoAngle())
        {
            _wrapper.StateOfView = BaseWrapper.State.Preview;
            _angleChangeInteraction.SetAngle(previewAngle);
        }
    }

    public void setFullviewState()
    {
        if (CheckForGazeNoAngle())
        {
            _wrapper.StateOfView = BaseWrapper.State.Fullview;
            _angleChangeInteraction.SetAngle(fullViewAngle);
        }
    }
    
    private bool CheckForGazeNoAngle()
    {
        if (_wrapper.InteractionManager.InteractionTech == InteractionManager.InteractionTechnique.gazeNoAngle&&!_wrapper.IsInHUD)
            return true;
        else
            return false;
    }
}
