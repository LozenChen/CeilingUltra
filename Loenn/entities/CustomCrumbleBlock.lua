local drawableNinePatch = require("structs.drawable_nine_patch")
local utils = require("utils")

local crumbleBlock = {}

local Directions = {
    "Left", "Right", "Down", "Up"
}

local DirToTexture = { }

DirToTexture["Left"] = "CeilingUltra/CrumbleBlock/left"
DirToTexture["Right"] = "CeilingUltra/CrumbleBlock/right"
DirToTexture["Up"] = "CeilingUltra/CrumbleBlock/up"
DirToTexture["Down"] = "CeilingUltra/CrumbleBlock/down"


crumbleBlock.name = "CeilingUltra/CustomCrumbleBlock"
crumbleBlock.depth = -1
crumbleBlock.fieldInformation = {
    Facing = {
        options = Directions,
        editable = false
    }
}

crumbleBlock.placements = { }
crumbleBlock.placements[1] = {
    name = "Custom Crumble Block (Wall)",
    data = {
        height = 8,
        Facing = "Left",
        ActivateOnDashCollide = true
    }
}
crumbleBlock.placements[2] = {
    name = "Custom Crumble Block (Ceiling)",
    data = {
        width = 8,
        Facing = "Down",
        ActivateOnDashCollide = true
    }
}


local ninePatchOptions = {
    mode = "fill",
    fillMode = "repeat",
    border = 0
}

function crumbleBlock.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local facing = entity.Facing
    local horizontal = facing == "Up" or facing == "Down"
    local width, height
    if horizontal then
        width, height = math.max(entity.width or 0, 8), 8
    else
        width, height = 8, math.max(entity.height or 0, 8)
    end

    local texture = DirToTexture[facing]

    local ninePatch = drawableNinePatch.fromTexture(texture, ninePatchOptions, x, y, width, height)

    return ninePatch
end

function crumbleBlock.selection(room, entity)
    local facing = entity.Facing
    local horizontal = facing == "Up" or facing == "Down"
    local result
    if horizontal then
        if entity.width == nil then
            entity.width = 8
        end
        result = utils.rectangle(entity.x or 0, entity.y or 0, math.max(entity.width or 0, 8), 8)
        entity.height = nil
    else
        if entity.height == nil then
            entity.height = 8
        end
        result = utils.rectangle(entity.x or 0, entity.y or 0, 8, math.max(entity.height or 0, 8))
        entity.width = nil
    end
    return result
end

return crumbleBlock