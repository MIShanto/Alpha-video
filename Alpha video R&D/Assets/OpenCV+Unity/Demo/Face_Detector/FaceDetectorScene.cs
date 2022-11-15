namespace OpenCvSharp.Demo
{
	using System;
	using UnityEngine;
	using System.Collections.Generic;
	using UnityEngine.UI;
	using OpenCvSharp;

	public class FaceDetectorScene : WebCamera
	{
		public TextAsset faces;
		public TextAsset eyes;
		public TextAsset shapes;
		public GameObject alphaVideo;
		public bool isDebugging = true;
		private FaceProcessorLive<WebCamTexture> processor;

		/// <summary>
		/// Default initializer for MonoBehavior sub-classes
		/// </summary>
		protected override void Awake()
		{
			base.Awake();
			base.forceFrontalCamera = true; // we work with frontal cams here, let's force it for macOS s MacBook doesn't state frontal cam correctly

			byte[] shapeDat = shapes.bytes;

			if (shapeDat.Length == 0)
			{
				string errorMessage =
					"In order to have Face Landmarks working you must download special pre-trained shape predictor " +
					"available for free via DLib library website and replace a placeholder file located at " +
					"\"OpenCV+Unity/Assets/Resources/shape_predictor_68_face_landmarks.bytes\"\n\n" +
					"Without shape predictor demo will only detect face rects.";

#if UNITY_EDITOR
				// query user to download the proper shape predictor
				if (UnityEditor.EditorUtility.DisplayDialog("Shape predictor data missing", errorMessage, "Download", "OK, process with face rects only"))
					Application.OpenURL("http://dlib.net/files/shape_predictor_68_face_landmarks.dat.bz2");
#else
             UnityEngine.Debug.Log(errorMessage);
#endif
			}

			processor = new FaceProcessorLive<WebCamTexture>();
			processor.Initialize(faces.text, eyes.text, shapes.bytes);

			// data stabilizer - affects face rects, face landmarks etc.
			processor.DataStabilizer.Enabled = true;        // enable stabilizer
			processor.DataStabilizer.Threshold = 2.0;       // threshold value in pixels
			processor.DataStabilizer.SamplesCount = 2;      // how many samples do we need to compute stable data

			// performance data - some tricks to make it work faster
			processor.Performance.Downscale = 256;          // processed image is pre-scaled down to N px by long side
			processor.Performance.SkipRate = 0;             // we actually process only each Nth frame (and every frame for skipRate = 0)
		}

		/// <summary>
		/// Per-frame video capture processor
		/// </summary>
		protected override bool ProcessTexture(WebCamTexture input, ref Texture2D output)
		{
			//GetComponent<RectTransform>().sizeDelta = Vector2.one;

			// detect everything we're interested in
			processor.ProcessTexture(input, TextureParameters);

			// mark detected objects
			processor.MarkDetected(false);

			var face = processor.Faces[0];

			var points = face.Marks;

			Vector2 p1, p2;
			p1.x = points[36].X;
			p1.y = points[36].Y;

			p2.x = points[42].X;
			p2.y = points[42].Y;

			//Debug.Log("l: "+  p1);
			//Debug.Log("r: " + p2);

			float w = Vector2.Distance(p1, p2);

			float W = 6.3f;
			float d = 50f;
			float f = (w*d)/W;
			//Debug.Log(f);

			f = 840f;
			d = (W * f) / w;

			//Debug.Log(d);
			if (!isDebugging)
			{
				if (d >= 121)
				{
					alphaVideo.SetActive(true);
				}
				else
				{
					alphaVideo.SetActive(false);
				}
			}
			else
			{
				alphaVideo.SetActive(true);
			}
			

			// processor.Image now holds data we'd like to visualize
			output = Unity.MatToTexture(processor.Image, output);   // if output is valid texture it's buffer will be re-used, otherwise it will be re-created

			return true;
		}

        /*private void OnDrawGizmos()
        {
			Vector2 p1, p2;
			p1.x = processor.Faces[0].Marks[36].X;
			p1.y = processor.Faces[0].Marks[36].Y;
			
			p2.x = processor.Faces[0].Marks[42].X;
			p2.y = processor.Faces[0].Marks[42].Y;

			Gizmos.DrawLine(p1, p2);
			Gizmos.color = Color.red;
		}*/
    }

	
}