Unified Trinity Community Edition
=================================

Major new features: 

* Source control :)

* Split Monolithic single file into seperate files by Class

* Split GilesTrinity class into separate files by Feature

* Repair Cache System for Gizmo's

* Create a settings class hierarchy which replaces all private fields in Plugin class.

* Rebuild entire UI system to utilize XAML/WPF

* Improved code Readability


Future Planned Improvements: 

* New cache system, trash the HashSet's and Dictionaries for an object based system

* Add stayInTown attribute for TrinityTownRun Profile Tag 

Changelog 1.7.1.10:

Simplified gold and item weighting forumulas.

* Adjustments for Kiting targeting.

* Gold inactivity timer reset on bot start.

* Should no longer attack through 'dummy' signal fires.

* Picks up all health potions again (was missing Greater health potions).

* Fixed forced vendor run (will no longer continue moving).

* Fixed item rules pickup validation, should now correctly pickup items again when using Item Rules

* Trinity now attacks Belial again

Changelog 1.7.1.9:

* Now opens vendor window before repairing

* Performance fixes (no longer calling ActorManager.Update(), since DB does this itself)

* Fixed Demonhunter Vault Delay slider

* Fixed localization issues for some countries in ItemRules2

* Additional debug logging for special movement

* changed logging and added some more options for config.dis

Changelog 1.7.1.8:

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

Changelog 1.7.1.7:

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


Changelog 1.7.1.6:

* Fixed legendary item attributes being blank

* Fixed "Test Backpack" scoring button

* Adjusted a few more log levels as appropriate

* WeaponSwapper now only attempts to run Swap and SecurityCheck if you're a Monk


Changelog 1.7.1.5:

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
  


Changelog 1.7.1.4:

* Fixes gold pickup radius

* Fixes errors/exceptions in incorrect log level

* Can now reload script rules from GUI


Changelog 1.7.1.3:

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




Changelog 1.7.1.2

* UI Works in all regions now



Changelog 1.7.1.1

* Fix for darkfriend77 item rulesets not being used

* Fixed WD grave injustice checkbox not being saved or used correctly



Changelog 1.7.1.0

* Entire new UI system using XAML/WPF instead of WinForms





