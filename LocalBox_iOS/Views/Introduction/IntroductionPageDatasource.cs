using System;
using UIKit;
using CoreGraphics;
using LocalBox_iOS.Views;

namespace LocalBox_iOS
{
	public class IntroductionPageDataSource : UIPageViewControllerDataSource
	{
		private UIPageViewController parentController;
		private HomeController homeController;

		private UIPageControl pageControl;
		public int indexOfPage = 0;

		public int TotalPages
		{
			get { return 4; }
		}

		public IntroductionPageDataSource(UIPageViewController parentController, HomeController homeController)
		{
			this.parentController = parentController;
			this.homeController = homeController;

			if (UIDevice.CurrentDevice.CheckSystemVersion (8, 0)) { //iOS 8
				pageControl = new UIPageControl {
					Pages = TotalPages,
					Frame = new CGRect (
						parentController.View.Frame.Width / 2 - 50,  //X
						parentController.View.Frame.Height - 75, 	//Y
						100, 										//Width
						50) 										//Height
				};
			} else { //iOS7
				pageControl = new UIPageControl { 
					Pages = TotalPages,
					Frame = new CGRect (
						parentController.View.Frame.Width / 2 - 50,  //X
						parentController.View.Frame.Height - 330, 	//Y
						100, 										//Width
						50) 										//Height
				};
			}

			pageControl.Enabled = false;
			pageControl.ValueChanged += HandlePageControlValueChanged;

			parentController.View.AddSubview(pageControl);
		}


		void HandlePageControlValueChanged (object sender, EventArgs e)
		{
			Console.WriteLine ("Change page!");

		}



		public override UIViewController GetPreviousViewController (UIPageViewController pageViewController, UIViewController referenceViewController)
		{
			IntroductionViewController currentPageController = referenceViewController as IntroductionViewController;

			if (currentPageController.pageIndex > 0) {
				indexOfPage--;
				pageControl.CurrentPage = currentPageController.pageIndex;

				int previousPageIndex = currentPageController.pageIndex - 1;
				return new IntroductionViewController (previousPageIndex, parentController, homeController);
			} 
			else if (currentPageController.pageIndex == 0) {
				pageControl.CurrentPage = currentPageController.pageIndex;

				return null;
			}
			else {
				return null;
			}
		}

		public override UIViewController GetNextViewController (UIPageViewController pageViewController, UIViewController referenceViewController)
		{
			IntroductionViewController currentPageController = referenceViewController as IntroductionViewController;

			if (currentPageController.pageIndex < TotalPages  - 1) {
				indexOfPage++;
				pageControl.CurrentPage = currentPageController.pageIndex;

				int nextPageIndex = currentPageController.pageIndex + 1;
				return new IntroductionViewController (nextPageIndex, pageViewController, homeController);
			} 
			else if (currentPageController.pageIndex == TotalPages - 1) {
				pageControl.CurrentPage = currentPageController.pageIndex;

				return null;
			}

			else {
				return null;
			}
		}

	}
}

