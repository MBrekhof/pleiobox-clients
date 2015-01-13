using System;
using UIKit;

namespace LocalBox_iOS.Helpers
{
    public static class FontHelper
    {
        public static void SetFont(this UILabel label)
        {
//            label.Font = UIFont.FromName("RijksoverheidSansTextTT-Regular", 16f);
            label.Font = UIFont.SystemFontOfSize(16f);
        }
    }
}

