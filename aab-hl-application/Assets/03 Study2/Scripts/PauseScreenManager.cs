using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseScreenManager : MonoBehaviour
{
    [SerializeField]
    private GameObject _QRScan;
    [SerializeField]
    private GameObject _HUD;
    [SerializeField]
    private GameObject _startScreen;
    [SerializeField]
    private GameObject _break1Screen;
    [SerializeField]
    private GameObject _break2Screen;
    [SerializeField]
    private GameObject _endScreen;
    [SerializeField]
    private List<GameObject> _deactivateWhilePause;


    public void StartPause(string pauseType)
    {
        foreach(var obj in _deactivateWhilePause)
        {
            obj.SetActive(false);
        }
        _HUD.SetActive(true);
        switch (pauseType)
        {
            case "StartScreen":
                _startScreen.SetActive(true);
                break;
            case "BreakScreen1":
                _break1Screen.SetActive(true);
                break;
            case "BreakScreen2":
                _break2Screen.SetActive(true);
                break;
            case "EndScreen":
                _endScreen.SetActive(true);
                break;
        }
    }

    public void StopPause()
    {
        foreach (var obj in _deactivateWhilePause)
        {
            obj.SetActive(true);
        }

        _QRScan.SetActive(false);
        _HUD.SetActive(false);
        _startScreen.SetActive(false);
        _break1Screen.SetActive(false);
        _break2Screen.SetActive(false);
        _endScreen.SetActive(false);
    }

    public void StopQRScreen()
    {
        _QRScan.SetActive(false);
    }
}
