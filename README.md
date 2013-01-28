## Unified Trinity Community Edition


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





