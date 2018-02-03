# UNSME - Ultimate Ninja Storm Model Editor

![unsmelogo](https://user-images.githubusercontent.com/33766519/35544569-cfeb386a-0549-11e8-8bdf-f2c8fa1acc6e.png)

Latest release in: github.com/zealottormunds/unsme/releases

# Changelog 1.5a:
- 6C and 80 fix is now optional, as it fucks up meshes with multiple groups.
- Only vertices you moved get saved now, so the rest don't get fucked up.
- Minor tweaks and bugfixes.
- Herobrine removed.

# Changelog 1.4a:
- Added support for multi-grouped meshes. Most meshes should work now without breaking triangles.
- Fixed a model-breaking bug when importing low poly models.
- Fixed a few bugs when saving an .xfbin
- Stability improved

# Changelog 1.3a:
- Rewritten most of the .obj import feature
- Fixed UVs loading incorrectly in skeletonless models
- The tool now supports .obj files from basically any editor
- Errors with the tool now prompt a message box, which makes them easier to report.
- Tweaks and bugfixes.
- Performance improved.

# Changelog 1.2a:
- Byte 6C gets fixed automatically.
- Byte 80 gets fixed automatically.
- Minor tweaks and bugfixes.

# Changelog 1.1a:
- Added weight editor.

# Changelog 1.0a:
- Added export options.
- The tool can now open .unsmf decompiled models like in previous builds. They can also be saved with /saveunsmf.
- Added /closefile.
- Fixed the debug message when saving as .xfbin.

# Changelog 0.9a:
- Fixed all the size bugs.
- Added support for more texture types.

# Changelog 0.8b:
- Fixed the bug when opening files.

# Changelog 0.8a:
- You can now open and save .xfbin files directly.
- Fixed triangle bug (again. I swear, this goddamn bug keeps coming back).
- Performance improved. Opening and saving models with lots of vertices won't take as long anymore.
- Fixed resolution problems.

# Changelog 0.7a:
- Fixed the model unpacker. Now it's included with the editor.
- Fixed mirroring when exporting obj files.
- Minor bugfixes.

# Changelog 0.6a:
- Added /openstage that scales the stages automatically so you can work with them.
- Added /inverttriangles (I thought I'd done this in 0.5a already).

# Changelog 0.5a
- Added tool box.
- Added bone editor tool.
- Command /github opens github.com/zealottormunds/unsme in your browser.

# Changelog 0.4a
- Fixed a bug that changed vertex order, thus breaking all the triangles and model data. Saving should work fine now.
- Fixed /exportobj. Models are exported and imported correctly now.
- You can either use file commands with the file explorer inputting only the command (like /openmodel) or you can also input a path like in earlier versions (like /openmodel C:/modelFolder).
- Support for 32 bits.

# Changelog 0.3c
- Added command /invertnormals for lighting issues

# Changelog 0.3b
- .obj importing improved
- Added a version checker everytime the tool is started

# Changelog 0.3a
- Added file explorer instead of using PATH.
- .obj models no longer have to be divided to be imported.
- Added /rotate command
- Added /sensitivity and a configuration file.
- Added /loadtexture to reload Texture0.png
- Vertex normals get exported correclty now
- Fixed .obj texture coordinate exporting
- Fixed /scale command
- /exporttoobj has been changed to /exportobj

# Changelog 0.2a
- Removed wireframe bug

# Changelog 0.1a
- First public release
- Added vertex edition
- Added triangle edition
- Added .obj export
- Added .obj import (experimental)
