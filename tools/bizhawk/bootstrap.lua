local Bootstrap = {}

function Bootstrap.battle01_intro_skip(profile, prompt_count, pulse)
    if profile ~= "battle01-intro-skip" then
        error("Battle 01 prompt handling requires the battle01-intro-skip profile")
    end
    if prompt_count == 1 or prompt_count == 2 then
        pulse("Right")
        pulse("C")
        return true
    end
    return false
end

return Bootstrap
