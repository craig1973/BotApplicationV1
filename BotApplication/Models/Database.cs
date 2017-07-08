using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace BotApplication.Models
{
	public class Database
	{
		public static int SaveToLeaser(string name, string phone, string price, string longt, string lat)
		{
			using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DB"].ConnectionString))
			{ 
				var sql = "INSERT INTO Leaser([name], [phone], [price], [longt], [lat]) OUTPUT Inserted.id values(@name, @phone, @price, @longt, @lat)";
				var cmd = new SqlCommand(sql, conn);
				cmd.Parameters.Add(new SqlParameter("@name", SqlDbType.NVarChar)).Value = name;
				cmd.Parameters.Add(new SqlParameter("@phone", SqlDbType.NVarChar)).Value = phone;
				cmd.Parameters.Add(new SqlParameter("@price", SqlDbType.NVarChar)).Value = price;
				cmd.Parameters.Add(new SqlParameter("@longt", SqlDbType.NVarChar)).Value = longt;
				cmd.Parameters.Add(new SqlParameter("@lat", SqlDbType.NVarChar)).Value = lat;
				conn.Open();
				var result = 0;
				using (var reader = cmd.ExecuteReader())
				{
					if (reader.HasRows)
					{
						reader.Read();
						result = reader.GetInt32(0);
					}
				}
				return result;
			}
		}

		public static bool SaveToImages(int leaserId, string imageUrl)
		{
			using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DB"].ConnectionString))
			{
				var sql = "INSERT INTO Images([leaserId], [imageUrl]) values(@leaserId, @imageUrl)";
				var cmd = new SqlCommand(sql, conn);
				cmd.Parameters.Add(new SqlParameter("@leaserId", SqlDbType.Int)).Value = leaserId;
				cmd.Parameters.Add(new SqlParameter("@imageUrl", SqlDbType.NVarChar)).Value = imageUrl;
				conn.Open();
				cmd.ExecuteNonQuery();
				return true;
			}
		}
	}
}