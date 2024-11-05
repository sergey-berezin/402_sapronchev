using System;
using System.IO;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

public class SafeJsonFileHandler<T>
{
    private readonly string _filePath;
    private readonly string _tempFilePath;
    private readonly string _backupFilePath;

    public SafeJsonFileHandler(string filePath)
    {
        _filePath = filePath;
        _tempFilePath = filePath + ".temp";
        _backupFilePath = filePath + ".backup";
    }

    public void SaveData(T data)
    {
        try
        {
            string jsonData = JsonConvert.SerializeObject(data, Formatting.Indented);

            using (FileStream tempFileStream = new FileStream(_tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (StreamWriter writer = new StreamWriter(tempFileStream))
            {
                writer.Write(jsonData);
                writer.Flush();
                tempFileStream.Flush(true);
            }

            if (File.Exists(_filePath))
            {
                File.Copy(_filePath, _backupFilePath, true);
            }

            File.Move(_tempFilePath, _filePath, true);
        }
        catch (Exception ex)
        {
            if (File.Exists(_backupFilePath))
            {
                File.Copy(_backupFilePath, _filePath, true);
            }
            throw;
        }
        finally
        {
            if (File.Exists(_tempFilePath))
            {
                File.Delete(_tempFilePath);
            }
        }
    }

    public T LoadData()
    {
        try
        {
            if (!File.Exists(_filePath))
                throw new FileNotFoundException("Data file not found");

            string jsonData;
            using (StreamReader reader = new StreamReader(_filePath))
            {
                jsonData = reader.ReadToEnd();
            }

            return JsonConvert.DeserializeObject<T>(jsonData);
        }
        catch (Exception ex)
        {
            if (File.Exists(_backupFilePath))
            {
                File.Copy(_backupFilePath, _filePath, true);
            }
            throw;
        }
    }
}