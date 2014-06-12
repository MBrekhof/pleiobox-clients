using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using LocalBox_iOS.Helpers;

namespace LocalBox_iOS.Views
{
    public partial class NodeViewCellDefault : UIView
    {

        public static readonly UINib Nib = UINib.FromName("NodeViewCellDefault", NSBundle.MainBundle);

        public UIImage Type { 
            get  {
                return TypeIcon.Image;
            } set {
                TypeIcon.Image = value;
            }
        }

        public UIImage Deel { 
            get  {
                return DeelIcon.Image;
            } set {
                DeelIcon.Image = value;
            }
        }

        public string Titel { 
            get  {
                return TitelLabel.Text;
            } set {
                TitelLabel.Text = value;
            }
        }

        public NodeViewCellDefault()
        {}

        public NodeViewCellDefault (IntPtr handle) : base (handle)
        {
        }

        public static NodeViewCellDefault Create()
        {
            var view = (NodeViewCellDefault)Nib.Instantiate(null, null)[0];
            view.TitelLabel.SetFont();
            return view;
        }
    }
}

