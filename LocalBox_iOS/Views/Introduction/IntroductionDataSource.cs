using System;
using UIKit;
using CoreGraphics;

namespace LocalBox_iOS
{
	public class IntroductionDataSource : UIPageViewControllerDataSource
	{
		UIViewController[] pages; 

		public int indexOfPage = 0;

		public IntroductionDataSource(UIViewController[] pages)
		{
			this.pages = pages;
		}
						
		override public UIViewController GetPreviousViewController(UIPageViewController pageViewController, UIViewController referenceViewController)
		{
			int index = Array.FindIndex (pages, p => p.Equals (referenceViewController));

			if (index == 0) {
				return null;
			} else {
				return pages[index-1];
			}
		}

		override public UIViewController GetNextViewController(UIPageViewController pageViewController, UIViewController referenceViewController)
		{
			int index = Array.FindIndex (pages, p => p.Equals (referenceViewController));

			if (index == pages.Length-1) {
				return null;
			} else {
				return pages[index+1];
			}
		}
	}
}

