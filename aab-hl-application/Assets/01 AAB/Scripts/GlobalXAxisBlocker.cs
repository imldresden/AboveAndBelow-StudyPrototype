using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalXAxisBlocker : MonoBehaviour
{
    [SerializeField]
    private float _angle = 0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //transform.RotateAroundLocal
        Debug.Log(transform.rotation.eulerAngles);
    }
}
