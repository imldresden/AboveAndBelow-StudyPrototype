using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloneToHUDInteraction : MonoBehaviour
{

    [SerializeField]
    private GameObject _parentToClone;
    [SerializeField]
    private BaseWrapper _wrapper;

    public GameObject Hud;
    public string HudNameToFind;
    private GameObject _cloneInHUD;
    private BaseWrapper _originalWrapper;
    private bool _isInHUD=false;

    public float FloorHeightAdjustment = -.6f;
    public float CeilingHeightAdjustment = .3f;


    public bool IsInHUD { get => _isInHUD; set => _isInHUD = value; }
    public BaseWrapper OriginalWrapper { get => _originalWrapper; set => _originalWrapper = value; }
    public GameObject CloneInHUD { get => _cloneInHUD; set => _cloneInHUD = value; }

    // Start is called before the first frame update
    void Start()
    {
        if (Hud == null)
            FindHUD();
        if (Hud == null)
            Debug.LogError($"Did not found a HUD pareant with the name: {HudNameToFind}");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        if (Hud == null)
            FindHUD();
        if (Hud == null)
            Debug.LogError("Did not found HUD_Parent");
    }
    public void OnClick()
    {
        
        if (_wrapper.InteractionManager.ClickForHUD && !_isInHUD)
        {
            // #Debug Unnecessary Debug Logs ...
            //Debug.Log(Hud + " | " + GetInstanceID().ToString());

            _cloneInHUD = Instantiate(_parentToClone, GameObject.Find(HudNameToFind).transform, worldPositionStays: false); //FindHUD sets _hud to the hud, but here it is null again?? Therfore use Find
            //_cloneInHUD = Instantiate(_parentToClone, _hud.transform, worldPositionStays: false);
            BaseWrapper cloneWrapper = _cloneInHUD.GetComponent<BaseWrapper>();
            Debug.Log(cloneWrapper);
            cloneWrapper.InteractionManager = cloneWrapper.gameObject.GetComponent<InteractionManager>();
            InteractionManager cloneInteractionManager = cloneWrapper.InteractionManager;
            int sign = Math.Sign(cloneWrapper.ConfigurationTransform.localRotation.eulerAngles.x);
            //_cloneInHUD.transform.localRotation = Quaternion.Euler(sign * 90, 0, 0);
            cloneWrapper.ConfigurationTransform.localPosition = new Vector3(cloneWrapper.ConfigurationTransform.localPosition.x, cloneWrapper.ConfigurationTransform.localPosition.y, Math.Abs(cloneWrapper.ConfigurationTransform.localPosition.z));
            cloneWrapper.CloneToHUDInteraction.IsInHUD = true;
            // cloneWrapper.FollowUser = true;
            cloneWrapper.ConfigurationTransform.localRotation = Quaternion.Euler(sign * 90, 0, 0);
            Debug.Log(cloneWrapper.ConfigurationTransform.localRotation);
            cloneWrapper.CloneToHUDInteraction.CloneInHUD = _cloneInHUD;
            cloneWrapper.AngleInteraction = false;
            cloneInteractionManager.setUseDistanceForAngle(false);
            cloneInteractionManager.setUseLookForAngle(false);
            cloneWrapper.StateOfView = BaseWrapper.State.Fullview;
            cloneWrapper.IsInHUD = true;
            cloneWrapper.CloneToHUDInteraction.OriginalWrapper = _wrapper;
            float heightAdjustment;
            switch (_wrapper.Location)
            {
                case LocationMarker.LocationName.Floor:
                    heightAdjustment = FloorHeightAdjustment;
                    break;
                case LocationMarker.LocationName.Ceiling:
                    heightAdjustment = CeilingHeightAdjustment;
                    break;
                default:
                    heightAdjustment = 0;
                    break;
            }
            cloneWrapper.transform.localRotation = Quaternion.Euler(0, 0, 0);
            cloneWrapper.transform.localPosition = Vector3.zero;
            cloneWrapper.ConfigurationTransform.localRotation = Quaternion.Euler(90, 0, 0);
            cloneWrapper.ConfigurationTransform.localPosition = new Vector3(0, heightAdjustment, _wrapper.DistanceInHUD);


        }
        else if (_isInHUD)
        {
            Destroy(CloneInHUD);
        }

    }


    private void FindHUD()
    {
        GameObject hud = GameObject.Find(HudNameToFind);
        Hud = hud;
        // #Debug Unnecessary Debug Logs ...
        //Debug.Log(_hud + " | " + GetInstanceID().ToString());

    }
}
