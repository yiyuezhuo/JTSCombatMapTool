namespace YYZ.JTS
{
    public static class StaticData
    {
        public static string PreWW1TerrainCode = @"
'w' => Water
'r' => Rough
's' => Marsh
'o' => Orchard
' ' => Clear
'f' => Forest
'x' => Blocked
";
        public static string NBTerrainCode = PreWW1TerrainCode + @"
'e' => Field

'v' => Village
'b' => Building
'c' => Chateau
";

        public static string CWBTerrainCode = PreWW1TerrainCode +  @"
'd' => Field

't' => Town
";
        public static string PZCTerrainCode = @"
'p' => Town
' ' => Clear
'q' => City
'o' => Village
'f' => Forest
'x' => Blocked
'b' => Field
'm' => Rough
'e' => Orchard
'a' => Water
'c' => Brush
'g' => Marsh
'l' => Sand
'h' => Swamp
'k' => Broken
";

        // Note: "" denotes " in the verbatim string
        /*
        1
      ____  
    6/    \2
    5\____/3
        4
        */
        // So 1245,; represents "; => 1245 has a edge" (the cell location is correspond to tilemap location)
        public static string EdgeCodeString = @"1245,;	1346,M	1246,K	1235,7	2346,N	1345,=
23,&	24,*	25,2	26,B	34,,	2,""
12,#	13,%	14,)	15,1	16,A	1,!
35,4	36,D	45,8	46,H	56,P	3,$
1256,S	1236,G	13456,]	12456,[	12356,W	12346,O
356,T	346,L	135,5	246,J	156,Q	6,@
126,C	123,'	234,.	345,<	456,X	2356,V
125,3	235,6	245,:	236,F	136,E	5,0
145,9	134,-	124,+	146,I	256,R	4,(
2456,Z	1356,U	1234,/	2345,>	3456,\	1456,Y
12345,?	23456,^	123456,_			";

        // Parameter Data includes only the Key-Value Part
        public static string NBParameterData = @"
General Data
    Title: The Waterloo Campaign, June 1815
    First Side: French
    Bridge Damage Value: 0	    Cavalry Charge Factor: 3	    Cavalry Charge Multiple: 4

Time Parameters
    Dawn: 3:15	    Dusk: 20:15
    Day Turn: 15 minutes	    Night Turn: 60 minutes
    Hours of Twilight: 1	    Twilight Visibility: 4 hexes

Stacking Parameters
    Max Stacking: 1800 men	    Max Counters: 8	    Strength Point: 25 men
    Art Max Stacking: 14 guns	    Cav Max Stacking: 600 men
    Skirmisher Fraction: 1/6	    Squadron Fraction: 1/4

Extended Line Values
    2-Line Infantry: 600 men	    3-Line Infantry: 800 men	    Artillery: 6 guns

British Command Distances
    Brigade: 3 hexes	    Division: 6 hexes	    Corps: 18 hexes
    Wing: 20 hexes	    Army: 30 hexes

French Command Distances
    Brigade: 4 hexes	    Division: 7 hexes	    Corps: 24 hexes
    Wing: 28 hexes	    Army: 40 hexes

Prussian Command Distances
    Brigade: 3 hexes	    Division: 6 hexes	    Corps: 18 hexes
    Wing: 20 hexes	    Army: 30 hexes

Fatigue Parameters
    Max Fatigue: 900	    Day Recovery: 15%	    Night Recovery: 75%

Movement Parameters
    Infantry Allow: 10	    Cavalry Allow: 14	    Artillery Allow: 10	    Supply Allow: 10
    Bridge Movement Cost: 1	    Rear Move: 1	    Disorder Movement: 2/3

Line Disorder Values
    British: 10%	    French: 10%	    Prussian: 10%

Fortification Values
    Abatis Movement: 2	    Abatis Fire: 25%
    Trench Movement: 4	    Trench Fire: -65%

Line Infantry Movement Costs
    Blocked: 0	    Clear: 3	    Water: 0	    Forest: 5
    Orchard: 3	    Marsh: 4	    Building: 2	    Chateau: 2
    Village: 4	    Rough: 3	    Field: 3	    Path: 0
    Road: 0	    Pike: 0	    Rail: 0	    Stream: 1
    Creek: 0	    Hedge: 1	    Wall: 2	    Embank: 2
    High: 0	    Gate: 1	    Fort: 2	    Elevation: 1

Column Infantry Movement Costs
    Blocked: 0	    Clear: 2	    Water: 0	    Forest: 5
    Orchard: 3	    Marsh: 4	    Building: 2	    Chateau: 2
    Village: 2	    Rough: 3	    Field: 3	    Path: 2
    Road: 1	    Pike: 1	    Rail: 2	    Stream: 1
    Creek: 0	    Hedge: 1	    Wall: 2	    Embank: 2
    High: 0	    Gate: 1	    Fort: 2	    Elevation: 1

Cavalry Movement Costs
    Blocked: 0	    Clear: 2	    Water: 0	    Forest: 6
    Orchard: 4	    Marsh: 8	    Building: 2	    Chateau: 0
    Village: 4	    Rough: 3	    Field: 3	    Path: 2
    Road: 1	    Pike: 1	    Rail: 2	    Stream: 2
    Creek: 0	    Hedge: 2	    Wall: 4	    Embank: 4
    High: 0	    Gate: 2	    Fort: 4	    Elevation: 1

Artillery Movement Costs
    Blocked: 0	    Clear: 2	    Water: 0	    Forest: 8
    Orchard: 6	    Marsh: 0	    Building: 2	    Chateau: 2
    Village: 4	    Rough: 4	    Field: 3	    Path: 2
    Road: 1	    Pike: 1	    Rail: 2	    Stream: 2
    Creek: 0	    Hedge: 2	    Wall: 6	    Embank: 6
    High: 0	    Gate: 2	    Fort: 6	    Elevation: 2

Supply Wagon Movement Costs
    Blocked: 0	    Clear: 3	    Water: 0	    Forest: 8
    Orchard: 6	    Marsh: 0	    Building: 3	    Chateau: 3
    Village: 4	    Rough: 4	    Field: 5	    Path: 2
    Road: 1	    Pike: 1	    Rail: 2	    Stream: 2
    Creek: 0	    Hedge: 2	    Wall: 6	    Embank: 6
    High: 0	    Gate: 2	    Fort: 6	    Elevation: 2

Change Facing Costs
    Infantry: 2	    Cavalry: 3	    Artillery: 2

About Face Costs
    Infantry: 1	    Cavalry: 3	    Artillery: 4

British Ammo Loss
    Infantry: 8%	    Artillery: 8%

French Ammo Loss
    Infantry: 6%	    Artillery: 6%

Prussian Ammo Loss
    Infantry: 7%	    Artillery: 7%

Fire Modifiers
    Enfiladed: 20%	    Cavalry: 20%	    Fanaticism Value: 2

Terrain Combat Modifiers
    Blocked: 0%	    Clear: 0%	    Water: 0%	    Forest: -30%
    Orchard: -10%	    Marsh: 0%	    Building: -10%	    Chateau: -30%
    Village: -20%	    Rough: -10%	    Field: 0%	    Path: 0%
    Road: 0%	    Pike: 0%	    Rail: 0%	    Stream: 0%
    Creek: 0%	    Hedge: -10%	    Wall: -20%	    Embank: -10%
    High: -40%	    Gate: -40%	    Fort: -30%	    Elevation: -10%

British Leader Loss Values
    Fire Wound: 2%	    Fire Kill: 3%
    Melee Wound: 3%	    Melee Kill: 4%	    Melee Capture: 5%

French Leader Loss Values
    Fire Wound: 2%	    Fire Kill: 3%
    Melee Wound: 3%	    Melee Kill: 4%	    Melee Capture: 5%

Prussian Leader Loss Values
    Fire Wound: 2%	    Fire Kill: 3%
    Melee Wound: 3%	    Melee Kill: 4%	    Melee Capture: 5%

Height Values
    Blocked: 0m	    Clear: 0m	    Water: 0m	    Forest: 12m
    Orchard: 5m	    Marsh: 1m	    Building: 0m	    Chateau: 10m
    Village: 10m	    Rough: 1m	    Field: 2m	    Man: 2m

Column Fire Modifier
    British: 1/5
    French: 1/5
    Prussian: 1/5

Artillery Resupply Values
    British: 8
    French: 8
    Prussian: 8

Artillery Combat Values
    Melee: 8 men	    Fire: 50 men";

    public static string CWBParameterData = @"
General Data
    Title: Bristoe Station October 14, 1863
    First Side: Union

Time Parameters
    Dawn: 6:00	    Dusk: 18:00
    Day Turn: 20 minutes	    Night Turn: 60 minutes
    Hours of Twilight: 1	    Twilight Visibility: 4 hexes
    Night Visibility: 1 hexes	    Maximum Visibility: 40 hexes

Stacking Parameters
    Max Stacking: 1000 men	    Max Counters: 8	    Strength Point: 20 men

Union Command Distances
    Brigade: 3 hexes	    Division: 6 hexes	    Corps: 12 hexes	    Army: 28 hexes

Confederate Command Distances
    Brigade: 3 hexes	    Division: 6 hexes	    Corps: 12 hexes	    Army: 28 hexes

Fatigue Parameters
    Max Fatigue: 900	    Day Recovery: 5%	    Night Recovery: 20%
    Night Attack Penalty: 300	    Night Movement Penalty : 50

Movement Parameters
    Infantry Allow: 12	    Cavalry Allow: 24	    Artillery Allow: 12	    Supply Allow: 10
    Skirmisher Cost: 5	    Rear Move: 0

Line Infantry Movement Costs
    Blocked: 0	    Clear: 2	    Water: 0	    Forest: 5
    Orchard: 3	    Marsh: 4	    Town: 4	    Field: 2
    Rough: 4	    Trail: 0	    Road: 0	    Pike: 0
    Rail: 0	    Stream: 1	    Creek: 0	    Fence: 1
    Stone: 1	    Embank: 2	    Cut: 0	    Elevation: 1

Column Infantry Movement Costs
    Blocked: 0	    Clear: 2	    Water: 0	    Forest: 4
    Orchard: 3	    Marsh: 4	    Town: 1	    Field: 2
    Rough: 4	    Trail: 2	    Road: 1	    Pike: 1
    Rail: 2	    Stream: 1	    Creek: 0	    Fence: 1
    Stone: 1	    Embank: 2	    Cut: 0	    Elevation: 1

Mounted Cavalry Movement Costs
    Blocked: 0	    Clear: 2	    Water: 0	    Forest: 6
    Orchard: 4	    Marsh: 8	    Town: 2	    Field: 3
    Rough: 6	    Trail: 2	    Road: 1	    Pike: 1
    Rail: 2	    Stream: 2	    Creek: 0	    Fence: 2
    Stone: 2	    Embank: 3	    Cut: 0	    Elevation: 2

Artillery Movement Costs
    Blocked: 0	    Clear: 2	    Water: 0	    Forest: 8
    Orchard: 6	    Marsh: 0	    Town: 2	    Field: 2
    Rough: 0	    Trail: 2	    Road: 1	    Pike: 1
    Rail: 2	    Stream: 2	    Creek: 0	    Fence: 2
    Stone: 6	    Embank: 6	    Cut: 0	    Elevation: 2

Supply Wagon Movement Costs
    Blocked: 0	    Clear: 2	    Water: 0	    Forest: 8
    Orchard: 6	    Marsh: 0	    Town: 2	    Field: 2
    Rough: 0	    Trail: 2	    Road: 1	    Pike: 1
    Rail: 2	    Stream: 2	    Creek: 0	    Fence: 2
    Stone: 6	    Embank: 6	    Cut: 0	    Elevation: 2

Change Facing Costs
    Infantry: 2	    Cavalry: 3	    Artillery: 2

About Face Costs
    Infantry: 2	    Cavalry: 2	    Artillery: 2

Formation Change Costs
    Infantry: 4	    Cavalry: 6	    Artillery: 3

Union Ammo Loss
    Infantry: 4%	    Artillery: 12%	    Gunboat: 5%

Confederate Ammo Loss
    Infantry: 4%	    Artillery: 12%	    Gunboat: 5%

Fire Modifiers
    Enfiladed: 50%	    Cavalry: 50%	    Gunboat: 1/10
    Crew Kill: 120

Terrain Combat Modifiers
    Blocked: 0%	    Clear: 0%	    Water: 0%	    Forest: -50%
    Orchard: -10%	    Marsh: 0%	    Town: -40%	    Field: -10%
    Rough: -30%	    Trail: 0%	    Road: 0%	    Pike: 0%
    Rail: 0%	    Stream: -20%	    Creek: -20%	    Fence: -30%
    Stone: -50%	    Embank: -70%	    Cut: 50%	    Elevation: -30%

Union Leader Loss Values
    Fire Wound: 2%	    Fire Kill: 2%
    Melee Wound: 1%	    Melee Kill: 1%	    Melee Capture: 2%

Confederate Leader Loss Values
    Fire Wound: 2%	    Fire Kill: 1%
    Melee Wound: 1%	    Melee Kill: 2%	    Melee Capture: 2%

Height Values
    Blocked: 50 ft	    Clear: 0 ft	    Water: 0 ft	    Forest: 30 ft
    Orchard: 10 ft	    Marsh: 0 ft	    Town: 30 ft	    Field: 0 ft
    Rough: 10 ft	    Man: 6 ft

Breastworks Values
    Movement: 1	    Combat: -50%	    Building: 50%

Abatis Values
    Movement: 2	    Fire: 20%

Trench Values
    Movement: 8	    Fire: -50%	    Construction: 45

Artillery Resupply Values
    Union: 6	    Confederate: 4

Bridge Values
    Maximum: 200	    Repair: 6";

    public static string PZCParameterData = @"
Time Parameters:
    Dawn: 4:00	    Dusk: 22:00
    Day turn: 2 hours	    Night turn: 4 hours
    Has Dawn and Dusk Turns: Yes

Stacking Limits:
    Road: 400 men	    Maximum: 1600 men

Miscellaneous:
    Foot Speed: 4 km/hr	    Infantry Defense: 16
    Digging-In: 10% per turn	    Bunker Prob: 0% per turn
    Mine Prob: 5% per turn	    Fatigue Recovery: 10	    Night Move Disruption: 0%
    Russian Air Availability: 25%	    Axis Air Availability: 25%
    Russian Air Interdiction: 0%	    Axis Air Interdiction: 1%
    Interdiction Effect: 25%
    Russian Air Interception: 5%	    Axis Air Interception: 12%
    Russian Rail Capacity: 25	    Axis Rail Capacity: 15
    Railroad Movement: 20 km/hr
    Infantry Strength of 70% men = 90% effectiveness
    Russian Unit Recovery: 2%	    Axis Unit Recovery: 5%
    Rubble Fire Value: 1000
    Refuel Percentage: 50%
    Russian Dust Spotting: 100%	    Axis Dust Spotting: 100%
    Russian Breakdown: 12	    Axis Breakdown: 6
    Russian Air Drop Loss: 100%	    Axis Air Drop Loss: 100%
    Congestion Side: Unknown	    Expiration: 0 turns
    Russian Frozen Penalty: 0%	    Axis Frozen Penalty: 0%
    Invasion Penalty: Unknown	    Invasion Percent: 0%	    Invasion Loss: 0%
    Russian Replacement: 1	    Axis Replacement: 1

Bridge Values:
    Russian Bridge Value: 75%	    Axis Bridge Value: 50%
    Russian Pontoon Value: 0	    Axis Pontoon Value: 0
    Light Bridge Strength: 8	    Medium Bridge Strength: 12	    Heavy Bridge Strength: 16

Air Limitation:
    Visibility 1 Hex: 0%	    Visibility 2 Hexes: 50%	    Visibility 3 Hexes: 100%
    Visibility 4 Hexes: 100%	    Visibility 5 Hexes: 100%
    Russian Air Strike Limit: 20	    Axis Air Strike Limit: 30

Artillery Values:
    Russian Counterbattery: 75%	    Axis Counterbattery: 150%
    Russian Stockpiling: 0%	    Axis Stockpiling: 0%
    Russian Stockpile Fire: 100%	    Axis Stockpile Fire: 100%
    Russian Artillery Set Up: 40%	    Axis Artillery Set Up: 90%
    Russian Indirect Mod: 50%	    Axis Indirect Mod: 100%

Combat Losses (per 1000 combat value):
    Fire Low Value: 10	    Fire High Value: 50
    Attacker Low Value: 40	    Attacker High Value: 200
    Defender Low Value: 20	    Defender High Value: 100

Deception Values
    Russian Range: 3	    Axis Range: 0
    Russian Effect: 10%	    Axis Effect: 0%
    Russian Damage: 50%	    Axis Damage: 0%
    Russian Detect: 5%	    Axis Detect: 0%

Terrain Elevations:
    Clear: 0 meters	    Water: 0 meters	    Field: 2 meters	    Brush: 2 meters
    Vineyard: 2 meters	    Orchard: 5 meters	    Forest: 10 meters	    Marsh: 0 meters
    Swamp: 10 meters	    Jungle: 10 meters	    Beach: 0 meters	    Broken: 2 meters
    Sand: 0 meters	    Rough: 2 meters	    Bocage: 3 meters	    Village: 3 meters
    Town: 4 meters	    City: 6 meters	    Industrial: 10 meters	    Impassible: 10 meters

Movement Cost: Foot
    Clear: 6 MP	    Water: 0 MP	    Field: 8 MP	    Brush: 8 MP
    Vineyard: 12 MP	    Orchard: 8 MP	    Forest: 12 MP	    Marsh: 12 MP
    Swamp: 16 MP	    Jungle: 16 MP	    Beach: 8 MP	    Broken: 12 MP
    Sand: 8 MP	    Rough: 18 MP	    Bocage: 16 MP	    Village: 8 MP
    Town: 8 MP	    City: 8 MP	    Industrial: 8 MP	    Impassible: 0 MP

    Trail: 6 MP	    Secondary: 5 MP	    Primary: 4 MP	    Rail: 6 MP
    Stream: 6 MP	    Gully: 3 MP	    Canal: 7 MP	    River: -1 MP
    Ford: 6 MP	    Lt Bridge: 1 MP	    Med Bridge: 1 MP	    Hvy Bridge: 0 MP
    Dune: 6 MP	    Embank: 6 MP	    Dike: 9 MP	    Escarp: 20 MP
    Cliff: -1 MP

Movement Cost: Ski
    Clear: 6 MP	    Water: 0 MP	    Field: 8 MP	    Brush: 8 MP
    Vineyard: 12 MP	    Orchard: 8 MP	    Forest: 12 MP	    Marsh: 12 MP
    Swamp: 16 MP	    Jungle: 16 MP	    Beach: 8 MP	    Broken: 14 MP
    Sand: 8 MP	    Rough: 0 MP	    Bocage: 16 MP	    Village: 8 MP
    Town: 8 MP	    City: 8 MP	    Industrial: 8 MP	    Impassible: 0 MP

    Trail: 6 MP	    Secondary: 5 MP	    Primary: 4 MP	    Rail: 6 MP
    Stream: 6 MP	    Gully: 3 MP	    Canal: -1 MP	    River: -1 MP
    Ford: 6 MP	    Lt Bridge: 1 MP	    Med Bridge: 1 MP	    Hvy Bridge: 0 MP
    Dune: 6 MP	    Embank: 6 MP	    Dike: 9 MP	    Escarp: 20 MP
    Cliff: -1 MP

Movement Cost: Bicycle
    Clear: 24 MP	    Water: 0 MP	    Field: 32 MP	    Brush: 32 MP
    Vineyard: 48 MP	    Orchard: 15 MP	    Forest: 120 MP	    Marsh: 0 MP
    Swamp: 0 MP	    Jungle: 0 MP	    Beach: 32 MP	    Broken: 120 MP
    Sand: 64 MP	    Rough: 0 MP	    Bocage: 208 MP	    Village: 15 MP
    Town: 15 MP	    City: 20 MP	    Industrial: 30 MP	    Impassible: 0 MP

    Trail: 6 MP	    Secondary: 5 MP	    Primary: 4 MP	    Rail: 24 MP
    Stream: 20 MP	    Gully: 10 MP	    Canal: -1 MP	    River: -1 MP
    Ford: 20 MP	    Lt Bridge: 1 MP	    Med Bridge: 1 MP	    Hvy Bridge: 0 MP
    Dune: 6 MP	    Embank: 20 MP	    Dike: 9 MP	    Escarp: -1 MP
    Cliff: -1 MP

Movement Cost: Horse
    Clear: 8 MP	    Water: 0 MP	    Field: 8 MP	    Brush: 8 MP
    Vineyard: 16 MP	    Orchard: 8 MP	    Forest: 18 MP	    Marsh: 24 MP
    Swamp: 32 MP	    Jungle: 32 MP	    Beach: 8 MP	    Broken: 24 MP
    Sand: 12 MP	    Rough: 28 MP	    Bocage: 24 MP	    Village: 10 MP
    Town: 10 MP	    City: 15 MP	    Industrial: 25 MP	    Impassible: 0 MP

    Trail: 6 MP	    Secondary: 5 MP	    Primary: 4 MP	    Rail: 8 MP
    Stream: 12 MP	    Gully: 6 MP	    Canal: -1 MP	    River: -1 MP
    Ford: 12 MP	    Lt Bridge: 1 MP	    Med Bridge: 1 MP	    Hvy Bridge: 0 MP
    Dune: 6 MP	    Embank: 12 MP	    Dike: 9 MP	    Escarp: -1 MP
    Cliff: -1 MP

Movement Cost: Motorcycle
    Clear: 24 MP	    Water: 0 MP	    Field: 32 MP	    Brush: 32 MP
    Vineyard: 48 MP	    Orchard: 32 MP	    Forest: 120 MP	    Marsh: 0 MP
    Swamp: 0 MP	    Jungle: 0 MP	    Beach: 32 MP	    Broken: 120 MP
    Sand: 64 MP	    Rough: 0 MP	    Bocage: 208 MP	    Village: 32 MP
    Town: 32 MP	    City: 40 MP	    Industrial: 40 MP	    Impassible: 0 MP

    Trail: 6 MP	    Secondary: 5 MP	    Primary: 4 MP	    Rail: 24 MP
    Stream: 52 MP	    Gully: 26 MP	    Canal: -1 MP	    River: -1 MP
    Ford: 52 MP	    Lt Bridge: 1 MP	    Med Bridge: 1 MP	    Hvy Bridge: 0 MP
    Dune: 6 MP	    Embank: 52 MP	    Dike: 9 MP	    Escarp: -1 MP
    Cliff: -1 MP

Movement Cost: Motorized
    Clear: 12 MP	    Water: 0 MP	    Field: 60 MP	    Brush: 60 MP
    Vineyard: 90 MP	    Orchard: 60 MP	    Forest: 90 MP	    Marsh: 0 MP
    Swamp: 0 MP	    Jungle: 0 MP	    Beach: 16 MP	    Broken: 90 MP
    Sand: 32 MP	    Rough: 0 MP	    Bocage: 120 MP	    Village: 60 MP
    Town: 60 MP	    City: 80 MP	    Industrial: 80 MP	    Impassible: 0 MP

    Trail: 6 MP	    Secondary: 5 MP	    Primary: 4 MP	    Rail: 12 MP
    Stream: 30 MP	    Gully: 15 MP	    Canal: -1 MP	    River: -1 MP
    Ford: 30 MP	    Lt Bridge: 1 MP	    Med Bridge: 1 MP	    Hvy Bridge: 1 MP
    Dune: 6 MP	    Embank: 30 MP	    Dike: -1 MP	    Escarp: -1 MP
    Cliff: -1 MP

Movement Cost: Soft Halftrk
    Clear: 8 MP	    Water: 0 MP	    Field: 20 MP	    Brush: 20 MP
    Vineyard: 45 MP	    Orchard: 30 MP	    Forest: 50 MP	    Marsh: 0 MP
    Swamp: 0 MP	    Jungle: 0 MP	    Beach: 11 MP	    Broken: 50 MP
    Sand: 22 MP	    Rough: 0 MP	    Bocage: 60 MP	    Village: 60 MP
    Town: 60 MP	    City: 80 MP	    Industrial: 80 MP	    Impassible: 0 MP

    Trail: 6 MP	    Secondary: 5 MP	    Primary: 4 MP	    Rail: 8 MP
    Stream: 30 MP	    Gully: 15 MP	    Canal: -1 MP	    River: -1 MP
    Ford: 30 MP	    Lt Bridge: 1 MP	    Med Bridge: 1 MP	    Hvy Bridge: 1 MP
    Dune: 6 MP	    Embank: 30 MP	    Dike: -1 MP	    Escarp: -1 MP
    Cliff: -1 MP

Movement Cost: Armored Car
    Clear: 12 MP	    Water: 0 MP	    Field: 80 MP	    Brush: 60 MP
    Vineyard: 90 MP	    Orchard: 60 MP	    Forest: 120 MP	    Marsh: 0 MP
    Swamp: 0 MP	    Jungle: 0 MP	    Beach: 21 MP	    Broken: 120 MP
    Sand: 42 MP	    Rough: 0 MP	    Bocage: 120 MP	    Village: 60 MP
    Town: 60 MP	    City: 80 MP	    Industrial: 80 MP	    Impassible: 0 MP

    Trail: 6 MP	    Secondary: 5 MP	    Primary: 4 MP	    Rail: 12 MP
    Stream: 30 MP	    Gully: 15 MP	    Canal: -1 MP	    River: -1 MP
    Ford: 30 MP	    Lt Bridge: 1 MP	    Med Bridge: 1 MP	    Hvy Bridge: 1 MP
    Dune: 6 MP	    Embank: 30 MP	    Dike: -1 MP	    Escarp: -1 MP
    Cliff: -1 MP

Movement Cost: Hard Halftrk
    Clear: 8 MP	    Water: 0 MP	    Field: 20 MP	    Brush: 20 MP
    Vineyard: 45 MP	    Orchard: 30 MP	    Forest: 50 MP	    Marsh: 0 MP
    Swamp: 0 MP	    Jungle: 0 MP	    Beach: 11 MP	    Broken: 50 MP
    Sand: 22 MP	    Rough: 0 MP	    Bocage: 60 MP	    Village: 60 MP
    Town: 60 MP	    City: 80 MP	    Industrial: 80 MP	    Impassible: 0 MP

    Trail: 6 MP	    Secondary: 5 MP	    Primary: 4 MP	    Rail: 8 MP
    Stream: 30 MP	    Gully: 15 MP	    Canal: -1 MP	    River: -1 MP
    Ford: 30 MP	    Lt Bridge: 1 MP	    Med Bridge: 1 MP	    Hvy Bridge: 1 MP
    Dune: 6 MP	    Embank: 30 MP	    Dike: -1 MP	    Escarp: -1 MP
    Cliff: -1 MP

Movement Cost: Tracked
    Clear: 6 MP	    Water: 0 MP	    Field: 25 MP	    Brush: 20 MP
    Vineyard: 24 MP	    Orchard: 24 MP	    Forest: 50 MP	    Marsh: 0 MP
    Swamp: 0 MP	    Jungle: 0 MP	    Beach: 8 MP	    Broken: 50 MP
    Sand: 16 MP	    Rough: 0 MP	    Bocage: 54 MP	    Village: 60 MP
    Town: 60 MP	    City: 80 MP	    Industrial: 80 MP	    Impassible: 0 MP

    Trail: 6 MP	    Secondary: 5 MP	    Primary: 4 MP	    Rail: 6 MP
    Stream: 18 MP	    Gully: 9 MP	    Canal: -1 MP	    River: -1 MP
    Ford: 18 MP	    Lt Bridge: 1 MP	    Med Bridge: 1 MP	    Hvy Bridge: 1 MP
    Dune: 6 MP	    Embank: 30 MP	    Dike: -1 MP	    Escarp: -1 MP
    Cliff: -1 MP

Movement Cost: Rail
    Clear: 0 MP	    Water: 0 MP	    Field: 0 MP	    Brush: 0 MP
    Vineyard: 0 MP	    Orchard: 0 MP	    Forest: 0 MP	    Marsh: 0 MP
    Swamp: 0 MP	    Jungle: 0 MP	    Beach: 0 MP	    Broken: 0 MP
    Sand: 0 MP	    Rough: 0 MP	    Bocage: 0 MP	    Village: 0 MP
    Town: 0 MP	    City: 0 MP	    Industrial: 0 MP	    Impassible: 0 MP

    Trail: 0 MP	    Secondary: 0 MP	    Primary: 0 MP	    Rail: 0 MP
    Stream: 0 MP	    Gully: 0 MP	    Canal: 0 MP	    River: 0 MP
    Ford: 0 MP	    Lt Bridge: 0 MP	    Med Bridge: 0 MP	    Hvy Bridge: 0 MP
    Dune: 0 MP	    Embank: 0 MP	    Dike: 0 MP	    Escarp: 0 MP
    Cliff: 0 MP

Movement Cost: Aircraft
    Clear: 0 MP	    Water: 0 MP	    Field: 0 MP	    Brush: 0 MP
    Vineyard: 0 MP	    Orchard: 0 MP	    Forest: 0 MP	    Marsh: 0 MP
    Swamp: 0 MP	    Jungle: 0 MP	    Beach: 0 MP	    Broken: 0 MP
    Sand: 0 MP	    Rough: 0 MP	    Bocage: 0 MP	    Village: 0 MP
    Town: 0 MP	    City: 0 MP	    Industrial: 0 MP	    Impassible: 0 MP

    Trail: 0 MP	    Secondary: 0 MP	    Primary: 0 MP	    Rail: 0 MP
    Stream: 0 MP	    Gully: 0 MP	    Canal: 0 MP	    River: 0 MP
    Ford: 0 MP	    Lt Bridge: 0 MP	    Med Bridge: 0 MP	    Hvy Bridge: 0 MP
    Dune: 0 MP	    Embank: 0 MP	    Dike: 0 MP	    Escarp: 0 MP
    Cliff: 0 MP

Movement Cost: Naval
    Clear: 0 MP	    Water: 3 MP	    Field: 0 MP	    Brush: 0 MP
    Vineyard: 0 MP	    Orchard: 0 MP	    Forest: 0 MP	    Marsh: 0 MP
    Swamp: 0 MP	    Jungle: 0 MP	    Beach: 0 MP	    Broken: 0 MP
    Sand: 0 MP	    Rough: 0 MP	    Bocage: 0 MP	    Village: 0 MP
    Town: 0 MP	    City: 0 MP	    Industrial: 0 MP	    Impassible: 0 MP

    Trail: 0 MP	    Secondary: 0 MP	    Primary: 0 MP	    Rail: 0 MP
    Stream: 0 MP	    Gully: 0 MP	    Canal: 0 MP	    River: 0 MP
    Ford: 0 MP	    Lt Bridge: 0 MP	    Med Bridge: 0 MP	    Hvy Bridge: 0 MP
    Dune: 0 MP	    Embank: 0 MP	    Dike: -1 MP	    Escarp: 0 MP
    Cliff: 0 MP

Movement Modifier: Foot
    Normal Conditions: 100%	    Soft Conditions: 150%	    Mud Conditions: 200%
    Snow Conditions: 150%	    Frozen Conditions: 150%

Movement Modifier: Ski
    Normal Conditions: 100%	    Soft Conditions: 150%	    Mud Conditions: 200%
    Snow Conditions: 100%	    Frozen Conditions: 100%

Movement Modifier: Bicycle
    Normal Conditions: 100%	    Soft Conditions: 200%	    Mud Conditions: 400%
    Snow Conditions: 200%	    Frozen Conditions: 200%

Movement Modifier: Horse
    Normal Conditions: 100%	    Soft Conditions: 130%	    Mud Conditions: 150%
    Snow Conditions: 130%	    Frozen Conditions: 130%

Movement Modifier: Motorcycle
    Normal Conditions: 100%	    Soft Conditions: 200%	    Mud Conditions: 400%
    Snow Conditions: 200%	    Frozen Conditions: 200%

Movement Modifier: Motorized
    Normal Conditions: 100%	    Soft Conditions: 200%	    Mud Conditions: 400%
    Snow Conditions: 200%	    Frozen Conditions: 200%

Movement Modifier: Soft Halftrk
    Normal Conditions: 100%	    Soft Conditions: 150%	    Mud Conditions: 300%
    Snow Conditions: 150%	    Frozen Conditions: 150%

Movement Modifier: Armored Car
    Normal Conditions: 100%	    Soft Conditions: 200%	    Mud Conditions: 400%
    Snow Conditions: 200%	    Frozen Conditions: 200%

Movement Modifier: Hard Halftrk
    Normal Conditions: 100%	    Soft Conditions: 150%	    Mud Conditions: 300%
    Snow Conditions: 150%	    Frozen Conditions: 150%

Movement Modifier: Tracked
    Normal Conditions: 100%	    Soft Conditions: 150%	    Mud Conditions: 300%
    Snow Conditions: 150%	    Frozen Conditions: 150%

Movement Modifier: Rail
    Normal Conditions: 100%	    Soft Conditions: 100%	    Mud Conditions: 100%
    Snow Conditions: 100%	    Frozen Conditions: 100%

Movement Modifier: Aircraft
    Normal Conditions: 100%	    Soft Conditions: 100%	    Mud Conditions: 100%
    Snow Conditions: 100%	    Frozen Conditions: 100%

Movement Modifier: Naval
    Normal Conditions: 100%	    Soft Conditions: 100%	    Mud Conditions: 100%
    Snow Conditions: 100%	    Frozen Conditions: 100%

Movement Modifiers:
    ZOC Movement Multiplier: 0	    Movement Elevation Modifier: 10 MP per 100 meters

Terrain Combat Modifiers:
    Clear: 0%	    Water: 0%	    Field: -10%	    Brush: -10%
    Vineyard: -10%	    Orchard: -10%	    Forest: -20%	    Marsh: -10%
    Swamp: -20%	    Jungle: -40%	    Beach: 0%	    Broken: -10%
    Sand: 0%	    Rough: -30%	    Bocage: -40%	    Village: -20%
    Town: -30%	    City: -40%	    Industrial: -50%	    Impassible: 0%

Hexside Combat Modifiers:
    Trail: 0%	    Secondary: 0%	    Primary: 0%	    Rail: 0%
    Stream: 0%	    Gully: 0%	    Canal: 0%	    River: 0%
    Ford: 0%	    Lt Bridge: 0%	    Med Bridge: 0%	    Hvy Bridge: 0%
    Dune: -10%	    Embank: -20%	    Dike: -30%	    Escarp: -40%
    Cliff: -50%

Combat Modifiers
    Improved: -10%	    Trench: -20%	    Bunker: -20%	    Pillbox: -30%
    Bunker Defense: 20	    Pillbox Defense: 40	    Elevation: -10%	    Range Effect: 3
    Art Hard Target Mod: 1	    Quality Fire Mod: 1";
    }
}