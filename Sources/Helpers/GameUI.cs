using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Zeta;
using Zeta.Common;
using Zeta.CommonBot;
using Zeta.Internals;

namespace Trinity
{
    public class GameUI
    {
        private const ulong mercenaryOKHash = 1591817666218338490;
        private const ulong conversationSkipHash = 0x942F41B6B5346714;
        private const ulong talkToInteractButton1 = 0x8EB3A93FB1E49EB8;
        private const ulong confirmTimedDungeonOK = 0xF9E7B8A635A4F725;

        public static UIElement ConfirmTimedDungeonOK
        {
            get
            {
                if (UIElement.IsValidElement(confirmTimedDungeonOK))
                    return UIElement.FromHash(confirmTimedDungeonOK);
                else
                    return null;
            }
        }

        public static UIElement MercenaryOKButton
        {
            get
            {
                if (UIElement.IsValidElement(mercenaryOKHash))
                    return UIElement.FromHash(mercenaryOKHash);
                else
                    return null;
            }
        }

        public static UIElement ConversationSkipButton
        {
            get
            {
                if (UIElement.IsValidElement(conversationSkipHash))
                    return UIElement.FromHash(conversationSkipHash);
                else
                    return null;
            }
        }
        public static UIElement PartyLeaderBossAccept
        {
            get
            {
                if (UIElement.IsValidElement(0x69B3F61C0F8490B0))
                    return UIElement.FromHash(0x69B3F61C0F8490B0);
                else
                    return null;
            }
        }
        public static UIElement PartyFollowerBossAccept
        {
            get
            {
                if (UIElement.IsValidElement(0xF495983BA9BE450F))
                    return UIElement.FromHash(0xF495983BA9BE450F);
                else
                    return null;
            }
        }
        public static UIElement GenericOK
        {
            get
            {
                if (UIElement.IsValidElement(0x891D21408238D18E))
                    return UIElement.FromHash(0x891D21408238D18E);
                else
                    return null;
            }
        }
        public static UIElement TalktoInteractButton1
        {
            get
            {
                if (UIElement.IsValidElement(talkToInteractButton1))
                    return UIElement.FromHash(talkToInteractButton1);
                else
                    return null;
            }
        }

        public static bool IsElementVisible(UIElement element)
        {
            if (element == null)
                return false;
            if (!element.IsValid)
                return false;
            if (!element.IsVisible)
                return false;

            return true;
        }

        public static void SafeClickElement(UIElement element, string name = "")
        {
            if (ZetaDia.Me.IsValid && !ZetaDia.Me.IsDead)
            {
                if (IsElementVisible(element))
                {
                    GameEvents.FireWorldTransferStart();
                    Thread.Sleep(250);
                    Logging.Write("[QuestTools] Clicking UI element {0} ({1})", name, element.BaseAddress);
                    element.Click();
                    Thread.Sleep(250);
                }
            }
        }

        private static DateTime lastCheckedUIButtons = DateTime.MinValue;
        public static void SafeClickUIButtons()
        {
            SafeClickElement(ConversationSkipButton, "Conversation Button");
            SafeClickElement(PartyLeaderBossAccept, "Party Leader Boss Accept");
            SafeClickElement(PartyFollowerBossAccept, "Party Follower Boss Accept");

            if (DateTime.Now.Subtract(lastCheckedUIButtons).TotalMilliseconds <= 500)
                return;

            lastCheckedUIButtons = DateTime.Now;

            if (ZetaDia.Me.LoopingAnimationEndTime <= 0)
            {
                SafeClickElement(MercenaryOKButton, "Mercenary OK Button");
                SafeClickElement(GenericOK, "GenericOK");
                SafeClickElement(UIElements.ConfirmationDialogOkButton, "ConfirmationDialogOKButton");
                SafeClickElement(ConfirmTimedDungeonOK, "Confirm Timed Dungeon OK Button");
            }
        }

        public static bool IsPartyDialogVisible
        {
            get
            {
                return IsElementVisible(PartyFollowerBossAccept) || IsElementVisible(PartyLeaderBossAccept);
            }
        }
    }
}
