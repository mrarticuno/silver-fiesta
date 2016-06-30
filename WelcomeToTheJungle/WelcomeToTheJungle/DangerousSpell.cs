using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;

namespace WelcomeToTheJungle
{

    /*
        Credits to MarioGK AKA Hu3 Lord <3
        */
    public class DangerousSpell
    {
        public bool DefaultValue;
        public int Delay;
        public Champion Hero;

        public SpellSlot Slot;

        public DangerousSpell(Champion hero, SpellSlot slot, bool defaultvalue = true, int delay = 0)
        {
            Slot = slot;
            Hero = hero;
            DefaultValue = defaultvalue;
            Delay = delay;
        }
    }

    public class DangerousSpells
    {
        public static List<DangerousSpell> Spells = new List<DangerousSpell>
        {
            new DangerousSpell(Champion.Aatrox, SpellSlot.Q, false),
            new DangerousSpell(Champion.Ahri, SpellSlot.E, false),
            new DangerousSpell(Champion.Amumu, SpellSlot.R),
            new DangerousSpell(Champion.Annie, SpellSlot.R),
            new DangerousSpell(Champion.Ashe, SpellSlot.R),
            new DangerousSpell(Champion.Azir, SpellSlot.R),
            new DangerousSpell(Champion.Bard, SpellSlot.Q, false),
            new DangerousSpell(Champion.Blitzcrank, SpellSlot.Q),
            new DangerousSpell(Champion.Brand, SpellSlot.R),
            new DangerousSpell(Champion.Braum, SpellSlot.R),
            new DangerousSpell(Champion.Caitlyn, SpellSlot.R, true, 600),
            new DangerousSpell(Champion.Cassiopeia, SpellSlot.R),
            new DangerousSpell(Champion.Chogath, SpellSlot.R),
            new DangerousSpell(Champion.Darius, SpellSlot.R, true, 100),
            new DangerousSpell(Champion.Draven, SpellSlot.R),
            new DangerousSpell(Champion.Ekko, SpellSlot.R),
            new DangerousSpell(Champion.Elise, SpellSlot.E),
            new DangerousSpell(Champion.Evelynn, SpellSlot.R, false),
            new DangerousSpell(Champion.Ezreal, SpellSlot.R),
            new DangerousSpell(Champion.Fiora, SpellSlot.R, true, 300),
            new DangerousSpell(Champion.Fizz, SpellSlot.R, true, 150),
            new DangerousSpell(Champion.Galio, SpellSlot.R),
            new DangerousSpell(Champion.Garen, SpellSlot.R),
            new DangerousSpell(Champion.Gnar, SpellSlot.R),
            new DangerousSpell(Champion.Gragas, SpellSlot.R),
            new DangerousSpell(Champion.Graves, SpellSlot.R),
            new DangerousSpell(Champion.Hecarim, SpellSlot.R, true, 150),
            new DangerousSpell(Champion.Illaoi, SpellSlot.R),
            new DangerousSpell(Champion.JarvanIV, SpellSlot.R),
            new DangerousSpell(Champion.Jax, SpellSlot.E, false, 350),
            new DangerousSpell(Champion.Jinx, SpellSlot.R, true, 600),
            new DangerousSpell(Champion.Kalista, SpellSlot.E, false),
            new DangerousSpell(Champion.Karthus, SpellSlot.R, true, 2800),
            new DangerousSpell(Champion.Kassadin, SpellSlot.R, false),
            new DangerousSpell(Champion.Katarina, SpellSlot.R),
            new DangerousSpell(Champion.Kennen, SpellSlot.R, true, 150),
            new DangerousSpell(Champion.Leblanc, SpellSlot.R),
            new DangerousSpell(Champion.LeeSin, SpellSlot.R),
            new DangerousSpell(Champion.Leona, SpellSlot.R),
            new DangerousSpell(Champion.Lissandra, SpellSlot.R),
            new DangerousSpell(Champion.Lux, SpellSlot.R),
            new DangerousSpell(Champion.Lux, SpellSlot.Q, false, 350),
            new DangerousSpell(Champion.Malphite, SpellSlot.R),
            new DangerousSpell(Champion.Malzahar, SpellSlot.R),
            new DangerousSpell(Champion.MissFortune, SpellSlot.R, true, 150),
            new DangerousSpell(Champion.MonkeyKing, SpellSlot.R),
            new DangerousSpell(Champion.Mordekaiser, SpellSlot.R, true, 650),
            new DangerousSpell(Champion.Morgana, SpellSlot.R, true, 2800),
            new DangerousSpell(Champion.Nami, SpellSlot.R, true, 700),
            new DangerousSpell(Champion.Nami, SpellSlot.Q, false, 250),
            new DangerousSpell(Champion.Nautilus, SpellSlot.R),
            new DangerousSpell(Champion.Nocturne, SpellSlot.R, true, 650),
            new DangerousSpell(Champion.Nunu, SpellSlot.R, true, 2800),
            new DangerousSpell(Champion.Olaf, SpellSlot.E, false, 350),
            new DangerousSpell(Champion.Orianna, SpellSlot.R),
            new DangerousSpell(Champion.Poppy, SpellSlot.R),
            new DangerousSpell(Champion.Riven, SpellSlot.R),
            new DangerousSpell(Champion.Rumble, SpellSlot.R, true, 150),
            new DangerousSpell(Champion.Sejuani, SpellSlot.R),
            new DangerousSpell(Champion.Skarner, SpellSlot.R),
            new DangerousSpell(Champion.Sona, SpellSlot.R),
            new DangerousSpell(Champion.Syndra, SpellSlot.R),
            new DangerousSpell(Champion.Talon, SpellSlot.R),
            new DangerousSpell(Champion.Tristana, SpellSlot.R),
            new DangerousSpell(Champion.Varus, SpellSlot.R),
            new DangerousSpell(Champion.Veigar, SpellSlot.R),
            new DangerousSpell(Champion.Velkoz, SpellSlot.R),
            new DangerousSpell(Champion.Vi, SpellSlot.R, true, 150),
            new DangerousSpell(Champion.Viktor, SpellSlot.R),
            new DangerousSpell(Champion.Vladimir, SpellSlot.R),
            new DangerousSpell(Champion.Warwick, SpellSlot.R),
            new DangerousSpell(Champion.Yasuo, SpellSlot.R),
            new DangerousSpell(Champion.Zed, SpellSlot.R, true, 2850),
            new DangerousSpell(Champion.Zyra, SpellSlot.R, true, 250)
        };
    }
}
