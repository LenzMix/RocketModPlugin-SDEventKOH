using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDEventKoH
{
    class Commandkohaddloc  : IRocketCommand
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
				return "kohaddloc";
			}
		}

		public string Help
		{
			get
			{
				return "Add locations for KOH";
			}
		}

		public string Syntax
		{
			get
			{
				return "Usage: /kohaddloc <Name> <radius>";
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
			if (command.Length < 2) return;
			Plugin.Instance.Configuration.Instance.Locations.Add(new Loc
			{
				Name = command[0],
				pos = player.Position,
				radius = Convert.ToSingle(command[1])
			});
			Plugin.Instance.Configuration.Save();
			UnturnedChat.Say(player, "Successful!");
		}
	}
}