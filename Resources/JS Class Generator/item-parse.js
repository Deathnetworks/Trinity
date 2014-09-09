

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
  // Amulet
  "aughilds-power": ["Aughild's Power", 197812, "Amulet_norm_unique_02"], 
  "blackthornes-duncraig-cross": ["Blackthorne's Duncraig Cross", 224189, "Amulet_norm_unique_16"], 
  "blackthornes-duncraig-cross": ["Blackthorne's Duncraig Cross", 298050, "amulet_norm_unique_19"], 
  "bottomless-potion-of-regeneration": ["Bottomless Potion of Regeneration", 297806, "amulet_norm_unique_17"], 
  "bottomless-potion-of-the-tower": ["Bottomless Potion of the Tower", 325062, "Amulet_norm_base_17"], 
  "dovu-energy-trap": ["Dovu Energy Trap", 298054, "amulet_norm_unique_23"], 
  "eye-of-etlich": ["Eye of Etlich", 197823, "Amulet_norm_unique_12"], 
  "golden-gorget-of-leoric": ["Golden Gorget of Leoric", 298052, "amulet_norm_unique_21"], 
  "halcyons-ascent": ["Halcyon's Ascent", 298056, "amulet_norm_unique_25"], 
  "haunt-of-vaxo": ["Haunt of Vaxo", 197821, "Amulet_norm_unique_10"], 
  "hellfire-amulet": ["Hellfire Amulet", 298057, "x1_Amulet_norm_unique_26"],  // IsCrafted
  "holy-beacon": ["Holy Beacon", 197822, "amulet_norm_unique_11"], 
  "kymbos-gold": ["Kymbo's Gold", 197824, "Amulet_norm_unique_13"], 
  "moonlight-ward": ["Moonlight Ward", 197813, "Amulet_norm_unique_03"], 
  "ouroboros": ["Ouroboros", 197815, "Amulet_norm_unique_05"], 
  "overwhelming-desire": ["Overwhelming Desire", 298053, "amulet_norm_unique_22"], 
  "rakoffs-glass-of-life": ["Rakoff's Glass of Life", 298055, "amulet_norm_unique_24"], 
  "rondals-locket": ["Rondal's Locket", 197818, "Amulet_norm_unique_07"], 
  "squirts-necklace": ["Squirt's Necklace", 197819, "amulet_norm_unique_08"], 
  "sunwukos-shines": ["Sunwuko's Shines", 336174, "amulet_norm_set_11"], 
  "tal-rashas-allegiance": ["Tal Rasha's Allegiance", 222486, "Amulet_norm_unique_14"], 
  "the-ess-of-johan": ["The Ess of Johan", 298051, "amulet_norm_unique_20"], 
  "the-flavor-of-time": ["The Flavor of Time", 193659, "Amulet_norm_unique_01"], 
  "the-star-of-azkaranth": ["The Star of Azkaranth", 197817, "amulet_norm_unique_06"], 
  "the-travelers-pledge": ["The Traveler's Pledge", 222490, "Amulet_norm_unique_15"], 
  "xephirian-amulet": ["Xephirian Amulet", 197814, "Amulet_norm_unique_04"], 
  // Axe
  "flesh-tearer": ["Flesh Tearer", 116388, "axe_norm_unique_03"], 
  "genzaniku": ["Genzaniku", 116386, "Axe_norm_unique_01"], 
  "hack": ["Hack", 271598, "axe_norm_unique_09"], 
  "sky-splitter": ["Sky Splitter", 116389, "Axe_norm_unique_04"], 
  "the-burning-axe-of-sankis": ["The Burning Axe of Sankis", 181484, "Axe_norm_unique_05"], 
  "the-butchers-sickle": ["The Butcher's Sickle", 189973, "Axe_norm_unique_06"], 
  // Belt
  "angel-hair-braid": ["Angel Hair Braid", 193666, "belt_norm_unique_03"], 
  "blackthornes-notched-belt": ["Blackthorne's Notched Belt", 224191, "Belt_norm_unique_14"], 
  "bottomless-potion-of-the-diamond": ["Bottomless Potion of the Diamond", 336184, "belt_norm_set_02"], 
  "captain-crimsons-silk-girdle": ["Captain Crimson's Silk Girdle", 222974, "Belt_norm_unique_12"],  // IsCrafted
  "cord-of-the-sherma": ["Cord of the Sherma", 298127, "belt_norm_unique_18"], 
  "countess-julias-cameo": ["Countess Julia's Cameo", 193668, "Belt_norm_unique_05"], 
  "fleeting-strap": ["Fleeting Strap", 193667, "Belt_norm_unique_04"],  // IsCrafted
  "forgotten-soul": ["Forgotten Soul", 298129, "belt_norm_unique_19"], 
  "goldwrap": ["Goldwrap", 193671, "belt_norm_unique_08"], 
  "guardians-case": ["Guardian's Case", 222976, "Belt_norm_unique_13"],  // IsCrafted
  "hexing-pants-of-mr-yan": ["Hexing Pants of Mr. Yan", 298131, "belt_norm_unique_21"], 
  "hwoj-wrap": ["Hwoj Wrap", 193670, "Belt_norm_unique_07"], 
  "innas-favor": ["Inna's Favor", 222487, "Belt_norm_unique_10"], 
  "insatiable-belt": ["Insatiable Belt", 298126, "belt_norm_unique_17"], 
  "jangs-envelopment-": ["Jang�s Envelopment ", 298130, "belt_norm_unique_20"], 
  "razor-strop": ["Razor Strop", 298124, "belt_norm_unique_15"], 
  "saffron-wrap": ["Saffron Wrap", 193664, "Belt_norm_unique_01"], 
  "sash-of-knives": ["Sash of Knives", 298125, "belt_norm_unique_16"], 
  "sebors-nightmare": ["Sebor�s Nightmare", 299381, "belt_norm_unique_22"], 
  "string-of-ears": ["String of Ears", 193669, "Belt_norm_unique_06"], 
  "tal-rashas-brace": ["Tal Rasha's Brace", 212657, "Belt_norm_unique_09"], 
  "vigilante-belt": ["Vigilante Belt", 193665, "Belt_norm_unique_02"], 
  // Boots
  "ashearas-finders": ["Asheara's Finders", 205618, "Boots_norm_unique_073"],  // IsCrafted
  "blackthornes-spurs": ["Blackthorne's Spurs", 222463, "Boots_norm_unique_050"], 
  "board-walkers": ["Board Walkers", 205621, "Boots_norm_unique_076"],  // IsCrafted
  "boj-anglers": ["Boj Anglers", 197224, "Boots_norm_unique_045"], 
  "boots-of-disregard": ["Boots of Disregard", 322905, "boots_norm_unique_02"], 
  "cains-sandals": ["Cain's Sandals", 197225, "Boots_norm_unique_046"],  // IsCrafted
  "captain-crimsons-waders": ["Captain Crimson's Waders", 197221, "Boots_norm_unique_043"],  // IsCrafted
  "eight-demon-boots": ["Eight-Demon Boots", 338031, "boots_norm_set_08"], 
  "fire-walkers": ["Fire Walkers", 205624, "Boots_norm_unique_085"], 
  "firebirds-tarsi": ["Firebird's Tarsi", 358793, "boots_norm_set_06"], 
  "haunt-of-vaxo": ["Haunt of Vaxo", 222464, "Boots_norm_unique_051"], 
  "helltooth-greaves": ["Helltooth Greaves", 340524, "boots_norm_set_16"], 
  "illusory-boots": ["Illusory Boots", 332342, "boots_norm_unique_03"], 
  "immortal-kings-stride": ["Immortal King's Stride", 205625, "Boots_norm_unique_086"], 
  "irontoe-mudsputters": ["Irontoe Mudsputters", 339125, "boots_norm_unique_04"], 
  "jade-harvesters-swiftness": ["Jade Harvester's Swiftness", 338037, "boots_norm_set_09"], 
  "lut-socks": ["Lut Socks", 205622, "Boots_norm_unique_077"], 
  "marauders-treads": ["Marauder's Treads", 336995, "boots_norm_set_07"], 
  "natalyas-bloody-footprints": ["Natalya's Bloody Footprints", 197223, "Boots_norm_unique_044"], 
  "raekors-striders": ["Raekor�s Striders", 336987, "boots_norm_set_05"], 
  "rolands-stride": ["Roland's Stride", 404094, "p1_Boots_norm_set_01"], 
  "sabatons-of-akkhan": ["Sabatons of Akkhan", 358795, "boots_norm_set_10"], 
  "the-crudest-boots": ["The Crudest Boots", 205620, "Boots_norm_unique_075"], 
  "the-shadows-heels": ["The Shadow�s Heels", 332364, "boots_norm_set_14"], 
  "vyrs-swaggering-stance": ["Vyr�s Swaggering Stance", 332363, "boots_norm_set_13"], 
  "zunimassas-trail": ["Zunimassa's Trail", 205627, "Boots_norm_unique_088"], 
  // Bow
  "cluckeye": ["Cluckeye", 175582, "Bow_norm_unique_03"], 
  "etrayu": ["Etrayu", 175581, "Bow_norm_unique_02"], 
  "kridershot": ["Kridershot", 271875, "bow_norm_unique_09"], 
  "leonine-bow-of-hashir": ["Leonine Bow of Hashir", 271882, "bow_norm_unique_11"], 
  "sydyru-crust": ["Sydyru Crust", 221893, "Bow_norm_unique_06"],  // IsCrafted
  "the-ravens-wing": ["The Raven's Wing", 221938, "Bow_norm_unique_07"], 
  "unbound-bolt": ["Unbound Bolt", 220654, "Bow_norm_unique_05"],  // IsCrafted
  "uskang": ["Uskang", 175580, "bow_norm_unique_01"], 
  "windforce": ["Windforce", 192602, "bow_norm_unique_04"], 
  // Bracer
  "ancient-parthan-defenders": ["Ancient Parthan Defenders", 298116, "bracers_norm_unique_12"], 
  "aughilds-search": ["Aughild's Search", 222972, "Bracers_norm_unique_09"],  // IsCrafted
  "custerian-wristguards": ["Custerian Wristguards", 298122, "bracers_norm_unique_17"], 
  "demons-animus": ["Demon's Animus", 222741, "Bracers_norm_unique_08"],  // IsCrafted
  "eberli-charo": ["Eberli Charo", 298118, "bracers_norm_unique_13"],  // IsCrafted
  "gungdo-gear": ["Gungdo Gear", 193688, "Bracers_norm_unique_06"], 
  "harrington-waistguard": ["Harrington Waistguard", 298121, "bracers_norm_unique_16"], 
  "kethryes-splint": ["Kethryes' Splint", 193683, "Bracers_norm_unique_01"],  // IsCrafted
  "krelms-buff-bracers": ["Krelm's Buff Bracers", 336185, "bracers_norm_set_02"], 
  "lacuni-prowlers": ["Lacuni Prowlers", 193687, "Bracers_norm_unique_05"], 
  "promise-of-glory": ["Promise of Glory", 193684, "Bracers_norm_unique_02"], 
  "sanguinary-vambraces": ["Sanguinary Vambraces", 298120, "bracers_norm_unique_15"], 
  "shackles-of-the-invoker": ["Shackles of the Invoker", 335030, "bracers_norm_set_12"], 
  "slave-bonds": ["Slave Bonds", 193685, "Bracers_norm_unique_03"], 
  "steady-strikers": ["Steady Strikers", 193686, "bracers_norm_unique_04"], 
  "strongarm-bracers": ["Strongarm Bracers", 193692, "Bracers_norm_unique_07"], 
  "tragoul-coils": ["Trag'Oul Coils", 298119, "bracers_norm_unique_14"], 
  "warzechian-armguards": ["Warzechian Armguards", 298115, "bracers_norm_unique_11"], 
  // CeremonialDagger
  "anessazi-edge": ["Anessazi Edge", 196250, "ceremonialDagger_norm_unique_04"], 
  "deadly-rebirth": ["Deadly Rebirth", 193433, "ceremonialDagger_norm_unique_02"], 
  "kymbos-gold": ["Kymbo's Gold", 271745, "ceremonialdagger_norm_unique_11"], 
  "last-breath": ["Last Breath", 195370, "ceremonialDagger_norm_unique_03"], 
  "manajumas-carving-knife": ["Manajuma's Carving Knife", 223365, "ceremonialDagger_norm_unique_06"], 
  "sacred-harvester": ["Sacred Harvester", 403748, "p1_ceremonialDagger_norm_unique_01"], 
  "starmetal-kukri": ["Starmetal Kukri", 271738, "ceremonialdagger_norm_unique_10"], 
  "the-dagger-of-darts": ["The Dagger of Darts", 403767, "p1_ceremonialDagger_norm_unique_02"], 
  "the-gidbinn": ["The Gidbinn", 209246, "ceremonialDagger_norm_unique_09"], 
  "the-spider-queens-grasp": ["The Spider Queen�s Grasp", 222978, "ceremonialDagger_norm_unique_05"], 
  "umbral-oath": ["Umbral Oath", 192540, "ceremonialDagger_norm_unique_01"],  // IsCrafted
  // Chest
  "aquila-cuirass": ["Aquila Cuirass", 197203, "chestarmor_norm_unique_047"], 
  "armor-of-the-kind-regent": ["Armor of the Kind Regent", 332202, "chestarmor_norm_unique_02"], 
  "aughilds-dominion": ["Aughild's Dominion", 197193, "chestArmor_norm_unique_043"],  // IsCrafted
  "aughilds-search": ["Aughild's Search", 222455, "chestArmor_norm_unique_049"], 
  "blackthornes-surcoat": ["Blackthorne's Surcoat", 222456, "chestArmor_norm_unique_050"], 
  "borns-frozen-soul": ["Born's Frozen Soul", 197199, "chestArmor_norm_unique_044"],  // IsCrafted
  "breastplate-of-akkhan": ["Breastplate of Akkhan", 358796, "chestarmor_norm_set_10"], 
  "chaingmail": ["Chaingmail", 197204, "chestArmor_norm_unique_048"], 
  "demons-marrow": ["Demon's Marrow", 205612, "chestArmor_norm_unique_085"],  // IsCrafted
  "firebirds-breast": ["Firebird's Breast", 358788, "chestarmor_norm_set_06"], 
  "goldskin": ["Goldskin", 205616, "chestArmor_norm_unique_089"], 
  "heart-of-iron": ["Heart of Iron", 205607, "chestArmor_norm_unique_074"], 
  "heart-of-the-crashing-wave": ["Heart of the Crashing Wave", 338032, "chestarmor_norm_set_08"], 
  "helltooth-tunic": ["Helltooth Tunic", 363088, "chestarmor_norm_set_16"], 
  "immortal-kings-eternal-reign": ["Immortal King's Eternal Reign", 205613, "chestArmor_norm_unique_086"], 
  "innas-vast-expanse": ["Inna's Vast Expanse", 205614, "chestArmor_norm_unique_087"], 
  "jade-harvesters-peace": ["Jade Harvester's Peace", 338038, "chestarmor_norm_set_09"], 
  "mantle-of-the-rydraelm": ["Mantle of the Rydraelm", 205609, "chestArmor_norm_unique_076"],  // IsCrafted
  "marauders-carapace": ["Marauder's Carapace", 363803, "chestarmor_norm_set_07"], 
  "nemesis-bracers": ["Nemesis Bracers", 205615, "chestArmor_norm_unique_088"], 
  "raekors-heart": ["Raekor�s Heart", 336984, "chestarmor_norm_set_05"], 
  "rolands-bearing": ["Roland's Bearing", 404095, "chestArmor_norm_base_flippy"], 
  "shi-mizus-haori": ["Shi Mizu's Haori", 332200, "chestarmor_norm_unique_01"], 
  "tal-rashas-relentless-pursuit": ["Tal Rasha's Relentless Pursuit", 211626, "chestArmor_norm_set_01"], 
  "the-shadows-bane": ["The Shadow�s Bane", 332359, "chestarmor_norm_set_14"], 
  "tyraels-might": ["Tyrael's Might", 205608, "chestArmor_norm_unique_075"], 
  "vyrs-astonishing-aura": ["Vyr�s Astonishing Aura", 332357, "chestarmor_norm_set_13"], 
  // Cloak
  "beckon-sail": ["Beckon Sail", 223150, "Cloak_norm_unique_02"], 
  "blackfeather": ["Blackfeather", 332206, "cloak_norm_unique_01"], 
  "cape-of-the-dark-night": ["Cape of the Dark Night", 223149, "Cloak_norm_unique_01"], 
  "cloak-of-deception": ["Cloak of Deception", 332208, "cloak_norm_unique_02"], 
  "natalyas-embrace": ["Natalya's Embrace", 208934, "Cloak_norm_set_03"], 
  "the-cloak-of-the-garwulf": ["The Cloak of the Garwulf", 223151, "Cloak_norm_unique_03"], 
  // CraftingMaterials
  "aughilds-search": ["Aughild's Search", 361984, "Crafting_AssortedParts_05"], 
  "fiery-brimstone": ["Fiery Brimstone", 189863, "Crafting_Tier_04D"], 
  "forgotten-soul": ["Forgotten Soul", 361988, "Crafting_Legendary_05"], 
  "gibbering-gemstone": ["Gibbering Gemstone", 214604, "Cow_Gem"], 
  "leorics-shinbone": ["Leoric's Shinbone", 214605, "Cow_Bone"], 
  "liquid-rainbow": ["Liquid Rainbow", 214603, "Cow_Water"], 
  "wirts-bell": ["Wirt's Bell", 180697, "CowBell"], 
  // CraftingPlan
  "arma-haereticorum": ["Arma Haereticorum", 398367, "CraftingPlan_Mystic_Transmog_Drop_Bound"], 
  "plan-arcane-barb": ["Plan: Arcane Barb", 192598, "CraftingPlan_Smith_Drop"], 
  "plan-hellish-staff-of-herding": ["Plan: Hellish Staff of Herding", 253241, "CraftingPlan_Smith_Drop_Soulbound"], 
  // CraftingReagent
  "aughilds-rule": ["Aughild's Rule", 364697, "CraftingReagent_Legendary_Unique_InfernalMachine_Diablo_x1"], 
  "black-mushroom": ["Black Mushroom", 162311, "A1_BlackMushroom"], 
  "bottomless-potion-of-the-tower": ["Bottomless Potion of the Tower", 364696, "CraftingReagent_Legendary_Unique_InfernalMachine_SiegeBreaker_x1"], 
  "heart-of-evil": ["Heart of Evil", 364725, "DemonOrgan_Diablo_x1"], 
  "idol-of-terror": ["Idol of Terror", 364724, "DemonOrgan_SiegeBreaker_x1"], 
  "key-of-bones": ["Key of Bones", 364694, "CraftingReagent_Legendary_Unique_InfernalMachine_SkeletonKing_x1"], 
  "key-of-bones": ["Key of Bones", 364695, "CraftingReagent_Legendary_Unique_InfernalMachine_Ghom_x1"], 
  "key-of-destruction": ["Key of Destruction", 255880, "DemonKey_Destruction"], 
  "key-of-hate": ["Key of Hate", 255881, "DemonKey_Hate"], 
  "key-of-terror": ["Key of Terror", 255882, "DemonKey_Terror"], 
  "key-of-war": ["Key of War", 361986, "Crafting_Rare_05"], 
  "leorics-regret": ["Leoric's Regret", 364722, "DemonOrgan_SkeletonKing_x1"], 
  "vial-of-putridness": ["Vial of Putridness", 364723, "DemonOrgan_Ghom_x1"], 
  // Crossbow
  "arcane-barb": ["Arcane Barb", 194957, "XBow_norm_unique_04"],  // IsCrafted
  "bakkan-caster": ["Bakkan Caster", 98163, "xbow_norm_unique_01"], 
  "buriza-do-kyanon": ["Buriza-Do Kyanon", 194219, "XBow_norm_unique_03"], 
  "chanon-bolter": ["Chanon Bolter", 271884, "xbow_norm_unique_08"], 
  "demon-machine": ["Demon Machine", 222286, "xbow_norm_unique_07"], 
  "hellrack": ["Hellrack", 192836, "XBow_norm_unique_02"], 
  "manticore": ["Manticore", 221760, "XBow_norm_unique_06"], 
  "pus-spitter": ["Pus Spitter", 204874, "XBow_norm_unique_05"], 
  "wojahnni-assaulter": ["Wojahnni Assaulter", 271889, "xbow_norm_unique_09"], 
  // CrusaderShield
  "akarats-awakening": ["Akarat's Awakening", 299414, "crushield_norm_unique_04"], 
  "hallowed-bulwark": ["Hallowed Bulwark", 299413, "crushield_norm_unique_03"], 
  "hellskull": ["Hellskull", 299415, "crushield_norm_unique_05"], 
  "jekangbord": ["Jekangbord", 299412, "crushield_norm_unique_02"], 
  "piro-marella": ["Piro Marella", 299411, "crushield_norm_unique_01"],  // IsCrafted
  "salvation": ["Salvation", 299418, "crushield_norm_unique_08"], 
  "sublime-conviction": ["Sublime Conviction", 299416, "crushield_norm_unique_06"], 
  "the-final-witness": ["The Final Witness", 299417, "crushield_norm_unique_07"], 
  // Dagger
  "blood-magic-edge": ["Blood-Magic Edge", 195655, "Dagger_norm_unique_04"],  // IsCrafted
  "envious-blade": ["Envious Blade", 271732, "dagger_norm_unique_09"], 
  "kill": ["Kill", 192579, "Dagger_norm_unique_02"], 
  "pig-sticker": ["Pig Sticker", 221313, "dagger_norm_unique_06"], 
  "plan-longshot": ["Plan: Longshot", 3906, "Dagger_norm_base_04"], 
  "the-barber": ["The Barber", 195174, "Dagger_norm_unique_03"], 
  "the-horadric-hamburger": ["The Horadric Hamburger", 200476, "offHand_norm_base_01"], 
  "wizardspike": ["Wizardspike", 219329, "Dagger_norm_unique_05"], 
  // Daibo
  "balance": ["Balance", 195145, "combatstaff_norm_unique_04"], 
  "flying-dragon": ["Flying Dragon", 197065, "combatStaff_norm_unique_02"], 
  "incense-torch-of-the-grand-temple": ["Incense Torch of the Grand Temple", 192342, "combatStaff_norm_unique_01"], 
  "innas-reach": ["Inna's Reach", 212208, "combatStaff_norm_unique_08"], 
  "staff-of-kyro": ["Staff of Kyro", 271749, "combatstaff_norm_unique_09"], 
  "the-flow-of-eternity": ["The Flow of Eternity", 197072, "combatStaff_norm_unique_06"], 
  "the-paddle": ["The Paddle", 197068, "combatStaff_norm_unique_03"], 
  "warstaff-of-general-quang": ["Warstaff of General Quang", 271765, "combatstaff_norm_unique_10"], 
  // FistWeapon
  "crystal-fist": ["Crystal Fist", 175939, "fistWeapon_norm_unique_08"], 
  "fleshrake": ["Fleshrake", 145850, "fistWeapon_norm_unique_03"], 
  "jawbreaker": ["Jawbreaker", 271957, "fistweapon_norm_unique_14"], 
  "logans-claw": ["Logan's Claw", 145849, "fistweapon_norm_unique_02"], 
  "rabid-strike": ["Rabid Strike", 196472, "fistweapon_norm_unique_10"], 
  "scarbringer": ["Scarbringer", 130557, "fistWeapon_norm_unique_01"], 
  "shenlongs-fist-of-legend": ["Shenlong's Fist of Legend", 208996, "fistWeapon_norm_unique_12"], 
  "shenlongs-relentless-assault": ["Shenlong's Relentless Assault", 208898, "fistWeapon_norm_unique_11"], 
  "sledge-fist": ["Sledge Fist", 175938, "fistWeapon_norm_unique_07"], 
  "the-fist-of-azturrasq": ["The Fist of Az'Turrasq", 175937, "fistWeapon_norm_unique_06"], 
  "vengeful-wind": ["Vengeful Wind", 403775, "p1_fistWeapon_norm_unique_02"], 
  "won-khim-lau": ["Won Khim Lau", 145851, "fistWeapon_norm_unique_04"], 
  // Flail
  "baleful-remnant": ["Baleful Remnant", 299435, "flail2h_norm_unique_02"], 
  "darklight": ["Darklight", 299428, "flail1h_norm_unique_06"], 
  "fate-of-the-fell": ["Fate of the Fell", 299436, "flail2h_norm_unique_03"], 
  "golden-flense": ["Golden Flense", 299437, "flail2h_norm_unique_04"], 
  "gyrfalcons-foote": ["Gyrfalcon's Foote", 299427, "flail1h_norm_unique_05"], 
  "inviolable-faith": ["Inviolable Faith", 299429, "flail1h_norm_unique_07"], 
  "justinians-mercy": ["Justinian's Mercy", 299424, "flail1h_norm_unique_02"], 
  "kassars-retribution": ["Kassar's Retribution", 299426, "flail1h_norm_unique_04"], 
  "swiftmount": ["Swiftmount", 299425, "flail1h_norm_unique_03"], 
  "the-mortal-drama": ["The Mortal Drama", 299431, "flail2h_norm_unique_01"], 
  // FollowerSpecial
  "bottomless-potion-of-regeneration": ["Bottomless Potion of Regeneration", 190639, "FollowerItem_Scoundrel_norm_base_02"], 
  "enchanting-favor": ["Enchanting Favor", 366968, "followeritem_templar_legendary_01"], 
  "hand-of-the-prophet": ["Hand of the Prophet", 366980, "followeritem_enchantress_legendary_02"], 
  "hillenbrands-training-sword": ["Hillenbrand�s Training Sword", 366969, "followeritem_templar_legendary_02"], 
  "ribald-etchings": ["Ribald Etchings", 366971, "followeritem_scoundrel_legendary_02"], 
  "ring-of-royal-grandeur": ["Ring of Royal Grandeur", 190635, "FollowerItem_Enchantress_norm_base_02"], 
  "skeleton-key": ["Skeleton Key", 366970, "followeritem_scoundrel_legendary_01"], 
  "smoking-thurible": ["Smoking Thurible", 366979, "followeritem_enchantress_legendary_01"], 
  // Gem
  "bottomless-potion-of-the-leech": ["Bottomless Potion of the Leech", 283118, "Ruby_15"], 
  "bottomless-potion-of-the-tower": ["Bottomless Potion of the Tower", 361559, "Diamond_15"], 
  "key-of-bones": ["Key of Bones", 283116, "Amethyst_15"], 
  "key-of-bones": ["Key of Bones", 283117, "Emerald_15"], 
  "key-of-gluttony": ["Key of Gluttony", 283119, "Topaz_15"], 
  "key-of-war": ["Key of War", 361568, "Ruby_16"], 
  // Gloves
  "ashearas-ward": ["Asheara's Ward", 205636, "Gloves_norm_unique_073"],  // IsCrafted
  "bottomless-potion-of-the-diamond": ["Bottomless Potion of the Diamond", 253993, "gloves_hell_base_08"], 
  "cains-scribe": ["Cain's Scribe", 197210, "Gloves_norm_unique_046"],  // IsCrafted
  "countess-julias-cameo": ["Countess Julia's Cameo", 358798, "gloves_norm_set_10"], 
  "firebirds-talons": ["Firebird's Talons", 358789, "gloves_norm_set_06"], 
  "fists-of-thunder": ["Fists of Thunder", 338033, "gloves_norm_set_08"], 
  "frostburn": ["Frostburn", 197205, "Gloves_norm_unique_043"], 
  "gladiator-gauntlets": ["Gladiator Gauntlets", 205635, "Gloves_norm_unique_090"], 
  "gloves-of-worship": ["Gloves of Worship", 332344, "gloves_norm_unique_03"], 
  "goldskin": ["Goldskin", 205642, "Gloves_norm_unique_078"], 
  "helltooth-gauntlets": ["Helltooth Gauntlets", 363094, "gloves_norm_set_16"], 
  "immortal-kings-irons": ["Immortal King's Irons", 205631, "Gloves_norm_unique_086"], 
  "jade-harvesters-mercy": ["Jade Harvester's Mercy", 338039, "gloves_norm_set_09"], 
  "magefist": ["Magefist", 197206, "Gloves_norm_unique_044"], 
  "marauders-gloves": ["Marauder's Gloves", 336992, "gloves_norm_set_07"], 
  "penders-purchase": ["Penders Purchase", 197207, "Gloves_norm_unique_045"],  // IsCrafted
  "pride-of-the-invoker": ["Pride of the Invoker", 335027, "gloves_norm_set_12"], 
  "pull-of-the-earth": ["Pull of the Earth", 340523, "gloves_norm_set_15"], 
  "raekors-wraps": ["Raekor�s Wraps", 336985, "gloves_norm_set_05"], 
  "rolands-grasp": ["Roland's Grasp", 404096, "Gloves_norm_base_flippy"], 
  "sages-purchase": ["Sage's Purchase", 205632, "Gloves_norm_unique_087"],  // IsCrafted
  "st-archews-gage": ["St. Archew's Gage", 332172, "gloves_norm_unique_01"], 
  "stone-gauntlets": ["Stone Gauntlets", 205640, "Gloves_norm_unique_076"], 
  "sunwukos-paws": ["Sunwuko's Paws", 336172, "gloves_norm_set_11"], 
  "the-shadows-grasp": ["The Shadow�s Grasp", 332362, "gloves_norm_set_14"], 
  "vyrs-grasping-gauntlets": ["Vyr�s Grasping Gauntlets", 346210, "gloves_norm_set_13"], 
  // HealthPotion
  "aughilds-search": ["Aughild's Search", 341333, "healthPotion_Legendary_01_x1"], 
  "aughilds-search": ["Aughild's Search", 341342, "healthPotion_Legendary_02_x1"], 
  "bottomless-potion-of-kulle-aid": ["Bottomless Potion of Kulle-Aid", 344093, "healthPotion_Legendary_06_x1"], 
  "bottomless-potion-of-mutilation": ["Bottomless Potion of Mutilation", 341343, "healthPotion_Legendary_03_x1"], 
  "bottomless-potion-of-mutilation": ["Bottomless Potion of Mutilation", 342824, "healthPotion_Legendary_05_x1"], 
  "bottomless-potion-of-rejuvenation": ["Bottomless Potion of Rejuvenation", 404808, "healthPotion_Legendary_07_x1"], 
  "bottomless-potion-of-the-leech": ["Bottomless Potion of the Leech", 342823, "healthPotion_Legendary_04_x1"], 
  "immortal-kings-triumph": ["Immortal King's Triumph", 304319, "healthPotion_Console"], 
  // Helm
  "andariels-visage": ["Andariel's Visage", 198014, "Helm_norm_unique_03"], 
  "aughilds-peak": ["Aughild's Peak", 223972, "Helm_norm_set_03"],  // IsCrafted
  "blind-faith": ["Blind Faith", 197037, "helm_norm_unique_07"], 
  "bottomless-potion-of-the-tower": ["Bottomless Potion of the Tower", 358799, "helm_norm_set_10"], 
  "broken-crown": ["Broken Crown", 220630, "Helm_norm_unique_02"], 
  "cains-insight": ["Cain's Insight", 222559, "Helm_norm_set_02"],  // IsCrafted
  "crown-of-the-invoker": ["Crown of the Invoker", 335028, "helm_norm_set_12"], 
  "deathseers-cowl": ["Deathseer's Cowl", 298146, "helm_norm_unique_14"], 
  "eyes-of-the-earth": ["Eyes of the Earth", 340528, "helm_norm_set_15"], 
  "firebirds-plume": ["Firebird's Plume", 358791, "helm_norm_set_06"], 
  "helltooth-mask": ["Helltooth Mask", 369016, "helm_norm_set_16"], 
  "immortal-kings-triumph": ["Immortal King's Triumph", 210265, "Helm_norm_unique_08"], 
  "jade-harvesters-wisdom": ["Jade Harvester's Wisdom", 338040, "helm_norm_set_09"], 
  "leorics-crown": ["Leoric's Crown", 196024, "Helm_norm_unique_01"], 
  "marauders-visage": ["Marauder's Visage", 336994, "helm_norm_set_07"], 
  "mask-of-the-searing-sky": ["Mask of the Searing Sky", 338034, "helm_norm_set_08"], 
  "mempo-of-twilight": ["Mempo of Twilight", 223577, "Helm_norm_unique_12"], 
  "natalyas-sight": ["Natalya's Sight", 210851, "helm_norm_unique_09"], 
  "prides-fall": ["Pride's Fall", 298147, "helm_norm_unique_15"], 
  "raekors-will": ["Raekor�s Will", 336988, "helm_norm_set_05"], 
  "rolands-visage": ["Roland's Visage", 404700, "Helm_norm_base_flippy"], 
  "sages-apogee": ["Sage's Apogee", 221624, "Helm_inferno_set_01"],  // IsCrafted
  "skull-of-resonance": ["Skull of Resonance", 220549, "Helm_norm_unique_04"], 
  "sunwukos-crown": ["Sunwuko's Crown", 336173, "helm_norm_set_11"], 
  "tal-rashas-guise-of-wisdom": ["Tal Rasha's Guise of Wisdom", 211531, "Helm_norm_unique_10"], 
  "the-helm-of-rule": ["The Helm of Rule", 222889, "Helm_norm_unique_11"],  // IsCrafted
  // InfernalMachine
  "infernal-machine-of-bones": ["Infernal Machine of Bones", 366946, "InfernalMachine_SkeletonKing_x1"],  // IsCrafted
  "infernal-machine-of-evil": ["Infernal Machine of Evil", 366949, "InfernalMachine_Diablo_x1"],  // IsCrafted
  "infernal-machine-of-gluttony": ["Infernal Machine of Gluttony", 366947, "InfernalMachine_Ghom_x1"],  // IsCrafted
  "infernal-machine-of-war": ["Infernal Machine of War", 366948, "InfernalMachine_SiegeBreaker_x1"],  // IsCrafted
  // KeystoneFragment
  "greater-rift-keystone-rank-": ["Greater Rift Keystone Rank 1", 408130, "TieredLootrunKey_1"], 
  "greater-rift-keystone-rank-": ["Greater Rift Keystone Rank 10", 408140, "TieredLootrunKey_10"], 
  "greater-rift-keystone-rank-": ["Greater Rift Keystone Rank 11", 408141, "TieredLootrunKey_11"], 
  "greater-rift-keystone-rank-": ["Greater Rift Keystone Rank 12", 408142, "TieredLootrunKey_12"], 
  "greater-rift-keystone-rank-": ["Greater Rift Keystone Rank 13", 408143, "TieredLootrunKey_13"], 
  "greater-rift-keystone-rank-": ["Greater Rift Keystone Rank 14", 408144, "TieredLootrunKey_14"], 
  "greater-rift-keystone-rank-": ["Greater Rift Keystone Rank 15", 408145, "TieredLootrunKey_15"], 
  "greater-rift-keystone-rank-": ["Greater Rift Keystone Rank 16", 408146, "TieredLootrunKey_16"], 
  "greater-rift-keystone-rank-": ["Greater Rift Keystone Rank 17", 408147, "TieredLootrunKey_17"], 
  "greater-rift-keystone-rank-": ["Greater Rift Keystone Rank 18", 408148, "TieredLootrunKey_18"], 
  "greater-rift-keystone-rank-": ["Greater Rift Keystone Rank 19", 408149, "TieredLootrunKey_19"], 
  "greater-rift-keystone-rank-": ["Greater Rift Keystone Rank 20", 408150, "TieredLootrunKey_20"], 
  "greater-rift-keystone-rank-": ["Greater Rift Keystone Rank 21", 408151, "TieredLootrunKey_21"], 
  "greater-rift-keystone-rank-": ["Greater Rift Keystone Rank 22", 408152, "TieredLootrunKey_22"], 
  "greater-rift-keystone-rank-": ["Greater Rift Keystone Rank 23", 408153, "TieredLootrunKey_23"], 
  "greater-rift-keystone-rank-": ["Greater Rift Keystone Rank 24", 408154, "TieredLootrunKey_24"], 
  "greater-rift-keystone-rank-": ["Greater Rift Keystone Rank 25", 408155, "TieredLootrunKey_25"], 
  "greater-rift-keystone-rank-": ["Greater Rift Keystone Rank 26", 408156, "TieredLootrunKey_26"], 
  "greater-rift-keystone-rank-": ["Greater Rift Keystone Rank 27", 408157, "TieredLootrunKey_27"], 
  "greater-rift-keystone-rank-": ["Greater Rift Keystone Rank 28", 408158, "TieredLootrunKey_28"], 
  "greater-rift-keystone-rank-": ["Greater Rift Keystone Rank 29", 408159, "TieredLootrunKey_29"], 
  "greater-rift-keystone-rank-": ["Greater Rift Keystone Rank 3", 408132, "TieredLootrunKey_3"], 
  "greater-rift-keystone-rank-": ["Greater Rift Keystone Rank 30", 408160, "TieredLootrunKey_30"], 
  "greater-rift-keystone-rank-": ["Greater Rift Keystone Rank 31", 408161, "TieredLootrunKey_31"], 
  "greater-rift-keystone-rank-": ["Greater Rift Keystone Rank 32", 408162, "TieredLootrunKey_32"], 
  "greater-rift-keystone-rank-": ["Greater Rift Keystone Rank 33", 408163, "TieredLootrunKey_33"], 
  "greater-rift-keystone-rank-": ["Greater Rift Keystone Rank 34", 408164, "TieredLootrunKey_34"], 
  "greater-rift-keystone-rank-": ["Greater Rift Keystone Rank 35", 408165, "TieredLootrunKey_35"], 
  "greater-rift-keystone-rank-": ["Greater Rift Keystone Rank 36", 408166, "TieredLootrunKey_36"], 
  "greater-rift-keystone-rank-": ["Greater Rift Keystone Rank 37", 408167, "TieredLootrunKey_37"], 
  "greater-rift-keystone-rank-": ["Greater Rift Keystone Rank 38", 408168, "TieredLootrunKey_38"], 
  "greater-rift-keystone-rank-": ["Greater Rift Keystone Rank 4", 408133, "TieredLootrunKey_4"], 
  "greater-rift-keystone-rank-": ["Greater Rift Keystone Rank 5", 408134, "TieredLootrunKey_5"], 
  "greater-rift-keystone-rank-": ["Greater Rift Keystone Rank 6", 408135, "TieredLootrunKey_6"], 
  "greater-rift-keystone-rank-": ["Greater Rift Keystone Rank 7", 408136, "TieredLootrunKey_7"], 
  "greater-rift-keystone-rank-": ["Greater Rift Keystone Rank 8", 408137, "TieredLootrunKey_8"], 
  "greater-rift-keystone-rank-": ["Greater Rift Keystone Rank 9", 408138, "TieredLootrunKey_9"], 
  "keystone-of-trials": ["Keystone of Trials", 408416, "TieredLootrunKey_0"], 
  // Legs
  "ashearas-pace": ["Asheara's Pace", 209054, "pants_norm_unique_073"],  // IsCrafted
  "blackthornes-jousting-mail": ["Blackthorne's Jousting Mail", 222477, "pants_norm_unique_050"], 
  "bottomless-potion-of-mutilation": ["Bottomless Potion of Mutilation", 197218, "pants_norm_unique_046"],  // IsCrafted
  "bottomless-potion-of-mutilation": ["Bottomless Potion of Mutilation", 336993, "pants_norm_set_07"], 
  "bottomless-potion-of-regeneration": ["Bottomless Potion of Regeneration", 209057, "pants_norm_unique_075"], 
  "captain-crimsons-bowsprit": ["Captain Crimson's Bowsprit", 197214, "pants_norm_unique_043"],  // IsCrafted
  "cuisses-of-akkhan": ["Cuisses of Akkhan", 358800, "pants_norm_set_10"], 
  "deaths-bargain": ["Death's Bargain", 332205, "pants_norm_unique_02"], 
  "demons-plate": ["Demon's Plate", 205644, "pants_norm_unique_085"],  // IsCrafted
  "depth-diggers": ["Depth Diggers", 197216, "pants_norm_unique_044"], 
  "firebirds-down": ["Firebird's Down", 358790, "pants_norm_set_06"], 
  "gehennas": ["Gehennas", 222476, "pants_norm_unique_049"],  // IsCrafted
  "hammer-jammers": ["Hammer Jammers", 209059, "pants_norm_unique_077"], 
  "helltooth-leg-guards": ["Helltooth Leg Guards", 340522, "pants_norm_set_16"], 
  "hexing-pants-of-mr-yan": ["Hexing Pants of Mr. Yan", 332204, "pants_norm_unique_01"], 
  "innas-temperance": ["Inna's Temperance", 205646, "pants_norm_unique_087"], 
  "jade-harvesters-courage": ["Jade Harvester's Courage", 338041, "pants_norm_set_09"], 
  "pox-faulds": ["Pox Faulds", 197220, "pants_norm_unique_048"], 
  "raekors-breeches": ["Raekor�s Breeches", 336986, "pants_norm_set_05"], 
  "rolands-determination": ["Roland's Determination", 404097, "p1_Pants_norm_set_01"], 
  "scales-of-the-dancing-serpent": ["Scales of the Dancing Serpent", 338035, "pants_norm_set_08"], 
  "the-shadows-coil": ["The Shadow�s Coil", 332361, "pants_norm_set_14"], 
  "vyrs-fantastic-finery": ["Vyr�s Fantastic Finery", 332360, "pants_norm_set_13"], 
  "weight-of-the-earth": ["Weight of the Earth", 340521, "pants_norm_set_15"], 
  // Mace
  "aughilds-power": ["Aughild's Power", 188173, "Mace_norm_unique_04"], 
  "devastator": ["Devastator", 188177, "Mace_norm_unique_06"],  // IsCrafted
  "echoing-fury": ["Echoing Fury", 188181, "Mace_norm_unique_07"], 
  "jaces-hammer-of-vigilance": ["Jace�s Hammer of Vigilance", 271648, "mace_norm_unique_10"], 
  "mad-monarchs-scepter": ["Mad Monarch's Scepter", 271663, "mace_norm_unique_12"], 
  "nailbiter": ["Nailbiter", 188158, "Mace_norm_unique_02"], 
  "neanderthal": ["Neanderthal", 102665, "Mace_norm_unique_01"], 
  "nutcracker": ["Nutcracker", 188169, "Mace_norm_unique_03"], 
  "odyn-son": ["Odyn Son", 188185, "Mace_norm_unique_08"], 
  "solanium": ["Solanium", 271662, "mace_norm_unique_11"], 
  "telrandens-hand": ["Telranden's Hand", 188189, "Mace_norm_unique_09"], 
  // Mighty Belt
  "ageless-might": ["Ageless Might", 193675, "BarbBelt_norm_unique_08"], 
  "chilaniks-chain": ["Chilanik�s Chain", 298133, "barbbelt_norm_unique_10"], 
  "dread-iron": ["Dread Iron", 193672, "BarbBelt_norm_unique_01"], 
  "girdle-of-giants": ["Girdle of Giants", 212232, "BarbBelt_norm_unique_04"], 
  "immortal-kings-tribal-binding": ["Immortal King's Tribal Binding", 212235, "BarbBelt_norm_unique_09"], 
  "kotuurs-brace": ["Kotuur's Brace", 193674, "BarbBelt_norm_unique_07"], 
  "lamentation": ["Lamentation", 212234, "barbbelt_norm_unique_05"], 
  "pride-of-cassius": ["Pride of Cassius", 193673, "BarbBelt_norm_unique_02"], 
  "the-undisputed-champion": ["The Undisputed Champion", 193676, "BarbBelt_norm_unique_06"], 
  "thundergods-vigor": ["Thundergod's Vigor", 212230, "BarbBelt_norm_unique_03"], 
  // Mojo
  "bitterness": ["Bitterness", 194988, "Mojo_norm_unique_03"],  // IsCrafted
  "countess-julias-cameo": ["Countess Julia's Cameo", 192468, "Mojo_norm_unique_02"], 
  "gazing-demise": ["Gazing Demise", 194995, "Mojo_norm_unique_05"], 
  "homunculus": ["Homunculus", 194991, "Mojo_norm_unique_04"], 
  "kymbos-gold": ["Kymbo's Gold", 216525, "Mojo_norm_unique_07"], 
  "manajumas-gory-fetch": ["Manajuma's Gory Fetch", 210993, "Mojo_norm_unique_06"], 
  "shukranis-triumph": ["Shukrani�s Triumph", 272070, "mojo_norm_unique_11"], 
  "uhkapian-serpent": ["Uhkapian Serpent", 191278, "Mojo_norm_unique_01"], 
  // OneHandCrossbow
  "balefire-caster": ["Balefire Caster", 192528, "handxbow_norm_unique_02"], 
  "blitzbolter": ["Blitzbolter", 195078, "handXbow_norm_unique_03"],  // IsCrafted
  "calamity": ["Calamity", 225181, "handXbow_norm_unique_08"], 
  "danettas-revenge": ["Danetta's Revenge", 211749, "handXbow_norm_unique_07"], 
  "danettas-spite": ["Danetta's Spite", 211745, "handXbow_norm_unique_06"], 
  "dawn": ["Dawn", 196409, "handXbow_norm_unique_04"], 
  "helltrapper": ["Helltrapper", 271914, "handxbow_norm_unique_11"], 
  "izzuccob": ["Izzuccob", 192467, "handXbow_norm_unique_01"], 
  "kmar-tenclip": ["K'mar Tenclip", 271892, "handxbow_norm_unique_10"], 
  "natalyas-slayer": ["Natalya's Slayer", 210874, "handXbow_norm_unique_05"], 
  // OneHandMightyWeapon
  "ambos-pride": ["Ambo's Pride", 193486, "mightyweapon_1h_norm_unique_03"], 
  "blade-of-the-warlord": ["Blade of the Warlord", 193611, "mightyweapon_1h_norm_unique_04"], 
  "bul-kathoss-solemn-vow": ["Bul-Kathos's Solemn Vow", 208771, "mightyWeapon_1H_norm_unique_05"], 
  "bul-kathoss-warrior-blood": ["Bul-Kathos's Warrior Blood", 208775, "mightyWeapon_1H_norm_unique_06"], 
  "fjord-cutter": ["Fjord Cutter", 192105, "mightyWeapon_1H_norm_unique_01"], 
  // Orb
  "chantodos-force": ["Chantodo's Force", 212277, "orb_norm_unique_05"], 
  "cosmic-strand": ["Cosmic Strand", 195127, "orb_norm_unique_03"],  // IsCrafted
  "firebirds-eye": ["Firebird's Eye", 358819, "orb_norm_set_06"], 
  "light-of-grace": ["Light of Grace", 272038, "orb_norm_unique_09"], 
  "mirrorball": ["Mirrorball", 272022, "orb_norm_unique_07"], 
  "mykens-ball-of-hate": ["Myken's Ball of Hate", 272037, "orb_norm_unique_08"], 
  "tal-rashas-unwavering-glare": ["Tal Rasha's Unwavering Glare", 212780, "orb_norm_unique_06"], 
  "the-oculus": ["The Oculus", 192320, "orb_norm_unique_02"], 
  "triumvirate": ["Triumvirate", 195325, "orb_norm_unique_04"], 
  "winter-flurry": ["Winter Flurry", 184199, "orb_norm_unique_01"], 
  // Polearm
  "bovine-bardiche": ["Bovine Bardiche", 272056, "polearm_norm_unique_05"], 
  "heart-slaughter": ["Heart Slaughter", 192569, "polearm_norm_unique_02"], 
  "pledge-of-caldeum": ["Pledge of Caldeum", 196570, "Polearm_norm_unique_04"], 
  "standoff": ["Standoff", 191570, "Polearm_norm_unique_01"], 
  "vigilance": ["Vigilance", 195491, "Polearm_norm_unique_03"], 
  // QuestItem
  "devils-fang": ["Devil's Fang", 257736, "Quest_Devils_fang_Flippy"], 
  "infernal-machine": ["Infernal Machine", 257737, "Quest_InfernalMachine_Flippy"],  // IsCrafted
  "vengeful-eye": ["Vengeful Eye", 257738, "Quest_Vengeful_eye"], 
  "writhing-spine": ["Writhing Spine", 257739, "Quest_Writhing_Spine_Flippy"], 
  // Quiver
  "archfiend-arrows": ["Archfiend Arrows", 197626, "Quiver_norm_unique_03"],  // IsCrafted
  "bombadiers-rucksack": ["Bombadier's Rucksack", 298171, "quiver_norm_unique_09"], 
  "dead-mans-legacy": ["Dead Man's Legacy", 197630, "quiver_norm_unique_07"], 
  "emimeis-duffel": ["Emimei�s Duffel", 298172, "quiver_norm_unique_10"], 
  "fletchers-pride": ["Fletcher's Pride", 197629, "Quiver_norm_unique_06"], 
  "flint-ripper-arrowheads": ["Flint Ripper Arrowheads", 197624, "Quiver_norm_unique_01"], 
  "holy-point-shot": ["Holy Point Shot", 197627, "Quiver_norm_unique_04"], 
  "silver-star-piercers": ["Silver Star Piercers", 197628, "quiver_norm_unique_05"], 
  "sin-seekers": ["Sin Seekers", 197625, "Quiver_norm_unique_02"], 
  "the-ninth-cirri-satchel": ["The Ninth Cirri Satchel", 298170, "quiver_norm_unique_08"], 
  // RiftKey
  "aughilds-power": ["Aughild's Power", 323722, "LootRunKey"], 
  // Ring
  "aughilds-power": ["Aughild's Power", 212582, "Ring_norm_unique_019"], 
  "aughilds-search": ["Aughild's Search", 212581, "Ring_norm_unique_010"], 
  "avarice-band": ["Avarice Band", 298095, "ring_norm_unique_032"], 
  "band-of-hollow-whispers": ["Band of Hollow Whispers", 197834, "ring_norm_unique_001"], 
  "band-of-the-rue-chambers": ["Band of the Rue Chambers", 298093, "ring_norm_unique_030"], 
  "band-of-untold-secrets": ["Band of Untold Secrets", 212602, "Ring_norm_unique_009"], 
  "bottomless-potion-of-mutilation": ["Bottomless Potion of Mutilation", 298088, "ring_norm_unique_025"], 
  "bottomless-potion-of-the-leech": ["Bottomless Potion of the Leech", 298094, "ring_norm_unique_031"], 
  "bottomless-potion-of-the-tower": ["Bottomless Potion of the Tower", 5044, "Ring_25"], 
  "broken-promises": ["Broken Promises", 212589, "Ring_norm_unique_006"], 
  "bul-kathoss-wedding-band": ["Bul-Kathos's Wedding Band", 212603, "Ring_norm_unique_020"], 
  "eternal-union": ["Eternal Union", 212601, "Ring_norm_unique_007"], 
  "focus": ["Focus", 332209, "ring_norm_set_001"], 
  "hellfire-ring": ["Hellfire Ring", 260327, "Ring_norm_unique_024"],  // IsCrafted
  "justice-lantern": ["Justice Lantern", 212590, "ring_norm_unique_008"], 
  "kredes-flame": ["Krede�s Flame", 197836, "Ring_norm_unique_003"], 
  "leorics-signet": ["Leoric's Signet", 197835, "Ring_norm_unique_002"], 
  "litany-of-the-undaunted": ["Litany of the Undaunted", 212651, "Ring_norm_unique_015"], 
  "manald-heal": ["Manald Heal", 212546, "Ring_norm_unique_021"], 
  "nagelring": ["Nagelring", 212586, "Ring_norm_unique_018"], 
  "natalyas-reflection": ["Natalya's Reflection", 212545, "Ring_norm_unique_011"], 
  "obsidian-ring-of-the-zodiac": ["Obsidian Ring of the Zodiac", 212588, "ring_norm_unique_023"], 
  "oculus-ring": ["Oculus Ring", 212648, "Ring_norm_unique_017"], 
  "pandemonium-loop": ["Pandemonium Loop", 298096, "ring_norm_unique_033"], 
  "puzzle-ring": ["Puzzle Ring", 197837, "Ring_norm_unique_004"], 
  "rechels-ring-of-larceny": ["Rechel's Ring of Larceny", 298091, "ring_norm_unique_028"], 
  "restraint": ["Restraint", 332210, "ring_norm_set_002"], 
  "rogars-huge-stone": ["Rogar's Huge Stone", 298090, "ring_norm_unique_027"], 
  "skull-grasp": ["Skull Grasp", 212618, "Ring_norm_unique_022"], 
  "stolen-ring": ["Stolen Ring", 197839, "Ring_norm_unique_005"], 
  "the-compass-rose": ["The Compass Rose", 212587, "Ring_norm_unique_013"], 
  "the-wailing-host": ["The Wailing Host", 212650, "ring_norm_unique_014"], 
  "wyrdward": ["Wyrdward", 298089, "ring_norm_unique_026"], 
  "zunimassas-pox": ["Zunimassa's Pox", 212579, "Ring_norm_unique_012"], 
  // Shield
  "covens-criterion": ["Coven�s Criterion", 298191, "shield_norm_unique_15"], 
  "defender-of-westmarch": ["Defender of Westmarch", 298182, "shield_norm_unique_09"], 
  "denial": ["Denial", 152666, "Shield_norm_unique_03"], 
  "eberli-charo": ["Eberli Charo", 298186, "shield_norm_unique_10"], 
  "freeze-of-deflection": ["Freeze of Deflection", 61550, "Shield_norm_unique_01"], 
  "hallowed-barricade": ["Hallowed Barricade", 223758, "Shield_norm_set_01"],  // IsCrafted
  "ivory-tower": ["Ivory Tower", 197478, "Shield_norm_unique_08"], 
  "lidless-wall": ["Lidless Wall", 195389, "Shield_norm_unique_07"], 
  "stormshield": ["Stormshield", 192484, "Shield_norm_unique_06"], 
  "votoyias-spiker": ["Vo'Toyias Spiker", 298188, "shield_norm_unique_12"], 
  "wall-of-bone": ["Wall of Bone", 152667, "Shield_norm_unique_04"],  // IsCrafted
  // Shoulders
  "ashearas-custodian": ["Asheara's Custodian", 225132, "shoulderPads_norm_unique_07"],  // IsCrafted
  "aughilds-power": ["Aughild's Power", 224051, "shoulderPads_norm_set_02"],  // IsCrafted
  "borns-impunity": ["Born's Impunity", 222948, "shoulderPads_norm_set_01"],  // IsCrafted
  "bottomless-potion-of-the-tower": ["Bottomless Potion of the Tower", 336989, "shoulderpads_norm_set_05"], 
  "burden-of-the-invoker": ["Burden of the Invoker", 335029, "shoulderpads_norm_set_12"], 
  "corruption": ["Corruption", 223619, "shoulderPads_norm_unique_04"],  // IsCrafted
  "death-watch-mantle": ["Death Watch Mantle", 200310, "shoulderPads_norm_unique_02"], 
  "demons-aileron": ["Demon's Aileron", 224397, "shoulderPads_norm_unique_06"],  // IsCrafted
  "firebirds-pinions": ["Firebird's Pinions", 358792, "shoulderpads_norm_set_06"], 
  "flesh-tearer": ["Flesh Tearer", 358801, "shoulderpads_norm_set_10"], 
  "helltooth-mantle": ["Helltooth Mantle", 340525, "shoulderpads_norm_set_16"], 
  "homing-pads": ["Homing Pads", 198573, "shoulderPads_norm_unique_01"], 
  "infernal-staff-of-herding": ["Infernal Staff of Herding", 253990, "shoulderpads_hell_base_08"], 
  "jade-harvesters-joy": ["Jade Harvester's Joy", 338042, "shoulderpads_norm_set_09"], 
  "mantle-of-the-upside-down-sinners": ["Mantle of the Upside-Down Sinners", 338036, "shoulderpads_norm_set_08"], 
  "marauders-spines": ["Marauder's Spines", 336996, "shoulderpads_norm_set_07"], 
  "pauldrons-of-the-skeleton-king": ["Pauldrons of the Skeleton King", 298164, "shoulderpads_norm_unique_11"], 
  "profane-pauldrons": ["Profane Pauldrons", 298158, "shoulderpads_norm_unique_08"], 
  "rolands-mantle": ["Roland's Mantle", 404699, "p1_shoulderPads_norm_set_01"], 
  "spaulders-of-zakara": ["Spaulders of Zakara", 298163, "shoulderpads_norm_unique_09"], 
  "spires-of-the-earth": ["Spires of the Earth", 340526, "shoulderpads_norm_set_15"], 
  "sunwukos-balance": ["Sunwuko's Balance", 336175, "shoulderpads_norm_set_11"], 
  "vile-ward": ["Vile Ward", 201325, "shoulderPads_norm_unique_03"], 
  // Spear
  "akanesh-the-herald-of-righteousness": ["Akanesh, the Herald of Righteousness", 272043, "spear_norm_unique_05"], 
  "arreats-law": ["Arreat's Law", 191446, "Spear_norm_unique_01"], 
  "empyrean-messenger": ["Empyrean Messenger", 194241, "spear_norm_unique_02"], 
  "scrimshaw": ["Scrimshaw", 197095, "Spear_norm_unique_04"], 
  "the-three-hundredth-spear": ["The Three Hundredth Spear", 196638, "Spear_norm_unique_03"], 
  // SpiritStone
  "bezoar-stone": ["Bezoar Stone", 222306, "spiritStone_norm_unique_11"], 
  "erlang-shen": ["Erlang Shen", 222173, "spiritStone_norm_unique_07"], 
  "eye-of-peshkov": ["Eye of Peshkov", 299464, "spiritstone_norm_unique_16"], 
  "gyana-na-kashu": ["Gyana Na Kashu", 222169, "spiritStone_norm_unique_05"], 
  "innas-radiance": ["Inna's Radiance", 222307, "spiritStone_norm_unique_08"], 
  "kekegis-unbreakable-spirit": ["Kekegi's Unbreakable Spirit", 299461, "spiritstone_norm_unique_15"], 
  "madstone": ["Madstone", 221572, "spiritStone_norm_unique_03"], 
  "see-no-evil": ["See No Evil", 222171, "spiritStone_norm_unique_13"], 
  "the-eye-of-the-storm": ["The Eye of the Storm", 222170, "spiritStone_norm_unique_02"], 
  "the-laws-of-seph": ["The Laws of Seph", 299454, "spiritstone_norm_unique_14"], 
  "the-minds-eye": ["The Mind's Eye", 222172, "spiritstone_norm_unique_06"], 
  "tzo-krins-gaze": ["Tzo Krin's Gaze", 222305, "spiritStone_norm_unique_12"], 
  // Staff
  "ahavarion-spear-of-lycander": ["Ahavarion, Spear of Lycander", 271768, "staff_norm_unique_08"], 
  "autumns-call": ["Autumn's Call", 184228, "staff_norm_unique_03"], 
  "harrington-waistguard": ["Harrington Waistguard", 210432, "StaffOfCow"],  // IsCrafted
  "infernal-staff-of-herding": ["Infernal Staff of Herding", 361989, "Crafting_Looted_Reagent_05"], 
  "maloths-focus": ["Maloth's Focus", 193832, "Staff_norm_unique_06"], 
  "the-broken-staff": ["The Broken Staff", 59601, "staff_norm_unique_01"], 
  "the-grand-vizier": ["The Grand Vizier", 192167, "Staff_norm_unique_04"], 
  "the-smoldering-core": ["The Smoldering Core", 271774, "staff_norm_unique_10"], 
  "the-tormentor": ["The Tormentor", 193066, "staff_norm_unique_05"], 
  "valtheks-rebuke": ["Valthek's Rebuke", 271773, "staff_norm_unique_09"], 
  "wormwood": ["Wormwood", 195407, "Staff_norm_unique_07"], 
  // Sword
  "azurewrath": ["Azurewrath", 192511, "sword_norm_unique_06"], 
  "borns-furious-wrath": ["Born's Furious Wrath", 223408, "Sword_norm_set_01"],  // IsCrafted
  "devil-tongue": ["Devil Tongue", 189552, "Sword_norm_unique_05"], 
  "doombringer": ["Doombringer", 185397, "sword_norm_unique_07"], 
  "exarian": ["Exarian", 271617, "sword_norm_unique_13"], 
  "fulminator": ["Fulminator", 271631, "sword_norm_unique_15"], 
  "gift-of-silaria": ["Gift of Silaria", 271630, "sword_norm_unique_14"], 
  "griswolds-perfection": ["Griswold's Perfection", 270977, "Sword_norm_unique_10"],  // IsCrafted
  "infernal-staff-of-herding": ["Infernal Staff of Herding", 270978, "Sword_norm_unique_11"], 
  "little-rogue": ["Little Rogue", 313291, "sword_norm_set_03"], 
  "monster-hunter": ["Monster Hunter", 115140, "Sword_norm_unique_01"], 
  "rimeheart": ["Rimeheart", 271636, "sword_norm_unique_20"], 
  "sever": ["Sever", 115141, "Sword_norm_unique_02"], 
  "shard-of-hate": ["Shard of Hate", 376463, "sword_norm_promo_02"], 
  "skycutter": ["Skycutter", 182347, "Sword_norm_unique_04"], 
  "spectrum": ["Spectrum", 200558, "Sword_norm_unique_09"], 
  "the-ancient-bonesaber-of-zumakalis": ["The Ancient Bonesaber of Zumakalis", 194481, "Sword_norm_unique_08"], 
  "the-slanderer": ["The Slanderer", 313290, "sword_norm_set_02"], 
  "thunderfury-blessed-blade-of-the-windseeker": ["Thunderfury, Blessed Blade of the Windseeker", 229716, "sword_norm_unique_12"], 
  // TwoHandAxe
  "burst-of-wrath": ["Burst of Wrath", 271601, "twohandedaxe_norm_unique_09"], 
  "butchers-carver": ["Butcher's Carver", 186494, "twoHandedAxe_norm_unique_03"], 
  "cinder-switch": ["Cinder Switch", 6329, "twoHandedAxe_norm_unique_01"],  // IsCrafted
  "messerschmidts-reaver": ["Messerschmidt's Reaver", 191065, "twoHandedAxe_norm_unique_04"], 
  "skorn": ["Skorn", 192887, "twoHandedAxe_norm_unique_05"], 
  "the-executioner": ["The Executioner", 186560, "twoHandedAxe_norm_unique_02"], 
  // TwoHandMace
  "arthefs-spark-of-life": ["Arthef�s Spark of Life", 59633, "twoHandedMace_norm_unique_01"], 
  "crushbane": ["Crushbane", 99227, "twoHandedMace_norm_unique_02"], 
  "schaefers-hammer": ["Schaefer's Hammer", 197717, "twoHandedMace_norm_unique_07"], 
  "skywarden": ["Skywarden", 190840, "twohandedmace_norm_unique_03"], 
  "sledge-of-athskeleng": ["Sledge of Athskeleng", 190866, "twoHandedMace_norm_unique_04"], 
  "soulsmasher": ["Soulsmasher", 271671, "twohandedmace_norm_unique_09"], 
  "the-furnace": ["The Furnace", 271666, "twohandedmace_norm_unique_08"], 
  "wrath-of-the-bone-king": ["Wrath of the Bone King", 191584, "twoHandedMace_norm_unique_06"], 
  // TwoHandMightyWeapon
  "bastions-revered": ["Bastion's Revered", 195690, "mightyWeapon_2H_norm_unique_03"], 
  "immortal-kings-boulder-breaker": ["Immortal King's Boulder Breaker", 210678, "mightyWeapon_2H_norm_unique_10"], 
  "madawcs-sorrow": ["Madawc's Sorrow", 272012, "mightyweapon_2h_norm_unique_11"], 
  "the-gavel-of-judgment": ["The Gavel of Judgment", 193657, "mightyWeapon_2H_norm_unique_01"], 
  // TwoHandSword
  "blackguard": ["Blackguard", 270979, "twoHandedSword_norm_unique_10"], 
  "blade-of-prophecy": ["Blade of Prophecy", 184184, "twoHandedSword_norm_unique_03"], 
  "cams-rebuttal": ["Cam's Rebuttal", 271644, "twohandedsword_norm_unique_12"], 
  "faithful-memory": ["Faithful Memory", 198960, "twoHandedSword_norm_unique_09"], 
  "maximus": ["Maximus", 184187, "twoHandedSword_norm_unique_04"], 
  "scourge": ["Scourge", 181511, "twoHandedSword_norm_unique_06"], 
  "stalgards-decimator": ["Stalgard's Decimator", 271639, "twohandedsword_norm_unique_11"], 
  "the-grandfather": ["The Grandfather", 190360, "twohandedsword_norm_unique_08"], 
  "the-sultan-of-blinding-sand": ["The Sultan of Blinding Sand", 184190, "twoHandedSword_norm_unique_05"], 
  "the-zweihander": ["The Zweihander", 59665, "twohandedsword_norm_unique_01"], 
  "warmonger": ["Warmonger", 181495, "twoHandedSword_norm_unique_07"], 
  // VoodooMask
  "aughilds-power": ["Aughild's Power", 299443, "voodoomask_norm_unique_08"], 
  "carnevil": ["Carnevil", 299442, "voodoomask_norm_unique_07"], 
  "quetzalcoatl": ["Quetzalcoatl", 204136, "voodooMask_norm_base_05"], 
  "split-tusk": ["Split Tusk", 221167, "voodooMask_norm_unique_03"], 
  "the-grin-reaper": ["The Grin Reaper", 221166, "voodooMask_norm_unique_01"], 
  "tiklandian-visage": ["Tiklandian Visage", 221382, "voodooMask_norm_unique_06"], 
  "visage-of-giyua": ["Visage of Giyua", 221168, "voodooMask_norm_unique_05"], 
  "zunimassas-vision": ["Zunimassa's Vision", 221202, "voodooMask_norm_unique_04"], 
  // Wand
  "atrophy": ["Atrophy", 182081, "Wand_norm_unique_05"],  // IsCrafted
  "blackhand-key": ["Blackhand Key", 193355, "wand_norm_unique_06"], 
  "chantodos-will": ["Chantodo's Will", 210479, "Wand_norm_unique_07"], 
  "fragment-of-destiny": ["Fragment of Destiny", 181995, "Wand_norm_unique_02"], 
  "gesture-of-orpheus": ["Gesture of Orpheus", 182071, "Wand_norm_unique_03"], 
  "serpents-sparker": ["Serpent's Sparker", 272084, "wand_norm_unique_02"], 
  "sloraks-madness": ["Slorak's Madness", 181982, "Wand_norm_unique_01"], 
  "starfire": ["Starfire", 182074, "Wand_norm_unique_04"], 
  "wand-of-woh": ["Wand of Woh", 272086, "wand_norm_unique_06"], 
  // WizardHat
  "archmages-vicalyke": ["Archmage's Vicalyke", 299471, "wizardhat_norm_unique_06"], 
  "dark-mages-shade": ["Dark Mage's Shade", 224908, "wizardhat_norm_unique_05"], 
  "infernal-staff-of-herding": ["Infernal Staff of Herding", 367201, "wizardhat_norm_base_02"], 
  "storm-crow": ["Storm Crow", 220694, "wizardHat_norm_unique_04"], 
  "the-magistrate": ["The Magistrate", 325579, "wizardhat_norm_unique_08"], 
  "the-swami": ["The Swami", 218681, "wizardhat_norm_unique_03"], 
  "velvet-camaral": ["Velvet Camaral", 299472, "wizardhat_norm_unique_07"], 
}