using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.Helpers;
using Trinity.Objects;
using Trinity.Reference;
using Trinity.Settings.Loot;
using Trinity.Technicals;
using Trinity.UIComponents;
using Zeta.Game;
using Zeta.Game.Internals.Actors;

namespace Trinity.Items
{
    public class ItemList
    {
        internal static bool ShouldStashItem(CachedACDItem cItem)
        {
            if (cItem.AcdItem != null && cItem.AcdItem.IsValid)
            {
                bool result = false;                

                var item = new Item(cItem.AcdItem);

                var wrappedItem = new ItemWrapper(cItem.AcdItem);

                result = ShouldStashItem(item, cItem);                

                string action = result ? "KEEP" : "TRASH";

                return result;
            }
            return false;
        }

        internal static bool ShouldStashItem(Item referenceItem, CachedACDItem cItem)
        {
            Item item;

            if (!Legendary.Items.TryGetValue(cItem.AcdItem.ActorSNO, out item))
            {
                Logger.LogDebug("  >>  Unknown Item {0} {1} - Auto-keeping", cItem.RealName, cItem.AcdItem.ActorSNO);
                return true;   
            }

            if (cItem.AcdItem.IsCrafted)
            {
                Logger.LogDebug("  >>  Crafted Item {0} {1} - Auto-keeping", cItem.RealName, cItem.AcdItem.ActorSNO);
                return true;                       
            }

            var itemSetting = Trinity.Settings.Loot.ItemList.SelectedItems.FirstOrDefault(i => referenceItem.Id == i.Id);
            if (itemSetting != null)
            {
                Logger.LogDebug("  >>  {0} is a Selected ListItem with {1} rules", cItem.RealName, itemSetting.Rules.Count);
                
                foreach (var itemRule in itemSetting.Rules)
                {
                    var result = false;
                    string friendlyVariant = string.Empty;
                    double itemValue = 0;
                    double ruleValue = 0;

                    switch (itemRule.ItemProperty)
                    {
                        case ItemProperty.Ancient:
                            itemValue = cItem.IsAncient ? 1 : 0;
                            ruleValue = itemRule.Value;
                            result = cItem.IsAncient == (itemRule.Value == 1);   
                            break;

                        case ItemProperty.PrimaryStat:
                            itemValue = ItemDataUtils.GetMainStatValue(cItem.AcdItem);
                            ruleValue = itemRule.Value;
                            result = itemValue >= ruleValue;
                            break;

                        case ItemProperty.CriticalHitChance:
                            itemValue = cItem.CritPercent;
                            ruleValue = itemRule.Value;
                            result = itemValue >= ruleValue;
                            break;

                        case ItemProperty.CriticalHitDamage:
                            itemValue = cItem.CritDamagePercent;
                            ruleValue = itemRule.Value;
                            result = itemValue >= ruleValue;
                            break;

                        case ItemProperty.AttackSpeed:
                            itemValue = Math.Round(cItem.AttackSpeedPercent, MidpointRounding.AwayFromZero);
                            ruleValue = itemRule.Value;
                            result = itemValue >= ruleValue;
                            break;

                        case ItemProperty.ResourceCost:
                            itemValue = Math.Round(cItem.AcdItem.Stats.ResourceCostReductionPercent, MidpointRounding.AwayFromZero);
                            ruleValue = itemRule.Value;
                            result = itemValue >= ruleValue;
                            break;

                        case ItemProperty.Cooldown:
                            itemValue = Math.Round(cItem.AcdItem.Stats.PowerCooldownReductionPercent, MidpointRounding.AwayFromZero);
                            ruleValue = itemRule.Value;
                            result = itemValue >= ruleValue;
                            break;

                        case ItemProperty.ResistAll:
                            itemValue = cItem.AcdItem.Stats.ResistAll;
                            ruleValue = itemRule.Value;
                            result = itemValue >= ruleValue;
                            break;

                        case ItemProperty.Sockets:
                            itemValue = cItem.AcdItem.Stats.Sockets;
                            ruleValue = itemRule.Value;
                            result = itemValue >= ruleValue;
                            break;

                        case ItemProperty.Vitality:
                            itemValue = cItem.AcdItem.Stats.Vitality;
                            ruleValue = itemRule.Value;
                            result = itemValue >= ruleValue;
                            break;

                        case ItemProperty.FireSkills:
                            itemValue = cItem.AcdItem.Stats.FireSkillDamagePercentBonus;
                            ruleValue = itemRule.Value;
                            result = itemValue >= ruleValue;
                            break;

                        case ItemProperty.ColdSkills:
                            itemValue = cItem.AcdItem.Stats.ColdSkillDamagePercentBonus;
                            ruleValue = itemRule.Value;
                            result = itemValue >= ruleValue;
                            break;

                        case ItemProperty.LightningSkills:
                            itemValue = cItem.AcdItem.Stats.LightningSkillDamagePercentBonus;
                            ruleValue = itemRule.Value;
                            result = itemValue >= ruleValue;
                            break;

                        case ItemProperty.ArcaneSkills:
                            itemValue = cItem.AcdItem.Stats.ArcaneSkillDamagePercentBonus;
                            ruleValue = itemRule.Value;
                            result = itemValue >= ruleValue;
                            break;

                        case ItemProperty.HolySkills:
                            itemValue = cItem.AcdItem.Stats.HolySkillDamagePercentBonus;
                            ruleValue = itemRule.Value;
                            result = itemValue >= ruleValue;
                            break;

                        case ItemProperty.PoisonSkills:
                            itemValue = cItem.AcdItem.Stats.PosionSkillDamagePercentBonus;
                            ruleValue = itemRule.Value;
                            result = itemValue >= ruleValue;
                            break;

                        case ItemProperty.PhysicalSkills:
                            itemValue = cItem.AcdItem.Stats.PhysicalSkillDamagePercentBonus;
                            ruleValue = itemRule.Value;
                            result = itemValue >= ruleValue;
                            break;

                        case ItemProperty.DamageAgainstElites:
                            itemValue = Math.Round(cItem.AcdItem.Stats.DamagePercentBonusVsElites, MidpointRounding.AwayFromZero);
                            ruleValue = itemRule.Value;
                            result = itemValue >= ruleValue;
                            break;

                        case ItemProperty.DamageFromElites:
                            itemValue = Math.Round(cItem.AcdItem.Stats.DamagePercentReductionFromElites, MidpointRounding.AwayFromZero);
                            ruleValue = itemRule.Value;
                            result = itemValue >= ruleValue;
                            break;

                        case ItemProperty.BaseMaxDamage:
                            itemValue = ItemDataUtils.GetMaxBaseDamage(cItem.AcdItem);
                            ruleValue = itemRule.Value;
                            result = itemValue >= ruleValue;
                            break;

                        case ItemProperty.SkillDamage:

                            var skillId = itemRule.Variant;
                            var skill = ItemDataUtils.GetSkillsForItemType(cItem.TrinityItemType, Trinity.Player.ActorClass).FirstOrDefault(s => s.Id == skillId);                            
                            if (skill != null)
                            {
                                friendlyVariant = skill.Name;
                                itemValue = cItem.AcdItem.SkillDamagePercent(skill.SNOPower);
                            }
                                                            
                            ruleValue = itemRule.Value;
                            result = itemValue >= ruleValue;
                            break;

                        case ItemProperty.ElementalDamage:

                            var elementId = itemRule.Variant;
                            var element = (Element)elementId;
                            if (element != Element.Unknown)
                            {
                                friendlyVariant = ((EnumValue<Element>) element).Name;
                                itemValue = Math.Round(ItemDataUtils.GetElementalDamage(cItem.AcdItem, element), MidpointRounding.AwayFromZero);
                            }

                            ruleValue = itemRule.Value;
                            result = itemValue >= ruleValue;
                            break;

                        case ItemProperty.PercentDamage:
                            itemValue = cItem.AcdItem.WeaponDamagePercent();
                            ruleValue = itemRule.Value;
                            result = itemValue >= ruleValue;
                            break;

                    }

                    Logger.LogDebug("  >>  Evaluated {0} -- {1} {5} (Item: {2} -v- Rule: {3}) = {4}", 
                        cItem.RealName, 
                        itemRule.ItemProperty.ToString().AddSpacesToSentence(), 
                        itemValue,
                        ruleValue,
                        result,
                        friendlyVariant);

                    if (!result)
                        return false;
                }
                return true;
            }

            Logger.LogDebug("  >>  Unselected ListItem {0} {1}", cItem.RealName, cItem.AcdItem.ActorSNO);

            return false;
        }

    }
}

