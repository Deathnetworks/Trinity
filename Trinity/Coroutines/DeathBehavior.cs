using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zeta.Bot;
using Zeta.Game;
using Zeta.TreeSharp;

namespace Trinity.Coroutines
{
    public class DeathBehavior
    {
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

            if (GameUI.IsElementVisible(GameUI.ReviveAtCorpseButton) && GameUI.ReviveAtCorpseButton.IsEnabled)
            {
                GameUI.SafeClickElement(GameUI.ReviveAtCorpseButton);
                return true;
            }
            if (GameUI.IsElementVisible(GameUI.ReviveAtCheckpointButton) && GameUI.ReviveAtCheckpointButton.IsEnabled)
            {
                GameUI.SafeClickElement(GameUI.ReviveAtCheckpointButton);
                return true;
            }
            if (GameUI.IsElementVisible(GameUI.ReviveInTownButton) && GameUI.ReviveInTownButton.IsEnabled)
            {
                GameUI.SafeClickElement(GameUI.ReviveInTownButton);
                return true;
            }

            return false;
        }
    }
}
