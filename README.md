# Unity Mesh Tilesets

[![openupm][OpenUPMBadge]][OpenUPMPackage]
![Last Commit][LastCommitBadge]
[![BuyMeCoffee][buymecoffeebadge]][buymecoffee]

Unity Mesh Tilesets is a [Unity] editor extension that can be used to automatically place objects along [ProBuilder] meshes based on rules from a tileset. The tool is heavily inspired by the [tile mesh feature of Valve's Source Engine 2][Source2] that was used for many different props in Half Life Alyx.

![Showcase](/Docs/Media/showcase.gif)

## Installation

Mesh Tilesets requires [ProBuilder]. ProBuilder will be added automatically to your project if it has not been installed yet. It is also highly recommended to install [ProGrids] as well (note that ProGrids is still in preview so you have to [enable preview packages][PreviewPackages] in the package manager).

### OpenUPM (recommended)

Make sure you have the [OpenUPM CLI][OpenUPM] installed. From the root of your project run:
```
openupm add com.freshlybrewedcode.unity-mesh-tilesets
```

### Unity Package Manager

Open the Unity package manager, click on the plus icon in the top left corner and select "Add package from git URL". Paste the following URL into the text field:
```
https://github.com/FreshlyBrewedCode/unity-mesh-tilesets.git#upm
```

**Note: You will not be able to select a specific version or update the package using this method. If you want to update to the latest version you have to remove the package and add it again using the url above.**

### Manual Download

Download or clone the repository and add the `Assets/Mesh Tilesets` folder into the `Assets` folder of your Unity project.

## Getting Started

Check out the [getting started section][GettingStarted] in the [documentation][Docs].

## How It Works

The tool works using primitive meshes that you create and modify directly inside of Unity using [ProBuilder]. You place the `TilesetRenderer` component on your tile mesh GameObject and it will find rectangular quads ("tile instances") in the mesh. Each tile instance has an orientation (top, bottom, left, right, normal), a size (width, height) and knows about other adjacent tiles. 

You assign a tileset to the `TilesetRenderer` that can contain any number of tiles. Each tile is basically a prefab that contains information about the size of the tile (width, height) and "rules" about if there can or should be any tiles adjacent to the left/right/top/bottom of the tile.

For each tile instance in the mesh the `TilesetRenderer` tries to find a tile from the assigned tileset that "fits" (by comparing size and adjacency).

Check out the [documentation][Docs] for more info.

## Examples

Here are some example use cases for the tool. I am sure there are many more creative ways to use this tool. If you have a cool example feel free leave a pull request and add it to the list.

### Pipes, Tubes

![Pipe Example](/Docs/Media/pipes_example.png)

### Ducts, Vents

![Duct Example](/Docs/Media/duct_example.png)

### Railings, Fences

![Railing Example](/Docs/Media/railing_example.png)

### Catwalks, Bridges, Stairs

![Catwalk Example](/Docs/Media/catwalk_example.png)

### Buildings (almost)

![Building Example](/Docs/Media/building_example.png)


[Unity]: https://unity.com
[ProBuilder]: https://unity3d.com/de/unity/features/worldbuilding/probuilder
[ProGrids]: https://docs.unity3d.com/Packages/com.unity.progrids@3.0/manual/index.html
[PreviewPackages]: https://docs.unity3d.com/Manual/upm-ui-list.html#ShowPreview
[Source2]: https://www.youtube.com/watch?v=3ki67VLL0xI&ab_channel=Hosomi
[Releases]: https://github.com/FreshlyBrewedCode/unity-mesh-tilesets/releases
[OpenUPM]: https://openupm.com/
[OpenUPMBadge]: https://img.shields.io/npm/v/com.freshlybrewedcode.unity-mesh-tilesets?label=openupm&registry_uri=https://package.openupm.com&style=for-the-badge
[OpenUPMPackage]: https://openupm.com/packages/com.freshlybrewedcode.unity-mesh-tilesets/
[Docs]: /Docs/README.md
[GettingStarted]: /Docs/README.md#getting-started

[LastCommitBadge]: https://img.shields.io/github/last-commit/freshlybrewedcode/unity-mesh-tilesets?style=for-the-badge
[ReleaseBadge]: https://img.shields.io/github/v/release/freshlybrewedcode/unity-mesh-tilesets?style=for-the-badge
[buymecoffee]: https://ko-fi.com/freshlybrewed
[buymecoffeebadge]: https://img.shields.io/badge/buy%20me%20a%20coffee-donate-yellow.svg?style=for-the-badge