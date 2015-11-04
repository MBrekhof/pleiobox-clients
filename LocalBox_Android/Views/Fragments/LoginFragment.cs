﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Webkit;

using LocalBox_Common.Remote.Authorization;
using LocalBox_Common;

namespace LocalBox_Droid
{
	public class LoginFragment : DialogFragment
	{
		private string PleioUrl;

		public LoginFragment(string pleioUrl)
		{
			PleioUrl = pleioUrl;
		}

		public override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
		}

		public override View OnCreateView (LayoutInflater layoutInflater, ViewGroup viewGroup, Bundle bundle)
		{
			Dialog.SetTitle("Inloggen");
				
			View view = layoutInflater.Inflate (Resource.Layout.fragment_login, viewGroup, false);

			Button buttonBack = view.FindViewById<Button> (Resource.Id.login);
			TextView username = view.FindViewById<TextView> (Resource.Id.username);
			TextView password = view.FindViewById<TextView> (Resource.Id.password);

			buttonBack.Click += delegate {
				Login(username.Text, password.Text);
			};
				
			return view;
		}

		private async void Login (string username, string password)
		{
			var authorization = new LocalBoxAuthorization (PleioUrl);

			var result = await authorization.Authorize (username, password);
			if (result) {
				Toast.MakeText (Activity, "Ingelogd", ToastLength.Short).Show ();

				var business = new BusinessLayer();

				if (DataLayer.Instance.GetLocalBoxesSync ().Count == 0) {
					await business.RegisterLocalBox (PleioUrl);
				}

			} else {
				Toast.MakeText (Activity, "Niet ingelogd", ToastLength.Short).Show ();
			}
		}

	}
}

