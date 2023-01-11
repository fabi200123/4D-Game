using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine.Android;
using UnityEditor;

[RequireComponent (typeof (MeshFilter))]
[RequireComponent (typeof (MeshRenderer))]


public class GPSGenerator : MonoBehaviour {
  	MeshFilter meshFilter;
	Mesh mesh;

	float last_lat, last_lon, last_alt;

	public List<Vector4> originalVerts;
	public List<Vector4> rotatedVerts;

	public List<Vector3> originalTris;

	public List<Vector3> verts;
	public List<int> tris;
	public List<Vector2> uvs;

	public List<Axis4D> rotationOrder;
	public Dictionary<Axis4D,float> rotation;
	public Text GPSStatus;
    public Text latitudeValue;
    public Text longitudeValue;
    public Text altitudeValue;
	
	void Start () {
		#if UNITY_ANDROID
            if (!Permission.HasUserAuthorizedPermission (Permission.FineLocation))
            {
                Permission.RequestUserPermission (Permission.FineLocation);
            }
        #elif UNITY_IOS
            PlayerSettings.iOS.locationUsageDescription = "Details to use location";
        #endif
		StartCoroutine(GPSLoc());
		rotationOrder = new List<Axis4D>();
		rotationOrder.Add(Axis4D.yz);
		rotationOrder.Add(Axis4D.xw);
		rotationOrder.Add(Axis4D.yw);
		rotationOrder.Add(Axis4D.zw);
		rotationOrder.Add(Axis4D.xy);
		rotationOrder.Add(Axis4D.xz);

		rotation = new Dictionary<Axis4D,float>();
		rotation.Add(Axis4D.xy, 0f);
		rotation.Add(Axis4D.xz, 0f);
		rotation.Add(Axis4D.xw, 0f);
		rotation.Add(Axis4D.yz, 0f);
		rotation.Add(Axis4D.yw, 0f);
		rotation.Add(Axis4D.zw, 0f);


		meshFilter = GetComponent<MeshFilter>();
		mesh = meshFilter.sharedMesh;
		if (mesh == null){
			meshFilter.mesh = new Mesh();
			mesh = meshFilter.sharedMesh;
		}


		originalVerts = new List<Vector4>(){
			new Vector4(80,80,80,80),
			new Vector4(80,80,80,-80),
			new Vector4(80,80,-80,80),
			new Vector4(80,80,-80,-80),
			new Vector4(80,-80,80,80),
			new Vector4(80,-80,80,-80),
			new Vector4(80,-80,-80,80),
			new Vector4(80,-80,-80,-80),
			new Vector4(-80,80,80,80),
			new Vector4(-80,80,80,-80),
			new Vector4(-80,80,-80,80),
			new Vector4(-80,80,-80,-80),
			new Vector4(-80,-80,80,80),
			new Vector4(-80,-80,80,-80),
			new Vector4(-80,-80,-80,80),
			new Vector4(-80,-80,-80,-80)
		};

		ResetVertices();
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
            yield break;
        }

