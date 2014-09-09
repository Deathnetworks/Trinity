using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Caching;
using Buddy.Coroutines;
using Zeta.Bot;
using Zeta.Bot.Coroutines;
using Zeta.Game;
using Zeta.TreeSharp;

namespace Trinity.Coroutines
{
    public class DeathBehavior
    {
        private static int DeathCount = 0;

        public static Composite OnDeathBehavior()
        {
            return new ActionRunCoroutine(ret => OnDeathRoutine());
        }

        private static async Task<bool> OnDeathRoutine()
        {
            if (!ZetaDia.IsInGame)
                return false;

            if (ZetaDia.IsLoadingWorld)
                return false;

            if (ZetaDia.Me.IsDead && ZetaDia.Service.Party.NumPartyMembers > 1)
            {
                // Dead in Party, wat for rez
                await Coroutine.Sleep(10000);
            }

            if (GameUI.IsElementVisible(GameUI.ReviveAtCorpseButton) && GameUI.ReviveAtCorpseButton.IsEnabled)
            {
                await Coroutine.Sleep(500);
                GameUI.SafeClickElement(GameUI.ReviveAtCorpseButton);
                return true;
            }
            if (GameUI.IsElementVisible(GameUI.ReviveAtCheckpointButton) && GameUI.ReviveAtCheckpointButton.IsEnabled)
            {
                await Coroutine.Sleep(500);
                GameUI.SafeClickElement(GameUI.ReviveAtCheckpointButton);
                return true;
            }
            if (GameUI.IsElementVisible(GameUI.ReviveInTownButton) && GameUI.ReviveInTownButton.IsEnabled)
            {
                await Coroutine.Sleep(500);
                GameUI.SafeClickElement(GameUI.ReviveInTownButton);
                return true;
            }

            return false;
        }
    }
}
