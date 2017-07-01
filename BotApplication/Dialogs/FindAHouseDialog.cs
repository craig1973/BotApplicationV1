using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace BotApplication.Dialogs
{
	public class FindAHouseDialog : IDialog<bool>
	{
		public async Task StartAsync(IDialogContext context)
		{
			await context.PostAsync($"Entered Find A House process");
			context.Done(true);
		}
	}
}