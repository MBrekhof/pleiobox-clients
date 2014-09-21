using System;
using System.IO;
using System.Net;
using System.Text;
using System.Security;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace LocalBox_Common
{
	public class CertificateHelper
	{
		public static byte[] BytesOfCertificate;

		public static void StoreCertificateFromUrl (string url)
		{
			Environment.SetEnvironmentVariable ("MONO_TLS_SESSION_CACHE_TIMEOUT", "0");

			ServicePointManager.CheckCertificateRevocationList = true;
			ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback((sender, cert, chain, errors) => 
				{ 
					if(errors == SslPolicyErrors.None)
					{
						foreach (var certificate in chain.ChainPolicy.ExtraStore) 
						{
							BytesOfCertificate = certificate.Export(X509ContentType.Cert);
							return true;
						}

						var certi = new X509Certificate2(cert);
						BytesOfCertificate = certi.Export(X509ContentType.Cert);
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
			BytesOfCertificate = null;

			StoreCertificateFromUrl (urlToOpen);
			var certificateBytes = CertificateHelper.BytesOfCertificate;

			if (certificateBytes != null) {
				return true;
			} else {
				return false;
			}
		}

	}
}