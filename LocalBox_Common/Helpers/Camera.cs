using System;

using Xamarin.Media;

namespace LocalBox_Common
{
	public class Camera
	{
		public Camera ()
		{
		}
			

		public static void PictureFromCamera (MediaPicker picker, Action<MediaFile> callback)
		{
			if (!picker.IsCameraAvailable || !picker.PhotosSupported) {
				//ToDo: How to handle unsupported
				return;
			}
			picker.TakePhotoAsync (new StoreCameraMediaOptions
			{
				Name = string.Format("photo{0}.jpg", Guid.NewGuid()),
				Directory = "LB"
			}).ContinueWith (t => {
				if(t.IsCanceled)
					return;

				callback(t.Result);
			});
		}
			
		public static void PictureFromGallery (MediaPicker picker, Action<MediaFile> callback)
		{
			if (!picker.PhotosSupported) {
				//ToDo: How to handle unsupported
				return;
			}
			picker.PickPhotoAsync().ContinueWith (t =>
			{
				if(t.IsCanceled)
					return;

				callback(t.Result);
			});
		}

	}
}

