 # JTD/WDS Combat Map Tools

JTS games have many counters and big maps, so sometimes it's hard to grasp the situation, for yourself or readers of your AAR! So I made the tool. Now, it supports JTS/WDS Napoleonic Battles title (I tested it only on [Wellington's Peninsular War](https://wargameds.com/collections/napoleonic-battles/products/wellingtons-peninsular-war) and [The Final Struggle](https://wargameds.com/collections/napoleonic-battles/products/the-final-struggle)). It should support other titles of NB if they have the same data structure.

The civil war series may be supported in the near future since they should have a similar data structure but I have not done the research.

## Motivation

Summary brigade (division would be supported in future) data and display an image like a pretty battle illustration like this:

<img src="https://en.wikipedia.org/wiki/Battle_of_Waterloo#/media/File:Battle_of_Waterloo.svg">

The tool creates an interactive map that shows the organization's name, strength, maneuver path, and loss on the map.

## Algorithm

Units are grouped by brigade and do simple clustering, then fit a 2d normal distribution to create a description for their shape.
