using Rocket.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CharacterSystem.Models;

namespace CharacterSystem
{
    public class Config : IRocketPluginConfiguration
    {
        public int MinimumKarakter, MaximumKarakter;
        public string AtılmaMesajı;
        public List<AloshaCharacter> Characters = new List<AloshaCharacter>();
        public void LoadDefaults()
        {
            MinimumKarakter = 4;
            MaximumKarakter = 32;
            AtılmaMesajı = "Karakterin oluşturuldu ve şimdi sunucudan atılıyorsun. Tekrar gireceksin.";
            Characters = new List<AloshaCharacter>();
        }
    }
}
