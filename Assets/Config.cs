using NUnit.Framework;
using System;
using System.Collections.Generic;

[System.Serializable]
public class Config
{
    public string name;
    public List<string> variables;
    public List<string> constants;
    public string axiom;
    public List<ProductionRules> rules;
    public List<Commands> commands;
    public int n;
    public int lineWidth;
}


[System.Serializable]
public struct ProductionRules
{
    public string key;
    public string value;
}

[System.Serializable]
public struct Commands
{
    public string key;
    public CommandType type;
}

[System.Serializable]
public class CommandType
{
    public TYPES  type;
    public float amount;

    public enum TYPES
    {
        DRAW,     
        TURN,
        POP,
        PUSH,
        PITCH,
        ROLL,
        HORIZONTAL
    }
}
