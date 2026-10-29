# Battle status gauge images

These four minimal PNG segments were authored for this project. They contain only geometric
blue/gold frames, dark tracks and shaded green/blue fills. They contain no extracted game pixels,
private material or third-party art, and do not claim original-game fidelity.

The project dedicates these four images to the public domain under CC0 1.0:
https://creativecommons.org/publicdomain/zero/1.0/

frame.png is 244×28 RGBA8, with a 2px pale-gold border and blue interior. track.png, hp-fill.png
and mp-fill.png are 232×16 RGBA8 segments. The common BattleStatusCard crops fill images to the
current/max ratio and draws them inside a 6px frame inset. Native labels supply all text.
The resources use nearest sampling without mipmaps. PNG is the runtime/master format for this
small set; no SVG candidate, new asset generator or private-asset repository admission is implied.
