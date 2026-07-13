using System;
using System.IO;
using System.Collections.Generic;
using SQLite;
using UnityEngine;
using TheDugout.Data;

namespace TheDugout.Database
{
    public static class SaveManager
    {
        private static string SavesFolder =>
            Path.Combine(Application.persistentDataPath, "Saves");

        public static string CreateNewSave(string saveName)
        {
            if (!Directory.Exists(SavesFolder))
                Directory.CreateDirectory(SavesFolder);

            string safeName = saveName.Replace(" ", "_");
            string savePath = Path.Combine(SavesFolder, $"{safeName}_{DateTime.Now:yyyyMMdd_HHmmss}.db");

            using (var connection = new SQLiteConnection(savePath))
            {
                // CODE-FIRST: схемата идва директно от моделите
                connection.CreateTable<Country>();
                connection.CreateTable<League>();
                connection.CreateTable<Team>();
                connection.CreateTable<Card>();
                connection.CreateTable<ManagerProfile>();

                // Съдържанието идва от JSON
                MasterDataImporter.ImportInto(connection);
            }

            Debug.Log("New save created at: " + savePath);
            return savePath;
        }

        public static List<string> GetAllSaves()
        {
            if (!Directory.Exists(SavesFolder))
                return new List<string>();

            return new List<string>(Directory.GetFiles(SavesFolder, "*.db"));
        }

        public static void DeleteSave(string savePath)
        {
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
                Debug.Log("Deleted save: " + savePath);
            }
        }
    }
}