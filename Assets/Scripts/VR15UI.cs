using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VR15UI : MonoBehaviour
{
    //private Transform textTransform;
    private Transform textTransform;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        textTransform = ARLogic.textAttachment.transform;

        this.transform.position = textTransform.position;
        this.transform.rotation = textTransform.rotation;
    }
}
