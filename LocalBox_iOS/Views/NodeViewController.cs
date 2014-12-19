using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using LocalBox_Common.Remote;

using LocalBox_Common;
using LocalBox_iOS.Helpers;
using LocalBox_iOS.Views.ItemView;

using Xamarin;

namespace LocalBox_iOS.Views
{
    public partial class NodeViewController : UIViewController
    {
        readonly List<BaseNode> _nodes;
        const int width = 400;
        readonly UIColor _balkKleur = null;
		private IHome _home;

		public NodeViewController(string balkKleur,  IHome _home) : base("NodeViewController", null)
        {
			this._home = _home;

			if (!string.IsNullOrEmpty(balkKleur) && balkKleur.StartsWith("#")) {
                float red, green, blue;
                var colorString = balkKleur.Replace ("#", "");
                red = Convert.ToInt32(string.Format("{0}", colorString.Substring(0, 2)), 16) / 255f;
                green = Convert.ToInt32(string.Format("{0}", colorString.Substring(2, 2)), 16) / 255f;
                blue = Convert.ToInt32(string.Format("{0}", colorString.Substring(4, 2)), 16) / 255f;
                _balkKleur =  UIColor.FromRGBA(red, green, blue, 1.0f);
            }            
            else
            {
                // Defaultkleur
                _balkKleur = UIColor.FromRGB(143, 202, 232);
            }


            _nodes = new List<BaseNode>();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            kleurenBalk.BackgroundColor = _balkKleur;

            var node = NodeView.Create(new RectangleF(0, 0, width, View.Frame.Height), this, "/", _balkKleur);
            _nodes.Add(node);

            Add(_nodes[0]);
        }

        public void ShowNode(TreeNode treeNode, BaseNode sender)
        {
            if (_nodes.Last().Node != null && treeNode.Id == _nodes.Last().Node.Id)
                return;
            int idx = _nodes.IndexOf(sender);
            if (sender != _nodes.Last())
            {
                _nodes.RemoveRange(idx + 1, _nodes.Count - idx - 1);
            }

            if (treeNode.IsDirectory)
            {
				try{
					var node = NodeView.Create(new RectangleF(View.Frame.Width, 0, width, View.Frame.Height), this, treeNode.Path, _balkKleur);
                	node.Layer.ZPosition = 10;
                	_nodes.Add(node);
                	Add(node);
                	AnimateViews();
				} catch (Exception ex){
					Insights.Report(ex);
					DialogHelper.ShowErrorDialog("Fout", "Er is een fout opgetreden bij het openen van de map.\n" +
												 "Ververs de huidige map en probeer het a.u.b. nogmaals.");
				}
            }
            else
            {
                string filePath = string.Empty;

                InvokeOnMainThread(() =>
                    DialogHelper.ShowProgressDialog("Bestand downloaden", "Bezig met het downloaden van een bestand", 
                    () => filePath = DataLayer.Instance.GetFilePathSync(treeNode.Path),
                    ()=> {
						try{

                        	var item = 
                            WebItemView.Create(new RectangleF(View.Frame.Width, 0, width, View.Frame.Height), this, treeNode, filePath, _balkKleur);

                        	item.Layer.ZPosition = 10;
                        	_nodes.Add(item);
                        	Add(item);
                        	AnimateViews();
						}
						catch (Exception ex){
							Insights.Report(ex);
							DialogHelper.ShowErrorDialog("Fout", "Er is een fout opgetreden bij het openen van het bestand." +
														 "\nVervers a.u.b. de map en probeer het opnieuw.");
						}

                    })
                );
            }
        }

        
		public void ShowFavorite ()
		{
			try {
				if (_nodes.Last ().GetType ().Equals (typeof(FavoriteNodeView)))
					return;

				var node = FavoriteNodeView.Create (new RectangleF (View.Frame.Width, 0, width, View.Frame.Height), this, _balkKleur);
				node.Node = new TreeNode () {
					ParentId = _nodes.Last ().Node.Id
				};
				node.Layer.ZPosition = 10;

				if (_nodes.Count > 1) {
					_nodes.RemoveRange (1, _nodes.Count - 1);
				}

				_nodes.Add (node);
				Add (node);
				AnimateViews ();
			}  catch (Exception ex){
				Insights.Report(ex);
			}
		}

