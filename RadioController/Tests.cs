using System;

namespace RadioController
{
	public class Tests
	{
		public Tests ()
		{
		}

		public void testMusicFading ()
		{
			//Debug: Play around with the Mplayer Object
			Mplayer mp3 = new Mplayer ("/home/apexys/Music/Ghosts.flac");
			Mplayer sound2 = new Mplayer ("/home/apexys/Music/System.mp3");
			mp3.Volume = 10;
			Mixer mixer = new Mixer ();

			mixer.FadeTo (mp3, 100, 5);

			//sound2.Play();
			//mixer.FadeTo(sound2,100,5);
			Console.WriteLine ("MP3: " + mp3.Length.ToString ());
			Console.WriteLine ("Sound2: " + sound2.Length.ToString ());


			while (true) {
				Console.WriteLine ("MP3: " + mp3.Position.ToString ());
				Console.WriteLine ("Sound2: " + sound2.Position.ToString ());
				Console.WriteLine ("Metadata : " + sound2.Metadata.ToString ());

				Console.ReadLine ();
			}
		}
	}
}

