using Assets.Scripts.Utils;
using MasterNetworking.EventHandling;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wrappers;

public class BaseWrapper : MonoBehaviour
{
    public enum State
    {
        Preview=0,
        Fullview=1,
        Icon=2
    }

    private ContentProperty _contentProperty;
    public ContentProperty ContentProperty
    {
        get => _contentProperty;
        set
        {
            _contentProperty = value;
            if (_contentProperty != null && !_contentProperty.PlacementEmpty)
            {
                ContentUpdate();
                _onNotification_UpdateParameter(null);
            }
        }
    }

    [SerializeField]
    private bool _isStudy2;
    [Tooltip("This is the id of the visualization. It is given by hand. This can be expanded for automatic id building.")]
    [SerializeField]
    private ContentType _contentType;
    [SerializeField]
    private LocationMarker.LocationName _location;
    [SerializeField]
    private bool _useConnections;
    [SerializeField]
    private StringsAndBlocks.Connections _connectionType;
    [Tooltip("if no InteractionManager is provided the InteractionManager of the Prefab gets used.")]
    [SerializeField]
    private InteractionManager _interactionManager = null;
    [SerializeField]
    private bool _angleInteraction = false;
    [SerializeField]
    private bool _billboarding = false;
    [SerializeField]
    private Transform _targetCamera = null;
    [SerializeField]
    private GameObject _contentParent = null;
    [SerializeField]
    private GameObject _iconView = null;
    [SerializeField]
    private GameObject _preview = null;
    [SerializeField]
    private GameObject _fullview = null;
    [SerializeField]
    private CloneToHUDInteraction _cloneToHUDInteraction;
    [SerializeField]
    private float _distanceInHUD;
    [SerializeField]
    private AngleChangeInteraction _angleChangeInteractionScript;
    [SerializeField]
    private PositionCopy _positionCopy;
    [SerializeField]
    private Transform _yRotParent = null;
    [SerializeField]
    private Transform _configurationTransform = null;

    private State _state = State.Preview;

    private bool isInHUD = false;

    private bool interactionManagerChanged = true;

    public bool IsInHUD { get => isInHUD; set { isInHUD = value; ContentUpdate(); } }

    public GameObject ContentParent
    {
        get { return _contentParent; }

    }

    public bool AngleInteraction
    {
        get { return _angleInteraction; }
        set { _angleInteraction = value; ContentUpdate(); }
    }

    public bool Billboarding
    {
        get { return _billboarding; }
        set { _billboarding = value; ContentUpdate(); }
    }

    public Transform TargetCamera
    {
        get { return _targetCamera; }
        set { _targetCamera = value; ContentUpdate(); }
    }

    public InteractionManager InteractionManager
    {
        get { return _interactionManager; }
        set { _interactionManager = value; interactionManagerChanged=true; ContentUpdate(); }
    }

    public StringsAndBlocks StringsAndBlocks
    {
        get { return GetStringsAndBlocks(); }
    }



    public GameObject ActiveContent
    {
        get { switch (_state)
            {
                case State.Icon:
                    return _iconView;
                case State.Preview:
                    return _preview;
                case State.Fullview:
                    return _fullview;
                default:
                    return _iconView;
            } 
        }
    }

    public State StateOfView
    {
        get { return _state; }
        set { _state = value; ContentUpdate(); }
    }

    public GameObject Preview { get => _preview; set { SetNewPreview(value); ContentUpdate(); } }
    public GameObject Fullview { get => _fullview; set { SetNewFullview(value); ContentUpdate(); } }
    public GameObject Iconview { get => _iconView; set { SetNewIconview(value); ContentUpdate(); } }

    public CloneToHUDInteraction CloneToHUDInteraction { get => _cloneToHUDInteraction; set { _cloneToHUDInteraction = value; ContentUpdate(); } }

    public AngleChangeInteraction AngleChangeInteractionScript { get => _angleChangeInteractionScript; set { _angleChangeInteractionScript = value; ContentUpdate(); } }

    public ContentType ContentType { get => _contentType; set => _contentType = value; }
    public LocationMarker.LocationName Location { get => _location; set => _location = value; }

    public Transform YRotParent { get => _yRotParent; set { _yRotParent = value; ContentUpdate(); } }

