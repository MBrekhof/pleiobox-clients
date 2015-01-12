using System;
using System.Text;
using System.Net.Http;
using System.Json;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

using Synchronization.Models;

using Newtonsoft.Json;

using LocalBox_Common.Remote.Authorization;
using LocalBox_Common.Remote.Model;
using System.Net.Http.Headers;
using System.Net;
using Newtonsoft.Json.Linq;
using Xamarin;

namespace LocalBox_Common.Remote
{
	public class RemoteExplorer
	{
		readonly LocalBox _localBox;

		public LocalBox LocalBox { get { return _localBox; } }

		public RemoteExplorer ()
		{
			_localBox = DataLayer.Instance.GetSelectedOrDefaultBox ();

			if(_localBox.OriginalServerCertificate != null){ //Selected localbox does have a ssl certificate

				//Set ssl validator for selected LocalBox
				SslValidator sslValidator = new SslValidator (_localBox);
				ServicePointManager.ServerCertificateValidationCallback = sslValidator.ValidateServerCertficate;
			}else {
				ServicePointManager.ServerCertificateValidationCallback = (p1, p2, p3, p4) => true;
			}
		}

		public RemoteExplorer (LocalBox box)
		{
			_localBox = box;
		}

		public bool IsAuthorized ()
		{
			DateTime expi;
			DateTime.TryParse (_localBox.DatumTijdTokenExpiratie, out expi);

			if (expi.Equals (new DateTime (1, 1, 1))) {
				// Not nooit geautoriseerd:
				return true;
			}


			if (expi.ToLocalTime () > DateTime.Now.ToLocalTime ()) {
				return true;
			}
			// Of nog niet geauthorizeer: doit
			// of expired: doit.
			return false;
		}


		private void ReAuthorise ()
		{
			try {
				var localBoxAuthorization = new LocalBoxAuthorization (_localBox);

				// Ververs het access token met het refreshtoken uit de database:
				string refreshToken = _localBox.RefreshToken;

				if (string.IsNullOrEmpty (refreshToken)) {
					throw new InvalidOperationException ("Refreshtoken is leeg voor refresh!");
				} else {
					localBoxAuthorization.RefreshAccessToken (refreshToken);

					if(localBoxAuthorization.AccessToken != null && localBoxAuthorization.RefreshToken != null){
						_localBox.AccessToken = localBoxAuthorization.AccessToken;
						_localBox.DatumTijdTokenExpiratie = localBoxAuthorization.Expiry.ToString ();
						_localBox.RefreshToken = localBoxAuthorization.RefreshToken;

						DataLayer.Instance.AddOrUpdateLocalBox (_localBox);
					}else {
						throw new InvalidOperationException ("Refreshtoken is leeg na refresh!");
					}
				}
			} catch (Exception ex) { 
				Insights.Report (ex);
			}
		}

		public DataGroup GetFiles (string currentFolderId = "")
		{
			if (!IsAuthorized ()) {
				ReAuthorise ();
			}

			StringBuilder localBoxUrl = new StringBuilder ();
			localBoxUrl.AppendFormat ("{0}lox_api/meta", _localBox.BaseUrl);

			if (!string.IsNullOrEmpty (currentFolderId) && !currentFolderId.Equals ("/")) {
				localBoxUrl.Append (currentFolderId);
			}

			string AccessToken = _localBox.AccessToken;

			using (var httpClient = new HttpClient ()) {
				httpClient.MaxResponseContentBufferSize = int.MaxValue;
				httpClient.DefaultRequestHeaders.ExpectContinue = false;
				httpClient.DefaultRequestHeaders.Add ("x-li-format", "json");
				httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue( "Bearer", Uri.EscapeDataString (AccessToken));

				var httpRequestMessage = new HttpRequestMessage {
					Method = HttpMethod.Get,
					RequestUri = new Uri (localBoxUrl.ToString ())
				};
				var response = httpClient.SendAsync (httpRequestMessage).Result;

				if (response.IsSuccessStatusCode) {
					var jsonString = response.Content.ReadAsStringAsync ().Result;

					var result = JsonConvert.DeserializeObject<DataGroup> (jsonString);
					return result;
				}
			}

			return new DataGroup ();
		}



