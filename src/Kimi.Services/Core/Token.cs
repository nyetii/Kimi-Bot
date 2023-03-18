namespace Kimi.Services.Core
{
    internal class Token
    {
        public static string GetToken()
        {
            try
            {
                //Log.Information("[{Source}] Fetching token!", "Kimi");
                string path;
                //#if DEBUG
                //path = @$"{Info.AppDataPath}\debugtoken.kimi";
                //#else
                path = @$"{Info.AppDataPath}\token.kimi";
                //#endif

                string[] token = File.ReadAllLines(path);

                if (!Info.IsDebug)
                    return token[0];
                else
                    return token[1];

            }
            catch (IndexOutOfRangeException ex)
            {
                //Log.Error("[{Source}] {ex}", "Kimi", ex.Message);
                //Log.Error("[{Source}] Probably the token for this instance hasn't been defined, using default token as fallback...", "Kimi");
                Info.IsDebug = false;
                return GetToken();
            }
            catch(Exception ex) when (ex is FileNotFoundException || ex is DirectoryNotFoundException)
            {
                //Log.Error("[{Source}] {ex}", "Kimi", ex.Message);
                //Log.Information("[{Source}] The file and directory will be automatically created.", "Kimi");
                //Log.Information("[{Source}] Creating directory...", "Kimi");
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
                //Log.Error("[{Source}] {ex}", "Kimi", ex.Message);
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
