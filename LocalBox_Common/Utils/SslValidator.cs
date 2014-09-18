using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace LocalBox_Common
{
	public class SslValidator
	{
		private X509Certificate2 _rootCertificate;
		private LocalBox localBox;

		public SslValidator ()
		{
			localBox = DataLayer.Instance.GetSelectedOrDefaultBox ();

			LoadRootCertificate (); 
		}

		private void LoadRootCertificate ()
		{
			_rootCertificate = GetCertFromPEM ();
		}

		X509Certificate2 GetCertFromPEM ()
		{
			var certificateBytes = localBox.OriginalSslCertificate;

			return new X509Certificate2 (certificateBytes);
		}


		public bool ValidateServerCertficate (object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			if (_rootCertificate == null) {
				return false;
			} else {
				return ValidateRootCertificate (cert, chain, _rootCertificate, sslPolicyErrors);
			}
		}

		private bool ValidateRootCertificate (X509Certificate cert, X509Chain chain, X509Certificate2 root, SslPolicyErrors sslPolicyErrors)
		{
			X509Chain newChain = new X509Chain ();
			newChain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain; 
			newChain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;

			foreach (var certificate in chain.ChainPolicy.ExtraStore) {
				// Root certificaten die niet door ons zelf zijn toegevoegd verwijderen
				if (!certificate.Issuer.Equals (certificate.Subject)) {
					newChain.ChainPolicy.ExtraStore.Add (certificate);
				}
			}

			// Root certificate toevoegen aan onze chain
			newChain.ChainPolicy.ExtraStore.Add (root);

			// Omdat we zelf de root toevoegen aan de chain, is deze untrusted.
			// Dit zal dan ook de enige error zijn die we doorlaten, de rest knalt die op.

			// Als we dus te maken hebben met een Man in The Middle attack (mitma), zal de error PartialChain zijn.

			// Eerst de chain opbouwen zodat het certificaat erin wordt opgenomen + alle errors.
			newChain.Build (new X509Certificate2 (cert));
			bool validRequest = true;

			if (sslPolicyErrors == SslPolicyErrors.None) {
				foreach (var errors in newChain.ChainStatus) {
					// Alle errors laten falen, behalve UntrustedRoot
					if (errors.Status != X509ChainStatusFlags.UntrustedRoot) {
						validRequest = false;
						break;
					}
				}
			} else
				validRequest = false;

			return validRequest;
		}
	}
}



