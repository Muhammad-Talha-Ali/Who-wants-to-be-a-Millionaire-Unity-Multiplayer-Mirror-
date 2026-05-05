using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Question
{
    public int id;
    public string question;
    public string type;
    public Option[] options;
}
