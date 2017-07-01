using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Data.SqlClient;

namespace BotApplication.Dialogs
{
	[Serializable]
	public class ForLeaseDialog : IDialog<bool>
	{
		private double lat = 0.0;
		private double longt = 0.0;
		private string name = "";
		private string phone = "";
		private string[] picUrl = new string[9];
		private int picCount = 0;
		public async Task StartAsync(IDialogContext context)
		{
			await context.PostAsync($"Welcome to system for lease.Lets complete 4 steps blow.");
			await context.PostAsync($"Step1: Please send me your name");
			context.Wait(this.GetNameAsync);
		}
		
		private async Task GetNameAsync(IDialogContext context, IAwaitable<object> result)
		{
			var activity = await result as Activity;
			/*********name validation*********/
			name = activity.Text;
			await context.PostAsync($"Step2: Please send me your phone.");
			context.Wait(this.GetPhoneAsync);
		}

		private async Task GetPhoneAsync(IDialogContext context, IAwaitable<object> result)
		{
			var activity = await result as Activity;
			/******phone validation********/
			if (IsNumber(activity.Text))
			{
				phone = activity.Text;
				await context.PostAsync($"Step3: Please send location of your house");
				context.Wait(this.GetLocationAsync);
				/*******test for local environment******/
				//await context.PostAsync($"Step3: Please send picture of your house");
				//context.Wait(this.GetPictureAsync);
			}
			else
			{
				if (activity.Text.ToLower() == "exit")
				{
					context.Done(false);
				}
				else
				{
					await context.PostAsync($"Must be only numbers. Please try agian or type in Exit");
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
					await context.PostAsync($"Step4: Please send a picture of your house");
					context.Wait(this.GetPictureAsync);
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
					await context.PostAsync($"I cant detect any location data. Please try agian or type in Exit");
				}
			}
		}
		private async Task GetPictureAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
		{
			var message = await argument;
			if (message.Attachments != null && message.Attachments.Count >= 1)
			{
				var attachmentData = message.Attachments[0].Content;
				var attachmentType = message.Attachments[0].ContentType;
				var attachmentUrl = message.Attachments[0].ContentUrl;
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
					if (SaveToDatabase(name, phone, lat, longt, picUrl))
					{
						context.Done(true);
					}
					else
					{
						context.Done(false);
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
						/*****for sql validat*******/
						//for (int i = 0; i < picUrl.Length; i++)
						//{
						//	if (picUrl[i] != "")
						//	{
						//		picUrl[i] = System.Web.HttpUtility.UrlEncode(picUrl[i]);
						//	}
						//	else
						//	{
						//		picUrl[i] = "";
						//	}
						//}
						//string test = "insert into for_lease values('" + lat + "','" + longt + "','" + name + "','" + phone + "','" + picUrl[0] + "','" + picUrl[1] + "','" + picUrl[2] + "','" + picUrl[3] + "','" + picUrl[4] + "','" + picUrl[5] + "','" + picUrl[6] + "','" + picUrl[7] + "','" + picUrl[8] + "')";
						//await context.PostAsync($"{test}");
						if (SaveToDatabase(name, phone, lat, longt, picUrl))
						{
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

		public bool SaveToDatabase(string name, string phone, double lat, double longt, string[] picUrl) {
			for (int i = 0; i < picUrl.Length; i++)
			{
				if (picUrl[i] != "")
				{
					picUrl[i] = System.Web.HttpUtility.UrlEncode(picUrl[i]);
				}
				else
				{
					picUrl[i] = "";
				}

			}
			string connectString = "";
			SqlConnection conn = new SqlConnection(connectString);
			SqlCommand cmd = conn.CreateCommand();
			cmd.CommandText = "insert into for_lease values('" + lat + "','" + longt + "','" + name + "','" + phone + "','" + picUrl[0] + "','" + picUrl[1] + "','" + picUrl[2] + "','" + picUrl[3] + "','" + picUrl[4] + "','" + picUrl[5] + "','" + picUrl[6] + "','" + picUrl[7] + "','" + picUrl[8] + "')";
			conn.Open();
			cmd.ExecuteNonQuery();
			conn.Close();
			return true;
		}
	}
}