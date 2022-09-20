using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrialManager : MonoBehaviour
{
    System.Random _random;

    [SerializeField]
    private float[] _distanceParams = new float[5];
    [SerializeField]
    private Vector2 _distanceMinMax;
    [SerializeField]
    private float[] _tiltParams = new float[5];
    [SerializeField]
    private Vector2 _tiltMinMax;
    [SerializeField]
    private float[] _sizeParams = new float[5];
    [SerializeField]
    private Vector2 _sizeMinMax;
    [SerializeField]
    private GameObject _ceilingObject;
    [SerializeField]
    private GameObject _floorObject;
    [SerializeField]
    private Sprite[] _trainingSprites=new Sprite[2];
    [SerializeField]
    private Sprite[] _iconSprites = new Sprite[20];
    [SerializeField]
    private Sprite[] _pictureSprites = new Sprite[20];

    private int _iconCount = -1;
    private int _pictureCount = -1;

    public ConfigInformation CurrentConfig { get; private set; }

    private GameObject _usedObject;
    private LocationMarker.LocationName _currentLocation;
    private ContentType _currentType;
    private int _currentContentIndex;
    private int _currentTrial;
    private Image _ceilingImage;
    private Image _floorImage;
    private List<int> _iconIndex = new List<int>() { 0, 1, 2, 3, 4
        , 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 
    };
    private List<int> _pictureIndex = new List<int>() { 0, 1, 2, 3, 4
       , 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19
    };
    // Start is called before the first frame update
    public enum ContentType
    {
        Icon,
        Picture,
        Training
    }

    public struct ConfigInformation
    {
        public LocationMarker.LocationName location;
        public ContentType contentType;
        public float size;
        public float distance;
        public float tilt;
        public int contentIndex;
    }

    void Start()
    {
        _random = new System.Random();
        _ceilingImage = _ceilingObject.GetComponentInChildren<Image>();
        _floorImage = _floorObject.GetComponentInChildren<Image>();
    }

    //setup trial
    public void SetupTrial(LocationMarker.LocationName location,int part,int fIndex,ContentType contentType,int trialIndex)
    {
        Debug.Log($"{location} | {part} | {fIndex} | {contentType} | {trialIndex}");

        Image usedImage;
        int factor = 1;
        //setup which location is active
        if (location == LocationMarker.LocationName.Ceiling)
        {
            _usedObject = _ceilingObject;
            _currentLocation = LocationMarker.LocationName.Ceiling;
            usedImage = _ceilingImage;
        }
        else
        {
            factor = -1;
            _usedObject = _floorObject;
            _currentLocation = LocationMarker.LocationName.Floor;
            usedImage = _floorImage;
        }

        _ceilingObject.SetActive(false);
        _floorObject.SetActive(false);

        //configure the transform according to part (part is not the index of the partorder but the partnumber itself
        Transform configTransform = _usedObject.GetComponent<BaseWrapper>().ConfigurationTransform;

        Vector3 newPos = Vector3.zero;
        Vector3 newTilt = Vector3.zero;
        Vector3 newScale = Vector3.zero;
        switch (part)
        {
            case 1:
                //set to random
                newPos = new Vector3(
                    configTransform.localPosition.x,
                    configTransform.localPosition.y,
                    NextFloat(_distanceMinMax.x, _distanceMinMax.y) * factor
                );
                //set to random
                newTilt = new Vector3(
                    NextFloat(_tiltMinMax.x, _tiltMinMax.y) * factor,
                    configTransform.localEulerAngles.y,
                    configTransform.localEulerAngles.z
                );
                //set to param
                newScale = new Vector3(
                    _sizeParams[fIndex], 
                    configTransform.localScale.y, 
                    _sizeParams[fIndex]
                ); 
                break;
            case 2:
                //set to random
                newPos = new Vector3(
                    configTransform.localPosition.x,
                    configTransform.localPosition.y,
                    NextFloat(_distanceMinMax.x, _distanceMinMax.y) * factor
                );
                //set to param
                newTilt = new Vector3(
                    _tiltParams[fIndex] * factor, 
                    configTransform.localEulerAngles.y, 
                    configTransform.localEulerAngles.z
                );
                float randomSize = NextFloat(_sizeMinMax.x, _sizeMinMax.y);
                //set to random
                newScale = new Vector3(
                    randomSize, 
                    configTransform.localScale.y,
                    randomSize
                );
                break;
            case 3:
                //set to param
                newPos = new Vector3(
                    configTransform.localPosition.x,
                    configTransform.localPosition.y,
                    _distanceParams[fIndex] * factor
                );
                //set to random
                newTilt = new Vector3(
                    NextFloat(_tiltMinMax.x, _tiltMinMax.y) * factor,
                    configTransform.localEulerAngles.y,
                    configTransform.localEulerAngles.z
                );
                float randomSize2 = NextFloat(_sizeMinMax.x, _sizeMinMax.y);
                //set to random
                newScale = new Vector3(
                    randomSize2, 
                    configTransform.localScale.y, 
                    randomSize2
                ); 
                break;
        }

        _usedObject.GetComponent<BaseWrapper>().ConfigurationTransform.localPosition = newPos;
        _usedObject.GetComponent<BaseWrapper>().ConfigurationTransform.localEulerAngles = newTilt;
        _usedObject.GetComponent<BaseWrapper>().ConfigurationTransform.localScale = newScale;

        //set the content type
        if (contentType == ContentType.Icon)
        {
            if (_iconCount == 20 || _iconCount == -1)
            {
                ShuffleExtension.Shuffle(_iconIndex);
                _iconCount = 0;
            }

            usedImage.sprite = _iconSprites[_iconIndex[_iconCount]];
            _currentType = ContentType.Icon;
            _currentContentIndex = _iconIndex[_iconCount];

            _iconCount++;
        }
        else
        {
            if (_pictureCount == 20 || _pictureCount == -1)
            {
                ShuffleExtension.Shuffle(_pictureIndex);
                _pictureCount = 0;
            }

            usedImage.sprite = _pictureSprites[_pictureIndex[_pictureCount]];
            _currentType = ContentType.Picture;
            _currentContentIndex = _pictureIndex[_pictureCount];

            _pictureCount++;
        }

        //set the config information
        ConfigInformation currentConfig;
        currentConfig.size = newScale.x;
        currentConfig.distance = Math.Abs(newPos.z);
        currentConfig.tilt = Math.Abs(newTilt.x);
        currentConfig.location = location;
        currentConfig.contentType = contentType;
        currentConfig.contentIndex = _currentContentIndex;
        CurrentConfig = currentConfig;

        if (location == LocationMarker.LocationName.Ceiling)
            _ceilingObject.SetActive(true);
        else
            _floorObject.SetActive(true);
    }
    
    //updates the Configinformation with the current config
    internal void UpdateCurrentConfig()
    {
        ConfigInformation currentConfig;
        Transform configTransform = _usedObject.GetComponent<BaseWrapper>().ConfigurationTransform;
        currentConfig.size = configTransform.localScale.x;
        currentConfig.distance = Math.Abs(configTransform.localPosition.z);
        currentConfig.tilt = Math.Abs(configTransform.localEulerAngles.x);
        currentConfig.location = _currentLocation;
        currentConfig.contentType = _currentType;
        currentConfig.contentIndex = _currentContentIndex;
        CurrentConfig = currentConfig;
    }

    //random float in range generator
    private float NextFloat(float min, float max)
    {
        double val = (_random.NextDouble() * (max - min) + min);
        return (float)val;
    }

    /// <summary>
    /// sets up the trainingsessions.
    /// </summary>
    /// <param name="traininigID">the index of the sprite to use</param>
    /// <param name="location">location of the training</param>
    public void SetupTraining(int traininigID, LocationMarker.LocationName location)
    {
        Image usedImage;
        float factor = 1f;
        if (location == LocationMarker.LocationName.Ceiling)
        {
            _floorObject.SetActive(false);
            _ceilingObject.SetActive(true);
            _usedObject = _ceilingObject;
            _currentLocation = LocationMarker.LocationName.Ceiling;
            usedImage = _ceilingImage;
        }
        else
        {
            _ceilingObject.SetActive(false);
            _floorObject.SetActive(true);
            factor = -1;
            _usedObject = _floorObject;
            _currentLocation = LocationMarker.LocationName.Floor;
            usedImage = _floorImage;
        }
        Transform configTransform = _usedObject.GetComponent<BaseWrapper>().ConfigurationTransform;
        
        Vector3 newScale = new Vector3(1,1,1); //set to param
        Vector3 newTilt = new Vector3(90 * factor, configTransform.localEulerAngles.y, configTransform.localEulerAngles.z);
        Vector3 newPos = new Vector3(configTransform.localPosition.x, configTransform.localPosition.y, 4.2f * factor);

        _usedObject.GetComponent<BaseWrapper>().ConfigurationTransform.localPosition = newPos;
        _usedObject.GetComponent<BaseWrapper>().ConfigurationTransform.localEulerAngles = newTilt;
        _usedObject.GetComponent<BaseWrapper>().ConfigurationTransform.localScale = newScale;

        usedImage.sprite = _trainingSprites[traininigID];
        _currentType = ContentType.Training;
        _currentContentIndex = -1;

        ConfigInformation currentConfig;
        currentConfig.size = newScale.x;
        currentConfig.distance = Math.Abs(newPos.z);
        currentConfig.tilt = Math.Abs(newTilt.x);
        currentConfig.location = location;
        currentConfig.contentType = _currentType;
        currentConfig.contentIndex = _currentContentIndex;
        CurrentConfig = currentConfig;

        _iconCount = -1;
        _pictureCount = -1;
    }
}
