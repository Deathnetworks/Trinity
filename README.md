Trinity
=======

Splitted version of GilesTrinity renamed to UnifiedTrinity: 

Already be done : 
- Split 1 class / file
- Move different HashSet / Dictionnary / Constants in specialized class 
		* Constants
		* DefaultSettingList (may be need to move in setting class after) 
		* DefaultList
- Rename Property & Fields in respect of standard naming convention 
	(no b / s /... prefixe, Camel Casing Property and Fields, 

Todo : 
- Create a setting class hierarchy for replace All private fields in Plugin class.
- Separate TownRun in simple class : TwonRunManager
- Extract Config Window management code to another class
- Extract RefreshDia and cache on EnvironmentManager class (with simple methode are exposed) 
- Put XML Comment on on class and member

Evolve : 
- Add Items Rules by darkfriends in specific config ( Giles Scoring / Items Rules / Db rules)
