
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Android.App;
using Android.OS;
using Android.Widget;
using Android.Net;
using Android.Content.PM;
using Android.Views;

namespace localbox.android
{
	[Activity (Label = "", ScreenOrientation = ScreenOrientation.Landscape)]			
	public class VideoPlayerActivity : Activity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			//Hides action bar
			RequestWindowFeature (WindowFeatures.NoTitle);

			SetContentView (Resource.Layout.activity_videoplayer);

			VideoView videoView = FindViewById<VideoView> (Resource.Id.videoView);
			videoView.Completion += delegate { //Activity sluiten wanneer video afgespeeld is
				this.Finish();
			};

			MediaController mediaController = new MediaController(this);
			mediaController.SetAnchorView (videoView);
			videoView.SetMediaController(mediaController);

			try {
				string pathToVideo = Intent.GetStringExtra ("PathToVideo");

				var uri = Android.Net.Uri.Parse (pathToVideo);

				videoView.SetVideoURI (uri);
				videoView.Start ();
			}
			catch {
				this.RunOnUiThread (new Action (() => { 
					Toast.MakeText (this, "Het openen van de video is mislukt. Probeer het a.u.b. opnieuw.", ToastLength.Long).Show ();
				}));
			}
		}

	}
}

