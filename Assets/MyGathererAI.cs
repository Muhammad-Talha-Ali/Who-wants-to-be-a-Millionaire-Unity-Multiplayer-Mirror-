/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MyGathererAI : MonoBehaviour
{

    private enum State
    {
        Idle,
        MovingToResourceNode,
        GatheringResources,
        MovingToStorage,
    }
    public Text Score; 
    public int ScoreAmount;
    public GameObject HarvestingParticles;
    public GameObject Tornado;
    public GameObject StoringParticles;
    public float speed;

    private IUnit unit;
    private State state;
    private Transform resourceNodeTransform;
    private Transform storageTransform;
    private int goldInventoryAmount;
    private float waitTime;
    public float TimeToWait;
    public Animator animator;
    private Vector3 LowerLevelTreePos;
    bool storingComplete;
    public float TornadoAttackTime;
    public float TornadoAttackResetTime;
    bool isTornadoAttacking;
    private GameHandler gameHandler;
    private void Awake()
    {
        unit = gameObject.GetComponent<IUnit>();
        state = State.Idle;
    }
    private void Start()
    {
        gameHandler = GameHandler.instance;
        waitTime = TimeToWait;
        storageTransform = GameHandler.GetStorage_Static();

    }
    void OnTriggerExit(Collider col)
    {
        if(col.tag == "Tree" && state==State.MovingToStorage)
        {
            GameObject Tree = col.gameObject;
            if (gameHandler.TreeNodes.Contains(Tree))
            {
                gameHandler.TreeNodes.Remove(Tree);
                Destroy(Tree, 0.5f);
            }
        }
    }
    void Storing()
    {
        if (storingComplete)
        {
                StoringParticles.SetActive(false);
            if (resourceNodeTransform != null)
                state = State.MovingToResourceNode;
            else if(resourceNodeTransform == null)
                state = State.Idle;
            storingComplete = false;
        }

        else
        {
            if (waitTime > 0)
            {
                StoringParticles.SetActive(true);
                waitTime -= Time.deltaTime ;
                animator.SetBool("Idle", true);
                animator.SetBool("Moving", false);
                animator.SetBool("Gather", false);

            }
            else
            {
               
                waitTime = TimeToWait;
                storingComplete = true;
            }
        }
    }
    void TornadoMove()
    {   if (Vector3.Distance(Tornado.transform.position, storageTransform.position) > 1f)
        {
            Tornado.transform.position = Vector3.MoveTowards(Tornado.transform.position, storageTransform.position, speed/5 * Time.deltaTime);
        }
        else
        {
            Tornado.SetActive(false);
            isTornadoAttacking = false;
            ScoreAmount -= 1;
            Tornado.transform.position += new Vector3(Random.Range(5, 10), 0, Random.Range(5, 10));
        }

    }
    private void Update()
    {
        if (isTornadoAttacking)
        {
            TornadoMove();
        }
        if (TornadoAttackTime > 0)
        {
            TornadoAttackTime -= Time.deltaTime;
        }
        if(TornadoAttackTime < 0)
        {
            isTornadoAttacking = true;
            Tornado.SetActive(true);
            TornadoAttackTime = TornadoAttackResetTime;
        }
        Score.text = ("Score :  " + ScoreAmount);
        switch (state)
        {
            case State.Idle:
                resourceNodeTransform = GameHandler.GetResourceNode_Static();
                Storing();
                print("Idle"+ state);
                break;
            case State.MovingToResourceNode:
             
               /* if (state == State.Idle)*/
/*            state = State.GatheringResources;*/
                print("moving to resource" + state);
                storageTransform = GameHandler.GetStorage_Static();
                LowerLevelTreePos = new Vector3(resourceNodeTransform.position .x,(resourceNodeTransform.position.y),resourceNodeTransform.position.z);
                Move();
                break;
            case State.GatheringResources:
             
                if(state == State.GatheringResources)
                {
                    if (goldInventoryAmount > 0)
                    {
                        // Move to storage
                        storageTransform = GameHandler.GetStorage_Static();
                        state = State.MovingToStorage;                      
                        break;
                    }
                    else
                    {
                        // Gather resources
                        
                        if (waitTime > 0)
                        {                            
                            HarvestingParticles.SetActive(true);
                            animator.SetBool("Gather", true);
                            animator.SetBool("Idle", false);
                            animator.SetBool("Moving", false);
                            waitTime -= Time.deltaTime;
                            print("adding amount idly" + state);
                        }
                        else
                        {
                            waitTime = TimeToWait;
                            HarvestingParticles.SetActive(false);
                            ScoreAmount++;
                            goldInventoryAmount++;
                        }
                        break;
                    }
                }
                break;
            case State.MovingToStorage:
                MoveBack();
                print("Setting state to Idle after moving to Storage" + state);
                goldInventoryAmount = 0;
                
                break;
        }
    }
    private void FixedUpdate()
    {
/*        Move();*/
    }
    void Move()
    {   
                print("Moving"+ state);
        if (Vector3.Distance(transform.position, LowerLevelTreePos) > 2f)
        {
            // change state
            animator.SetBool("Moving", true);
            animator.SetBool("Idle", false);
            animator.SetBool("Gather", false);
            transform.position = Vector3.MoveTowards(transform.position, LowerLevelTreePos, speed * Time.deltaTime);
            transform.LookAt(resourceNodeTransform);
        }
        else
        {
            state = State.GatheringResources;
        }
    }
    void MoveBack()
    {
        if (Vector3.Distance(transform.position, storageTransform.position) > 2f)
        {
            // change state
            animator.SetBool("Moving", true);
            animator.SetBool("Idle", false);
            animator.SetBool("Gather", false);
            transform.position = Vector3.MoveTowards(transform.position, storageTransform.position, speed * Time.deltaTime);
            transform.LookAt(storageTransform);
        }
        else
        {
            state = State.Idle;
        }
    }
}
