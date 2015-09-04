﻿using System;
using UIKit;
using Foundation;
using System.Collections.Generic;
using CoreGraphics;

namespace DHDialogs
{
	public abstract class DHDialogView : UIView
	{

		#region Fields

		private String _Title;

		private String _Message;

		private String _SubmitButtonText;

		private String _CancelButtonText;

		private DHDialogType _BoxType;

		private nint _NumberOfDecimals;

		private UIBlurEffectStyle _BlurEffectStyle = UIBlurEffectStyle.Light;

		private List<UIView> _Elements;

		private UIVisualEffectView _VisualEffectView;

		private UITextField _TextInput;

		private UITextField _SecureInput;

		private UIView _ActualBox;

		private NSObject mKeyboardDidSHowNotification;
		private NSObject mKeyboardDidHideNotification;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the content view.
		/// </summary>
		/// <value>The content view.</value>
		protected abstract UIView ContentView {get;}

		/// <summary>
		/// Gets or sets the title.
		/// </summary>
		/// <value>The title.</value>
		public String Title {
			get {
				return this._Title;
			}
			set {
				this._Title = value;
			}
		}

		/// <summary>
		/// Gets or sets the message.
		/// </summary>
		/// <value>The message.</value>
		public String Message {
			get {
				return this._Message;
			}
			set {
				this._Message = value;
			}
		}