		public Task<string> GetActivePEMFromServer ()
		{
			string urlToRegistrionJson = _localBox.BaseUrl + "register_app";
			return null;
			//TODO

//			string result = null;
//
//			try {
//				HttpWebRequest request = (HttpWebRequest)WebRequest.Create (boxUrl); 
//
//				request.ContentType = "application/json";
//				request.Timeout	= 10000; //10 seconds before timeout
//
//				//If cookie not null then add to http request header
//				//if (cookieString != null) {
//				//request.Headers.Add (HttpRequestHeader.
//				//}
//				string authToken = "Bearer" + " " + _localBox.AccessToken;
//				request.Headers.Add ("Authorization", authToken);
//				request.Method = "GET";
//
//				using (HttpWebResponse response = request.GetResponse () as HttpWebResponse) {
//					if (response.StatusCode != HttpStatusCode.OK) {
//
//						Console.Out.WriteLine ("Error fetching data. Server returned status code: {0}", response.StatusCode);
//						return null;
//					}
//
//					using (StreamReader reader = new StreamReader (response.GetResponseStream ())) {
//						var content = reader.ReadToEnd ();
//						if (string.IsNullOrWhiteSpace (content)) {
//							Console.Out.WriteLine ("Response contained empty body...");
//							return null;
//						} else {
//							try {
//								LocalBox box = JsonConvert.DeserializeObject<LocalBox> (content);
//									
//								return null;
//							} catch (Exception ex) {
//								result = null;
//							}
//						}
//					}
//				}
//			} catch (Exception ex) {
//				Console.WriteLine (ex.Message);					
//				return null;
//			}
		}



		public byte[] GetFile (string path)
		{
			return DownloadFile (path);
		}

		private byte[] DownloadFile (string item)
		{
			try {
				if (!IsAuthorized ()) {
					ReAuthorise ();
				}

				StringBuilder localBoxUrl = new StringBuilder ();
				localBoxUrl.Append (_localBox.BaseUrl + "lox_api/files");

				if (!string.IsNullOrEmpty (item)) {
					localBoxUrl.Append (item);
				}

				string AccessToken = _localBox.AccessToken;

				using (var httpClient = new HttpClient ()) {
					httpClient.MaxResponseContentBufferSize = int.MaxValue;
					httpClient.DefaultRequestHeaders.ExpectContinue = false;
					httpClient.DefaultRequestHeaders.Add ("x-li-format", "json");
					httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue( "Bearer", Uri.EscapeDataString (AccessToken));

					var httpRequestMessage = new HttpRequestMessage {
						Method = HttpMethod.Get,
						RequestUri = new Uri (localBoxUrl.ToString ())
					};
					var response = httpClient.SendAsync (httpRequestMessage).Result;

					if (response.IsSuccessStatusCode) {
						byte[] responseByteArray = response.Content.ReadAsByteArrayAsync ().Result;
						return responseByteArray;
					}
				}

				return null;
			} catch (Exception ex){
				Insights.Report(ex);
				return null;
			}
		}

		public bool DeleteFile (string filePath)
		{
			if (!IsAuthorized ()) {
				ReAuthorise ();
			}

			StringBuilder localBoxUrl = new StringBuilder ();
			localBoxUrl.Append (_localBox.BaseUrl + "lox_api/operations/delete");

			string AccessToken = _localBox.AccessToken;

			using (var httpClient = new HttpClient ()) {
				httpClient.MaxResponseContentBufferSize = int.MaxValue;
				httpClient.DefaultRequestHeaders.ExpectContinue = false;
				httpClient.DefaultRequestHeaders.Add ("x-li-format", "json");
				httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue( "Bearer", Uri.EscapeDataString (AccessToken));

				var data = new List<KeyValuePair<string, string>> ();
				data.Add (new KeyValuePair<string, string> ("path", filePath));
				HttpContent content = new FormUrlEncodedContent (data);

				try {
					var response = httpClient.PostAsync (new Uri (localBoxUrl.ToString ()), content).Result;
					if (response.IsSuccessStatusCode) {
						return true;
					}
				} catch (Exception ex){
					Insights.Report(ex);
					return false;
				}

			}

			return false;
		}

