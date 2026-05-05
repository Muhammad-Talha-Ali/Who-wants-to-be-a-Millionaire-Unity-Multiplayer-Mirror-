using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Match
{
    public int id;
    public new string name;
    public string event_date;
    public string status;
    public int question_per_team;
    public Team[] teams;
}
