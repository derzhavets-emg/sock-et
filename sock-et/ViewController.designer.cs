// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace socket
{
	[Register ("ViewController")]
	partial class ViewController
	{
		[Outlet]
		AppKit.NSButton BtnStartServerText { get; set; }

		[Outlet]
		AppKit.NSTextField ClientsInput { get; set; }

		[Outlet]
		AppKit.NSTextField IpInput { get; set; }

		[Outlet]
		AppKit.NSTextField MessageInput { get; set; }

		[Outlet]
		AppKit.NSTextField PortInput { get; set; }

		[Outlet]
		AppKit.NSTextField ReceivedMesages { get; set; }

		[Action ("BtnLoadTemplateStatus:")]
		partial void BtnLoadTemplateStatus (Foundation.NSObject sender);

		[Action ("BtnSendMessage:")]
		partial void BtnSendMessage (Foundation.NSObject sender);

		[Action ("BtnStartServer:")]
		partial void BtnStartServer (Foundation.NSObject sender);

		[Action ("ClickButton:")]
		partial void ClickButton (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (BtnStartServerText != null) {
				BtnStartServerText.Dispose ();
				BtnStartServerText = null;
			}

			if (ClientsInput != null) {
				ClientsInput.Dispose ();
				ClientsInput = null;
			}

			if (IpInput != null) {
				IpInput.Dispose ();
				IpInput = null;
			}

			if (PortInput != null) {
				PortInput.Dispose ();
				PortInput = null;
			}

			if (MessageInput != null) {
				MessageInput.Dispose ();
				MessageInput = null;
			}

			if (ReceivedMesages != null) {
				ReceivedMesages.Dispose ();
				ReceivedMesages = null;
			}
		}
	}
}
