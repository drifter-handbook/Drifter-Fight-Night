using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;

public class PlayerInputData : ICloneable
{
    public float MoveX = 0;
    public float MoveY = 0;
    public bool Jump = false;
    public bool Light = false;
    public bool Special = false;
    public bool Grab = false;
    public bool Guard = false;

    public object Clone()
    {
        return new PlayerInputData()
        {
            MoveX = MoveX,
            MoveY = MoveY,
            Jump = Jump,
            Light = Light,
            Special = Special,
            Grab = Grab,
            Guard = Guard
        };
    }

    public void CopyFrom(PlayerInputData data)
    {
        MoveX = data.MoveX;
        MoveY = data.MoveY;
        Jump = data.Jump;
        Light = data.Light;
        Special = data.Special;
        Grab = data.Grab;
        Guard = data.Guard;
    }
}