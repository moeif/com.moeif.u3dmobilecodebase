using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoaderRotation : MonoBehaviour
{
    public float rotateSpeed;

    // Update is called once per frame
    void Update()
    {
        transform.localEulerAngles += new Vector3(0, 0, rotateSpeed * Time.deltaTime);        
    }
}
