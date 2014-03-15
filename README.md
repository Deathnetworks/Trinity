
# Trinity

### Changelog 1.8.6

* Monk Mantra of Healing now used when low on health

* Simpler monk Manta spam logic

* Fixed Wizard Storm Armor casting every 15 seconds

* TrinityTownRun added 2 attributes. Usage: <TrinityTownRun questId="1" minFreeBagSlots="10" minDurabilityPercent="50" /> - makes it easy to put at the start o

* Added gems to item reports

* Fixed IndexOutOfRange exception for Diamonds

* Diamonds are now picked up according to the UI options (instead of Amethyst+Diamonds)

* ItemRules now only loaded when used

* Added a bunch of useful methods to TargetUtil

* Barbarian Furious Charge and Leap should work much better and more often

* Barbarians now have an option to choose when to sprint (Always, Combat, or Movement)

* Wizard will now use Arcane Torrent or Blizzard for destructible objects

* Removed Old Tristram Road and The Old Ruins from QuestLevelAreaId's

### Changelog 1.8.5

* Added option to hide BattleTag from window title

* Barbarian Call of the Ancients will now be used on trash (3 minimum) instead of only elites

* DemonHunter Rain of Vengeance now works with Ignoring Elites

* DemonHunter Companion is now used when off cooldown and valid target(s) are in range

* WitchDoctor Spirit Barrage is now used (not just Manitou rune)

* Elites should now get priority when in Archon form (and not ignoring elites)

* Added Scarecrow to Container Whitelist

* Legendary Follower items will now always get picked up

* Wizard Archon Blast, Arcane Strike and Disintegrate work correctly now

* Monk Mantra Spam will now only be cast when elites are present, or when ignoring elites when trash pack settings are met

* Monk Inner Sanctuary now triggers when any elite is in range, or 3+ trash mobs is in range (removed Spirit requirement)

* Fixed error/bug in missing/incorrect DemonHunter TVar

* Monk Exploding Palm is now used every 7 seconds (from 15)

* Stashing Whites option now also stashes inferior (gray) items

* Fixed always picking up white/gray items

### Changelog 1.8.4

* Ignore elites should work much better

* Fixed casting magic weapon every 10 seconds with archon in hotbar

* Fixed stashing blues when we should have been stashing whites only

* Fixed not using shines/wells/pools with DB Beta 374+

* Added Force Salvage all Rares option

* Trinity will now perform a town run when in town, and has 6 or less free bag slots remaining, or average item durability is less than 50%

* Changed default BeesWasps Aoe to 6 radius / 25% health

* Ignore elites should work much better

* Trinity will now perform a town run when in town, and has 6 or less free bag slots remaining, or average item durability is less than 50%

* Fixed casting magic weapon every 10 seconds with Archon in hotbar

* Fixed stashing blues when we should have been stashing whites only

* Added Force Salvage all Rares




### Changelog 1.8.3

* Fixed Monk refreshing sweeping wind while casting TownPortal

* Added Wizard option for Archon with Elites only (still requires buffs up if on hotbar)

* Fixed (Suppressed to debug) common Memory Reading errors

* Added OverPower as a Barbarian Destructible Power

* Dashing strike can now be used for OOC movement

* Fixed Trinity not reading new hotbar skills when changed

* Fixed the 10-20 second stuck when PowerGlobes are dropped

* Removed electrified affix avoidance bug

* Fixed monk Mystic Ally TVar error

* Fixed startup crash on missing ItemRules directory/files

* Fixed TrinityTownPortal causing lots of lag in town

* Added support for identifying Summoner units (will eventually get higher priority in another release)

* Modified FrozenPulse defaults for all classes to 5 radius / 25% health

* Modified ThunderStorm defaults for all classes to 5 radius / 50% health

### Changelog 1.8.2

#### Items and Looting

* Fixed not picking up legendary/set plans with "Legendary Plans" checkbox checked

* Added option for Inspecting Corpses

* Fixed not picking up PowerGlobes

* Added new Container/Chest GUI options

* Added options to pickup white and gray items

* Added White Item Salvage Options

* Fixed not opening demonic vessels

* (Hopefully) fixed missing legendaries - related to Loot 2.0/X1 item names.

* Trinity now picks up diamonds

#### Combat

* Fixed hero attacking friendly summoned minions (from items and such)

* Fixed not casting WOTB on single elites

* Set default Wizard Arcane Orb time to spam (from 1/2 second)

* Default BarbHOTA changed to 45 fury (up from 20)

* Fixed Health Wells

* Added mortar avoidance defaults

* Fixed using potions

* Added Avoidance for FrozenPulse

* Changed default potion levels for all classes to 35% (potions restore a constant 60% health)

* Fixed Monk Mantra Passive checks

* Added support for new Weapon Throw and Ancient Spear

* Added AoE logic for Wormhole, Thunderstorm, Poison Enchanted, Pulse, Orbiter

#### General/Technical

* Added TrinitySetQuesting tag to disable Trashpack settings for Questing

* Fixed IndexOutOfRangeException in ItemDropStats

* Completely rebuilt chest/container cache logic

* Add Equipped/Backpack/Ground/Stash Item Attribute/Property debug dump buttons to Items/Scoring page

* Removed potion types from UI, added more potion stack options

* Healthglobes should now be picked up even if we're trying to Town Portal

* Fixed Exception/errors when refreshing Gizmo/Doors

* Added Trinity Debug Logger

* Reworked Gizmo identification for RoS

* Fixed warnings, .313 GameDifficulty error

### Changelog 1.8.0

* Removed force local pathfinding, DB now always has local pathfinding

* Added Litany/Wailing host rings summoned skeleton to blacklist

* Increase Max Trash Combat Range to 200 in UI

* Using intell's persistent stats

* Forcetownrun with Minimum free bag slots now only triggered in town (intell)

* Using intell's Barbarian TVars

* Ignore Two-Hand Pickup now only ignores Rare and lesser

* Fixed TrinityExploreDungeon to ACTUALLY disable SetNodesExploredAutomatically

* Changed default behavior of TrinityExploreDungeon to automatically disable new minimap explored logic

* Bombastic's "-1 or Invalid" Trinity logs fix

* Added debug logging for Units that somehow get casted to Gizmo's

* Fixed IgnoreScenes broken in TrinityExploreDungeon due to new caching system

* Added fail-safe checks to TrinityMoveToScene

* Fixed TrinityTownPortal only working *once* :)

* Cache: always refresh units in Quest areas

* Cache: Moved blacklist check to after Object type check

* DemonHunter: Added MarkedForDeath to tracked spells

* Fixed bot TPS default

* Fixed exception in TrinityMoveToScene where no scene name was specified

* Added a few more QuestLevelAreaId's

* TrinityExploreDungeon will now investigate an actor if the actorId is defined and EndType=ExitFound

* Added Conversation Close button to UI Element click checks

* Added ItemQualityLevelIdentified to ItemsDropped.log

* Added PriorityScene as an alias to PrioritizeScene TrinityDungeonExplorer PrioritizeScenes element

* Converted a bunch of stuff to be Async for performance.

* UI XAML is now loaded Asynchronously when Plugin is enabled.

* Added potential safety checks for new game reset if we get disconnected during a town run

* Changed "Pickup Two Handed Weapons" to "Ignore Two Handed Weapons"

* Fixed a few NullReferenceExceptions

* Refactored GameUI a little more

* Fixed not picking up 2 hand weapons

* Optimizations for HotbarSkills / Hotbar

* Optimizations for UI Elements

* Removed Bot TPS (now set to Maximum)

* Optimizations for TrinityExploreDungeon

* DemonHunter: Fixed Chakram: Shuriken Cloud

* DemonHunter: Set default kite distance to 0

### Changelog 1.7.3.10

