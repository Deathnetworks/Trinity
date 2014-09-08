

var Items = function() {

    var ItemType = {

        Helm: "helm",
        SpiritStone: "spirit-stone",
        VoodooMask: "voodoo-mask",
        WizardHat: "wizard-hat",
        Shoulders: "pauldrons",
        Chest: "chest-armor",
        Cloak: "cloak",
        Bracers: "bracers",
        Gloves: "gloves",
        Belts: "belt",
        MightyBelt: "mighty-belt",
        Legs: "pants",
        Feet: "boots",
        Amulet: "amulet",
        Rings: "ring",
        Shields: "shield",
        CrusaderShield: "crusader-shield",
        Mojos: "mojo",
        Orbs: "orb",
        Quivers: "quiver",
        Focus: "enchantress-focus",
        Token: "scoundrel-token",
        Relic: "templar-relic",
        Axe1h: "axe-1h",
        Dagger: "dagger",
        Mace1h: "mace-1h",
        Spears: "spear",
        Swords: "sword-1h",
        Knives: "ceremonial-knife",
        FistWeapon: "fist-weapon",
        Flail1h: "flail-1h",
        Mighty1h: "mighty-weapon-1h",
        Axe2h: "axe-2h",
        Mace: "mace-2h",
        Polearm: "polearm",
        Staves: "staff",
        Sword2h: "sword-2h",
        Diabo: "daibo",
        Flail2h: "flail-2h",
        Mighty2h: "mighty-weapon-2h",
        Bows: "bow",
        Crossbow: "crossbow",
        HandXBow: "hand-crossbow",
        Wand: "wand",

        GetKey: function (value) {
            var keys = Object.getOwnPropertyNames(ItemType);
            for (i = 0; i < keys.length; i++) {
                if (ItemType[keys[i]] == value) return keys[i];
            }
        }
    }

    var ItemData = function () {

        var data = {

            Helm: [],
            SpiritStone: [],
            VoodooMask: [],
            WizardHat: [],
            Shoulders: [],
            Chest: [],
            Cloak: [],
            Bracers: [],
            Gloves: [],
            Belts: [],
            MightyBelt: [],
            Legs: [],
            Feet: [],
            Amulet: [],
            Rings: [],
            Shields: [],
            CrusaderShield: [],
            Mojos: [],
            Orbs: [],
            Quivers: [],
            Focus: [],
            Token: [],
            Relic: [],
            Axe1h: [],
            Dagger: [],
            Mace1h: [],
            Spears: [],
            Swords: [],
            Knives: [],
            FistWeapon: [],
            Flail1h: [],
            Mighty1h: [],
            Axe2h: [],
            Mace: [],
            Polearm: [],
            Staves: [],
            Sword2h: [],
            Diabo: [],
            Flail2h: [],
            Mighty2h: [],
            Bows: [],
            Crossbow: [],
            HandXBow: [],
            Wand: []

        };

        var Item = {
            Quality: "",
            Type: "",
            ActorSNO: 0,
            Slug: "",
            Id: "",
            InternalName: "",
            Slot: "",
            ZetaType: "",
            Name: "",
            DataUrl: "",
            Url: "",
            RelativeUrl: "",
            IsCrafted: false,
            LegendaryText: "",
            IsSet: false
    }

        var request = function(itemTypeSlug, callback) {            

            var ItemTypeKey = ItemType.GetKey(itemTypeSlug);

            console.log("Requesting " + ItemTypeKey + " Data!");

            $.get("http://us.battle.net/d3/en/item/" + itemTypeSlug + "/", {}, function (response) {

                // Origin bypass with YQL wraps as a HTML document
                // So we have to strip it all out again.
                var doc = $("<div>").html(response.responseText);

                // Find all item records
                var items = doc.find(".table-items table tr");
                var staffofherding = false;

                // Process each item
                $.each(items, function () {

                    var item = Object.create(Item);

                    var itemHtml = $(this);
                    var itemDetails = itemHtml.find(".item-details");
                    var itemType = itemDetails.find(".item-type");

                    var itemDetailsLink = itemDetails.find(".item-details-text a");
                    
                    item.Name = itemDetailsLink.text();
                    if (item.Name == "")
                        return;

                    // strange double listing
                    if (item.Name == "Staff of Herding" && staffofherding) {
                        return;
                    } else {
                        staffofherding = true;
                    }

                    item.RelativeUrl = itemDetailsLink.attr("href");
                    item.Url = "https://us.battle.net" + item.RelativeUrl;

                    var splitUrl = item.Url.split("/");

                    item.IsCrafted = item.Url.contains("recipe");
                    item.Slug = splitUrl[splitUrl.length - 1]
                    item.DataUrl = "https://us.battle.net/api/d3/data/" + ((item.IsCrafted) ? "recipe" : "item") + "/" + item.Slug;

                    if (itemLookup[item.Slug] != null) {
                        item.ActorSNO = itemLookup[item.Slug][1];
                        item.InternalName = itemLookup[item.Slug][2];
                    }

                    // Quality
                    var qualityClassName = itemType.find("span").attr("class")
                    switch (qualityClassName) {
                        case "d3-color-green": item.Quality = "Legendary"; break;
                        case "d3-color-default": item.Quality = "Normal"; break;
                        case "d3-color-orange": item.Quality = "Legendary"; break;
                        case "d3-color-yellow": item.Quality = "Rare"; break;
                        case "d3-color-blue": item.Quality = "Magic"; break;
                    }

                    // Hellfire Variants
                    if (item.Slug.contains("hellfire")) {
                        var hellfireSlugParts = item.Slug.split("-");
                        item.Name = item.Name + " " + toTitleCase(hellfireSlugParts[hellfireSlugParts.length - 1]);
                    }

                    // Only record legendary/set items
                    if (item.Quality != "Legendary")
                        return;

                    item.Type = itemType.text();
                
                    
                    if (itemHtml.find(".item-weapon-damage").length !== 0) {
                        item.BaseType = "Weapon"
                    } else if (item.Type == "Amulet" || item.Type == "Ring") {
                        item.BaseType = "Jewelry"
                    } else if (itemDetails.find(".item-armor-weapon.item-armor-armor").length !== 0) {
                        item.BaseType = "Armor"
                    } else {
                        item.BaseType = "None"
                    }

                    var legText = itemHtml.find(".d3-color-orange.d3-item-property-default");
                    if (legText.length !== 0) {
                        item.LegendaryText = legText.text().trim();
                    }

                    var setNameHtml = itemHtml.find(".item-itemset-name");
                    if (setNameHtml.length !== 0) {
                        item.SetName = setNameHtml.text().trim();
                        item.IsSet = true;
                    }
                    

                    // Zeta Type
                    var itemTypeName = itemType.text();                    
                    if(itemTypeName.contains("Bracers"))
                        item.ZetaType = "Bracer";
                    else if(itemTypeName.contains("Chest"))
                        item.ZetaType = "Chest";
                    else if (itemTypeName.contains("Cloak"))
                        item.ZetaType = "Chest";
                    else if(itemTypeName.contains("Shoulders"))
                        item.ZetaType = "Shoulder";
                    else if (itemTypeName.contains("Helm"))
                        item.ZetaType = "Helm";
                    else if(itemTypeName.contains("Spirit"))
                        item.ZetaType = "SpiritStone";
                    else if (itemTypeName.contains("Voodoo"))
                        item.ZetaType = "VoodooMask";
                    else if (itemTypeName.contains("Wizard Hat"))
                        item.ZetaType = "WizardHat";
                    else if (itemTypeName.contains("Gloves"))
                        item.ZetaType = "Gloves";
                    else if (itemTypeName.contains("Belt"))
                        item.ZetaType = "Belt";
                    else if (itemTypeName.contains("Mighty Belt"))
                        item.ZetaType = "MightyBelt";
                    else if (itemTypeName.contains("Pants"))
                        item.ZetaType = "Legs";
                    else if (itemTypeName.contains("Boots"))
                        item.ZetaType = "Boots";
                    else if (itemTypeName.contains("Amulet"))
                        item.ZetaType = "Amulet";
                    else if (itemTypeName.contains("Ring"))
                        item.ZetaType = "Ring";
                    else if (itemTypeName.contains("Shield"))
                        item.ZetaType = "Shield";
                    else if (itemTypeName.contains("Crusader"))
                        item.ZetaType = "CrusaderShield";
                    else if (itemTypeName.contains("Mojo"))
                        item.ZetaType = "Mojo";
                    else if (itemTypeName.contains("Source"))
                        item.ZetaType = "Orb";
                    else if (itemTypeName.contains("Quiver"))
                        item.ZetaType = "Quiver";
                    else if (itemTypeName.contains("Focus"))
                        item.ZetaType = "FollowerSpecial";
                    else if (itemTypeName.contains("Token"))
                        item.ZetaType = "FollowerSpecial";
                    else if (itemTypeName.contains("Relic"))
                        item.ZetaType = "FollowerSpecial";
                    else if (itemTypeName.contains("Polearm"))
                        item.ZetaType = "Polearm";
                    else if (itemTypeName.contains("Staff"))
                        item.ZetaType = "Staff";
                    else if (itemTypeName.contains("Daibo"))
                        item.ZetaType = "Daibo";
                    else if (itemTypeName.contains("Axe"))
                        item.ZetaType = "Axe";
                    else if (itemTypeName.contains("Dagger"))
                        item.ZetaType = "Dagger";
                    else if (itemTypeName.contains("Mace"))
                        item.ZetaType = "Mace";
                    else if (itemTypeName.contains("Spear"))
                        item.ZetaType = "Spear";
                    else if (itemTypeName.contains("Sword"))
                        item.ZetaType = "Sword";
                    else if (itemTypeName.contains("Knife"))
                        item.ZetaType = "CeremonialDagger";
                    else if (itemTypeName.contains("Fist"))
                        item.ZetaType = "FistWeapon";
                    else if (itemTypeName.contains("Flail"))
                        item.ZetaType = "Flail";
                    else if (itemTypeName.contains("Mighty Weapon"))
                        item.ZetaType = "MightyWeapon";
                    else if (itemTypeName.contains("Hand Crossbow"))
                        item.ZetaType = "HandCrossbow";
                    else if (itemTypeName.contains("Crossbow"))
                        item.ZetaType = "Crossbow";
                    else if (itemTypeName.contains("Bow"))
                        item.ZetaType = "Bow";
                    else if (itemTypeName.contains("Wand"))
                        item.ZetaType = "Wand";
                    else 
                        item.ZetaType = "Unknown"

                    data[ItemTypeKey].push(item);
                });
	
                // handle callback
                callback(data[ItemTypeKey], itemTypeSlug);

            });

        }

        console.log("Initialized Skills Object!");

        return {
            Helm: function (callback) {
                $.isEmptyObject(data.Helm) ? request(ItemType.Helm, callback) : callback(data.Helm, ItemType.Helm);
            },
            SpiritStone: function (callback) {
                $.isEmptyObject(data.SpiritStone) ? request(ItemType.SpiritStone, callback) : callback(data.SpiritStone, ItemType.SpiritStone);
            },
            VoodooMask: function (callback) {
                $.isEmptyObject(data.VoodooMask) ? request(ItemType.VoodooMask, callback) : callback(data.VoodooMask, ItemType.VoodooMask);
            },
            WizardHat: function (callback) {
                $.isEmptyObject(data.WizardHat) ? request(ItemType.WizardHat, callback) : callback(data.WizardHat, ItemType.WizardHat);
            },
            Shoulders: function (callback) {
                $.isEmptyObject(data.Shoulders) ? request(ItemType.Shoulders, callback) : callback(data.Shoulders, ItemType.Shoulders);
            },
            Chest: function (callback) {
                $.isEmptyObject(data.Chest) ? request(ItemType.Chest, callback) : callback(data.Chest, ItemType.Chest);
            },
            Cloak: function (callback) {
                $.isEmptyObject(data.Cloak) ? request(ItemType.Cloak, callback) : callback(data.Cloak, ItemType.Cloak);
            },
            Bracers: function (callback) {
                $.isEmptyObject(data.Bracers) ? request(ItemType.Bracers, callback) : callback(data.Bracers, ItemType.Bracers);
            },
            Gloves: function (callback) {
                $.isEmptyObject(data.Gloves) ? request(ItemType.Gloves, callback) : callback(data.Gloves, ItemType.Gloves);
            },
            Belts: function (callback) {
                $.isEmptyObject(data.Belts) ? request(ItemType.Belts, callback) : callback(data.Belts, ItemType.Belts);
            },
            MightyBelt: function (callback) {
                $.isEmptyObject(data.MightyBelt) ? request(ItemType.MightyBelt, callback) : callback(data.MightyBelt, ItemType.MightyBelt);
            },
            Legs: function (callback) {
                $.isEmptyObject(data.Legs) ? request(ItemType.Legs, callback) : callback(data.Legs, ItemType.Legs);
            },
            Feet: function (callback) {
                $.isEmptyObject(data.Feet) ? request(ItemType.Feet, callback) : callback(data.Feet, ItemType.Feet);
            },
            Amulet: function (callback) {
                $.isEmptyObject(data.Amulet) ? request(ItemType.Amulet, callback) : callback(data.Amulet, ItemType.Amulet);
            },
            Rings: function (callback) {
                $.isEmptyObject(data.Rings) ? request(ItemType.Rings, callback) : callback(data.Rings, ItemType.Rings);
            },
            Shields: function (callback) {
                $.isEmptyObject(data.Shields) ? request(ItemType.Shields, callback) : callback(data.Shields, ItemType.Shields);
            },
            CrusaderShield: function (callback) {
                $.isEmptyObject(data.CrusaderShield) ? request(ItemType.CrusaderShield, callback) : callback(data.CrusaderShield, ItemType.CrusaderShield);
            },
            Mojos: function (callback) {
                $.isEmptyObject(data.Mojos) ? request(ItemType.Mojos, callback) : callback(data.Mojos, ItemType.Mojos);
            },
            Orbs: function (callback) {
                $.isEmptyObject(data.Orbs) ? request(ItemType.Orbs, callback) : callback(data.Orbs, ItemType.Orbs);
            },
            Quivers: function (callback) {
                $.isEmptyObject(data.Quivers) ? request(ItemType.Quivers, callback) : callback(data.Quivers, ItemType.Quivers);
            },
            //Focus: function (callback) {
            //    $.isEmptyObject(data.Focus) ? request(ItemType.Focus, callback) : callback(data.Focus, ItemType.Focus);
            //},
            //Token: function (callback) {
            //    $.isEmptyObject(data.Token) ? request(ItemType.Token, callback) : callback(data.Token, ItemType.Token);
            //},
            //Relic: function (callback) {
            //    $.isEmptyObject(data.Relic) ? request(ItemType.Relic, callback) : callback(data.Relic, ItemType.Relic);
            //},
            Axe1h: function (callback) {
                $.isEmptyObject(data.Axe1h) ? request(ItemType.Axe1h, callback) : callback(data.Axe1h, ItemType.Axe1h);
            },
            Dagger: function (callback) {
                $.isEmptyObject(data.Dagger) ? request(ItemType.Dagger, callback) : callback(data.Dagger, ItemType.Dagger);
            },
            Mace1h: function (callback) {
                $.isEmptyObject(data.Mace1h) ? request(ItemType.Mace1h, callback) : callback(data.Mace1h, ItemType.Mace1h);
            },
            Spears: function (callback) {
                $.isEmptyObject(data.Spears) ? request(ItemType.Spears, callback) : callback(data.Spears, ItemType.Spears);
            },
            Swords: function (callback) {
                $.isEmptyObject(data.Swords) ? request(ItemType.Swords, callback) : callback(data.Swords, ItemType.Swords);
            },
            Knives: function (callback) {
                $.isEmptyObject(data.Knives) ? request(ItemType.Knives, callback) : callback(data.Knives, ItemType.Knives);
            },
            FistWeapon: function (callback) {
                $.isEmptyObject(data.FistWeapon) ? request(ItemType.FistWeapon, callback) : callback(data.FistWeapon, ItemType.FistWeapon);
            },
            Flail1h: function (callback) {
                $.isEmptyObject(data.Flail1h) ? request(ItemType.Flail1h, callback) : callback(data.Flail1h, ItemType.Flail1h);
            },
            Mighty1h: function (callback) {
                $.isEmptyObject(data.Mighty1h) ? request(ItemType.Mighty1h, callback) : callback(data.Mighty1h, ItemType.Mighty1h);
            },
            Axe2h: function (callback) {
                $.isEmptyObject(data.Axe2h) ? request(ItemType.Axe2h, callback) : callback(data.Axe2h, ItemType.Axe2h);
            },
            Mace: function (callback) {
                $.isEmptyObject(data.Mace) ? request(ItemType.Mace, callback) : callback(data.Mace, ItemType.Mace);
            },
            Polearm: function (callback) {
                $.isEmptyObject(data.Polearm) ? request(ItemType.Polearm, callback) : callback(data.Polearm, ItemType.Polearm);
            },
            Staves: function (callback) {
                $.isEmptyObject(data.Staves) ? request(ItemType.Staves, callback) : callback(data.Staves, ItemType.Staves);
            },
            Sword2h: function (callback) {
                $.isEmptyObject(data.Sword2h) ? request(ItemType.Sword2h, callback) : callback(data.Sword2h, ItemType.Sword2h);
            },
            Diabo: function (callback) {
                $.isEmptyObject(data.Diabo) ? request(ItemType.Diabo, callback) : callback(data.Diabo, ItemType.Diabo);
            },
            Flail2h: function (callback) {
                $.isEmptyObject(data.Flail2h) ? request(ItemType.Flail2h, callback) : callback(data.Flail2h, ItemType.Flail2h);
            },
            Mighty2h: function (callback) {
                $.isEmptyObject(data.Mighty2h) ? request(ItemType.Mighty2h, callback) : callback(data.Mighty2h, ItemType.Mighty2h);
            },
            Bows: function (callback) {
                $.isEmptyObject(data.Bows) ? request(ItemType.Bows, callback) : callback(data.Bows, ItemType.Bows);
            },
            Crossbow: function (callback) {
                $.isEmptyObject(data.Crossbow) ? request(ItemType.Crossbow, callback) : callback(data.Crossbow, ItemType.Crossbow);
            },
            HandXBow: function (callback) {
                $.isEmptyObject(data.HandXBow) ? request(ItemType.HandXBow, callback) : callback(data.HandXBow, ItemType.HandXBow);
            },
            Wand: function (callback) {
                $.isEmptyObject(data.Wand) ? request(ItemType.Wand, callback) : callback(data.Wand, ItemType.Wand);
            },
        };

    }();
    
    var HandleItemData = function (onDataLoadFinished) {

        var todo = Object.keys(ItemData).length;

        var data = {
            timestamp: (new Date()).toUTCString(),
            All: [],
            Helm: [],
            SpiritStone: [],
            VoodooMask: [],
            WizardHat: [],
            Shoulders: [],
            Chest: [],
            Cloak: [],
            Bracers: [],
            Gloves: [],
            Helm: [],
            SpiritStone: [],
            VoodooMask: [],
            WizardHat: [],
            Shoulders: [],
            Chest: [],
            Cloak: [],
            Bracers: [],
            Gloves: [],
            Belts: [],
            MightyBelt: [],
            Legs: [],
            Feet: [],
            Amulet: [],
            Rings: [],
            Shields: [],
            CrusaderShield: [],
            Mojos: [],
            Orbs: [],
            Quivers: [],
            Focus: [],
            Token: [],
            Relic: [],
            Axe1h: [],
            Dagger: [],
            Mace1h: [],
            Spears: [],
            Swords: [],
            Knives: [],
            FistWeapon: [],
            Flail1h: [],
            Mighty1h: [],
            Axe2h: [],
            Mace: [],
            Polearm: [],
            Staves: [],
            Sword2h: [],
            Diabo: [],
            Flail2h: [],
            Mighty2h: [],
            Bows: [],
            Crossbow: [],
            HandXBow: [],
            Wand: []
        }

        // Gets all data (request or cached) and then passes it back to onDataLoadFinished 
        $.each(ItemData, function(index, process) {

            process(function (itemData, itemTypeName) {

                $.each(ItemType, function() {
                    if (this == itemTypeName)
                        data[itemTypeName] = itemData;
                });

                $.each(itemData, function() {
                    data.All.push(this);
                });

                todo--;
                if (todo == 0) {
                    onDataLoadFinished(data);
                }

            });

        });

    };



    return {
        HandleItemData: function (delegate) {
            return HandleItemData(delegate);
        },

    }

}();


