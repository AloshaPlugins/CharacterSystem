using CharacterSystem.Models;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Permissions;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace CharacterSystem
{
    public class Main : RocketPlugin<Config>
    {
        public List<CharacterScreen> Screens = new List<CharacterScreen>();
        protected override void Load()
        {
            UnturnedPermissions.OnJoinRequested += UnturnedPermissionsOnOnJoinRequested;
            U.Events.OnPlayerConnected += EventsOnOnPlayerConnected;
            U.Events.OnPlayerDisconnected += EventsOnOnPlayerDisconnected;
            EffectManager.onEffectButtonClicked += OnEffectButtonClicked;
            EffectManager.onEffectTextCommitted += OnEffectTextCommitted;
        }

        protected override void Unload()
        {
            UnturnedPermissions.OnJoinRequested -= UnturnedPermissionsOnOnJoinRequested;
            U.Events.OnPlayerConnected -= EventsOnOnPlayerConnected;
            U.Events.OnPlayerDisconnected -= EventsOnOnPlayerDisconnected;
            EffectManager.onEffectButtonClicked -= OnEffectButtonClicked;
            EffectManager.onEffectTextCommitted -= OnEffectTextCommitted;
            Screens.Clear();
        }

        private void OnEffectButtonClicked(Player player, string buttonname)
        {
            var data = Screens.FirstOrDefault(character =>
                character.Id == player.channel.owner.playerID.steamID);
            if (data == null) return;

            if (buttonname == "Karakter_Olustur")
            {
                if (!YazıKontrol(data.Character.Name) || !YazıKontrol(data.Character.Surname) ||
                    string.IsNullOrWhiteSpace(data.Character.History) || string.IsNullOrWhiteSpace(data.Character.Age.ToString())) return;
                data.Character.Id = player.channel.owner.playerID.steamID.m_SteamID;
                Configuration.Instance.Characters.Add(data.Character);
                Configuration.Save();
                Screens.RemoveAll(screen => screen.Id == data.Id);
                var untPlayer = UnturnedPlayer.FromPlayer(player);
                untPlayer.Kick(Configuration.Instance.AtılmaMesajı);
                return;
            }
            if (buttonname == "K_Geri")
            {
                EffectManager.sendUIEffect(27300, 894, player.channel.owner.playerID.steamID, true);
                return;
            }

            if (buttonname.Contains("Skin_Tone"))
            {
                for (int i = 0; i < 10; i++)
                {
                    if (buttonname == "Skin_Tone_" + i)
                    {
                        data.Character.SkinColor = i;
                        EffectManager.sendUIEffect(27301, 894, player.channel.owner.playerID.steamID, true);
                        for (int j = 0; j < (!player.channel.owner.isPro ? 5 : 23); j++) EffectManager.sendUIEffectVisibility(894, player.channel.owner.playerID.steamID, true, "Hair_" + j, true);
                        return;
                    }
                }
            }

            if (buttonname.Contains("Hair_Tone"))
            {
                for (int i = 0; i < 10; i++)
                {
                    if (buttonname == "Hair_Tone_" + i)
                    {
                        data.Character.OtherColor = i;
                        EffectManager.sendUIEffect(27304, 894, player.channel.owner.playerID.steamID, true);
                        return;
                    }
                }
            }

            if (buttonname.Contains("Hair"))
            {
                for (int i = 0; i < 23; i++)
                {
                    if (buttonname == "Hair_" + i)
                    {
                        data.Character.Hair = i;
                        EffectManager.sendUIEffect(27302, 894, player.channel.owner.playerID.steamID, true);
                        for (int j = 0; j < (!player.channel.owner.isPro ? 5 : 16); j++) EffectManager.sendUIEffectVisibility(894, player.channel.owner.playerID.steamID, true, "Bear_" + j, true);
                        return;
                    }
                }
            }

            if (buttonname.Contains("Bear"))
            {
                for (int i = 0; i < 16; i++)
                {
                    if (buttonname == "Bear_" + i)
                    {
                        data.Character.Bear = i;
                        EffectManager.sendUIEffect(27303, 894, player.channel.owner.playerID.steamID, true);
                        return;
                    }
                }
            }
        }
        private void OnEffectTextCommitted(Player player, string buttonname, string text)
        {
            var data = Screens.FirstOrDefault(character =>
                character.Id == player.channel.owner.playerID.steamID);
            if (data == null) return;

            if (buttonname == "Name_Input" && YazıKontrol(text))
            {
                data.Character.Name = text;
            }
            else if (buttonname == "Surname_Input" && YazıKontrol(text))
            {
                data.Character.Surname = text;
            }
            else if (buttonname == "Age_Input" && int.TryParse(text, out var result))
            {
                data.Character.Age = result;
            }
            else if (buttonname == "History_Input" && text.Length > 0)
            {
                data.Character.History = text;
            }
        }

        public bool YazıKontrol(string içerik)
        {
            if (string.IsNullOrWhiteSpace(içerik)) return false;
            if (içerik.Length < Configuration.Instance.MinimumKarakter || içerik.Length > Configuration.Instance.MaximumKarakter) return false;
            return true;
        }

        private void UnturnedPermissionsOnOnJoinRequested(CSteamID player, ref ESteamRejection? rejectionreason)
        {
            try
            {
                SteamPending steamPending = Provider.pending.Find(s => s.playerID.steamID == player);
                var playerCharacter = Configuration.Instance.Characters.FirstOrDefault(character =>
                    character.Id == steamPending.playerID.steamID.m_SteamID);
                if (playerCharacter == null) return;

                steamPending.playerID.characterName = $"{playerCharacter.Name} {playerCharacter.Surname}";

                steamPending.GetType().GetField("_skin", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(steamPending, SkinColors[playerCharacter.SkinColor]);
                steamPending.GetType().GetField("_beard", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(steamPending, Bears[playerCharacter.Bear]);
                steamPending.GetType().GetField("_hair", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(steamPending, Hairs[playerCharacter.Hair]);
                steamPending.GetType().GetField("_color", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(steamPending, OtherColors[playerCharacter.OtherColor]);
            }
            catch (Exception istisna)
            {
                Rocket.Core.Logging.Logger.LogException(istisna, "Bir hata oluştu;\r\n");
            }
        }

        private void EventsOnOnPlayerConnected(UnturnedPlayer player)
        {
            if (Configuration.Instance.Characters.Any(character =>
                character.Id == player.CSteamID.m_SteamID)) return;
            var characterScreen = new CharacterScreen();
            characterScreen.Id = player.CSteamID;
            characterScreen.Character = new AloshaCharacter();
            Screens.Add(characterScreen);
            SendCharacterMenu(player.Player, characterScreen);
        }

        private void EventsOnOnPlayerDisconnected(UnturnedPlayer player)
        {
            if (!Screens.Any(character =>
                character.Id == player.CSteamID)) return;
            Screens.RemoveAll(screen => screen.Id == player.CSteamID);
        }

        public void SendCharacterMenu(Player player, CharacterScreen screen)
        {
            EffectManager.sendUIEffect(27300, 894, player.channel.owner.playerID.steamID, true);
            player.setPluginWidgetFlag(EPluginWidgetFlags.Modal, true);
        }

        public static Color[] SkinColors = new Color[]
        {
            new Color32(244, 230, 210, byte.MaxValue),
            new Color32(217, 202, 180, byte.MaxValue),
            new Color32(190, 165, 130, byte.MaxValue),
            new Color32(157, 136, 106, byte.MaxValue),
            new Color32(148, 118, 75, byte.MaxValue),
            new Color32(112, 96, 73, byte.MaxValue),
            new Color32(83, 71, 53, byte.MaxValue),
            new Color32(75, 61, 50, byte.MaxValue),
            new Color32(50, 42, 38, byte.MaxValue),
            new Color32(34, 28, 28, byte.MaxValue)
        };

        public static Color[] OtherColors = new Color[]
        {
            new Color32(215, 215, 215, byte.MaxValue),
            new Color32(193, 193, 193, byte.MaxValue),
            new Color32(205, 192, 140, byte.MaxValue),
            new Color32(172, 106, 56, byte.MaxValue),
            new Color32(102, 79, 56, byte.MaxValue),
            new Color32(86, 69, 46, byte.MaxValue),
            new Color32(71, 56, 38, byte.MaxValue),
            new Color32(53, 42, 34, byte.MaxValue),
            new Color32(56, 56, 56, byte.MaxValue),
            new Color32(22, 22, 22, byte.MaxValue)
        };

        public static byte[] Bears = new byte[]
        {
            0,
            1,
            2,
            3,
            4,
            5,
            6,
            7,
            8,
            9,
            10,
            11,
            12,
            13,
            14,
            15
        };

        public static byte[] Hairs = new byte[]
        {
            0,
            1,
            2,
            3,
            4,
            5,
            6,
            7,
            8,
            9,
            10,
            11,
            12,
            13,
            14,
            15,
            16,
            17,
            18,
            19,
            20,
            21,
            22
        };
    }
}