    public Transform ConfigurationTransform { get => _configurationTransform; set => _configurationTransform = value; }
    public bool UseConnections { get => _useConnections; set { _useConnections = value; ContentUpdate(); } }
    public StringsAndBlocks.Connections ConnectionType { get => _connectionType; set { _connectionType = value; ContentUpdate(); } }

    public float DistanceInHUD { get => _distanceInHUD; set { _distanceInHUD = value; ContentUpdate(); } }

    private bool testingFlagInteraction;
    private bool testingFlagBillboard;
    // Start is called before the first frame update
    protected virtual void Start()
    {
        JsonRpcHandler.Instance.AddNotificationDelegate(rpcEvent: "UpdateParametersS1", notification: _onNotification_UpdateParameter);
        JsonRpcHandler.Instance.AddNotificationDelegate(rpcEvent: "UpdateParameters", notification: _onNotification_UpdateParameters);
        ContentObjectsInit();
        testingFlagBillboard = Billboarding;
        testingFlagInteraction = AngleInteraction;
        if(_positionCopy==null)
            _positionCopy = GetComponent<PositionCopy>();
        if (TargetCamera != null)
        {
            _positionCopy.ReferenceObject = TargetCamera;
        }
        //if (_isStudy2)
        //{
        //    ContentProperty = new ContentProperty();
        //    ContentProperty.InitLocal(ConfigurationTransform.localPosition.z, ConfigurationTransform.localPosition.y, ConfigurationTransform.localPosition.x, ConfigurationTransform.localRotation.eulerAngles.x, YRotParent.localRotation.eulerAngles.y, ConfigurationTransform.localEulerAngles.y, ConfigurationTransform.localScale.x, Location, ReferenceFrame.Ego, ContentType);
        //}

        ContentUpdate();
    }

    private void _onNotification_UpdateParameters(JsonRpcMessage message)
    {
        float distance = 0;
        float tilt = 0;
        float size = 0;

        if (message.Data["distance"] != null && message.Data["distance"]?.Type != JTokenType.Null)
            distance = message.Data["distance"].ToObject<float>();
        if (message.Data["tilt"] != null && message.Data["tilt"]?.Type != JTokenType.Null)
            tilt = message.Data["tilt"].ToObject<float>();
        if (message.Data["size"] != null && message.Data["size"]?.Type != JTokenType.Null)
            size = message.Data["size"].ToObject<float>();
        switch (Location)
        {
            case LocationMarker.LocationName.Ceiling:
                ConfigurationTransform.localPosition = new Vector3(ConfigurationTransform.localPosition.x, ConfigurationTransform.localPosition.y, distance);
                ConfigurationTransform.localEulerAngles = new Vector3(tilt, ConfigurationTransform.localEulerAngles.y, ConfigurationTransform.localEulerAngles.z);
                ConfigurationTransform.localScale = new Vector3(size, ConfigurationTransform.localScale.y, size);
                break;
            case LocationMarker.LocationName.Floor:
                ConfigurationTransform.localPosition = new Vector3(ConfigurationTransform.localPosition.x, ConfigurationTransform.localPosition.y, - distance);
                ConfigurationTransform.localEulerAngles = new Vector3(-tilt, ConfigurationTransform.localEulerAngles.y, ConfigurationTransform.localEulerAngles.z);
                ConfigurationTransform.localScale = new Vector3(size, ConfigurationTransform.localScale.y, size);
                break;
            default:
                break;
        }



    }

    [ContextMenu("FakeMessage")]
    public void FakeMessage()
    {
        JObject data = new JObject(
            new JProperty("id", (int)ContentType.Email),
            new JProperty("distance", 1),
            new JProperty("heightAddition", 0.2),
            new JProperty("posX", 0.4),
            new JProperty("tilt", 90),
            new JProperty("scale", 2),
            new JProperty("yaw",90),
            new JProperty("egoRotation", 45),
            new JProperty("followUser",false)
        );
        _onNotification_UpdateParameter(new JsonRpcMessage(data, "PropertyUpdateEvent"));
    }

    public void InternalPropertieUpdate(float distance, float heightAddition, float posX, float tilt, float egoRotation, float yaw, float scale)
    {
        ContentProperty.SetTransformationValuesInternal(distance, heightAddition, posX, tilt, egoRotation, yaw, scale);
        ApplyContentProperties();

    }

