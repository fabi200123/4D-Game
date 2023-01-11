// Original source: https://github.com/alexisgea/sphere_generator and post: https://www.alexisgiard.com/icosahedron-sphere-remastered/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Text;

namespace AlexisGea {
	public enum SphereType {tetrasphere}

	[RequireComponent(typeof(MeshRenderer))]
	[RequireComponent(typeof(MeshFilter))]
	[DisallowMultipleComponent]
	public class SphereGenerator : MonoBehaviour {


		[HideInInspector] public SphereType SphereType = SphereType.tetrasphere;
		[HideInInspector] public float Radius = 0.5f;
		[HideInInspector] public int Resolution = 4;
		[HideInInspector] public bool Smooth = false;
		[HideInInspector] public bool RemapVertices = false;

        private bool _generating = false;
        private MeshData _sphereMesh = null;
		private MeshFilter _filter = null;
		private MeshRenderer _renderer = null;

		public Text myText;

		public void GenerateMesh() {
			if (_generating) { return; }

			if(_filter == null) {
				_filter = GetComponent<MeshFilter>();
			}
			if(_renderer == null) {
				_renderer = GetComponent<MeshRenderer>();
			}

			_sphereMesh = null;
			_generating = true;

			if (Application.isPlaying) {
                System.Threading.ThreadPool.QueueUserWorkItem(GenerateSphereMeshThread);
            }
			else {
				GenerateSphereMeshThread(null);
				UpdateMesh();
			}
		}

		private void Reset() {
			Awake();
		}

        private void Awake() {
			_sphereMesh = null;
        	_generating = false;

            GenerateMesh();
        }

		private void Update() {
			//if (_generating && _sphereMesh != null) { 
			if (_sphereMesh != null) {
				UpdateMesh();
			}
		}

		private void GenerateSphereMeshThread(object obj) {
			IPlatonicSolid baseSolid = new Tetrahedron();
			_sphereMesh = SphereBuilder.Build(baseSolid, Radius, Resolution, Smooth, RemapVertices);
		}

		private void UpdateMesh() {
			if (!_filter.sharedMesh) {
				_filter.sharedMesh = new Mesh();
			}

			_filter.sharedMesh.Clear();
			_filter.sharedMesh.name = SphereType.ToString();
			_filter.sharedMesh.vertices = _sphereMesh.Vertices;
			_filter.sharedMesh.triangles = _sphereMesh.Triangles;
			_filter.sharedMesh.uv = _sphereMesh.Uv;
			_filter.sharedMesh.normals = _sphereMesh.Normals;
			_filter.sharedMesh.tangents = _sphereMesh.Tangents;

			if(_renderer.sharedMaterial == null) {
				_renderer.sharedMaterial = new Material(Shader.Find("Diffuse"));
			}

			_generating = false;
        } 

		public Dictionary<Axis4D,float> rotation, slider, oldPosition;
		bool freezeRotation = false;
		float movement;

		void Start () {
			rotation = new Dictionary<Axis4D,float>();
			rotation.Add(Axis4D.xy, 0f);
			rotation.Add(Axis4D.xz, 0f);
			rotation.Add(Axis4D.xw, 0f);
			rotation.Add(Axis4D.yz, 0f);
			rotation.Add(Axis4D.yw, 0f);
			rotation.Add(Axis4D.zw, 0f);

			slider = new Dictionary<Axis4D,float>();
			slider.Add(Axis4D.xy, 0f);
			slider.Add(Axis4D.xz, 0f);
			slider.Add(Axis4D.xw, 0f);
			slider.Add(Axis4D.yz, 0f);
			slider.Add(Axis4D.yw, 0f);
			slider.Add(Axis4D.zw, 0f);

			oldPosition = new Dictionary<Axis4D,float>();
			oldPosition.Add(Axis4D.xy, 0f);
			oldPosition.Add(Axis4D.xz, 0f);
			oldPosition.Add(Axis4D.xw, 0f);
			oldPosition.Add(Axis4D.yz, 0f);
			oldPosition.Add(Axis4D.yw, 0f);
			oldPosition.Add(Axis4D.zw, 0f);

			movement = 0.0f;
		}

