// +---------------------------------------------------------------------------+
// | _ _| __ __| __|   \  |     __|  __| __ __|    _ \  |  | |     __|   __| 
// |   |     |   _|   |\/ |   \__ \  _|     |        /  |  | |     _|  \__ \ 
// | ___|   _|  ___| _|  _|   ____/ ___|   _|     _|_\ \__/ ____| ___| ____/ 
// +---------------------------------------------------------------------------+
// | Created by darkfriend ... idear from D2 Pickit
// +---------------------------------------------------------------------------+
// +---------------------------------------------------------------------------+
// |   __| __ __| _ \  |  |  __| __ __| |  | _ \  __| 
// | \__ \    |     /  |  | (       |   |  |   /  _|  
// | ____/   _|  _|_\ \__/ \___|   _|  \__/ _|_\ ___|                                            
// +---------------------------------------------------------------------------+
// | only use one line per rule
// | 
// | '#' sign divides the rule in to 3 parts
// | 1 # 2 # 3 example. [TYPE] == Legs # [STR] < 50 # [TRASH]
// | 
// | part 1 =  [TYPE] == Legs
// | the info available on unidentified items ...
// | 
// | part 2 =  [STR] < 50
// | the info available on identified items ...
// | 
// | part 3 =  [TRASH]
// | the action to do with the item on a positive rule outcome
// | 
// | TO TEST any kind of logical coposition u can use [1]
// | that returns 1 and [TEST] that returns TEST.
// | ex. [TEST] == Test # [1] == 1 && ([1] != 1 || [1] != 1)
// +---------------------------------------------------------------------------+
// +---------------------------------------------------------------------------+
// |   _ \  _ \ __|  _ \    \ __ __| _ \  _ \   __| 
// |  (   | __/ _|     /   _ \   |  (   |   / \__ \ 
// | \___/ _|  ___| _|_\ _/  _\ _| \___/ _|_\ ____/ 
// +---------------------------------------------------------------------------+
// | and                   "&&"
// | or                    "||"
// | equal then            "=="
// | not equal then        "!="
// | less or equal then    "<="
// | bigger or equal then  ">="
// | less then             "<"
// | bigger then           ">"
// | u can use round brackets (blah || blah || blah)
// +---------------------------------------------------------------------------+
// +---------------------------------------------------------------------------+
// |    \ __ __| __ __| _ \ _ _|  _ )  |  | __ __| __|   __| 
// |   _ \   |      |     /   |   _ \  |  |    |   _|  \__ \ 
// | _/  _\ _|     _|  _|_\ ___| ___/ \__/    _|  ___| ____/     
// +---------------------------------------------------------------------------+
// | -CODE-                           | Attribute-                  | Example                                                 
// +----------------------------------+-----------------------------+----------+
// |  [BASETYPE]                      | ItemBaseType                | Weapon 
// |  [QUALITY]                       | ItemQuality                 | Rare						 
// |  [TYPE]                          | ItemType                    | Axe							 
// |  [LEVEL]                         | Level                       | 60								 
// |  [ONEHAND]                       | OneHand                     | true							
// |  [TWOHAND]                       | TwoHand                     | false							 
// +----------------------------------+-----------------------------+----------+
// |  [STR]                           | Strength                    | 100							 
// |  [DEX]                           | Dexterity                   | 100							 
// |  [INT]                           | Intelligence                | 100						 
// |  [VIT]                           | Vitality                    | 100							 
// +----------------------------------+-----------------------------+----------+
// |  [REGEN]                         | HealthPerSecond             | 254					 
// |  [LIFE%]                         | LifePercent                 | 12						 
// |  [LS%]                           | LifeSteal                   | 3							 
// |  [LOH]                           | LifeOnHit                   | 655							 
// |  [MS%]                           | MovementSpeed               | 12						 
// |  [AS%]                           | AttackSpeedPercent          | 6				 
// +----------------------------------+-----------------------------+----------+
// |  [CRIT%]                         | CritPercent                 | 4.5						 
// |  [CRITDMG%]                      | CritDamagePercent           | 59					 
// |  [BLOCK%]                        | BlockChance                 | 8						 
// +----------------------------------+-----------------------------+----------+
// |  [ALLRES]                        | ResistAll                   | 80							 
// |  [RESPHYSICAL]                   | ResistPhysical              | 60					 
// |  [RESFIRE]                       | ResistFire                  | 60						 
// |  [RESCOLD]                       | ResistCold                  | 60						 
// |  [RESLIGHTNING]                  | ResistLightning             | 60					 
// |  [RESARCAN]                      | ResistArcane                | 60						 
// |  [RESPOISON]                     | ResistPoison                | 60						 
// |  [RESHOLY]                       | ResistHoly                  | 60						 
// +----------------------------------+-----------------------------+----------+
// |  [ARMOR]                         | Armor                       | 345								 
// |  [ARMORBONUS]                    | ArmorBonus                  | 200						 
// |  [ARMORTOT]                      | ArmorTotal                  | 1300					 
// +----------------------------------+-----------------------------+----------+
// |  [FIREDMG%]                      | FireDamagePercent           | 3					 
// |  [LIGHTNINGDMG%]                 | LightningDamagePercent      | 3			 
// |  [COLDDMG%]                      | ColdDamagePercent           | 3					 
// |  [POISONDMG%]                    | PoisonDamagePercent         | 3				 
// |  [ARCANEDMG%]                    | ArcaneDamagePercent         | 3				 
// |  [HOLYDMG%]                      | HolyDamagePercent           | 3					 
// +----------------------------------+-----------------------------+----------+
// |  [DPS]                           | WeaponDamagePerSecond       | 1100				 
// |  [WEAPAS]                        | WeaponAttacksPerSecond      | 1.5			
// |  [WEAPMAXDMG]                    | WeaponMaxDamage             | 560					 
// |  [WEAPMINDMG]                    | WeaponMinDamage             | 255					 
// |  [MINDMG]                        | MinDamage                   | 100							 
// |  [MAXDMG]                        | MaxDamage                   | 200							 
// +----------------------------------+-----------------------------+----------+
// |  [THORNS]                        | Thorns                      | 2345							 
// |  [DMGREDPHYSICAL]                | DamageReductionPhysicalPerc.| 2		 
// +----------------------------------+-----------------------------+----------+
// |  [MAXARCPOWER]                   | MaxArcanePower              | 15					 
// |  [ARCONCRIT]                     | ArcaneOnCrit                | 10						 
// |  [MAXMANA]                       | MaxMana                     | 5							 
// |  [MANAREG]                       | ManaRegen                   | 5							 
// |  [MAXFURY]                       | MaxFury                     | 5							 
// |  [HEALTHSPIRIT]                  | HealthPerSpiritSpent        | 245				 
// |  [MAXSPIRIT]                     | MaxSpirit                   | 10							 
// |  [SPIRITREG]                     | SpiritRegen                 | 5						 
// |  [HATREDREG]                     | HatredRegen                 | 5						 
// |  [MAXDISCIP]                     | MaxDiscipline               | 5						 
// +----------------------------------+-----------------------------+----------+
// |  [GF%]                           | GoldFind                    | 25							 
// |  [MF%]                           | MagicFind                   | 20							 
// |  [PICKUP]                        | PickUpRadius                | 5						 
// |  [GLOBEBONUS]                    | HealthGlobeBonus            | 5468					 
// +----------------------------------+-----------------------------+----------+
// |  [SOCKETS]                       | Sockets                     | 1							 
// +----------------------------------+-----------------------------+----------+
// |  [MAXSTAT]                       | highest class specific stat | 200	
// |                                  | (str,int,dex)		    |
// |  [MAXSTATVIT]                    | highest class specific stat | 250		
// |                                  | (str,int,dex) + vit         |				
// |  [MAXONERES]                     | highest single resist       | 60				
// |                                  | (arcane,cold,fire,holy,     |
// |                                  |  lightning,physical,poison) |
// |  [TOTRES]                        | total resistance            | 140					
// |                                  | (allres,arcane,cold,fire,holy,
// |                                  |  lightning,physical,poison) |
// |  [DMGFACTOR]                     | dmg factor                  | 12						
// |                                  | = as% + crit%*2 + critdmg%/5|
// |                                  |   + average/20              |
// |  [STRVIT],[DEXVIT],[INTVIT]      | primary attribut vitality   | 200			
// |  [AVGDMG]                        | average dmg                 | 200						
// |                                  | = (mindmg + maxdmg) / 2	    |
// |  [OFFSTATS]                      | offensiv stats              | 3					
// |                                  | = as%,crit%,critdmg%,avgdmg |
// |                                  | counting each as one if it is
// |                                  | bigger then 0	            |
// |  [DEFSTATS]                      | defensiv stats              | 3					
// |                                  | = vit,allres,armorbonus,    |
// |                                  |   block%,life%,regen        |
// |                                  | counting each as one if it is
// |                                  | bigger then 0               |
// +----------------------------------+----------------------------------------+			
// +---------------------------------------------------------------------------+
// | \ \   /  \    |     |  | __|   __| 
// |  \ \ /  _ \   |     |  | _|  \__ \ 
// |   \_/ _/  _\ ____| \__/ ___| ____/ 
// +---------------------------------------------------------------------------+
// |  ItemType:
// |  ---------
// |  Axe,Sword,Mace,Dagger,Bow,Crossbow,Staff,Spear,Shield
// |  Gloves,Boots,Chest,Ring,Amulet,Quiver,Shoulder,Legs
// |  FistWeapon,Mojo,CeremonialDagger,WizardHat,Helm,Belt
// |  Bracer,Orb,MightyWeapon,MightyBelt,Polearm,Cloak,Wand
// |  SpiritStone,Daibo,HandCrossbow,VoodooMask,FollowerSpecial
// +---------------------------------------------------------------------------+
// |  ItemBaseType
// |  ------------
// |  Armor,Weapon,Jewelry,Misc,Gem
// +---------------------------------------------------------------------------+
// |  ItemQuality
// |  -----------
// |  Inferior,Normal,Superior,Rare,Magic,Legendary,Special
// |
// |  for CraftingPlans use Rare4,Rare5,Rare6 & Legendary
// +---------------------------------------------------------------------------+
// +---------------------------------------------------------------------------+
// |  |  /   \ |   _ \ \ \      / \ |    _ )  |  |  __|   __| 
// |  . <   .  |  (   | \ \ \  / .  |    _ \  |  | (_ | \__ \ 
// | _|\_\ _|\_| \___/   \_/\_/ _|\_|   ___/ \__/ \___| ____/ 
// +---------------------------------------------------------------------------+
// | - [AS%] not working on weapons ... use [WEAPAS] to check for speed
// | - [BLOCK%] not working on shields (untested) 
// +---------------------------------------------------------------------------+