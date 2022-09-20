using MasterNetworking.EventHandling;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Wrappers
{

    public enum ReferenceFrame
    {
        Ego = 0,
        Exo = 1
    }

    public enum ContentType
    {
        Id1 = 1,
        Id2 = 2,
        Id3 = 3,
        Id4 = 4,
        Id5 = 5,
        Id6 = 6,
        Email = 7,
        Recipe = 8,
        Navigation = 9
    }

    public class ContentProperty
    {
        public bool PlacementEmpty = true;

        public float Distance { get; private set; } = 2;
        public float HeightAddition { get; private set; } = 0;
        public float PosX { get; private set; } = 0;
        public float Tilt { get; private set; } = 55;
        public float EgoRotation { get; private set; } = 0;
        public float Yaw { get; private set; } = 0;
        public float Size { get; private set; } = 1;


        public LocationMarker.LocationName Location { get; private set; } = LocationMarker.LocationName.Ceiling;
        public ReferenceFrame ReferenceFrame { get; private set; } = ReferenceFrame.Exo;
        public ContentType ContentType { get; private set; } = ContentType.Id1;

        public void InitLocal(float distance,float heightAddition,float posx,float tilt,float egorot,float yaw, float size, LocationMarker.LocationName location, ReferenceFrame referenceFrame, ContentType type)
        {
            Distance = distance;
            HeightAddition = heightAddition;
            PosX = posx;
            Tilt = tilt;
            EgoRotation = egorot;
            Yaw = yaw;
            Size = size;
            Location = location;
            ReferenceFrame = referenceFrame;
            ContentType = type;
        }

        public void SetSceneValues(JsonRpcMessage message)
        {
            if (message.Data["referenceFrame"] != null && message.Data["referenceFrame"]?.Type != JTokenType.Null)
                switch (message.Data["referenceFrame"].ToObject<string>())
                {
                    case "egocentric":  ReferenceFrame = ReferenceFrame.Ego;    break;
                    case "exocentric":  ReferenceFrame = ReferenceFrame.Exo;    break;
                }

            switch (message.Data["placementArea"].ToObject<string>())
            {
                case "ceiling": Location = LocationMarker.LocationName.Ceiling;    break;
                case "floor":   Location = LocationMarker.LocationName.Floor;      break;
            }


            switch (message.Data["contentType"].ToObject<string>())
            {
                case "content1":    ContentType = ContentType.Id1;  break;
                case "content2":    ContentType = ContentType.Id2;  break;
                case "content3":    ContentType = ContentType.Id3;  break;
                case "content4":    ContentType = ContentType.Id4;  break;
                case "content5":    ContentType = ContentType.Id5;  break;
                case "content6":    ContentType = ContentType.Id6;  break;
            }
        }

        public void SetTransformationValues(JsonRpcMessage message)
        {
            Debug.Log(message.Data["tilt"]);
            if (message.Data["distance"] != null && message.Data["distance"]?.Type != JTokenType.Null)
                Distance = message.Data["distance"].ToObject<float>();
            if (message.Data["heightAddition"] != null && message.Data["heightAddition"]?.Type != JTokenType.Null)
                HeightAddition = message.Data["heightAddition"].ToObject<float>();
            if (message.Data["posX"] != null && message.Data["posX"]?.Type != JTokenType.Null)
                PosX = message.Data["posX"].ToObject<float>();
            if (message.Data["tilt"] != null && message.Data["tilt"]?.Type != JTokenType.Null)
                Tilt = message.Data["tilt"].ToObject<float>();
            if (message.Data["egoRotation"] != null && message.Data["egoRotation"]?.Type != JTokenType.Null)
                EgoRotation = message.Data["egoRotation"].ToObject<float>();
            if (message.Data["yaw"] != null && message.Data["yaw"]?.Type != JTokenType.Null)
                Yaw = message.Data["yaw"].ToObject<float>();
            if (message.Data["size"] != null && message.Data["size"]?.Type != JTokenType.Null)
                Size = message.Data["size"].ToObject<float>();

            PlacementEmpty = false;
        }

        public void SetTransformationValuesInternal(float distance,float heightAddition,float posX, float tilt, float egoRotation, float yaw, float size)
        {
            Distance = distance;
            HeightAddition =heightAddition;
            PosX = posX;
            Tilt = tilt;
            EgoRotation = egoRotation;
            Yaw = yaw;
            Size = size;
            PlacementEmpty = false;
        }
    }
}
