using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxColliderFixer : MonoBehaviour
{
    [SerializeField]
    private BaseWrapper _wrapper;
    [SerializeField]
    private GameObject _contentParent;
    private BoxCollider _collider; 

    // Start is called before the first frame update
    void Start()
    {
        _collider = GetComponent<BoxCollider>();
        UpdateCollider();
    }

    private void Update()
    {
        UpdateCollider();
    }
    void UpdateCollider()
    {

        //add vector products and calulation to account for rotation and position change in activeChildTransform
        Transform currentActiveChild = _wrapper.ActiveContent.transform;
        BoxCollider childCollider = currentActiveChild.GetComponent<BoxCollider>();
        _collider.size = Vector3.Scale(childCollider.size , currentActiveChild.transform.localScale);
       
        //only for needed rotation for plane. Find better way to calculate this
        if (currentActiveChild.localRotation.eulerAngles.x != 0)
        {
            Vector3 size = new Vector3(_collider.size.x, _collider.size.z, _collider.size.y);
            _collider.size = size;
        }
        if (currentActiveChild.transform.localPosition == Vector3.zero)
            _collider.center = childCollider.center;
        else if (currentActiveChild.transform.localPosition.x==0&& currentActiveChild.transform.localPosition.z == 0)
        {
            _collider.center = new Vector3(_collider.size.x/2f,_collider.size.y,0);
        }
        else
            _collider.center = currentActiveChild.transform.localPosition;
        childCollider.enabled=false;
    }
}
