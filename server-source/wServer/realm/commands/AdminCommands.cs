#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wServer.networking;
using wServer.networking.svrPackets;
using wServer.realm.entities;
using wServer.realm.entities.player;
using wServer.realm.setpieces;
using wServer.realm.worlds;

#endregion


namespace wServer.realm.commands
{
    internal class TestCommand : Command
    {
        public TestCommand()
            : base("t", 1)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            Entity en = Entity.Resolve(player.Manager, "Zombie Wizard");
            en.Move(player.X, player.Y);
            player.Owner.EnterWorld(en);
            player.UpdateCount++;
            //player.Client.SendPacket(new DeathPacket
            //{
            //    AccountId = player.AccountId,
            //    CharId = player.Client.Character.CharacterId,
            //    Killer = "mountains.beholder",
            //    obf0 = 10000,
            //    obf1 = 10000
            //});
            return true;
        }
    }

    /*internal class AddGiftCodeCommand : Command
    {
        public AddGiftCodeCommand()
            : base("gcode", 1)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            try
            {
                if (!String.IsNullOrWhiteSpace(args[0]))
                    player.Manager.FindPlayer(args[0])?.Client.GiftCodeReceived("LevelUp");
                else
                    player.Client.GiftCodeReceived("LevelUp");
            }
            catch (Exception) { }
            return true;
        }
    }*/

    /*internal class TagCommand : Command
    {
        public TagCommand() : base("tag") { }
        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (string.IsNullOrEmpty(args[0]))
            {
                player.SendHelp("Usage: /tag <TagYouWant>");
                return false;
            }
            if (args.Length == 1)
            {  
                player.Manager.Database.DoActionAsync(db =>
                {
                    player.Name = "[" + args[0] + "] " + player.Client.Account.Name;
                });
                player.SendInfo("Your tag is now: " + args[0]);
            }
            return true;
        }
    }*/

    internal class freeze : Command
    {
        public freeze()
            : base("freeze", Ranks.Staff)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (args.Length == 0)
            {
                player.SendHelp("Usage: /freeze <playername>");
                return false;
            }
            foreach (KeyValuePair<string, Client> i in player.Manager.Clients)
            {
                if (i.Value.Player.Name.EqualsIgnoreCase(args[0]))
                {
                    i.Value.Player.ApplyConditionEffect(new ConditionEffect
                    {
                        Effect = ConditionEffectIndex.Paralyzed,
                        DurationMS = -1
                    });
                    i.Value.Player.ApplyConditionEffect(new ConditionEffect
                    {
                        Effect = ConditionEffectIndex.Paused,
                        DurationMS = -1
                    });
                    i.Value.Player.UpdateCount++;
                    player.SendInfo("Player frozen!");
                    return true;
                }
            }
            player.SendError("Player could not be found");
            return false;
        }
    }



    internal class Heal : Command
    {
        public Heal()
            : base("heal", Ranks.DL7)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (args.Length == 0)
            {
                player.SendHelp("Usage: /heal <playername>");
                return false;
            }
            foreach (Client i in player.Manager.Clients.Values)
            {
                if (i.Account.Name.EqualsIgnoreCase(args[0]))
                {
                    int max = i.Player.MaxHp;
                    i.Player.HP += (i.Player.HP + 100);
                    player.SendInfo("Player healed!");
                    return true;
                }
            }
            player.SendError("Player could not be found");
            return false;
        }
    }

    internal class VanishCommand : Command
    {
        public VanishCommand()
        : base("vanish", Ranks.Designer)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (!player.isNotVisible)
            {
                player.isNotVisible = true;
                player.Owner.PlayersCollision.Remove(player);
                if (player.Pet != null)
                    player.Owner.LeaveWorld(player.Pet);
                player.SendInfo("You're now hidden from all players!");
                return true;
            }
            player.isNotVisible = false;

            player.SendInfo("You're now visible to all players!");
            return true;
        }
    }

    internal class VisitCommand : Command
    {
        public VisitCommand()
            : base("visit", Ranks.Staff)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (args.Length < 1)
            {
                player.SendHelp("Usage: /visit <playername>");
                return false;
            }
            else if (String.Equals(player.Name.ToLower(), args[0].ToLower()))
            {
                player.SendInfo("You are You.");
                return false;
            }
            foreach (KeyValuePair<string, Client> i in player.Manager.Clients)
            {
                if (i.Value.Player.Owner is PetYard)
                {
                    player.SendInfo("Player is in his Pet Yard you cannot visit him.");
                    return false;
                }
                else if (i.Value.Player.Name.EqualsIgnoreCase(args[0]))
                {
                    if (player.Owner == i.Value.Player.Owner)
                    {
                        player.SendInfo("You are already in the same world as this Player.");
                        return false;
                    }
                    else
                    {
                        player.Client.Reconnect(new ReconnectPacket
                        {
                            GameId = i.Value.Player.Owner.Id,
                            Host = "",
                            IsFromArena = false,
                            Key = i.Value.Player.Owner.PortalKey,
                            KeyTime = -1,
                            Name = i.Value.Player.Owner.Name,
                            Port = -1
                        });
                    }
                    return true;
                }
            }
            player.SendError("Player could not be found!");
            return false;
        }
    }

    internal class posCmd : Command
    {
        public posCmd()
            : base("p", Ranks.Staff)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            player.SendInfo("X: " + (int)player.X + " - Y: " + (int)player.Y);
            return true;
        }
    }

    internal class BanCommand : Command
    {
        public BanCommand() : 
            base("ban", Ranks.Staff)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            var p = player.Manager.FindPlayer(args[1]);
            if (p == null)
            {
                player.SendError("Player not found");
                return false;
            }
            player.Manager.Database.DoActionAsync(db =>
            {
                var cmd = db.CreateQuery();
                cmd.CommandText = "UPDATE accounts SET banned=1 WHERE id=@accId;";
                cmd.Parameters.AddWithValue("@accId", p.AccountId);
                cmd.ExecuteNonQuery();
            });
            return true;
        }
    }


    internal class AddWorldCommand : Command
    {
        public AddWorldCommand()
            : base("addworld", Ranks.Staff)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            Task.Factory.StartNew(() => GameWorld.AutoName(1, true)).ContinueWith(_ => player.Manager.AddWorld(_.Result), TaskScheduler.Default);
            return true;
        }
    }

    internal class EarthQuake : Command
    {
        public EarthQuake()
            : base("shake", Ranks.Staff)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (args.Length == 0)
            {
                player.SendHelp("Usage: /shake <playername>");
                return false;
            }
            foreach (KeyValuePair<string, Client> i in player.Manager.Clients)
            {
                Packet pkt;

                if (i.Value.Player.Name.EqualsIgnoreCase(args[0]))
                {
                    pkt = new ShowEffectPacket { EffectType = EffectType.Earthquake };
                    i.Value.Player.UpdateCount++;
                    player.SendInfo("Player shaken!");
                    i.Value.SendPacket(pkt);
                    return true;
                }
            }
            player.SendError("Player could not be found Or Player info not found!");
            return false;
        }
    }

    internal class SpawnCommand : Command
    {
        public SpawnCommand()
            : base("spawn", Ranks.Staff)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            int num;
            if (args.Length > 0 && int.TryParse(args[0], out num)) //multi
            {
                string name = string.Join(" ", args.Skip(1).ToArray());
                ushort objType;
                //creates a new case insensitive dictionary based on the XmlDatas
                Dictionary<string, ushort> icdatas = new Dictionary<string, ushort>(
                    player.Manager.GameData.IdToObjectType,
                    StringComparer.OrdinalIgnoreCase);
                if (!icdatas.TryGetValue(name, out objType) ||
                    !player.Manager.GameData.ObjectDescs.ContainsKey(objType))
                {
                    player.SendInfo("Unknown entity!");
                    return false;
                }
                int c = int.Parse(args[0]);
                if (!(player.Client.Account.Rank > 2) && c > 200)
                {
                    player.SendError("Maximum spawn count is set to 200!");
                    return false;
                }
                if (player.Client.Account.Rank > 2 && c > 200)
                {
                    player.SendInfo("Bypass made!");
                }
                for (int i = 0; i < num; i++)
                {
                    Entity entity = Entity.Resolve(player.Manager, objType);
                    entity.Move(player.X, player.Y);
                    player.Owner.EnterWorld(entity);
                }
                player.SendInfo("Success!");
            }
            else
            {
                string name = string.Join(" ", args);
                ushort objType;
                //creates a new case insensitive dictionary based on the XmlDatas
                Dictionary<string, ushort> icdatas = new Dictionary<string, ushort>(
                    player.Manager.GameData.IdToObjectType,
                    StringComparer.OrdinalIgnoreCase);
                if (!icdatas.TryGetValue(name, out objType) ||
                    !player.Manager.GameData.ObjectDescs.ContainsKey(objType))
                {
                    player.SendHelp("Usage: /spawn <entityname>");
                    return false;
                }
                Entity entity = Entity.Resolve(player.Manager, objType);
                entity.Move(player.X, player.Y);
                player.Owner.EnterWorld(entity);
            }
            return true;
        }
    }

    internal class AddEffCommand : Command
    {
        public AddEffCommand()
            : base("addeff", Ranks.Staff)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (args.Length == 0)
            {
                player.SendHelp("Usage: /addeff <Effectname or Effectnumber>");
                return false;
            }
            try
            {
                player.ApplyConditionEffect(new ConditionEffect
                {
                    Effect = (ConditionEffectIndex)Enum.Parse(typeof(ConditionEffectIndex), args[0].Trim(), true),
                    DurationMS = -1
                });
                {
                    player.SendInfo("Success!");
                }
            }
            catch
            {
                player.SendError("Invalid effect!");
                return false;
            }
            return true;
        }
    }

    internal class RemoveEffCommand : Command
    {
        public RemoveEffCommand()
            : base("remeff", Ranks.Staff)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (args.Length == 0)
            {
                player.SendHelp("Usage: /remeff <Effectname or Effectnumber>");
                return false;
            }
            try
            {
                player.ApplyConditionEffect(new ConditionEffect
                {
                    Effect = (ConditionEffectIndex)Enum.Parse(typeof(ConditionEffectIndex), args[0].Trim(), true),
                    DurationMS = 0
                });
                player.SendInfo("Success!");
            }
            catch
            {
                player.SendError("Invalid effect!");
                return false;
            }
            return true;
        }
    }

    internal class GiveCommand : Command
    {
        public GiveCommand()
            : base("give", Ranks.DL9)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (args.Length == 0)
            {
                player.SendHelp("Usage: /give <Itemname>");
                return false;
            }
            string name = string.Join(" ", args.ToArray()).Trim();
            ushort objType;
            //creates a new case insensitive dictionary based on the XmlDatas
            Dictionary<string, ushort> icdatas = new Dictionary<string, ushort>(player.Manager.GameData.IdToObjectType,
                StringComparer.OrdinalIgnoreCase);
            if (!icdatas.TryGetValue(name, out objType))
            {
                player.SendError("Unknown type!");
                return false;
            }
            if (!player.Manager.GameData.Items[objType].Secret || player.Client.Account.Rank >= 4)
            {
                for (int i = 0; i < player.Inventory.Length; i++)
                    if (player.Inventory[i] == null)
                    {
                        player.Inventory[i] = player.Manager.GameData.Items[objType];
                        player.UpdateCount++;
                        player.SaveToCharacter();
                        player.SendInfo("Success!");
                        break;
                    }
            }
            else
            {
                player.SendError("Item cannot be given!");
                return false;
            }
            return true;
        }
    }

    internal class GlandsCommand : Command
    {
        public GlandsCommand()
            : base("glands", Ranks.Standard)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            int x = 1050, y = 1050;
            player.Move(x + 0.5f, y + 0.5f);
            if (player.Pet != null)
                player.Pet.Move(x + 0.5f, y + 0.5f);
            player.UpdateCount++;
            player.Owner.BroadcastPacket(new GotoPacket
            {
                ObjectId = player.Id,
                Position = new Position
                {
                    X = player.X,
                    Y = player.Y
                }
            }, null);
            return true;
        }
    }
   
    internal class TpCommand : Command
    {
        public TpCommand()
            : base("tp", Ranks.Standard)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (args.Length == 0 || args.Length == 1)
            {
                player.SendHelp("Usage: /tp <X coordinate> <Y coordinate>");
            }
            else
            {
                int x, y;
                try
                {
                    x = int.Parse(args[0]);
                    y = int.Parse(args[1]);
                }
                catch
                {
                    player.SendError("Invalid coordinates!");
                    return false;
                }
                player.Move(x + 0.5f, y + 0.5f);
                if (player.Pet != null)
                    player.Pet.Move(x + 0.5f, y + 0.5f);
                player.UpdateCount++;
                player.Owner.BroadcastPacket(new GotoPacket
                {
                    ObjectId = player.Id,
                    Position = new Position
                    {
                        X = player.X,
                        Y = player.Y
                    }
                }, null);
            }
            return true;
        }
    }

    class KillAll : Command
    {
        public KillAll() : base("killAll", Ranks.Staff) { }
        
        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            var iterations = 0;
            var lastKilled = -1;
            var killed = 0;

            var mobName = args.Aggregate((s, a) => string.Concat(s, " ", a));
            while (killed != lastKilled)
            {
                lastKilled = killed;
                foreach (var i in player.Owner.Enemies.Values.Where(e =>
                    e.ObjectDesc?.ObjectId != null && e.ObjectDesc.ObjectId.ContainsIgnoreCase(mobName)))
                {
                    i.Death(time);
                    killed++;
                }
                if (++iterations >= 5)
                    break;
            }

            player.SendInfo($"{killed} enemy killed!");
            return true;
        }
    }

    internal class Kick : Command
    {
        public Kick()
            : base("kick", Ranks.Staff)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (args.Length == 0)
            {
                player.SendHelp("Usage: /kick <playername>");
                return false;
            }
            try
            {
                foreach (KeyValuePair<int, Player> i in player.Owner.Players)
                {
                    if (i.Value.Name.ToLower() == args[0].ToLower().Trim())
                    {
                        player.SendInfo("Player Disconnected");
                        i.Value.Client.Disconnect();
                    }
                }
            }
            catch
            {
                player.SendError("Cannot kick!");
                return false;
            }
            return true;
        }
    }

    internal class Mute : Command
    {
        public Mute()
            : base("mute", Ranks.Staff)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (args.Length == 0)
            {
                player.SendHelp("Usage: /mute <playername>");
                return false;
            }
            try
            {
                foreach (KeyValuePair<int, Player> i in player.Owner.Players)
                {
                    if (i.Value.Name.ToLower() == args[0].ToLower().Trim())
                    {
                        i.Value.Muted = true;
                        i.Value.Manager.Database.DoActionAsync(db => db.MuteAccount(i.Value.AccountId));
                        player.SendInfo("Player Muted.");
                    }
                }
            }
            catch
            {
                player.SendError("Cannot mute!");
                return false;
            }
            return true;
        }
    }

    internal class Max : Command
    {
        public Max()
            : base("max", Ranks.DL4)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            try
            {
                player.Stats[0] = player.ObjectDesc.MaxHitPoints;
                player.Stats[1] = player.ObjectDesc.MaxMagicPoints;
                player.Stats[2] = player.ObjectDesc.MaxAttack;
                player.Stats[3] = player.ObjectDesc.MaxDefense;
                player.Stats[4] = player.ObjectDesc.MaxSpeed;
                player.Stats[5] = player.ObjectDesc.MaxHpRegen;
                player.Stats[6] = player.ObjectDesc.MaxMpRegen;
                player.Stats[7] = player.ObjectDesc.MaxDexterity;
                player.SaveToCharacter();
                player.Client.Save();
                player.UpdateCount++;
                player.SendInfo("Success");
            }
            catch
            {
                player.SendError("Error while maxing stats");
                return false;
            }
            return true;
        }
    }

    internal class UnMute : Command
    {
        public UnMute()
            : base("unmute", Ranks.Staff)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (args.Length == 0)
            {
                player.SendHelp("Usage: /unmute <playername>");
                return false;
            }
            try
            {
                foreach (KeyValuePair<int, Player> i in player.Owner.Players)
                {
                    if (i.Value.Name.ToLower() == args[0].ToLower().Trim())
                    {
                        i.Value.Muted = true;
                        i.Value.Manager.Database.DoActionAsync(db => db.UnmuteAccount(i.Value.AccountId));
                        player.SendInfo("Player Unmuted.");
                    }
                }
            }
            catch
            {
                player.SendError("Cannot unmute!");
                return false;
            }
            return true;
        }
    }

    internal class OryxSay : Command
    {
        public OryxSay()
            : base("osay", Ranks.Designer)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (args.Length == 0)
            {
                player.SendHelp("Usage: /oryxsay <saytext>");
                return false;
            }
            string saytext = string.Join(" ", args);
            player.SendEnemy("Oryx the Mad God", saytext);
            return true;
        }
    }

    internal class SWhoCommand : Command //get all players from all worlds (this may become too large!)
    {
        public SWhoCommand()
            : base("swho", Ranks.DL1)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            StringBuilder sb = new StringBuilder("All conplayers: ");

            foreach (KeyValuePair<int, World> w in player.Manager.Worlds)
            {
                World world = w.Value;
                if (w.Key != 0)
                {
                    Player[] copy = world.Players.Values.ToArray();
                    if (copy.Length != 0)
                    {
                        for (int i = 0; i < copy.Length; i++)
                        {
                            sb.Append(copy[i].Name);
                            sb.Append(", ");
                        }
                    }
                }
            }
            string fixedString = sb.ToString().TrimEnd(',', ' '); //clean up trailing ", "s

            player.SendInfo(fixedString);
            return true;
        }
    }

    internal class Announcement : Command
    {
        public Announcement()
            : base("announce", Ranks.Designer)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (args.Length == 0)
            {
                player.SendHelp("Usage: /announce <saytext>");
                return false;
            }
            string saytext = string.Join(" ", args);

            foreach (Client i in player.Manager.Clients.Values)
            {
                i.SendPacket(new TextPacket
                {
                    BubbleTime = 0,
                    Stars = -1,
                    Name = "@ANNOUNCEMENT",
                    Text = " " + saytext
                });
            }
            return true;
        }
    }

    internal class Summon : Command
    {
        public Summon()
            : base("summon", Ranks.Staff)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (player.Owner is Vault || player.Owner is PetYard)
            {
                player.SendInfo("You cant summon in this world.");
                return false;
            }
            foreach (KeyValuePair<string, Client> i in player.Manager.Clients)
            {
                if (i.Value.Player.Name.EqualsIgnoreCase(args[0]))
                {
                    Packet pkt;
                    if (i.Value.Player.Owner == player.Owner)
                    {
                        i.Value.Player.Move(player.X, player.Y);
                        pkt = new GotoPacket
                        {
                            ObjectId = i.Value.Player.Id,
                            Position = new Position(player.X, player.Y)
                        };
                        i.Value.Player.UpdateCount++;
                        player.SendInfo("Player summoned!");
                    }
                    else
                    {
                        pkt = new ReconnectPacket
                        {
                            GameId = player.Owner.Id,
                            Host = "",
                            IsFromArena = false,
                            Key = player.Owner.PortalKey,
                            KeyTime = -1,
                            Name = player.Owner.Name,
                            Port = -1
                        };
                        player.SendInfo("Player will connect to you now!");
                    }

                    i.Value.SendPacket(pkt);

                    return true;
                }
            }
            player.SendError(string.Format("Player '{0}' could not be found!", args));
            return false;
        }
    }

    /*
    internal class PetYardCommand : Command
    {
        public PetYardCommand()
            : base("petyard")
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            World world = player.Client.Manager.AddWorld(new PetYard(player));
            player.Client.Reconnect(new ReconnectPacket
            {
                Host = "",
                Port = Program.Settings.GetValue<int>("port"),
                GameId = world.Id,
                Name = world.Name,
                Key = world.PortalKey,
            });
            return true;
        }
    }
    */

    internal class GiftCommand : Command
    {
        public GiftCommand()
            : base("gift", Ranks.Staff)
        {
        }
        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (args.Length == 1)
            {
                player.SendHelp("Usage: /gift <Playername> <Itemname>");
                return false;
            }
            string name = string.Join(" ", args.Skip(1).ToArray()).Trim();
            var plr = player.Manager.FindPlayer(args[0]);
            ushort objType;
            Dictionary<string, ushort> icdatas = new Dictionary<string, ushort>(player.Manager.GameData.IdToObjectType,
                StringComparer.OrdinalIgnoreCase);
            if (!icdatas.TryGetValue(name, out objType))
            {
                player.SendError("Item not found, spelling error?");
                return false;
            }
            if (!player.Manager.GameData.Items[objType].Secret || player.Client.Account.Rank >= 4)
            {
                for (int i = 0; i < plr.Inventory.Length; i++)
                    if (plr.Inventory[i] == null)
                    {
                        plr.Inventory[i] = player.Manager.GameData.Items[objType];
                        plr.UpdateCount++;
                        plr.SaveToCharacter();
                        player.SendInfo("Success sending " + name + " to " + plr.Name);
                        plr.SendInfo("You got a " + name + " from " + player.Name);
                        break;
                    }
            }
            else
            {
                player.SendError("Item failed sending to " + plr.Name + ", make sure you spelt the command right, and their name!");
                return false;
            }
            return true;
        }
    }

    internal class ReviveCommand : Command
    {
        public ReviveCommand()
            : base("revive", Ranks.Staff)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (args.Length == 2)
            {
                player.Manager.Database.DoActionAsync(db =>
                {
                    var testcmd = db.CreateQuery();
                    testcmd.CommandText = "SELECT dead FROM characters WHERE accId=@accId AND charId=@charId";
                    testcmd.Parameters.AddWithValue("@accId", args[0]);
                    testcmd.Parameters.AddWithValue("@charId", args[1]);
                    try
                    {
                        bool success = (bool)testcmd.ExecuteScalar();
                        if (success)
                        {
                            var cmd = db.CreateQuery();
                            cmd.CommandText = "UPDATE characters SET hp=50, mp=50, dead=FALSE, deathTime='0000-00-00 00:00:00' WHERE accId=@accId AND charId=@charId";
                            cmd.Parameters.AddWithValue("@accId", args[0]);
                            cmd.Parameters.AddWithValue("@charId", args[1]);
                            cmd.ExecuteNonQuery();

                            var cmd2 = db.CreateQuery();
                            cmd2.CommandText = "DELETE FROM death WHERE accId=@accId AND chrId=@chrId";
                            cmd2.Parameters.AddWithValue("@accId", args[0]);
                            cmd2.Parameters.AddWithValue("@chrId", args[1]);
                            cmd2.ExecuteNonQuery();

                            player.SendInfo("Successfully revived!");
                        }
                        else
                        {
                            player.SendHelp("Character is not dead or you put the parameters wrong!");
                        }
                    }
                    catch (Exception)
                    {
                        player.SendHelp("Character does not yet exist!");
                    }
                });
                return true;
            }
            else
            {
                player.SendHelp("Usage: /revive <accId> <chrId>");
                return false;
            }
        }

    }

    internal class VaultCommand : Command
    {
        public VaultCommand()
            : base("vault")
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            player.Client.Reconnect(new ReconnectPacket
            {
                Host = "",
                Port = Program.Settings.GetValue<int>("port"),
                GameId = player.Manager.PlayerVault(player.Client).Id,
                Name = player.Manager.PlayerVault(player.Client).Name,
                Key = player.Manager.PlayerVault(player.Client).PortalKey,
            });
            return true;
        }
    }



    internal class GHallCommand : Command
    {
        public GHallCommand()
            : base("ghall")
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            try
            {
                World world = player.Guild.GuildHall;
                player.Client.Reconnect(new ReconnectPacket
                {
                    Host = "",
                    Port = Program.Settings.GetValue<int>("port"),
                    GameId = world.Id,
                    Name = world.Name,
                    Key = world.PortalKey,
                });
                return true;
            }
            catch
            {
                player.SendError("You are not a member of a guild!");
                return false;
            }
        }
    }

    internal class RealmCommand : Command
    {
        public RealmCommand()
            : base("realm")
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            World world = player.Client.Manager.Monitor.GetRandomRealm();

            player.Client.Reconnect(new ReconnectPacket
            {
                Host = "",
                Port = Program.Settings.GetValue<int>("port"),
                GameId = world.Id,
                Name = world.Name,
                Key = world.PortalKey,
            });
            return true;
        }
    }

    internal class DevChatCommand : Command
    {
        public DevChatCommand()
            : base("dev", 1)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (args.Length == 0)
            {
                player.SendHelp("Usage: /dev <saytext>");
                return false;
            }
            string saytext = string.Join(" ", args);

            foreach (Client i in player.Manager.Clients.Values)

            {
                i.SendPacket(new TextPacket
                {
                    BubbleTime = 0,
                    Stars = 85,
                    Name = "[Dev]",
                    Text = " " + saytext
                });
            }
            return true;
        }
    }

    internal class RemoveTossEffCommand : Command
    {
        public RemoveTossEffCommand()
            : base("remtosseff", 1)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (args.Length != 2)
            {
                player.SendHelp("Usage: /tosseff <PlayerName> <Effectname or Effectnumber>");
                return false;
            }
            try
            {
                foreach (KeyValuePair<string, Client> i in player.Manager.Clients.Where(i => i.Value.Player.Name.EqualsIgnoreCase(args[0])))
                {
                    i.Value.Player.ApplyConditionEffect(new ConditionEffect
                    {
                        Effect =
                            (ConditionEffectIndex)Enum.Parse(typeof(ConditionEffectIndex), args[1].Trim(), true),
                        DurationMS = 0
                    });
                    player.SendInfo("Success!");
                }
            }
            catch
            {
                player.SendError("Invalid effect or player name! ");
                return false;
            }
            return true;
        }
    }

    internal class TossEffCommand : Command
    {
        public TossEffCommand()
            : base("tosseff", Ranks.Staff)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (args.Length != 2)
            {
                player.SendHelp("Usage: /tosseff <PlayerName> <Effectname or Effectnumber>");
                return false;
            }
            try
            {
                foreach (KeyValuePair<string, Client> i in player.Manager.Clients.Where(i => i.Value.Player.Name.EqualsIgnoreCase(args[0])))
                {
                    i.Value.Player.ApplyConditionEffect(new ConditionEffect
                    {
                        Effect =
                            (ConditionEffectIndex)Enum.Parse(typeof(ConditionEffectIndex), args[1].Trim(), true),
                        DurationMS = -1
                    });
                    player.SendInfo("Success!");
                }
            }
            catch
            {
                player.SendError("Invalid effect or player name! ");
                return false;
            }
            return true;
        }
    }

    internal class CalculateCommand : Command
    {
        public CalculateCommand()
            : base("calculate", 0)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            int first = int.Parse(args[0]);
            var symbol = string.Join(" ", args[1]);
            int second = int.Parse(args[2]);
            int answer = 0;
            if (symbol == "+")
            {
                answer = first + second;
            }
            if (symbol == "*")
            {
                answer = first * second;
            }
            if (symbol == "-")
            {
                answer = first - second;
            }
            if (symbol == "/")
            {
                answer = first / second;
            }
            player.SendInfo(answer.ToString());
            return true;
        }
    }

    internal class CFameCommand : Command
    {
        public CFameCommand()
        : base("cfame", Ranks.Staff)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (args[0] == "")
            {
                player.SendHelp("Usage: /cfame <Fame Amount>");
                return false;
            }
            try
            {
                int newFame = Convert.ToInt32(args[0]);
                int newXP = Convert.ToInt32(newFame.ToString() + "000");
                player.Fame = newFame;
                player.Experience = newXP;
                player.SaveToCharacter();
                player.Client.Save();
                player.UpdateCount++;
                player.SendInfo("Updated Character Fame To: " + newFame);
            }
            catch
            {
                player.SendInfo("Error Setting Fame");
                return false;
            }
            return true;
        }
    }

    internal class PetMaxCommand : Command
    {
        public PetMaxCommand()
            : base("petmax", Ranks.Staff)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            Item feed = player.Manager.GameData.Items[0x9d4];
            for (int i = 0; i < 50; i++)
            {
                player.Pet.Feed(feed);
                player.Pet.UpdateCount++;
            }
            return true;
        }
    }

    internal class AngelModeCommand : Command
    {
        public AngelModeCommand()
            : base("angel", Ranks.Designer)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (player.HasConditionEffect(ConditionEffectIndex.Invincible))
            {
                player.ApplyConditionEffect(new ConditionEffect
                {
                    Effect = ConditionEffectIndex.Invincible,
                    DurationMS = 0
                });
                player.SendInfo("Angelmode Off");
            }
            else
            {
                player.ApplyConditionEffect(new ConditionEffect
                {
                    Effect = ConditionEffectIndex.Invincible,
                    DurationMS = -1
                });
                player.SendInfo("Angelmode On");
            }
            return true;
        }
    }

    internal class KillPlayerCommand : Command
    {
        public KillPlayerCommand()
            : base("kill", Ranks.Staff)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            foreach (Client i in player.Manager.Clients.Values)
            {
                if (i.Account.Name.EqualsIgnoreCase(args[0]))
                {
                    i.Player.HP = 0;
                    i.Player.Death("Admin");
                    player.SendInfo("Player killed!");
                    return true;
                }
            }
            player.SendError(string.Format("Player '{0}' could not be found!", args));
            return false;
        }
    }

    internal class RestartCommand : Command
    {
        public RestartCommand()
            : base("restart", Ranks.Staff)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            try
            {
                foreach (KeyValuePair<int, World> w in player.Manager.Worlds)
                {
                    World world = w.Value;
                    if (w.Key != 0)
                    {
                        world.BroadcastPacket(new TextPacket
                        {
                            Name = "@ANNOUNCEMENT",
                            Stars = -1,
                            BubbleTime = 0,
                            Text =
                                "Server restarting soon. Please be ready to disconnect. Estimated server down time: 30 Seconds - 1 Minute"
                        }, null);
                    }
                }
            }
            catch
            {
                player.SendError("Cannot say that in announcement!");
                return false;
            }
            return true;
        }
    }

    //class VitalityCommand : ICommand
    //{
    //    public string Command { get { return "vit"; } }
    //    public int RequiredRank { get { return 3; } }

    //    protected override bool Process(Player player, RealmTime time, string[] args)
    //    {
    //        try
    //        {
    //            if (args.Length == 0)
    //            {
    //                player.SendHelp("Use /vit <ammount>");
    //            }
    //            else if (args.Length == 1)
    //            {
    //                player.Client.Player.Stats[5] = int.Parse(args[0]);
    //                player.UpdateCount++;
    //                player.SendInfo("Success!");
    //            }
    //        }
    //        catch
    //        {
    //            player.SendError("Error!");
    //        }
    //    }
    //}

    //class DefenseCommand : ICommand
    //{
    //    public string Command { get { return "def"; } }
    //    public int RequiredRank { get { return 3; } }

    //    protected override bool Process(Player player, RealmTime time, string[] args)
    //    {
    //        try
    //        {
    //            if (args.Length == 0)
    //            {
    //                player.SendHelp("Use /def <ammount>");
    //            }
    //            else if (args.Length == 1)
    //            {
    //                player.Client.Player.Stats[3] = int.Parse(args[0]);
    //                player.UpdateCount++;
    //                player.SendInfo("Success!");
    //            }
    //        }
    //        catch
    //        {
    //            player.SendError("Error!");
    //        }
    //    }
    //}

    //class AttackCommand : ICommand
    //{
    //    public string Command { get { return "att"; } }
    //    public int RequiredRank { get { return 3; } }

    //    protected override bool Process(Player player, RealmTime time, string[] args)
    //    {
    //        try
    //        {
    //            if (args.Length == 0)
    //            {
    //                player.SendHelp("Use /att <ammount>");
    //            }
    //            else if (args.Length == 1)
    //            {
    //                player.Client.Player.Stats[2] = int.Parse(args[0]);
    //                player.UpdateCount++;
    //                player.SendInfo("Success!");
    //            }
    //        }
    //        catch
    //        {
    //            player.SendError("Error!");
    //        }
    //    }
    //}

    //class DexterityCommand : ICommand
    //{
    //    public string Command { get { return "dex"; } }
    //    public int RequiredRank { get { return 3; } }

    //    protected override bool Process(Player player, RealmTime time, string[] args)
    //    {
    //        try
    //        {
    //            if (args.Length == 0)
    //            {
    //                player.SendHelp("Use /dex <ammount>");
    //            }
    //            else if (args.Length == 1)
    //            {
    //                player.Client.Player.Stats[7] = int.Parse(args[0]);
    //                player.UpdateCount++;
    //                player.SendInfo("Success!");
    //            }
    //        }
    //        catch
    //        {
    //            player.SendError("Error!");
    //        }
    //    }
    //}

    //class LifeCommand : ICommand
    //{
    //    public string Command { get { return "hp"; } }
    //    public int RequiredRank { get { return 3; } }

    //    protected override bool Process(Player player, RealmTime time, string[] args)
    //    {
    //        try
    //        {
    //            if (args.Length == 0)
    //            {
    //                player.SendHelp("Use /hp <ammount>");
    //            }
    //            else if (args.Length == 1)
    //            {
    //                player.Client.Player.Stats[0] = int.Parse(args[0]);
    //                player.UpdateCount++;
    //                player.SendInfo("Success!");
    //            }
    //        }
    //        catch
    //        {
    //            player.SendError("Error!");
    //        }
    //    }
    //}

    //class ManaCommand : ICommand
    //{
    //    public string Command { get { return "mp"; } }
    //    public int RequiredRank { get { return 3; } }

    //    protected override bool Process(Player player, RealmTime time, string[] args)
    //    {
    //        try
    //        {
    //            if (args.Length == 0)
    //            {
    //                player.SendHelp("Use /mp <ammount>");
    //            }
    //            else if (args.Length == 1)
    //            {
    //                player.Client.Player.Stats[1] = int.Parse(args[0]);
    //                player.UpdateCount++;
    //                player.SendInfo("Success!");
    //            }
    //        }
    //        catch
    //        {
    //            player.SendError("Error!");
    //        }
    //    }
    //}

    //class SpeedCommand : ICommand
    //{
    //    public string Command { get { return "spd"; } }
    //    public int RequiredRank { get { return 3; } }

    //    protected override bool Process(Player player, RealmTime time, string[] args)
    //    {
    //        try
    //        {
    //            if (args.Length == 0)
    //            {
    //                player.SendHelp("Use /spd <ammount>");
    //            }
    //            else if (args.Length == 1)
    //            {
    //                player.Client.Player.Stats[4] = int.Parse(args[0]);
    //                player.UpdateCount++;
    //                player.SendInfo("Success!");
    //            }
    //        }
    //        catch
    //        {
    //            player.SendError("Error!");
    //        }
    //    }
    //}

    //class WisdomCommand : ICommand
    //{
    //    public string Command { get { return "wis"; } }
    //    public int RequiredRank { get { return 3; } }

    //    protected override bool Process(Player player, RealmTime time, string[] args)
    //    {
    //        try
    //        {
    //            if (args.Length == 0)
    //            {
    //                player.SendHelp("Use /spd <ammount>");
    //            }
    //            else if (args.Length == 1)
    //            {
    //                player.Client.Player.Stats[6] = int.Parse(args[0]);
    //                player.UpdateCount++;
    //                player.SendInfo("Success!");
    //            }
    //        }
    //        catch
    //        {
    //            player.SendError("Error!");
    //        }
    //    }
    //}

    //class Ban : ICommand
    //{
    //    public string Command { get { return "ban"; } }
    //    public int RequiredRank { get { return 4; } }

    //    protected override bool Process(Player player, RealmTime time, string[] args)
    //    {
    //        if (args.Length == 0)
    //        {
    //            player.SendHelp("Usage: /ban <username>");
    //        }
    //        try
    //        {
    //            using (Database dbx = new Database())
    //            {
    //                var cmd = dbx.CreateQuery();
    //                cmd.CommandText = "UPDATE accounts SET banned=1, rank=0 WHERE name=@name";
    //                cmd.Parameters.AddWithValue("@name", args[0]);
    //                if (cmd.ExecuteNonQuery() == 0)
    //                {
    //                    player.SendInfo("Could not ban");
    //                }
    //                else
    //                {
    //                    foreach (var i in player.Owner.Players)
    //                    {
    //                        if (i.Value.Name.ToLower() == args[0].ToLower().Trim())
    //                        {
    //                            i.Value.Client.Disconnect();
    //                            player.SendInfo("Account successfully Banned");
    //                            log.InfoFormat(args[0] + " was Banned.");
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //        catch
    //        {
    //            player.SendInfo("Could not ban");
    //        }
    //    }
    //}

    //class UnBan : ICommand
    //{
    //    public string Command { get { return "unban"; } }
    //    public int RequiredRank { get { return 4; } }

    //    protected override bool Process(Player player, RealmTime time, string[] args)
    //    {
    //        if (args.Length == 0)
    //        {
    //            player.SendHelp("Usage: /unban <username>");
    //        }
    //        try
    //        {
    //            using (Database dbx = new Database())
    //            {
    //                var cmd = dbx.CreateQuery();
    //                cmd.CommandText = "UPDATE accounts SET banned=0, rank=1 WHERE name=@name";
    //                cmd.Parameters.AddWithValue("@name", args[0]);
    //                if (cmd.ExecuteNonQuery() == 0)
    //                {
    //                    player.SendInfo("Could not unban");
    //                }
    //                else
    //                {
    //                    player.SendInfo("Account successfully Unbanned");
    //                    log.InfoFormat(args[1] + " was Unbanned.");

    //                }
    //            }
    //        }
    //        catch
    //        {
    //            player.SendInfo("Could not unban, please unban in database");
    //        }
    //    }
    //}

    //class Rank : ICommand
    //{
    //    public string Command { get { return "rank"; } }
    //    public int RequiredRank { get { return 4; } }

    //    protected override bool Process(Player player, RealmTime time, string[] args)
    //    {
    //        if (args.Length < 2)
    //        {
    //            player.SendHelp("Usage: /rank <username> <number>\n0: Player\n1: Donator\n2: Game Master\n3: Developer\n4: Head Developer\n5: Admin");
    //        }
    //        else
    //        {
    //            try
    //            {
    //                using (Database dbx = new Database())
    //                {
    //                    var cmd = dbx.CreateQuery();
    //                    cmd.CommandText = "UPDATE accounts SET rank=@rank WHERE name=@name";
    //                    cmd.Parameters.AddWithValue("@rank", args[1]);
    //                    cmd.Parameters.AddWithValue("@name", args[0]);
    //                    if (cmd.ExecuteNonQuery() == 0)
    //                    {
    //                        player.SendInfo("Could not change rank");
    //                    }
    //                    else
    //                        player.SendInfo("Account rank successfully changed");
    //                }
    //            }
    //            catch
    //            {
    //                player.SendInfo("Could not change rank, please change rank in database");
    //            }
    //        }
    //    }
    //}
    //class GuildRank : ICommand
    //{
    //    public string Command { get { return "grank"; } }
    //    public int RequiredRank { get { return 4; } }

    //    protected override bool Process(Player player, RealmTime time, string[] args)
    //    {
    //        if (args.Length < 2)
    //        {
    //            player.SendHelp("Usage: /grank <username> <number>");
    //        }
    //        else
    //        {
    //            try
    //            {
    //                using (Database dbx = new Database())
    //                {
    //                    var cmd = dbx.CreateQuery();
    //                    cmd.CommandText = "UPDATE accounts SET guildRank=@guildRank WHERE name=@name";
    //                    cmd.Parameters.AddWithValue("@guildRank", args[1]);
    //                    cmd.Parameters.AddWithValue("@name", args[0]);
    //                    if (cmd.ExecuteNonQuery() == 0)
    //                    {
    //                        player.SendInfo("Could not change guild rank. Use 10, 20, 30, 40, or 50 (invisible)");
    //                    }
    //                    else
    //                        player.SendInfo("Guild rank successfully changed");
    //                    log.InfoFormat(args[1] + "'s guild rank has been changed");
    //                }
    //            }
    //            catch
    //            {
    //                player.SendInfo("Could not change rank, please change rank in database");
    //            }
    //        }
    //    }
    //}
    //class ChangeGuild : ICommand
    //{
    //    public string Command { get { return "setguild"; } }
    //    public int RequiredRank { get { return 4; } }

    //    protected override bool Process(Player player, RealmTime time, string[] args)
    //    {
    //        if (args.Length < 2)
    //        {
    //            player.SendHelp("Usage: /setguild <username> <guild id>");
    //        }
    //        else
    //        {
    //            try
    //            {
    //                using (Database dbx = new Database())
    //                {
    //                    var cmd = dbx.CreateQuery();
    //                    cmd.CommandText = "UPDATE accounts SET guild=@guild WHERE name=@name";
    //                    cmd.Parameters.AddWithValue("@guild", args[1]);
    //                    cmd.Parameters.AddWithValue("@name", args[0]);
    //                    if (cmd.ExecuteNonQuery() == 0)
    //                    {
    //                        player.SendInfo("Could not change guild.");
    //                    }
    //                    else
    //                        player.SendInfo("Guild successfully changed");
    //                    log.InfoFormat(args[1] + "'s guild has been changed");
    //                }
    //            }
    //            catch
    //            {
    //                player.SendInfo("Could not change guild, please change in database.                                Use /setguild <username> <guild id>");
    //            }
    //        }
    //    }
    //}

    internal class TqCommand : Command
    {
        public TqCommand()
            : base("tq", Ranks.Designer)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (player.Quest == null)
            {
                player.SendError("Player does not have a quest!");
                return false;
            }
            player.Move(player.Quest.X + 0.5f, player.Quest.Y + 0.5f);
            if (player.Pet != null)
                player.Pet.Move(player.Quest.X + 0.5f, player.Quest.Y + 0.5f);
            player.UpdateCount++;
            player.Owner.BroadcastPacket(new GotoPacket
            {
                ObjectId = player.Id,
                Position = new Position
                {
                    X = player.Quest.X,
                    Y = player.Quest.Y
                }
            }, null);
            player.SendInfo("Success!");
            return true;
        }
    }

    //class GodMode : ICommand
    //{
    //    public string Command { get { return "god"; } }
    //    public int RequiredRank { get { return 3; } }

    //    protected override bool Process(Player player, RealmTime time, string[] args)
    //    {
    //        if (player.HasConditionEffect(ConditionEffects.Invincible))
    //        {
    //            player.ApplyConditionEffect(new ConditionEffect()
    //            {
    //                Effect = ConditionEffectIndex.Invincible,
    //                DurationMS = 0
    //            });
    //            player.SendInfo("Godmode Off");
    //        }
    //        else
    //        {

    //            player.ApplyConditionEffect(new ConditionEffect()
    //            {
    //                Effect = ConditionEffectIndex.Invincible,
    //                DurationMS = -1
    //            });
    //            player.SendInfo("Godmode On");
    //        }
    //    }
    //}
    //class StarCommand : ICommand
    //{
    //    public string Command { get { return "stars"; } }
    //    public int RequiredRank { get { return 2; } }

    //    protected override bool Process(Player player, RealmTime time, string[] args)
    //    {
    //        try
    //        {
    //            if (args.Length == 0)
    //            {
    //                player.SendHelp("Use /stars <ammount>");
    //            }
    //            else if (args.Length == 1)
    //            {
    //                player.Client.Player.Stars = int.Parse(args[0]);
    //                player.UpdateCount++;
    //                player.SendInfo("Success!");
    //            }
    //        }
    //        catch
    //        {
    //            player.SendError("Error!");
    //        }
    //    }
    //}

    internal class LevelCommand : Command
    {
        public LevelCommand()
            : base("level", Ranks.DL1)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    player.SendHelp("Use /level <ammount>");
                    return false;
                }
                if (args.Length == 1)
                {
                    player.Client.Character.Level = int.Parse(args[0]);
                    player.Client.Player.Level = int.Parse(args[0]);
                    player.UpdateCount++;
                    player.SendInfo("Success!");
                }
            }
            catch
            {
                player.SendError("Error!");
                return false;
            }
            return true;
        }
    }

    internal class SetCommand : Command
    {
        public SetCommand()
            : base("setStat", Ranks.Staff)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (args.Length == 2)
            {
                try
                {
                    string stat = args[0].ToLower();
                    int amount = int.Parse(args[1]);
                    switch (stat)
                    {
                        case "health":
                        case "hp":
                            player.Stats[0] = amount;
                            break;
                        case "mana":
                        case "mp":
                            player.Stats[1] = amount;
                            break;
                        case "atk":
                        case "attack":
                            player.Stats[2] = amount;
                            break;
                        case "def":
                        case "defence":
                            player.Stats[3] = amount;
                            break;
                        case "spd":
                        case "speed":
                            player.Stats[4] = amount;
                            break;
                        case "vit":
                        case "vitality":
                            player.Stats[5] = amount;
                            break;
                        case "wis":
                        case "wisdom":
                            player.Stats[6] = amount;
                            break;
                        case "dex":
                        case "dexterity":
                            player.Stats[7] = amount;
                            break;
                        default:
                            player.SendError("Invalid Stat");
                            player.SendHelp("Stats: Health, Mana, Attack, Defence, Speed, Vitality, Wisdom, Dexterity");
                            player.SendHelp("Shortcuts: Hp, Mp, Atk, Def, Spd, Vit, Wis, Dex");
                            return false;
                    }
                    player.SaveToCharacter();
                    player.Client.Save();
                    player.UpdateCount++;
                    player.SendInfo("Success");
                }
                catch
                {
                    player.SendError("Error while setting stat");
                    return false;
                }
                return true;
            }
            else if (args.Length == 3)
            {
                foreach (Client i in player.Manager.Clients.Values)
                {
                    if (i.Account.Name.EqualsIgnoreCase(args[0]))
                    {
                        try
                        {
                            string stat = args[1].ToLower();
                            int amount = int.Parse(args[2]);
                            switch (stat)
                            {
                                case "health":
                                case "hp":
                                    i.Player.Stats[0] = amount;
                                    break;
                                case "mana":
                                case "mp":
                                    i.Player.Stats[1] = amount;
                                    break;
                                case "atk":
                                case "attack":
                                    i.Player.Stats[2] = amount;
                                    break;
                                case "def":
                                case "defence":
                                    i.Player.Stats[3] = amount;
                                    break;
                                case "spd":
                                case "speed":
                                    i.Player.Stats[4] = amount;
                                    break;
                                case "vit":
                                case "vitality":
                                    i.Player.Stats[5] = amount;
                                    break;
                                case "wis":
                                case "wisdom":
                                    i.Player.Stats[6] = amount;
                                    break;
                                case "dex":
                                case "dexterity":
                                    i.Player.Stats[7] = amount;
                                    break;
                                default:
                                    player.SendError("Invalid Stat");
                                    player.SendHelp("Stats: Health, Mana, Attack, Defence, Speed, Vitality, Wisdom, Dexterity");
                                    player.SendHelp("Shortcuts: Hp, Mp, Atk, Def, Spd, Vit, Wis, Dex");
                                    return false;
                            }
                            i.Player.SaveToCharacter();
                            i.Player.Client.Save();
                            i.Player.UpdateCount++;
                            player.SendInfo("Success");
                        }
                        catch
                        {
                            player.SendError("Error while setting stat");
                            return false;
                        }
                        return true;
                    }
                }
                player.SendError(string.Format("Player '{0}' could not be found!", args));
                return false;
            }
            else
            {
                player.SendHelp("Usage: /setStat <Stat> <Amount>");
                player.SendHelp("or");
                player.SendHelp("Usage: /setStat <Player> <Stat> <Amount>");
                player.SendHelp("Shortcuts: Hp, Mp, Atk, Def, Spd, Vit, Wis, Dex");
                return false;
            }
        }
    }

    internal class SetpieceCommand : Command
    {
        public SetpieceCommand()
            : base("setpiece", Ranks.Staff)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            ISetPiece piece = (ISetPiece)Activator.CreateInstance(Type.GetType(
                "wServer.realm.setpieces." + args[0], true, true));
            piece.RenderSetPiece(player.Owner, new IntPoint((int)player.X + 1, (int)player.Y + 1));
            return true;
        }
    }

    internal class ListCommands : Command
    {
        public ListCommands() : base("commands", Ranks.Standard) { }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            Dictionary<string, Command> cmds = new Dictionary<string, Command>();
            Type t = typeof(Command);
            foreach (Type i in t.Assembly.GetTypes())
                if (t.IsAssignableFrom(i) && i != t)
                {
                    Command instance = (Command)Activator.CreateInstance(i);
                    cmds.Add(instance.CommandName, instance);
                }
            StringBuilder sb = new StringBuilder("");
            Command[] copy = cmds.Values.ToArray();
            for (int i = 0; i < copy.Length; i++)
            {
                if (i != 0) sb.Append(", ");
                sb.Append(copy[i].CommandName);
            }

            player.SendInfo(sb.ToString());
            return true;
        }
    }
}