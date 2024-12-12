using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.AddressableAssets;
using UnityEngine;
using static CaeliImperiumPlugin.CaeliImperium;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using RiskOfOptions;
using System.Xml.Linq;
using BepInEx.Configuration;

namespace CaeliImperium.Items
{
    internal static class Chalk //: ItemBase<FirstItem>
    {
        internal static GameObject ChalkPrefab;
        internal static Sprite ChalkIcon;
        public static ItemDef ChalkItemDef;
        public static string name = "Chalk";
        public static ConfigEntry<bool> ChalkEnable;
        public static ConfigEntry<bool> ChalkEnableConfig;
        public static ConfigEntry<bool> ChalkAIBlacklist;
        public static ConfigEntry<float> ChalkTier;
		public static ConfigEntry<float> ChalkDamage;
		public static ConfigEntry<float> ChalkTimescale;

		internal static void Init()
        {
			AddConfigs();
			string tier = "Assets/Icons/ChalkTier1.png";
			switch (ConfigFloat(ChalkTier, ChalkEnableConfig))
			{
				case 1:
					tier = "Assets/Icons/ChalkTier1.png";
					break;
				case 2:
					tier = "Assets/Icons/ChalkTier2.png";
					break;
				case 3:
					tier = "Assets/Icons/ChalkTier3.png";
					break;

			}
			ChalkPrefab = MainAssets.LoadAsset<GameObject>("Assets/Models/Prefabs/Chalk.prefab");
            ChalkIcon = MainAssets.LoadAsset<Sprite>(tier);
            Item();

            AddLanguageTokens();
        }
		public static void AddConfigs()
		{
			ChalkEnable = Config.Bind<bool>("Item : " + name,
            "Activation",
							 true,
							 "Enable this item?");
			ChalkEnableConfig = Config.Bind<bool>("Item : " + name,
							 "Config Activation",
							 false,
							 "Enable config?\nActivation option and |options under these brackets| are always taken in effect");
			ChalkAIBlacklist = Config.Bind<bool>("Item : " + name,
            "AI Blacklist",
							 false,
							 "Blacklist this item from enemies?");
			ChalkTier = Config.Bind<float>("Item : " + name,
            "Item tier",
            1f,
										 "1: Common/White\n2: Rare/Green\n3: Legendary/Red");
			ChalkDamage = Config.Bind<float>("Item : " + name,
			"Damage",
			15f,
										 "Control the damage increase in percentage");
			ChalkTimescale = Config.Bind<float>("Item : " + name,
			"Time scale",
			4f,
										 "Control the time multiplier in the sinus formula");
			ModSettingsManager.AddOption(new CheckBoxOption(ChalkEnable, new CheckBoxConfig() { restartRequired = true }));
			ModSettingsManager.AddOption(new CheckBoxOption(ChalkEnableConfig));
			ModSettingsManager.AddOption(new CheckBoxOption(ChalkAIBlacklist, new CheckBoxConfig() { restartRequired = true }));
			ModSettingsManager.AddOption(new StepSliderOption(ChalkTier, new StepSliderConfig() { min = 1, max = 3, increment = 1f, restartRequired = true }));
            ModSettingsManager.AddOption(new FloatFieldOption(ChalkDamage));
            ModSettingsManager.AddOption(new FloatFieldOption(ChalkTimescale));
        }
		private static void Item()
        {
            ChalkItemDef = ScriptableObject.CreateInstance<ItemDef>();
            ChalkItemDef.name = "Chalk";
            ChalkItemDef.nameToken = "CHALKE_NAME";
            ChalkItemDef.pickupToken = "CHALKE_PICKUP";
            ChalkItemDef.descriptionToken = "CHALKE_DESC";
            ChalkItemDef.loreToken = "CHALKE_LORE";
            switch (ConfigFloat(ChalkTier, ChalkEnableConfig))
            {
                case 1:
                    ChalkItemDef.deprecatedTier = ItemTier.Tier1;
                    break;
                case 2:
                    ChalkItemDef.deprecatedTier = ItemTier.Tier2;
                    break;
                case 3:
                    ChalkItemDef.deprecatedTier = ItemTier.Tier3;
                    break;

            }
            ChalkItemDef.pickupIconSprite = ChalkIcon;
            ChalkItemDef.pickupModelPrefab = ChalkPrefab;
            ChalkItemDef.canRemove = true;
            ChalkItemDef.hidden = false;
			ChalkItemDef.requiredExpansion = CaeliImperiumExpansionDef;
			var tags = new List<ItemTag>() { ItemTag.Damage };
			if (ConfigBool(ChalkAIBlacklist, ChalkEnableConfig))
			{
				tags.Add(ItemTag.AIBlacklist);
			}
			ChalkItemDef.tags = tags.ToArray();
			var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(ChalkItemDef, displayRules));
            On.RoR2.HealthComponent.TakeDamageProcess += SinusDamage;
        }

		private static void SinusDamage(On.RoR2.HealthComponent.orig_TakeDamageProcess orig, HealthComponent self, DamageInfo damageInfo)
		{
			if (damageInfo != null && self != null && damageInfo.attacker && self.body && !damageInfo.rejected)
			{
				var attackerBody = damageInfo.attacker ? damageInfo.attacker.GetComponent<CharacterBody>() : null;
				int itemCount = 0;
				if (attackerBody != null)
				{
					try
					{
						itemCount = attackerBody.inventory ? attackerBody.inventory.GetItemCount(ChalkItemDef) : 0;

					}
					catch
					{
					}
				}

				if (itemCount > 0)
				{
                    damageInfo.damage += ((float)Math.Sin(Time.time * ConfigFloat(ChalkTimescale, ChalkEnableConfig)) * itemCount * (ConfigFloat(ChalkDamage, ChalkEnableConfig) / 200) * damageInfo.damage) + (itemCount * (ConfigFloat(ChalkDamage, ChalkEnableConfig) / 200) * damageInfo.damage);
				}
			}
			orig(self, damageInfo);	

		}

		private static void AddLanguageTokens()
        {
            LanguageAPI.Add("CHALKE_NAME", "Chalk");
            LanguageAPI.Add("CHALKE_PICKUP", "Gain damage bonus by sinus of current time");
            LanguageAPI.Add("CHALKE_DESC", "Boost your <style=cIsDamage>outcoming damage</style> by <style=cIsDamage>" + ConfigFloat(ChalkDamage, ChalkEnableConfig) + "%</style> <style=cStack>(+" + ConfigFloat(ChalkDamage, ChalkEnableConfig) + " per item stack)</style> and multiplies it by the sinus of current time");
            LanguageAPI.Add("CHALKE_LORE", "");
        }
    }
}