    private void ApplyContentProperties()
    {
        switch (Location)
        {
            // #BugCheck Why are both HeighAdditions negative? Is this caused by some parent rotation?
            case LocationMarker.LocationName.Ceiling:
                ConfigurationTransform.localPosition = new Vector3(ContentProperty.PosX, -ContentProperty.HeightAddition, ContentProperty.Distance);
                // #TODO Check if this size application works.
                ConfigurationTransform.localScale = new Vector3(ContentProperty.Size, ContentProperty.Size, ContentProperty.Size);
                ConfigurationTransform.localRotation = Quaternion.Euler(ContentProperty.Tilt, ContentProperty.Yaw, 0);
                ConfigurationTransform.GetComponent<LocalAxisBlocker>().maxAngle = ContentProperty.Tilt;
                YRotParent.localRotation = Quaternion.Euler(transform.parent.localEulerAngles.x, ContentProperty.EgoRotation, transform.parent.localEulerAngles.z);
                break;
            case LocationMarker.LocationName.Floor:
                ConfigurationTransform.localPosition = new Vector3(ContentProperty.PosX, -ContentProperty.HeightAddition, -ContentProperty.Distance);
                // #TODO Check if this size application works.
                ConfigurationTransform.localScale = new Vector3(ContentProperty.Size, ContentProperty.Size, ContentProperty.Size);
                ConfigurationTransform.localRotation = Quaternion.Euler(-ContentProperty.Tilt, ContentProperty.Yaw, 0);
                ConfigurationTransform.GetComponent<LocalAxisBlocker>().maxAngle = -ContentProperty.Tilt;
                YRotParent.localRotation = Quaternion.Euler(transform.parent.localEulerAngles.x, -ContentProperty.EgoRotation, transform.parent.localEulerAngles.z);
                break;
        }
    }

    private void _onNotification_UpdateParameter(JsonRpcMessage message)
    {
        if (ContentProperty == null)
            return;

        if (message != null)
        {
            ContentProperty.SetTransformationValues(message);
            if (ContentType != ContentProperty.ContentType&&!_isStudy2)
                return;
        }

        ApplyContentProperties();
        
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (testingFlagInteraction != AngleInteraction || testingFlagBillboard != Billboarding)
        {
            ContentUpdate();
            testingFlagBillboard = Billboarding;
            testingFlagInteraction = AngleInteraction;
        }
        switch (this.StateOfView)
        {
            case State.Preview:
                Fullview.SetActive(false);
                Iconview.SetActive(false);
                Preview.SetActive(true);
                break;
            case State.Fullview:
                Preview.SetActive(false);
                Iconview.SetActive(false);
                Fullview.SetActive(true);
                break;
            case State.Icon:
                Preview.SetActive(false);
                Fullview.SetActive(false);
                Iconview.SetActive(true);
                break;
        }
    }

    public virtual void ContentObjectsInit()
    {
        if(_contentParent == null)
        {
            _contentParent = RecursiveFindChild(this.transform, "ContentParent").gameObject;
        }

        if (Preview == null)
        {
            _preview = ContentParent.transform.GetChild(0).gameObject;
        }
        if (_fullview == null)
        {
            if (ContentParent.transform.childCount>=2)
                _fullview = ContentParent.transform.GetChild(1).gameObject;
            else
                _fullview = _preview;
        }
        if (_iconView == null)
        {
            if (ContentParent.transform.childCount >= 3)
                _iconView = ContentParent.transform.GetChild(2).gameObject;
            else
                _iconView = _preview;
        }

    }