		public bool CreateFolder (string path)
		{
			if (string.IsNullOrWhiteSpace (path)) {
				throw new ArgumentNullException ("path", "Een mapnaam is verplicht");
			}

			if (!IsAuthorized ()) {
				ReAuthorise ();
			}

			StringBuilder localBoxUrl = new StringBuilder ();
			localBoxUrl.Append (_localBox.BaseUrl + "lox_api/operations/create_folder");

			string AccessToken = _localBox.AccessToken;

			using (var httpClient = new HttpClient ()) {
				httpClient.MaxResponseContentBufferSize = int.MaxValue;
				httpClient.DefaultRequestHeaders.ExpectContinue = false;
				httpClient.DefaultRequestHeaders.Add ("x-li-format", "json");
				httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue( "Bearer", Uri.EscapeDataString (AccessToken));

				var data = new List<KeyValuePair<string, string>> ();
				data.Add (new KeyValuePair<string, string> ("path", path));
				HttpContent content = new FormUrlEncodedContent (data);

				try {
					var response = httpClient.PostAsync (new Uri (localBoxUrl.ToString ()), content).Result;
					if (response.IsSuccessStatusCode) {
						return true;
					}
				} catch (Exception ex){
					Insights.Report(ex);
					return false;
				}
			}
			return false;
		}

		public bool Copy (string from, string to)
		{
			if (string.IsNullOrWhiteSpace (from) || string.IsNullOrWhiteSpace (to)) {
				throw new ArgumentNullException ("Een bron en bestemming is verplicht");
			}

			if (!IsAuthorized ()) {
				ReAuthorise ();
			}

			StringBuilder localBoxUrl = new StringBuilder ();
			localBoxUrl.Append (_localBox.BaseUrl + "lox_api/operations/copy");

			string AccessToken = _localBox.AccessToken;

			using (var httpClient = new HttpClient ()) {
				httpClient.MaxResponseContentBufferSize = int.MaxValue;
				httpClient.DefaultRequestHeaders.ExpectContinue = false;
				httpClient.DefaultRequestHeaders.Add ("x-li-format", "json");
				httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue( "Bearer", Uri.EscapeDataString (AccessToken));

				var data = new List<KeyValuePair<string, string>> () {
					new KeyValuePair<string, string> ("from_path", from),
					new KeyValuePair<string, string> ("to_path", to)
				};
				HttpContent content = new FormUrlEncodedContent (data);

				try {
					var response = httpClient.PostAsync (new Uri (localBoxUrl.ToString ()), content).Result;
					if (response.IsSuccessStatusCode) {
						return true;
					}
				} catch (Exception ex){
					Insights.Report(ex);
					return false;
				}
			}
			return false;
		}

