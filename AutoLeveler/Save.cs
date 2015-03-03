using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using LeagueSharp;

namespace AutoLeveler
{
    internal class Save
    {
        public string Data;
        public string FilePath;
        public int[] Sequence;
        public Items List;
        public Champion Champion;

        public Save(string path, string data, bool updateSave = false)
        {
            FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
            Data = data;

            if (!File.Exists(FilePath) || updateSave)// || IsNewerVersion())
            {
                File.WriteAllText(FilePath, Data);
            }

            var serializer = new XmlSerializer(typeof(Items));
            List = (Items)serializer.Deserialize(new FileStream(FilePath, FileMode.Open));

            var champs = List.Champion;
            Champion = champs.FirstOrDefault(o => o.Name.Equals(ObjectManager.Player.ChampionName));

            //new champ
            if (Champion == null)
            {
                Array.Clear(Sequence, 0, 18);
                List.Champion[List.Champion.Length + 1] = new Champion
                {
                    Name = ObjectManager.Player.ChampionName,
                    Sequence =
                        String.Join("", new List<int>(Sequence).ConvertAll(i => i.ToString() + ", ").ToArray())
                            .TrimEnd(',', ' ')
                };
                Champion = List.Champion[List.Champion.Length];
            }
            else
            {
                Sequence = Champion.Sequence.Trim().Split(',').Select(n => Convert.ToInt32(n)).ToArray();
            }

            AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

        }

        private void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            SaveData();
        }

        private void CurrentDomain_DomainUnload(object sender, EventArgs e)
        {
            SaveData();
        }

        public void SaveData()
        {
            try
            {
                var serializer = new XmlSerializer(typeof(Items));
                serializer.Serialize(new FileStream(FilePath, FileMode.Create), List);
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.ToString());
            }
        }

        public void UpdateSequence(int[] sequence)
        {
            Sequence = sequence;
            Champion.Sequence = String.Join("", new List<int>(Sequence).ConvertAll(i => i.ToString() + ", ").ToArray()).TrimEnd(',', ' ');
        }
    }

    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false, ElementName = "levels")]
    public class Items
    {
        [XmlElement("Champion")]
        public Champion[] Champion { get; set; }
    }


    [XmlType(AnonymousType = true, TypeName = "levelsChampion")]
    public class Champion
    {
        public string Name { get; set; }
        public string Sequence { get; set; }
    }
}