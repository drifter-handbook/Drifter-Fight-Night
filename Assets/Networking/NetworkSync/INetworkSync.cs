﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

public interface INetworkSync
{
    string Type { get; }
    int ID { get; set; }

    void Deserialize(INetworkEntityData data);
    INetworkEntityData Serialize();
}

public interface INetworkEntityData
{
    string Type { get; set; }
    int ID { get; set; }
}

public class NetworkEntityConverter : Newtonsoft.Json.Converters.CustomCreationConverter<INetworkEntityData>
{
    public override INetworkEntityData Create(Type objectType)
    {
        throw new NotImplementedException();
    }

    public INetworkEntityData Create(Type objectType, JObject jObject)
    {
        var type = (string)jObject.Property("Type");

        switch (type)
        {
            case "Nero":
            case "Lady Parhelion":
            case "Spacejam":
                return new PlayerSync.PlayerData();
            case "Box":
                return new BoxSync.BoxData();
            case "NeroSpear":
                return new NeroSpearSync.SpearData();
        }
        throw new InvalidOperationException(string.Format("The entity type {0} is not supported!", type));
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        // Load JObject from stream
        JObject jObject = JObject.Load(reader);

        // Create target object based on JObject 
        var target = Create(objectType, jObject);

        // Populate the object properties 
        serializer.Populate(jObject.CreateReader(), target);

        return target;
    }
}