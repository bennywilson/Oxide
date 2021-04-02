using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [SerializeField]
    private Camera FollowCam;

    [SerializeField]
    private GameObject FollowTarget;

    [SerializeField]
    private float FollowOffset;

    [SerializeField]
    private Vector3 LookAtOffset;

    Vector3 VecOffset;

    // Start is called before the first frame update
    void Start()
    {
        VecOffset = transform.position - FollowTarget.transform.position;
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (FollowCam == null || FollowTarget == null)
        {
            return;
        }

        FollowCam.transform.position = FollowTarget.transform.position + VecOffset;
        //  transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, SmoothTime);

        // update rotation
        FollowCam.transform.LookAt(FollowTarget.transform.position + LookAtOffset);
    }
}