		public bool Move (string from, string to)
		{
			if (string.IsNullOrWhiteSpace (from) || string.IsNullOrWhiteSpace (to)) {
				throw new ArgumentNullException ("Een bron en bestemming is verplicht");
			}

			if (!IsAuthorized ()) {
				ReAuthorise ();
			}

			StringBuilder localBoxUrl = new StringBuilder ();
			localBoxUrl.Append (_localBox.BaseUrl + "lox_api/operations/move");

			string AccessToken = _localBox.AccessToken;

			using (var httpClient = new HttpClient ()) {
				httpClient.MaxResponseContentBufferSize = int.MaxValue;
				httpClient.DefaultRequestHeaders.ExpectContinue = false;
				httpClient.DefaultRequestHeaders.Add ("x-li-format", "json");
				httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue( "Bearer", Uri.EscapeDataString (AccessToken));

				var data = new List<KeyValuePair<string, string>> () {
					new KeyValuePair<string, string> ("from_path", from),
					new KeyValuePair<string, string> ("to_path", to)
				};
				HttpContent content = new FormUrlEncodedContent (data);

				try {
					var response = httpClient.PostAsync (new Uri (localBoxUrl.ToString ()), content).Result;
					if (response.IsSuccessStatusCode) {
						return true;
					}
				} catch (Exception ex){
					Insights.Report(ex);
					return false;
				}
			}
			return false;
		}

        public bool UploadFile (string destination, Stream file)
		{
			StringBuilder localBoxUrl = new StringBuilder ();
			localBoxUrl.Append (_localBox.BaseUrl + "lox_api/files");
			localBoxUrl.Append (destination);

			string AccessToken = _localBox.AccessToken;

			using (var httpClient = new HttpClient ()) {
				httpClient.MaxResponseContentBufferSize = int.MaxValue;
				httpClient.DefaultRequestHeaders.ExpectContinue = false;
				httpClient.DefaultRequestHeaders.Add ("x-li-format", "json");
				httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue( "Bearer", Uri.EscapeDataString (AccessToken));

				var httpRequestMessage = new HttpRequestMessage {
					Method = HttpMethod.Post,
					RequestUri = new Uri (localBoxUrl.ToString ()),
                    Content = new StreamContent (file)
				};
				try {
					var response = httpClient.SendAsync (httpRequestMessage).Result;

					if (response.IsSuccessStatusCode) {
						return true;
					} else {
						return false;
					}
				} catch (Exception ex){
					Insights.Report(ex);
					return false;
				}
			}
		}


		public Task<List<Identity>> GetLocalBoxUsers ()
		{
			return Task.Run (() => {

				if (!IsAuthorized ()) {
					ReAuthorise ();
				}

				List<Identity> foundUsers = new List<Identity> ();

				StringBuilder localBoxUrl = new StringBuilder ();
				localBoxUrl.Append (_localBox.BaseUrl + "lox_api/identities");

				string AccessToken = _localBox.AccessToken;

				using (var httpClient = new HttpClient ()) {
					httpClient.MaxResponseContentBufferSize = int.MaxValue;
					httpClient.DefaultRequestHeaders.ExpectContinue = false;
					httpClient.DefaultRequestHeaders.Add ("x-li-format", "json");
					httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue( "Bearer", Uri.EscapeDataString (AccessToken));

					var httpRequestMessage = new HttpRequestMessage {
						Method = HttpMethod.Get,
						RequestUri = new Uri (localBoxUrl.ToString ())
					};
					var response = httpClient.SendAsync (httpRequestMessage).Result;

					if (response.IsSuccessStatusCode) {
						var jsonString = response.Content.ReadAsStringAsync ().Result;

						var allUsers = JsonConvert.DeserializeObject<List<Identity>> (jsonString);

						//Return only users with keys
						foreach(Identity identity in allUsers)
						{
							if(identity.Username != null && identity.HasKeys == true)
							{
								foundUsers.Add(identity);
							}
						}
					}
				}
				return foundUsers;
			});
		}