* Barbarian: Fixed Fury Dump with Battle Rage

### Changelog 1.7.3.9

#### WitchDoctor

* WitchDoctor: VisionQuest passive support works better now.

* WitchDoctor: Fixed breaking bug/typo with Spirit Barrage: Well of Souls

* WitchDoctor now supports the Vision Quest passive

* WitchDoctor: Fixed Spirit Walk: Honored Guest not ever working

* WitchDoctor now supports Sacrifice: Circle of Life rune

* WitchDoctor: Firebats no longer used for destructible objects

#### Monk

* Modified Monk Exploding palm, should use more frequently now

#### General

* Added TrinityCombatIgnore tag

* Added Option in Items > Scoring/TownRun for "Free Bag Slots for TownRun". When the number of empty bag slots is less than or equal to this slider, the bot will force a town run.

* *may* have fixed the forever-present problem where bot keeps running even though backpack is full

* Modified Units Weight/Priorities

* Added "Item Pickup" options for Ignoring Legendaries, Non Legendaries, and Gold near AoE and near Elites (6 options)

* Added Item Pickup option to ignore Two Handed weapons (overrides ItemRules)

* Added Health Well Minimum Health slider ("World Objects" tab)

* Chests/Containers should no longer be Kamikaze mode

* Fixed trash mob targeting when elite units are near it & we're ignoring elites

* TrinityTownPortal will now move a little when stuck casting town portal

#### Technical

* Added caching for BattleTagName. Should no longer cause the bot to derp out with the new DB Beta.

* Added "IsStandingInAvoidance" property for TrinityCacheObject.

* Fixed Out of thread "save settings" throwing ReadProcess/WriteProcess error for DB Beta compatibility

* Refactored TrinityUseOnce a little

* Started work on WitchDoctor Health/Mana Sliders for healing journey/honored guest

* Routine check now does async bot stop/restart

### TrinityCombatIgnore tag:

Will cause the bot to ignore combat with the given unit. Is only overridden when attempting to town portal.

##### Attributes:

actorId is the "ActorSNO" of the unit. You can get this from the info dumping tab > click "Dump Objects"

name is optional - just the name of the unit for readability purposes only

excludeElites: When set to "True", will cause the bot to attack elite units of this type.

ExcludeTrash: When set to "True", will cause the bot to attack trash mob units of this type.

Example to ignore a unit type, except elites of that unit type:

    <TrinityCombatIgnore actorId="4157" name="FleshPitFlyer_B" exceptElites="True" exceptTrash="False" />

Example to ignore a unit type, including elites and trash:

    <TrinityCombatIgnore actorId="4157" name="FleshPitFlyer_B" exceptElites="False" exceptTrash="False" />

Example to ignore a unit type, except trash mobs of that unit type:

    <TrinityCombatIgnore actorId="4157" name="FleshPitFlyer_B" exceptElites="False" exceptTrash="True" />

Setting both excludeElites and excludeTrash to True will effectively *do nothing* - the bot will still attack all units of this type.


### Changelog 1.7.3.8

#### Combat:

* WitchDoctor: WitchDoctor is a little better at maintaining firebats channeling now

* WitchDoctor: Upgraded WitchDoctor to use CombatBase.CanCast... hopefully helps fix the "WD derping"!

* WitchDoctor: WitchDoctor: Added Spirit Barage/Rush of Essence

* Barbarian: Added GUI options for Use Leap, Sprint, Furious Charge OOC

* Barbarian: Fixed erroneous Barbarian messages about Call of the Ancients and Earthquake

#### General: 

* (Hopefully) Fixed TownRun / Notifications not being sent

* Fixed Trash Kill Radius not actually working

* Removed all Framelocks for DB Beta

* Added additional checks for HealthWells. UseShrines GUI option now applies to Health Wells too.

* Fixed picking up low level items with ItemRules enabled (now overrides ItemRules if enabled)

* Disabled SetWindowTitle()

* Fixed HotSpots / SimpleFollow causing weird behavior


### Changelog 1.7.3.7

* Monk: DualGen/TriGen + Exploding Palm builds now work 

* Barbarian: Rend no longer cast out of range 

* Added avoidance radius/health sliders for Grotesque 

* Added "Always kill elites below % health" slider (thanks to zorked for the idea) 

* Barbarian: Rend tweaked - now always forces minimum delay of TVar value (default: 1000msec) 

* Monk: Fixed sweeping wind stuck stuff 

* WitchDoctor: Soul Harvest now used based on current buff stack 

* WitchDoctor: Fixed wrong TargetUtil function 

* Reduced Weighting multiplier for monster custom weights 

* Cleaned up townrun a bit from old code 

* Fixed settings not being loaded 

* Fixed exception in GroupHotSpot 

* Fixed crash in TVars 

* Fixed Grotesque avoidance/health to AvoidanceManager bug 

* Fixed IsInTown to emergency durability leave game bug 

* Adjusted Weight/Priority down on interactables 

### Changelog 1.7.3.6

#### Combat

* Barbarian: Whirlwind will now path into health globes if needed

* Barbarian: Added WOTB Poison Tree (100%) and Belial (100%) AoE health (non) modifications & variables.

* Barbarian: HOTA on Elites is now used only if there's not 3 trash mobs in our immediate melee range

* Barbarian: Tuned up Barbarian Rend Parameters, should now use more liberally

* Barbarian: Added Poison Spore, Belial and Beast Charge WOTB Avoidance Helth Modifiers

* Monk: Fixed Sweeping Wind not casting on trash mobs

* DemonHunter: Reorder spell priorities (Sentry Turrent + Caltrops have higher priority)

* DemonHunter: Now using dogmeat's use ranges and mob counts (Strafe, Grenades,etc)

* DemonHunter: Added Preparation Punishment rune support

* DemonHunter: Combat Vault now uses an intelligent "Kite" point instead of just "backwards"

* DemonHunter: Upgraded spell checks to use CombatBase.CanCast and TargetUtil, and removed redundant logic checks

* DemonHunter: ShadowPower is now used on low health (50%)

* Combat > Misc: Added Options for Ignore Trash  Mob below Health Percent (15% default) and Ignore Trash Below Health Percent with DoT applied (50%). Credits to tesslerc for the idea.

* BIG re-working of weights. Spent a few hours looking at weights and priorities of the bot for units.

* Fixed attacking Shielded units when we shouldn't

* Trinity should be better about not trying to attack monsters behind other monsters (AoE/Cluster spells not included)

#### Avoidance

* Added experimental fire chains avoidance; check Combat > Misc for enable/disable

* Added experimental Savage Beast charger avoidance; check Combat > Misc for enable/disable

#### Misc

* Trinity Variables Window is now sorted automatically

* Added emergency leave game on 0% item durability (for when when we keep spawning and dying repeatedly)

* Refactored all "AnythingWithinRange" and "ElitesWithinRange" to use TargetUtil methods

* Added AnyTrashInRange function to TargetUtil

* Fixed crash on "Test Scoring" button and to actually produce ItemRules results

* Reloading item rules and sorting the stash should no longer crash Demonbuddy

### Changelog 1.7.3.5

#### Barbarian

* Barbarian: Now using dogmeat's Spell Priorities

* Barbarian: Added force WOTB on Emergency Health (50%) (Adjustable via Trinity Variables window)

* Barbarian: Added Radio button options for WOTB: Hard Elites Only | Normal | Pop when not on CD

* Barbarian: Added Rend spam with Bloodlust rune when < 50% health (Adjustable via Trinity Variables window)

* Barbarian: Added Sprint MinUseDelay TVar

* Fixed Barbarian Avoidance GUI

#### Monk

* Monk: Added Spam Sweeping Winds on Low HP Option in Monk GUI

#### WitchDoctor

