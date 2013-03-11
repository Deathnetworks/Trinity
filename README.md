
# Unified Trinity Community Edition

### Known issues:

* DH Sentry Turrent still does not work, appears to be DB bug

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





