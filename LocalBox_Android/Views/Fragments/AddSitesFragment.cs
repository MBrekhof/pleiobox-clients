using System;
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
using LocalBox_Common.Remote;
using LocalBox_Common;
using Xamarin;

namespace LocalBox_Droid
{
	public class AddSitesFragment : DialogFragment
	{

		private List<Site> Sites;

		public override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
		}

		public override View OnCreateView (LayoutInflater layoutInflater, ViewGroup viewGroup, Bundle bundle)
		{
			Dialog.SetTitle("Site toevoegen");

			View view = layoutInflater.Inflate (Resource.Layout.fragment_add_site, viewGroup, false);

			var remoteExplorer = new RemoteExplorer ();

			List<Site> sites = new List<Site>();

			try {
				sites = remoteExplorer.GetSites ().Result;

				foreach (LocalBox box in DataLayer.Instance.GetLocalBoxesSync()) {
					for (int i = 0; i < sites.Count; i++) {
						if (box.BaseUrl == sites[i].Url) {
							sites.RemoveAt(i);
							break;
						}
					}
				}
			} catch (Exception ex) {
				Insights.Report (ex);
			}


			var sitesAdapter = new SitesAdapter (Activity, sites);

			var siteListView = view.FindViewById<ListView> (Resource.Id.sitelist);
			siteListView.Adapter = sitesAdapter;

			Button buttonBack = view.FindViewById<Button> (Resource.Id.toevoegen);
			buttonBack.Click += delegate {
				Toast.MakeText (Activity, "Nog maken....", ToastLength.Short).Show ();
			};

			return view;
		}
	}
}