var itemLookup = {
    "ageless-might": ["Ageless Might", 193675, "BarbBelt_norm_unique_08"],
    "ahavarion-spear-of-lycander": ["Ahavarion, Spear of Lycander", 271768, "staff_norm_unique_08"],
    "ambos-pride": ["Ambo's Pride", 193486, "mightyWeapon_1H_norm_unique_03"],
    "ancient-parthan-defenders": ["Ancient Parthan Defenders", 298116, "bracers_norm_unique_12"],
    "andariels-visage": ["Andariel's Visage", 198014, "helm_norm_unique_03"],
    "anessazi-edge": ["Anessazi Edge", 196250, "ceremonialDagger_norm_unique_04"],
    "angel-hair-braid": ["Angel Hair Braid", 193666, "belt_norm_unique_03"],
    "aquila-cuirass": ["Aquila Cuirass", 197203, "chestArmor_norm_unique_047"],
    "arcane-dust": ["Arcane Dust", 361985, "Crafting_Magic_05"],
    "archmages-vicalyke": ["Archmage's Vicalyke", 299471, "wizardhat_norm_unique_06"],
    "armor-of-the-kind-regent": ["Armor of the Kind Regent", 332202, "chestarmor_norm_unique_02"],
    "arreats-law": ["Arreat's Law", 191446, "spear_norm_unique_01"],
    "arthefs-spark-of-life": ["Arthef’s Spark of Life", 59633, "twoHandedMace_norm_unique_01"],
    "aughilds-power": ["Aughild's Power", 224051, "shoulderPads_norm_set_02"],
    "aughilds-rule": ["Aughild's Rule", 197193, "chestArmor_norm_unique_043"],
    "aughilds-search": ["Aughild's Search", 222972, "Bracers_norm_unique_09"],
    "aughilds-spike": ["Aughild's Spike", 223972, "Helm_norm_set_03"],
    "autumns-call": ["Autumn's Call", 184228, "Staff_norm_unique_03"],
    "avarice-band": ["Avarice Band", 298095, "ring_norm_unique_032"],
    "azurewrath": ["Azurewrath", 192511, "Sword_norm_unique_06"],
    "bakkan-caster": ["Bakkan Caster", 98163, "XBow_norm_unique_01"],
    "balefire-caster": ["Balefire Caster", 192528, "handXbow_norm_unique_02"],
    "baleful-remnant": ["Baleful Remnant", 299435, "flail2h_norm_unique_02"],
    "band-of-hollow-whispers": ["Band of Hollow Whispers", 197834, "Ring_norm_unique_001"],
    "band-of-untold-secrets": ["Band of Untold Secrets", 212602, "Ring_norm_unique_009"],
    "beckon-sail": ["Beckon Sail", 223150, "cloak_norm_unique_02"],
    "blackfeather": ["Blackfeather", 332206, "cloak_norm_unique_01"],
    "blackguard": ["Blackguard", 270979, "twohandedsword_norm_unique_10"],
    "blackhand-key": ["Blackhand Key", 193355, "Wand_norm_unique_06"],
    "blackthornes-duncraig-cross": ["Blackthorne's Duncraig Cross", 224189, "Amulet_norm_unique_16"],
    "blackthornes-jousting-mail": ["Blackthorne's Jousting Mail", 222477, "pants_norm_unique_050"],
    "blackthornes-notched-belt": ["Blackthorne's Notched Belt", 224191, "Belt_norm_unique_14"],
    "blackthornes-spurs": ["Blackthorne's Spurs", 222463, "Boots_norm_unique_050"],
    "blackthornes-surcoat": ["Blackthorne's Surcoat", 222456, "chestarmor_norm_unique_050"],
    "blade-of-prophecy": ["Blade of Prophecy", 184184, "twoHandedSword_norm_unique_03"],
    "blind-faith": ["Blind Faith", 197037, "Helm_norm_unique_07"],
    "boj-anglers": ["Boj Anglers", 197224, "Boots_norm_unique_045"],
    "bombadiers-rucksack": ["Bombadier's Rucksack", 298171, "quiver_norm_unique_09"],
    "borns-frozen-soul": ["Born's Frozen Soul", 197199, "chestArmor_norm_unique_044"],
    "bottomless-potion-of-kulle-aid": ["Bottomless Potion of Kulle-Aid", 344093, "healthPotion_Legendary_06_x1"],
    "bottomless-potion-of-mutilation": ["Bottomless Potion of Mutilation", 342824, "healthPotion_Legendary_05_x1"],
    "bottomless-potion-of-regeneration": ["Bottomless Potion of Regeneration", 341343, "healthPotion_Legendary_03_x1"],
    "bottomless-potion-of-the-diamond": ["Bottomless Potion of the Diamond", 341342, "healthPotion_Legendary_02_x1"],
    "bottomless-potion-of-the-leech": ["Bottomless Potion of the Leech", 342823, "healthPotion_Legendary_04_x1"],
    "bottomless-potion-of-the-tower": ["Bottomless Potion of the Tower", 341333, "healthPotion_Legendary_01_x1"],
    "bovine-bardiche": ["Bovine Bardiche", 272056, "polearm_norm_unique_05"],
    "breastplate-of-akkhan": ["Breastplate of Akkhan", 358796, "chestarmor_norm_set_10"],
    "broken-crown": ["Broken Crown", 220630, "Helm_norm_unique_02"],
    "broken-promises": ["Broken Promises", 212589, "Ring_norm_unique_006"],
    "bul-kathoss-solemn-vow": ["Bul-Kathos's Solemn Vow", 208771, "mightyWeapon_1H_norm_unique_05"],
    "bul-kathoss-warrior-blood": ["Bul-Kathos's Warrior Blood", 208775, "mightyweapon_1h_norm_unique_06"],
    "bul-kathoss-wedding-band": ["Bul-Kathos's Wedding Band", 212603, "Ring_norm_unique_020"],
    "burden-of-the-invoker": ["Burden of the Invoker", 335029, "shoulderpads_norm_set_12"],
    "butchers-carver": ["Butcher's Carver", 186494, "twoHandedAxe_norm_unique_03"],
    "cains-habit": ["Cain's Habit", 197218, "pants_norm_unique_046"],
    "cains-travelers": ["Cain's Travelers", 197225, "Boots_norm_unique_046"],
    "cape-of-the-dark-night": ["Cape of the Dark Night", 223149, "cloak_norm_unique_01"],
    "captain-crimsons-silk-girdle": ["Captain Crimson's Silk Girdle", 222974, "Belt_norm_unique_12"],
    "captain-crimsons-thrust": ["Captain Crimson's Thrust", 197214, "pants_norm_unique_043"],
    "captain-crimsons-waders": ["Captain Crimson's Waders", 197221, "Boots_norm_unique_043"],
    "carnevil": ["Carnevil", 299442, "voodoomask_norm_unique_07"],
    "chaingmail": ["Chaingmail", 197204, "chestArmor_norm_unique_048"],
    "chantodos-force": ["Chantodo's Force", 212277, "orb_norm_unique_05"],
    "chantodos-will": ["Chantodo's Will", 210479, "Wand_norm_unique_07"],
    "chilanik-s-chain": ["Chilanik’s Chain", 298133, "barbbelt_norm_unique_10"],
    "cindercoat": ["Cindercoat", 222455, "chestArmor_norm_unique_049"],
    "cloak-of-deception": ["Cloak of Deception", 332208, "cloak_norm_unique_02"],
    "cluckeye": ["Cluckeye", 175582, "Bow_norm_unique_03"],
    "cord-of-the-sherma": ["Cord of the Sherma", 298127, "belt_norm_unique_18"],
    "countess-julias-cameo": ["Countess Julia's Cameo", 298050, "amulet_norm_unique_19"],
    "coven-s-criterion": ["Coven’s Criterion", 298191, "shield_norm_unique_15"],
    "crown-of-the-invoker": ["Crown of the Invoker", 335028, "helm_norm_set_12"],
    "crushbane": ["Crushbane", 99227, "twohandedmace_norm_unique_02"],
    "cuisses-of-akkhan": ["Cuisses of Akkhan", 358800, "pants_norm_set_10"],
    "custerian-wristguards": ["Custerian Wristguards", 298122, "bracers_norm_unique_17"],
    "danettas-revenge": ["Danetta's Revenge", 211749, "handXbow_norm_unique_07"],
    "danettas-spite": ["Danetta's Spite", 211745, "handXbow_norm_unique_06"],
    "dark-mages-shade": ["Dark Mage's Shade", 224908, "wizardHat_norm_unique_05"],
    "darklight": ["Darklight", 299428, "flail1h_norm_unique_06"],
    "dawn": ["Dawn", 196409, "handXbow_norm_unique_04"],
    "dead-mans-legacy": ["Dead Man's Legacy", 197630, "Quiver_norm_unique_07"],
    "deadly-rebirth": ["Deadly Rebirth", 193433, "ceremonialDagger_norm_unique_02"],
    "death-watch-mantle": ["Death Watch Mantle", 200310, "shoulderpads_norm_unique_02"],
    "deaths-bargain": ["Death's Bargain", 332205, "pants_norm_unique_02"],
    "deaths-breath": ["Death's Breath", 361989, "Crafting_Looted_Reagent_05"],
    "deathseers-cowl": ["Deathseer's Cowl", 298146, "helm_norm_unique_14"],
    "defender-of-westmarch": ["Defender of Westmarch", 298182, "shield_norm_unique_09"],
    "demon-machine": ["Demon Machine", 222286, "XBow_norm_unique_07"],
    "denial": ["Denial", 152666, "shield_norm_unique_03"],
    "depth-diggers": ["Depth Diggers", 197216, "pants_norm_unique_044"],
    "devastator": ["Devastator", 188177, "Mace_norm_unique_06"],
    "devil-tongue": ["Devil Tongue", 189552, "Sword_norm_unique_05"],
    "doombringer": ["Doombringer", 185397, "Sword_norm_unique_07"],
    "dovu-energy-trap": ["Dovu Energy Trap", 298054, "amulet_norm_unique_23"],
    "dread-iron": ["Dread Iron", 193672, "BarbBelt_norm_unique_01"],
    "eberli-charo": ["Eberli Charo", 298186, "shield_norm_unique_10"],
    "echoing-fury": ["Echoing Fury", 188181, "Mace_norm_unique_07"],
    "eight-demon-boots": ["Eight-Demon Boots", 338031, "boots_norm_set_08"],
    "emimei-s-duffel": ["Emimei’s Duffel", 298172, "quiver_norm_unique_10"],
    "empyrean-messenger": ["Empyrean Messenger", 194241, "Spear_norm_unique_02"],
    "enchanting-favor": ["Enchanting Favor", 366968, "followeritem_templar_legendary_01"],
    "eternal-union": ["Eternal Union", 212601, "ring_norm_unique_007"],
    "etrayu": ["Etrayu", 175581, "Bow_norm_unique_02"],
    "exarian": ["Exarian", 271617, "sword_norm_unique_13"],
    "eye-of-etlich": ["Eye of Etlich", 197823, "Amulet_norm_unique_12"],
    "eyes-of-the-earth": ["Eyes of the Earth", 340528, "helm_norm_set_15"],
    "faithful-memory": ["Faithful Memory", 198960, "twoHandedSword_norm_unique_09"],
    "fate-of-the-fell": ["Fate of the Fell", 299436, "flail2h_norm_unique_03"],
    "fire-walkers": ["Fire Walkers", 205624, "Boots_norm_unique_085"],
    "firebirds-breast": ["Firebird's Breast", 358788, "chestarmor_norm_set_06"],
    "firebirds-down": ["Firebird's Down", 358790, "pants_norm_set_06"],
    "firebirds-eye": ["Firebird's Eye", 358819, "orb_norm_set_06"],
    "firebirds-pinions": ["Firebird's Pinions", 358792, "shoulderpads_norm_set_06"],
    "firebirds-plume": ["Firebird's Plume", 358791, "helm_norm_set_06"],
    "firebirds-talons": ["Firebird's Talons", 358789, "gloves_norm_set_06"],
    "firebirds-tarsi": ["Firebird's Tarsi", 358793, "boots_norm_set_06"],
    "fjord-cutter": ["Fjord Cutter", 192105, "mightyWeapon_1H_norm_unique_01"],
    "fleeting-strap": ["Fleeting Strap", 193667, "Belt_norm_unique_04"],
    "flesh-tearer": ["Flesh Tearer", 116388, "Axe_norm_unique_03"],
    "flint-ripper-arrowheads": ["Flint Ripper Arrowheads", 197624, "Quiver_norm_unique_01"],
    "focus": ["Focus", 332209, "ring_norm_set_001"],
    "forgotten-soul": ["Forgotten Soul", 361988, "Crafting_Legendary_05"],
    "fragment-of-destiny": ["Fragment of Destiny", 181995, "Wand_norm_unique_02"],
    "freeze-of-deflection": ["Freeze of Deflection", 61550, "Shield_norm_unique_01"],
    "frostburn": ["Frostburn", 197205, "Gloves_norm_unique_043"],
    "fulminator": ["Fulminator", 271631, "sword_norm_unique_15"],
    "gauntlets-of-akkhan": ["Gauntlets of Akkhan", 358798, "gloves_norm_set_10"],
    "gazing-demise": ["Gazing Demise", 194995, "Mojo_norm_unique_05"],
    "genzaniku": ["Genzaniku", 116386, "Axe_norm_unique_01"],
    "gesture-of-orpheus": ["Gesture of Orpheus", 182071, "Wand_norm_unique_03"],
    "gift-of-silaria": ["Gift of Silaria", 271630, "sword_norm_unique_14"],
    "girdle-of-giants": ["Girdle of Giants", 212232, "BarbBelt_norm_unique_04"],
    "gladiator-gauntlets": ["Gladiator Gauntlets", 205635, "gloves_norm_unique_090"],
    "gloves-of-worship": ["Gloves of Worship", 332344, "gloves_norm_unique_03"],
    "golden-gorget-of-leoric": ["Golden Gorget of Leoric", 298052, "amulet_norm_unique_21"],
    "goldskin": ["Goldskin", 205616, "chestarmor_norm_unique_089"],
    "goldwrap": ["Goldwrap", 193671, "Belt_norm_unique_08"],
    "gungdo-gear": ["Gungdo Gear", 193688, "bracers_norm_unique_06"],
    "hack": ["Hack", 271598, "axe_norm_unique_09"],
    "halcyons-ascent": ["Halcyon's Ascent", 298056, "amulet_norm_unique_25"],
    "hallowed-bulwark": ["Hallowed Bulwark", 299413, "crushield_norm_unique_03"],
    "hammer-jammers": ["Hammer Jammers", 209059, "pants_norm_unique_077"],
    "hand-of-the-prophet": ["Hand of the Prophet", 366980, "followeritem_enchantress_legendary_02"],
    "harrington-waistguard": ["Harrington Waistguard", 298129, "belt_norm_unique_19"],
    "haunt-of-vaxo": ["Haunt of Vaxo", 297806, "amulet_norm_unique_17"],
    "health-potion": ["Health Potion", 304319, "healthPotion_Console"],
    "heart-of-iron": ["Heart of Iron", 205607, "chestArmor_norm_unique_074"],
    "heart-of-the-crashing-wave": ["Heart of the Crashing Wave", 338032, "chestarmor_norm_set_08"],
    "heart-slaughter": ["Heart Slaughter", 192569, "Polearm_norm_unique_02"],
    "hellcat-waistguard": ["Hellcat Waistguard", 193668, "belt_norm_unique_05"],
    "hellfire-amulet": ["Hellfire Amulet", 298057, "x1_Amulet_norm_unique_26"],
    "hellfire-ring": ["Hellfire Ring", 260327, "Ring_norm_unique_024"],
    "hellrack": ["Hellrack", 192836, "XBow_norm_unique_02"],
    "hellskull": ["Hellskull", 299415, "crushield_norm_unique_05"],
    "helltooth-gauntlets": ["Helltooth Gauntlets", 363094, "gloves_norm_set_16"],
    "helltooth-greaves": ["Helltooth Greaves", 340524, "boots_norm_set_16"],
    "helltooth-leg-guards": ["Helltooth Leg Guards", 340522, "pants_norm_set_16"],
    "helltooth-mantle": ["Helltooth Mantle", 340525, "shoulderpads_norm_set_16"],
    "helltooth-tunic": ["Helltooth Tunic", 363088, "chestarmor_norm_set_16"],
    "helltrapper": ["Helltrapper", 271914, "handxbow_norm_unique_11"],
    "helm-of-akkhan": ["Helm of Akkhan", 358799, "helm_norm_set_10"],
    "hexing-pants-of-mr-yan": ["Hexing Pants of Mr. Yan", 332204, "pants_norm_unique_01"],
    "holy-point-shot": ["Holy Point Shot", 197627, "quiver_norm_unique_04"],
    "homing-pads": ["Homing Pads", 198573, "shoulderpads_norm_unique_01"],
    "homunculus": ["Homunculus", 194991, "Mojo_norm_unique_04"],
    "horadric-cache": ["Horadric Cache", 360166, "HoradricCacheA1"],
    "hwoj-wrap": ["Hwoj Wrap", 298131, "belt_norm_unique_21"],
    "ice-climbers": ["Ice Climbers", 222464, "Boots_norm_unique_051"],
    "illusory-boots": ["Illusory Boots", 332342, "boots_norm_unique_03"],
    "immortal-kings-boulder-breaker": ["Immortal King's Boulder Breaker", 210678, "mightyWeapon_2H_norm_unique_10"],
    "immortal-kings-eternal-reign": ["Immortal King's Eternal Reign", 205613, "chestArmor_norm_unique_086"],
    "immortal-kings-irons": ["Immortal King's Irons", 205631, "Gloves_norm_unique_086"],
    "immortal-kings-stride": ["Immortal King's Stride", 205625, "Boots_norm_unique_086"],
    "immortal-kings-tribal-binding": ["Immortal King's Tribal Binding", 212235, "BarbBelt_norm_unique_09"],
    "immortal-kings-triumph": ["Immortal King's Triumph", 210265, "Helm_norm_unique_08"],
    "innas-favor": ["Inna's Favor", 222487, "Belt_norm_unique_10"],
    "innas-radiance": ["Inna's Radiance", 222307, "spiritStone_norm_unique_08"],
    "innas-temperance": ["Inna's Temperance", 205646, "pants_norm_unique_087"],
    "innas-vast-expanse": ["Inna's Vast Expanse", 205614, "chestArmor_norm_unique_087"],
    "insatiable-belt": ["Insatiable Belt", 298126, "belt_norm_unique_17"],
    "inviolable-faith": ["Inviolable Faith", 299429, "flail1h_norm_unique_07"],
    "irontoe-mudsputters": ["Irontoe Mudsputters", 339125, "boots_norm_unique_04"],
    "ivory-tower": ["Ivory Tower", 197478, "Shield_norm_unique_08"],
    "izzuccob": ["Izzuccob", 192467, "handXbow_norm_unique_01"],
    "jade-harvesters-courage": ["Jade Harvester's Courage", 338041, "pants_norm_set_09"],
    "jade-harvesters-joy": ["Jade Harvester's Joy", 338042, "shoulderpads_norm_set_09"],
    "jade-harvesters-mercy": ["Jade Harvester's Mercy", 338039, "gloves_norm_set_09"],
    "jade-harvesters-peace": ["Jade Harvester's Peace", 338038, "chestarmor_norm_set_09"],
    "jade-harvesters-swiftness": ["Jade Harvester's Swiftness", 338037, "boots_norm_set_09"],
    "jade-harvesters-wisdom": ["Jade Harvester's Wisdom", 338040, "helm_norm_set_09"],
    "jang-s-envelopment-": ["Jang’s Envelopment ", 298130, "belt_norm_unique_20"],
    "jekangbord": ["Jekangbord", 299412, "crushield_norm_unique_02"],
    "justice-lantern": ["Justice Lantern", 212590, "ring_norm_unique_008"],
    "justinians-mercy": ["Justinian's Mercy", 299424, "flail1h_norm_unique_02"],
    "kassars-retribution": ["Kassar's Retribution", 299426, "flail1h_norm_unique_04"],
    "kill": ["Kill", 192579, "Dagger_norm_unique_02"],
    "kotuurs-brace": ["Kotuur's Brace", 193674, "BarbBelt_norm_unique_07"],
    "krede-s-flame": ["Krede’s Flame", 197836, "Ring_norm_unique_003"],
    "kredes-flame": ["Krede's Flame", 197836, "Ring_norm_unique_003"],
    "krelms-buff-belt": ["Krelm's Buff Belt", 336184, "belt_norm_set_02"],
    "kymbos-gold": ["Kymbo's Gold", 197812, "Amulet_norm_unique_02"],
    "lacuni-prowlers": ["Lacuni Prowlers", 193687, "Bracers_norm_unique_05"],
    "lamentation": ["Lamentation", 212234, "barbbelt_norm_unique_05"],
    "last-breath": ["Last Breath", 195370, "ceremonialDagger_norm_unique_03"],
    "leorics-crown": ["Leoric's Crown", 196024, "Helm_norm_unique_01"],
    "leorics-signet": ["Leoric's Signet", 197835, "Ring_norm_unique_002"],
    "lidless-wall": ["Lidless Wall", 195389, "Shield_norm_unique_07"],
    "light-of-grace": ["Light of Grace", 272038, "orb_norm_unique_09"],
    "litany-of-the-undaunted": ["Litany of the Undaunted", 212651, "ring_norm_unique_015"],
    "lut-socks": ["Lut Socks", 205622, "Boots_norm_unique_077"],
    "mad-monarchs-scepter": ["Mad Monarch's Scepter", 271663, "mace_norm_unique_12"],
    "madawcs-sorrow": ["Madawc's Sorrow", 272012, "mightyweapon_2h_norm_unique_11"],
    "magefist": ["Magefist", 197206, "Gloves_norm_unique_044"],
    "maloths-focus": ["Maloth's Focus", 193832, "Staff_norm_unique_06"],
    "manajumas-carving-knife": ["Manajuma's Carving Knife", 223365, "ceremonialDagger_norm_unique_06"],
    "manajumas-gory-fetch": ["Manajuma's Gory Fetch", 210993, "Mojo_norm_unique_06"],
    "manald-heal": ["Manald Heal", 212546, "Ring_norm_unique_021"],
    "manticore": ["Manticore", 221760, "XBow_norm_unique_06"],
    "mantle-of-the-upside-down-sinners": ["Mantle of the Upside-Down Sinners", 338036, "shoulderpads_norm_set_08"],
    "maras-kaleidoscope": ["Mara's Kaleidoscope", 197824, "Amulet_norm_unique_13"],
    "marauders-carapace": ["Marauder's Carapace", 363803, "chestarmor_norm_set_07"],
    "marauders-encasement": ["Marauder's Encasement", 336993, "pants_norm_set_07"],
    "marauders-gloves": ["Marauder's Gloves", 336992, "gloves_norm_set_07"],
    "marauders-spines": ["Marauder's Spines", 336996, "shoulderpads_norm_set_07"],
    "marauders-treads": ["Marauder's Treads", 336995, "boots_norm_set_07"],
    "marauders-visage": ["Marauder's Visage", 336994, "helm_norm_set_07"],
    "marquise-amethyst": ["Marquise Amethyst", 283116, "Amethyst_15"],
    "marquise-diamond": ["Marquise Diamond", 361559, "Diamond_15"],
    "marquise-emerald": ["Marquise Emerald", 283117, "Emerald_15"],
    "marquise-ruby": ["Marquise Ruby", 283118, "Ruby_15"],
    "marquise-topaz": ["Marquise Topaz", 283119, "Topaz_15"],
    "mask-of-jeram": ["Mask of Jeram", 369016, "helm_norm_set_16"],
    "mask-of-jeram": ["Mask of Jeram", 299443, "voodoomask_norm_unique_08"],
    "mask-of-the-searing-sky": ["Mask of the Searing Sky", 338034, "helm_norm_set_08"],
    "maximus": ["Maximus", 184187, "twoHandedSword_norm_unique_04"],
    "mempo-of-twilight": ["Mempo of Twilight", 223577, "Helm_norm_unique_12"],
    "messerschmidts-reaver": ["Messerschmidt's Reaver", 191065, "twoHandedAxe_norm_unique_04"],
    "mirrorball": ["Mirrorball", 272022, "orb_norm_unique_07"],
    "monster-hunter": ["Monster Hunter", 115140, "Sword_norm_unique_01"],
    "moonlight-ward": ["Moonlight Ward", 197813, "Amulet_norm_unique_03"],
    "mykens-ball-of-hate": ["Myken's Ball of Hate", 272037, "orb_norm_unique_08"],
    "nagelring": ["Nagelring", 212586, "Ring_norm_unique_018"],
    "nailbiter": ["Nailbiter", 188158, "Mace_norm_unique_02"],
    "natalyas-bloody-footprints": ["Natalya's Bloody Footprints", 197223, "Boots_norm_unique_044"],
    "natalyas-embrace": ["Natalya's Embrace", 208934, "Cloak_norm_set_03"],
    "natalyas-reflection": ["Natalya's Reflection", 212545, "Ring_norm_unique_011"],
    "natalyas-sight": ["Natalya's Sight", 210851, "helm_norm_unique_09"],
    "natalyas-slayer": ["Natalya's Slayer", 210874, "handXbow_norm_unique_05"],
    "neanderthal": ["Neanderthal", 102665, "Mace_norm_unique_01"],
    "nemesis-bracers": ["Nemesis Bracers", 298121, "bracers_norm_unique_16"],
    "nutcracker": ["Nutcracker", 188169, "Mace_norm_unique_03"],
    "obsidian-ring-of-the-zodiac": ["Obsidian Ring of the Zodiac", 212588, "Ring_norm_unique_023"],
    "oculus-ring": ["Oculus Ring", 212648, "ring_norm_unique_017"],
    "odyn-son": ["Odyn Son", 188185, "Mace_norm_unique_08"],
    "ouroboros": ["Ouroboros", 197815, "Amulet_norm_unique_05"],
    "pandemonium-loop": ["Pandemonium Loop", 298096, "ring_norm_unique_033"],
    "pauldrons-of-akkhan": ["Pauldrons of Akkhan", 358801, "shoulderpads_norm_set_10"],
    "pauldrons-of-the-skeleton-king": ["Pauldrons of the Skeleton King", 298164, "shoulderpads_norm_unique_11"],
    "pig-sticker": ["Pig Sticker", 221313, "dagger_norm_unique_06"],
    "pledge-of-caldeum": ["Pledge of Caldeum", 196570, "Polearm_norm_unique_04"],
    "pox-faulds": ["Pox Faulds", 197220, "pants_norm_unique_048"],
    "pride-of-the-invoker": ["Pride of the Invoker", 335027, "gloves_norm_set_12"],
    "prides-fall": ["Pride's Fall", 298147, "helm_norm_unique_15"],
    "profane-pauldrons": ["Profane Pauldrons", 298158, "shoulderpads_norm_unique_08"],
    "promise-of-glory": ["Promise of Glory", 193684, "Bracers_norm_unique_02"],
    "pull-of-the-earth": ["Pull of the Earth", 340523, "gloves_norm_set_15"],
    "pus-spitter": ["Pus Spitter", 204874, "XBow_norm_unique_05"],
    "puzzle-ring": ["Puzzle Ring", 197837, "Ring_norm_unique_004"],
    "quetzalcoatl": ["Quetzalcoatl", 204136, "voodooMask_norm_base_05"],
    "raekors-breeches": ["Raekor’s Breeches", 336986, "pants_norm_set_05"],
    "raekors-burden": ["Raekor’s Burden", 336989, "shoulderpads_norm_set_05"],
    "raekors-heart": ["Raekor’s Heart", 336984, "chestarmor_norm_set_05"],
    "raekors-striders": ["Raekor’s Striders", 336987, "boots_norm_set_05"],
    "raekors-will": ["Raekor’s Will", 336988, "helm_norm_set_05"],
    "raekors-wraps": ["Raekor’s Wraps", 336985, "gloves_norm_set_05"],
    "rakoffs-glass-of-life": ["Rakoff's Glass of Life", 298055, "amulet_norm_unique_24"],
    "razor-strop": ["Razor Strop", 298124, "belt_norm_unique_15"],
    "reapers-wraps": ["Reaper's Wraps", 298118, "bracers_norm_unique_13"],
    "rechels-ring-of-larceny": ["Rechel's Ring of Larceny", 298091, "ring_norm_unique_028"],
    "relic-of-akarat": ["Relic of Akarat", 366969, "followeritem_templar_legendary_02"],
    "restraint": ["Restraint", 332210, "ring_norm_set_002"],
    "reusable-parts": ["Reusable Parts", 361984, "Crafting_AssortedParts_05"],
    "rhen'ho-flayer": ["Rhen'ho Flayer", 271745, "ceremonialdagger_norm_unique_11"],
    "ribald-etchings": ["Ribald Etchings", 366971, "followeritem_scoundrel_legendary_02"],
    "rift-keystone-fragment": ["Rift Keystone Fragment", 323722, "LootRunKey"],
    "rimeheart": ["Rimeheart", 271636, "sword_norm_unique_20"],
    "ring-of-royal-grandeur": ["Ring of Royal Grandeur", 298094, "ring_norm_unique_031"],
    "rogars-huge-stone": ["Rogar's Huge Stone", 298090, "ring_norm_unique_027"],
    "rondals-locket": ["Rondal's Locket", 197818, "Amulet_norm_unique_07"],
    "sabatons-of-akkhan": ["Sabatons of Akkhan", 358795, "boots_norm_set_10"],
    "saffron-wrap": ["Saffron Wrap", 193664, "belt_norm_unique_01"],
    "salvation": ["Salvation", 299418, "crushield_norm_unique_08"],
    "sanguinary-vambraces": ["Sanguinary Vambraces", 298120, "bracers_norm_unique_15"],
    "sash-of-knives": ["Sash of Knives", 298125, "belt_norm_unique_16"],
    "scales-of-the-dancing-serpent": ["Scales of the Dancing Serpent", 338035, "pants_norm_set_08"],
    "schaefers-hammer": ["Schaefer's Hammer", 197717, "twohandedmace_norm_unique_07"],
    "scourge": ["Scourge", 181511, "twoHandedSword_norm_unique_06"],
    "scrimshaw": ["Scrimshaw", 197095, "Spear_norm_unique_04"],
    "sebor-s-nightmare": ["Sebor’s Nightmare", 299381, "belt_norm_unique_22"],
    "sebors-nightmare": ["Sebor's Nightmare", 299381, "belt_norm_unique_22"],
    "serpents-sparker": ["Serpent's Sparker", 272084, "wand_norm_unique_02"],
    "sever": ["Sever", 115141, "sword_norm_unique_02"],
    "shackles-of-the-invoker": ["Shackles of the Invoker", 335030, "bracers_norm_set_12"],
    "shard-of-hate": ["Shard of Hate", 376463, "sword_norm_promo_02"],
    "shenlongs-fist-of-legend": ["Shenlong's Fist of Legend", 208996, "fistWeapon_norm_unique_12"],
    "shi-mizus-haori": ["Shi Mizu's Haori", 332200, "chestarmor_norm_unique_01"],
    "silver-star-piercers": ["Silver Star Piercers", 197628, "quiver_norm_unique_05"],
    "sin-seekers": ["Sin Seekers", 197625, "quiver_norm_unique_02"],
    "skeleton-key": ["Skeleton Key", 366970, "followeritem_scoundrel_legendary_01"],
    "skorn": ["Skorn", 192887, "twohandedaxe_norm_unique_05"],
    "skull-grasp": ["Skull Grasp", 212618, "Ring_norm_unique_022"],
    "skull-of-resonance": ["Skull of Resonance", 220549, "Helm_norm_unique_04"],
    "sky-splitter": ["Sky Splitter", 116389, "Axe_norm_unique_04"],
    "skycutter": ["Skycutter", 182347, "Sword_norm_unique_04"],
    "skywarden": ["Skywarden", 190840, "twoHandedMace_norm_unique_03"],
    "slave-bonds": ["Slave Bonds", 193685, "Bracers_norm_unique_03"],
    "sledge-of-athskeleng": ["Sledge of Athskeleng", 190866, "twoHandedMace_norm_unique_04"],
    "sloraks-madness": ["Slorak's Madness", 181982, "Wand_norm_unique_01"],
    "smoking-thurible": ["Smoking Thurible", 366979, "followeritem_enchantress_legendary_01"],
    "solanium": ["Solanium", 271662, "mace_norm_unique_11"],
    "soulsmasher": ["Soulsmasher", 271671, "twohandedmace_norm_unique_09"],
    "spaulders-of-zakara": ["Spaulders of Zakara", 298163, "shoulderpads_norm_unique_09"],
    "spines-of-seething-hatred": ["Spines of Seething Hatred", 197628, "Quiver_norm_unique_05"],
    "spires-of-the-earth": ["Spires of the Earth", 340526, "shoulderpads_norm_set_15"],
    "split-tusk": ["Split Tusk", 221167, "voodooMask_norm_unique_03"],
    "squirts-necklace": ["Squirt's Necklace", 197819, "Amulet_norm_unique_08"],
    "st-archews-gage": ["St. Archew's Gage", 332172, "gloves_norm_unique_01"],
    "stalgards-decimator": ["Stalgard's Decimator", 271639, "twohandedsword_norm_unique_11"],
    "standoff": ["Standoff", 191570, "Polearm_norm_unique_01"],
    "starmetal-kukri": ["Starmetal Kukri", 271738, "ceremonialdagger_norm_unique_10"],
    "steady-strikers": ["Steady Strikers", 193686, "bracers_norm_unique_04"],
    "stolen-ring": ["Stolen Ring", 197839, "Ring_norm_unique_005"],
    "stone-gauntlets": ["Stone Gauntlets", 205640, "gloves_norm_unique_076"],
    "stone-of-jordan": ["Stone of Jordan", 212582, "Ring_norm_unique_019"],
    "storm-crow": ["Storm Crow", 220694, "wizardHat_norm_unique_04"],
    "stormshield": ["Stormshield", 192484, "Shield_norm_unique_06"],
    "string-of-ears": ["String of Ears", 193669, "Belt_norm_unique_06"],
    "strongarm-bracers": ["Strongarm Bracers", 193692, "Bracers_norm_unique_07"],
    "sublime-conviction": ["Sublime Conviction", 299416, "crushield_norm_unique_06"],
    "sun-keeper": ["Sun Keeper", 188173, "Mace_norm_unique_04"],
    "sunwukos-shines": ["Sunwuko's Shines", 336174, "amulet_norm_set_11"],
    "swamp-land-waders": ["Swamp Land Waders", 209057, "pants_norm_unique_075"],
    "tal-rashas-allegiance": ["Tal Rasha's Allegiance", 222486, "amulet_norm_unique_14"],
    "tal-rashas-brace": ["Tal Rasha's Brace", 212657, "Belt_norm_unique_09"],
    "tal-rashas-guise-of-wisdom": ["Tal Rasha's Guise of Wisdom", 211531, "Helm_norm_unique_10"],
    "tal-rashas-relentless-pursuit": ["Tal Rasha's Relentless Pursuit", 211626, "chestArmor_norm_set_01"],
    "tal-rashas-unwavering-glare": ["Tal Rasha's Unwavering Glare", 212780, "orb_norm_unique_06"],
    "talisman-of-aranoch": ["Talisman of Aranoch", 197821, "Amulet_norm_unique_10"],
    "tasker-and-theo": ["Tasker and Theo", 205642, "Gloves_norm_unique_078"],
    "telrandens-hand": ["Telranden's Hand", 188189, "mace_norm_unique_09"],
    "the-ancient-bonesaber-of-zumakalis": ["The Ancient Bonesaber of Zumakalis", 194481, "sword_norm_unique_08"],
    "the-barber": ["The Barber", 195174, "Dagger_norm_unique_03"],
    "the-broken-staff": ["The Broken Staff", 59601, "Staff_norm_unique_01"],
    "the-burning-axe-of-sankis": ["The Burning Axe of Sankis", 181484, "Axe_norm_unique_05"],
    "the-butchers-sickle": ["The Butcher's Sickle", 189973, "axe_norm_unique_06"],
    "the-cloak-of-the-garwulf": ["The Cloak of the Garwulf", 223151, "Cloak_norm_unique_03"],
    "the-compass-rose": ["The Compass Rose", 212587, "Ring_norm_unique_013"],
    "the-crudest-boots": ["The Crudest Boots", 205620, "Boots_norm_unique_075"],
    "the-ess-of-johan": ["The Ess of Johan", 298051, "amulet_norm_unique_20"],
    "the-executioner": ["The Executioner", 186560, "twoHandedAxe_norm_unique_02"],
    "the-eye-of-the-storm": ["The Eye of the Storm", 222170, "spiritStone_norm_unique_02"],
    "the-final-witness": ["The Final Witness", 299417, "crushield_norm_unique_07"],
    "the-flavor-of-time": ["The Flavor of Time", 193659, "Amulet_norm_unique_01"],
    "the-gavel-of-judgment": ["The Gavel of Judgment", 193657, "mightyWeapon_2H_norm_unique_01"],
    "the-gidbinn": ["The Gidbinn", 209246, "ceremonialDagger_norm_unique_09"],
    "the-grand-vizier": ["The Grand Vizier", 192167, "Staff_norm_unique_04"],
    "the-grandfather": ["The Grandfather", 190360, "twoHandedSword_norm_unique_08"],
    "the-grin-reaper": ["The Grin Reaper", 221166, "voodooMask_norm_unique_01"],
    "the-laws-of-seph": ["The Laws of Seph", 299454, "spiritstone_norm_unique_14"],
    "the-magistrate": ["The Magistrate", 325579, "wizardhat_norm_unique_08"],
    "the-minds-eye": ["The Mind's Eye", 222172, "spiritStone_norm_unique_06"],
    "the-mortal-drama": ["The Mortal Drama", 299431, "flail2h_norm_unique_01"],
    "the-ninth-cirri-satchel": ["The Ninth Cirri Satchel", 298170, "quiver_norm_unique_08"],
    "the-oculus": ["The Oculus", 192320, "orb_norm_unique_02"],
    "the-ravens-wing": ["The Raven's Wing", 221938, "Bow_norm_unique_07"],
    "the-shadow-s-bane": ["The Shadow’s Bane", 332359, "chestarmor_norm_set_14"],
    "the-shadow-s-coil": ["The Shadow’s Coil", 332361, "pants_norm_set_14"],
    "the-shadow-s-grasp": ["The Shadow’s Grasp", 332362, "gloves_norm_set_14"],
    "the-shadows-grasp": ["The Shadow's Grasp", 332362, "gloves_norm_set_14"],
    "the-spider-queen-s-grasp": ["The Spider Queen’s Grasp", 222978, "ceremonialDagger_norm_unique_05"],
    "the-sultan-of-blinding-sand": ["The Sultan of Blinding Sand", 184190, "twohandedsword_norm_unique_05"],
    "the-swami": ["The Swami", 218681, "wizardHat_norm_unique_03"],
    "the-tall-mans-finger": ["The Tall Man's Finger", 298088, "ring_norm_unique_025"],
    "the-three-hundredth-spear": ["The Three Hundredth Spear", 196638, "Spear_norm_unique_03"],
    "the-tormentor": ["The Tormentor", 193066, "Staff_norm_unique_05"],
    "the-travelers-pledge": ["The Traveler's Pledge", 222490, "amulet_norm_unique_15"],
    "the-undisputed-champion": ["The Undisputed Champion", 193676, "BarbBelt_norm_unique_06"],
    "the-wailing-host": ["The Wailing Host", 212650, "Ring_norm_unique_014"],
    "the-witching-hour": ["The Witching Hour", 193670, "Belt_norm_unique_07"],
    "the-zweihander": ["The Zweihander", 59665, "twohandedsword_norm_unique_01"],
    "thing-of-the-deep": ["Thing of the Deep", 192468, "Mojo_norm_unique_02"],
    "thunderfury-blessed-blade-of-the-windseeker": ["Thunderfury, Blessed Blade of the Windseeker", 229716, "sword_norm_unique_12"],
    "thundergods-vigor": ["Thundergod's Vigor", 212230, "barbbelt_norm_unique_03"],
    "tiklandian-visage": ["Tiklandian Visage", 221382, "voodooMask_norm_unique_06"],
    "tragoul-coils": ["Trag'Oul Coils", 298119, "bracers_norm_unique_14"],
    "triumvirate": ["Triumvirate", 195325, "orb_norm_unique_04"],
    "tyraels-might": ["Tyrael's Might", 205608, "chestarmor_norm_unique_075"],
    "uhkapian-serpent": ["Uhkapian Serpent", 191278, "Mojo_norm_unique_01"],
    "unity": ["Unity", 212581, "Ring_norm_unique_010"],
    "uskang": ["Uskang", 175580, "Bow_norm_unique_01"],
    "veiled-crystal": ["Veiled Crystal", 361986, "Crafting_Rare_05"],
    "vigilante-belt": ["Vigilante Belt", 193665, "Belt_norm_unique_02"],
    "vile-ward": ["Vile Ward", 201325, "shoulderPads_norm_unique_03"],
    "visage-of-giyua": ["Visage of Giyua", 221168, "voodooMask_norm_unique_05"],
    "votoyias-spiker": ["Vo'Toyias Spiker", 298188, "shield_norm_unique_12"],
    "vyrs-astonishing-aura": ["Vyr’s Astonishing Aura", 332357, "chestarmor_norm_set_13"],
    "vyrs-fantastic-finery": ["Vyr’s Fantastic Finery", 332360, "pants_norm_set_13"],
    "vyrs-grasping-gauntlets": ["Vyr’s Grasping Gauntlets", 346210, "gloves_norm_set_13"],
    "vyrs-swaggering-stance": ["Vyr’s Swaggering Stance", 332363, "boots_norm_set_13"],
    "warmonger": ["Warmonger", 181495, "twoHandedSword_norm_unique_07"],
    "warzechian-armguards": ["Warzechian Armguards", 298115, "bracers_norm_unique_11"],
    "weight-of-the-earth": ["Weight of the Earth", 340521, "pants_norm_set_15"],
    "wildwood": ["Wildwood", 270978, "sword_norm_unique_11"],
    "windforce": ["Windforce", 192602, "Bow_norm_unique_04"],
    "wings-of-valor": ["Wings of Valor", 378291, "angelwings_imperius"],
    "wizardspike": ["Wizardspike", 219329, "dagger_norm_unique_05"],
    "wormwood": ["Wormwood", 195407, "staff_norm_unique_07"],
    "wrath-of-the-bone-king": ["Wrath of the Bone King", 191584, "twoHandedMace_norm_unique_06"],
    "wyrdward": ["Wyrdward", 298089, "ring_norm_unique_026"],
    "xephirian-amulet": ["Xephirian Amulet", 197814, "Amulet_norm_unique_04"],
    "zunimassas-marrow": ["Zunimassa's Marrow", 205615, "chestArmor_norm_unique_088"],
    "zunimassas-pox": ["Zunimassa's Pox", 212579, "Ring_norm_unique_012"],
    "zunimassas-string-of-skulls": ["Zunimassa's String of Skulls", 216525, "Mojo_norm_unique_07"],
    "zunimassas-trail": ["Zunimassa's Trail", 205627, "Boots_norm_unique_088"],
    "zunimassas-vision": ["Zunimassa's Vision", 221202, "voodooMask_norm_unique_04"]
}