extends "res://probes/engine_private_wind_cast_observation.gd"

var hd_review = preload("res://probes/HdBattleProjection.gd").new()

func scene() -> Dictionary:
    var current := super.scene()
    hd_review.observe(current.scene, view, check)
    return current

func finish() -> void:
    hd_review.finish(check)
    for row in frames_seen.values():
        check(row.hd and row.textureWidth == row.width * 4 and row.textureHeight == row.height * 4,
            "actual BLAST fragment density and logical footprint")
    samples.append({"hdReadback":hd_review.samples,"hdDeath":hd_review.death_seen})
    super.finish()
