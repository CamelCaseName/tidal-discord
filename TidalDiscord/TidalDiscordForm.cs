using DarkUI.Forms;
using DiscordRPC;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Nodes;
using System.Windows.Forms.VisualStyles;

namespace TidalDiscord
{
    public enum PlayingState
    {
        Idle,
        TidalClosed,
        Playing,
    }

    public partial class TidalDiscordForm : DarkForm
    {
        #region Dark Theme Window Caption
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private static bool IsWindows10OrGreater(int build = -1) => Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= build;

        private static bool UseImmersiveDarkMode(IntPtr handle, bool enabled)
        {
            if (IsWindows10OrGreater(17763))
            {
                int attribute = DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1;
                if (IsWindows10OrGreater(18985))
                {
                    attribute = DWMWA_USE_IMMERSIVE_DARK_MODE;
                }

                int useImmersiveDarkMode = enabled ? 1 : 0;
                return DwmSetWindowAttribute(handle, attribute, ref useImmersiveDarkMode, sizeof(int)) == 0;
            }

            return false;
        }
        #endregion

        public DiscordRpcClient? client = null;
        public string? discordUser = null;
        public string? previousMainWndTitle = null;
        public RichPresence? currentPresence = null;
        public PlayingState state = PlayingState.TidalClosed;
        public string Album = "Loading...";
        public string Artist = "";
        private string lastSong = "nothing";
        public TimeSpan PlayDuration = TimeSpan.Zero;
        public DateTime PlayStart = DateTime.UtcNow;
        public DateTime PlayStop = DateTime.UtcNow;
        public bool resuming = false;
        public string SongURL = "https://tidal.com/browse/track/149644465?u";

        //add your token here
        //create an app on developer.tidal.com
        //get your cliend id and secret and run echo -n "<CLIENT_ID>:<CLIENT_SECRET>" | base64 to get this string 
        private const string TIDAL_TOKEN = ;

        public const string MY_GUID = "8BF610C1-30F3-4E63-B5DA-F50A7EC1B66A";
        public const int UDP_PORT = 11000;
        public static IPAddress MULTICAST_IP = IPAddress.Parse("127.0.0.1");
        public static IPEndPoint ENDPOINT = new(MULTICAST_IP, UDP_PORT);

        public Mutex? mutex;
        public Socket? socket;
        public static UdpClient? listener;
        public HttpClient webClient = new();
        public string token = "";
        public DateTime tokenExpiry = DateTime.MinValue;

        public static TidalDiscordForm? Instance { get; private set; }
        public string LastSong { get => lastSong; set => lastSong = value; }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            IPEndPoint e = ENDPOINT;
            if (listener is null)
            {
                return;
            }

            byte[] receivedBytes = listener.EndReceive(ar, ref e);
            string receivedString = Encoding.ASCII.GetString(receivedBytes);

            Instance?.BroadcastMessageReceived(receivedString);

            _ = listener.BeginReceive(new AsyncCallback(ReceiveCallback), null);
        }

        public TidalDiscordForm()
        {
            Instance = this;

            // Create a socket to broadcast message (this is because hidden windows have no handle)
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            listener = new UdpClient();
            listener.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            listener.Client.Bind(ENDPOINT);
            _ = listener.BeginReceive(new AsyncCallback(ReceiveCallback), null);

            mutex = new Mutex(true, $"MUTEX_{MY_GUID}", out bool onlyInstance);
            if (!onlyInstance)
            {
                Process myself = Process.GetCurrentProcess();
                byte[] message = Encoding.UTF8.GetBytes($"{MY_GUID}-{myself.Handle}");
                _ = socket.SendTo(message, ENDPOINT);

                Debug.WriteLine("Already running");
                Environment.Exit(1);
            }

            // Now can initialize components etc
            InitializeComponent();

            timer1.Interval = 1000;
            timer1.Tick += new EventHandler(UpdatePresence);
            timer1.Enabled = true;

            try
            {
                client = new DiscordRpcClient("1318633795628437594");
                currentPresence = new RichPresence()
                {
                    Timestamps = new Timestamps()
                };

                //Subscribe to events
                client.OnReady += (sender, e) =>
                {
                    Debug.WriteLine($"Received Ready from user '{client.CurrentUser.Username}'");
                    discordUser = client.CurrentUser.Username;
                    previousMainWndTitle = "";
                };

                client.OnConnectionFailed += (sender, e) =>
                {
                    Debug.WriteLine($"Received Connection Failed from Discord {e}");
                    discordUser = null;
                };

                //Connect to the RPC
                _ = client.Initialize();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Could not create Discord RPC Client: {ex.Message}");
                _ = DarkMessageBox.ShowError(ex.Message, "Could not create Discord RPC Client");
                Environment.Exit(1);
            }

            //get tidal auth token for the next 24h

            GetTidalToken();

        }

