using System;

using AppKit;
using Foundation;

namespace socket
{
    public partial class ViewController : NSViewController
    {
        private int clickedNumber = 0;

        public ViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Do any additional setup after loading the view.
            //ClickedLabel.StringValue = "Button has not been clicked yet";
        }

        public override NSObject RepresentedObject
        {
            get
            {
                return base.RepresentedObject;
            }
            set
            {
                base.RepresentedObject = value;
                // Update the view, if already loaded.
            }
        }

        //partial void ClickButton(NSObject sender)
        //{
        //    ClickedLabel.StringValue = string.Format("The button has been clicked {0} time{1}.", ++clickedNumber, (clickedNumber < 2) ? "" : "s");
        //}
    }
}
