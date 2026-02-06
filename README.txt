TxpConvert

2024-2026 Haruka

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

txpconvert extract --v-flip texture.bin images texture_table.bin

Extracts all textures from texture.bin, mapped by the file table in texture_table.bin into a folder called "images" and flips them vertically.
This will also create a "txp_metadata.ini", which contains information required for repacking.
Using texture table files is optional. Not specifying them will cause textures to be created without meaningful file names.

txpconvert create SetWithSubsets images texture.bin

Repacks all textures from the folder "images" back to texture.bin.
If a file table was used during extraction, the sprites are repacked. If not, the textures are repacked.


To pack and unpack the .farc containers, use FarcPack: https://github.com/akechi-haruka/MikuMikuLibrary/releases
