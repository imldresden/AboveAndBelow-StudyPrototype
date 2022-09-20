using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationMarker : MonoBehaviour
{
    public enum LocationName
    {
        Ceiling = 0,
        Floor = 1
    }

    // Start is called before the first frame update
    [SerializeField]
    private LocationName _markedLocation;

    public LocationName MarkedLocation { get => _markedLocation; set => _markedLocation = value; }

    void Start()
    {

    }
}
