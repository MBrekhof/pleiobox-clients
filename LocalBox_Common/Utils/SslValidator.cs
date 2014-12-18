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


		public SslValidator ()
		{
			localBox = DataLayer.Instance.GetSelectedOrDefaultBox ();
			LoadRootCertificate (); 
		}


		private void LoadRootCertificate ()
		{
			originalServerCertificate = new X509Certificate (localBox.OriginalServerCertificate);
		}


		public bool ValidateServerCertficate (object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			if (originalServerCertificate == null) {
				return false;
			} else {
				if (originalServerCertificate.Equals (cert)) {
					return true;
				} 
				else {
					//incorrect certificate found so notify user
					CertificateHelper.BytesOfServerCertificate = cert.Export (X509ContentType.Cert);

					EventHandler handler = CertificateMismatchFound;
					if (handler != null) {
						handler (this, null);
					}
					return false;
				}
			}
		}


	}
}