		public Task<bool> ShareFolder (string pathOfFolder, List<Identity> usersToShareWith)
		{
			return Task.Run (() => {
				StringBuilder localBoxUrl = new StringBuilder ();
				localBoxUrl.Append (_localBox.BaseUrl + "lox_api/share_create");
				localBoxUrl.Append (pathOfFolder);

				string AccessToken = _localBox.AccessToken;

				string jsonContentString = "{ \"identities\":" + JsonConvert.SerializeObject (usersToShareWith) + "}";

				using (var httpClient = new HttpClient ()) {
					httpClient.MaxResponseContentBufferSize = int.MaxValue;
					httpClient.DefaultRequestHeaders.ExpectContinue = false;
					httpClient.DefaultRequestHeaders.Add ("x-li-format", "json");
					httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue( "Bearer", Uri.EscapeDataString (AccessToken));

					var httpRequestMessage = new HttpRequestMessage {
						Method = HttpMethod.Post,
						RequestUri = new Uri (localBoxUrl.ToString ()),
						Content = new StringContent (jsonContentString, Encoding.UTF8, "application/json")
					};

					try {
						var response = httpClient.SendAsync (httpRequestMessage).Result;

						if (response.IsSuccessStatusCode) {
							return true;
						} else {
							return false;
						}
					} catch (Exception ex){
						Insights.Report(ex);
						return false;
					}
				}
			});
		}


		public Task<Share> GetShareSettings (string pathOfShare)
		{
			return Task.Run (() => {
				if (!IsAuthorized ()) {
					ReAuthorise ();
				}

				StringBuilder localBoxUrl = new StringBuilder ();
				localBoxUrl.Append (_localBox.BaseUrl + "lox_api/shares/");
				localBoxUrl.Append (pathOfShare);

				string AccessToken = _localBox.AccessToken;

				using (var httpClient = new HttpClient ()) {
					httpClient.MaxResponseContentBufferSize = int.MaxValue;
					httpClient.DefaultRequestHeaders.ExpectContinue = false;
					httpClient.DefaultRequestHeaders.Add ("x-li-format", "json");
					httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue( "Bearer", Uri.EscapeDataString (AccessToken));

					var httpRequestMessage = new HttpRequestMessage {
						Method = HttpMethod.Get,
						RequestUri = new Uri (localBoxUrl.ToString ())
					};

					try {
						var response = httpClient.SendAsync (httpRequestMessage).Result;

						if (response.IsSuccessStatusCode) {
							var jsonString = response.Content.ReadAsStringAsync ().Result;
					
							return JsonConvert.DeserializeObject<Share> (jsonString);
						}
						return null;
					} catch (Exception ex){
						Insights.Report(ex);
						return null;
					}
				}
			});
		}


		public Task<bool> UpdateSettingsSharedFolder (int shareId, List<Identity> usersToShareWith)
		{
			return Task.Run (() => {

				if (usersToShareWith.Count == 0) { //Geen users geselecteerd om me te sharen, dus unshare folder
					return UnShareFolder (shareId);
				}
				else{
					StringBuilder localBoxUrl = new StringBuilder ();
					localBoxUrl.Append (_localBox.BaseUrl + "lox_api/shares/" + shareId + "/edit");

					string AccessToken = _localBox.AccessToken;

					string jsonContentString = "{ \"identities\":" + JsonConvert.SerializeObject (usersToShareWith) + "}";

					using (var httpClient = new HttpClient ()) {
						httpClient.MaxResponseContentBufferSize = int.MaxValue;
						httpClient.DefaultRequestHeaders.ExpectContinue = false;
						httpClient.DefaultRequestHeaders.Add ("x-li-format", "json");
						httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue( "Bearer", Uri.EscapeDataString (AccessToken));

						var httpRequestMessage = new HttpRequestMessage {
							Method = HttpMethod.Post,
							RequestUri = new Uri (localBoxUrl.ToString ()),
							Content = new StringContent (jsonContentString, Encoding.UTF8, "application/json")
						};

						try {
							var response = httpClient.SendAsync (httpRequestMessage).Result;

							if (response.IsSuccessStatusCode) {
								return true;
							} else {
								return false;
							}
						} catch (Exception ex){
							Insights.Report(ex);
							return false;
						}
					}
				}
			});
		}