		/// <summary>
		/// Gets or sets the submit button text.
		/// </summary>
		/// <value>The submit button text.</value>
		public String SubmitButtonText {
			get {
				return this._SubmitButtonText;
			}
			set {
				this._SubmitButtonText = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this instance cancel button text.
		/// </summary>
		/// <value><c>true</c> if this instance cancel button text; otherwise, <c>false</c>.</value>
		public String CancelButtonText {
			get {
				return this._CancelButtonText;
			}
			set {
				this._CancelButtonText = value;
			}
		}

		/// <summary>
		/// Gets the type of the box.
		/// </summary>
		/// <value>The type of the box.</value>
		public DHDialogType BoxType {
			get {
				return this._BoxType;
			}
			private set {
				this._BoxType = value;
			}
		}
			
		/// <summary>
		/// Gets or sets the blur effect style.
		/// </summary>
		/// <value>The blur effect style.</value>
		public UIBlurEffectStyle BlurEffectStyle {
			get {
				return this._BlurEffectStyle;
			}
			set {
				this._BlurEffectStyle = value;
			}
		}
			
		private UIVisualEffectView VisualEffectView {
			get {
				return this._VisualEffectView;
			}
			set {
				this._VisualEffectView = value;
			}
		}
			
		private UIView ActualBox {
			get {
				return this._ActualBox;
			}
			set {
				this._ActualBox = value;
			}
		}
			
		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="DHDialogs.DHDialogView"/> constantly update any value changes
		/// </summary>
		/// <value><c>true</c> if constant updates; otherwise, <c>false</c>.</value>
		public bool ConstantUpdates {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the button mode.
		/// </summary>
		/// <value>The button mode.</value>
		public ButtonMode ButtonMode {
			get;
			set;
		}
		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="MKInputBoxView.MKInputBoxView"/> class.
		/// </summary>
		/// <param name="boxType">Box type.</param>
		public DHDialogView (DHDialogType boxType)
			: base ()
		{
			var actualBoxHeight = 155.0f;
			var window = UIApplication.SharedApplication.Windows [0];
			var allFrame = window.Frame;

			var boxFrame = new CGRect (0, 0, Math.Min (325, window.Frame.Size.Width - 50), actualBoxHeight);

			this.Frame = allFrame;

			this.BoxType = boxType;
			this.BackgroundColor = UIColor.FromWhiteAlpha (1.0f, 0.0f);
			this.AutoresizingMask = UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleHeight;
			this.ActualBox = new UIView (boxFrame);

			this.ActualBox.Center = new CGPoint (window.Center.X, window.Center.Y);

			this.Center = new CGPoint (window.Center.X, window.Center.Y);

			this.Add (this.ActualBox);

		}

		#endregion

		#region Methods

		/// <summary>
		/// Show this instance.
		/// </summary>
		public void Show ()
		{

			this.Alpha = 0.0f;

			SetupView ();

			UIView.Animate (0.3f, () => {
				this.Alpha = 1.0f;
			});

			var window = UIApplication.SharedApplication.Windows [0];
			window.Add (this);
			window.BringSubviewToFront (this);

			UIDevice.CurrentDevice.BeginGeneratingDeviceOrientationNotifications ();


			NSNotificationCenter.DefaultCenter.AddObserver (new NSString ("UIDeviceOrientationDidChangeNotification"), DeviceOrientationDidChange);

			mKeyboardDidSHowNotification = UIKeyboard.Notifications.ObserveDidShow ((s, e) => {
				KeyboardDidShow (e.Notification);
			});


			mKeyboardDidHideNotification = UIKeyboard.Notifications.ObserveDidHide ((s, e) => {
				KeyboardDidHide (e.Notification);

			});

		}

		/// <summary>
		/// Hide this instance.
		/// </summary>
		public void Hide ()
		{
			UIView.AnimateNotify (0.3f, () => {
				this.Alpha = 0.0f;
			}, (finished) => {
				this.RemoveFromSuperview ();

				UIDevice.CurrentDevice.EndGeneratingDeviceOrientationNotifications ();

				NSNotificationCenter.DefaultCenter.RemoveObserver (new NSString ("UIDeviceOrientationDidChangeNotification"));

				mKeyboardDidSHowNotification.Dispose ();
				mKeyboardDidHideNotification.Dispose ();
			});

		}

		/// <summary>
		/// Setups the view.
		/// </summary>
		private void SetupView ()
		{
			
			// 
			this.ActualBox.Layer.CornerRadius = 4.0f;
			this.ActualBox.Layer.MasksToBounds = true;

			// 
			UIColor titleLabelTextColor = null;
			UIColor messageLabelTextColor = null;
			UIColor elementBackgroundColor = null;
			UIColor buttonBackgroundColor = null;

			// 
			var style = this.BlurEffectStyle;

			// 
			switch (style) {
			case UIBlurEffectStyle.Dark:
				{
					titleLabelTextColor = UIColor.White;
					messageLabelTextColor = UIColor.White;
					elementBackgroundColor = UIColor.FromWhiteAlpha (1.0f, 0.07f);
					buttonBackgroundColor = UIColor.FromWhiteAlpha (1.0f, 0.07f);
				}
				break;
			default:
				{
					titleLabelTextColor = UIColor.Black;
					messageLabelTextColor = UIColor.Black;
					elementBackgroundColor = UIColor.FromWhiteAlpha (1.0f, 0.50f);
					buttonBackgroundColor = UIColor.FromWhiteAlpha (1.0f, 0.2f);
				}

				break;
			}


			this.VisualEffectView = new UIVisualEffectView (UIBlurEffect.FromStyle (style));

			var padding = 10.0f;
			var width = this.ActualBox.Frame.Size.Width - padding * 2;


			UILabel titleLabel = new UILabel (new CGRect (padding, padding, width, 20));

			titleLabel.Font = UIFont.BoldSystemFontOfSize (17.0f);
			titleLabel.Text = this.Title;
			titleLabel.TextAlignment = UITextAlignment.Center;
			titleLabel.TextColor = titleLabelTextColor;

			this.VisualEffectView.ContentView.Add (titleLabel);

			var messageLabel = new UILabel (new CGRect (padding, padding + titleLabel.Frame.Size.Height + 5, width, 31.5));
			messageLabel.Lines = 2;
			messageLabel.Font = UIFont.SystemFontOfSize (13.0f);
			messageLabel.Text = this.Message;
			messageLabel.TextAlignment = UITextAlignment.Center;
			messageLabel.TextColor = messageLabelTextColor;

			messageLabel.SizeToFit ();

			this.VisualEffectView.ContentView.Add (messageLabel);

			var aView = this.ContentView;

			var conFrame = aView.Frame;
			conFrame.Y = messageLabel.Frame.Y + messageLabel.Frame.Height + padding / 1.5f;
			conFrame.X = (this.ActualBox.Frame.Size.Width / 2) - (conFrame.Width / 2);
			aView.Frame = conFrame;

			CGRect extendedFrame = this.ActualBox.Frame;
			extendedFrame.Height = 110.0f + padding;
			extendedFrame.Height += aView.Frame.Height;
			this.ActualBox.Frame = extendedFrame;

			this.VisualEffectView.ContentView.Add (aView);
			// 
			foreach (UIView element in aView.Subviews) 
			{
				if (element is UITextField) 
				{
					element.Layer.BorderColor = UIColor.FromWhiteAlpha (0.0f, 0.1f).CGColor;
					element.Layer.BorderWidth = 0.5f;
					element.BackgroundColor = elementBackgroundColor;
				}

			}
			// 
			var buttonHeight = 40.0f;
			var buttonWidth = this.ActualBox.Frame.Size.Width / 2;

			// 
			UIButton cancelButton = new UIButton (new CGRect (0, this.ActualBox.Frame.Size.Height - buttonHeight, buttonWidth, buttonHeight));
			cancelButton.SetTitle (!(String.IsNullOrWhiteSpace (this.CancelButtonText)) ? this.CancelButtonText : @"Cancel", UIControlState.Normal);
			cancelButton.TouchUpInside += (object sender, EventArgs e) => {
				CancelButtonTapped ();

			};


			cancelButton.TitleLabel.Font = UIFont.SystemFontOfSize (16.0f);
			cancelButton.SetTitleColor (titleLabelTextColor, UIControlState.Normal);
			cancelButton.SetTitleColor (UIColor.Gray, UIControlState.Highlighted);
			cancelButton.BackgroundColor = buttonBackgroundColor;

			cancelButton.Layer.BorderColor = UIColor.FromWhiteAlpha (0.0f, 0.1f).CGColor;
			cancelButton.Layer.BorderWidth = 0.5f;
			this.VisualEffectView.ContentView.Add (cancelButton);


			// 
			var submitButton = new UIButton (new CGRect (buttonWidth, this.ActualBox.Frame.Size.Height - buttonHeight, buttonWidth, buttonHeight));

			submitButton.SetTitle (!(String.IsNullOrWhiteSpace (this.SubmitButtonText)) ? this.SubmitButtonText : @"OK", UIControlState.Normal);
			submitButton.TouchUpInside += (object sender, EventArgs e) => {
				SubmitButtonTapped ();
			};

			submitButton.TitleLabel.Font = UIFont.SystemFontOfSize (16.0f);

			submitButton.SetTitleColor (titleLabelTextColor, UIControlState.Normal);
			submitButton.SetTitleColor (UIColor.Gray, UIControlState.Highlighted);


			submitButton.BackgroundColor = buttonBackgroundColor;
			submitButton.Layer.BorderColor = UIColor.FromWhiteAlpha (0.0f, 0.1f).CGColor;
			submitButton.Layer.BorderWidth = 0.5f;


			this.VisualEffectView.ContentView.Add (submitButton);

			// 
			this.VisualEffectView.Frame = new CGRect (0, 0, this.ActualBox.Frame.Size.Width, this.ActualBox.Frame.Size.Height + 45);    
			this.ActualBox.Add (this.VisualEffectView);

			this.ActualBox.Center = this.Center;
		}

		/// <summary>
		/// Determines whether this instance cancel button tapped.
		/// </summary>
		/// <returns><c>true</c> if this instance cancel button tapped; otherwise, <c>false</c>.</returns>
		private void CancelButtonTapped ()
		{
			HandleCancel ();

			this.Hide ();
		}

		/// <summary>
		/// Submits the button tapped.
		/// </summary>
		private void SubmitButtonTapped ()
		{
			if (CanSubmit ()) 
			{
				HandleSubmit ();
				Hide ();
			}

		}
			
		/// <summary>
		/// Devices the orientation did change.
		/// </summary>
		/// <param name="notification">Notification.</param>
		private void DeviceOrientationDidChange (NSNotification notification)
		{
			ResetFrame (true);

		}

		/// <summary>
		/// Keyboards the did show.
		/// </summary>
		/// <param name="notification">Notification.</param>
		private void KeyboardDidShow (NSNotification notification)
		{
			ResetFrame (true);

			UIView.Animate (0.2f, () => {
				CGRect frame = this.ActualBox.Frame;
				frame.Y -= YCorrection ();
				this.ActualBox.Frame = frame;

			});

		}

		/// <summary>
		/// Keyboards the did hide.
		/// </summary>
		/// <param name="notification">Notification.</param>
		private void KeyboardDidHide (NSNotification notification)
		{
			ResetFrame (true);

		}

		/// <summary>
		/// Ys the correction.
		/// </summary>
		/// <returns>The correction.</returns>
		private nfloat YCorrection ()
		{
			var yCorrection = 115.0f;


			if (UIKit.UIDeviceOrientationExtensions.IsLandscape (UIDevice.CurrentDevice.Orientation)) {
				if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone) {
					yCorrection = 80.0f;
				} else if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad) {
					yCorrection = 100.0f;
				}

				if (this.BoxType == DHDialogType.LoginAndPasswordInput) {
					yCorrection += 45.0f;
				}
			} else {
				if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad) {
					yCorrection = 0.0f;
				}
			}
			return yCorrection;

		}