        private void GetTidalToken()
        {
            const string credString = "grant_type=client_credentials";
            var content = new StringContent(credString);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            content.Headers.ContentLength = credString.Length;
            webClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("BASIC", TIDAL_TOKEN);
            webClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            webClient.DefaultRequestHeaders.AcceptEncoding.Add(StringWithQualityHeaderValue.Parse("gzip"));
            webClient.DefaultRequestHeaders.AcceptEncoding.Add(StringWithQualityHeaderValue.Parse("deflate"));
            webClient.DefaultRequestHeaders.AcceptEncoding.Add(StringWithQualityHeaderValue.Parse("br"));
            webClient.DefaultRequestHeaders.Connection.Add("keep-alive");
            webClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("DiscordRpcTi", "1.0.0"));
            webClient.PostAsync("https://auth.tidal.com/v1/oauth2/token", content).ContinueWith((Task<HttpResponseMessage> t) =>
            {
                if (t.Result.StatusCode == HttpStatusCode.OK)
                {
                    var response = JsonNode.Parse(t.Result.Content.ReadAsStream());
                    if (response is null)
                    {
                        return;
                    }

                    string? maybeToken = response["access_token"]?.GetValue<string>();
                    int? expiry = response["expires_in"]?.GetValue<int>();
                    if (string.IsNullOrEmpty(maybeToken) || expiry is null)
                    {
                        return;
                    }

                    token = maybeToken;
                    tokenExpiry = DateTime.UtcNow + TimeSpan.FromSeconds((double)expiry);
                    webClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                else
                {
                    Debug.WriteLine(t.Result);
                }
            });

        }

