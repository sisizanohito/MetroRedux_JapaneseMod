@echo off
chcp 932
set PATH=%PATH%;%CD%\tools;

rem -- 必要なものの確認
if not exist .\resource\unpack\config.bin (
  echo resource\unpack\config.bin がありません。ゲームの content\config.bin をコピーしてください。
  exit /b 1
)
if not exist .\resource\unpack\content.vfx (
  echo resource\unpack\content.vfx がありません。MOD を当てていないゲームの content.vfx をコピーしてください。
  exit /b 1
)
where python >nul 2>&1
if errorlevel 1 (
  echo python が見つかりません。Python 3 をインストールしてください。
  exit /b 1
)

.\tools\excel\xlsx-text .\MetroLLRedux日本語化作業所.xlsx .\resource\localization\stable_jp.txt
lngpack_redux .\resource\localization\stable_jp.txt .\out\stable_us.lng .\resource\textures\font\chtable.txt

rem -- フォント生成
pushd .\
cd .\resource\textures\font

bmfont64 -c font_metro_for_menu_main_strings_us.bmfc -o font_metro_for_menu_main_strings_us.fnt -t chtable.txt
makefont_redux chtable.txt font_metro_for_menu_main_strings_us.fnt > font_metro_for_menu_main_strings_us.txt
bmfont64 -c font_metro_2034_font_2_us.bmfc -o font_metro_2034_font_2_us.fnt -t chtable.txt
makefont_redux chtable.txt font_metro_2034_font_2_us.fnt > font_metro_2034_font_2_us.txt

rem map フォントは 2048x2048 で作る (R=G=文字、B=0、A=不透明の tga。crunch で DXT1 にする)
bmfont64 -c font_metro_for_map_us_2048.bmfc -o font_metro_for_map_us_2048.fnt -t chtable.txt
makefont_redux chtable.txt font_metro_for_map_us_2048.fnt > font_metro_for_map_us.txt

bmfont64 -c font_metro_for_menu_us.bmfc -o font_metro_for_menu_us.fnt -t chtable.txt
makefont_redux chtable.txt font_metro_for_menu_us.fnt > font_metro_for_menu_us.txt
bmfont64 -c font_font_regular_us.bmfc -o font_font_regular_us.fnt -t chtable.txt
makefont_redux chtable.txt font_font_regular_us.fnt > font_font_regular_us.txt
popd

rem -- symbol.xml ファイル合成
rem ゲームの fonts_us は 6 つの用途すべてで map のテクスチャを使うため、グリフ表もすべて map (2048) のものを入れる。
rem symbol_N.txt の pitch (1/2048) と h7 は 2048 用の値にしてある
type .\resource\symbol\symbol_1.txt > .\resource\symbol.xml
type .\resource\textures\font\font_metro_for_map_us.txt >> .\resource\symbol.xml
type .\resource\symbol\symbol_2.txt >> .\resource\symbol.xml
type .\resource\textures\font\font_metro_for_map_us.txt >> .\resource\symbol.xml
type .\resource\symbol\symbol_3.txt >> .\resource\symbol.xml
type .\resource\textures\font\font_metro_for_map_us.txt >> .\resource\symbol.xml
type .\resource\symbol\symbol_4.txt >> .\resource\symbol.xml
type .\resource\textures\font\font_metro_for_map_us.txt >> .\resource\symbol.xml
type .\resource\symbol\symbol_5.txt >> .\resource\symbol.xml
type .\resource\textures\font\font_metro_for_map_us.txt >> .\resource\symbol.xml
type .\resource\symbol\symbol_6.txt >> .\resource\symbol.xml
type .\resource\textures\font\font_metro_for_map_us.txt >> .\resource\symbol.xml
type .\resource\symbol\symbol_7.txt >> .\resource\symbol.xml

