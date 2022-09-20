using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionCopy : MonoBehaviour
{
    [SerializeField]
    private Transform _referenceObject;
    [SerializeField]
    private bool _copyYPosition;
    [SerializeField]
    private GameObject _gizmo;
    [SerializeField]
    private bool _useGizmo;
    private bool _copyPositionFlag = true;

    public bool IsUserBound { get => _copyPositionFlag; set => _copyPositionFlag = value; }
    public Transform ReferenceObject { get => _referenceObject; set => _referenceObject = value; }

    // Start is called before the first frame update
    void Start()
    {
        OnEnable();
    }

    private void OnEnable()
    {
        if (_referenceObject == null)
        {
            _referenceObject = CameraCache.Main.transform;
        }    
    }

    // Update is called once per frame
    void Update()
    {
        if (_copyPositionFlag)
        {
            Vector3 referenceVector;
            if (!_copyYPosition)
            {
                referenceVector = new Vector3(_referenceObject.position.x, this.transform.position.y, _referenceObject.position.z);
            }
            else
            {
                referenceVector = new Vector3(_referenceObject.position.x, _referenceObject.position.y, _referenceObject.position.z);

            }

            this.transform.position = referenceVector;
        }

    }

    [ContextMenu("ToggleStationary")]
    public void TogglePositionCopy()
    {
        _copyPositionFlag = !_copyPositionFlag;
        if (_useGizmo)
        {
            _gizmo.SetActive(!_copyPositionFlag);
        }
    }

    public void SetPositionCopy(bool positionCopy)
    {
        _copyPositionFlag = positionCopy;
        if (_useGizmo)
        {
            _gizmo.SetActive(positionCopy);
        }
    }

}
