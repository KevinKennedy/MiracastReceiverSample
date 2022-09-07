using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Media.Miracast;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Media.Playback;
using System.Runtime.CompilerServices;

namespace MiracastSample
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private MiracastReceiver miracastReceiver;
        private MiracastReceiverSession miracastSession;
        private MediaPlayer mediaPlayer;

        private async void PageLoaded(object sender, RoutedEventArgs e)
        {
            await this.InitializeMiracastAsync();
        }

        private async Task InitializeMiracastAsync()
        {
            this.miracastReceiver = new MiracastReceiver();
            this.miracastReceiver.StatusChanged += (receiver, o) => { this.Log($"StatusChanged: {receiver.GetStatus().ListeningStatus}"); };

            MiracastReceiverSettings settings = this.miracastReceiver.GetDefaultSettings();
            settings.FriendlyName += " Miracast Sample";
            settings.AuthorizationMethod = MiracastReceiverAuthorizationMethod.None;
            settings.RequireAuthorizationFromKnownTransmitters = false;
            MiracastReceiverApplySettingsResult applyResult = await this.miracastReceiver.DisconnectAllAndApplySettingsAsync(settings);
            this.Log($"DisconnectAllAndApplySettingsAsync={applyResult.Status}");


            this.miracastSession = await this.miracastReceiver.CreateSessionAsync(null /* CoreApplication.MainView */);
            this.Log($"CreateSession={this.miracastSession}");
            miracastSession.AllowConnectionTakeover = true;
            this.miracastSession.ConnectionCreated += (session, args) => { this.Log($"ConnectionCreated {args.Connection.Transmitter.Name}"); };
            this.miracastSession.Disconnected += (session, args) => { this.Log($"Disconnected {args.Connection.Transmitter.Name}"); };
            this.miracastSession.MediaSourceCreated += MiracastSessionMediaSourceCreated;

            MiracastReceiverSessionStartResult startResult = await this.miracastSession.StartAsync();
            this.Log($"Session.Start={startResult.Status}");
        }

        private void MiracastSessionMediaSourceCreated(MiracastReceiverSession sender, MiracastReceiverMediaSourceCreatedEventArgs args)
        {
            this.Log($"args={args.MediaSource.Uri}");
            this.ResetMediaPlayer();

            this.mediaPlayer = new MediaPlayer();
            this.mediaPlayer.Source = args.MediaSource;
            this.mediaPlayerElement.SetMediaPlayer(this.mediaPlayer);
            this.mediaPlayer.Play();
        }

        private void ResetMediaPlayer()
        {
            if(this.mediaPlayer != null)
            {
                this.mediaPlayerElement.SetMediaPlayer(null);
                this.mediaPlayer.Dispose();
                this.mediaPlayer = null;
            }
        }

        private void PageUnloaded(object sender, RoutedEventArgs e)
        {
            this.ResetMediaPlayer();

            if (this.miracastSession != null)
            {
                this.miracastSession.MediaSourceCreated -= MiracastSessionMediaSourceCreated;
                this.miracastSession.Dispose();
                this.miracastSession = null;
            }
        }

        private void Log(string msg, [CallerMemberName] string caller = null)
        {
            Debug.WriteLine($"MainPage::{caller}  {msg}");
        }
    }
}
