local drawableSpriteStruct = require("structs.drawable_sprite")

local cloud = {}

cloud.name = "CeilingUltra/SidewaysCloud"
cloud.depth = -9000
cloud.placements = {
    name = "Sideways Cloud",
    data = {
        left = true,
        fragile = false,
        small = false,
        letSeekersThrough = false,
        pushPlayer = true,
        cornerCorrect = false,
        ExitSpeed = 90,
        CoyoteTime = 0.1
    }
}
cloud.associatedMods = {"MaxHelpingHand", "CeilingUltra"}
cloud.fieldOrder = {"x", "y", "ExitSpeed", "CoyoteTime", "left", "fragile", "small", "letSeekersThrough", "pushPlayer", "cornerCorrect" }

local normalScale = 1.0
local smallScale = 29 / 35

local function getTexture(entity)
    local fragile = entity.fragile

    if fragile then
        return "objects/clouds/fragile00"

    else
        return "objects/clouds/cloud00"
    end
end

function cloud.sprite(room, entity)
    local texture = getTexture(entity)
    local sprite = drawableSpriteStruct.fromTexture(texture, entity)
    local small = entity.small
    local scale = small and smallScale or normalScale

    sprite:setScale(scale, 1.0)
    sprite:setJustification(1/2, 1/2)
    sprite:addPosition(0, -16)
    if entity.left then
        sprite.rotation = - math.pi / 2
    else
        sprite.rotation = math.pi / 2
    end

    return sprite
end

function cloud.selection(room, entity)
    return utils.rectangle(entity.x - 4, entity.y - 32, 8, 32)
end

return cloud