rem -- scripts.bin ファイルパック
rem 前回の分割結果が残っていると結合時に混ざるため削除する
if exist .\resource\scripts rmdir /s /q .\resource\scripts
scriptsplitter.exe .\resource\unpack\config.bin .\resource\scripts
rem フォント定義チャンク(ID:0xFA8EEA67)の番号はゲームのバージョンで変わるため、IDで探す
set SYMSPLIT=
for %%F in (.\resource\scripts\*_FA8EEA67.split) do set SYMSPLIT=%%F
if "%SYMSPLIT%"=="" (
  echo FA8EEA67 チャンクが見つかりません。
  exit /b 1
)
sympack_redux .\resource\symbol.xml %SYMSPLIT%
scriptconcat .\resource\scripts .\out\config.bin

rem -- テクスチャ生成
if not exist .\dds mkdir .\dds
copy /Y .\resource\textures\font\font_metro_for_menu_main_strings_us_0.dds .\dds\font_metro_for_menu_main_strings_us.dds
copy /Y .\resource\textures\font\font_metro_2034_font_2_us_0.dds .\dds\font_metro_2034_font_2_us.dds
copy /Y .\resource\textures\font\font_metro_for_menu_us_0.dds .\dds\font_metro_for_menu_us.dds
copy /Y .\resource\textures\font\font_font_regular_us_0.dds .\dds\font_font_regular_us.dds
rem crunch はマルチスレッドだと実行ごとに結果がわずかに変わるため、-helperThreads 0 (シングルスレッド) で動かす
set MAPTGA=.\resource\textures\font\font_metro_for_map_us_2048_0.tga
crunch_x64 -file %MAPTGA% -fileformat dds -DXT1 -mipMode None -out .\dds\font_metro_for_map_us_2048.dds -helperThreads 0 -noprogress
crunch_x64 -file %MAPTGA% -rescale 1024 1024 -fileformat dds -DXT1 -mipMode None -out .\dds\font_metro_for_map_us_1024.dds -helperThreads 0 -noprogress
crunch_x64 -file %MAPTGA% -rescale 512 512 -fileformat dds -DXT1 -mipMode Generate -out .\dds\font_metro_for_map_us_512.dds -helperThreads 0 -noprogress

DDSConverter .\dds\font_metro_for_menu_main_strings_us.dds .\out\font_metro_for_menu_main_strings_us.512
DDSConverter .\dds\font_metro_2034_font_2_us.dds .\out\font_metro_2034_font_2_us.512
DDSConverter .\dds\font_metro_for_map_us_2048.dds .\out\font_metro_for_map_us.2048
DDSConverter .\dds\font_metro_for_map_us_1024.dds .\out\font_metro_for_map_us.1024
DDSConverter .\dds\font_metro_for_map_us_512.dds .\out\font_metro_for_map_us.512
DDSConverter .\dds\font_metro_for_menu_us.dds .\out\font_metro_for_menu_us.512
DDSConverter .\dds\font_font_regular_us.dds .\out\font_font_regular_us.512
rem テクスチャの幅・高さを 2048 にした .bin (バニラの .bin のオフセット 6 と 10 を書き換えたもの)
copy /Y .\resource\textures\font\font_metro_for_map_us.bin .\out\font_metro_for_map_us.bin

rem -- .vfx / .vfs0 生成 (出力先: release フォルダ)
rem パッカーは content\... の階層で照合するので、pack フォルダに並べ直す
if exist .\pack rmdir /s /q .\pack
mkdir .\pack\content\localization
mkdir .\pack\content\textures\font
copy /Y .\out\config.bin .\pack\content\config.bin
copy /Y .\out\stable_us.lng .\pack\content\localization\stable_us.lng
copy /Y .\out\font_*.* .\pack\content\textures\font\
rem パッカーのログをコンソールと同じ文字コード (cp932) で出す
set PYTHONIOENCODING=cp932
python .\tools\redux_pack.py .\resource\unpack\content.vfx .\pack .\release
if errorlevel 1 (
  echo パックに失敗しました。
  exit /b 1
)
