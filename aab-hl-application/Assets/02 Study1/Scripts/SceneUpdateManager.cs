using MasterNetworking.EventHandling;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wrappers;

public class SceneUpdateManager : MonoBehaviour
{
    public GameObject ContentRoot;

    public ContentProperty Scene1ContentProperty = new ContentProperty();
    private SceneMarker.SceneId _activeScene = SceneMarker.SceneId.Scene1;
    private Dictionary<SceneMarker.SceneId, List<SceneMarker>> _scenes = new Dictionary<SceneMarker.SceneId, List<SceneMarker>>();
    private List<BaseWrapper> _scene1ContentsWrapper = new List<BaseWrapper>();
    void Start()
    {
        List<SceneMarker> sceneObjects = ContentRoot.GetComponentsInChildren<SceneMarker>(includeInactive: true).ToList();
        foreach (SceneMarker scene in sceneObjects)
        {
            if (!_scenes.ContainsKey(scene.Scene))
                _scenes.Add(scene.Scene, new List<SceneMarker>());
            _scenes[scene.Scene].Add(scene);
            scene.gameObject.SetActive(false);
        }

        foreach (SceneMarker scene in _scenes[SceneMarker.SceneId.Scene1])
            _scene1ContentsWrapper.AddRange(scene.GetComponentsInChildren<BaseWrapper>(includeInactive: true).ToList());

        SetScenesActive(_activeScene);
        JsonRpcHandler.Instance.AddNotificationDelegate(rpcEvent: "SceneUpdateEvent", notification: UpdateScene);
        ConfigureScene1Content();
    }

    private void UpdateScene(JsonRpcMessage message)
    {
        Scene1ContentProperty.SetSceneValues(message);

        SceneMarker.SceneId newScene = SceneMarker.SceneId.None;
        if (message.Data["scene"] != null && message.Data["scene"]?.Type != JTokenType.Null)
        {
            switch (message.Data["scene"].ToObject<string>())
            {
                case "scene1": newScene = SceneMarker.SceneId.Scene1; break;
                case "scene2": newScene = SceneMarker.SceneId.Scene2; break;
                case "scene3": newScene = SceneMarker.SceneId.Scene3; break;
                default: newScene = SceneMarker.SceneId.None; break;
            }
        }

        if (newScene != SceneMarker.SceneId.None)
            SetScenesActive(newScene);


        if (_activeScene == SceneMarker.SceneId.Scene1)
            ConfigureScene1Content();
    }

    private void ConfigureScene1Content()
    {
        foreach(var wrapper in _scene1ContentsWrapper)
        {
            if (wrapper.Location == Scene1ContentProperty.Location && wrapper.ContentType == Scene1ContentProperty.ContentType)
            {
                wrapper.gameObject.SetActive(true);
                wrapper.ContentProperty = Scene1ContentProperty;
            }
            else
            {
                wrapper.gameObject.SetActive(false);
                wrapper.ContentProperty = null;
            }
        }
    }

    private void SetScenesActive(SceneMarker.SceneId activeScene)
    {
        if (activeScene == SceneMarker.SceneId.None)
            return;
        _activeScene = activeScene;

        foreach (KeyValuePair<SceneMarker.SceneId, List<SceneMarker>> pair in _scenes)
            foreach (SceneMarker scene in pair.Value)
                scene.gameObject.SetActive(scene.Scene == activeScene);
    }
}
