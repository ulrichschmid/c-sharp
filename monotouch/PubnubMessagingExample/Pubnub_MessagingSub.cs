using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using System.Threading;
using System.Drawing;
using MonoTouch.ObjCRuntime;
using PubNubMessaging.Core;
using MonoTouch.SlideoutNavigation;

namespace PubnubMessagingExample
{
    public class LeftNavController : DialogViewController
    {
        Pubnub_MessagingSub pubnub_MessagingSub;
        SlideoutNavigationController menu;

        public LeftNavController (SlideoutNavigationController menu, Pubnub_MessagingSub pubnub_MessagingSub) 
            : base(UITableViewStyle.Plain,new RootElement(""))
        {
            this.pubnub_MessagingSub = pubnub_MessagingSub;
            this.menu = menu;
        }

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();

            Root.Add (new Section () {
                new StyledStringElement("Subscribe", () => {menu.Hide(true); pubnub_MessagingSub.Subscribe();}),
                new StyledStringElement("Publish", () => {menu.Hide(true); pubnub_MessagingSub.Publish();}),
                new StyledStringElement("Presence", () => {menu.Hide(true); pubnub_MessagingSub.Presence();}),
                new StyledStringElement("Detailed History", () => {menu.Hide(true); pubnub_MessagingSub.DetailedHistory();}),
                new StyledStringElement("Here Now", () => {menu.Hide(true); pubnub_MessagingSub.HereNow();}),
                new StyledStringElement("Time", () => {menu.Hide(true); pubnub_MessagingSub.GetTime();}),
                new StyledStringElement("Unsubscribe", () => {menu.Hide(true); pubnub_MessagingSub.Unsub();}),
                new StyledStringElement("Presence-Unubscribe", () => {menu.Hide(true); pubnub_MessagingSub.UnsubPresence();}),
                new StyledStringElement("Subscribe Grant", () => {menu.Hide(true); pubnub_MessagingSub.SubscribeGrant();}),
                new StyledStringElement("Subscribe Audit", () => {menu.Hide(true); pubnub_MessagingSub.SubscribeAudit();}),
                new StyledStringElement("Subscribe Revoke", () => {menu.Hide(true); pubnub_MessagingSub.SubscribeRevoke();}),
                new StyledStringElement("Presence Grant", () => {menu.Hide(true); pubnub_MessagingSub.PresenceGrant();}),
                new StyledStringElement("Presence Audit", () => {menu.Hide(true); pubnub_MessagingSub.PresenceAudit();}),
                new StyledStringElement("Presence Revoke", () => {menu.Hide(true); pubnub_MessagingSub.PresenceRevoke();}),
                new StyledStringElement("Auth Key", () => {menu.Hide(true); pubnub_MessagingSub.AuthKey();}),
                new StyledStringElement("Presence Heartbeat", () => {menu.Hide(true); pubnub_MessagingSub.SetPresenceHeartbeat();}),
                new StyledStringElement("Presence Interval", () => {menu.Hide(true); pubnub_MessagingSub.SetPresenceInterval();}),
                new StyledStringElement("Add/Mod Local User State", () => {menu.Hide(true); pubnub_MessagingSub.SetLocalUserState();}),
                new StyledStringElement("Del Local User State", () => {menu.Hide(true); pubnub_MessagingSub.DelLocalUserState();}),
                new StyledStringElement("View Local User State", () => {menu.Hide(true); pubnub_MessagingSub.ViewLocalUserState();}),
                new StyledStringElement("Set User State", () => {menu.Hide(true); pubnub_MessagingSub.SetUserState();}),
                new StyledStringElement("Get User State", () => {menu.Hide(true); pubnub_MessagingSub.GetUserState();}),
                new StyledStringElement("Where Now", () => {menu.Hide(true); pubnub_MessagingSub.WhereNow();}),
                new StyledStringElement("Global Here Now", () => {menu.Hide(true); pubnub_MessagingSub.GlobalHereNow();}),
                new StyledStringElement("Change UUID", () => {menu.Hide(true); pubnub_MessagingSub.ChangeUuid();}),
            });
        }
    }
    public partial class Pubnub_MessagingSub : DialogViewController
    {
        Pubnub pubnub;

        public enum CommonDialogStates
        {
            HereNow,
            Auth,
            PresenceHeartbeat,
            PresenceHeartbeatInterval,
            AddLocalUserState,
            ViewLocalUserState,
            DeleteLocalUserState,
            SetUserState,
            GetUserState,
            WhereNow,
            GlobalHereNow,
            ChangeUuid,
            SubscribeGrant,
            PresenceGrant
        }

        public SlideoutNavigationController Menu { get; private set; }

        string Channel {
            get;set;
        }

        string Cipher {
            get;set;    
        }

        bool Ssl {
            get;set;
        }

        DialogViewController dvc;
        RootElement root;
        Section secOutput;
        UIFont font12 = UIFont.SystemFontOfSize (12);
        UIFont font13 = UIFont.SystemFontOfSize (13);
        public bool showErrorMessageSegments = true;
        UITextField tfChannels;
        UITextField newChannels;

        public Pubnub_MessagingSub (string channelName, string cipher, bool enableSSL, Pubnub pubnub) 
            : base (UITableViewStyle.Grouped, null)
        {
            Channel = channelName;
            Ssl = enableSSL;
            Cipher = cipher;
            this.pubnub = pubnub;

            string strSsl = "";
            if (Ssl) {
                strSsl = "SSL,";
            }
            
            string strCip = "";
            if (!String.IsNullOrWhiteSpace (Cipher)) {
                strCip = "Cipher";
            }
            
            string head = String.Format ("{0} {1}", strSsl, strCip);

            Section secAction = new Section ();
            
            bool bIphone = true;
            
            int viewHeight = 70;
            
            secAction.HeaderView = CreateHeaderView(viewHeight);
            
            secOutput = new Section("Output");
            
            root = new RootElement (head) {
                secAction,
                secOutput
            };

            Root = root;
            dvc = new DialogViewController (root, true);
            var tap = new UITapGestureRecognizer ();
            tap.AddTarget (() =>{
                dvc.View.EndEditing (true);
            });
            dvc.View.AddGestureRecognizer (tap);

            tap.CancelsTouchesInView = false;
            dvc.NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Cancel, delegate {
                pubnub.EndPendingRequests ();
                AppDelegate.navigation.PopToRootViewController(true);
            });
            Menu = new SlideoutNavigationController ();
            Menu.TopView = dvc;

            Menu.MenuViewLeft = new LeftNavController (Menu, this);

            AppDelegate.navigation.PushViewController (Menu, true);
            Menu.ShowMenuLeft ();
            newChannels.Text = Channel;
        }

        UIView CreateHeaderView (int iViewHeight)
        {
            UIView uiView = new UIView (new RectangleF (0, 0, this.View.Bounds.Width, iViewHeight));
            uiView.MultipleTouchEnabled = true;

            /*UILabel lblChannel = new UILabel(new RectangleF (10, 2, 80, 25));
            lblChannel.Font = font13;
            lblChannel.Text = "Channel(s):";
            uiView.Add (lblChannel);

            tfChannels = new UITextField (new RectangleF (90, 2, 200, 25));
            tfChannels.Enabled = false;
            tfChannels.BackgroundColor = UIColor.FromRGB(239, 239, 244);
            tfChannels.Font = font12;
            uiView.Add (tfChannels);*/

            UILabel lblInfo = new UILabel(new RectangleF (10, 2, 300, 25));
            lblInfo.Font = font12;
            lblInfo.Text = "Enter new channel(s) and/or use the menu for actions";
            uiView.Add (lblInfo);

            UILabel lblNewChannel = new UILabel(new RectangleF (10, 32, 100, 25));
            lblNewChannel.Font = font13;
            lblNewChannel.Text = "New Channel(s):";
            uiView.Add (lblNewChannel);

            newChannels = new UITextField (new RectangleF (120, 32, 185, 25));
            newChannels.AutocorrectionType = UITextAutocorrectionType.No;
            newChannels.BackgroundColor = UIColor.White;
            newChannels.Font = font12;
            uiView.Add (newChannels);

            return uiView;
        }

        public void Subscribe()
        {
            Display("Running Subscribe");
            pubnub.Subscribe<string>(Channel, DisplayReturnMessage, DisplayConnectStatusMessage, DisplayErrorMessage);
        }

        public void Publish()
        {
            UIAlertView alert = new UIAlertView ();
            alert.AlertViewStyle = UIAlertViewStyle.PlainTextInput;
            alert.Title = "Publish";
            alert.Message = "Enter message to publish";
            alert.AddButton ("Publish");
            alert.AddButton ("Cancel");
            alert.Clicked += delegate(object sender, UIButtonEventArgs e) {
                if(e.ButtonIndex == 0)
                {
                    if(alert.GetTextField(0) != null){
                        string input = alert.GetTextField(0).Text;
                        Display("Running Publish");
                        string[] channels = Channel.Split(',');
                        foreach (string channel in channels)
                        {
                            pubnub.Publish<string> (channel.Trim(), input, DisplayReturnMessage, DisplayErrorMessage);
                        }
                    }
                }           
            };
            alert.Show();
        }

        void PublishAlertDismissed (object sender, UIButtonEventArgs e)
        {
            InputAlertView iav = (InputAlertView)sender;
            if ((iav != null) && (!String.IsNullOrWhiteSpace (iav.EnteredText))) {
                Display("Running Publish");
                string[] channels = Channel.Split(',');
                foreach (string channel in channels)
                {
                    pubnub.Publish<string> (channel.Trim(), iav.EnteredText, DisplayReturnMessage, DisplayErrorMessage);
                }
            }
        }
        
        public void Presence()
        {
            Display("Running Presence");
            pubnub.Presence<string>(Channel, DisplayReturnMessage, null, DisplayErrorMessage);
        }
        
        public void DetailedHistory ()
        {
            Display ("Running Detailed History");
            string[] channels = Channel.Split (',');
            foreach (string channel in channels) {
                pubnub.DetailedHistory<string> (channel.Trim(), 100, DisplayReturnMessage, DisplayErrorMessage);
            }
        }
        
        public void HereNow ()
        {
            ShowAlertType2 (CommonDialogStates.HereNow);
        }
        
        public void Unsub()
        {
            Display("Running unsubscribe");
            pubnub.Unsubscribe<string>(Channel, DisplayReturnMessage, 
                                       DisplayConnectStatusMessage, DisplayConnectStatusMessage, 
                                       DisplayErrorMessage);
        }

        public void UnsubPresence()
        {
            Display("Running presence-unsubscribe");
            pubnub.PresenceUnsubscribe<string>(Channel, DisplayReturnMessage, 
                                               DisplayConnectStatusMessage, DisplayConnectStatusMessage, 
                                               DisplayErrorMessage);
        }

        public void GetTime()
        {
            Display("Running Time");
            pubnub.Time<string>(DisplayReturnMessage, DisplayErrorMessage);
        }

        public void SubscribeGrant()
        {
            ShowAlertType2 (CommonDialogStates.SubscribeGrant);
        }

        public void SubscribeAudit()
        {
            Display("Running Subscribe Audit");
            pubnub.AuditAccess<string>(Channel,DisplayReturnMessage, DisplayErrorMessage);
        }

        public void SubscribeRevoke()
        {
            Display("Running Subscribe Revoke");
            pubnub.GrantAccess<string>(Channel, false,false, DisplayReturnMessage, DisplayErrorMessage);
        }

        public void PresenceGrant()
        {
            ShowAlertType2 (CommonDialogStates.PresenceGrant);
        }

        public void PresenceAudit()
        {
            Display("Running Presence Audit");
            pubnub.AuditPresenceAccess<string>(Channel, DisplayReturnMessage, DisplayErrorMessage);
        }

        public void PresenceRevoke()
        {
            Display("Running Presence Revoke");
            pubnub.GrantPresenceAccess<string>(Channel, false, false, DisplayReturnMessage, DisplayErrorMessage);
        }

        public void SetPresenceHeartbeat(){
            ShowAlertType1 (CommonDialogStates.PresenceHeartbeat);
        }

        public void SetPresenceInterval ()
        {
            ShowAlertType1 (CommonDialogStates.PresenceHeartbeatInterval);
        }

        public void ChangeUuid ()
        {
            ShowAlertType1 (CommonDialogStates.ChangeUuid);
        }

        public void GlobalHereNow ()
        {
            ShowAlertType2 (CommonDialogStates.GlobalHereNow);
        }

        public void WhereNow ()
        {
            ShowAlertType1 (CommonDialogStates.WhereNow);
        }

        public void GetUserState ()
        {
            ShowAlertType3 (CommonDialogStates.GetUserState);
        }

        public void SetUserState ()
        {
            ShowAlertType3 (CommonDialogStates.SetUserState);
        }

        public void ViewLocalUserState ()
        {
            string[] channels = Channel.Split (',');
            foreach (string channelToCall in channels) {
                string currentUserStateView = pubnub.GetLocalUserState (channelToCall);
                if (!string.IsNullOrEmpty (currentUserStateView)) {
                    Display (string.Format("User state for channel {0}:{1}", channelToCall, currentUserStateView));
                } else {
                    Display (string.Format("No User State Exists for channel {0}", channelToCall));
                }
            }

        }

        public void DelLocalUserState ()
        {
            ShowAlertType3 (CommonDialogStates.DeleteLocalUserState);
        }

        public void SetLocalUserState ()
        {
            ShowAlertType3 (CommonDialogStates.AddLocalUserState);
        }

        public void AuthKey()
        {
            ShowAlertType1 (CommonDialogStates.Auth);
        }

        void ShowAlertType3 (CommonDialogStates cds)
        {
            bool showEntryText3 = true;

            string strHead = "", elementText1 = "", elementText2 = "", elementText3 = "";
            string elementSubText1 = "", elementSubText2 = "", elementSubText3 = "", buttonTitle = "";
            if (cds == CommonDialogStates.AddLocalUserState) {
                strHead = "Add Local User State";
                elementText1 = "Channel";
                elementText2 = "Key";
                elementText3 = "Value";
                elementSubText1 = "Enter Channel";
                elementSubText2 = "Enter Key";
                elementSubText3 = "Enter Value";
                buttonTitle = "Add";
            } else if (cds == CommonDialogStates.DeleteLocalUserState) {
                strHead = "Delete Local User State";
                elementText1 = "Channel";
                elementText2 = "Key";
                elementSubText1 = "Enter Channel";
                elementSubText2 = "Key to delete";
                buttonTitle = "Delete";
                showEntryText3 = false;
            } else if (cds == CommonDialogStates.SetUserState){
                strHead = "Add User State";
                elementText1 = "Channel";
                elementText2 = "State";
                elementText3 = "UUID";
                elementSubText1 = "Enter Channel";
                elementSubText2 = "enter json format or skip to use local";
                elementSubText3 = "Enter UUID";
                buttonTitle = "Add";
            } else if (cds == CommonDialogStates.GetUserState){
                strHead = "Get User State";
                elementText1 = "Channel";
                elementText2 = "UUID";
                elementSubText1 = "Enter Channel";
                elementSubText2 = "Enter UUID";
                buttonTitle = "Get";
                showEntryText3 = false;
            }

            EntryElement entryText3 = null;

            if (showEntryText3) {
                entryText3 = new EntryElement (elementText3, elementSubText3, "");
                entryText3.AutocapitalizationType = UITextAutocapitalizationType.None;
                entryText3.AutocorrectionType = UITextAutocorrectionType.No;
            }

            EntryElement entryText1 = new EntryElement (elementText1, elementSubText1, "");
            entryText1.AutocapitalizationType = UITextAutocapitalizationType.None;
            entryText1.AutocorrectionType = UITextAutocorrectionType.No;

            EntryElement entryText2 = new EntryElement (elementText2, elementSubText2, "");
            entryText2.AutocapitalizationType = UITextAutocapitalizationType.None;
            entryText2.AutocorrectionType = UITextAutocorrectionType.No;

            var newroot = new RootElement (strHead, 0, 0){
                new Section (){
                    entryText1,
                    entryText2,
                    entryText3
                },
                new Section (""){
                    new StyledStringElement (buttonTitle, () => {
                        if (cds == CommonDialogStates.AddLocalUserState) {
                            string currentState = pubnub.SetLocalUserState (entryText1.Value, entryText2.Value, entryText3.Value);
                            Display (string.Format("Current user state: {0}", currentState));
                        } else if (cds == CommonDialogStates.DeleteLocalUserState) {
                            string currentUserStateAfterDelete = pubnub.SetLocalUserState (entryText1.Value, entryText2.Value, null);
                            Display (string.Format("Current user state: {0}", currentUserStateAfterDelete));
                        } else if (cds == CommonDialogStates.SetUserState){
                            string jsonUserState = "";
                            if (string.IsNullOrEmpty (entryText2.Value)) {
                                jsonUserState = pubnub.GetLocalUserState (entryText1.Value);
                            } else {
                                jsonUserState = entryText2.Value;
                            }
                            pubnub.SetUserState<string> (entryText1.Value, entryText3.Value, jsonUserState, DisplayReturnMessage, DisplayErrorMessage);
                        } else if (cds == CommonDialogStates.GetUserState){
                            Display ("Running get user state");
                            pubnub.GetUserState<string> (entryText1.Value, entryText2.Value, DisplayReturnMessage, DisplayErrorMessage);
                        }

                        AppDelegate.navigation.PopViewControllerAnimated(true);
                    })
                    {
                        BackgroundColor = UIColor.Blue,
                        TextColor = UIColor.White,
                        Alignment = UITextAlignment.Center
                    },
                },
            };
            dvc = new DialogViewController (newroot, true);
            AppDelegate.navigation.PushViewController (dvc, true);
        }

        void ShowAlertType1(CommonDialogStates cds){
            UIAlertView alert = new UIAlertView ();
            alert.AlertViewStyle = UIAlertViewStyle.PlainTextInput;
            bool isHeartbeatInterval = false;

            if (cds == CommonDialogStates.Auth) {
                alert.Title = "Auth Key";
                alert.Message = "Enter Auth Key";
            } else if (cds == CommonDialogStates.PresenceHeartbeat){
                alert.GetTextField (0).KeyboardType = UIKeyboardType.NumberPad;
                alert.Title = "Presence Heartbeat";
                alert.Message = "Enter Presence Heartbeat";
            } else if (cds == CommonDialogStates.PresenceHeartbeatInterval){
                isHeartbeatInterval = true;
                alert.GetTextField (0).KeyboardType = UIKeyboardType.NumberPad;
                alert.Title = "Presence Heartbeat Interval";
                alert.Message = "Enter Presence Heartbeat Interval";
            } else if (cds == CommonDialogStates.ChangeUuid){
                alert.Title = "Change UUID";
                alert.Message = "Enter UUID";
            } else if (cds == CommonDialogStates.WhereNow){
                alert.Title = "Where Now";
                alert.Message = "Enter UUID (optional)";
            }

            alert.AddButton ("Set");
            alert.AddButton ("Cancel");
            alert.Clicked += delegate(object sender, UIButtonEventArgs e) {
                if(e.ButtonIndex == 0)
                {
                    if(alert.GetTextField(0) != null){
                        string input = alert.GetTextField(0).Text;
                        if (cds == CommonDialogStates.Auth) {
                            pubnub.AuthenticationKey = input;
                            Display("Auth Key Set");
                        } else if ((cds == CommonDialogStates.PresenceHeartbeat) || (cds == CommonDialogStates.PresenceHeartbeatInterval)){
                            int iVal;
                            Int32.TryParse(input, out iVal);
                            if (iVal != 0) 
                            {
                                if(isHeartbeatInterval){
                                    pubnub.PresenceHeartbeatInterval = iVal;
                                    Display (string.Format("Presence Heartbeat Interval set to {0}", pubnub.PresenceHeartbeatInterval));
                                } else {
                                    pubnub.PresenceHeartbeat = iVal;
                                    Display (string.Format("Presence Heartbeat set to {0}", pubnub.PresenceHeartbeat));
                                }
                            } else {
                                Display (string.Format("Value not numeric"));
                            }
                        } else if (cds == CommonDialogStates.ChangeUuid){
                            pubnub.ChangeUUID(input);
                            Display (string.Format("UUID set to {0}", pubnub.SessionUUID));
                        } else if (cds == CommonDialogStates.WhereNow){
                            Display ("Running where now");
                            pubnub.WhereNow<string> (input, DisplayReturnMessage, DisplayErrorMessage);
                        }
                    }
                }           
            };
            alert.Show();
        }

        void ShowAlertType2 (CommonDialogStates cds)
        {
            bool isPresenceGrant = false;
            bool showEntryText = true;
            bool boolval1 = false;
            UIKeyboardType keyboardType = UIKeyboardType.Default;

            string strHead = "", elementText1 = "", elementText2 = "", 
                elementText3 = "", elementSubText = "", buttonTitle = "";
            if (cds == CommonDialogStates.PresenceGrant) {
                strHead = "Presence Grant";
                elementText1 = "Read";
                elementText2 = "Write";
                elementText3 = "TTL";
                elementSubText = "Enter TTL (default 1440)";
                buttonTitle = "Grant";
                keyboardType = UIKeyboardType.NumberPad;
                isPresenceGrant = true;
            } else if (cds == CommonDialogStates.SubscribeGrant) {
                elementText1 = "Read";
                elementText2 = "Write";
                elementText3 = "TTL";
                elementSubText = "Enter TTL (default 1440)";
                strHead = "Subscribe Grant";
                buttonTitle = "Grant";
                keyboardType = UIKeyboardType.NumberPad;
            } else if (cds == CommonDialogStates.HereNow) {
                elementText1 = "Show UUID";
                elementText2 = "Include User State";
                elementText3 = "Channel";
                elementSubText = "Enter channel name";
                strHead = "Here now";
                buttonTitle = "Here Now";
                boolval1 = true;
            } else if (cds == CommonDialogStates.GlobalHereNow){
                elementText1 = "Show UUID";
                elementText2 = "Include User State";
                strHead = "Global Here Now";
                buttonTitle = "Global Here Now";
                showEntryText = false;
                boolval1 = true;
            }

            BooleanElement be1 = new BooleanElement (elementText1, boolval1);
            BooleanElement be2 = new BooleanElement (elementText2, false);

            EntryElement entryText = null;

            if (showEntryText) {
                entryText = new EntryElement (elementText3, elementSubText, "");
                entryText.KeyboardType = keyboardType;
                entryText.AutocapitalizationType = UITextAutocapitalizationType.None;
                entryText.AutocorrectionType = UITextAutocorrectionType.No;
            }

            var newroot = new RootElement (strHead, 0, 0){
                new Section (){
                    be1,
                    be2,
                    entryText
                },
                new Section (""){
                    new StyledStringElement (buttonTitle, () => {
                        if ((cds == CommonDialogStates.PresenceGrant) || (cds == CommonDialogStates.SubscribeGrant)) {
                            int iTtl;
                            Int32.TryParse(entryText.Value, out iTtl);
                            if (iTtl == 0) 
                            {
                                iTtl = 1440;
                                entryText.Value = "1440";
                            }
                            if (isPresenceGrant) {
                                Display("Running Presence Grant");
                                pubnub.GrantPresenceAccess<string>(Channel, be1.Value, be2.Value, iTtl, DisplayReturnMessage, DisplayErrorMessage);
                            }else{
                                Display("Running Subscribe Grant");
                                pubnub.GrantAccess<string>(Channel, be1.Value, be2.Value, iTtl, DisplayReturnMessage, DisplayErrorMessage);
                            }
                        } else if (cds == CommonDialogStates.HereNow) {
                            Display ("Running Here Now");
                            string[] channels = entryText.Value.Split(',');//Channel.Split (',');
                            foreach (string channel in channels) {
                                pubnub.HereNow<string> (channel.Trim(), be1.Value, be2.Value, DisplayReturnMessage, DisplayErrorMessage);
                            }
                        } else if (cds == CommonDialogStates.GlobalHereNow){
                            pubnub.GlobalHereNow<string> (be1.Value, be2.Value, DisplayReturnMessage, DisplayErrorMessage);
                        }

                        AppDelegate.navigation.PopViewControllerAnimated(true);
                    })
                    {
                        BackgroundColor = UIColor.Blue,
                        TextColor = UIColor.White,
                        Alignment = UITextAlignment.Center
                    },
                },
            };
            dvc = new DialogViewController (newroot, true);
            AppDelegate.navigation.PushViewController (dvc, true);
        }

        /// <summary>
        /// Callback method to provide the connect status of Subscribe call
        /// </summary>
        /// <param name="result"></param>
        void DisplayErrorMessage(string result)
        {
            Display(String.Format("Error Callback - {0}", result));
        }


        /// <summary>
        /// Callback method to provide the connect status of Subscribe call
        /// </summary>
        /// <param name="result"></param>
        void DisplayConnectStatusMessage(string result)
        {
            Display(String.Format("Connect Callback - {0}", result));
        }
        
        public void Display (string strText)
        {
            StyledMultilineElement sme = new StyledMultilineElement (strText)
            {
                Font = font12
            };
            ThreadPool.QueueUserWorkItem (delegate {
                
                System.Threading.Thread.Sleep(2000);
                
                AppDelegate.navigation.BeginInvokeOnMainThread(delegate {
                    if(secOutput.Count > 20)
                    {
                        secOutput.RemoveRange(0, 10);
                    }
                    if (secOutput.Count > 0) {
                        secOutput.Insert (secOutput.Count, sme);                    }
                    else
                    {
                        secOutput.Add (sme);
                    }
                    this.TableView.ReloadData();
                    var lastIndexPath = this.root.Last()[this.root.Last().Count-1].IndexPath;
                    this.TableView.ScrollToRow(lastIndexPath, UITableViewScrollPosition.Middle, true);    
                });
            });
        }

        void DisplayReturnMessage(string result)
        {
            Display (result);
        }

        /// <summary>
        /// Callback method for error messages
        /// </summary>
        /// <param name="result"></param>
        void DisplayErrorMessage(PubnubClientError result)
        {
            Console.WriteLine();
            Console.WriteLine(result.Description);
            Console.WriteLine();

            switch (result.StatusCode)
            {
            case 103:
                //Warning: Verify origin host name and internet connectivity
                break;
            case 104:
                //Critical: Verify your cipher key
                break;
            case 106:
                //Warning: Check network/internet connection
                break;
            case 108:
                //Warning: Check network/internet connection
                break;
            case 109:
                //Warning: No network/internet connection. Please check network/internet connection
                break;
            case 110:
                //Informational: Network/internet connection is back. Active subscriber/presence channels will be restored.
                break;
            case 111:
                //Informational: Duplicate channel subscription is not allowed. Internally Pubnub API removes the duplicates before processing.
                break;
            case 112:
                //Informational: Channel Already Subscribed/Presence Subscribed. Duplicate channel subscription not allowed
                break;
            case 113:
                //Informational: Channel Already Presence-Subscribed. Duplicate channel presence-subscription not allowed
                break;
            case 114:
                //Warning: Please verify your cipher key
                break;
            case 115:
                //Warning: Protocol Error. Please contact PubNub with error details.
                break;
            case 116:
                //Warning: ServerProtocolViolation. Please contact PubNub with error details.
                break;
            case 117:
                //Informational: Input contains invalid channel name
                break;
            case 118:
                //Informational: Channel not subscribed yet
                break;
            case 119:
                //Informational: Channel not subscribed for presence yet
                break;
            case 120:
                //Informational: Incomplete unsubscribe. Try again for unsubscribe.
                break;
            case 121:
                //Informational: Incomplete presence-unsubscribe. Try again for presence-unsubscribe.
                break;
            case 122:
                //Informational: Network/Internet connection not available. C# client retrying again to verify connection. No action is needed from your side.
                break;
            case 123:
                //Informational: During non-availability of network/internet, max retries for connection were attempted. So unsubscribed the channel.
                break;
            case 124:
                //Informational: During non-availability of network/internet, max retries for connection were attempted. So presence-unsubscribed the channel.
                break;
            case 125:
                //Informational: Publish operation timeout occured.
                break;
            case 126:
                //Informational: HereNow operation timeout occured
                break;
            case 127:
                //Informational: Detailed History operation timeout occured
                break;
            case 128:
                //Informational: Time operation timeout occured
                break;
            case 4000:
                //Warning: Message too large. Your message was not sent. Try to send this again smaller sized
                break;
            case 4001:
                //Warning: Bad Request. Please check the entered inputs or web request URL
                break;
            case 4002:
                //Warning: Invalid Key. Please verify the publish key
                break;
            case 4010:
                //Critical: Please provide correct subscribe key. This corresponds to a 401 on the server due to a bad sub key
                break;
            case 4020:
                // PAM is not enabled. Please contact PubNub support
                break;
            case 4030:
                //Warning: Not authorized. Check the permimissions on the channel. Also verify authentication key, to check access.
                break;
            case 4031:
                //Warning: Incorrect public key or secret key.
                break;
            case 4140:
                //Warning: Length of the URL is too long. Reduce the length by reducing subscription/presence channels or grant/revoke/audit channels/auth key list
                break;
            case 5000:
                //Critical: Internal Server Error. Unexpected error occured at PubNub Server. Please try again. If same problem persists, please contact PubNub support
                break;
            case 5020:
                //Critical: Bad Gateway. Unexpected error occured at PubNub Server. Please try again. If same problem persists, please contact PubNub support
                break;
            case 5040:
                //Critical: Gateway Timeout. No response from server due to PubNub server timeout. Please try again. If same problem persists, please contact PubNub support
                break;
            case 0:
                //Undocumented error. Please contact PubNub support with full error object details for further investigation
                break;
            default:
                break;
            }
            if (showErrorMessageSegments)
            {
                DisplayErrorMessageSegments(result);
                Console.WriteLine();
            }
        }

        void DisplayErrorMessageSegments(PubnubClientError pubnubError)
        {
            Console.WriteLine("<STATUS CODE>: {0}", pubnubError.StatusCode); // Unique ID of Error
            Display(String.Format("Error: {0}", pubnubError.Message));
            Console.WriteLine("<MESSAGE>: {0}", pubnubError.Message); // Message received from server/clent or from .NET exception

            Console.WriteLine("<SEVERITY>: {0}", pubnubError.Severity); // Info can be ignored, Warning and Error should be handled

            if (pubnubError.DetailedDotNetException != null)
            {
                Console.WriteLine(pubnubError.IsDotNetException); // Boolean flag to check .NET exception
                Console.WriteLine("<DETAILED DOT.NET EXCEPTION>: {0}", pubnubError.DetailedDotNetException.ToString()); // Full Details of .NET exception
            }
            Console.WriteLine("<MESSAGE SOURCE>: {0}", pubnubError.MessageSource); // Did this originate from Server or Client-side logic
            if (pubnubError.PubnubWebRequest != null)
            {
                //Captured Web Request details
                Console.WriteLine("<HTTP WEB REQUEST>: {0}", pubnubError.PubnubWebRequest.RequestUri.ToString()); 
                Console.WriteLine("<HTTP WEB REQUEST - HEADERS>: {0}", pubnubError.PubnubWebRequest.Headers.ToString()); 
            }
            if (pubnubError.PubnubWebResponse != null)
            {
                //Captured Web Response details
                Console.WriteLine("<HTTP WEB RESPONSE - HEADERS>: {0}", pubnubError.PubnubWebResponse.Headers.ToString());
            }
            Console.WriteLine("<DESCRIPTION>: {0}", pubnubError.Description); // Useful for logging and troubleshooting and support
            Console.WriteLine("<CHANNEL>: {0}", pubnubError.Channel); //Channel name(s) at the time of error
            Console.WriteLine("<DATETIME>: {0}", pubnubError.ErrorDateTimeGMT); //GMT time of error

        }
    }
}
