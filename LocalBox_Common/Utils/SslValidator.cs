using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace LocalBox_Common
{
	public class SslValidator
	{
		public static event EventHandler CertificateMismatchFound;
		public static bool CertificateErrorRaised = false;
		private X509Certificate originalServerCertificate;
		private LocalBox localBox;


		public SslValidator (LocalBox localBox)
		{
			this.localBox = localBox;
			LoadRootCertificate (); 
		}


		private void LoadRootCertificate ()
		{
			originalServerCertificate = new X509Certificate (localBox.OriginalServerCertificate);
		}


		public bool ValidateServerCertficate (object sender, X509Certificate receivedCertificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			if (originalServerCertificate == null) {
				return false;
			} else {

				if (receivedCertificate.Subject.IndexOf(".xamarin.com", 0, StringComparison.CurrentCultureIgnoreCase) == -1) { //not a call to a Xamarin server so verify certificate

					if (originalServerCertificate.Equals (receivedCertificate)) {
						return true;
					} else {
						//incorrect certificate found so notify user
						CertificateHelper.BytesOfServerCertificate = receivedCertificate.Export (X509ContentType.Cert);

						EventHandler handler = CertificateMismatchFound;
						if (handler != null) {
							handler (this, null);
						}
						return false;
					}
				} else {
					//Call to Xamarin (used for Xamarin.Insights) so accept
					return true;
				}
			}
		}


	}
}



