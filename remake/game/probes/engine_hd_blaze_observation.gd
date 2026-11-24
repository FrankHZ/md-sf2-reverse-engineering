extends "res://probes/engine_prepared_blaze_observation.gd"

var hd_review = preload("res://probes/HdBattleProjection.gd").new()

func scene() -> Dictionary:
    var current := super.scene()
    hd_review.observe(current.scene, view, check)
    return current

func finish() -> void:
    hd_review.finish(check)
    for row in frames_seen.values():
        var raster = row if rank_two else row.flame
        check(raster.hd and raster.textureWidth == raster.width * 4 and raster.textureHeight == raster.height * 4,
            "actual BLAZE fragment density and logical footprint")
    samples.append({"hdReadback":hd_review.samples,"hdDeath":hd_review.death_seen})
    super.finish()
