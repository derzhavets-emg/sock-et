using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AppKit;
using Foundation;

namespace socket
{
    public partial class ViewController : NSViewController
    {

        private bool _serverStarted = false;

        public ViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Do any additional setup after loading the view.
            SetIpAddress();
            InitEventListeners();
        }

        private void InitEventListeners()
        {
            AsynchronousSocketListener.ConnectionStatusChanged += HandleConnectionStatus;
            AsynchronousSocketListener.OnMessageReceived += HandleNewMessage;
        }

        private void HandleNewMessage(object sender, string e)
        {
            InvokeOnMainThread(() => ReceivedMesages.StringValue = e + " \n");
        }

        private void HandleConnectionStatus(object sender, bool connected)
        {
            InvokeOnMainThread(() => {
                if (connected)
                    BtnStartServerText.Title = "Stop server";
                else
                    BtnStartServerText.Title = "Start server";
            });
        }

        private void SetIpAddress()
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IpInput.StringValue = ipAddress.ToString();
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

        partial void BtnStartServer(NSObject sender)
        {
            _serverStarted = !_serverStarted;

            if (_serverStarted)
            {
                if (int.TryParse(PortInput.StringValue, out int port))
                    Task.Run(() => AsynchronousSocketListener.StartListening(port));
            } else
            {
                Task.Run(() => AsynchronousSocketListener.StopListening());
            }
        }

        partial void BtnSendMessage(NSObject sender)
        {
            AsynchronousSocketListener.SendMessage(MessageInput.StringValue);
        }

        partial void BtnLoadTemplateStatus(NSObject sender)
        {
            MessageInput.StringValue = "{\"status\":1,\"temperature\":\"36.6\"}";
        }


    }

    // State object for reading client data asynchronously  
    public class StateObject
    {
        // Size of receive buffer.  
        public const int BufferSize = 1024;

        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];

        // Received data string.
        public StringBuilder sb = new StringBuilder();

        // Client socket.
        public Socket workSocket = null;
    }

    public class AsynchronousSocketListener
    {
        private static Socket listener;
        private static List<Socket> handlers = new List<Socket>();

        public static event EventHandler<bool> ConnectionStatusChanged;
        public static event EventHandler<string> OnMessageReceived;

        // Thread signal.  
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        private static bool _connected = false;

        public AsynchronousSocketListener()
        {
        }

        public static void StartListening(int port)
        {
            // Establish the local endpoint for the socket.  
            // The DNS name of the computer  
            // running the listener is "host.contoso.com".  
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

            // Create a TCP/IP socket.  
            Socket listener = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.  
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                _connected = true;
                ConnectionStatusChanged?.Invoke(null, true);


                while (_connected)
                {

                    // Set the event to nonsignaled state.  
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.  
                    Console.WriteLine("Waiting for a connection...");

                    listener.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        listener);

                    // Wait until a connection is made before continuing.  
                    allDone.WaitOne();


                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            
            try
            {
                // Get the socket that handles the client request.  
                listener = (Socket)ar.AsyncState;
                Socket handler = listener.EndAccept(ar);

                handlers.Add(handler);

                // Create the state object.  
                StateObject state = new StateObject();
                state.workSocket = handler;
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);

            } catch (Exception e)
            {
                Console.WriteLine("Socket closed, AcceptCallback interrupted");
            }

            // Signal the main thread to continue.  
            allDone.Set();


        }

        public static void ReadCallback(IAsyncResult ar)
        {
            try
            {
                string content = string.Empty;

                // Retrieve the state object and the handler socket  
                // from the asynchronous state object.  
                StateObject state = (StateObject)ar.AsyncState;
                Socket handler = state.workSocket;

                // Read data from the client socket.
                int bytesRead = handler.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // There  might be more data, so store the data received so far.  
                    state.sb.Append(Encoding.ASCII.GetString(
                        state.buffer, 0, bytesRead));

                    // Check for end-of-file tag. If it is not there, read
                    // more data.  
                    content = state.sb.ToString();

                    // send content to UI
                    OnMessageReceived?.Invoke(null, content);

                    //if (content.IndexOf("<EOF>") > -1)
                    //{
                    //    // All the data has been read from the
                    //    // client. Display it on the console.  
                    //    Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
                    //        content.Length, content);
                    //    // Echo the data back to the client.  
                    //SendMessage(content, handler);
                    //}
                    //else
                    //{
                    //    // Not all data received. Get more.  
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
                    //}
                }
            } catch (ObjectDisposedException e)
            {
                Console.WriteLine("Socket closed, ReadCallback interrupted");
            }
        }

        public static void SendMessage(string data)
        {
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            foreach (Socket handler in handlers)
            {
                handler.BeginSend(byteData, 0, byteData.Length, 0,
                    new AsyncCallback(SendCallback), handler);
            }

        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                //handler.Shutdown(SocketShutdown.Both);
                //handler.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static void StopListening()
        {
            _connected = false;
            foreach (Socket handler in handlers)
            {
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            handlers.Clear();
            listener.Close();
            ConnectionStatusChanged?.Invoke(null, false);
        }
    }

}
