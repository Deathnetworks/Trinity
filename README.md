Unified Trinity Community Edition
=================================

Major new features: 
* Split Monolithic single file into seperate files by Class
* Split GilesTrinity class into separate files by Feature
* Repair Cache System for Gizmo's
* Create a setting class hierarchy for replace All private fields in Plugin class.
* Rebuild entire UI system to utilize XAML/WPF
* Improve Readability

Code Cleanup TODO: 
* More Refactoring
* Separate TownRun in simple class : TownRunManager
* Put XML Comment on all class and member
* Refactor logging system to use DbHelper class
* Refactor Ability Selector for readability
* Refactor TargetHandler for readability / flow

Future Planned Improvements: 
* Add stayInTown attribut for <TrinityTownRun /> 
* Default Loot Settings options for Questing, Champion Farming


Changelog 1.7.1.3:
* UI options have been added for managing Selling and Salvaging of Magic, Rare, and Legendary Items. This is still a WORK IN PROGRESS. More documentation and help text to come soon.
* Combat Looting re-activated. Will use Demonbuddy > Settings > Enable combat looting checkbox
* Combat Looting will no longer attempt to prioritize loot where a monster is in the path
* Whimsyshire Pinata's are now used if within Container Open range (make sure to increase this if you're in Whimsywhire!)
* Infernal Keys are now forced combat looting regardless of Demonbuddy setting
* Trinity will now repair all inventory instead of just equipped items. Shout if you want this as an option in GUI.
* Avoid AOE checkbox works again
* Script rule selection screen now includes a link to the forum thread for people who don't understand what it's for or what it does.
* Monk sweeping wind weapon swap will now instantly re-swap after casting sweeping wind.
* Added logging for Weapon Swap.
* A few small performance enhancements in cache logic.
* Cache Logger now contains a performance counter.
* Lots of refactoring of Caching System and Target Handler
* Lots of refactoring for Logging mechanism

Changelog 1.7.1.2
* UI Works in all regions now

Changelog 1.7.1.1
* Fix for darkfriend77 item rulesets not being used
* Fixed WD grave injustice checkbox not being saved or used correctly

Changelog 1.7.1.0
* Entire new UI system using XAML/WPF instead of WinForms

