# Component Reference

- [Component Reference](#component-reference)
  - [TilesetRenderer](#tilesetrenderer)
  - [Tileset](#tileset)
  - [Tile](#tile)


## TilesetRenderer

![TilesetRenderer Inspector](/Docs/Media/tileset_renderer.png)

The `TilesetRenderer` component should be placed on the ProBuilder mesh. It is responsible for analyzing the mesh and finding tile instances, and placing matching tiles from the assigned tileset. 

Note that the `TilesetRenderer` will assign a special material to the ProBuilder mesh that simply hides the entire mesh. The mesh itself is visualized using gizmos.

The tiles created by the `TilesetRenderer` are created as children of the mesh GameObject but are **hidden by default** to reduce clutter in the hierarchy. Additionally, **scene view selection is also disabled for all tiles**. Because of this you have to select the mesh GameObject from the hierarchy are click on the invisible mesh in the scene view (Clicking just on the tiles in the scene view won't work).

Also, **do not add children to the mesh GameObject**. The `TilesetRenderer` will frequently remove/disable its children.

If at any point you don't want to use the `TilesetRenderer` anymore but work with the individual generated tiles you can simply remove the `TilesetRenderer` component. This will show all tiles.

The `TilesetRenderer` component should not be used to create tiles at runtime. The package included a scene post processor that will remove all `TilesetRenderer` components from the scene for builds.

The following table shows a list of options you can configure in the inspector:

| Option                 | Description                                                                                                                                                                                                                                                                                                                                                                                                                           |
| ---------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Tileset                | The tileset component that should be used. Drag your tileset Prafab here.                                                                                                                                                                                                                                                                                                                                                             |
| Tile Size Match        | The method that should be used to compare the size (width/height) of the tiles. `Exact` will **only** match a tile if it is exactly the same size as the tile instance. `Closest Match` will **always** match the tile that is closest (useful if you want more loose tile placement like the railing tileset).                                                                                                                       |
| Prefer Fewer Rotations | The `TilesetRenderer` will rotate a tile if its orientation does not match initially with the orientation of the tile instance. If this option is enabled tiles that can be matched with fewer rotations are preferred. This is useful for tilesets that contain tiles with fixed orientations (i.e. the railing tileset).                                                                                                            |
| Tile Orientation       | The method that should be used to determine the orientation of the tile instances. Default is `From Vertex Order` which is fine for most tilesets that do not depend on fixed tile orientations. The other options can be used to determine orientation based on world/object space orientation. You can enable the `Orientation` option in the `Visualization` section to display the orientation of each tile instance in the mesh. |
| Selected Tile          | Shows debug information about the selected tile instance (the selected face) and allows you to edit the tile flags.                                                                                                                                                                                                                                                                                                                   |
| Visualization          | Allows you to select different visualization options for the tile instances (mainly for debugging). `Edge Flags` shows the adjacency of each tile instance using small spheres at the edges. The color indicates convex (red) or concave (green) edges.                                                                                                                                                                               |
| Debug                  | Some debug buttons to fix problems with the tiles. If something does not look right, tiles are not refreshed or cleaned up properly try to press "Clear Pool", "Rebuild Pool", and "Full Refresh". This will delete all tiles from the pool an perform a full refresh of the tile instances.                                                                                                                                          |

## Tileset

*TODO*

## Tile

*TODO*