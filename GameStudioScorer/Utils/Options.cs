using System;
using CommandLine;
using CommandLine.Text;

namespace GameStudioScorer
{
	//Command line arguments parsed using the CommandLineParser library.
	public class Options
	{
		[Option("regression", Required = false, HelpText = "The action to take in terms of regression learning. The default is 'm' for model.", Default='m')]
		public char RegressionType { get; set; }

		[Option("file", Required = false, HelpText = "The name of the file to save to or learn from. Must be a .txt file.", Default="data.txt")]
		public string fileName { get; set; }
	}
}
