using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Data.SqlClient;
using System.Net;
using System.IO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using BotApplication.Models;

namespace BotApplication.Dialogs
{
	[Serializable]
	public class ForLeaseDialog : IDialog<bool>
	{
		private string longt = "";
		private string lat = "";
		private string name = "";
		private string phone = "";
		private string price = "";
		private string[] picUrl = new string[9];
		private int picCount = 0;
		public async Task StartAsync(IDialogContext context)
		{
			await context.PostAsync($"Welcome to system for lease.Lets complete several steps blow.");
			await context.PostAsync($"Step1: Please tell me your name");
			context.Wait(this.GetNameAsync);
		}
		
		private async Task GetNameAsync(IDialogContext context, IAwaitable<object> result)
		{
			var activity = await result as Activity;
			/*********name validation*********/
			name = activity.Text;
			await context.PostAsync($"Step2: Please tell me your phone.");
			context.Wait(this.GetPhoneAsync);
		}

		private async Task GetPhoneAsync(IDialogContext context, IAwaitable<object> result)
		{
			var activity = await result as Activity;
			/******phone validation********/
			if (IsNumber(activity.Text))
			{
				phone = activity.Text;
				await context.PostAsync($"Step3: Please send me your location of your house");
				context.Wait(this.GetLocationAsync);
			}
			else
			{
				if (activity.Text.ToLower() == "exit")
				{
					context.Done(false);
				}
				else
				{
					await context.PostAsync($"Must be only numbers. Please try agian or type Exit");
				}
			}
			
		}

		private async Task GetLocationAsync(IDialogContext context, IAwaitable<object> result)
		{
			var activity = await result as Activity;
			if (context.Activity.ChannelData.message != null)
			{
				if (context.Activity.ChannelData.message.attachments[0].type == "location")
				{
					lat = context.Activity.ChannelData.message.attachments[0].payload.coordinates.lat;
					longt = context.Activity.ChannelData.message.attachments[0].payload.coordinates.@long;
					await context.PostAsync($"Step4: Please tell me the price you want to set");
					context.Wait(this.GetPriceAsync);
				}
			}
			else
			{
				if (activity.Text.ToLower() == "exit")
				{
					context.Done(false);
				}
				else
				{
					await context.PostAsync($"I cant detect any location data. Please try agian or type Exit");
				}
			}
		}

		private async Task GetPriceAsync(IDialogContext context, IAwaitable<object> result)
		{
			var activity = await result as Activity;
			/*********name validation*********/
			price = activity.Text;
			await context.PostAsync($"Step5: Please send me a picture of your house");
			context.Wait(this.GetPictureAsync);
		}
		private async Task GetPictureAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
		{
			var message = await argument;
			if (message.Attachments != null && message.Attachments.Count >= 1)
			{
				var attachmentData = message.Attachments[0].Content;
				var attachmentType = message.Attachments[0].ContentType;
				var attachmentUrl = message.Attachments[0].ContentUrl;
				WebClient client = new WebClient();
				byte[] bytes = client.DownloadData(new Uri(attachmentUrl));

				MemoryStream ms = new MemoryStream(bytes);
				string uniqueBlobName = $"{Guid.NewGuid().ToString().Replace("-", "")}.jpg";
				attachmentUrl = Storage.SaveFile(ms, "images", uniqueBlobName, "image/jpg");
				picUrl[picCount] = attachmentUrl;
				picCount++;
				if (picCount < 9)
				{
					PromptDialog.Choice(
						context,
						this.AfterChoiceSelected,
						new[] { "Continue Uploading", "Done" },
						"You can upload " + (9 - picCount) + " pictures remaining",
						"I am sorry but I didn't understand that. I need you to select one of the options below",
						attempts: 2);
				}
				else
				{
					int leaserId = Database.SaveToLeaser(name, phone, price, longt, lat);
					if (leaserId > 0)
					{
						for (int i = 0; i < picCount; i++)
						{
							Database.SaveToImages(leaserId, picUrl[i]);
						}
						context.Done(true);
					}
				}
			}
			else {
				await context.PostAsync($"I cant detect any pictures. Please try again.");
			}
		}
		private async Task AfterChoiceSelected(IDialogContext context, IAwaitable<string> result)
		{
			try
			{
				var selection = await result;
				switch (selection)
				{
					case "Continue Uploading":
						await context.PostAsync($"Please send a picture of your house");
						context.Wait(this.GetPictureAsync);
						break;
					case "Done":
						int leaserId = Database.SaveToLeaser(name, phone, price, longt, lat);
						if (leaserId > 0)
						{
							for (int i = 0; i < picCount; i++)
							{
								Database.SaveToImages(leaserId, picUrl[i]);
							}
							context.Done(true);
						}
						else
						{
							context.Done(false);
						}
						break;
				}
			}
			catch (TooManyAttemptsException)
			{
				await this.StartAsync(context);
			}
		}

		public bool IsNumber(string str_number)
		{
			return System.Text.RegularExpressions.Regex.IsMatch(str_number, @"^[0-9]*$");
		}

		
	}
}