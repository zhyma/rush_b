using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class MagLogic : MonoBehaviour
{
    public GameObject leftController;
    public GameObject rightController;

    public static int numBullets = 10;
    public const int maxBullets = 10;

    private GameObject magModel;
    private GameObject bullet1, bullet2, bullet3;
    private GameObject follower;

    private float reloading;
    // Start is called before the first frame update
    void Start()
    {
        magModel = this.transform.GetChild(0).gameObject;
        follower = magModel.transform.GetChild(1).gameObject;
        bullet1  = magModel.transform.GetChild(2).gameObject;
        bullet2  = magModel.transform.GetChild(3).gameObject;
        bullet3  = magModel.transform.GetChild(4).gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        bool magInserted = ((SerialCom.switchStates & (1 << 2)) == 0);

        // if magzine is inserted, attach is to the weapon (right hand controller), otherwise attach to the left hand controller
        if (!magInserted)
        {
            this.transform.position = leftController.transform.position;
            this.transform.rotation = leftController.transform.rotation;
            // + right, + up, + front 
            magModel.transform.localPosition = new Vector3(-0.025f, 0.03f, -0.1f);
            magModel.transform.localRotation = Quaternion.Euler(-7.5f, 180f, 4f);
        }
        else
        {
            this.transform.position = rightController.transform.position;
            this.transform.rotation = rightController.transform.rotation;
            // + front, + up, + left
            magModel.transform.localPosition = new Vector3(-0.505f, 0.025f, 0.015f);
            magModel.transform.localRotation = Quaternion.Euler(15f, -90f, 0f);
        }

        if (!magInserted)
        {
            reloading += Time.deltaTime;
            if (reloading > 2.0f)
            {
                if (numBullets <= 0)
                    numBullets = maxBullets;
                else
                    reloading = .0f;
            }
        }
        else
        {
            reloading = 0f;
        }

        showBullets();

        return;
    }

    void showBullets()
    {
        if (numBullets >= 3)
        {
            bullet1.SetActive(true);
            bullet1.transform.localPosition = new Vector3(0.004f, 0.19f, 0f);
            bullet2.SetActive(true);
            bullet2.transform.localPosition = new Vector3(-0.004f, 0.187f, 0.001f);
            bullet3.SetActive(true);
            bullet3.transform.localPosition = new Vector3(0.004f, 0.184f, 0.002f);

            follower.transform.localPosition = new Vector3(0.0001662f, 0.179f, 0.00145f);
        }
        else if (numBullets == 2)
        {
            bullet1.SetActive(true);
            bullet1.transform.localPosition = new Vector3(-0.004f, 0.19f, 0f);
            bullet2.SetActive(true);
            bullet2.transform.localPosition = new Vector3(0.004f, 0.187f, 0.001f);
            bullet3.SetActive(false);

            follower.transform.localPosition = new Vector3(0.0001662f, 0.182f, 0.00145f);
        }
        else if (numBullets == 1)
        {
            bullet1.SetActive(true);
            bullet1.transform.localPosition = new Vector3(0.004f, 0.19f, 0f);
            bullet2.SetActive(false);
            bullet3.SetActive(false);

            follower.transform.localPosition = new Vector3(0.0001662f, 0.185f, 0.00145f);
        }
        else
        {
            bullet1.SetActive(false);
            bullet2.SetActive(false);
            bullet3.SetActive(false);
            follower.transform.localPosition = new Vector3(0.0001662f, 0.18746f, 0.00145f);
        }
    }
}
