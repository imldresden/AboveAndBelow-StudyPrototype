using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class StringsAndBlocks : MonoBehaviour
{
    public enum Connections
    {
        strings,
        block
    }
    [SerializeField]
    private BaseWrapper _wrapper;
    [SerializeField]
    private bool _drawStringsOrBlocks;
    [SerializeField]
    private Connections _connectionType;
    [SerializeField]
    private Vector2 _topLeftIcon;
    [SerializeField]
    private Vector2 _topRightIcon;
    [SerializeField]
    private Vector2 _bottomLeftIcon;
    [SerializeField]
    private Vector2 _bottomRightIcon;
    [SerializeField]
    private Vector2 _topLeftPreview;
    [SerializeField]
    private Vector2 _topRightPreview;
    [SerializeField]
    private Vector2 _bottomLeftPreview;
    [SerializeField]
    private Vector2 _bottomRightPreview;
    [SerializeField]
    private Vector2 _topLeftFullview;
    [SerializeField]
    private Vector2 _topRightFullview;
    [SerializeField]
    private Vector2 _bottomLeftFullview;
    [SerializeField]
    private Vector2 _bottomRightFullview;
    [SerializeField][Tooltip("set to true, if the block location should not be calculated from the string location, but taken from the offset")]
    private bool _useOffsetInsted;
    [SerializeField]
    [Tooltip("Marks the offset from the connecting edge")]
    private float _offsetOfBlock;
    [SerializeField]
    private GameObject _stringPrefab;
    [SerializeField]
    private GameObject _blockPrefab;


    private Vector2 _topLeft;
    private Vector2 _topRight;
    private Vector2 _bottomLeft;
    private Vector2 _bottomRight;
    GameObject _stringFL;
    GameObject _stringFR;
    GameObject _stringBL;
    GameObject _stringBR;
    GameObject _block;

    Mesh originalCubeMesh;

    public Connections ConnectionType { get => _connectionType; set => _connectionType = value; }
    public bool DrawStringsOrBlocks { get => _drawStringsOrBlocks; set => _drawStringsOrBlocks = value; }
    public bool UseOffsetInsted { get => _useOffsetInsted; set => _useOffsetInsted = value; }
    public float OffsetOfBlock { get => _offsetOfBlock; set => _offsetOfBlock = value; }
    public Vector2 TopLeftIcon { get => _topLeftIcon; set => _topLeftIcon = value; }
    public Vector2 TopRightIcon { get => _topRightIcon; set => _topRightIcon = value; }
    public Vector2 BottomLeftIcon { get => _bottomLeftIcon; set => _bottomLeftIcon = value; }
    public Vector2 BottomRightIcon { get => _bottomRightIcon; set => _bottomRightIcon = value; }
    public Vector2 TopLeftPreview { get => _topLeftPreview; set => _topLeftPreview = value; }
    public Vector2 TopRightPreview { get => _topRightPreview; set => _topRightPreview = value; }
    public Vector2 BottomLeftPreview { get => _bottomLeftPreview; set => _bottomLeftPreview = value; }
    public Vector2 BottomRightPreview { get => _bottomRightPreview; set => _bottomRightPreview = value; }
    public Vector2 TopLeftFullview { get => _topLeftFullview; set => _topLeftFullview = value; }
    public Vector2 TopRightFullview { get => _topRightFullview; set => _topRightFullview = value; }
    public Vector2 BottomLeftFullview { get => _bottomLeftFullview; set => _bottomLeftFullview = value; }
    public Vector2 BottomRightFullview { get => _bottomRightFullview; set => _bottomRightFullview = value; }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        switch (_wrapper.StateOfView)
        {
            case BaseWrapper.State.Icon:
                _topLeft = _topLeftIcon;
                _topRight = _topRightIcon;
                _bottomLeft = _bottomLeftIcon;
                _bottomRight = _bottomRightIcon;
                break;
            case BaseWrapper.State.Preview:
                _topLeft = _topLeftPreview;
                _topRight = _topRightPreview;
                _bottomLeft = _bottomLeftPreview;
                _bottomRight = _bottomRightPreview;
                break;
            case BaseWrapper.State.Fullview:
                _topLeft = _topLeftFullview;
                _topRight = _topRightFullview;
                _bottomLeft = _bottomLeftFullview;
                _bottomRight = _bottomRightFullview;
                break;
            default:
                break;
        }
        if (_drawStringsOrBlocks && !_wrapper.IsInHUD)
            DrawStringsOrBlock();
        if (_wrapper.IsInHUD || !_drawStringsOrBlocks)
        {
            DeleteStringsAndBlocks();
        }
    }

    private void DeleteStringsAndBlocks()
    {
        List<StringOrBlockMarker> connectors = GetComponentsInChildren<StringOrBlockMarker>().ToList();
        foreach(var connector in connectors)
        {
            Destroy(connector.gameObject);
        }
    }

    private void DrawStringsOrBlock()
    {
        InstantiateStringsOrBlock();
        if (_wrapper.Location == LocationMarker.LocationName.Ceiling)
            DrawAtCeiling();
        if (_wrapper.Location == LocationMarker.LocationName.Floor)
            DrawAtFloor();
    }

    private void InstantiateStringsOrBlock()
    {
        Transform parentToBe = _wrapper.ConfigurationTransform.GetChild(0);
        switch (_connectionType)
        {
            case Connections.strings:
                if (_block != null)
                {
                    Destroy(_block);
                }
                if (_stringBL == null || _stringBR == null || _stringFL == null || _stringFR == null)
                {
                    GameObject[] Strings = { _stringBL, _stringBR, _stringFL, _stringFR };
                    foreach (var str in Strings)
                    {
                        if (str != null)
                            Destroy(str);
                    }

                    
                    _stringFL = Instantiate(_stringPrefab, parentToBe, worldPositionStays: false);
                    _stringBL = Instantiate(_stringPrefab, parentToBe, worldPositionStays: false);
                    _stringFR = Instantiate(_stringPrefab, parentToBe, worldPositionStays: false);
                    _stringBR = Instantiate(_stringPrefab, parentToBe, worldPositionStays: false);
                }
                break;
            case Connections.block:
                if (_stringBL != null || _stringBR != null || _stringFL != null || _stringFR != null)
                {
                    GameObject[] Strings = { _stringBL, _stringBR, _stringFL, _stringFR };
                    foreach (var str in Strings)
                    {
                        if (str != null)
                            Destroy(str);
                    }
                }
                if (_block == null)
                {
                    _block = Instantiate(_blockPrefab, parentToBe, worldPositionStays: false);
                }
                break;
        }
    }

    private void DrawAtFloor()
    {
        float angleWithNegatives = _wrapper.ConfigurationTransform.localRotation.eulerAngles.x - _wrapper.AngleChangeInteractionScript.CurrentAngle;
        if (_wrapper.ConfigurationTransform.localRotation.eulerAngles.x > 180)
        {
            angleWithNegatives = 360 - (360 - _wrapper.ConfigurationTransform.localRotation.eulerAngles.x - _wrapper.AngleChangeInteractionScript.CurrentAngle);
        }

        float heightLeft = ((_topLeft.y - _bottomLeft.y) * (float)Math.Sin((Math.PI / 180) * (angleWithNegatives)));
        float heightRight = ((_topRight.y - _bottomRight.y) * (float)Math.Sin((Math.PI / 180) * (angleWithNegatives)));
        float heightBlock = (((((_topLeft.y - _bottomLeft.y) / 2f)+((_topRight.y-_bottomRight.y)/2f) )/2f) * (float)Math.Sin((Math.PI / 180) * (angleWithNegatives)));
        if (_useOffsetInsted)
            heightBlock = -1*_offsetOfBlock * (float)Math.Sin((Math.PI / 180) * (angleWithNegatives));
 //       Debug.Log("wrapperRot: " + _wrapper.ConfigurationTransform.localRotation.eulerAngles.x + " | AngleInteractionAngle: " + _wrapper.AngleChangeInteractionScript.CurrentAngle + " | Sum: " + _wrapper.ConfigurationTransform.localRotation.eulerAngles.x + _wrapper.AngleChangeInteractionScript.CurrentAngle + " | Sin: " + Math.Sin(_wrapper.ConfigurationTransform.localRotation.eulerAngles.x + _wrapper.AngleChangeInteractionScript.CurrentAngle));
        //Vector3 relativeLocationBackLeft = new Vector3(_bottomLeft.x, -heightLeft, (float)Math.Sqrt(((_bottomLeft.y - _topLeft.y) * (_bottomLeft.y - _topLeft.y)) - (heightLeft * heightLeft)));
        Vector3 relativeLocationBackLeft = new Vector3(_topLeft.x, 0, _topLeft.y);
        //Vector3 relativeLocationBackRight = new Vector3(_bottomRight.x, -heightRight, (float)Math.Sqrt(((_bottomRight.y - _topRight.y) * (_bottomRight.y - _topRight.y)) - (heightRight * heightRight)));
        Vector3 relativeLocationBackRight = new Vector3(_topRight.x, 0, _topRight.y);
        

        if (_connectionType == Connections.strings)
        {
            _stringFL.transform.localPosition = new Vector3(_bottomLeft.x, 0, _bottomLeft.y);
            _stringFL.transform.localScale = new Vector3(1, Math.Abs(_wrapper.ConfigurationTransform.localPosition.y), 1);
            _stringFL.transform.localRotation = Quaternion.Euler(-(angleWithNegatives), 0, 0);


            // _stringBL.transform.localPosition = new Vector3(relativeLocationBackLeft.x,-(relativeLocationBackLeft.y+ Math.Abs(_wrapper.ConfigurationTransform.localPosition.y)),relativeLocationBackLeft.z);
            _stringBL.transform.localPosition = relativeLocationBackLeft;
            Debug.Log("Floor: "+relativeLocationBackLeft);
            _stringBL.transform.localScale = new Vector3(1, heightLeft + Math.Abs(_wrapper.ConfigurationTransform.localPosition.y), 1);
            _stringBL.transform.localRotation = Quaternion.Euler(-(angleWithNegatives), 0, 0);

            _stringFR.transform.localPosition = new Vector3(_bottomRight.x, 0, _bottomRight.y);
            _stringFR.transform.localScale = new Vector3(1, Math.Abs(_wrapper.ConfigurationTransform.localPosition.y), 1);
            _stringFR.transform.localRotation = Quaternion.Euler(-(angleWithNegatives), 0, 0);

            // _stringBR.transform.localPosition = new Vector3(relativeLocationBackRight.x, -( relativeLocationBackRight.y + Math.Abs(_wrapper.ConfigurationTransform.localPosition.y)), relativeLocationBackRight.z);
            _stringBR.transform.localPosition = relativeLocationBackRight;
            _stringBR.transform.localScale = new Vector3(1, heightRight + Math.Abs(_wrapper.ConfigurationTransform.localPosition.y), 1);
            _stringBR.transform.localRotation = Quaternion.Euler(-(angleWithNegatives), 0, 0);
            
            GameObject[] Strings = { _stringBL, _stringBR, _stringFL, _stringFR };
            foreach (var str in Strings)
            {
                if (Math.Abs(str.transform.localScale.y) < 0.0001)
                {
                    str.SetActive(false);
                }
                else
                {
                    str.SetActive(true);
                }
                    
            }
        }

        if (_connectionType == Connections.block)
        {
            _block.transform.localPosition = new Vector3((_topLeft.x+ _topRight.x)/2f, 0.0001f,( _topLeft.y + _bottomLeft.y)/2f);
            if (_useOffsetInsted)
                _block.transform.localPosition = new Vector3(0, 0.0001f, -1*_offsetOfBlock);
            _block.transform.localScale = new Vector3(1, heightBlock + Math.Abs(_wrapper.ConfigurationTransform.localPosition.y), 1);
            _block.transform.localRotation = Quaternion.Euler(-(angleWithNegatives), 0, 0);
            Mesh blockmesh = new Mesh();
            
            Mesh cube = _block.GetComponentInChildren<MeshFilter>().sharedMesh;
            if (originalCubeMesh == null)
            {
                originalCubeMesh = cube;
            }
            blockmesh.vertices = cube.vertices;
            blockmesh.triangles = cube.triangles;
            blockmesh.normals = cube.normals;
            blockmesh.uv = cube.uv;
            var verts = originalCubeMesh.vertices;
            //Debug.Log(verts[2]);
            float alpha = angleWithNegatives;
            float a = -(float)Math.Tan((Math.PI / 180f) * alpha) * (0.5f * _block.GetComponentInChildren<MeshFilter>().transform.localScale.z / Math.Abs(_block.transform.localScale.y));
            //A
            verts[0] = new Vector3(blockmesh.vertices[0].x,verts[0].y + a , verts[0].z);
            verts[13] = new Vector3(blockmesh.vertices[13].x,verts[13].y + a , verts[13].z);
            verts[23] = new Vector3(blockmesh.vertices[23].x,verts[23].y + a , verts[23].z);
            //B
            verts[1] = new Vector3(blockmesh.vertices[1].x,verts[1].y + a , verts[1].z);
            verts[14] = new Vector3(blockmesh.vertices[14].x,verts[14].y + a , verts[14].z);
            verts[16] = new Vector3(blockmesh.vertices[16].x,verts[16].y + a , verts[16].z);
            //C
            verts[6] = new Vector3(blockmesh.vertices[6].x, verts[6].y - a , verts[6].z);
            verts[12] = new Vector3(blockmesh.vertices[12].x, verts[12].y - a , verts[12].z);
            verts[20] = new Vector3(blockmesh.vertices[20].x, verts[20].y - a , verts[20].z);
            //D
            verts[7] = new Vector3(blockmesh.vertices[7].x, verts[7].y - a , verts[7].z);
            verts[15] = new Vector3(blockmesh.vertices[15].x, verts[15].y - a , verts[15].z);
            verts[19] = new Vector3(blockmesh.vertices[19].x, verts[19].y - a , verts[19].z);


            blockmesh.vertices = verts;
            _block.GetComponentInChildren<MeshFilter>().mesh = blockmesh;
            if (Math.Abs(_block.transform.localScale.y) < 0.001)
            {
                _block.SetActive(false);
            }
            else
            {
                _block.SetActive(true);
            }
            _block.GetComponentInChildren<MeshCollider>().sharedMesh = blockmesh;
        }
    }

    private void DrawAtCeiling()
    {
        float angleWithNegatives = _wrapper.ConfigurationTransform.localRotation.eulerAngles.x + _wrapper.AngleChangeInteractionScript.CurrentAngle;
        if (_wrapper.ConfigurationTransform.localRotation.eulerAngles.x > 180)
        {
            angleWithNegatives = 360 - (360 - _wrapper.ConfigurationTransform.localRotation.eulerAngles.x + _wrapper.AngleChangeInteractionScript.CurrentAngle);
        }

        float heightLeft = ((_bottomLeft.y - _topLeft.y) * (float)Math.Sin((Math.PI / 180) * (_wrapper.ConfigurationTransform.localRotation.eulerAngles.x + _wrapper.AngleChangeInteractionScript.CurrentAngle)));
        float heightRight = ((_bottomRight.y - _topRight.y) * (float)Math.Sin((Math.PI / 180) * (_wrapper.ConfigurationTransform.localRotation.eulerAngles.x + _wrapper.AngleChangeInteractionScript.CurrentAngle)));
        float heightBlock = (((((_topLeft.y - _bottomLeft.y) / 2f) + ((_topRight.y - _bottomRight.y) / 2f)) / 2f) * (float)Math.Sin((Math.PI / 180) * (angleWithNegatives)));
        if (_useOffsetInsted)
            heightBlock = _offsetOfBlock * (float)Math.Sin((Math.PI / 180) * (angleWithNegatives));
        //Debug.Log("wrapperRot: " + _wrapper.ConfigurationTransform.localRotation.eulerAngles.x + " | AngleInteractionAngle: " + _wrapper.AngleChangeInteractionScript.CurrentAngle + " | Sum: " + _wrapper.ConfigurationTransform.localRotation.eulerAngles.x + _wrapper.AngleChangeInteractionScript.CurrentAngle + " | Sin: " + Math.Sin(_wrapper.ConfigurationTransform.localRotation.eulerAngles.x + _wrapper.AngleChangeInteractionScript.CurrentAngle));
        //Vector3 relativeLocationBackLeft = new Vector3(_bottomLeft.x, -heightLeft, (float)Math.Sqrt(((_bottomLeft.y - _topLeft.y) * (_bottomLeft.y - _topLeft.y)) - (heightLeft * heightLeft)));
        Vector3 relativeLocationBackLeft = new Vector3(_bottomLeft.x, 0, _bottomLeft.y);
        //Vector3 relativeLocationBackRight = new Vector3(_bottomRight.x, -heightRight, (float)Math.Sqrt(((_bottomRight.y - _topRight.y) * (_bottomRight.y - _topRight.y)) - (heightRight * heightRight)));
        Vector3 relativeLocationBackRight = new Vector3(_bottomRight.x, 0, _bottomRight.y);


        if (_connectionType == Connections.strings)
        {
            _stringFL.transform.localPosition = new Vector3(_topLeft.x, 0, _topLeft.y);
            _stringFL.transform.localScale = new Vector3(1, Math.Abs(_wrapper.ConfigurationTransform.localPosition.y), 1);
            _stringFL.transform.localRotation = Quaternion.Euler( - (_wrapper.ConfigurationTransform.localRotation.eulerAngles.x + _wrapper.AngleChangeInteractionScript.CurrentAngle), 0, 0);

            // _stringBL.transform.localPosition = new Vector3(relativeLocationBackLeft.x,-(relativeLocationBackLeft.y+ Math.Abs(_wrapper.ConfigurationTransform.localPosition.y)),relativeLocationBackLeft.z);
            _stringBL.transform.localPosition = relativeLocationBackLeft;
            _stringBL.transform.localScale = new Vector3(1, heightLeft+ Math.Abs(_wrapper.ConfigurationTransform.localPosition.y), 1);
            _stringBL.transform.localRotation = Quaternion.Euler( - (_wrapper.ConfigurationTransform.localRotation.eulerAngles.x + _wrapper.AngleChangeInteractionScript.CurrentAngle), 0, 0);

            _stringFR.transform.localPosition = new Vector3(_topRight.x, 0, _topRight.y);
            _stringFR.transform.localScale = new Vector3(1, Math.Abs(_wrapper.ConfigurationTransform.localPosition.y), 1);
            _stringFR.transform.localRotation = Quaternion.Euler( - (_wrapper.ConfigurationTransform.localRotation.eulerAngles.x + _wrapper.AngleChangeInteractionScript.CurrentAngle), 0, 0);

            // _stringBR.transform.localPosition = new Vector3(relativeLocationBackRight.x, -( relativeLocationBackRight.y + Math.Abs(_wrapper.ConfigurationTransform.localPosition.y)), relativeLocationBackRight.z);
            _stringBR.transform.localPosition = relativeLocationBackRight;
            _stringBR.transform.localScale = new Vector3(1, heightRight + Math.Abs(_wrapper.ConfigurationTransform.localPosition.y), 1);
            _stringBR.transform.localRotation = Quaternion.Euler( - (_wrapper.ConfigurationTransform.localRotation.eulerAngles.x + _wrapper.AngleChangeInteractionScript.CurrentAngle), 0, 0);
            
            GameObject[] Strings = { _stringBL, _stringBR, _stringFL, _stringFR };
            foreach (var str in Strings)
            {
                if (Math.Abs(str.transform.localScale.y) < 0.0001)
                {
                    str.SetActive(false);
                }
                else
                {
                    str.SetActive(true);
                }

            }
        }

        if (_connectionType == Connections.block)
        {
            _block.transform.localPosition = new Vector3((_topLeft.x + _topRight.x) / 2f, 0.0001f, (_topLeft.y + _bottomLeft.y) / 2f);
            if (_useOffsetInsted)
                _block.transform.localPosition = new Vector3(0, 0.0001f, _offsetOfBlock);
            _block.transform.localScale = new Vector3(1, heightBlock - Math.Abs(_wrapper.ConfigurationTransform.localPosition.y), 1);
            _block.transform.localRotation = Quaternion.Euler(180 - (angleWithNegatives), 0, 0);
            Mesh blockmesh = new Mesh();

            Mesh cube = _block.GetComponentInChildren<MeshFilter>().sharedMesh;
            if (originalCubeMesh == null)
            {
                originalCubeMesh = cube;
            }
            blockmesh.vertices = cube.vertices;
            blockmesh.triangles = cube.triangles;
            blockmesh.normals = cube.normals;
            blockmesh.uv = cube.uv;
            var verts = originalCubeMesh.vertices;
            //Debug.Log(verts[2]);
            // float xScaleCube = _blok
            float alpha = angleWithNegatives;
            
            float a = (float)Math.Tan((Math.PI / 180f) * alpha) * (0.5f*_block.GetComponentInChildren<MeshFilter>().transform.localScale.z/Math.Abs(_block.transform.localScale.y));
            //a = 0;
            //Debug.Log("alpha:"+alpha + " a: " +a);
            //Debug.Log("Wrapper angle:"+ _wrapper.ConfigurationTransform.localRotation.eulerAngles.x + " Current Angle" + _wrapper.AngleChangeInteractionScript.CurrentAngle);
            //A
            verts[0] = new Vector3(blockmesh.vertices[0].x, verts[0].y + a, verts[0].z);
            verts[13] = new Vector3(blockmesh.vertices[13].x, verts[13].y + a, verts[13].z);
            verts[23] = new Vector3(blockmesh.vertices[23].x, verts[23].y + a, verts[23].z);
            //B
            verts[1] = new Vector3(blockmesh.vertices[1].x, verts[1].y + a, verts[1].z);
            verts[14] = new Vector3(blockmesh.vertices[14].x, verts[14].y + a, verts[14].z);
            verts[16] = new Vector3(blockmesh.vertices[16].x, verts[16].y + a, verts[16].z);
            //C
            verts[6] = new Vector3(blockmesh.vertices[6].x, verts[6].y - a, verts[6].z);
            verts[12] = new Vector3(blockmesh.vertices[12].x, verts[12].y - a, verts[12].z);
            verts[20] = new Vector3(blockmesh.vertices[20].x, verts[20].y - a, verts[20].z);
            //D
            verts[7] = new Vector3(blockmesh.vertices[7].x, verts[7].y - a, verts[7].z);
            verts[15] = new Vector3(blockmesh.vertices[15].x, verts[15].y - a, verts[15].z);
            verts[19] = new Vector3(blockmesh.vertices[19].x, verts[19].y - a, verts[19].z);
            //Debug.Log("A:" + verts[0]);
            //Debug.Log("B:" + verts[1]);
            //Debug.Log("c:" + verts[6]);
            //Debug.Log("D:" + verts[7]);

            blockmesh.vertices = verts;
            _block.GetComponentInChildren<MeshFilter>().mesh = blockmesh;
            if (Math.Abs(_block.transform.localScale.y) < 0.001)
            {
                _block.SetActive(false);
            }
            else
            {
                _block.SetActive(true);
            }
            _block.GetComponentInChildren<MeshCollider>().sharedMesh = blockmesh;
        }
    }
}