    private Transform RecursiveFindChild(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
            {
                return child;
            }
            else
            {
                Transform found = RecursiveFindChild(child, childName);
                if (found != null)
                {
                    return found;
                }
            }
        }
        return null;
    }

    public virtual void ContentUpdate()
    {
        if (TargetCamera == null)
        {
            TargetCamera = CameraCache.Main.transform;
        }
        AngleChangeInteraction angleInteraction = GetComponentInChildren<AngleChangeInteraction>(true);
        if (InteractionManager == null)
        {
            InteractionManager = GetComponent<InteractionManager>();
            InteractionManager.TargetCamera = TargetCamera;
        }

        StringsAndBlocks.DrawStringsOrBlocks = _useConnections;
        StringsAndBlocks.ConnectionType = _connectionType;
        _positionCopy.SetPositionCopy(ContentProperty == null ? false : ContentProperty.ReferenceFrame == ReferenceFrame.Ego);
        angleInteraction.enabled = AngleInteraction;
        EyeTrackingTarget eyeTrackingTarget = GetComponentInChildren<EyeTrackingTarget>(true);
        eyeTrackingTarget.enabled = AngleInteraction;
        LocalAxisBlocker axisBlocker = GetComponentInChildren<LocalAxisBlocker>(true);
        ExtendedBillboard extendedBillboard = GetComponentInChildren<ExtendedBillboard>(true);
        axisBlocker.enabled = Billboarding;
        extendedBillboard.enabled = Billboarding;
        extendedBillboard.TargetTransform = TargetCamera;
    }

    private void SetNewPreview(GameObject previewContent)
    {
        if (previewContent == null)
        {
#if UNITY_EDITOR
            DestroyImmediate(_preview,true);
#endif      
            return;
        }
        
#if UNITY_EDITOR
        DestroyImmediate(_preview,true);
#else
        Destroy(_preview);
#endif
        _preview = Instantiate(previewContent, ContentParent.transform, worldPositionStays: false);
        _preview.GetComponent<BoxCollider>().enabled = false;
        _preview.SetActive(true);
    }

    private void SetNewFullview(GameObject fullviewContent)
    {
        if (fullviewContent == null)
        {
#if UNITY_EDITOR
            DestroyImmediate(_fullview,true);
#endif
            return;
        }

#if UNITY_EDITOR
        DestroyImmediate(_fullview,true);
#else
        Destroy(_fullview);
#endif
        _fullview = Instantiate(fullviewContent,ContentParent.transform,worldPositionStays:false);
        _fullview.GetComponent<BoxCollider>().enabled = false;
        _fullview.SetActive(true);
    }

    private void SetNewIconview(GameObject iconviewContent)
    {
        if (iconviewContent == null)
        {
#if UNITY_EDITOR
            DestroyImmediate(_iconView,true);
#endif
            return;
        }

#if UNITY_EDITOR
        DestroyImmediate(_iconView,true);
#else
        Destroy(_iconView);
#endif
        _iconView = Instantiate(iconviewContent,ContentParent.transform,worldPositionStays:false);
        _iconView.GetComponent<BoxCollider>().enabled = false;
        _iconView.SetActive(true);
    }

    public void FixContentrefsIfLessThanThree()
    {
        if (_preview == null)
        {
            if (_iconView != null)
            {
                _preview = _iconView;
            }
            else if (_fullview != null)
            {
                _preview = _fullview;
            }
            else
            {
                Debug.LogError("you need at least one content!");
            }
        }
        if (_iconView == null)
        {
            if (_preview != null)
            {
                _iconView = _preview;
            }
            else if (_fullview != null)
            {
                _iconView = _fullview;
            }
            else
            {
                Debug.LogError("you need at least one content!");
            }
        }
        if (_fullview == null)
        {
            if (_preview != null)
            {
                _fullview = _preview;
            }
            else if (_iconView != null)
            {
                _fullview = _iconView;
            }
            else
            {
                Debug.LogError("you need at least one content!");
            }
        }
    }

    public virtual void Initilize(bool? useAngleInteraction = null, bool? useLookAngleInteraction = null, InteractionManager interactionManager = null, Transform targetCam = null, GameObject preview=null, GameObject fullview = null, GameObject iconview = null)
    {
        if(interactionManager != null)
            InteractionManager = interactionManager;
        if (preview!= null)
        {
            Preview = preview;
        }
        if (fullview != null)
        {
            Fullview = fullview;
        }
        if (iconview != null)
        {
            Iconview = iconview;
        }


        //was wenn preview und fullview null
        ContentObjectsInit();
        if(useAngleInteraction!=null)
            AngleInteraction = (bool)useAngleInteraction;
        if (useLookAngleInteraction != null)
        {
            InteractionManager.setUseLookForAngle((bool)useLookAngleInteraction);
            InteractionManager.setUseDistanceForAngle(!(bool)useLookAngleInteraction);
        }
        TargetCamera = targetCam;
        ContentUpdate();

    }

    private StringsAndBlocks GetStringsAndBlocks()
    {
        return GetComponentInChildren<StringsAndBlocks>(); ;
    }
}
