[![GitHub release](https://img.shields.io/github/release/benpollarduk/plankton.svg)](https://github.com/benpollarduk/ktvn/releases)
[![License](https://img.shields.io/github/license/benpollarduk/plankton.svg)](https://opensource.org/licenses/MIT)

# Introduction 
A .NET 4.6.1 program that crudely simulates plankton in a pond. This was mainly developed as a way to help me understand WPF rendering, particularly the Visual and Geometry classes. Physics are not intended to be realistic or accurate, instead the emphasis is on having fun!

![image](https://user-images.githubusercontent.com/129943363/231509420-44052394-a6ed-4f17-bb1a-aa17b466b92a.png)

Plankton interact with the mouse (represented as a bubble) and the boundaries of the window. The mouse can spawn child bubbles which the plankton will also interact with. Sea beds can be drawn using a built in editor, and textures can be generated for the sea beds.

![image](https://user-images.githubusercontent.com/129943363/231221103-c8c0b5ab-2ed3-4a4d-acd0-e9dfb2ecd907.png)

Features lots of controls to adjust:
  * Plankton
  * Bubbles
  * Water
  * Current
  * Mass
  * Environment 
  * UI

![image](https://user-images.githubusercontent.com/129943363/231221171-fa35cb3d-464a-40b9-b6e8-7a5460d29341.png)

Ctrl+N randomises settings to create some unique combinations of physics and plankton settings. This can create some really interesting combinations and is worth playing about with.

![image](https://user-images.githubusercontent.com/129943363/231221471-9af8d6c2-1a84-4162-a7c8-d77878d75964.png)

# Notes
This is a fairly old project now, it was started in 2013 as a bit of fun and grew from there. It has received some quality of life updates but requires full separation of the model from the UI before it is progressed further. The code isn't optimised, and there are a handful of ways that the performance could be improved. A couple of things that really kill performance are:
 * Generating too many plankton at once
 * Using brushes with transparency
 * Generating complex sea bed paths
 * Too many bubbles

# Prerequisites
 * Windows
   * Download free IDE Visual Studio 2022 Community ( >> https://visualstudio.microsoft.com/de/vs/community/ ), or use commercial Visual Studio 2022 Version.

# Getting Started
 * Clone the repo
 * Build all projects
 * Run the BP.Plankton project

# For Open Questions
Visit https://github.com/benpollarduk/plankton/issues
