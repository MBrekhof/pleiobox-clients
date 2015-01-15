using System;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using System.Security;
using System.Net.Security;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using LocalBox_Common.Remote;

using Xamarin;

namespace LocalBox_Common
{
	public class CertificateHelper
	{
		public static byte[] BytesOfServerCertificate;

		public static bool ErrorsOccured = false;
		public static bool IgnoreSllErrorsInWebView = false;
		private static bool FoundSelfSignedCertificate = false;
		private static bool isCertificateChecked = false;
	
		public static void GetCertificateFromUrl (string url)
		{
			Environment.SetEnvironmentVariable ("MONO_TLS_SESSION_CACHE_TIMEOUT", "0");

			ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback ((sender, cert, chain, errors) => { 

				isCertificateChecked = true;

				if (errors == SslPolicyErrors.None) {
					var serverX509Certificate2 = new X509Certificate2 (cert);
					BytesOfServerCertificate = serverX509Certificate2.Export (X509ContentType.Cert);

					return true;
				} else {
					if ((errors & System.Net.Security.SslPolicyErrors.RemoteCertificateChainErrors) != 0) {
						if (chain != null && chain.ChainStatus != null) {
							foreach (System.Security.Cryptography.X509Certificates.X509ChainStatus status in chain.ChainStatus) {

								if (status.Status == System.Security.Cryptography.X509Certificates.X509ChainStatusFlags.UntrustedRoot){
									// Self-signed certificate
									var serverX509Certificate2 = new X509Certificate2 (cert);
									BytesOfServerCertificate = serverX509Certificate2.Export (X509ContentType.Cert);
									FoundSelfSignedCertificate = true;

									return true;
								} 
								else if(status.Status == System.Security.Cryptography.X509Certificates.X509ChainStatusFlags.PartialChain){
									//Happens because app can't retrieve the complete chain
									var serverX509Certificate2 = new X509Certificate2 (cert);
									BytesOfServerCertificate = serverX509Certificate2.Export (X509ContentType.Cert);

									IgnoreSllErrorsInWebView = true;

									return true;
								}
								else {
									if (status.Status != System.Security.Cryptography.X509Certificates.X509ChainStatusFlags.NoError) {

										// If there are any other errors in the certificate chain, the certificate is invalid,
										// so the method returns false.
										return false;
									}
								}
							}
						}
					}

					return true;
				}

			});

			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url); 
			request.KeepAlive = false;
			request.ContentType = "application/json";
			request.Timeout	= 3000;
			request.Method = "GET";

			using (HttpWebResponse response = request.GetResponse () as HttpWebResponse) {
				Console.WriteLine (response.StatusCode.ToString ());
			};

		}


		public static async Task<CertificateValidationStatus> GetCertificateStatusForUrl (string urlToOpen)
		{
			BytesOfServerCertificate = null;

			GetCertificateFromUrl (urlToOpen);


			if (ErrorsOccured || !isCertificateChecked) {
				ResetValues ();
				return CertificateValidationStatus.Error;
			}
			else if (CertificateHelper.BytesOfServerCertificate != null) 
			{
				if (FoundSelfSignedCertificate) {
					ResetValues ();
					return CertificateValidationStatus.SelfSigned;
				} 
				else if (IgnoreSllErrorsInWebView) {
					return CertificateValidationStatus.ValidWithErrors;
				}
				else {
					ResetValues ();
					return CertificateValidationStatus.Valid;
				}
			}
			else {
				ResetValues ();
				return CertificateValidationStatus.Invalid;
			}

		}

		private static void ResetValues()
		{
			ErrorsOccured = false;
			IgnoreSllErrorsInWebView = false;
			FoundSelfSignedCertificate = false;
			isCertificateChecked = false;
		}



		public static bool RenewCertificateForLocalBox (LocalBox localBox)
		{
			//Temporarly accept all certificates to succeed
			ServicePointManager.ServerCertificateValidationCallback = (p1, p2, p3, p4) => true;

			//Current certificate
			var serverCertificateBytes = CertificateHelper.BytesOfServerCertificate;

			//Renewed Certificate
			GetCertificateFromUrl (localBox.BaseUrl);
			var newServerCertificateBytes = CertificateHelper.BytesOfServerCertificate;


			if (newServerCertificateBytes != null) {
			
				if (newServerCertificateBytes.SequenceEqual (serverCertificateBytes)) { //Compare byte arrays of certificates
					localBox.OriginalServerCertificate = newServerCertificateBytes;
					DataLayer.Instance.AddOrUpdateLocalBox (localBox);

					return true;
				} else {
					return false;
				}
			} else {
				return false;
			}
		}


	}
}