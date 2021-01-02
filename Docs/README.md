# Documentation

- [Documentation](#documentation)
  - [Installation](#installation)
  - [Getting Started](#getting-started)
    - [Using the example pipes tileset](#using-the-example-pipes-tileset)
  - [Creating Tilesets](#creating-tilesets)
  - [Components](#components)

## Installation

Check out the [installation section](/README.md#installation) in the main README.

## Getting Started

First, you should familiarize yourself with the basic concept of the tool. You can find a [basic explanation of how the tool works](/README.md#how-it-works) in the main README. Additionally, I would highly recommend to check out the Half Life Alyx workshop tools for yourself and take a look at how Valve used this kind of tooling in the game. After all, this is what inspired the tool in the first place.

You can also take a look at the [included samples](/Assets/Mesh%20Tilesets/Samples). You can import them into your project from the package manager window.

### Using the example pipes tileset

In this section I will break down the included example pipes tileset, how to use it and how it works.

First, let's take a look at the content of pipe tileset:

1. Import the example tilesets (Package Manager > Mesh Tilesets > Samples > Example Tilesets)
2. Open the `Pipe_Tileset` prefab (`Assets/Samples/Mesh Tilesets/<version>/Tilesets`)

You should be able to see the all the tiles from the tileset in the scene view:

![Pipe Tileset](/Docs/Media/pipe_tileset.png)

Each tile is a child of the tileset prefab. If you zoom in on one of tiles you can see the width and height indicated by the numbers as well as the orientation indicated by the small arrow gizmos. The red arrow points towards the "right" of the tile and the green arrow towards the "top". If you select one of the tiles from the hierarchy or the scene view you can see the `Tile` component in the inspector. Here you can find the settings of the tile including the width/height and the adjacency rules ("edge flags"). 

As you can see, the pipe tileset works with tiles that use whole unit increments for their width and height (1x1, 1x2, etc.). Because of this, it is best to set the grid size/increment size to 1 unit when working with this tileset.

Let's use the tileset in a scene.

1. Open an existing or new scene.
2. Open the ProBuilder window (Tools > ProBuilder > ProBuilder Window)
3. Click the plus icon next to the "New Shape" option at the top of the ProBuilder window
4. Create a simple quad by selecting the "Plane" shape and setting width and length segments to 0.
5. Select the new GameObject and add the `TilesetRenderer` component.
6. Drag the `Pipe_Tileset` Prefab on the `Tileset` field of the `TilesetRenderer` component.
7. Go into ProBuilders edge mode and extrude one edge of the plane to you end up with two connected 1x1 quads (hold shift and drag to extrude using ProBuilder)

You should end up with something like this:

![Simple Pipe](/Docs/Media/simple_pipe.png)

You probably noticed that the two end pieces of the pipe appeared as soon as you extruded the second quad. This is because before the extrusion no tile of the tileset matched the tile instance in the mesh. 

You can keep extruding the mesh to add more segments to the pipe and experiment with bends and longer segments. Note that you are still just editing a primitive mesh using ProBuilder so you are not limited to just extruding the mesh. However, the `TilesetRenderer` will only create tile instances for rectangular quads. Any other faces will also appear red in the scene view:

![Invalid Mesh](/Docs/Media/rectangular_quads.png)

Take a look at the next section to learn how you can create your own tilesets. Or check out the [component reference][Components] for more information on the individual components and fields.

## Creating Tilesets

*TODO*

## Components

See the [component reference][Components] for an overview of the individual components.


[Components]: /Docs/Components.md