using System;
using System.Text;
using System.Net.Http;
using System.Json;
using System.Diagnostics;
using System.Net.Http.Headers;
using Xamarin;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LocalBox_Common.Remote.Authorization
{
    public class LocalBoxAuthorization
    {
        private string client_key = "pleiobox";
		private string client_secret = "Ex3EgyUeoQehogQdF7GqtRaVHYArpa";
        private string localBoxBaseUrl;

		private LocalBox _localBox;
		private Database _database = DataLayer.Instance.DbInstance();

        private string _accessToken = null;
        public string AccessToken { 
            get { return _accessToken; }
        }

        private string _refreshToken;
        public string RefreshToken {
            get { return _refreshToken; }
        }

        private DateTime _expiry;
        public DateTime Expiry {
            get { return _expiry; }
        }

		public LocalBoxAuthorization(string baseUrl)
		{
			localBoxBaseUrl = baseUrl;
		}

        public LocalBoxAuthorization(LocalBox localBox)
		{			
			_localBox = localBox;

			var tokens = _database.GetTokens ();
			_accessToken = tokens.AccessToken;
			_refreshToken = tokens.RefreshToken;

			var expires = tokens.Expires;
			DateTime.TryParse (expires, out _expiry);

			localBoxBaseUrl = localBox.BaseUrl;
        }

		public bool IsAuthorized()
		{
			if (_expiry.Equals (new DateTime (1,1,1))) {
				// Not nooit geautoriseerd:
				return true;
			}

			if (_expiry > DateTime.UtcNow.ToLocalTime ()) {
				return true;
			}
			// Of nog niet geauthorizeer: doit
			// of expired: doit.
			return false;
		}

		public Task<bool> Authorize(string username, string password)
		{
			return Task.Run (() => {
				StringBuilder localBoxUrl = new StringBuilder ();
				localBoxUrl.Append (localBoxBaseUrl);
				localBoxUrl.Append ("/oauth/v2/token");

				var data = new List<KeyValuePair<string, string>> ();
				data.Add (new KeyValuePair<string, string> ("client_id", client_key));
				data.Add (new KeyValuePair<string, string> ("client_secret", client_secret));
				data.Add (new KeyValuePair<string, string> ("grant_type", "password"));
				data.Add (new KeyValuePair<string, string> ("username", username));
				data.Add (new KeyValuePair<string, string> ("password", password));

				HttpContent content = new FormUrlEncodedContent (data);

				var handler = new HttpClientHandler {};

				using (var httpClient = new HttpClient (handler)) {
					httpClient.MaxResponseContentBufferSize = int.MaxValue;
					httpClient.DefaultRequestHeaders.ExpectContinue = false;
					httpClient.DefaultRequestHeaders.Add ("x-li-format", "json");

					try {
						var response = httpClient.PostAsync (new Uri (localBoxUrl.ToString ()), content).Result;
						if (response.IsSuccessStatusCode) {
							string rawResponse = response.Content.ReadAsStringAsync ().Result;
							var jsonObject = JsonValue.Parse (rawResponse);

							_accessToken = jsonObject ["access_token"];
							_refreshToken = jsonObject ["refresh_token"];

							// We laten de key al vervallen als al 90% van de tijd is verstreken
							_expiry = DateTime.UtcNow.AddSeconds ((int)jsonObject ["expires_in"] * 0.9).ToLocalTime ();
						
							var tokens = _database.GetTokens();
							tokens.AccessToken = _accessToken;
							tokens.RefreshToken = _refreshToken;
							tokens.Expires = _expiry.ToString();
							_database.UpdateTokens(tokens);

							return true;
						} else {
							return false;
						}
					} catch (Exception ex) {
						Insights.Report (ex);
						return false;
					}
				}
			});
		}

		public bool RefreshAccessToken()
        {
            StringBuilder localBoxUrl = new StringBuilder();
            localBoxUrl.Append(localBoxBaseUrl);
			localBoxUrl.Append("oauth/v2/token");

			if (_refreshToken == null) {
				throw new InvalidOperationException ("Refreshtoken is leeg!");
			}
            
			var data = new List<KeyValuePair<string, string>> ();
			data.Add (new KeyValuePair<string, string> ("client_id", client_key));
			data.Add (new KeyValuePair<string, string> ("client_secret", client_secret));
			data.Add (new KeyValuePair<string, string> ("grant_type", "refresh_token"));
			data.Add (new KeyValuePair<string, string> ("refresh_token", _refreshToken));

			HttpContent content = new FormUrlEncodedContent (data);

			return RequestAccessToken(new Uri(localBoxUrl.ToString()), content);
           
        }

		private bool RequestAccessToken(Uri uri, HttpContent data) {
			bool result = false;

			var handler = new HttpClientHandler {};

			using (var httpClient = new HttpClient (handler)) {
				httpClient.MaxResponseContentBufferSize = int.MaxValue;
				httpClient.DefaultRequestHeaders.ExpectContinue = false;
				httpClient.DefaultRequestHeaders.Add ("x-li-format", "json");

				try {
					var response = httpClient.PostAsync (uri, data).Result;
					if (response.IsSuccessStatusCode) {
						string content = response.Content.ReadAsStringAsync().Result;
						var jsonObject = JsonValue.Parse (content);

						_accessToken = jsonObject ["access_token"];

						if (!string.IsNullOrEmpty (jsonObject ["refresh_token"])) {

							_refreshToken = jsonObject ["refresh_token"];

							int expiry = 0;
							if (jsonObject ["expires_in"].JsonType == JsonType.Number) {
								expiry = (int)jsonObject ["expires_in"];
								// We laten de key al vervallen als al 90% van de tijd is verstreken
								_expiry = DateTime.UtcNow.AddSeconds (expiry * 0.9).ToLocalTime();

								result = true;
							}
						}
							
						var tokens = _database.GetTokens();
						tokens.AccessToken = _accessToken;
						tokens.RefreshToken = _refreshToken;
						tokens.Expires = _expiry.ToString();
						_database.UpdateTokens(tokens);

					} else {
						Debug.WriteLine ("Fout in requestaccesstoken: " + response.Headers); 
					}
				} catch (Exception ex) {
					Insights.Report (ex);
					return false;
				}
			} 

			return result;
        }
    }
}

