using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YARG.Assets.Script.YargAP
{
    internal class APData
    {
        public enum APFiller
        {
            YargGem = 1
        }

        public static readonly Dictionary<string, string> SongHashMap = new()
        {
            {"52C851738B94A7A5DF709847845CA67A2B21E0A7", "106"},
            {"DCB9E53BB2EBA6F80464E7C85001EBAD46254218", "1nput This 2 Y0ur Spine"},
            {"531D290A24AD4BF99CCB84F3EEDDC36E1A838FB6", "322"},
            {"AC4EDC6887766803CA05824E6310100ADBC322A4", "A Visitant (feat. Victor Borba)"},
            {"EDC31DD1F144396A29126815D2B4AF2853CF1B43", "About the Author"},
            {"FC05CF82FB15C5BE5BA773ECFC68D966C966AA93", "The Afterparty"},
            {"B2DE40B7B699EF6DF98B58F4397BBB764E1883AA", "Al Gore Rhythm"},
            {"22562DF7EDF850606BF4CF6A198B0CA3485F8343", "Alibi"},
            {"548AC4034DA0039A5A3215F776C53BD561CF461E", "All of a Sudden"},
            {"B53076C9016F125F08495B2C40BF51956355E9A0", "Allure"},
            {"786933623E47B517C4CBD9A5AB3A8E1D034B6BA8", "Avatar the Last Cakebender"},
            {"EEBB8A500509AF709444E4BE9C02AA990CA1190B", "Avatara"},
            {"A9479E255F6BFCCF8F049D38916C79E7AC8251FB", "Bathe in Blood"},
            {"4D0D6EC50ABE7935E3D38C86251CCA98C474D83D", "Bedroom Community"},
            {"462C37CA24013E5CFE9CF7EE59410DE4E94B3D7B", "Beverly"},
            {"6545406B7F52979D70B77D45AE01EF5898242FB3", "Blue (feat. Miori Celesta)"},
            {"39FE697F1ED8A7F2D1AB41A8D7A2C6C0BDF26DD2", "Boom Slayer (feat. Scott Foster Harris)"},
            {"BD92740798FC5603C9DDD49F3E0AF8D337050679", "Bottleneck"},
            {"9B206AA92E0BA4E3568D3461984A8A594091CD90", "Buds"},
            {"C03DAD52DF099D947862F100DDBF97D6EDE2CCF1", "Butterflies"},
            {"47335DA19DF08DD925C815CEABD99DA62970C2BB", "Choked Up"},
            {"8D86825182F422799B936455EB648A82A9E8454D", "Circles"},
            {"86B7F9637871D85B515D5EC201E72EF1051725B7", "Cowboy Tanaka"},
            {"8BCFBD61DD50D0F283A3801E28DE16B3EB066766", "Cruisin'"},
            {"BDFAA2F66E0B8EDA0AEA5945E8A0434A07AF7A3E", "Discipline"},
            {"A6929E6B409D99E9513E38B9649DD288BA7448E5", "Do"},
            {"A16839828E73CADA26D2BCCF9E8501222CF1B8D1", "Don't Look!"},
            {"D6AA37C501263468E77F4670A5EBFF28E37B6235", "Don't Spook the Owl"},
            {"AFA52DDA0627E83F5C4E68138D2CC3D8918BDD3F", "Dreamweaver"},
            {"25915BB3C7606C7668D162176D3C222C1CC09AB7", "Duvet Thief"},
            {"88E5C060F18300874F42356B269A7E3B868900E5", "Eleven"},
            {"CC73301EE43523F770298EF8C42E71C4AD6851E5", "Emperor Rising"},
            {"6B7C13AB0A0DE5A68C3EA9CE7F7DC0144CE22C4B", "Empires"},
            {"D2F3931B3935803FE232F03C505F8ADA96F12CCD", "Everybody Do the Flop"},
            {"B04B15D2A39B50D20B188C7DD9CDE2A050315779", "Exeter"},
            {"634DDD8A10E92E628FEC82E7E4A22FD6765A3D2F", "Fine"},
            {"D75B1310E1270EAB4D9C4BBBE9939DAC677EC47E", "Flight of the Bumblebee"},
            {"7C22C512894EB9D850D765925083CFD1939ED30E", "Formless Collective"},
            {"60172C4347C055A9E5729BE8AAB099401F8828FE", "Frank Scored a Video Game"},
            {"955C2DA6273CB614EA3A0A0CCB4E01B05C375385", "Front Row Seats (To Watching Your World Burn)"},
            {"188B7FD69BBA9E6CED79B094BC6AAFFB224FECCD", "The Game"},
            {"663383F7484BC25564B8CF834F7095F10AEDC45B", "God Only Knows (feat. Kasane Teto)"},
            {"63EA4B3A3255122D24BF59A5641CB9932193DB7A", "Guess I'll Never Know"},
            {"69F660A6BDA34F99458A7AC42A5EEB7C2C636053", "Half Measures"},
            {"CFB6BFEEA144DDC0966EB1A9C36B999592DF09E9", "I Don't Wanna Talk"},
            {"A90C1A3B3AD824AED10F71573E48EFEF8DA2EF56", "I Wish That I Could Fall (feat. GUMI)"},
            {"8AB8073AEDE1C8AD90C50E9327FA4878CF715332", "I'm a Bug"},
            {"23E5F609F414EF72DFD7A5E91868ADCC51D7642C", "Igowallah"},
            {"27D9ACF2FD36A772D663EF4B5D5F38B17670F969", "In My Head"},
            {"48C57BD9C1B96C406B3CAEA8AF1B556B4AA67BD5", "Is This What You Wanted"},
            {"9D7BC33A510C6B430DA9EA143A08590CB4F5C995", "It Kills Me"},
            {"4CB78DC47141688A7C04F77F53452A7FFCB4A845", "Jenny B"},
            {"8988E80F1AA37FF5EC5E972429367B6226771470", "John, Take Me with You"},
            {"3150E6D897D679A2BC713A1F799CDEA5FE0A165B", "Join the Club"},
            {"A52D5E2F00A191C1E5DE5550A6E8B260AD16B9BC", "Languish"},
            {"DE649194B88D5F8DE89C402272E1EFF3443082D1", "Long in the Tooth"},
            {"BF567734BA20A4DE1336D7AC180B569104420AB0", "Luminaire"},
            {"4C1CEF8F5AEBF9E4AFE1DD2B0FA08804689DACA5", "Marlboro Mountain"},
            {"642723D03BD1A9D8325D885DD930964EE03BFB91", "The Masquerade"},
            {"6B517E9F5D11D2095C78107D4237A62AB1AED233", "Mass Gap"},
            {"1038FF7AB09FCC0141FD153F74477EA90432421E", "Moonlight Sonata 3rd Mvt (Big Band Version)"},
            {"B3E8F0DA935EBA3963E5E153FFFC5F36C665A88B", "Need 2"},
            {"D289263FFF9470A09B0A595AABCCD98F32507BE8", "The New World Disorder"},
            {"9E9C9556BA710FDEC598D7EE9DF6413AA2BF4B24", "No Nations"},
            {"B722FA06C06E89FA6859E261D6E84AE1D8A7A83A", "No Remedy"},
            {"1CF48BAFA736F9CA45BB54FDB96EBDE04CBDE10F", "Nomu"},
            {"FD57E5B742ABC10D3D3CADCD83C9001B1CAFCB82", "Numb the Mind"},
            {"0F49DD913FCEE544525749828B8B7235A48E54CC", "Oh, Krissy Baby!"},
            {"268BF9CC9427F6D62D21567BB83912F8EC9DE99D", "Oopsie Daisy"},
            {"D1B5251FB29996C27A0FDE5B61E10D13F7A94E87", "Over Again"},
            {"CED73C13274C7172D1FA6B2259FAE035AF6ADF4D", "Overdrive"},
            {"88204A8D79E8FC3317934A8207641308C13CDF09", "Oxygen"},
            {"1DCC6B01BF216235CF9C2AA4042CD61D7C2FC848", "Participation Trophy Wife"},
            {"757BCD0F8E154BA59F9322818B565C160BA1B322", "Pixel Galaxy"},
            {"28199E0E3F9845642C458CC253C0C71F3836855D", "Pizza Rolls"},
            {"A2C970D9623BE2CC65C6FB8C79E65371AA053D5F", "Plastic Boogie"},
            {"F2D20E64B122ADCB8FD2BF7845DDF5510A48BDE2", "Poser"},
            {"E7000CCF1160C159536D738EDB882A44A8E1A0CE", "Positively Clark Street"},
            {"8582624C05DB4AD75184477B67BF2357BF5AD11E", "Queen of the Night"},
            {"4878B263621247720957B46848F23379128CB7BD", "Runnin Man"},
            {"6D7948C79F81B9400B5854EF3E68F8EDB769A063", "Sadness (feat. Sapphire Noel)"},
            {"00392D1E6E65737723E47E4A31BB08E4B7CAF03F", "Seasons (feat. Shiki Miyoshino)"},
            {"5EC2554BE1B383E118AD696F1E0535188CD6F6F5", "Show Ya"},
            {"ED87C1F686D8D6942048B5FA179AC9464FA8DE91", "Smile for Me"},
            {"B4DB1094C97319C3D1719FB5E788A3DFC3B06E96", "Song of November"},
            {"E65A6B65BF7F0C1B46E4801572CAADD52AF4C079", "Spirit"},
            {"E69288F799AEB8E79126D271CA8A28C7C88947CD", "Splinter"},
            {"103AC1F80616836442E331FF02CDBF4BEEC0A2EB", "Stowaway Ants"},
            {"C106E6AE0D82B811FE0B5D93DDD78B73F48DB795", "Strangers Once Again (feat. Treb and Ofir Tabakov)"},
            {"8F4499B725EA51F9D161B4E3F4A66347C3507200", "Sweet Victory"},
            {"A466DEBE7212D651F1DA7666B12178CB5780DC46", "Synthespian"},
            {"26AED77F0BEBDFB019727D6A6F95ACDE6EF5E3BC", "They Call"},
            {"B8D53A0AB5504692E817FC29532E015101A3E756", "Time"},
            {"0215F5683487C33A3F1B247EA00078EF0AD0184C", "To Let Go"},
            {"FC5855E59E539B52102860627D35DEB18D512274", "Vehemence"},
            {"6E5438E01F009F20BB1760EB1766509D12ACAC25", "Voidwalker"},
            {"B87B46F8FFC03417161F286B3B11AE8FE852939E", "We All Float Down Here"},
        };

        private static Dictionary<long, string> _APLocationIDToHash;
        public static Dictionary<long, string> APLocationIDToHash()
        {
            if (_APLocationIDToHash is not null)
                return _APLocationIDToHash;
            _APLocationIDToHash = new();
            var Index = 1; //Song locations start at index 1 in the apworld 
            foreach(var i in SongHashMap)
            {
                _APLocationIDToHash[Index] = i.Key;
                Index++;
                _APLocationIDToHash[Index] = i.Key;
                Index++;
            }
            return _APLocationIDToHash;
        }

        private static Dictionary<string, long[]> _SongHashToAPLocations;
        public static Dictionary<string, long[]> SongHashToAPLocations()
        {
            if (_SongHashToAPLocations is not null)
                return _SongHashToAPLocations;
            _SongHashToAPLocations = new();
            int Index = 1; //Song locations start at index 1 in the apworld 
            foreach (var i in SongHashMap)
            {
                _SongHashToAPLocations[i.Key] = new long[] { Index, Index + 1 };
                Index += 2;
            }
            return _SongHashToAPLocations;
        }

        private static Dictionary<long, string> _APItemIDToHash;
        public static Dictionary<long, string> APItemIDToHash()
        {
            if (_APItemIDToHash is not null)
                return _APItemIDToHash;
            _APItemIDToHash = new();
            var Index = 2; //Song Items start at index 2 in the apworld, index 1 is Yarg Gem
            foreach (var i in SongHashMap)
            {
                _APItemIDToHash[Index] = i.Key;
                Index++;
            }
            return _APItemIDToHash;
        }
    }
}
