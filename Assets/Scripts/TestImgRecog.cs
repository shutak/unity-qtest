using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;

public class TestImgRecog : MonoBehaviour {

	string recogName;
	WebCamTexture webCamTex;
	Texture2D capturedTex;
	public GameObject viewGO;
	public GameObject captViewGO;

	// Use this for initialization
	void Start () {
		recogName = "Unknown";
		webCamTex = new WebCamTexture ();
		webCamTex.Play ();
		viewGO.GetComponent<Renderer> ().material.mainTexture = webCamTex;
		capturedTex = new Texture2D(webCamTex.width,webCamTex.height);

	}
	
	// Update is called once per frame
	void Update () {

	}
	IEnumerator ReqRecogImg(Texture2D sourceTex) {
		// NTTサイトで取得したAPIKEYを指定
		string apikey="547047744738754a4a553175766354513855635349304c7650574a6a32486e69632f4c49464a306d7a5a43";
		// NTTの画像認識サーバーへのリクエストURL
		string url="https://api.apigw.smt.docomo.ne.jp/imageRecognition/v1/recognize?APIKEY="+apikey+"&recog=product-all&numOfCandidates=2";

		//テクスチャをJPG形式のバイナリに変換して、リクエストのボディーに設定
		Texture2D tex2d2 = new Texture2D (sourceTex.width, sourceTex.height);
		tex2d2.SetPixels32 (sourceTex.GetPixels32 ());
		tex2d2.Apply ();
		byte[] bytes = tex2d2.EncodeToJPG ();

		Dictionary<string,string> has = new Dictionary<string,string>();
		has.Add("Content-Type", "application/octet-stream");
		// HTTPリクエスト開始
		WWW www = new WWW(url,bytes,has);
		
		// サーバーにリクエストを行いレスポンスを待つ
		yield return www;
		Debug.Log(www.text);
		// サーバーから取得したレスポンスのテキストをJSONパーサー(MiniJson)でパースする
		var json = Json.Deserialize (www.text) as Dictionary<string, object>;

		recogName = "unknown";
		if (!json.ContainsKey ("candidates")) {
			yield break;
		}
		IList candi = json ["candidates"] as IList;
		if (candi != null) {
			foreach (var obj in candi) {
				var c = obj as IDictionary;

				if(c==null || !c.Contains("detail")){
					continue;
				}
				IDictionary detail=(IDictionary)c["detail"];
				if(detail!=null){
					string itemName=(string)detail["itemName"];
					if(itemName!=null){
						Debug.Log (itemName);
						recogName=itemName;
						break;
					}

				}

			}
		}
	}
	void OnGUI(){
		if (GUI.Button (new Rect (0, 0, 300, 50), "Test")) {
			Debug.Log ("Test");

	


		//	capturedTex= Resources.Load("pp") as Texture2D;


			capturedTex.SetPixels32(webCamTex.GetPixels32());
			capturedTex.Apply();
			if (capturedTex == null) {
				Debug.Log ("Load Img Error");
				
			}
			else {
				captViewGO.GetComponent<Renderer> ().material.mainTexture = capturedTex;
				//画像認識スタート
				StartCoroutine(ReqRecogImg(capturedTex));
			}
		}
		GUI.Label (new Rect (Screen.width - 200, 0, 200, 50), recogName);
	}
}
