using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneMarker : MonoBehaviour
{
    public enum SceneId
    {
        None = 0,
        Scene1 = 1,
        Scene2 = 2,
        Scene3 = 3
    }

    [SerializeField]
    public SceneId Scene;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
