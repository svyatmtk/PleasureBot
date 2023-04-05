using System.Data.SQLite;
using OpenAI.GPT3.ObjectModels.RequestModels;
using Telegram.Bot.Types;

namespace PleasureBot
{
    internal class SqLite
    {
        private static string dataBaseName = "BotUsers";
        private static string backupDir = "/root/to/backup";
        private static string[] backupFiles = Directory.GetFiles(backupDir, "*.db");
        public static string[] filteredFiles =
            backupFiles.Where(file => Path.GetFileNameWithoutExtension(file).StartsWith(dataBaseName)).ToArray();
        public static string? lastFile =
            filteredFiles.MaxBy(file => new FileInfo(file).CreationTime);
        public static string ConnectionString
        {
            get
            {
                if (OperatingSystem.IsWindows())
                    return @"Data Source = " + Environment.CurrentDirectory + @"\BotUsers.db; Version = 3;";
                return $@"Data Source  = {lastFile};";
            }
        }

        public static void SaveMessage(Message message, string prompt)
        {
            using var connection = new SQLiteConnection(ConnectionString);
            connection.Open();
            var command = new SQLiteCommand();
            command.Connection = connection;
            command.CommandText = "insert into Users(user_id, first_name, last_name, text, created_at) values " +
                                  "(@userId, @firstName, @lastName, @text, @createdAt)";

            command.Parameters.AddWithValue("@userId", message.Chat.Id);
            command.Parameters.AddWithValue("@firstName", message.Chat.FirstName);
            command.Parameters.AddWithValue("@lastName", message.Chat.LastName);
            command.Parameters.AddWithValue("@text", message.Text);
            command.Parameters.AddWithValue("@createdAt", DateTime.Now);
            command.ExecuteNonQuery();
        }

        public static IList<ChatMessage> LoadMessagesForPrompt(Message message)
        {
            using var connection = new SQLiteConnection(ConnectionString);
            connection.Open();
            var command = new SQLiteCommand();
            command.Connection = connection;
            command.CommandText = "select text from Users where user_id = @user_id";
            command.Parameters.AddWithValue("@user_id", message.Chat.Id);
            var reader = command.ExecuteReader();

            var messageStrings = new List<string>();
            while (reader.Read())
            {
                messageStrings.Add(reader.GetString(0));
            }

            return messageStrings.Select(ChatMessage.FromUser).ToList();
        }

        public static void DeleteUserMessages(Message message)
        {
            using var connection = new SQLiteConnection(ConnectionString);
            connection.Open();
            var command = new SQLiteCommand();
            command.Connection = connection;
            command.CommandText = "delete from Users where user_id = @user_id";

            command.Parameters.AddWithValue("@user_id", message.Chat.Id);

            command.ExecuteNonQuery();
        }

}
}
