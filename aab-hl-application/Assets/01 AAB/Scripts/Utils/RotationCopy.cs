using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationCopy : MonoBehaviour
{
    [SerializeField]
    private Transform _targetObject;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.localRotation = Quaternion.Euler(0,_targetObject.localRotation.eulerAngles.y,0);
    }
}