* WitchDoctor: Added Spirit Walk: Honored Guest and Healing Journey rune Support

* WitchDoctor: Will now attempt to auto-detect if we're using 0-dogs (e.g. if there is no other "primary attack" present)

#### Wizard

#### DemonHunter

* DemonHunter: Added Chakram: Shuriken Cloud support

* DemonHunter: ShadowPower is no longer cast when we already have the buff

#### General

* Trinity now supports the <TrinityTownPortal /> tag. Simply replace any existing "UseTownPortal" with "TrinityTownPortal" to use this functionality. TrinityTownPortal will automatically clear the area of all mobs before using Town Portal.

* Trinity now has SimpleFollow Integration using a "Hot Spot" system, so that the leaders and followers should no longer get seperated if one or the other remains in combat.

* Added Grotesque "Stitcher Suicide Bomb" AoE avoidance

* Added field set/group boxes to Combat.Misc GUI

* Removed combat radius reading from Default DB settings. Combat radius logic is now always used from Trinity Combat misc settings.

#### Technical

* Added Unit Animation Avoidance Logic, with new AnimationAvoidance DataDictionary Property

* Added option to disable avoidance pathing - maybe help with OutOfMemoryException's

* Added maximum 30f cap to avoidance SNO's, in case there's a random large radius sneaking into the avoidance manager - maybe help with OutOfMemoryException's

* Moved OnPulse UI button click event after safety checks

* Added Memory.AcquireFrame() around Trinity.OnPulse()

* Remove and sort all usings

* Disabled cache double check guid logic

* Increased default large destructible object minimum distance from 2f to 5.9f

### Changelog 1.7.3.4

#### Barbarian

* Barbarian: Fixed Threating Shout so it casts as expected in combat

* Barbarian: Added Treasure Goblins as 'elites' to HOTA check (for HOTA/WW builds)

* Barbarian: Fixed derp Whirlwind TVar name bug

* Small tweaks to Barbarian Combat spell checks to make sure we can cast them

* Barbarian: Added logic to Elite/HOTA when Ignoring Elites (will now use on stacked and "large" monsters, for HOTA/WW builds)

* Barbarian: Added slight delay for Sprint when dumping fury, fixes "fury dump stuck"

* Barbarian: Added Min Health setting for HOTA when using with Whirlwind

* Barbarian: Added Support for Bash: Punish rune

* Barbarian: Fixed IgnoreElites randomly triggering WOTB

* Barbarian: Added check for EliteRareUnique when checking for "Hard" monster affixes for WOTB

#### Witch Doctor

* WitchDoctor: Optimized/updated 0dogs a bit

* WitchDoctor: Hex: Angry Chicken works again

* WitchDoctor: Added rune support to horrify

#### Wizard

* Wizard: Fixed Cancel and rebuff Archon with Magic Weapon / Familiar

#### General

* Added Combat logic and GUI option to Ignore Elites when we have 5 NV Stacks

* Added Carguy's Pushover.net notifications

* Trinity should now instantly click Conversation buttons, and Boss portal 'Accept' button

* Added Global configuration file option under Advanced tab - will force all bots running under a shared Demonbuddy to use the same configuration file.

* Removed "has been primary target" check for trash mob ignore

* Removed avoidance count check for ignoring trash mobs

* Finally fixed re-using old shrines (GizmoState Attribute)

* Refactored Health Globes a little for smarter weighting

* Health Globes now have weight even when current health is less than "emergency health globe" health.

* Health Globes given more weight when players in party are low on health.

* Updated UI Combat.Misc.AllowOOCMovement checkbox label

* Fixed combat and avoidance for Belial and Azmodan

* Fixed a few Quests in Act 1 where bot was ignoring hostile quest NPC's

* Tweaked "Champion Hunting" Item pickup defaults: Now uses "Hard" item rules, no gems, no junk yellows, 1500 gold stack minimum


#### Technical

* Added Pre-Townrun max distance check

* Fixed crash condition for Generic Cache manager

* Added TVar's for low priority health globes

* Added TVar invalid key logging to detect bugs

* Added IsDead check to TownRun to avoid noisy logs

* Added buff logging

* Added hotbar logging

* Added SpellHistory class, so we can track every spell used and when

* Added StraightLinePathing LevelAreaId's (fix for Belial navigation)

* Added QuestLevelAreaId's HashSet and logic for better questing support - disables ignoring trash mobs in these LevelAreaId's


### Changelog 1.7.3.3

