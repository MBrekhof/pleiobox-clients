using System;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using System.Security;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

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


		public static bool VerifyCertificateForLocalBox(LocalBox localBox)
		{
			var serverCertificateBytes = CertificateHelper.BytesOfServerCertificate;

			if (serverCertificateBytes != null) {
			
				//TODO: Get certificate from remote PEM file
				byte[] BytesOfRemotePEMCertificate = CertificateHelper.BytesOfServerCertificate;

				if(BytesOfRemotePEMCertificate.SequenceEqual(BytesOfServerCertificate)) //Compare byte arrays of certificates
				{
					localBox.OriginalServerCertificate = serverCertificateBytes;
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