using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Team
{
    public int id;
    public string name;
    public string logo;
    public int code;
    public Question[] questions;
    public int points;
    public int questionsNumber;

    public bool expertLifeline = true;
    public bool audienceLifeline = true;
    public bool swapLifeline = true;
    public bool halfLifeline = true;

    [NonSerialized]
    public Sprite logoSprite;
    [NonSerialized]
    public byte[] spriteData;
    public bool logoReady = false;

    public float answerTime;

    public static Team CreateCopyFrom(Team team)
    {
        Team newTeam = new Team();
        newTeam.id = team.id;
        newTeam.name = team.name;
        newTeam.logo = team.logo;
        newTeam.code = team.code;
        newTeam.questions = team.questions;
        newTeam.points = team.points;
        newTeam.questionsNumber = team.questionsNumber;
        newTeam.expertLifeline = team.expertLifeline;
        newTeam.audienceLifeline = team.audienceLifeline;
        newTeam.swapLifeline = team.swapLifeline;
        newTeam.halfLifeline = team.halfLifeline;
        newTeam.logoSprite = team.logoSprite;
        newTeam.logoReady = team.logoReady;
        newTeam.answerTime = team.answerTime;
        return newTeam;
    }
}
