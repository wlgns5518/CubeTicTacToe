using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeSkyBox : MonoBehaviour
{
    // Skybox materials to switch between
    public Material[] skyboxMaterials;

    void Start()
    {
        SetRandomSkybox();
    }

    // Method to set a random skybox
    public void SetRandomSkybox()
    {
        if (skyboxMaterials.Length == 0)
        {
            Debug.LogWarning("No skybox materials assigned.");
            return;
        }

        // Select a random skybox
        int randomIndex = Random.Range(0, skyboxMaterials.Length);
        RenderSettings.skybox = skyboxMaterials[randomIndex];

        // Optionally update lighting
        DynamicGI.UpdateEnvironment();
    }
}