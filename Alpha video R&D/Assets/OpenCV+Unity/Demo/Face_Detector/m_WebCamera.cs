using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class m_WebCamera : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        WebCamTexture webCamTexture = new WebCamTexture();

        Renderer renderer = GetComponent<Renderer>();

        renderer.material.mainTexture = webCamTexture;

        webCamTexture.Play();
    }

    
}
