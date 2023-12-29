local drawableSprite = require "structs.drawable_sprite"
local utils = require "utils"

local iceCeiling = {}

iceCeiling.name = "CeilingUltra/IceCeiling"
iceCeiling.depth = 1999

iceCeiling.placements = {
        name = "Ice Ceiling",
        placementType = "rectangle",
        data = {
            width = 8,
            Attached = true,
        }
}


local iceLeftTexture = "CeilingUltra/IceCeiling/iceLeft00"
local iceMiddleTexture = "CeilingUltra/IceCeiling/iceMid00"
local iceRightTexture = "CeilingUltra/IceCeiling/iceRight00"

local function getTextures(entity)
    return iceLeftTexture, iceMiddleTexture, iceRightTexture
end

function iceCeiling.sprite(room, entity)
    local sprites = {}

    local width = entity.width or 8
    local tileWidth = math.floor(width / 8)

    local leftTexture, middleTexture, rightTexture = getTextures(entity)

    for i = 2, tileWidth - 1 do
        local middleSprite = drawableSprite.fromTexture(middleTexture, entity)

        middleSprite:addPosition((i - 1) * 8 , 0)
        middleSprite:setJustification(0.0, 0.0)

        table.insert(sprites, middleSprite)
    end

    local leftSprite = drawableSprite.fromTexture(leftTexture, entity)
    local rightSprite = drawableSprite.fromTexture(rightTexture, entity)

    leftSprite:setJustification(0.0, 0.0)

    rightSprite:addPosition((tileWidth - 1) * 8, 0)
    rightSprite:setJustification(0.0, 0.0)

    table.insert(sprites, rightSprite)
    table.insert(sprites, leftSprite)

    return sprites
end

function iceCeiling.rectangle(room, entity)
    return utils.rectangle(entity.x, entity.y, entity.width or 8, 8)
end

return iceCeiling
