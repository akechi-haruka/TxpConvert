TxpConvert

2024 Haruka

Command line tool to convert texture.bin/*.txp to *.dds and back.
Specialized for KC:Arcade

Based on the Project Diva noesis script
Origin: github.com/h-kidd/noesis-project-diva
By Minmode
Special thanks: Chrrox, korenkonder, BlueSkyth, Brolijah, samyuu

Modified By Billons007, Kancolle Shader param / Animation Support
Special Thanks: Joschka, tjoener, chronovore, SirKane

---------------------------------------------

Example Usage:

txpconvert extract --v-flip texture.bin images
Extracts all textures from texture.bin into a folder called "images" and flips them vertically.
This will also create a "txp_metadata.ini", which contains information required for repacking.

txpconvert create SetWithSubsets images texture.bin
Repacks all textures from the folder "images" back to texture.bin.

To pack and unpack the .farc containers, use FarcPack: https://github.com/blueskythlikesclouds/MikuMikuLibrary/releases