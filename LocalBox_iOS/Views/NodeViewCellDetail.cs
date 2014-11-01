using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using LocalBox_Common;
using System.Drawing;
using System.Collections.Generic;
using LocalBox_iOS.Helpers;
using System.Linq;
using MonoTouch.MessageUI;
using System.IO;

namespace LocalBox_iOS.Views
{
    public partial class NodeViewCellDetail : UIView
    {

        public static readonly UINib Nib = UINib.FromName("NodeViewCellDetail", NSBundle.MainBundle);

        private UIView _buttonContainer;

        private IListNode _nodeView;

        private TreeNode _treeNode;


        public TreeNode Node { get 
            { 
                return _treeNode;
            }
            set { 
                _treeNode = value;
                if(value != null)
                    UpdateDetail();
            }
        }

        public NodeViewCellDetail()
        {
        }

        public NodeViewCellDetail(IntPtr handle) : base(handle)
        {
        }


        void UpdateDetail()
        {
			try{
				Array.ForEach(Subviews, e => e.RemoveFromSuperview());

            	bool inRoot = ViewHelper.IsInRootNode(_treeNode);
            	bool isShare = IsShare(_treeNode);

            	List<UIButton> buttons = new List<UIButton>();
            	if (_treeNode.IsDirectory && inRoot && !_treeNode.IsShare)
            	{
            	    buttons.Add(DelenRootButton());
           	     	buttons.Add(VerwijderenButton());
            	}
            	else if (!_treeNode.IsDirectory && inRoot)
            	{
                	buttons.Add(DelenButton());
                	buttons.Add(OpenenMetButton());
                	buttons.Add(FavorietButton());
					buttons.Add(VerplaatsButton());
                	buttons.Add(VerwijderenButton());
            	}
            	else if (_treeNode.IsDirectory && !inRoot && !isShare)
            	{
                	buttons.Add(VerwijderenButton());
            	}
            	else if (!_treeNode.IsDirectory && !inRoot)
            	{
                	if (!isShare && !IsEncrypted(_treeNode))
                	{
                    	buttons.Add(DelenButton());
                	}
                	buttons.Add(OpenenMetButton());
                	buttons.Add(FavorietButton());
                	if (!isShare)
                	{
						buttons.Add(VerplaatsButton());
                    	buttons.Add(VerwijderenButton());
                	}
            	}
            	AddButtons(buttons);
			}catch{
				DialogHelper.ShowErrorDialog("Fout", "Er is een fout opgetreden bij het openen van de map.\n" +
											 "Probeer het a.u.b. nogmaals.");
			}
        }

        private bool IsShare(TreeNode node){
            if (node.Path.Count(e => e == '/') > 1)
            {
                var path = node.Path;
                int index = path.IndexOf('/', path.IndexOf('/') + 1);
                var rootFolder = path.Substring(0, index);
                var folder = DataLayer.Instance.GetFolder(rootFolder).Result;
                return folder.IsShare;
            }
            else
                return node.IsShare;
        }

        public bool IsEncrypted(TreeNode node) {
            if (node.Path.Count(e => e == '/') > 1)
            {
                var path = node.Path;
                int index = path.IndexOf('/', path.IndexOf('/') + 1);
                var rootFolder = path.Substring(0, index);
                var folder = DataLayer.Instance.GetFolder(rootFolder).Result;
                return folder.HasKeys;
            }
            else
                return node.HasKeys;
        }

        void AddButtons(List<UIButton> buttons) {
            if(buttons == null || (buttons.Count == 0))
                return;

            float spacing = 10f;
            float height = 44f;
            float width = (buttons.Count * 44) + ((buttons.Count - 1) * spacing);
            var rect = new RectangleF((Frame.Width - width) / 2, (Frame.Height - height) / 2, width, height);
            _buttonContainer = new UIView(rect);
            _buttonContainer.AutoresizingMask = UIViewAutoresizing.FlexibleMargins;

            for (int i = 0; i < buttons.Count; i++)
            {
                UIButton button = buttons[i];
                button.Frame = new RectangleF(54 * i, 0, button.Frame.Width, button.Frame.Height);
                _buttonContainer.Add(button);
            }
            Add(_buttonContainer);
        }


        private UIButton DelenRootButton() {
            var button = Button("IcDelen", null);

            button.TouchUpInside += (object sender, EventArgs e) =>
            {
                _nodeView.ShareFolder(_treeNode.Path);
            };

            return button;
        }

        private UIButton DelenButton() {
            var button = Button("IcDelen", null);
            
			button.TouchUpInside += (object sender, EventArgs e) =>
            {
				_nodeView.ShareFile(_treeNode.Path);
            };

            return button;
        }

