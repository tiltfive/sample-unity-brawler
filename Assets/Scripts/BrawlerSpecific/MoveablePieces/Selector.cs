/*
 * Copyright (C) 2020-2023 Tilt Five, Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selector : ControllableObject
{
    public GameObject characterPrefab;

    public GameObject frame;

    public Texture2D defaultPortraitTexture;

    public AudioSource audioSource;
    public AudioClip SFXselected;
    public AudioClip SFXdeselected;

    public bool isSpectator = false;

    new private void Start()
    {
        base.Start();
        //Find the frame game object for your player
        frame = GameObject.Find("Player" + ((int)controller.playerNumber));
    }

    private void Update()
    {
        RaycastHit hit;
        if(Physics.Raycast(transform.position, Vector3.down, out hit, Mathf.Infinity))
        {
            
            var portrait = hit.collider.gameObject.GetComponent<Portrait>();
            if (portrait != null)
            {
                frame.GetComponent<MeshRenderer>().material.mainTexture = portrait.portrait;
                if (portrait.characterPrefab)
                {
                    characterPrefab = portrait.characterPrefab;
                }
                else
                {
                    isSpectator = true;
                }
            }
            else
            {
                frame.GetComponent<MeshRenderer>().material.mainTexture = defaultPortraitTexture;
                characterPrefab = null;
                isSpectator = false;
            }
        }
        else
        {
            frame.GetComponent<MeshRenderer>().material.mainTexture = defaultPortraitTexture;
            characterPrefab = null;
            isSpectator = false;
        }
        SetColor();
    }

    public void SetColor()
    {
        this.gameObject.GetComponent<MeshRenderer>().material = mc.playerSelectorColors[(int)controller.playerNumber - 1];
    }
}