		public bool UnShareFolder (int shareId)
		{
			StringBuilder localBoxUrl = new StringBuilder ();
			localBoxUrl.Append (_localBox.BaseUrl + "lox_api/shares/" + shareId + "/remove");

			string AccessToken = _localBox.AccessToken;

			using (var httpClient = new HttpClient ()) {
				httpClient.MaxResponseContentBufferSize = int.MaxValue;
				httpClient.DefaultRequestHeaders.ExpectContinue = false;
				httpClient.DefaultRequestHeaders.Add ("x-li-format", "json");
				httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue( "Bearer", Uri.EscapeDataString (AccessToken));

				var httpRequestMessage = new HttpRequestMessage {
					Method = HttpMethod.Post,
					RequestUri = new Uri (localBoxUrl.ToString ()),
				};

				try {
					var response = httpClient.SendAsync (httpRequestMessage).Result;

					if (response.IsSuccessStatusCode) {
						return true;
					} else {
						return false;
					}
				} catch (Exception ex){
					Insights.Report(ex);
					return false;
				}
			}
		}


		public Task<PublicUrl> CreatePublicFileShare (string pathOfFileToShare, DateTime expirationDateOfShare)
		{
			return Task.Run (() => {
				if (!IsAuthorized ()) {
					ReAuthorise ();
				}
				try {
					string iso8601FormattedDate = expirationDateOfShare.ToString("yyyy-MM-ddTHH:mm:ssZ");

					StringBuilder localBoxUrl = new StringBuilder ();
					localBoxUrl.Append (_localBox.BaseUrl + "lox_api/links");
					localBoxUrl.Append (pathOfFileToShare);

					string AccessToken = _localBox.AccessToken;

					using (var httpClient = new HttpClient ()) {
						httpClient.MaxResponseContentBufferSize = int.MaxValue;
						httpClient.DefaultRequestHeaders.ExpectContinue = false;
						httpClient.DefaultRequestHeaders.Add ("x-li-format", "json");
						httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue( "Bearer", Uri.EscapeDataString (AccessToken));

						var data = new List<KeyValuePair<string, string>> ();
						data.Add (new KeyValuePair<string, string> ("expires", iso8601FormattedDate));
						HttpContent content = new FormUrlEncodedContent (data);
						
						var response = httpClient.PostAsync (new Uri (localBoxUrl.ToString ()), content).Result;

						if (response.IsSuccessStatusCode) {
							var jsonString = response.Content.ReadAsStringAsync ().Result;

							PublicUrl createdPublicUrl = JsonConvert.DeserializeObject<PublicUrl> (jsonString);

							string incompletePublicUrl = createdPublicUrl.publicUri;

							createdPublicUrl.publicUri = _localBox.BaseUrl + "public/" + incompletePublicUrl;

							return createdPublicUrl;
						}
						return null;
					}
				} catch (Exception ex){
					Insights.Report(ex);
					return null;
				}
			});
		}


		public List <ShareInventation> GetPendingShareInventations ()
		{
			if (!IsAuthorized ()) {
				ReAuthorise ();
			}
					
			List<ShareInventation> foundShareInventations = new List<ShareInventation> ();

			StringBuilder localBoxUrl = new StringBuilder ();
			localBoxUrl.Append (_localBox.BaseUrl + "lox_api/invitations");

			string AccessToken = _localBox.AccessToken;

			using (var httpClient = new HttpClient ()) {
				httpClient.MaxResponseContentBufferSize = int.MaxValue;
				httpClient.DefaultRequestHeaders.ExpectContinue = false;
				httpClient.DefaultRequestHeaders.Add ("x-li-format", "json");
				httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue( "Bearer", Uri.EscapeDataString (AccessToken));

				var httpRequestMessage = new HttpRequestMessage {
					Method = HttpMethod.Get,
					RequestUri = new Uri (localBoxUrl.ToString ())
				};
				var response = httpClient.SendAsync (httpRequestMessage).Result;

				if (response.IsSuccessStatusCode) {
					var jsonString = response.Content.ReadAsStringAsync ().Result;

					foundShareInventations = JsonConvert.DeserializeObject<List<ShareInventation>> (jsonString);
				}
			}
			return foundShareInventations;
		}


