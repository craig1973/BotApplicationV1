using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace BotApplication.Dialogs
{
	[Serializable]
	public class RootDialog : IDialog<object>
	{
		private const string ForLeaseOption = "For Lease";
		private const string FindAHouseOption = "Find a house";
		public Task StartAsync(IDialogContext context)
		{
			context.Wait(MessageReceivedAsync);
			return Task.CompletedTask;
		}

		private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
		{
			var activity = await result as Activity;
			if (activity.Text.ToLower() == ForLeaseOption.ToLower())
			{
				context.Call(new ForLeaseDialog(), this.AfterForLeaseDialog);
			}
			else if (activity.Text.ToLower() == FindAHouseOption.ToLower())
			{
				context.Call(new FindAHouseDialog(), this.AfterFindAHouseDialog);
			}
			else
			{
				PromptDialog.Choice(
				context,
				this.AfterChoiceSelected,
				new[] { ForLeaseOption, FindAHouseOption },
				"Hello, Welcome, what can i do for u?",
				"I am sorry but I didn't understand that. I need you to select one of the options below",
				attempts: 2);
			}
		}
		private async Task AfterChoiceSelected(IDialogContext context, IAwaitable<string> result)
		{
			try
			{
				var selection = await result;

				switch (selection)
				{
					case ForLeaseOption:
						context.Call(new ForLeaseDialog(), this.AfterForLeaseDialog);
						break;

					case FindAHouseOption:
						context.Call(new FindAHouseDialog(), this.AfterFindAHouseDialog);
						break;
				}
			}
			catch (TooManyAttemptsException)
			{
				await this.StartAsync(context);
			}
		}
		private async Task AfterForLeaseDialog(IDialogContext context, IAwaitable<bool> result)
		{
			var success = await result;

			if (success)
			{
				await context.PostAsync("Your house has been published success");
			}
			else
			{
				await context.PostAsync("You have exited the process.I will be restarted");
			}
			await this.StartAsync(context);
		}
		private async Task AfterFindAHouseDialog(IDialogContext context, IAwaitable<bool> result)
		{
			var success = await result;

			if (success)
			{
				await context.PostAsync("Find a house finished");
			}
			else
			{
				await context.PostAsync("You have exited the process.I will be restarted");
			}
			await this.StartAsync(context);
		}
	}
}