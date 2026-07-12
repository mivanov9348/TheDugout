using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace TheDugout.Database
{
    public static class SaveManager
    {
        private static string SavesFolder =>
            Path.Combine(Application.persistentDataPath, "Saves");

        /// <summary>
        /// Създава нов Save файл, копирайки MasterData едно към едно.
        /// Връща пътя до новия Save файл.
        /// </summary>
        public static string CreateNewSave(string saveName)
        {
            if (!Directory.Exists(SavesFolder))
                Directory.CreateDirectory(SavesFolder);

            string safeName = saveName.Replace(" ", "_");
            string savePath = Path.Combine(SavesFolder, $"{safeName}_{DateTime.Now:yyyyMMdd_HHmmss}.db");

            File.Copy(MasterDatabaseManager.MasterDbPath, savePath, overwrite: false);

            Debug.Log("New save created at: " + savePath);
            return savePath;
        }

        public static List<string> GetAllSaves()
        {
            if (!Directory.Exists(SavesFolder))
                return new List<string>();

            var files = Directory.GetFiles(SavesFolder, "*.db");
            return new List<string>(files);
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