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
            filteredFiles.MinBy(file => new FileInfo(file).CreationTime);

        public static string ConnectionString
        {
            get
            {
                if (OperatingSystem.IsWindows())
                    return @"Data Source = " + Environment.CurrentDirectory + @"\BotUsers.db; Version = 3;";
                return $@"Data Source  = {lastFile};BusyTimeout=5000;";
            }
        }

        public static void RegisterUsers(Message message)
        {
            using var connection = new SQLiteConnection(ConnectionString);
            connection.Open();
            var command = new SQLiteCommand();
            command.Connection = connection;
            command.CommandText = @"
                insert or ignore into Limits (user_id, first_name, last_name, messages_count, is_subscribed)
                VALUES (@UserId, @first_name, @last_name, 0, 0)";
            command.Parameters.AddWithValue("@UserId", message.Chat.Id);
            command.Parameters.AddWithValue("@first_name", message.Chat.FirstName);
            command.Parameters.AddWithValue("@last_name", message.Chat.LastName);
            command.ExecuteNonQuery();
        }

        public static bool Registered(Message message)
        {
            using var connection = new SQLiteConnection(ConnectionString);
            connection.Open();
            try
            {
                using var command = new SQLiteCommand();
                command.Connection = connection;
                command.CommandText = "select count(*) from Limits where user_id = @user_id";
                command.Parameters.AddWithValue("@user_id", message.Chat.Id);
                var count = Convert.ToInt64(command.ExecuteScalar());
                Console.WriteLine(count);
                return count > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }

        }

        public static void SaveMessage(Message message, string prompt)
        {
            try
            {
                using var connection = new SQLiteConnection(ConnectionString);
                Console.WriteLine(ConnectionString);
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving message: {ex.Message}");
            }
        }

        public static void UpdateMessageCount(Message message)
        {
            using var connection = new SQLiteConnection(ConnectionString);
            connection.Open();
            var command = new SQLiteCommand();
            command.Connection = connection;
            command.CommandText = "update Limits set messages_count = messages_count + 1 where user_id = @user_id";

            command.Parameters.AddWithValue("@user_id", message.Chat.Id);
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
            using var reader = command.ExecuteReader();

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

        public static bool CanSendMessage(Message message, int messageLimit)
        {
            using var connection = new SQLiteConnection(ConnectionString);
            connection.Open();

            using var command = new SQLiteCommand();
            command.Connection = connection;
            command.CommandText = "select is_subscribed, messages_count from Limits where user_id = @user_id";
            command.Parameters.AddWithValue("@user_id", message.Chat.Id);

            using var reader = command.ExecuteReader();

            if (!reader.Read())
            {
                return false;
            }

            var hasSubscription = reader.GetInt32(0) == 1;
            var messageCount = reader.GetInt32(1);

            return hasSubscription || messageCount < messageLimit;
        }

        public static bool SetSubscription(Message message)
        {
            using var connection = new SQLiteConnection(ConnectionString);
            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                var command = new SQLiteCommand();
                command.Connection = connection;
                command.CommandText = "select is_subscribed from Limits where user_id = @user_id";
                command.Parameters.AddWithValue("@user_id", message.Chat.Id);
                var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    var isSubscribed = reader.GetInt32(0);
                    if (isSubscribed == 0)
                    {
                        reader.Close();
                        command.CommandText = "update Limits set is_subscribed = 1 where user_id = @user_id";
                        command.ExecuteNonQuery();
                        transaction.Commit();
                        return true; // Подписка активирована
                    }

                    transaction.Commit();
                    return false; // Подписка уже активна
                }

                reader.Close();
                command.CommandText = "insert into Limits (user_id, messages_count, is_subscribed) values (@user_id, 0, 1)";
                command.ExecuteNonQuery();
                transaction.Commit();
                return true; // Подписка создана и активирована
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }

        }

        public static int FreeMessagesLeft(Message message)
        {
            using var connection = new SQLiteConnection(ConnectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                using var command = new SQLiteCommand();
                command.Connection = connection;
                command.CommandText = "select messages_count from Limits where user_id = @user_id";
                command.Parameters.AddWithValue("@user_id", message.Chat.Id);

                using var reader = command.ExecuteReader();
                reader.Read();
                var leftMessages = reader.GetInt32(0);
                reader.Close();
                transaction.Commit();
                return leftMessages;
                
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine($"Error: {ex.Message}");
                return 0;
                
            }
        }

        public static bool HasSubscription(Message message)
        {
            using var connection = new SQLiteConnection(ConnectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                using var command = new SQLiteCommand();
                command.Connection = connection;
                command.CommandText = "select is_subscribed from Limits where user_id = @user_id";
                command.Parameters.AddWithValue("@user_id", message.Chat.Id);

                using var reader = command.ExecuteReader();
                reader.Read();
                var isSubscribed = reader.GetInt32(0);
                reader.Close();
                transaction.Commit();
                return isSubscribed == 1;

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine($"Error: {ex.Message}");
                return false;

            }
        }
    }

}
