using Kimi.Logging;

namespace Kimi.Services.Core
{
    public class Token
    {
        public static string GetToken()
        {
            try
            {
                Log.Write("Fetching token!");
                var path = @$"{Info.AppDataPath}\token.kimi";

                string[] token = File.ReadAllLines(path);

                if (!Info.IsDebug)
                    return token[0];
                else
                    return token[1];

            }
            catch (IndexOutOfRangeException ex)
            {
                Log.Write(ex.Message, Severity.Error);
                Log.Write("Probably the token for this instance hasn't been defined, using default token as fallback...", Severity.Warning);
                Info.IsDebug = false;
                return GetToken();
            }
            catch(Exception ex) when (ex is FileNotFoundException || ex is DirectoryNotFoundException)
            {
                Log.Write(ex.Message, Severity.Error);
                Log.Write("The file and directory will be automatically created", Severity.Warning);
                Log.Write("Creating directory...");
                CreateDirectory();

                string path = $@"{Info.AppDataPath}\token.kimi";

                Console.Write("Insert your token: ");
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                string? token = Console.ReadLine();
                Console.ForegroundColor = default;
                if(string.IsNullOrWhiteSpace(token))
                    Environment.Exit(1);

                File.WriteAllText(path, token);

                return GetToken();
            }
            catch(Exception ex)
            {
                Log.Write(ex.Message, Severity.Fatal);
                Console.ReadKey();
                Environment.Exit(1);
                return ex.Message.ToString();
            }
        }

        private static void CreateDirectory()
        {
            
            Directory.CreateDirectory(@$"{Info.AppDataPath}");
        }
    }
}
