using System;
using System.IO;

namespace GameStudioScorer.Utils
{
	public class Logger
	{
		public enum LogLevel
		{
			DEBUG,
			INFO,
			WARNING,
			ERROR,
			CRITICAL
		}

		const string LOG_FILE = "log.txt";

		//Adds dashed line to file to split program runs.
		static bool firstLog = true;

		/// <summary>
		/// Logs a message to the Console and to the Log file.
		/// </summary>
		/// <param name="message">The message to log.</param>
		/// <param name="level">The severity of the message.</param>
		/// <param name="toConsole">Should the message be written to the console, or only the Log file?</param>
		public static void Log(string message, LogLevel level, bool toConsole)
		{
			if (toConsole)
			{
				//Change ConsoleColor for Log Level
				switch (level)
				{
					case LogLevel.DEBUG:
						Console.ForegroundColor = ConsoleColor.Blue;
						break;
					case LogLevel.INFO:
						Console.ForegroundColor = ConsoleColor.Gray;
						break;
					case LogLevel.WARNING:
						Console.ForegroundColor = ConsoleColor.Yellow;
						break;
					case LogLevel.ERROR:
						Console.ForegroundColor = ConsoleColor.Red;
						break;
					case LogLevel.CRITICAL:
						Console.ForegroundColor = ConsoleColor.DarkRed;
						break;
				}
			}

			string line = "[" + DateTime.Now + "]:\t" + level.ToString() + " -\t" + message + "\n";

			if (toConsole)
			{
				//Write to Console
				Console.WriteLine(line);
				Console.ForegroundColor = ConsoleColor.Gray;
			}

			//Add a dashed line to split up program runs in the log file.
			if (firstLog)
			{
				firstLog = false;
				line = "\n\n--------------------------------------------------------------------\n\n" + line;
			}

			//Write to Log File
			string[] lines = File.ReadAllLines(LOG_FILE);
			FileStream fs = File.OpenWrite(LOG_FILE);
			StreamWriter writer = new StreamWriter(fs);

			//Keep lines from previous Logs.
			foreach (string l in lines)
				writer.WriteLine(l);

			//Write new line.
			writer.WriteLine(line);

			//Cleanup
			writer.Close();
			writer.Dispose();

			fs.Close();
			fs.Dispose();
		}

		/// <summary>
		/// Logs a message to the Console and to the Log file.
		/// </summary>
		/// <param name="message">The message to log.</param>
		/// <param name="toConsole">Should the message be written to the Console, or only the Log file?</param>
		public static void Log(string message, bool toConsole)
		{
			Log(message, LogLevel.INFO, toConsole);
		}

		/// <summary>
		/// Logs a message to the Console and to the Log file.
		/// </summary>
		/// <param name="message">The message to log.</param>
		public static void Log(string message)
		{
			Log(message, LogLevel.INFO, true);
		}
	}
}
