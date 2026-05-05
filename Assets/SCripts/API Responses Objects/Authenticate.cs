using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Authenticate
{
    public bool status;
    public AuthenticationData data;
    public string message;
}

[Serializable]
public class AuthenticationData
{
    public string access_token;
}