        private UIButton FavorietButton() {
            var button =  Button("IcMaak-favoriet", null);
            button.TouchUpInside += (object sender, EventArgs e) => {
                if(_treeNode.IsFavorite) {
                    DialogHelper.ShowProgressDialog("Favoriet verwijderen", "Bestand verwijderd uit de lijst met favorieten", async () =>  {
                        await DataLayer.Instance.UnFavoriteFile(_treeNode);
                        _nodeView.Refresh(false, false);
                    });
                } else {
					DialogHelper.ShowProgressDialog("Favoriet toevoegen", "Bestand wordt aan favorieten toegevoegd", async () =>  {
                        await DataLayer.Instance.FavoriteFile(_treeNode);
                        _nodeView.Refresh(false, false);
						DialogHelper.HideProgressDialog();
                    });
                }
				DialogHelper.HideProgressDialog();
            };
            return button;
        }

        private UIButton OpenenMetButton() {
			var button = Button("IcOpenen-in", null);
            button.TouchUpInside += (object sender, EventArgs e) => {
                DialogHelper.ShowProgressDialog("Bestand ophalen", "Bestand aan het ophalen", async () => {
					try{
						string path = await DataLayer.Instance.GetFilePath(_treeNode.Path);

						if (MimeTypeHelper.GetMimeType(_treeNode.Name).Equals("application/pdf"))
						{
							string filePath = string.Empty;

							//Create temp file
							var documentsPath = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
							string temporaryFilePath = System.IO.Path.Combine (documentsPath, _treeNode.Name);

							if (File.Exists (temporaryFilePath)) {
								File.Delete (temporaryFilePath);
							}

							//Save settings of last opened file
							NSUserDefaults.StandardUserDefaults.SetString(_treeNode.Name.Substring(0, _treeNode.Name.Length - 4), "fileNameLastOpenedPdf"); 
							NSUserDefaults.StandardUserDefaults.SetString(_treeNode.Path, "pathLastOpenedPdf"); 
							NSUserDefaults.StandardUserDefaults.SetString(temporaryFilePath, "temporaryFilePath");
							NSUserDefaults.StandardUserDefaults.SetBool(_treeNode.IsFavorite, "isFavorite");
							NSUserDefaults.StandardUserDefaults.Synchronize ();

							File.Copy( path, temporaryFilePath );
						}
                    	var iac = UIDocumentInteractionController.FromUrl(NSUrl.FromFilename(path));
                    	iac.PresentOptionsMenu(ConvertRectFromView(button.Frame, this), _buttonContainer, true);

						DialogHelper.HideProgressDialog();
					}catch{
						DialogHelper.HideProgressDialog();
						DialogHelper.ShowErrorDialog("Fout", "Er is een fout opgetreden bij het openen van het bestand." +
															 "\n Ververs a.u.b. de map en probeer het opnieuw.");
					}
                });
            };
            return button;
        }


		private UIButton VerplaatsButton() {
			return Button("IcVerplaatsen", () => {
			
				HomeController homeController = HomeController.homeController;
				homeController.MoveFile(_treeNode.Path, _nodeView);

				//Hide detail cell
				//this._buttonContainer.RemoveFromSuperview();
			});
		}



        private UIButton VerwijderenButton() {
            return Button("IcVerwijderen", () => {
                
				UIAlertView alertView = new UIAlertView("Waarschuwing", 
													"Bent u zeker van deze verwijder actie? \nDeze actie is niet terug te draaien.", 
													null, 
													"Annuleer",
													"Verwijder");
				alertView.Clicked += delegate(object a, UIButtonEventArgs eventArgs) {
					if(eventArgs.ButtonIndex == 1){
						DialogHelper.ShowProgressDialog("Verwijderen", "Bezig om een bestand of map te verwijderen", async () => {
							bool result = await DataLayer.Instance.DeleteFileOrFolder(_treeNode.Path);
							_nodeView.Refresh(scrollToTop: true);
							if(!result) {
								DialogHelper.ShowErrorDialog("Fout", "Er is een fout opgetreden bij het verwijderen van het bestand of map." +
																	 "\nHet kan zijn dat de geselecteerde map of bestand niet meer bestaat.");
							}
							DialogHelper.HideProgressDialog();
						});
					}
				};
				alertView.Show();

            });
        }

        private UIButton Button(String image, Action action) {
            var button = new UIButton(UIButtonType.Custom);
            button.Frame = new RectangleF(0, 0, 44, 44);
            button.SetImage(UIImage.FromBundle(image), UIControlState.Normal);
            button.AutoresizingMask = UIViewAutoresizing.None;
            button.TouchUpInside += delegate
            {
                if(action != null) {
                    action();
                }
            };
            return button;
        }

		public static NodeViewCellDetail Create(IListNode nodeView)
        {
            var view = (NodeViewCellDetail)Nib.Instantiate(null, null)[0];
            view._nodeView = nodeView;
            return view;
        }
    }
}

