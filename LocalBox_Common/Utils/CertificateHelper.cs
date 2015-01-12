using System;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using System.Security;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using LocalBox_Common.Remote;
using System.Threading.Tasks;
using Xamarin;

namespace LocalBox_Common
{
	public class CertificateHelper
	{
		public static byte[] BytesOfServerCertificate;


		public static void GetCertificateFromUrl (string url)
		{
			Environment.SetEnvironmentVariable ("MONO_TLS_SESSION_CACHE_TIMEOUT", "0");

			ServicePointManager.CheckCertificateRevocationList = true;
			ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback((sender, cert, chain, errors) => 
				{ 
					if(errors == SslPolicyErrors.None) //self signed ssl certificate retrieves: System.Net.Security.SslPolicyErrors.RemoteCertificateChainErrors
					{
						var serverX509Certificate2 = new X509Certificate2(cert);
						BytesOfServerCertificate = serverX509Certificate2.Export(X509ContentType.Cert);
					}
					return true;
				});

			using (var client = new WebClient ()) {
				try {
					client.DownloadString(new Uri (url));

				} catch (Exception ex){
					Insights.Report(ex);
					Console.WriteLine (ex.Message);
				}
			}


		}


		public static bool DoesHaveAValidCertificate (string urlToOpen)
		{
			BytesOfServerCertificate = null;

			GetCertificateFromUrl (urlToOpen);
			var certificateBytes = CertificateHelper.BytesOfServerCertificate;

			if (certificateBytes != null) {
				return true;
			} else {
				return false;
			}
		}


		public static bool RenewCertificateForLocalBox(LocalBox localBox)
		{
			//Temporarly accept all certificates to succeed
			ServicePointManager.ServerCertificateValidationCallback = (p1, p2, p3, p4) => true;

			//Current certificate
			var serverCertificateBytes = CertificateHelper.BytesOfServerCertificate;

			//Renewed Certificate
			GetCertificateFromUrl (localBox.BaseUrl);
			var newServerCertificateBytes = CertificateHelper.BytesOfServerCertificate;


			if (newServerCertificateBytes != null) {
			
				if(newServerCertificateBytes.SequenceEqual(serverCertificateBytes)) //Compare byte arrays of certificates
				{
					localBox.OriginalServerCertificate = newServerCertificateBytes;
					DataLayer.Instance.AddOrUpdateLocalBox (localBox);

					return true;
				}else {
					return false;
				}
			} else {
				return false;
			}
		}


	}
}