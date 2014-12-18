using System;

namespace LocalBox_Droid
{
	public static class LockHelper
	{
		private static DateTime timeLastActivityOpened = DateTime.Now;
		private static string nameLastActivity;


		public static void SetLastActivityOpenedTime(string activityName){
			LockHelper.timeLastActivityOpened = DateTime.Now;
			LockHelper.nameLastActivity 	  = activityName;
		}

		public static bool ShouldLockApp(string activityName)
		{
			if (activityName == LockHelper.nameLastActivity) {
				DateTime now = DateTime.Now;

				double minutes = (now - timeLastActivityOpened).TotalMinutes;
				if (minutes >= 15) {
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

