using System.IO;
using WPass.Core.Model;
using WPass.Utility.OtherHandler;
using WPass.Utility.SecurityHandler;

namespace WPass.Utility.DataHandler
{
    public static class DataImport
    {
        // Microsoft Edge profile
        // name,url,username,password,note
        public static async Task<(List<Entry>, List<Website>)> ReadCSV(string path)
        {
            var entries = new List<Entry>();
            var websites = new List<Website>();
            var temp = new List<(string name, string url, string username, string password, string note)>();

            try
            {
                if (File.Exists(path) && Path.GetExtension(path).Equals(".csv"))
                {
                    // Use FileStream to open the file
                    using FileStream fileStream = new(path, FileMode.Open, FileAccess.Read);
                    // Use StreamReader to read the file
                    using StreamReader reader = new(fileStream);
                    string line;
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                    while ((line = await reader.ReadLineAsync()) != null)
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                    {
                        var arr = line?.Split(',').Select(l => l.Trim().Replace("\"", "")).ToList();

                        temp.Add((arr?[0] ?? "", arr?[1] ?? "", arr?[2] ?? "", arr?[3] ?? "", arr?[4] ?? ""));
                    }
                    temp.RemoveAt(0); // remove header


                    // Handle logic
                    foreach (var (name, url, username, password, note) in temp)
                    {
                        if (string.IsNullOrEmpty(username)) continue;

                        var newEntry = new Entry
                        {
                            Username = username,
                            EncryptedPassword = Security.Encrypt(password),
                        };
                        var newWebsite = new Website
                        {
                            EntryId = newEntry.Id,
                            Url = url,
                        };

                        var existedEntry = entries.FirstOrDefault(r => r.Username == username && Security.Decrypt(r.EncryptedPassword) == password);

                        if (existedEntry != null)
                        {
                            // add website
                            newWebsite.EntryId = existedEntry.Id;
                            websites.Add(newWebsite);
                        }
                        else
                        {
                            // add new and website
                            entries.Add(newEntry);
                            websites.Add(newWebsite);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // log err
                Logger.Write(ex.Message);
            }

            return (entries, websites);
        }


    }
}
