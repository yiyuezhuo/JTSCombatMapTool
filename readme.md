 # JTD/WDS Combat Map Tools

JTS games have many counters and big maps, so sometimes it's hard to grasp the situation, for yourself or readers of your AAR! So I made the tool. Now, it supports JTS/WDS Napoleonic Battles title (I tested it only on [Wellington's Peninsular War](https://wargameds.com/collections/napoleonic-battles/products/wellingtons-peninsular-war) and [The Final Struggle](https://wargameds.com/collections/napoleonic-battles/products/the-final-struggle)). It should support other titles of NB if they have the same data structure.

The Civil War Battles series may be supported in the near future since they should have a similar data structure but I have not done the research.

## Control

- Right dragging to move camera
- Scroll to zooming
- Hover to show detail of units
- Load builtin & custom scenario/save files from left bar. (Some samples are included on attachedments)

## Screenshots

<img src="https://img.itch.zone/aW1hZ2UvMjA2OTg3NC8xMjE3MzQ5Ny5wbmc=/original/gp6qMb.png">
<img src="https://img.itch.zone/aW1hZ2UvMjA2OTg3NC8xMjE3MzQ5OC5wbmc=/original/CSO2Z4.png">
<img src="https://img.itch.zone/aW1hZ2UvMjA2OTg3NC8xMjE3MzQ5OS5wbmc=/original/mJn36k.png">

## Motivation

Summary brigade (division would be supported in future) data and display an image like a pretty battle illustration like [Waterloo map](https://en.wikipedia.org/wiki/Battle_of_Waterloo#The_Grand_Battery_starts_its_bombardment) and [Leipzig Map](https://en.wikipedia.org/wiki/Battle_of_Leipzig#Action_at_Markkleeberg).

The tool creates an interactive map that shows the organization's name, strength, maneuver path on the map.

## TODO List

- [ ] French & Spanish Font Problem (It's too late to solve due to the time limit of the Jam)
- [ ] Formation Depth (Use the second eigenvalue instead of hardcoding)
- [ ] Division/Corp Level Summary
- [ ] Displaying Road
- [ ] Location Labels
- [ ] Simple 3D to show elevation (the built-in display method of JTS games is terrible!!!)
- [ ] Refined Artillry display
- [ ] Global optimization for label locations to prevent collision and overlap.
- [ ] Fix Cavalry icon Direction
- [ ] Show the leader's name on the tooltip
- [ ] Support PBEM save files.
- [ ] Label Size invariance when zooming
- [ ] Summarized attack arrow.
- [ ] Permit some manual editing (hide unit, adjust direction to fix artifact due to algorithm.)

## Algorithm

Units are grouped by brigade and do simple clustering, then fit a 2d normal distribution to create a description for their shape.
