using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ARLogic : MonoBehaviour
{
    // Start is called before the first frame update
    public static GameObject ar15_main;
    
    private GameObject fireSelector;
    private GameObject trigger;

    public static bool loaded = false;
    private int loadingStage = 0;

    // Command send to vr15, valve turns on for 20ms. Electromagnet's delay is none or +100ms.
    private byte[] cmdFire = { 0x20 };
    private byte[] cmdHoldOpen = { 0x2A };

    private GameObject firingPoint;
    public GameObject fire;
    private AudioSource audioPlayer;
    public AudioClip fireClip;
    // dry fire
    public AudioClip dryClip;

    private float fireCD = 0.2f;
    private float fireTimer = 0;
    private bool lastTriggerPulled = false;
    private bool semiReady = false;

    public static GameObject textAttachment;

    //private float time;

    //StreamWriter writer;

    void Start()
    {
        //vr15_pos = vr15.transform.localPosition;
        //vr15 = GameObject.Find("ar15/ar15_main");
        //charginHandle = GameObject.Find("ar15/ar15_main/charging_handle.step");
        //boltCarrier = GameObject.Find("ar15/ar15_main/bolt_carrier.step");
        //boltExtractor = GameObject.Find("ar15/ar15_main/bolt_extractor.step");

        // Init gun
        ar15_main = this.transform.GetChild(0).gameObject;
        // Prepare the muzzle brake to show some flash
        firingPoint = this.transform.GetChild(1).gameObject;
        // Init gun parts
        fireSelector = ar15_main.transform.GetChild(3).gameObject;
        trigger = ar15_main.transform.GetChild(6).gameObject;

        audioPlayer = GetComponent<AudioSource>();

        textAttachment = this.transform.GetChild(3).gameObject;

        //string path = "d:\\tof_log.txt";
        //writer = new StreamWriter(path, true);
    }

    // Update is called once per frame
    void Update()
    {
        // pull the charging handle
        //int TOFDistance = Filter.Smooth(SerialCom.TOFDistance);
        int TOFDistance = SerialCom.TOFDistance;
        //writer.WriteLine(TOFDistance);
        //Debug.Log(TOFDistance);

        // check switches
        // auto?
        bool fullAuto = ((SerialCom.switchStates & 1) > 0);
        // trigger pulled?
        bool triggerPulled = ((SerialCom.switchStates & (1<<1)) > 0);
        // Mag inserted?
        // need to test
        bool magInserted = ((SerialCom.switchStates & (1<<2)) == 0);
        // bolt hold open?
        // need to test
        bool holdOpen = ((SerialCom.switchStates & (1 << 3)) == 0);

        if (!loaded)
            BoltLogic.hideBullet();
        else
            BoltLogic.showBullet();

        if (fullAuto)
            fireSelector.transform.localRotation = Quaternion.Euler(90f, -90f, -90f);
        else
            fireSelector.transform.localRotation = Quaternion.Euler(0f, -90f, -90f);

        
        Vector3 vec = BoltLogic.chargingHandlePos(TOFDistance);

        // 0. TOFDistance: <= TOFLowThreshold  ==>  loadingStage =0
        //    locked
        if (TOFDistance <= BoltLogic.TOFLowThreshold + 10)
            loadingStage = 0;
        // 1. TOFDistance:  < 57  ==> loadingStage = 1
        //    do nothing
        // 2. TOFDistance:  >=57  ==>  loadingStage = 2
        //    eject
        if (TOFDistance >= BoltLogic.TOFHighThreshold-2 && loadingStage!=3)
        {
            loadingStage = 2;
            loaded = false;
            BoltLogic.hideBullet();
        }
        // 4. TOFDistance >=45 & <55
        //    if mag is not empty and mounted, pusheen one round into the chamber, goto stage 3
        //    if mag is empty/no mag, go to stage 1
        if (TOFDistance < 45 && loadingStage == 2)
        {
            if (magInserted && (MagLogic.numBullets > 0))
            {
                loadingStage = 3;
                MagLogic.numBullets--;
                loaded = true;
                BoltLogic.showBullet();
                semiReady = true;
            }
        }
        // 5. TOFDistance: >=TOFLowThreshold & <45  ==>  loadingStage = 5
        //    set the round to position


        BoltLogic.updateHandlePos(vec);
        // if hold open:
        // charging handle moves, bolt fixed to the farest offset
        if (holdOpen)
            vec = BoltLogic.boltOpenOffset();
        BoltLogic.updateBoltPos(vec);
        BoltLogic.updateBulletPos(vec);

        // show states 
        Debug.LogFormat("TOF: {0}, loaded: {1}, bullets: {2}", TOFDistance, loaded, MagLogic.numBullets);
        // fire
        if (triggerPulled)
        {
            fireTimer -= Time.deltaTime;
            trigger.transform.localRotation = Quaternion.Euler(-180f, 0f, 20f);
            // gun is loaded and ready
            if (loaded && (SerialCom.operating == false) && fireTimer <=0
                && (TOFDistance <= BoltLogic.TOFLowThreshold) && (holdOpen == false))
            {
                // if set to semi, must had trigger released before
                // or set to fullauto
                if ((fullAuto == false && semiReady) || (fullAuto == true))
                {
                    // reset cooldown
                    fireTimer = fireCD;
                    // fire animation
                    Instantiate(fire, firingPoint.transform.position, firingPoint.transform.rotation);
                    audioPlayer.PlayOneShot(fireClip);
                    if ((magInserted == false) || (MagLogic.numBullets <= 0))
                    {
                        // last round in the chamber AND (no mag, or mag empty) ==> hold open
                        MagLogic.numBullets = 0;
                        // No bullet left, bolt hold open
                        SerialCom.SendData(cmdHoldOpen);
                        loaded = false;
                        semiReady = false;
                    }
                    else
                    {
                        SerialCom.SendData(cmdFire);
                        MagLogic.numBullets--;
                        loaded = true;
                        semiReady = false;
                    }
                }
            }
            // dry fire
            if (loaded==false && lastTriggerPulled == false)
                    audioPlayer.PlayOneShot(dryClip);

            lastTriggerPulled = true;
        }
        else
        {
            // trigger released
            fireTimer = -.1f;
            trigger.transform.localRotation = Quaternion.Euler(-180f, 0f, 4f);
            lastTriggerPulled = false;
            semiReady = true;
        }
    }

    void OnDestroy()
    {
        //writer.Close();
    }
}
