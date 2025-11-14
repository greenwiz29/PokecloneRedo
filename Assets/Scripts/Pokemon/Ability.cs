using System;

public class Ability
{
    public string Name {get; set;}
    public string Description {get;set;}
    public Func<float, Pokemon, Pokemon, Move, float> OnModifyAtk {get;set;}
    public Func<float, Pokemon, Pokemon, Move, float> OnModifySpAtk {get;set;}
    public Func<float, Pokemon, Pokemon, Move, float> OnModifyDef {get;set;}
    public Func<float, Pokemon, Pokemon, Move, float> OnModifySpDef {get;set;}
    public Func<float, Pokemon, Pokemon, Move, float> OnModifySpd {get;set;}
    public Func<float, Pokemon, Pokemon, Move, float> OnModifyAcc {get;set;}
    public Func<float, Pokemon, Pokemon, Move, float> OnModifyEva {get;set;}
}
