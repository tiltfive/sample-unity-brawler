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

public class ProjectileMovement : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject meshReference;
    public GameObject selfReference;
    public GameObject impactParticles;
    public float projectileSpeed = 10;
    public float projectileLifetime = 5;

    public AudioSource audioSource;
    public List<AudioClip> isHitSFX;
    public List<AudioClip> spawnSFX;

    public Character.ProjectileInfo projInfo;
    public Character charInfo;

    void Start()
    {
        StartCoroutine(DestroyMe());
        AudioClip spawnedSFX = spawnSFX[Random.Range(0, spawnSFX.Count)];
        audioSource.clip = spawnedSFX;
        audioSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.forward * projectileSpeed * Time.deltaTime);
    }

    public void Destroy()
    {
        AudioClip hitSFX = isHitSFX[Random.Range(0, isHitSFX.Count)];
        audioSource.clip = hitSFX;
        audioSource.Play();
        Instantiate(impactParticles, transform.position, transform.rotation);
        Destroy(meshReference);
        projectileSpeed = 0;
        Destroy(selfReference, 1f);
    }


    public IEnumerator DestroyMe()
    {

        yield return new WaitForSeconds(projectileLifetime);
        Destroy();
    }
}