        if(Input.location.status == LocationServiceStatus.Failed){
            GPSStatus.text = "Unable to determine device location";
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
			float lat = float.Parse(latitudeValue.text.ToString(), System.Globalization.NumberStyles.Float);
			float lon = float.Parse(longitudeValue.text.ToString(), System.Globalization.NumberStyles.Float);
			float alt = float.Parse(altitudeValue.text.ToString(), System.Globalization.NumberStyles.Float);
			if(alt != last_alt && lon != last_lon && lat != last_lat){
			Rotate(Axis4D.xy,lat);
			Rotate(Axis4D.xz,lon);
			Rotate(Axis4D.xw,alt);
			Rotate(Axis4D.yw,lat);
			Rotate(Axis4D.yz,lon);
			Rotate(Axis4D.zw,alt);
			ApplyRotationToVerts();
			last_lat = lat;
			last_lon = lon;
			last_alt = alt;
			}
        }
        else{
            GPSStatus.text = "STOPPED";
            // service is stopped
        }
    } //End of UpdateGPSData

	void Update(){
		DrawTesseract();
	}

	void DrawTesseract () {
		mesh.Clear();
		verts = new List<Vector3>();
		tris = new List<int>();
		uvs = new List<Vector2>();

		CreatePlane(rotatedVerts[0],rotatedVerts[1],rotatedVerts[5],rotatedVerts[4]);
		CreatePlane(rotatedVerts[0],rotatedVerts[2],rotatedVerts[6],rotatedVerts[4]);
		CreatePlane(rotatedVerts[0],rotatedVerts[8],rotatedVerts[12],rotatedVerts[4]);
		CreatePlane(rotatedVerts[0],rotatedVerts[2],rotatedVerts[3],rotatedVerts[1]);
		CreatePlane(rotatedVerts[0],rotatedVerts[1],rotatedVerts[9],rotatedVerts[8]);
		CreatePlane(rotatedVerts[0],rotatedVerts[2],rotatedVerts[10],rotatedVerts[8]);

		CreatePlane(rotatedVerts[1],rotatedVerts[3],rotatedVerts[7],rotatedVerts[5]);
		CreatePlane(rotatedVerts[1],rotatedVerts[9],rotatedVerts[13],rotatedVerts[5]);
		CreatePlane(rotatedVerts[1],rotatedVerts[3],rotatedVerts[9],rotatedVerts[11]);

		CreatePlane(rotatedVerts[2],rotatedVerts[3],rotatedVerts[7],rotatedVerts[6]);
		CreatePlane(rotatedVerts[2],rotatedVerts[3],rotatedVerts[10],rotatedVerts[11]);
		CreatePlane(rotatedVerts[2],rotatedVerts[10],rotatedVerts[14],rotatedVerts[6]);

		CreatePlane(rotatedVerts[3],rotatedVerts[11],rotatedVerts[15],rotatedVerts[7]);

		CreatePlane(rotatedVerts[4],rotatedVerts[12],rotatedVerts[13],rotatedVerts[5]);
		CreatePlane(rotatedVerts[4],rotatedVerts[6],rotatedVerts[14],rotatedVerts[12]);
		CreatePlane(rotatedVerts[4],rotatedVerts[6],rotatedVerts[7],rotatedVerts[5]);

		CreatePlane(rotatedVerts[5],rotatedVerts[7],rotatedVerts[15],rotatedVerts[13]);

		CreatePlane(rotatedVerts[6],rotatedVerts[7],rotatedVerts[14],rotatedVerts[15]);

		CreatePlane(rotatedVerts[8],rotatedVerts[10],rotatedVerts[14],rotatedVerts[12]);
		CreatePlane(rotatedVerts[8],rotatedVerts[9],rotatedVerts[13],rotatedVerts[12]);
		CreatePlane(rotatedVerts[8],rotatedVerts[9],rotatedVerts[10],rotatedVerts[11]);

		CreatePlane(rotatedVerts[9],rotatedVerts[11],rotatedVerts[15],rotatedVerts[13]);

		CreatePlane(rotatedVerts[10],rotatedVerts[11],rotatedVerts[15],rotatedVerts[14]);

	}


	void CreatePlane (Vector3 p1,Vector3 p2,Vector3 p3,Vector3 p4) {
		Vector2 uv0 = Vector2.zero;
		Vector2 uv1 = Vector2.zero;
		Vector2 uv2 = Vector2.zero;

		
		List<Vector3> newVerts = new List<Vector3>(){
			p1,p3,p2,
			p1,p2,p4,
			p2,p3,p4,
			p1,p4,p3
		};

		verts.AddRange(newVerts);
		mesh.vertices = verts.ToArray();

		int t = tris.Count;
		for(int j = 0; j < 12; j++){
			tris.Add(j+t);
		}

		mesh.SetTriangles(tris.ToArray(),0);


		uv0 = new Vector2(0,0);
		uv1 = new Vector2(1,0);
		uv2 = new Vector2(0.5f,1);

		uvs.AddRange(
			new List<Vector2>(){
				uv0,uv1,uv2,
				uv0,uv1,uv2,
				uv0,uv1,uv2,
				uv0,uv1,uv2
			}
		);
		mesh.uv = uvs.ToArray();
		
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		mesh.Optimize();
	}

    Vector4 GetRotatedVertex(Axis4D axis, Vector4 v, float s, float c){
    	switch(axis) {
			case Axis4D.xy: {
                return RotateAroundXY(v,s,c);
				break;
            }
			case Axis4D.xz: {
                return RotateAroundXZ(v,s,c);
				break;
            }
			case Axis4D.xw: {
                return RotateAroundXW(v,s,c);
				break;
            }
			case Axis4D.yz: {
                return RotateAroundYZ(v,s,c);
				break;
            }
			case Axis4D.yw: {
                return RotateAroundYW(v,s,c);
				break;
            }
			case Axis4D.zw: {
                return RotateAroundZW(v,s,c);
				break;
            }
		}

		return new Vector4(0,0,0,0);
    }

    Vector4 RotateAroundXY(Vector4 v, float s, float c) {
        float tmpX = c * v.x + s * v.y;
        float tmpY = -s * v.x + c * v.y;
        return new Vector4(tmpX,tmpY,v.z,v.w);
    }

    Vector4 RotateAroundXZ(Vector4 v, float s, float c) {
		float tmpX = c * v.x + s * v.z;
		float tmpZ = -s * v.x + c * v.z;
		return new Vector4(tmpX,v.y,tmpZ,v.w);
    }

    Vector4 RotateAroundXW(Vector4 v, float s, float c) {
		float tmpX = c * v.x + s * v.w;
        float tmpW = -s * v.x + c * v.w;
        return new Vector4(tmpX,v.y,v.z,tmpW);
    }

    Vector4 RotateAroundYZ(Vector4 v, float s, float c) {
		float tmpY = c * v.y + s * v.z;
        float tmpZ = -s * v.y + c * v.z;
         return new Vector4(v.x,tmpY,tmpZ,v.w);
    }

    Vector4 RotateAroundYW(Vector4 v, float s, float c) {
		float tmpY = c * v.y - s * v.w;
        float tmpW = s * v.y + c * v.w;
		return new Vector4(v.x,tmpY,v.z,tmpW);
    }

    Vector4 RotateAroundZW(Vector4 v, float s, float c) {
		float tmpZ = c * v.z - s * v.w;
		float tmpW = s * v.z + c * v.w;
        return new Vector4(v.x,v.y,tmpZ,tmpW);
    }

	void Rotate(Axis4D axis, float theta){
		AddToRotationDictionary(axis, theta);
		ApplyRotationToVerts();
	}

	void AddToRotationDictionary(Axis4D axis, float theta) {
		rotation[axis] = (rotation[axis] + theta);

  }

  void ApplyRotationToVerts() {
    ResetVertices();

    foreach (Axis4D axis in rotationOrder) {
      float s = Mathf.Sin(Mathf.Deg2Rad*rotation[axis]);
      float c = Mathf.Cos(Mathf.Deg2Rad*rotation[axis]);
      for(int i = 0; i < rotatedVerts.Count; i++){
        rotatedVerts[i] = GetRotatedVertex(axis,rotatedVerts[i], s, c);
      }
    }
  }

  void ResetVertices() {
    rotatedVerts = new List<Vector4>();
    rotatedVerts.AddRange(originalVerts);
  }

  void OnDrawGizmosSelected() {
    Gizmos.color = Color.yellow;
    for(int i = 0; i < rotatedVerts.Count; i++){
      Gizmos.DrawSphere(rotatedVerts[i], 0.1f);
    }  
  }
}