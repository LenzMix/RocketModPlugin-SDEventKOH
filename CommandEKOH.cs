using Rocket.API;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDEventKoH
{
    class CommandEKOH : IRocketCommand
	{
		public AllowedCaller AllowedCaller
		{
			get
			{
				return AllowedCaller.Player;
			}
		}

		public string Name
		{
			get
			{
				return "ekoh";
			}
		}

		public string Help
		{
			get
			{
				return "Start event KoH";
			}
		}

		public string Syntax
		{
			get
			{
				return "Usage: /ekoh";
			}
		}

		public List<string> Aliases
		{
			get
			{
				return new List<string>
				{
					"eventkoh"
				};
			}
		}

		public List<string> Permissions
		{
			get
			{
				return new List<string>
				{
					"ekoh"
				};
			}
		}

		public void Execute(IRocketPlayer caller, string[] command)
		{
			UnturnedPlayer player = (UnturnedPlayer)caller;
			Plugin.Instance.StartGame();
		}
	}
}