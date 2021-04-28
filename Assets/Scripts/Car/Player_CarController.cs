﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Player_CarController : MonoBehaviour
{
    //objects & components
    [Header("Objects & Components")]
    public Rigidbody rb;
    public Transform groundRayPoint;
    public CarCollision carCol;

    public CinemachineVirtualCamera vc;


    //car variables
    [Header("Car Variables")]
    public float speedMultiplier = 10; public float originalMaxSpeed = 20;internal float maxSpeed; public float verticalDelayTime = 0.2f; public float turnStrength = 7.5f; internal float driftMultiplier = 1f; public float boostAmount;

    internal float speedInput, driftInput;
    internal float currentSpeed;
    internal bool isDrifting;

    //groundcheck
    [Header("Ground Check")]
    public LayerMask whatIsGround = 8;
    public float groundRayLength = 3;
    public float snapSpeed;

    internal bool isGrounded; 

    //counters
    internal float stopWatch_VerticalBuildUp; internal float stopWatch_Boost; internal float stopWatch_Drift; internal float stopwatch_trails; internal float stopwatch_DriftMove; internal float stopwatch_StopDrift;

    internal bool isBoosted;
    internal bool isOffTrack;
    internal bool readyToBoost;

    [Header("Wheels")]
    
    //front
    public Transform leftFrontWheel;
    public Transform rightFrontWheel;

    //back
    public Transform leftBackWheel;
    public Transform rightBackWheel;

    [Header("Trails / Particles")]

    public ParticleSystem driftTransitionParticle;
    public ParticleSystem boostParticle;
    public ParticleSystem smokeParticles;

    public List <ParticleSystem> driftParticles;
    public List<Material> driftParticlesMaterials;

    public Transform driftPoint1;
    public Transform driftPoint2;


    public GameObject allParticles;

    float driftBoostStage = 0;

    float screenX;



    //public List<GameObject> trails;


    // Start is called before the first frame update
    void Start()
    {
        //rb = transform.parent.FindChild("CarSphere").GetComponent<Rigidbody>();

        for (int i = 0; i < driftParticles.Count; i++)
        {
            driftParticles[i].Stop();
        }

        smokeParticles.Stop();

        

    }



    // Update is called once per frame
    void Update()
    {
        if (isGrounded)
        {
            VerticalInput();
            TurnInput();

            if (Input.GetKey(KeyCode.Q) && Input.GetKeyDown(KeyCode.Q))
            {
                vc.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.z *= -1;
            }
            else if (Input.GetKeyUp(KeyCode.Q))
            {
                vc.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.z *= -1;
            }

            screenX = vc.GetCinemachineComponent<CinemachineComposer>().m_ScreenX;

        }

        //follows the car at all times
        transform.position = rb.transform.position;

    }


    void FixedUpdate()
    {
        GroundCheck();
        ApplyForce();
    }



    void VerticalInput()
    {
        maxSpeed = originalMaxSpeed + carCol.coinCount;

        stopWatch_VerticalBuildUp += Time.deltaTime;

        if (!isDrifting)
        {
            speedInput = currentSpeed * speedMultiplier * Mathf.Abs(Input.GetAxisRaw("Vertical"));
        }
        else
        {
            speedInput = currentSpeed * speedMultiplier * 1;
        }        
        



        if (maxSpeed > originalMaxSpeed + carCol.maxCoinCount)
        {
            maxSpeed = originalMaxSpeed + carCol.maxCoinCount;
        }

        if (currentSpeed > maxSpeed)
        {
            currentSpeed = maxSpeed;
        }
        else if (currentSpeed < -maxSpeed)
        {
            currentSpeed = -maxSpeed;
        }


        if (!isDrifting)
        {

            if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.S))
            {
                smokeParticles.Play();
            }
            else
            {
                smokeParticles.Stop();
            }



            //changes the speed value on input
            if (Input.GetAxisRaw("Vertical") == 1 && currentSpeed < maxSpeed)
            {
                

                if (stopWatch_VerticalBuildUp >= verticalDelayTime)
                {
                    currentSpeed++;
                }

                if (vc.m_Lens.FieldOfView < 40)
                {
                    vc.m_Lens.FieldOfView += 1f;
                }


                

            }
            else if (Input.GetAxisRaw("Vertical") == -1 && currentSpeed > -maxSpeed)
            {
                

                if (stopWatch_VerticalBuildUp >= verticalDelayTime)
                {
                    currentSpeed--;

                    if (currentSpeed < 0)
                    {
                        stopWatch_VerticalBuildUp = 0;
                    }

                } 
                
                if (vc.m_Lens.FieldOfView > 35)
                {
                    vc.m_Lens.FieldOfView -= 1f;
                }

                
            }
            else if (Input.GetAxisRaw("Vertical") == 0)
            {
                if (vc.m_Lens.FieldOfView > 35)
                {
                    vc.m_Lens.FieldOfView -= 1f;
                }

                //changes speed until it is equal to 0
                if (currentSpeed < 0)
                {
                    currentSpeed++;
                }
                else if (currentSpeed > 0)
                {
                    currentSpeed--;
                }

                //stops the rigidbody from moving once it moves too slowly
                if (rb.velocity.magnitude < 5f)
                {
                    rb.velocity = new Vector3(0, 0, 0);
                    currentSpeed = 0;
                }

            }
        }
        
    }



    void TurnInput()
    {

        if (!isDrifting)
        {
            if (screenX < 0.5)
            {
                vc.GetCinemachineComponent<CinemachineComposer>().m_ScreenX += 0.01f;
            }
            else if (screenX > 0.5)
            {
                vc.GetCinemachineComponent<CinemachineComposer>().m_ScreenX -= 0.01f;
            }
        }

        //Debug.Log("current speed: " + currentSpeed);

        //when the key is pressed
        if (Input.GetKeyDown(KeyCode.Space) && Input.GetAxisRaw("Horizontal") != 0 && currentSpeed > 10 && currentSpeed <= maxSpeed && !isBoosted)
        {
            driftInput = Input.GetAxisRaw("Horizontal");

            stopWatch_Drift = 0;

            //location of the particle systems

            if (driftInput == 1)
            {
                allParticles.transform.position = driftPoint1.position;
                allParticles.transform.rotation = driftPoint1.rotation;
            }   
            else if (driftInput == -1)
            {
                allParticles.transform.position = driftPoint2.position;
                allParticles.transform.rotation = driftPoint2.rotation;
            }


            //-----------------------------------------------------------]
            
           
            

            if (driftBoostStage == 0)
            {
                driftParticles[0].Play();
                currentSpeed = 10;
                isDrifting = true;
            }
            


            
            
            //driftMultiplier = 3;
        }
        else
        {
            
        }
        
        if (isDrifting)
        {
            

            //when the key is held
            if (Input.GetKey(KeyCode.Space))
            {

                if (driftInput == 1 && screenX >= 0.325)
                {
                    vc.GetCinemachineComponent<CinemachineComposer>().m_ScreenX -= 0.01f;
                }
                else if (driftInput == -1 && screenX <= 0.625)
                {
                    vc.GetCinemachineComponent<CinemachineComposer>().m_ScreenX += 0.01f;
                }


                //Debug.Log(screenX);


                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0f, currentSpeed / 3 * driftInput * turnStrength * Time.deltaTime, 0f));

                /*
                //trails
                for (int i = 0; i < trails.Count; i++)
                {
                    trails[i].SetActive(true);
                }*/

                stopWatch_Drift += Time.deltaTime;

                //Debug.Log("drift " + (int)stopWatch_Drift);

                //drift boost

                //Debug.Log(driftBoostStage);
                if (stopWatch_Drift >= 30 / currentSpeed && driftBoostStage == 0)
                {
                    

                    driftParticles[1].GetComponent<ParticleSystemRenderer>().material = driftParticlesMaterials[0];

                    //boostParticle.GetComponent<ParticleSystemRenderer>().material = driftParticlesMaterials[0];
                    //boostParticle.GetComponentInChildren<ParticleSystemRenderer>().material = driftParticlesMaterials[0];

                    driftParticles[0].Stop();

                    driftTransitionParticle.Play();
                    driftParticles[1].Play();

                    readyToBoost = true;

                    

                    stopWatch_Drift = 0;
                    driftBoostStage = 1;

                }
                else if (stopWatch_Drift >= 40 / currentSpeed && driftBoostStage == 1)
                {
                    driftParticles[1].GetComponent<ParticleSystemRenderer>().material = driftParticlesMaterials[1];

                    //boostParticle.GetComponent<ParticleSystemRenderer>().material = driftParticlesMaterials[1];
                    //boostParticle.GetComponentInChildren<ParticleSystemRenderer>().material = driftParticlesMaterials[1];

                    driftTransitionParticle.Play();
                    driftParticles[1].Play();

                    stopWatch_Drift = 0;
                    driftBoostStage = 2;

                    




                }
                else if (stopWatch_Drift >= 40 / currentSpeed && driftBoostStage == 2)
                {
                    driftParticles[1].GetComponent<ParticleSystemRenderer>().material = driftParticlesMaterials[2];

                    //boostParticle.GetComponent<ParticleSystemRenderer>().material = driftParticlesMaterials[2];
                    //boostParticle.GetComponentInChildren<ParticleSystemRenderer>().material = driftParticlesMaterials[2];

                    driftTransitionParticle.Play();
                    driftParticles[1].Play();

                    stopWatch_Drift = 0;
                    driftBoostStage = 3;

                    
                }

            }
            else
            {
                stopwatch_StopDrift += Time.deltaTime;

                if (stopwatch_StopDrift >= 0.75)
                {

                    
                    stopwatch_StopDrift = 0;
                    isDrifting = false;

                    for (int i = 0; i < driftParticles.Count; i++)
                    {
                        driftParticles[i].Stop();
                    }


                }

            }

            stopwatch_DriftMove += Time.deltaTime;

            if (stopwatch_DriftMove > 0.05)
            {
                stopwatch_DriftMove = 0;


                if (Input.GetAxisRaw("Horizontal") == driftInput && currentSpeed < maxSpeed)
                {
                    currentSpeed += 1;
                }
                else if (Input.GetAxisRaw("Horizontal") == -driftInput && currentSpeed > 10)
                {
                    currentSpeed -= 1;
                }
            }
            

        }
        else if (Input.GetAxisRaw("Horizontal") != 0)
        {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0f, Input.GetAxis("Horizontal") * turnStrength * Time.deltaTime * Input.GetAxis("Vertical") * 2.5f, 0f));

            if (readyToBoost)
            {
                isBoosted = true;
                readyToBoost = false;
            }


            /*stopwatch_trails += Time.deltaTime;

            if (trails[0].activeInHierarchy && stopwatch_trails >= 1)
            {
                stopwatch_trails = 0;

                for (int i = 0; i < trails.Count; i++)
                {
                    trails[i].SetActive(false);
                }
            }*/
        }
        else
        {
            if (readyToBoost)
            {
                isBoosted = true;
                readyToBoost = false;
            }


            /*stopwatch_trails += Time.deltaTime;

            if (trails[0].activeInHierarchy && stopwatch_trails >= 1)
            {
                stopwatch_trails = 0;

                for (int i = 0; i < trails.Count; i++)
                {
                    trails[i].SetActive(false);
                }
            }*/

        }

        //leftBackWheel.rotation = Quaternion.Euler(leftBackWheel.rotation.eulerAngles.x + currentSpeed * 5, leftBackWheel.rotation.eulerAngles.y, leftBackWheel.rotation.eulerAngles.z);
        //rightBackWheel.rotation = Quaternion.Euler(rightBackWheel.rotation.eulerAngles.x + currentSpeed * 5, rightBackWheel.rotation.eulerAngles.y, rightBackWheel.rotation.eulerAngles.z);

        //visual wheel turning
        if (Input.GetAxisRaw("Vertical") != 0)
        {
            //front wheel turn x
            rightFrontWheel.transform.Rotate(new Vector3(currentSpeed * 100, 0f, 0f) * Time.deltaTime);
            leftFrontWheel.transform.Rotate(new Vector3(currentSpeed * 100, 0f) * Time.deltaTime);


            //back wheel turn x
            rightBackWheel.transform.Rotate(new Vector3(currentSpeed * 100, 0f, 0f) * Time.deltaTime);
            leftBackWheel.transform.Rotate(new Vector3(currentSpeed * 100, 0f, 0f) * Time.deltaTime);
        }
        else if (Input.GetAxisRaw("Horizontal") != 0)
        {
           

            //front wheel turn y
            leftFrontWheel.localRotation = Quaternion.Euler(leftFrontWheel.localRotation.eulerAngles.x, Input.GetAxis("Horizontal") * 30 - 180, leftFrontWheel.localRotation.eulerAngles.z);
            rightFrontWheel.localRotation = Quaternion.Euler(rightFrontWheel.localRotation.eulerAngles.x, Input.GetAxis("Horizontal") * 30, rightBackWheel.localRotation.eulerAngles.z);

            
        }
    }




    public void GroundCheck()
    {
        RaycastHit hit;

        if (Physics.Raycast(groundRayPoint.position, -transform.up, out hit, groundRayLength, whatIsGround))
        {
            isGrounded = true;


            
        }
        else
        {
            isGrounded = false;
        }

        if (Physics.Raycast(groundRayPoint.position, -transform.up, out hit, 35, whatIsGround))
        {

            Quaternion smoothtransition = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;

            transform.rotation = Quaternion.Lerp(transform.rotation, smoothtransition, Time.deltaTime * snapSpeed);


        }
    }



    public void ApplyForce()
    {
        //Debug.Log(isGrounded);



        if (isGrounded)
        {
            rb.drag = 3;

            if (carCol.isBot)
            {
                rb.AddForce(transform.forward * speedInput * 10);
            }
            else// if (Mathf.Abs(speedInput) > 0)
            {
                //moves car
                rb.AddForce(transform.forward * speedInput * 10);


                
            }


            if (isBoosted)
            {
                if (driftBoostStage != 0)
                {
                    boostParticle.GetComponent<ParticleSystemRenderer>().material = driftParticlesMaterials[(int)driftBoostStage - 1];
                    boostParticle.gameObject.transform.GetChild(0).GetComponent<ParticleSystemRenderer>().material = driftParticlesMaterials[(int)driftBoostStage - 1];

                    rb.AddForce(transform.forward * boostAmount * 50 * driftBoostStage);
                }
                else
                {
                    rb.AddForce(transform.forward * boostAmount * 50);
                }
                

                stopWatch_Boost += Time.deltaTime;
                

                boostParticle.Play();

                if (stopWatch_Boost >= 3)
                {
                    
                    stopWatch_Boost = 0;

                    boostParticle.Stop();

                    driftBoostStage = 0;

                    carCol.isBoosted = false;
                    isBoosted = false;

                    
                }

            }

            //force to apply when grounded
            rb.AddForce(-transform.up * 250);
        }
        else
        {
            rb.drag = 0.05f;

            rb.AddForce(-transform.up * 500);
        }
    }
}
