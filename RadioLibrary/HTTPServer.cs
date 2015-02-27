using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using RadioLogger;

namespace RadioLibrary
{
	public class HTTPServer
	{
		bool state;

		public bool State {
			get {
				return state;
			}
		}

		TcpListener tcpl;
		Thread t;
		public HTTPServer (int port)
		{
			state = false;
			tcpl = new TcpListener(IPAddress.Any, port);
			t = new Thread(new ThreadStart(listen));
			t.Start();

		}

		~HTTPServer(){
			t.Abort();
		}

		void listen(){
			tcpl.Start();
			while(true){
				TcpClient tclient = tcpl.AcceptTcpClient();
				StreamWriter srw = new StreamWriter(tclient.GetStream());
				srw.AutoFlush = true;
				StreamReader strw = new StreamReader (tclient.GetStream ());
				srw.WriteLine("HTTP/1.1 200 OK");
				srw.WriteLine("Connection: close");
				srw.WriteLine("Content-Type: text/plain");
				srw.WriteLine();
				state = !state;
				Logger.LogInformation ("Toggled live to " + state.ToString ());
				srw.WriteLine(state.ToString());
				srw.WriteLine ();
				srw.WriteLine ();
				strw.ReadLine ();
				srw.Flush ();
				tclient.Close ();
			}
		}


	}
}

