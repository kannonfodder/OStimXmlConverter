# OStim Scene XML Converter tool

The OStim SML converter tool starts the conversion process from OSA xml files to OStim json files. 

Run the tool with `XmlConverter.exe <input> <output> <modpackname>` or omit the input and output and it will ask for values.

`<input>` is the source directory to look for animations (e.g. C:/Mods/OpenAnimations RE)

`<output>` is the directory the converted jsons will saved. They will lose any internal folder structure so any clashing names will be overwritten.

`<modpackname>` (optional) is the modPack field on the converted xmls. If not provided it will use the foldername directly under /scene/ in the input filename.

It will trawl the folders for valid scene files, convert them to json and dump them into the output folder


V0.1:
- Scene:
  - Name
  - ModPack Name
  - Length
  - Destination
  - Navigations
  - Speeds
  - NoRandomSelection
  - Furninture
  - Tags
  - Actors
  - Actions