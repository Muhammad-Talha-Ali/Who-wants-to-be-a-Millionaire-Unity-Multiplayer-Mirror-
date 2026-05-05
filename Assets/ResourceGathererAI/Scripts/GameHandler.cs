/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */
 
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey;
using CodeMonkey.Utils;
using CodeMonkey.MonoBehaviours;

public class GameHandler : MonoBehaviour {

    public static GameHandler instance;
    
    [SerializeField] private Transform goldNode1Transform;
    [SerializeField] private Transform goldNode2Transform;
    [SerializeField] private Transform goldNode3Transform;
    [SerializeField] public  List <GameObject> TreeNodes;
    [SerializeField] private Transform storageTransform;

    private void Awake() {
        instance = this;
    }

    private Transform GetResourceNode() {
        List<Transform> resourceNodeList = new List<Transform>();
        foreach (GameObject go in TreeNodes)
        {                          
            resourceNodeList.Add(go.GetComponent<Transform>());
        }
        //List<Transform> resourceNodeList = TreeNodes; /*new List<Transform>() { goldNode1Transform, goldNode2Transform, goldNode3Transform };*/
        if (TreeNodes.Count != 0)
            return resourceNodeList[UnityEngine.Random.Range(0, TreeNodes.Count - 1)];
        else return null;
    }

    public static Transform GetResourceNode_Static() {
        return instance.GetResourceNode();
    }

    private Transform GetStorage() {
        return storageTransform;
    }

    public static Transform GetStorage_Static() {
        return instance.GetStorage();
    }
}