		/// <summary>
		/// Resets the frame.
		/// </summary>
		/// <param name="animated">If set to <c>true</c> animated.</param>
		private void ResetFrame (Boolean animated)
		{

			var window = UIApplication.SharedApplication.Windows [0];

			this.Frame = window.Frame;

			// 
			if (animated) {
				UIView.Animate (0.3f, () => {
					this.Center = new CGPoint (window.Center.X, window.Center.Y);
					this.ActualBox.Center = this.Center;

				});

			} else {
				this.Center = new CGPoint (window.Center.X, window.Center.Y);
				this.ActualBox.Center = this.Center;
			}

		}

		/// <summary>
		/// Displaies the N decimal.
		/// </summary>
		/// <returns>The N decimal.</returns>
		/// <param name="dbValue">Db value.</param>
		/// <param name="nDecimal">N decimal.</param>
		private string DisplayNDecimal (double dbValue, int nDecimal)
		{
			string decimalPoints = "0";
			if (nDecimal > 0) {
				decimalPoints += ".";
				for (int i = 0; i < nDecimal; i++)
					decimalPoints += "0";
			}
			return dbValue.ToString (decimalPoints);
		}

		protected abstract bool CanSubmit ();

		protected abstract void HandleCancel ();

		protected abstract void HandleSubmit ();
		#endregion

		#region Static Methods

		/// <summary>
		/// Boxs the type of the of.
		/// </summary>
		/// <returns>The of type.</returns>
		/// <param name="boxType">Box type.</param>
		public static DHDialogView BoxOfType (DHDialogType boxType)
		{
			return null;
		}

		#endregion
	}
}
