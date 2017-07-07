using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace BotApplication.Dialogs
{
	[Serializable]
	public class FindAHouseDialog : IDialog<bool>
	{
		private string longt = "";
		private string lat = "";
		private const string sortByDistanceOption = "Distance";
		private const string sortByPriceOption = "Price";
		public async Task StartAsync(IDialogContext context)
		{
			await context.PostAsync($"Welcome.Sent me your location then we can provide u suitable houses with in 10 Miles");
			context.Wait(this.GetLocationAsync);
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
					PromptDialog.Choice(
						context,
						this.AfterSortByChoiceSelected,
						new[] { sortByDistanceOption, sortByPriceOption },
						"Sort the result by:",
						"I am sorry but I didn't understand that. I need you to select one of the options below",
					attempts: 2);
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

		private async Task AfterSortByChoiceSelected(IDialogContext context, IAwaitable<string> result)
		{
			try
			{
				var selection = await result;
				string sql = "DECLARE @g geography;SET @g = geography::STPointFromText('POINT(" + longt + " "+ lat + " )', 4326); SELECT TOP 10 geography::STPointFromText('POINT('+longt+' '+lat+')', 4326).STDistance(@g) as Distance, id, longt, lat, name, phone, price,pic1,pic2,pic3,pic4,pic5,pic6,pic7,pic8,pic9 FROM for_lease where geography::STPointFromText('POINT('+longt+' '+lat+')', 4326).STDistance(@g) < 10000 ";
				var reply = context.MakeMessage();
				switch (selection)
				{
					case sortByDistanceOption:
						sql = sql + " order by Distance ASC ";
						reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
						reply.Attachments = GetCardsAttachments(sql);
						await context.PostAsync(reply);
						context.Done(true);
						break;

					case sortByPriceOption:
						sql = sql + " order by price ASC ";
						reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
						reply.Attachments = GetCardsAttachments(sql);
						await context.PostAsync(reply);
						context.Done(true);
						break;
				}
			}
			catch (TooManyAttemptsException)
			{
				await this.StartAsync(context);
			}
		}

		private static IList<Attachment> GetCardsAttachments(string sql)
		{
			List<Attachment> attachments = new List<Attachment>();
			string connectString = "Server=tcp:msgbot.database.windows.net,1433;Initial Catalog=MSGBOT;Persist Security Info=False;User ID=craig;Password=MSGbot123456;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
			SqlConnection conn = new SqlConnection(connectString);
			SqlCommand cmd = new SqlCommand(sql, conn);
			conn.Open();
			var reader = cmd.ExecuteReader();
			if (reader.HasRows)
			{
				while (reader.Read())
				{
					attachments.Add(
						GetHeroCard(
							"Distance: " + Math.Round(reader.GetDouble(reader.GetOrdinal("Distance"))*0.001, 2) + " Miles",
							"Price: $" + reader.GetString(reader.GetOrdinal("price")),
							"Phone: " + reader.GetString(reader.GetOrdinal("phone")),
							new CardImage(url: System.Web.HttpUtility.UrlDecode(reader.GetString(reader.GetOrdinal("pic1")))),
							new CardAction(ActionTypes.OpenUrl, "Details", value: "http://detecttype.azurewebsites.net?id="+ reader.GetInt32(reader.GetOrdinal("id")))
						)
					);
				}
			}
			conn.Close();
			return attachments;
		}

		private static Attachment GetHeroCard(string title, string subtitle, string text, CardImage cardImage, CardAction cardAction)
		{
			var heroCard = new HeroCard
			{
				Title = title,
				Subtitle = subtitle,
				Text = text,
				Images = new List<CardImage>() { cardImage },
				Buttons = new List<CardAction>() { cardAction },
			};

			return heroCard.ToAttachment();
		}
	}
}