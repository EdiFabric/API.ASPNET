using System.Diagnostics;

namespace EdiFabric.Api.ASPNET
{
    public class TokenFileCache
    {
        //  Change path to whatever you prefer
        private static string _tokenFile = Directory.GetCurrentDirectory() + @"\token.txt";

        public static void Set(string serialKey)
        {
            try
            {
                var token = ReadFromCache(serialKey);
                SerialKey.SetToken(token);

                //  Refresh token before expiration
                Refresh(serialKey);
            }
            catch
            {
                //  Try one last time
                try
                {
                    var token = GetFromApi(serialKey);
                    WriteToCache(token);
                    SerialKey.SetToken(token);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    //  Contact support@edifabric.com for assistance
                    throw;
                }
            }
        }

        private static void Refresh(string serialKey)
        {
            try
            {
                //  Refresh the token two days before it expires
                if (SerialKey.DaysToExpiration < 3)
                    WriteToCache(GetFromApi(serialKey));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                //  If can't get a token a day before the current expires - throw an exception
                //  Otherwise keep trying
                if (SerialKey.DaysToExpiration <= 1)
                    throw;
            }
        }

        private static string GetFromApi(string serialKey)
        {
            int retries = 3;
            int index = 0;

            //  Try to get a token with retries
            while (index < retries)
            {
                try
                {
                    return SerialKey.GetToken(serialKey);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    index++;

                    if (index >= retries)
                        throw;
                }
            }

            throw new Exception("Can't get a token.");
        }

        private static string ReadFromCache(string serialKey)
        {
            if (!File.Exists(_tokenFile))
            {
                var token = GetFromApi(serialKey);
                WriteToCache(token);
                return token;
            }

            return File.ReadAllText(_tokenFile);
        }

        private static void WriteToCache(string token)
        {
            File.WriteAllText(_tokenFile, token);
        }
    }
}
