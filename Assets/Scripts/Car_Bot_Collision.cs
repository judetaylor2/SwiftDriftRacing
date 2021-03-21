﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car_Bot_Collision : MonoBehaviour
{
    public AudioSource powerUpBoxSound;
    public Car_Bot bot;

    public int coinCount = 0;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
         if (other.gameObject.tag == "Coin")
         {
             coinCount++;

             Destroy(other.gameObject);
         }

         if (other.gameObject.tag == "PowerUpBox")
         {
             Destroy(other.gameObject);
             powerUpBoxSound.Play();
         }

        if (other.gameObject.tag == "weapon")
        {
            bot.forwardAccelBuildUp = bot.forwardAccelBuildUp / 2;
        }
    }
    

}