        private void UpdatePresence(object? Sender, EventArgs e)
        {
            Process[] localByName = Process.GetProcessesByName("TIDAL");
            string? currentMainWndTitle = localByName
                        .Where(p => p.MainWindowHandle.ToInt64() != 0)
                        .Select(p => p.MainWindowTitle)
                        .Distinct()
                        .FirstOrDefault();

            if (currentMainWndTitle == null)
            {
                SongLabel.Text = "nothing";
                state = PlayingState.TidalClosed;
                client?.ClearPresence();
            }
            else if (previousMainWndTitle != currentMainWndTitle && currentMainWndTitle != string.Empty)
            {
                if (currentMainWndTitle == "TIDAL")
                {
                    Debug.WriteLine("pause");
                    LastSong = SongLabel.Text;
                    PlayStop = DateTime.UtcNow;
                    PlayDuration = PlayStop - PlayStart;
                    SongLabel.Text = "nothing";
                    currentPresence = new RichPresence()
                    {
                        Assets = new Assets() { LargeImageKey = "tidal", LargeImageText = "Tidal", SmallImageKey = "pause", SmallImageText = "Playback is paused" },
                        Details = "nothing"
                    };
                    client?.SetPresence(currentPresence);
                }
                else
                {
                    SongLabel.Text = currentMainWndTitle;
                    if (currentMainWndTitle == LastSong)
                    {
                        Debug.WriteLine("resuming");
                        resuming = true;
                        PlayStart = DateTime.UtcNow - PlayDuration;
                        currentPresence = new RichPresence()
                        {
                            Timestamps = new Timestamps() { Start = PlayStart },
                            Assets = new Assets() { LargeImageKey = "tidal", LargeImageText = "Tidal", SmallImageKey = "play", SmallImageText = "Currently listening" },
                            Details = SongLabel.Text,
                            State = "Album: " + Album,
                            Buttons = new DiscordRPC.Button[] { new() { Label = "Listen", Url = SongURL } }
                        };
                        client?.SetPresence(currentPresence);
                    }
                    else
                    {
                        Debug.WriteLine("next song");
                        LastSong = SongLabel.Text;
                        PlayDuration = TimeSpan.Zero;
                        PlayStart = DateTime.UtcNow;
                        Album = "loading...";

                        if (DateTime.UtcNow > tokenExpiry)
                        {
                            GetTidalToken();
                        }

                        currentPresence = new RichPresence()
                        {
                            Timestamps = new Timestamps() { Start = PlayStart },
                            Assets = new Assets() { LargeImageKey = "tidal", LargeImageText = "Tidal", SmallImageKey = "play", SmallImageText = "Currently listening" },
                            Details = SongLabel.Text,
                            State = "Album: " + Album
                        };
                        client?.SetPresence(currentPresence);

                        StringBuilder sb = new(100);
                        string[] splits = SongLabel.Text.Split(" - ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        for (int i = 0; i < splits.Length; i++)
                        {
                            if (i == splits.Length - 1)
                            {
                                sb.Append(' ');
                                sb.Append(splits[i]);
                                Artist = splits[i];
                                break;
                            }

                            sb.Append(splits[i]);
                        }

                        string songWhenSearchStarted = SongLabel.Text;
                        webClient.GetAsync($"https://openapi.tidal.com/v2/searchresults/{sb}?countryCode={System.Globalization.RegionInfo.CurrentRegion.TwoLetterISORegionName}&include=tracks,albums").ContinueWith((Task<HttpResponseMessage> t) =>
                        {
                            if (t.Result.StatusCode != HttpStatusCode.OK)
                            {
                                return;
                            }
                            if (songWhenSearchStarted != SongLabel.Text)
                            {
                                return;
                            }

                            var response = JsonNode.Parse(t.Result.Content.ReadAsStream());
                            if (response is null)
                            {
                                return;
                            }


                            string? maybeID = response["data"]?["relationships"]?["tracks"]?["data"]?[0]?["id"]?.GetValue<string>();
                            if(maybeID is null)
                            {
                                return ;
                            }

                            SongURL = $"https://tidal.com/browse/track/{maybeID}?u";
                            Debug.WriteLine("got id " + maybeID);

                            webClient.GetAsync($"https://openapi.tidal.com/v2/tracks/{maybeID}/relationships/albums?countryCode={System.Globalization.RegionInfo.CurrentRegion.TwoLetterISORegionName}&include=tracks%2Calbums").ContinueWith((Task<HttpResponseMessage> t) =>
                            {
                                if (t.Result.StatusCode != HttpStatusCode.OK)
                                {
                                    return;
                                }

                                if (songWhenSearchStarted != SongLabel.Text)
                                {
                                    return;
                                }

                                var response = JsonNode.Parse(t.Result.Content.ReadAsStream());
                                if (response is null)
                                {
                                    return;
                                }


                                string? maybeAlbum = response["included"]?[0]?["attributes"]?["title"]?.GetValue<string>();
                                if (maybeAlbum is null)
                                {
                                    return;
                                }
                                Album = maybeAlbum;

                                Debug.WriteLine("got album " + maybeAlbum);

                                currentPresence = new RichPresence()
                                {
                                    Timestamps = new Timestamps() { Start = PlayStart },
                                    Assets = new Assets() { LargeImageKey = "tidal", LargeImageText = "Tidal", SmallImageKey = "play", SmallImageText = "Currently listening" },
                                    Details = SongLabel.Text,
                                    State = "Album: " + Album,
                                    Buttons = new DiscordRPC.Button[] { new() { Label = "Listen", Url = SongURL } }
                                };
                                client?.SetPresence(currentPresence);
                            });

                        });
                    }
                    SongLabel.Text = currentMainWndTitle;
                    state = PlayingState.Playing;
                }
            }

            previousMainWndTitle = currentMainWndTitle;
            lblDiscordStatus.Text = discordUser
                                    != null ? $"DISCORD user '{discordUser}'"
                                    : "No DISCORD user detected";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            notifyIcon1.Text = "TIDAL Rich Presence";
            notifyIcon1.Visible = true;

            notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
            notifyIcon1.BalloonTipText = "TIDAL Rich Presence is running in background.";
            notifyIcon1.BalloonTipTitle = "TIDAL Rich Presence";
            notifyIcon1.ShowBalloonTip(1500);

            WindowState = FormWindowState.Minimized;
            Visible = false;
            Hide();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            switch (WindowState)
            {
                case FormWindowState.Minimized:
                    Hide();
                    ShowInTaskbar = false;
                    break;
                case FormWindowState.Normal:
                    Show();
                    ShowInTaskbar = true;
                    _ = UseImmersiveDarkMode(Handle, true);
                    break;
            }
        }

        private void IconClick(object sender, EventArgs e)
        {
            switch (WindowState)
            {
                case FormWindowState.Minimized:
                    Visible = true;
                    ShowInTaskbar = true;
                    WindowState = FormWindowState.Normal;
                    break;
                case FormWindowState.Normal:
                    Visible = false;
                    ShowInTaskbar = false;
                    WindowState = FormWindowState.Minimized;
                    break;
            }
        }

        private void Form1_Closing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason != CloseReason.WindowsShutDown)
            {
                if (DarkMessageBox.ShowInformation("Do you really want to close the application?", "Close TIDAL Rich Presence on Discord", DarkDialogButton.YesNo) == DialogResult.No)
                {
                    e.Cancel = true;
                }
                else
                {
                    mutex?.ReleaseMutex();
                }
            }
        }

        public void BroadcastMessageReceived(string message)
        {
            Process myself = Process.GetCurrentProcess();
            if (message != null && message.StartsWith(MY_GUID) && message != $"{MY_GUID}-{myself.Handle}")
            {
                // Received a broadcast message and it's not from myself
                if (InvokeRequired)
                {
                    Invoke(delegate
                    { BroadcastMessageReceived(message); });
                }
                else
                {
                    Visible = true;
                    ShowInTaskbar = true;
                    WindowState = FormWindowState.Normal;
                }
            }
        }
    }
}
