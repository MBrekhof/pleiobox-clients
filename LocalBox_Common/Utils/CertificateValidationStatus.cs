using System;

namespace LocalBox_Common
{

	public enum CertificateValidationStatus
	{
		NotDetermined,
		Valid,
		ValidWithErrors,
		SelfSigned,
		Invalid,
		Error
	}
}