		public bool AcceptShareInventation(int shareInventationId)
		{
            if (!IsAuthorized ()) {
                ReAuthorise ();
            }

			StringBuilder localBoxUrl = new StringBuilder ();
			localBoxUrl.Append (_localBox.BaseUrl + "lox_api/invite/");
			localBoxUrl.Append (shareInventationId);

			localBoxUrl.Append ("/accept");

			string AccessToken = _localBox.AccessToken;

			using (var httpClient = new HttpClient ()) {
				httpClient.MaxResponseContentBufferSize = int.MaxValue;
				httpClient.DefaultRequestHeaders.ExpectContinue = false;
				httpClient.DefaultRequestHeaders.Add ("x-li-format", "json");
				httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue( "Bearer", Uri.EscapeDataString (AccessToken));

				var httpRequestMessage = new HttpRequestMessage {
					Method = HttpMethod.Post,
					RequestUri = new Uri (localBoxUrl.ToString ()),
				};

				try {
					var response = httpClient.SendAsync (httpRequestMessage).Result;

					if (response.IsSuccessStatusCode) {
						return true;
					} else {
						return false;
					}
				} catch (Exception ex){
					Insights.Report(ex);
					return false;
				}
			}
		}

        public UserResponse GetUser(string name = null)
        {
            if (!IsAuthorized ()) {
                ReAuthorise ();
            }

            StringBuilder localBoxUrl = new StringBuilder ();
            localBoxUrl.Append (_localBox.BaseUrl + "lox_api/user");
            if (!string.IsNullOrWhiteSpace(name))
            {
                localBoxUrl.Append ("/");
                localBoxUrl.Append (name);
            }

            string AccessToken = _localBox.AccessToken;

            using (var httpClient = new HttpClient ()) {
                httpClient.MaxResponseContentBufferSize = int.MaxValue;
                httpClient.DefaultRequestHeaders.ExpectContinue = false;
                httpClient.DefaultRequestHeaders.Add ("x-li-format", "json");
				httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue( "Bearer", Uri.EscapeDataString (AccessToken));

                var httpRequestMessage = new HttpRequestMessage {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri (localBoxUrl.ToString ()),
                };

                try {
                    var response = httpClient.SendAsync (httpRequestMessage).Result;

                    if (response.IsSuccessStatusCode) {
                        var jsonString = response.Content.ReadAsStringAsync ().Result;

                        var result = JsonConvert.DeserializeObject<UserResponse> (jsonString);
                        return result;
                    }
                    return null;
				} catch (Exception ex){
					Insights.Report(ex);
                    return null;
                }
            }

        }

        public bool UpdateUser(UserPost post)
        {
            if (!IsAuthorized ()) {
                ReAuthorise ();
            }

            StringBuilder localBoxUrl = new StringBuilder();
            localBoxUrl.Append(_localBox.BaseUrl + "lox_api/user");

            string AccessToken = _localBox.AccessToken;

            using (var httpClient = new HttpClient())
            {
                httpClient.MaxResponseContentBufferSize = int.MaxValue;
                httpClient.DefaultRequestHeaders.ExpectContinue = false;
                httpClient.DefaultRequestHeaders.Add("x-li-format", "json");
				httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue( "Bearer", Uri.EscapeDataString (AccessToken));

                var jsonString = JsonConvert.SerializeObject(post);
                var httpRequestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(localBoxUrl.ToString()),
                    Content = new StringContent(jsonString)
                };

                try
                {
                    var response = httpClient.SendAsync(httpRequestMessage).Result;

                    return response.IsSuccessStatusCode;
                }
				catch (Exception ex){
					Insights.Report(ex);
                    return false;
                }
            }
        }

