# Recreation

Recreation buildings can be used during
Downtime
and award a
Morale
buff.
Summary of recreation buildings
Each recreation building provides a buff which awards a certain amount of morale for a certain duration, and dupes prefer some buildings more.
Building
Preference
Morale
Duration
Usage Time
Morale×Duration
Beach Chair
(Lit)
2
8
11.95
150
95.6
Beach Chair
(Dark)
2
5
11.95
150
59.75
Hot Tub
2
5
7.95
90
39.75
Vertical Wind Tunnel
2
5
7.95
90
39.75
Arcade Cabinet
3
3
3.95
15
11.85
Jukebot
3
2
3.95
15
7.9
Sauna
3
2
3.95
30
7.9
Mechanical Surfboard
3
2
3.95
30
7.9
Water Cooler
(Brackene)
4
2
1.1
5
4.4
Espresso Machine
1
4
0.75
30
3
Juicer
1
4
0.75
30
3
Soda Fountain
1
4
0.75
30
3
Party Line Phone
(Sociable)
1
4
0.75
40
3
Party Line Phone
(Full of Gossip)
1
2
0.75
40
1.5
Party Line Phone
(Less Anxious)
1
1
0.75
40
.75
Shower
5
3
0.9
15
2.7
Water Cooler
(Water)
4
1
1.1
5
1.1
Usage Time is reduced by Lit Workspace for all buildings except Beach Chair. The game code indicates the Party Line Phone isn't meant to benefit from Lit Workspace, but it seems actually to (possibly a bug).
Morale×Duration is the "morale⋅cycles" provided, for instance the Soda Fountain provides +4 morale, but only for 0.75 cycles, hence "3" morale⋅cycles. It indicates how valuable the buff is considering not only the magnitude but also the duration.
Duplicants prefer to use the closest Recreation Building from the highest preference tier, as long as they don't already have the buff. If the Duplicant already has all available buffs, they simply use the nearest available recreation building.
Duplicant Decision Making
Tiers of Desirability
There are five tiers of desirability, in order:
Soda Fountain, Juicer, Espresso Machine, Party Line Phone (these all provide a 0.75 cycle buff and have a 30s or 40s usage time)
Beach Chair, Hot Tub, Vertical Wind Tunnel (these all provide a large, long duration buff and have long usage times)
Arcade Cabinet, Jukebot, Mechanical Surfboard, Sauna (these all provide a 3.95 cycle buff and have a 15s or 30s usage time)
Shower (the Shower can also be used during Bathtime schedule)
Watercooler
Overall decision making process
When deciding what recreation building to use, a Duplicant considers the following factors:
In general a dupe prefers the closest building from the highest desirability tier.
If they have already used that kind of rec building in the last 300 seconds the dupe will absolutely not use it again (this 300s hard lockout period is uniquely shared between the three drink machines).
If they already have the morale buff provided by that building but it's outside the 300 seconds hard lockout, they will only use it as a last resort.
If the dupe already has all buffs their behaviour depends on whether there is a Recreation Room or not:
If there is a Recreation Room, they will hang out, not necessarily in the Recreation Room.
Otherwise they will choose the nearest available recreation building disregarding desirability tier, thus it is possible to force dupes to refresh a buff by not having a recreation room.
If the dupe is locked from using any recreation building or all recreation buildings are occupied, they will either idle or go to work if they don't have a location (e.g.
Printing Pod
) to idle at.
Recreation Strategy
Note that the drink machines and the party line phone are the most desirable recreation buildings, but actually provide a very weak buff considering the usage time and/or resource cost. For example a Dupe who uses a Soda Fountain for 30s every cycle gets on average +3 morale, while a Dupe who uses an Arcade Cabinet for 15s every 4th cycle, gets +2.97 Morale for 1/8th the usage time.
Because drink machines are both very desirable and can be used every cycle and have quite a long usage time (more than one downtime slot anyway), a dupe on a small shift (e.g. a solo caretaker on an asteroid in Spaced Out) will always use the closest drink machine and never use any other recreation building.
This contrasts with the rec buildings that give longer duration buffs, for example if the same Duplicant was provided an Arcade Cabinet, Jukebot and Mechanical Surfboard it would rotate between them, and maintain roughly +7 morale, throw in a Sauna and they're perfectly rotating between the 4 buildings over 4 cycles and getting nearly +9 morale for an average usage time of 22.5s per cycle.
It could be considered
highly advisable
to simply not provide drink machines or party line phones at all as they don't provide very good buffs anyway.
Managing long-duration buffs
The Beach Chair, Hot Tub and Vertical Wind Tunnel all provide long duration buffs, but for proportionally long usage times. It is undesirable for a Duplicant to use the rec building again until the buff has actually expired. It is one thing to spend 150 seconds on a Beach Chair every 12-13 cycles, another to do that every cycle!
There are three ways to ensure a dupe won't re-use the building:
Have a Recreation Room (doesn't matter what is in it) for its suppressive effect on reusing Recreation Buildings.
Provide "distraction" recreation options with fast expiring buffs, for example Shower and Watercooler, to ensure the Dupe is distracted until their downtime ends.
Ensure that fast usage options like the Arcade Cabinet and Jukebot are located closer to the Great Hall than the Beach Chair etc, so Dupes will favor using the fast options even if they already have all the buffs.
Scheduling for long usage times
It is common to use a schedule along the lines of "2-3 downtime, 2 bedtime" (3-2), dupes will go to bed then continue sleeping during worktime until reaching 100% stamina. This sort of schedule is very flexible and optimal for short usage time recreation buildings as whether a dupe spends 15 seconds, 30 seconds or 45 seconds using rec buildings, they'll still make it to bed and will just make up the difference sleeping during worktime.
However a building like the Beach Chair requires a whopping 6 downtime slots, which is more than most players are willing to give, there are two main solutions:
Use a normal 3-2 schedule, when the dupe uses a Beach Chair, they'll miss bedtime. Later during the cycle they'll take an exhausted nap. Exhausted naps are far from ideal, but if they only happen once every 12-13 cycles it's actually a very acceptable price to pay for the huge value of the Beach Chair buff. This option is definitely better if you are only mixing in the Beach Chair, and not using all three long usage time rec buildings.
Use a schedule along the lines of "1-2 downtime, 3 bedtime, 1 downtime" (1-3-1). During the first period of downtime, dupes will run home, poop and probably eat. Then they'll go to bed, and due to having 3 bedtime slots, stay in bed even when they've reached 100% stamina. Then they get up, they'll eat now if they ran out of time earlier (due to long commute), and otherwise they'll start using a rec building. They can keep using the rec building during worktime, thus not interfering with their sleep. This works well, but the downside is that dupes don't get exactly the amount of sleep they need.