		void OnGUI() {
			GUIStyle style = new GUIStyle();
			style.fixedHeight = 60;
			style.fixedWidth = 60;
			style.fontSize = 60;
			style.fontStyle = FontStyle.Bold;
			style.normal.textColor = Color.white;
			
			freezeRotation = GUI.Toggle (new Rect(350,740,600,70), freezeRotation, "Freeze Rotation", style); 

			GUI.skin.horizontalSlider.fixedHeight = 30;
			GUI.skin.horizontalSliderThumb.fixedHeight = 30; 

			GUI.skin.label.fontSize = 40;
			GUI.Label (new Rect(350, 100, 500,40), "XY");
			slider[Axis4D.xy] = GUI.HorizontalSlider(new Rect(350, 140, 500,40), Mathf.Repeat(slider[Axis4D.xy],360f), 0.0F, 360.0F);
			this.transform.Rotate(0, rotation[Axis4D.xy], 0, Space.Self);
			
			GUI.skin.label.fontSize = 40;
			GUI.Label (new Rect(350, 200, 500,40), "XZ");
			slider[Axis4D.xz] = GUI.HorizontalSlider(new Rect(350, 240, 500,40), Mathf.Repeat(slider[Axis4D.xz],360f), 0.0F, 360.0F);
			this.transform.Rotate(rotation[Axis4D.xz], 0, 0, Space.Self);

			GUI.skin.label.fontSize = 40;
			GUI.Label (new Rect(350, 300, 500,40), "XW");
			slider[Axis4D.xw] = GUI.HorizontalSlider(new Rect(350, 340, 500,40), Mathf.Repeat(slider[Axis4D.xw],360f), 0.0F, 360.0F);
			this.transform.Rotate(rotation[Axis4D.xw], movement, movement, Space.Self);
			
			GUI.skin.label.fontSize = 40;
			GUI.Label (new Rect(350, 400, 500,40), "YZ");
			slider[Axis4D.yz] = GUI.HorizontalSlider(new Rect(350, 440, 500,40), Mathf.Repeat(slider[Axis4D.yz],360f), 0.0F, 360.0F);
			this.transform.Rotate(0, 0, rotation[Axis4D.yz], Space.Self);

			GUI.skin.label.fontSize = 40;
			GUI.Label (new Rect(350, 500, 500,40), "YW");
			slider[Axis4D.yw] = GUI.HorizontalSlider(new Rect(350, 540, 500,40), Mathf.Repeat(slider[Axis4D.yw],360f), 0.0F, 360.0F);
			this.transform.Rotate(movement, movement, rotation[Axis4D.yw], Space.Self);

			GUI.skin.label.fontSize = 40;
			GUI.Label (new Rect(350, 600, 500,40), "ZW");
			slider[Axis4D.zw] = GUI.HorizontalSlider(new Rect(350, 640, 500,40), Mathf.Repeat(slider[Axis4D.zw],360f), 0.0F, 360.0F);
			this.transform.Rotate(movement, rotation[Axis4D.zw], movement, Space.Self);
			
			if(freezeRotation){
				rotation[Axis4D.xy] = 0.0f;
				rotation[Axis4D.xz] = 0.0f;
				rotation[Axis4D.xw] = 0.0f;
				rotation[Axis4D.yw] = 0.0f;
				rotation[Axis4D.yz] = 0.0f;
				rotation[Axis4D.zw] = 0.0f;

				if(slider[Axis4D.xy] != oldPosition[Axis4D.xy]){
					oldPosition[Axis4D.xy] = rotation[Axis4D.xy] = slider[Axis4D.xy];
				}
				if(slider[Axis4D.xz] != oldPosition[Axis4D.xz]){
					oldPosition[Axis4D.xz] = rotation[Axis4D.xz] = slider[Axis4D.xz];
				}
				if(slider[Axis4D.xw] != oldPosition[Axis4D.xw]){
					oldPosition[Axis4D.xw] = rotation[Axis4D.xw] = slider[Axis4D.xw];
				}
				if(slider[Axis4D.yw] != oldPosition[Axis4D.yw]){
					oldPosition[Axis4D.yw] = rotation[Axis4D.yw] = slider[Axis4D.yw];
				}
				if(slider[Axis4D.yz] != oldPosition[Axis4D.yz]){
					oldPosition[Axis4D.yz] = rotation[Axis4D.yz] = slider[Axis4D.yz];
				}
				if(slider[Axis4D.zw] != oldPosition[Axis4D.zw]){
					oldPosition[Axis4D.zw] = rotation[Axis4D.zw] = slider[Axis4D.zw];
				}
				
				movement = 0.0f;
			}
			else{
				slider[Axis4D.xy] += 0.1f;
				slider[Axis4D.xz] += 0.15f;
				slider[Axis4D.xw] += 0.6f;
				slider[Axis4D.yw] += 0.3f;
				slider[Axis4D.yz] += 0.45f;
				slider[Axis4D.zw] += 0.5f;

				oldPosition[Axis4D.xy] = rotation[Axis4D.xy] = slider[Axis4D.xy];
				oldPosition[Axis4D.xz] = rotation[Axis4D.xz] = slider[Axis4D.xz];
				oldPosition[Axis4D.xw] = rotation[Axis4D.xw] = slider[Axis4D.xw];
				oldPosition[Axis4D.yw] = rotation[Axis4D.yw] = slider[Axis4D.yw];
				oldPosition[Axis4D.yz] = rotation[Axis4D.yz] = slider[Axis4D.yz];
				oldPosition[Axis4D.zw] = rotation[Axis4D.zw] = slider[Axis4D.zw];

				movement = 0.015f;
			}
		}
    }
}

