@echo off
chcp 932
set PATH=%PATH%;%CD%\tools;
.\tools\excel\xlsx-text .\MetroLLRedux日本語化作業所.xlsx .\resource\localization\stable_jp.txt
lngpack_redux .\resource\localization\stable_jp.txt .\out\stable_us.lng .\resource\textures\font\chtable.txt

rem -- フォント生成
pushd .\
cd .\resource\textures\font

bmfont64 -c font_metro_for_menu_main_strings_us.bmfc -o font_metro_for_menu_main_strings_us.fnt -t chtable.txt
makefont_redux chtable.txt font_metro_for_menu_main_strings_us.fnt > font_metro_for_menu_main_strings_us.txt
bmfont64 -c font_metro_2034_font_2_us.bmfc -o font_metro_2034_font_2_us.fnt -t chtable.txt
makefont_redux chtable.txt font_metro_2034_font_2_us.fnt > font_metro_2034_font_2_us.txt

bmfont64 -c font_metro_for_map_us_512.bmfc -o font_metro_for_map_us_512.fnt -t chtable.txt
bmfont64 -c font_metro_for_map_us_1024.bmfc -o font_metro_for_map_us_1024.fnt -t chtable.txt
makefont_redux chtable.txt font_metro_for_map_us_1024.fnt > font_metro_for_map_us.txt

bmfont64 -c font_metro_for_menu_us.bmfc -o font_metro_for_menu_us.fnt -t chtable.txt
makefont_redux chtable.txt font_metro_for_menu_us.fnt > font_metro_for_menu_us.txt
bmfont64 -c font_font_regular_us.bmfc -o font_font_regular_us.fnt -t chtable.txt
makefont_redux chtable.txt font_font_regular_us.fnt > font_font_regular_us.txt
popd

rem -- symbol.xml ファイル合成
type .\resource\symbol\symbol_1.txt > .\resource\symbol.xml
type .\resource\textures\font\font_metro_for_menu_main_strings_us.txt >> .\resource\symbol.xml
type .\resource\symbol\symbol_2.txt >> .\resource\symbol.xml
type .\resource\textures\font\font_metro_2034_font_2_us.txt >> .\resource\symbol.xml
type .\resource\symbol\symbol_3.txt >> .\resource\symbol.xml
type .\resource\textures\font\font_metro_for_map_us.txt >> .\resource\symbol.xml
type .\resource\symbol\symbol_4.txt >> .\resource\symbol.xml
type .\resource\textures\font\font_metro_for_menu_us.txt >> .\resource\symbol.xml
type .\resource\symbol\symbol_5.txt >> .\resource\symbol.xml
type .\resource\textures\font\font_font_regular_us.txt >> .\resource\symbol.xml
type .\resource\symbol\symbol_6.txt >> .\resource\symbol.xml
type .\resource\textures\font\font_metro_for_map_us.txt >> .\resource\symbol.xml
type .\resource\symbol\symbol_7.txt >> .\resource\symbol.xml

rem -- scripts.bin ファイルパック
scriptsplitter.exe .\resource\unpack\config.bin .\resource\scripts
sympack_redux .\resource\symbol.xml .\resource\scripts\54_FA8EEA67.split
scriptconcat .\resource\scripts .\out\config.bin

rem -- .vfs 生成
copy /Y .\resource\textures\font\font_metro_for_menu_main_strings_us_0.dds .\dds\font_metro_for_menu_main_strings_us.dds
copy /Y .\resource\textures\font\font_metro_2034_font_2_us_0.dds .\dds\font_metro_2034_font_2_us.dds
copy /Y .\resource\textures\font\font_metro_for_map_us_512_0.dds .\dds\font_metro_for_map_us_512.dds
copy /Y .\resource\textures\font\font_metro_for_map_us_1024_0.dds .\dds\font_metro_for_map_us_1024.dds
copy /Y .\resource\textures\font\font_metro_for_menu_us_0.dds .\dds\font_metro_for_menu_us.dds
copy /Y .\resource\textures\font\font_font_regular_us_0.dds .\dds\font_font_regular_us.dds

DDSConverter .\dds\font_metro_for_menu_main_strings_us.dds .\out\font_metro_for_menu_main_strings_us.512
DDSConverter .\dds\font_metro_2034_font_2_us.dds .\out\font_metro_2034_font_2_us.512
DDSConverter .\dds\font_metro_for_map_us_512.dds .\out\font_metro_for_map_us.512
DDSConverter .\dds\font_metro_for_map_us_1024.dds .\out\font_metro_for_map_us.1024
DDSConverter .\dds\font_metro_for_menu_us.dds .\out\font_metro_for_menu_us.512
DDSConverter .\dds\font_font_regular_us.dds .\out\font_font_regular_us.512

metro_redux_insert_newvfs