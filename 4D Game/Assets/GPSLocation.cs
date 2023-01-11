using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GPSLocation : MonoBehaviour
{
    public static GPSLocation Instance { set; get; }

    public Text GPSStatus;
    public Text latitudeValue;
    public Text longitudeValue;
    public Text altitudeValue;
    public bool status = false;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(GPSLoc());
    }

    IEnumerator GPSLoc(){
        // check if user has location service enabled...
        if(!Input.location.isEnabledByUser)
            yield break;

        // start service before querying location
        Input.location.Start();

        //wait until service initialize
        int maxWait = 20;
        while(Input.location.status == LocationServiceStatus.Initializing && maxWait > 0){
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // service did not init in 20 sec
        if(maxWait < 1){
            GPSStatus.text = "Timed out";
            status = false;
            yield break;
        }

        if(Input.location.status == LocationServiceStatus.Failed){
            GPSStatus.text = "Unable to determine device location";
            status = false;
            yield break;
        }
        else {
            // Acces granted
            GPSStatus.text = "Running";
            InvokeRepeating("UpdateGPSData", 0.5f, 1f);
        } // end of GPSLoc
    }
    // Update is called once per frame
    void UpdateGPSData()
    {
        if(Input.location.status == LocationServiceStatus.Running){
            // Access granted to GPS values and it has been init
            GPSStatus.text = "Running";
            latitudeValue.text = Input.location.lastData.latitude.ToString();
            longitudeValue.text = Input.location.lastData.longitude.ToString();
            altitudeValue.text = Input.location.lastData.altitude.ToString();
            status = true;
        }
        else{
            GPSStatus.text = "STOPPED";
            status = false;
            // service is stopped
        }
    } //End of UpdateGPSData
}
