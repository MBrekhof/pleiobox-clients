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

		private List<Site> sites = new List<Site> ();
		private SitesAdapter sitesAdapter;

		public async override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			var remoteExplorer = new RemoteExplorer ();

			try {
				var remoteSites = await remoteExplorer.GetSites ();

				foreach (LocalBox box in DataLayer.Instance.GetLocalBoxesSync()) {
					for (int i = 0; i < remoteSites.Count; i++) {
						if (box.BaseUrl == remoteSites[i].Url) {
							remoteSites.RemoveAt(i);
							break;
						}
					}
				}

				foreach (Site site in remoteSites) {
					sites.Add(site);
				}

				sitesAdapter.NotifyDataSetChanged();
			} catch (Exception ex) {
				Insights.Report (ex);
			}
		}

		public override View OnCreateView (LayoutInflater layoutInflater, ViewGroup viewGroup, Bundle bundle)
		{
			Dialog.SetTitle("Site toevoegen");

			View view = layoutInflater.Inflate (Resource.Layout.fragment_add_site, viewGroup, false);
			sitesAdapter = new SitesAdapter (Activity, sites);

			var siteListView = view.FindViewById<ListView> (Resource.Id.sitelist);
			siteListView.Adapter = sitesAdapter;
			siteListView.ChoiceMode = ChoiceMode.Multiple;

			Button buttonBack = view.FindViewById<Button> (Resource.Id.toevoegen);
			buttonBack.Click += delegate {

				var localBox = DataLayer.Instance.GetSelectedOrDefaultBox ();

				for (var i = 0; i < siteListView.CheckedItemPositions.Size(); i++) {
					if (siteListView.CheckedItemPositions.ValueAt(i) == false) {
						continue;
					}

					Site site = sites[i];

					LocalBox box = new LocalBox();
					box.BackColor = localBox.BackColor;
					box.BaseUrl = site.Url;
					box.DatumTijdTokenExpiratie = localBox.DatumTijdTokenExpiratie;
					box.LogoUrl = localBox.LogoUrl;
					box.Name = site.Name;
					box.OriginalServerCertificate = null;
					box.PassPhrase = null;
					box.PrivateKey = null;
					box.PublicKey = null;
					box.User = localBox.User;
					DataLayer.Instance.AddOrUpdateLocalBox(box);
				}
					
				var activity = (HomeActivity) Activity;
				activity.ShowToast("Sites toegevoegd...");
				activity.menuFragment.UpdateLocalBoxes();
				activity.HideAddSitesDialog();
			};

			return view;
		}
	}
}

