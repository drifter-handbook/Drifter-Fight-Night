using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

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
            case "Ryyke":
            case "Orro":
            case "Megurin":
            case "Swordfrog":
            case "Bojo":
                return new PlayerSync.PlayerData();
            case "Summary":
                return new SummarySync.SummaryData();
            case "Box":
                return new BoxSync.BoxData();
            case "NeroSpear":
                return new NeroSpearSync.SpearData();
            case "SpacejamBell":
                return new SpacejamBellSync.BellData();
            case "Main Camera":
                return new CameraSync.ShakeData();
            case "LaunchRing":
            case "Chadwick":
            case "Megunado":
            case "HoldPerson":
            case "WeakBolt":
            case "StrongBolt":
            case "MegurinStorm":
            case "GuidingBolt":
            case "HaloPlatform":
            case "BeanSpit":
            case "Mockery":
            case "Kamikaze":
            case "Marble":
            case "DairExplosion":
            case "UairExplosion":
            case "Windwave":
            case "Arrow":
                return new BasicProjectileSync.ProjectileData();
            case "ParhelionBolt":
            case "MegurinDairBolt":
                return new ParhelionBoltSync.BoltData();
            case "LongArmOfTheLaw":
                return new LongArmSync.ArmData();
            case "MegurinBar":
                return new BarSync.BarData();
            case "Bean":
                return new BeanSync.BeanData();
            case "MovementParticle":
                return new JuiceSync.JuiceData();
            case "Amber":
                return new OopsiePoospieSync.AmberData();
            case "ChromaticOrb":
                return new ChromaticOrbSync.ChromaticData();
            case "RyykeTombstone":
                return new RyykeTombstoneSync.TombstoneData();
            case "OrroSideW":
                return new OrroSideWSync.FireBallData();
            case "HitSparks":
                return new HitSparksSync.HitSparksData();
            case "DeathExplosion":
                return new DeathExplosionSync.DeathExplosionData();
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