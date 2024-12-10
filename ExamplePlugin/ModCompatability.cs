using CaeliImperium.Items;
using ProperSave;
using RoR2;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using static CaeliImperium.Items.CapturedPotential;
using UnityEngine;
using System.Linq;

namespace CaeliImperium
{
    public class ModCompatability
    {
        public static class ProperSaveCompatibility
        {
            private static bool? _enabled;

            public static bool enabled
            {
                get
                {
                    if (_enabled == null)
                    {
                        _enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.KingEnderBrine.ProperSave");
                    }
                    return (bool)_enabled;
                }
            }

            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            public static void SomeMethodThatRequireTheDependencyToBeHere()
            {
                if (ModCompatability.ProperSaveCompatibility.enabled) { }
            }
        }


        public static class EmotesCompatibility
        {
            private static bool? _brynzaEmotesEnabled;

            public static bool brynzaEmotesEnabled
            {
                get
                {
                    if (_brynzaEmotesEnabled == null)
                    {
                        _brynzaEmotesEnabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.brynzananas.brynzaemotes");
                    }
                    return (bool)_brynzaEmotesEnabled;
                }
            }
            private static bool? _badassEmotesEnabled;

            public static bool badassEmotesEnabled
            {
                get
                {
                    if (_badassEmotesEnabled == null)
                    {
                        _badassEmotesEnabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.weliveinasociety.badassemotes");
                    }
                    return (bool)_badassEmotesEnabled;
                }
            }

            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            public static void SomeMethodThatRequireTheDependencyToBeHere()
            {
                // stuff that require the dependency to be loaded
            }
        }
    }
}






