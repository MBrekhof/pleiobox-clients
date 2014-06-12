namespace localbox.android
{
    using System.IO;

	public static class FileSystemHelper
    {
	
		public static bool IsDirectory(this FileSystemInfo fileSystemInfo)
        {
			if (fileSystemInfo == null)
            {
                return false;
            }
			return (fileSystemInfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory;
        }
			

		public static bool IsFile(this FileSystemInfo fileSystemInfo)
        {
			return !IsDirectory(fileSystemInfo);
        }


		public static bool IsVisible(this FileSystemInfo fileSystemInfo)
        {
			if (fileSystemInfo == null)
            {
                return false;
            }

			var isHidden = (fileSystemInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
            return !isHidden;
        }
    }
}