        public void Refresh()
        {

            if(_nodes.Last().GetType() == typeof(FavoriteNodeView))
                return;

            // Create a copy of _nodes
            var nodes = _nodes.Select(e => e).ToList();

            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                if(!DataLayer.Instance.NodeExists(nodes[i].Node)){
                    _nodes.Remove(nodes[i]);
                    if (_nodes.Count - 2 >= 0)
                    {
                        var add = _nodes[_nodes.Count - 2];
                        add.Frame = new RectangleF(new PointF(-add.Frame.Width, 0), add.Frame.Size);
                        Add(add);
                    }
                }
            }
            _nodes.Last().Layer.ZPosition = 1;
            AnimateViews();
        }

        public void PopView()
        {
            var toPop = _nodes.Last();

            NSAction onCompletion = () =>
            {
                toPop.Layer.ZPosition = -10;
                _nodes.Remove(toPop);

                if (_nodes.Count >= 2)
                {
                    var oldNode = _nodes[_nodes.Count - 2];
                    oldNode.Frame = new RectangleF(new PointF(0, 0), oldNode.Frame.Size);
                    _nodes.Last().Layer.ZPosition = 1;
                    Add(oldNode);
                }
                AnimateViews();
            };


            var rootView = UIApplication.SharedApplication.KeyWindow.RootViewController.View;
            if (rootView.Subviews.Contains(toPop))
            {
                ToggleFullScreen(onCompletion);
            }
            else
            {
                onCompletion();
            }
        }

        public void ToggleFullScreen(NSAction onCompletion = null)
        {


            BaseNode last = _nodes.Last();
            UIView rootView = UIApplication.SharedApplication.KeyWindow.RootViewController.View;

            if (rootView.Subviews.Contains(last))
            {
                View.Add(last);
                last.Frame = new RectangleF(-View.Frame.X, View.Frame.Y, last.Bounds.Width, last.Bounds.Height);
                UIView.Animate(0.5, () =>
                {
                    last.Frame = new RectangleF(new PointF(width, 0), new SizeF(width, View.Bounds.Height)); 
                    last.ViewWillResize();
                }, onCompletion);
            }
            else
            {
                UIApplication.SharedApplication.KeyWindow.RootViewController.View.Add(last);
                last.Frame = new RectangleF(last.Frame.X + View.Frame.X, last.Bounds.Y, last.Bounds.Width, last.Bounds.Height);
                UIView.Animate(0.5, () =>
                {
                    last.Frame = rootView.Bounds;
                    last.ViewWillResize();
                }, onCompletion);
            }
        }
          
        private void AnimateViews() {
            UIView.Animate(0.5, 0, UIViewAnimationOptions.CurveEaseOut, () =>
            {
                if(_nodes.Count > 2) {
                    var nodes = _nodes.GetRange(_nodes.Count - 2, 2);
                    for(int i = 0; i < nodes.Count; i++) {
                        var node = nodes[i];
                        node.Frame = new RectangleF(new PointF(i * width, 0), node.Frame.Size);
                    }
                } else {
                    // hier gewoon de views naastelkaar plakken
                    for(int i = 0; i < _nodes.Count; i++) {
                        var node = _nodes[i];
                        node.Frame = new RectangleF(new PointF(i *width, 0), node.Frame.Size);
                    }
                }
                for(int i = 0; i < _nodes.Count; i++) {
                    var node = _nodes[i];
                    if(i < 2 || _nodes.Count - 1 != i) {
                        node.HideBackButton();
                    } else {
                        node.ShowBackButton();
                    }
                }
            }, () =>
            {
                if(_nodes.Count > 2) {
                    _nodes[_nodes.Count - 3].RemoveFromSuperview();
                }
                var viewsThatShouldntBeShown = View.Subviews.Except(_nodes).Where(e => e is BaseNode);
                foreach(UIView view in viewsThatShouldntBeShown) {
                    view.RemoveFromSuperview();
                }
                for(int i = 0; i < _nodes.Count -1; i++) {
                    //reset alle Zposities (behalve die van de laatste!)
                    _nodes[i].Layer.ZPosition = 0;
                }
            });
        }
    }
}

