﻿using CommandLine;

namespace GameStudioScorer
{
	//Command line arguments parsed using the CommandLineParser library.
	public class Options
	{
		[Option("studio", Required = false, Default="null", HelpText="A specific studio to calculate for. Should only be used with 'p' and 'm' regression modes.")]
		public string studio { get; set; }

		[Option("regression", Required = false, HelpText = "The action to take in terms of regression learning. The default is 'm' for model.", Default='m')]
		public char RegressionType { get; set; }

		[Option("file", Required = false, HelpText = "The name of the file to save to or learn from. Must be a .txt file.", Default="data")]
		public string fileName { get; set; }

		[Option("model", Required = false, HelpText = "The name of the model to use in predicting values.", Default = "Model-0")]
		public string modelName { get; set; }

		[Option("set", Required = false, HelpText = "The name of the set to load Studio names from. The first line is studios that crunch, the second is studios that don't.", Default = "set-0")]
		public string setName { get; set; }

		[Option("verbose", Required = false, HelpText = "Whether the scorer should print extra information.", Default = false)]
		public bool verbose { get; set; }

		[Option("debug", Required = false, HelpText = "Is the scorer in debug mode?", Default=false)]
		public bool debug { get; set; }

		[Option("force", Required = false, HelpText = "Whether the scorer should forcibly refresh values (in other words, ignore the local cache)", Default = false)]
		public bool force { get; set; }
	}
}