        public bool AddAesKey(string path, AesKeyPost post)
        {
            if (!IsAuthorized ()) {
                ReAuthorise ();
            }

            StringBuilder localBoxUrl = new StringBuilder();
            localBoxUrl.Append(_localBox.BaseUrl);
            localBoxUrl.Append("lox_api/key");
            localBoxUrl.Append(path);

            string AccessToken = _localBox.AccessToken;

            using (var httpClient = new HttpClient())
            {
                httpClient.MaxResponseContentBufferSize = int.MaxValue;
                httpClient.DefaultRequestHeaders.ExpectContinue = false;
                httpClient.DefaultRequestHeaders.Add("x-li-format", "json");
				httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue( "Bearer", Uri.EscapeDataString (AccessToken));

                var jsonString = JsonConvert.SerializeObject(post, new JsonSerializerSettings() {
                    NullValueHandling = NullValueHandling.Ignore
                });
                var httpRequestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(localBoxUrl.ToString()),
                    Content = new StringContent(jsonString)
                };

                try
                {
                    var response = httpClient.SendAsync(httpRequestMessage).Result;

                    return response.IsSuccessStatusCode;
                }
				catch (Exception ex){
					Insights.Report(ex);
                    return false;
                }
            }
        }

        public bool RevokeAesKey(string path, AesKeyRevoke post)
        {
            if (!IsAuthorized ()) {
                ReAuthorise ();
            }

            StringBuilder localBoxUrl = new StringBuilder();
            localBoxUrl.Append(_localBox.BaseUrl);
            localBoxUrl.Append("lox_api/key_revoke");
            localBoxUrl.Append(path);

            string AccessToken = _localBox.AccessToken;

            using (var httpClient = new HttpClient())
            {
                httpClient.MaxResponseContentBufferSize = int.MaxValue;
                httpClient.DefaultRequestHeaders.ExpectContinue = false;
                httpClient.DefaultRequestHeaders.Add("x-li-format", "json");
				httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue( "Bearer", Uri.EscapeDataString (AccessToken));

                var jsonString = JsonConvert.SerializeObject(post, new JsonSerializerSettings() {
                    NullValueHandling = NullValueHandling.Ignore
                });
                var httpRequestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(localBoxUrl.ToString()),
                    Content = new StringContent(jsonString)
                };

                try
                {
                    var response = httpClient.SendAsync(httpRequestMessage).Result;

                    return response.IsSuccessStatusCode;
                }
				catch (Exception ex){
					Insights.Report(ex);
                    return false;
                }
            }
        }

        public bool GetAesKey(string path, out AesKeyResponse result)
        {
            if (!IsAuthorized ()) {
                ReAuthorise ();
            }

            StringBuilder localBoxUrl = new StringBuilder();
            localBoxUrl.Append(_localBox.BaseUrl);
            localBoxUrl.Append("lox_api/key");
            localBoxUrl.Append(path);

            string AccessToken = _localBox.AccessToken;

            using (var httpClient = new HttpClient())
            {
                httpClient.MaxResponseContentBufferSize = int.MaxValue;
                httpClient.DefaultRequestHeaders.ExpectContinue = false;
                httpClient.DefaultRequestHeaders.Add("x-li-format", "json");
				httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue( "Bearer", Uri.EscapeDataString (AccessToken));


                var httpRequestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(localBoxUrl.ToString())
                };

                try
                {
                    var response = httpClient.SendAsync(httpRequestMessage).Result;

                    if (response.IsSuccessStatusCode) {
                        var jsonString = response.Content.ReadAsStringAsync ().Result;

                        result = JsonConvert.DeserializeObject<AesKeyResponse> (jsonString);
                        return true;
                    } else if (response.StatusCode == System.Net.HttpStatusCode.NotFound) {
                        // geen key gevonden, maar op zich niet fout.
                        result = null;
                        return true;
                    } else {
                        result = null;
                        return false;
                    }
                }
				catch (Exception ex){
					Insights.Report(ex);
                    result = null;
                    return false;
                }
            }
        }


	}
}

