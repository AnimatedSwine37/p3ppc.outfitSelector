using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace p3ppc.outfitSelector
{
    public class Enums
    {
        public enum EquipmentType
        {
            Weapon = 0,
            Body = 1,
            Feet = 2,
            Accessory = 3,
            Outfit = 4,
        }

        public enum PartyMember
        {
            MaleProtag = 1,
            Yukari = 2,
            Aigis = 3,
            Mitsuru = 4,
            Junpei = 5,
            Fuuka = 6,
            Akihiko = 7,
            Ken = 8,
            Sinjiro = 9,
            Koromaru = 10,
            FemaleProtag = 99,
        }

        public enum MaleProtagOutfit
        {
            Default = -1,
            Swimsuit = 0,
            Summer = 1,
            Winter = 2,
            Shirt_Of_Chivalry = 5,
            Winter_No_SEES = 9
        }

        public enum YukariOutfit
        {
            Default = -1,
            Swimsuit = 0,
            Summer = 1,
            Winter = 2,
            Maid = 3,
            High_Cut = 5,
            Christmas = 6
        }

        public enum AigisOutfit
        {
            Default = -1,
            Blue_Dress = 0,
            Maid = 3,
            School_Uniform = 4,
            Christmas_Outfit = 6,
        }

        public enum MitsuruOutfit
        {
            Default = -1,
            Swimsuit = 0,
            Summer = 1,
            Winter = 2,
            Maid = 3,
            High_Cut = 5,
            Chrismtas = 6
        }

        public enum JunpeiOutfit
        {
            Default = -1,
            Swimsuit = 0,
            Summer = 1,
            Winter = 2,
            Shirt_Of_Chivalry = 5,
            Butler = 6
        }

        public enum AkihikoOutfit
        {
            Default = -1,
            Swimsuit = 0,
            Summer = 1,
            Winter = 2,
            Shirt_Of_Chivalry = 5,
            Butler = 6
        }

        public enum KenOutfit
        {
            Default = -1,
            Swimsuit = 0,
            Summer = 1,
            Winter = 2,
            Shirt_Of_Chivalry = 5,
            Butler = 6
        }

        public enum ShinjiroOutfit
        {
            Default = -1,
            Shirt_Of_Chivalry = 5,
            Butler = 6
        }

        public enum KoromaruOutfit
        {
            Default = -1,
            Butler = 6,
        }

        public enum FemaleProtagOutfit
        {
            Default = -1,
            Swimsuit = 0,
            Summer = 1,
            Winter = 2,
            Maid = 3,
            High_Cut = 5,
            Winter_No_SEES = 9
        }
    }
}
