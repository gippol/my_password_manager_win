using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using PasswordManager.Models;

namespace PasswordManager.Services
{
    public class CsvFileService
    {
        private readonly CryptoService _crypto;

        public CsvFileService(CryptoService crypto)
        {
            _crypto = crypto;
        }

        public string FindLatestFile()
        {
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var files = Directory.GetFiles(dir, "passwords_*.csv");
            return files.OrderByDescending(f => f).FirstOrDefault();
        }

        public List<PasswordEntry> Load(string filePath, string passphrase)
        {
            var entries = new List<PasswordEntry>();
            var lines = File.ReadAllLines(filePath, Encoding.UTF8);

            // skip header
            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var fields = ParseCsvLine(line);
                if (fields.Count < 4) continue;

                var decryptedPassword = string.Empty;
                if (!string.IsNullOrEmpty(fields[2]))
                    decryptedPassword = _crypto.Decrypt(fields[2], passphrase);

                entries.Add(new PasswordEntry
                {
                    Site = fields[0],
                    Username = fields[1],
                    Password = decryptedPassword,
                    Notes = fields[3]
                });
            }
            return entries;
        }

        public string Save(IEnumerable<PasswordEntry> entries, string passphrase)
        {
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var filePath = Path.Combine(dir, $"passwords_{timestamp}.csv");

            var sb = new StringBuilder();
            sb.AppendLine("Site,Username,Password,Notes");

            foreach (var e in entries)
            {
                var encryptedPassword = string.IsNullOrEmpty(e.Password)
                    ? string.Empty
                    : _crypto.Encrypt(e.Password, passphrase);

                sb.AppendLine(
                    $"{EscapeCsv(e.Site)}," +
                    $"{EscapeCsv(e.Username)}," +
                    $"{EscapeCsv(encryptedPassword)}," +
                    $"{EscapeCsv(e.Notes)}");
            }

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
            return filePath;
        }

        private static string EscapeCsv(string field)
        {
            if (field == null) return string.Empty;
            if (field.Contains(',') || field.Contains('"') || field.Contains('\n'))
                return $"\"{field.Replace("\"", "\"\"")}\"";
            return field;
        }

        private static List<string> ParseCsvLine(string line)
        {
            var fields = new List<string>();
            var sb = new StringBuilder();
            bool inQuotes = false;
            int i = 0;

            while (i < line.Length)
            {
                char c = line[i];
                if (inQuotes)
                {
                    if (c == '"')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '"')
                        {
                            sb.Append('"');
                            i += 2;
                        }
                        else
                        {
                            inQuotes = false;
                            i++;
                        }
                    }
                    else
                    {
                        sb.Append(c);
                        i++;
                    }
                }
                else
                {
                    if (c == '"')
                    {
                        inQuotes = true;
                        i++;
                    }
                    else if (c == ',')
                    {
                        fields.Add(sb.ToString());
                        sb.Clear();
                        i++;
                    }
                    else
                    {
                        sb.Append(c);
                        i++;
                    }
                }
            }
            fields.Add(sb.ToString());
            return fields;
        }
    }
}