* Added SpellTracker class - this can be used to track timed spells (like DoT's) without having to use DOTDPS Actor Attribute. This as added as the WitchDoctor

* WitchDoctor Haunt is no longer spammed, but used after the duration of the DoT spell has expired

* Cleaned up ItemRules Identification logging

* Fixed incorrect reference to Target ACDGuid (Was RActorGUID)

* Removed Demonbuddy Version check

* Barbarian will now use HOTA on elites when available

* PreTownRunPosition now checks to make sure we're not in town

### Changelog 1.7.3.2

* Removed 2f distance check for ignoring trash mobs

* Fixed WOTB Fury Dump

* Added Barbarian options for Ignore Avoidance in WOTB, and Ignore Gold stacks in WOTB

### Changelog 1.7.3.1

* Fixed all references to AbilityLastUsedCache (Monk combat works again)

* Fixed picking up white items

### Changelog 1.7.3.0 - FRESH INSTALL REQUIRED:

Highlights:

* New feature: Trinity Variables. This allows you to control many internal numbers used inside Trinity. Currently implimented for Barbarian Combat and general spell delays only. More variables will be implimented in future versions.

* Implimented new BarbarianCombat and CombatBase classes for (hopefully) easier editing and customization.

* After a town run, Trinity will now return to the location where the bot requested a town run (bags full, repairs needed). This will help make sure no loot is missed if the bot is distracted while bags are full.

* Bot will now path around avoidance.

* ItemRules now supports Unidentify, Salvage rules

Barbarian: 

* Barbarian will no longer senselessly WW to health globes out of combat

* Fixed Barbarian logic not correctly waiting for WOTB

* Barbarian will now wait for BattleRage before using WOTB or Overpower.

* Tightened up Whirlwind a bit, works better on single targets and units that are spread too far apart

* Tuned Sprint to work better with Whirlwind

Witch Doctor:

* Added WitchDoctor Gruesome Feast passive support for health globes when on full health and low mana

* Added WD GUI slider for WD Firebats range setting

* Improved support for WD Firebats, added support for Cloud of Bats rune

* Moved Spirit Barage priority lower than acid cloud

* Increased Firebats priority higher than acid cloud

Wizard:

* Fixed Wizard Blizzard (Cluster on trash was not working)

* Re-added Familiar to Archon pre-buffs

* Added enhanced support and tuned Ray Of Frost : Sleet Storm for super-melee mode

Profile Tags:

* Fixed missing lastMoveResult stuff in TrinityMoveTo

* Fixed TrinityMoveTo to finish when the Navigator fails or finishes (rather than a nice ugly perma-stuck!)

* Fixed bug with TrinityLoadOnce showing more profiles completed than available

* Added SimpleFollow profile compatability to TrinityExploreDungeon

* Fixed bug in TrinityOffsetMove only being used once (lastMoveResult was never reset)

* Refactored TrinityUseOnce a little

General:

* Trinity will now automatically install the latest Combat routine 

* Trinity will now automatically select the Trinity Combat routine if it's not selected

* Removed A2 birds from blacklist

* Disabled LoS/Navigator raycast checks (force use navigator now) - will fix pathing through stuff we can't path through.

* Monsters near other players will now receive a higher targetting weight

* Monsters now get weight when near other players

* Added API'able IsCombatAllowed - for later use maybe in SimpleFollow

* Fixed ignoring and Increased resplendent chest distance, Added additional Resplendent chest SNO's

* Should no longer attempt to run through monsters to get to a legendary (and die)

* Fixed interacting with pretty much all Gizmos...

* Fixed blacklist flipflop for elites with low health

* Bot will now always attack monsters regardless if they are standing in plague or not (overrides "Attack monsters in AoE" setting)

* FindSafeZone (Avoidance) takes into better consideration where monsters are standing (won't stand next to them and get beat on all day)

* Fixed Plugin/Routine checks. Trinity will no longer allow a user to start the bot with Trinity enabled and the wrong routine selected.

* Fixed "ignore avoidance" adding avoidance SNO's as Units

Technical/Developer:

* Removed ALL references to "Giles" except for Plugin Credits and Settings Migration

* Renamed Namespace and GodClass to Trinity

* Refactored all static dictionaries into DataDictionary class

* Reorganized all directories

* Removed old commented code in various parts.

* Re-added UseNavMeshTargeting UI option for testing purposes

* Removed all unnecessary references and usage of MainGridProvider

* Added Settings Migration capability (now uses Trinity.xml, will automatically read, migrate, and delete old file). New file does not use Namespaces for easier editing/parsing.

* PersistentStats will now automatically reset/delete itself if it encounters an exception

* Fixed duplicate expirecache key exception

* Renamed Settings namespace to Config (was conflicting between Trinity.Settings property and Trinity.Settings namespace)

* Added Lots of new TrinityPower constructors for various purposes

* Added Lots of utility methods in Combat Base

* Rebuilt Targeting Provider to actually give the DiaObject of our Current Target

* Added exception handling around Item Rules Reload (for catching FileNotFound and IO exceptions and such)

* Fixed exceptions when attempting to load missing identify/salvage itemrules .dis files



### Changelog 1.7.2.13:

* Added FireWorldTransferStart() to TrinityInteract - this wil help prevent TrinityInteract from crashing DB. We need to depreciate this tag...

* Added FireItemLooted event, Demonbuddy Item Stats in UI now shows items looted counters when using Trinity

* ItemRules.log archive into one file now, better to use for itemViewer

* Corrected default Cache Refresh Rate in UI

* Completely disabled Avoidance cancellation timers ("fixes" avoidance, but maybe it breaks it somewhere else.. )

* Disabled ClientNav LoS check (Performance per Nesox)

* Changed WD Sacrifice range to 9f

* Added additional OnStart/Nav logic to TrinityMoveTo and TrinityInteract

* Fixed exceptions flushing Persistent Stats streams

* Added UseProfileManager class - helper class to record and manage profiles and profile blacklists

* Moving a few "maintenance" tasks (UsedProfileManager.RecordProfile and output item stats report) into Plugin Pulse

* Fixed Demonhunter Sentry (I think) [herbfunk/Nazair]

* Added additional safety check on Plugin Pulse

* Added try/catch around Test Scoring and Order Stash commands, hopefully prevents DB from crashing

* Fixed incorrect ZetaDia.Actors.Me references (should be ZetaDia.Me)

* Added un-openable FoS gate to blacklist

### Changelog 1.7.2.12:

* Tuned combat for WD Zero Dogs

* Fixed bot start non-Trinity routine warning

* Fixed LoS exception bug

* Added a few more boss SNO's

* Added a shit ton of safety checks to help prevent crashes

* Hopefully fixed Barbarian Bash (e.g. Non-WW) specific targets

* Added Zero Dogs GUI option/logic for WD

* Small tweaks for Barbarian IsWaitingForSpecial + WW

* Fixed error in removing object from generic cache

* Added N00b check for running Trinity with any other non-trinity combat routine


### Changelog 1.7.2.11:

* Added support for Barbarian Maniac Rune

* Updated Barbarian "Use WOTB on hard elites only" tooltip, to include the affixes which are used

* Added Minimum Threating Shout Mob Count to Barbarian settings

* Barbarian Whirlwind will now be used for OOC movement if there are any non-primary target units within whirlwind range (~10 yards)

* Added GUI option for using Barbarian Threating Shout OOC

* Added support for Monk Dashing Strike: Quicksilver rune

* Added GUI option for for force casting Monk Sweeping Wind before using Wave of Light

* Added Monk Breath of Heaven:Infused With Light + Way of the Hundred Fists:Fists of Fury support

* Monk Cyclone strike will no longer be spammed on single-target elites when other abilities can be used

* Increased Monk Wave of Light priority

* Fixed 12 yard radius distance LoS bug (now is 5 yard from CenterDistance force LoS)

* Added extra navigation obstacle SNOId for Demonic Forges - thanks Gardettos for the find

* Changed XAML ui loader encoding to UTF8 to support chinese clients translate

* Fixed persistent stats percentage output format (now only 2 decimal places)

* Ignore gold/non-leg items near Elites actually works now

* Unstuck checker moved to 3sec since it works much better recently

* Added Elite Kill range GUI slider

* Added Force Ignore Destructible GUI option

* Handle target logic will now correctly return TreeRunning after pickup up an item (should help with picking up items with a UseTownPortal tag)

* Updated Ignore trash mob logic to account for IgnoreElites settings

* Added IgnoreLastNodes / MinVisistedNodes to TrinityExploreDungeon, for use with until="FullyExplored", allows Trinity to end the tag prematurely to reduce/eliminate the backtracking at the end of exploring large dungeons (like keeps 2)

##### Notes on minVisistedNodes:
Used with ignoreLastNodes, minimum visited node count before tag can end.   
The minVisistedNodes is purely, and only for use with ignoreLastNodes and with until="FullyExplored" - it does not serve any other function like you expect.   
The reason this attribute exists, is to prevent prematurely exiting the dungeon exploration when used with ignoreLastNodes.   
For example, when the bot first starts exploring an area, it needs to navigate a few dungeon nodes first before other dungeon nodes even appear - otherwise with ignoreLastNodes > 2,   
the bot would immediately exit from navigation without exploring anything at all.  

Example Keeps 2 tag:
      <TrinityExploreDungeon questId="101758" stepId="1" 
        until="FullyExplored" boxSize="80" boxTolerance="0.01" 
        timeoutType="GoldInactivity" townPortalOnTimeout="True" timeoutValue="60" 
        ignoreMarkers="True" ignoreLastNodes="3" minVisitedNodes="10">
        <IgnoreScenes>
          <IgnoreScene sceneName="_N_" />
          <IgnoreScene sceneName="_S_" />
          <IgnoreScene sceneName="_E_" />
          <IgnoreScene sceneName="_W_" />
        </IgnoreScenes>
      </TrinityExploreDungeon>




### Changelog 1.7.2.10:

* Fixed CPU utilization increase bug, due to monster TeamID cache

* Added GUI Combat>Misc option to Attack monsters standing in AoE

* Added GUI Combat>Misc option to ignore all elite/rare/unique monsters

* Fixed Monk WoL spirit, was attempting to use at 70 spirit instead of 75

### Changelog 1.7.2.9:

* More fixes for not-attacking stuff after a single attack is used

* Removed UI checkboxes for WD Grave Injustice and Barb Boon of Bull Kathos, as the passives are now auto-detected

* Added Witch Doctor Hex: Angry Chicken support

* Added improved support for WD Grave Injustice with acid cloud and zombie bears, taking into account pickup radius

* Wizard Blizzard is now cast on the best cluster area instead of the primary target

* Added more logging for main HandleTarget for debugging CM/WW problems...

* Modified unstucker weighting for current destination position

* Refactored/Moved UpdateCachedPlayerData, RefreshBuffs, RefreshHotbar methods into PlayerCache class

### Changelog 1.7.2.8:

* Fixed rogue IsWaitingForPower in Refresh HealthWell section, causing stucks

* Removed SNOPower.Walk from Wizard ET/Blizzard spells

* Tweaked RandomXY ZigZag from 2/3+1+5/4 to 1/2+3/4+1 distance

* Destructible radius is now always forced to 50%, re-purposed dictSNOExtendedDestructRange to be used as a range when object should be 'added' to cache instea

* More tweaks for destructible object range and detection

### Changelog 1.7.2.7:

Combat logic:

* Very-large destructibles (e.g. that huge catapult in skycrown) will now be always attacked

* Re-worked trash/elite/gold weighting/prioritization a little to better pick targets

* Bot should be better about not attacking units that are standing directly in avoidance

* Tweaked single target zig-zag a little, changed multi-target zig zag to use clusters

* Trinity behavior tree will now wait for power/spell use based on time rather than ticks. This will help reduce CPU utilization slightly and strange behavior when using extremely low (<6) or high (>20) tick rates.

* Close Target prioritization now takes effect when 3 or more units are within close melee range (5f)

* Kiting will now only kite monsters who are weighted (e.g. valid targets)

* Fixed default ranged weapon attack distance

* Improved Kiting logic some, should suck less

* Increased Ignore Trash Mob max count UI slider to 15

* Tweaked ignore trash mob logic slightly - should be better about REALLY ignoring trash mobs now, added some logging for helping me debug

Combat classes:

* Monk Sweeping Wind spell will no longer interrupt Tempest Rush channeling

* Monk Wave of Light trash mob max count UI slider increased to 15

* Monk Tempest Rush targetting optimized a little more Optimized - works better now except when navigating tight corners and with destructibles/barricades

* Fixed monk TR Movement+TrashOnly bug being used on Elites

* Fixed Monk Wave of Light spirit check (was attempting to cast even without enough spirit)

* Destructibles are now ignored if monk is channling Tempest Rush or has Sweeping Wind buff 

* Barbarian Rend is now used more when current target is not bleeding or 2 or more nearby targets are not bleeding - and fixed use timer bug

* Barbarian Whirlwind and Tempest Rush ZigZag for single/sparse targets has been updated, it will now criss/cross the target better

* WitchDoctor Manitou rune is now used correctly OOC (only when Manitou is not present), may help with other OOC buffs as well

* WitchDoctor Locust Swarm is now used correctly (only when current target is not affected)

* WitchDoctor Soul Harvest + VengeFul spirit works better

* WitchDoctor Vengeful Spirit rune is now supported and has special logic

* DemonHunter Range weapon default attacks should work correctly now

Items and Looting:

* Items should now be picked up when the current profile behavior is a UseTownPortal tag

* Fixed bug standing around trying to pickup gold that we were already standing on

* Fixed bot acting confused when dropped items are sitting in AoE range

* Added potential fix for Demonic Forges and other navigation obstacles... needs testing

Core Logic:

* Added some optimizations and caching where appropriate for TeamID, IsBurrowed, IsUntargettable, IsInvulnerable - should help reduce CPU utilization slightly

* Adjusted defaults for Gold Inactivity (300) TPS Limit when modified (8) and Cache Refresh Rate (300)

* TownRunTimer will now keep BehaviorTree running while waiting (should no longer continue running profile when bags are full)

* Added client navigation check if we can actually move to attack a unit/pickup an item (replaces GridProvider.CanStandAt(Point v2))

* Fixed GoldInactivity not triggering while Behavior tree is in running state for extended periods of time

* Fixed Azmodan avoidance, it actuall works now

* Set default SuccubusStar avoidance health slider to 0 (disabled)

* Added TrinityTownRun to IsTryingToTownPortal() safety checks

* Added AWE's fix for TrinityLoadOnce random profile selector

* Added AWE's TrinityLoadOnce multi-profile stuff

* Tweaked destructible AddToCache logic slightly to be less sensitive for smaller objects

* Lots of refactoring - moved ItemValuation and Constants into their own classes, created NavHelper/MathUtil, Improved TargetUtil, rebuilt several functions and reorganized a lot of old code

* Trinity will now force-reload the profile OnGameChanged since Demonbuddy forgets to do this sometimes (may cause double-reloads, but that's OK)

* Fixed bug with persistent stats XML data not being flushed completely and causing XML document errors in logs

* Improved Unit attribute check order in hopes to reduce CPU utilization

* Temporary caches are now reset on new games only, rather than periodically (was 5/10/30/150 seconds), should help reduce CPU utilization

* Fixed RunStats Item Stats (IPH) counter / GenericObjectCache

* Monsters are now always added to the cache regardless of kill radius, but are now ignored from weighting based on kill radius (so they can be included in TargetUtil checks)

* Started work on using only cached values and minimizing reads from DB/D3 memory - focus on ZeteDia and ZetaDia.Me

### Changelog 1.7.2.6:

* Fixed Avoidance ... yes... all avoidance :) It actually works now. Molten cores and Arcane sentries and everything.. 

* Disabled ignoring trash mobs while avoidance is nearby

* Monk Wave of Light is now used more liberally

* Added GUI options for Minimum trash mob counts for Monk Wave of Light and Cyclone Strike

* Optimized Monk Cyclone Strike timing and monster clustering

* Tempest Rush should no longer be maintained with UseObject/UsePortal/UseWaypoint/UseTownPortal tags

* Tempest Rush is now maintained when picking up gold and moving to monsters, barricades and destructibles

* Improved Tempest Rush ZigZag cluster numbers

* Tempest Rush will be better about *not* picking up non-legendary items while channeling with monsters nearby

* Optimized Monk Dashing Strike. Added support for Way of the Falling Star rune.

* Fixed legendary plans in ItemRules 

* Barb Rend is now used more intelligently (borrowed some code from herbfunk!)

* Barb Hammer of the Ancients is now actually used (was incorrectly setup)

* Hopefully fixed flip/flop item pickup bug

* Forced units within 20 yards to be in LoS regardless if Navigator Raycast says so or not

* Force close range fighting/targeting is now detected faster - should help reduce deaths and stucks from trying to run into large packs walls of monsters (after a shrine, perhaps?)

* Changed world object tracking hashing method from SHA1 to MD5 (should reduce CPU slightly)

* Added options to completely disable TrinityLogs outputs (ItemRules, Reports)

* Small optimization for TrinityExploreDungeon to mark future nodes as already visited if the navigable center is also nearby (within PathPrecision distance)

* Unstucker in town now used at 15 sec instead of 30, should help with lame Demonbuddy A3 blacksmith stucks

* Adjustments/optimizations to WD soul harvest

* GoldInactivity now reloads the current profile instead of the last used profile when exiting game

* Changed default value of GoldInactivity to 600sec

* Adjusted weighting for Legendary items (should help pick them up faster to avoid being missed)

* Fixed crash on 'Reload Item Rules' button

* Changed HandleTargetTimeout (target blacklisting) to only trigger if we're not moving/attacking.

* Fixed IsTryingToTownPortal - it now correctly detects if the current profile behavior is a UseTownPortal tag and extends trash kill range appropriately

* Fixed bot attacking Untargettable/Invulnerable/Burrowed units (|| instead of && derp) - fixes "wall climbers"!

* Added avoidance for succubus projectiles

* Fixed weird monk bug where it was trying to cast sweeping winds 10 times per second after taking a town portal

* Adjusted movement position shift handler - should help with weird stucks near demonic forges and other navigation obstacles

### Changelog 1.7.2.5:

* Removed A3 Skycrown signal fires from navigation obstacle list... was causing stucks!

* Re-enabled DB closing on D3 crash.

* Fixed a townrun bug & missing legendary loot. Bot will not properly stand around and wait for a target while townrun timer is running.

* Added TempestRush on Trash only option.

* Small fix for Treasure Goblins behind doors and Keeps 2 Destructible (Barricade) doors

* Gold Inactivity timer should no longer crash D3/DB

* GilesTrinity.xml will now be saved in with indentation (for easier human consumption)

* Added some additional safety checks for not picking legendary items

* Added a LineOfSight check for BodyBlocking for Items/Shrines/Gold/Containers - should help the mysterious "zomg didn't pickup my legendary!" and missing random shrines etc

### Changelog 1.7.2.4:

* Now picks up craft tomes regardless of misc item level selection if we've chosen to pickup craft tomes

* Hard rules will now always keep Archon plans and Marquise designs - feel free to edit :)

* Should be better about checking for nearby monsters while trying to Town Portal

* Added a few missing entries to translation.dis

* Fixed TrinityExploreDungeon using townportal on timeout and not finishing tag. Also added extra node-done check for ReachedDestination (since we're using DB navigator now)

* Fixed molten core avoidance (i hope!)

* Wizard Teleport, Twister, and WitchDoctor Acid Rain are now better at finding cluster targets

* Some additions and fixes/tweaks to TargetUtil GetBestClusterPoint

* Once again attacks and kills Ghom (his boss SNO was missing... weird)

* Small adjustment to IsAttackable detection - should be faster and more reliable as well

* Small adjustment to TrashMob Cluster detection - changed movement speed update to every Movement request/tick and reduced minimum movement speed to ignore trash mobs, and reduced minimum range for trash mob to be ignored to 2f RadiusDistance

* Added townPortalOnTimeout="True/False" to TrinityExploreDungeon (for tinnvec :))

### Known issues 1.7.2.3/4:

* WD Gargantuan is still spammed

* Gold Inactivity Timer will currently crash D3 - this is believed to be a Demonbuddy related problem (Nesox has confirmed a fix in the next version). No workaround currently available.

* Disconnects still being investigated.

* More analysis to be done on CPU utilization (but this version is at least better)

### Changelog 1.7.2.3:

* Added options to set minimum trash mob pack size and trash mob pack radius (default = 1, 20f)

* Set Demonbuddy to not close when game client crashes (should help with crash reports, except when using YAR)

* Made a bunch of optimizations that should help reduce CPU utilization

* Added more fine-grained pickup options for Designs/Plans, Essences, etc

* Monk SW buff logic is no longer "ticked" when merely running past a monster

* Unstucker is slightly less sensitive (now requires stuck for 5 seconds instead of 3). 

* Unstucker no longer kicks-in on Town Runs.

* Added additional logging mechanism to log ALL dropped items (and whether or not we wanted to pick it up) with a bunch of useful info for Devs

* Fixed Player Summons counters (e.g. Zombie dogs/Gargantuan) not counting correctly - still needs more stuff to work in PVP/Multiplayer games

* Gold Inactivity timer will no longer keep trying to leave the game if bot stop button is pressed

* Will now pickup Grey and White Items if player level is less than 6 and 11 (respectively)

* Added GUI options to pickup/ignore Crafting Materials (Demonic Essences)

* Removed 'Ignore solitary trash mob (40f)' option

* Added psuedo clustering logic into TargetUtil - could be useful for all "AoE" type of powers!

* Wizard: CM/WW Wiz's will now move into melee range before casting energy twister (like Trinity 1.6)

* Wizard Energy Twister will now try to use the best 'cluster' to drop tornadoes

* CM/WW Wizards may now teleport into the best nearby cluster location

* TrinityExploreDungeon will be better about checking for Grid Segmentation resets

* Removed a bunch of unused legacy code 

### Changelog 1.7.2.2:

* Fix for stucks investigating MiniMapMarkers (also helps with until=ExitFound on TrinityExplore.. )  

* Wizard Archon Cancel for MagicWeapon and Familiar now works correctly  

* Changed Default Trinity pickup settings to "Champion Hunting" (was Questing... legacy from my Questing pack)  

* Fixed Demonbuddy Crashing on first item drop when trying to use Demonbuddy Loot rules  

* Updated all Weighting formulas for consistency - removed magic number/formula weights, max distance and max weight are clear now. Shrines work again  

* Doubled minimum elite kill radius (from 60 to 120)  

* Wizard Critical Mass passive now automatically detected, updated all skills to use passive detection  

* Wizard Frost Nova now detects Deep Freeze rune.  

* Wizard Frost Nova is now properly spammed when paired with Critical Mass regardless of rune  

* Wizard Blizzard now detects Snowbound Rune  

* Wizard Energy Twister + Wicked Wind rune works better  

* Wizard Teleport Wormhole rune works now  

* Fixed default attacks for 2Handed weapons (e.g. for Wiz CM/WW with a 2H)  

* Removed maximum range for shrines (still weighted based on range)  

* Tweaked Barbarian Multi-Target Whirlwind ZigZag a little bit (added range and reduce minimum mob count)  

* Tweaked Monk Tempest Rush check a little bit, improved debug logging  

* Added tesslerc's Monk Dashing Strike tweaks

### Changelog 1.7.2.1:

* Requires DemonbuddyBETA .172+ or Demonbuddy release .300+

* Picks up demonic essances with both Trinity scoring and ItemRules 

* Fixes for TrinityOffsetMove and MiniMapMarker investigation attempting to move to un-navigable locations

### Changelog 1.7.2.0:

* Requires DemonbuddyBETA .159+

* ItemRules2 now has a GUI that replaces config.dis

* Wizards now have Archon Cancellation options (thank Nesox!)

* Wizard-Archon arcane strike is now only used when the current target is within melee range, and there are 2 or more targets within melee range

* Unstucker will activate in town if needed - but not with vendor/salvage window open

* Added fix for not attacking Sin Heart in Heart of the Cursed

* Small fix for Wizard Kiting options (forgot to hook settings to kite logic in .19)

* Fixed performance bug with Stashing/Selling/Salvaging logic

### Changelog 1.7.1.19:

* Requires DemonbuddyBETA .155 or higher (will not compile otherwise)

* Implimented new Item Manager - Demonbuddy is now (mostly) in control of town runs (stashing, selling, salvaging)

* Fixed Monk SeepingWind not spamming 

* Modified all PathFinding and Navigation code to use the DB Navigator now (there were fixes for generated worlds in .151)

* Improved a few monk abilities with rune detection / cyclone strike / wave of light

* Wired up new Wiz GUI options to Wizard Ability selection - can now select minimum archon mob count and minimum mob distance, and selectable archon kite form (anytime/normal only/archon only)

* Created new TargetUtil helper class - makes for a cleaner, more organized ability selector spaghetti

* Created new HotbarSkills helper class - we can now detect runes in ability selection

* Stucks are again logged to UserInformation category (always visible in logs)

* Changed Raycast methods to use DB Navigator/Mesh raycast (should be faster and hopefully crash less!)  

* Should no longer continue with profile tags while bags are full before doing a town run. 

* Barbarian WW / Monk TR single target zig zag should now work a little better

* TrinityExploreDungeon can now use sceneName with until="SceneFound", and PrioritizeScenes pathPrecision attribute actually works now (instead of using tag pa

* Added new TrinityMoveToScene tag - moves to the center of a matching scene, if found (basically same code from TrinityExploreDungeon)

* Increased treasure goblin 'Prioritize' weight from 10k to 15k

* Reverted TrinityMoveTo to always use the DB Navigator (but still always with "local" navigation)

ItemRules changes:  

    - added [WEIGHTS] you can use now scoring from DB inside rules  

    - fixed [AS%] working now on weapon, armor and jewelry  
    - name is now correctly working (item name bug is fixed)  
    - changed [AS%] now working also on weapons   
    - changed [BLOCK%] now working also on shield  
    - added [TOTBLOCK%] for total blockchance on shield  
    - added [DMGVSELITE%] for percent damage bonus vs elite  
    - added [DMGREDELITE%] for percent damage reduction from elite  
    - added [EXPBONUS] for experience bonus  
    - added [LOK] for Life on kill  
    - added [REQLEVEL] for item required level  
    - added [WEAPDMGTYPE] (Arcane,Holy, etv.)  
    - added [WEAPDMG%] for damage % enhancment on wepon  
    - added [ROLL] representing the attribut roll on magic and rare items  
    - added [D3QUALITY] orginal db quality rare3,rare4 etc.  
    - Polearm and Andariels workaround removed again ... DB is right now  
    - maximum function is now usable examples will follow  
    - changed logging to use with itemviewer  



### Changelog 1.7.1.18:

* Improved new Navigator/Pathfinder - Should fix backtracking and stuttering issues

* Adjusted speed sensor to better detect when we need to destroy destructables (only when we're stuck!)

* Increased gold weight significantly to help reduce backtracking after combat.

* Increased default ObjectDistance (60f) in TrinitExploreDungeon for AlternateActors and increased default PathPrecision (15f) for PrioritizedScenes.

* ItemRules now has an optional account-specific ItemRules set (soft/hard/custom) setting through the GUI (still possible to use config.dis as well)

### Changelog 1.7.1.17:

* Removed all WeaponSwap related code. See this for more info: http://www.thebuddyforum.com/demonbuddy-forum/plugins/trinity/102820-item-swap-future.html

* TrinityExploreDungeon is now fully "release ready". Documentation to follow. Features include reduced backtracking, automatically moving to minimap markers, prioritized scenes, ignored scenes, and more!

* Monk Tempest Rush has new usage options - Always, Movement Only, Elites and Groups, and all Combat. TR is now maintained after combat as needed as well.

* PlayerMover will no longer use special movement within 10 seconds of being stuck

* Disabled checking for Toggle Looting tags and missing Profile PickupLoot elements (your bot should now always loot regardless of bad profiles, like before Trinity .13)

* Disabled check for Profile <Combat /> profile element (combat now default enabled, but still togglable through ToggleTargetting tag)

* Included 4seti's fix for "1 slot left in bag and not townrunning" - hopefully it works?

* Added dynamically increasing radius for Unstucker based off how many stuck attempts - should no longer run away 1/2 a mile and get lost...

* Increased Barricade destructable range

* Added fixed kite locations for Azmodan avoidance

* Added a few memory safety checks in target handler and player mover - should help reduce crashes

* Modified PlayerStatus to no longer cache Primary/Secondary resource, health, and position and is now read directly from DB (this is "fast" since DB .298 / BETA .140)

* TrinityMoveTo will now use the PathFinder in Generated areas, and the Navigator in static areas (should be more reliable) - tested with many profiles including questing, alkaizer, etc.

* Added logic to blacklist targets that are added/removed from object manager too many times (fixes weird stucks trying to pickup gold)

* Improved layout of Advanced tab / logging options

* Fixed backwards destructible weighting (now weights destructables correctly according to distance)

* Changed default ItemRules2 rules to "soft"

* Added new XmlTag: TrinityOffsetMove. Documentation to follow.

* Fix for trash mob in/out of range flip/flop (while moving to attack).

* Merged Persistent Stats from tomasd. Trinity will now record and save persistent statistics in a seperate file, including per-world stats.* 

### KNOWN ISSUES 1.7.1.17:

* Wizard's without a signature spell will not use the default attack, for example CM/WW builds (seems to be a limitation within Demonbuddy... still trying to find a fix)

* Wizards will not cancel archon buff (Coming Soon™!)

* Tempest Rush Movement will sometimes get stuck on corners and objects and requires the unstucker to kick in

* Demonbuddy DungeonExplorer will (at maybe, 0.01% of the time) read and cache incorrect scenes, causing long stucks in generated dungeons. TrinityExploreDungeon has an built in 15 minute timer (adjustable!) as a workaround.


### Changelog 1.7.1.16:

* Fixed TrinityMoveTo 

* Disabled dropped items log, and skipped gold log (were really only intended for dev purpose only)

* Fixed not destroying some barricades / operating some gizmos

### Changelog 1.7.1.15:

* Fixed inflated Item Dropped per hour statistics. Your IPH in stats logs will probably decrease considerably.

* Fixed gold pickup derp bug, increased weight for very close gold piles

* Fixed townrun ignoring mobs + extended kill radius logic now includes UseTownPortal profile tag

* Added configurable cache refresh rate in Trinity Advanced tab to optionaly help reduce CPU utilization and diagnose crashes. May cause bot to act strangely, use with caution.

* Added Gold gained to stats log (thanks Tesslerc!)

* Removed Pause/Townrun buttons from GUI (now included natively in latest DemonbuddyBETA).

* Current Profile is now displayed in the DB window title

* Fix for flip/flopping current target if gizmo (shrine/door) changes into and out of range.

* Slightly Modified Champ Hunting Items tab configuration defaults.

* Now logs all items dropped into CSV file in Trinitylogs

* Now logs all skipped gold piles into CSV file in TrinityLogs (or, "ScroogeMcDuck mode" as darkfriend puts it)

* Added caching for if a unit/item/gold/shrine is ever in LoS/Navigable/RayCast (should help with flip flopping and missed targets)

* TrinityMoveTo profile tag now uses Navigator by default (disable with useNavigation="false")

* ItemRules2: Removed Medium Rules (no longer maintaned)

* ItemRules2: Fixes for cached item name bug

* ItemRules2: Added [WeaponDamageType] (Arcane, Holy, etc)

* TrinityExploreDungeon prototype profile tag included. Don't use it, it doesn't work. You will get stuck, and rrrix won't answer questions or help you with it. For educational purposes only.

### Changelog 1.7.1.14:

###### REQUIRES DemonbuddyBETA 1.0.1240.115 OR HIGHER  

###### WILL NOT WORK WITH .294!  

* New XML Tag: TrinityLoadOnce - will load a set of profiles in random order within a single game session.   
This XML tag will load a random profile from the list, but only once during this game session 

* New Barbarian multi-target Whirlwind and monk tempest rush logic, with GUI option to disable if you don't like it.   
This helps with "chaining" large packs of trash mobs, rather than X/criss-cross only a single target in a pack.

* Added GUI option to ignore solitary trash mobs. This will cause bot to ignore a trash mob when no other trash mob is within 40yds of it. Automatically disabled if elites are present. 

* Bot will no longer continue on profile behaviors while waiting for pre-TownRun timer.

* Supports new DB BETA CanTownRun() logic, also fixed town run with bags 1/2 full.

* Improved TrinityRandomWait tag (no longer using Thread.Sleep()), does not lock Demonbuddy - allows combat/looting to continue while waiting.

* More improvements to destructibles/barricades logic.

* Increased DemonHunter destructible power range.

* Decreased DemonHunter Caltrops timer from 6 sec to 3 sec.

* Fixed reset gold counter on new game.

* Added additional logging for vendor movement logic during town run (to help determine stucks).

### Changelog 1.7.1.13:

* New XML Tags: TrinityRandomWait and TrinityCastSweepingWinds

* Fixed UnStucker.

* Improved destructible and barricade logic

* Adjustment for destructible object radius's and weighting. Destructible object minimum and default on slider is now 1 (was 6).

* Improved navigation obstacle handling (should now correctly avoid Demonic Forges in Arreat Crater)

* Bot will no longer attempt to town portal on A1 Quest 1 Step 1

* New *trash mob* blacklisting logic - if > 90% health, hasn't been attacked in 4 seconds, and not raycastable, it's blacklisted.

* Bot will now town-run if it happens to be in town and bags are more than half full.

* Re-added 14yd ZDiff check for non-boss units.

* TrinityMoveTo now always uses local navigation.

* Bot will now sell non-optimal potions (if higher level potions are found).

* Now uses potions from the smallest stack first

* Added TeamID check for Units (should help with un-attackable units like Sin Heart before it's attackable)

* Fixed monk ability selector always closing inventory window

* Fixed monk tempest rush not picking up items.

* LoS/NavMesh Raycast is used again when needing to town run.

* ItemRules2: Additional logging added.

* ItemRules2: Default rules changed to soft;

* ItemRules2: Now accepts any language as string (russian, chinese, etc.)

### Changelog 1.7.1.12:

* Fixed A3 Skycrown/Stonefort barricade problem

* Latest WeaponSwap 1.0.2a

### Changelog 1.7.1.11:

* New GUI option: "Use NavMesh to prevent stucks" on the Combat>Misc tab. Default and recommendation is to enable this option. Disabling this may lead to stucks for bot attempting to target monsters/shines through walls and floor gaps. If you experience severe performance problems, try disabling this option. 

* Barbarian Weapon Throw can now be used as "primary" if no other primary ability is present.

* Reduced barbarian bash destructable attack range.

* Changed Targetable/Invulnerable/Burrowed/NPC checks (should be faster now)

* Profile toggle targetting / toggle loot tags now work as expected

* TrinityLogs moved to Demonbuddy directory.

* Added more performance logging.

### Changelog 1.7.1.10:

* Fixed performance problem accessing SceneId.

* Fixed monk tempest rush movement and other channeling spells.

* Fixed bot stuck when manually clicking on D3 ground position.

* Bot now sells items from backpack sorted ascending first by row then by column (left to right, top to bottom).

* Added more in-depth performance logging & Reduced logging noise for very fast performance logging sections.

* Can now specify Unsafe Kite zones for boss area kiting.

* Simplified gold and item weighting forumulas, should now always pickup items and gold if in range.

* Adjustments for Kiting targeting.

* Gold inactivity timer reset on bot start.

* Should no longer attack through 'dummy' signal fires.

* Picks up all health potions again (was missing Greater health potions).

* Fixed forced vendor run (will no longer continue moving).

* Fixed item rules pickup validation, should now correctly pickup items again when using Item Rules

* Trinity now attacks Belial again

### Changelog 1.7.1.9:

* Now opens vendor window before repairing

* Performance fixes (no longer calling ActorManager.Update(), since DB does this itself)

* Fixed Demonhunter Vault Delay slider

* Fixed localization issues for some countries in ItemRules2

* Additional debug logging for special movement

* changed logging and added some more options for config.dis

### Changelog 1.7.1.8:

* Fixed reverse gold pickup bug

* darkfriend77 ItemRules2 2.0.20

* New Prototype Kiting logic, still getting refined but so far works better than before. Is now grid-based (instead of a set of circles) and also uses path-finding. (Tested with DemonHunter @ 20 yards)

* Trinity unstucker and Bot TPS now take immediate effect (previously needed bot stop/start)

* Bot TPS slider now goes to 30 for those of us with more horsepower

* Kiting players will no longer attempt to rush head-first into monsters/avoidance for a health globe

* Added some adjusments to help prevent town portalling while monsters are present

* Fixed stucks A3 Skycrown Catapult barricade/destructables

* Ranged players will no longer attempt to attack through navigation obstacles (like Signal Fires in Skycrown) 

* Removed blacklisting for 0-hitpoint monsters (now they're simply just not added to cache), monsters were possibly ignored due to D3 memory exception on first read.

* Added check for town-portalling in boss areas and non-town-portalable places like A2 caldeum bazaar

* Now displays XP Per hour in the log file

* Options to ignore Shrine types added

* Monk now has Tempest Rush movement option

* Monk WeaponSwap fixes/updates

* DemonHunter: Added options for for Spam smoke screen and preparation OOC. 

* DemonHunter: Evasive fire can now be used as a primary ability and will no longer case default attacks to be used.  

* DemonHunter: Added avoidance and kiting safety checks for DemonHunter vault. 

* WitchDoctor: Added  AcidCloud to destructable spells (Thanks Yadda).

* Wizard: Energy twister is now only "used" if we have enough energy (default attacks are now used). 

* Wizard: Archon Arcane strike is now used at 7 instead of 13 (should prevent chasing). 

* Wizard: Timers on Wizard armors changed from 115sec to 60sec. 

### Changelog 1.7.1.7:

* Monk performance issues should be resolved

* Fix for monk blinding flash (by Magi)

* Improved overall performance (removed potential duplicate actor update & frame locking)

* Fixed attacking monsters/ubers with 0% health

* Removed kiting when below 15% health (kiting still works if turned on in class config)

* Fixed salvaging legendaries

* Increased range that blocking destructables are destroyed at (from 2 to 5), may need further adjustment

* Rolled back dynamic gold pickup logic to "simple" - (for a more advanced version, see thread 1.7.1.7 mods by SP). 

* Added additional chest/resplendent chest SNO's

* Fixed gold pickup flip-flop (stuck) while moving to pickup far away gold


### Changelog 1.7.1.6:

* Fixed legendary item attributes being blank

* Fixed "Test Backpack" scoring button

* Adjusted a few more log levels as appropriate

* WeaponSwapper now only attempts to run Swap and SecurityCheck if you're a Monk


### Changelog 1.7.1.5:

* Fixed memory read errors

* Fixed incorrect log level for cache refresh exceptions

* Darkfriend77 ItemRules2 included

* Latest version of tesslerc's Monk WeaponSwap

* Fixes for Magi's Uber run profiles

* Can now set item pickup default levels - Questing / Champion Hunting

* Fixed "Iron Gate" for good :)

* Fixed player summons being blacklisted prior to being counted

* Fixed stuck after killing the Butcher

* New gold pickup logic from user !sp

* Hopefully fixed kiting & Changed Kiting defaults for WIZ/WD to 0

* Hopefully fixed the possibilities of not picking up items (legendaries) even if not in LoS or not navigable.

* Started implimenting new cache system

* Started implimenting new item rule scripting system
  


### Changelog 1.7.1.4:

* Fixes gold pickup radius

* Fixes errors/exceptions in incorrect log level

* Can now reload script rules from GUI


### Changelog 1.7.1.3:

* Tooltips describe each new UI option for managing Selling and Salvaging of Magic, Rare, and Legendary items. Note: Legendaries are never salvaged/sold when using only Trinity Scoring.

* Combat Looting re-activated. Will use Demonbuddy > Settings > Enable combat looting checkbox

* Combat Looting will no longer attempt to prioritize loot where a monster is in the path

* Whimsyshire Pinata's are now used if within Container Open range (make sure to increase this if you're in Whimsywhire!)

* Trinity has a hunger for chickens once again! 

* Infernal Keys are now always combat looted regardless of Combat Looting setting

* Trinity will now repair all inventory instead of just equipped items. Tell me if you want this as an option in GUI.

* Avoidance checkbox works again

* Script rule selection screen now includes a link to the forum thread for people who don't understand what it's for or what it does.

* Monk sweeping wind weapon swap will now instantly re-swap after casting sweeping wind.

* Added logging for Weapon Swap.

* A few small performance enhancements in cache logic.

* Cache Logger now contains a performance counter.

* Lots of refactoring of Caching System and Target Handler

* Lots of refactoring for Logging, many new advanced options for logging selection

### Changelog 1.7.1.2

* UI Works in all regions now

### Changelog 1.7.1.1

* Fix for darkfriend77 item rulesets not being used

* Fixed WD grave injustice checkbox not being saved or used correctly

### Changelog 1.7.1.0

* Entire new UI system using XAML/WPF instead of WinForms





