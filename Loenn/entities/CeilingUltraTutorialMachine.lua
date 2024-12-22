local drawableSprite = require "structs.drawable_sprite"
local utils = require "utils"

local tutorialMachine = {}

tutorialMachine.name = "CeilingUltra/CeilingUltraTutorialMachine"
tutorialMachine.depth = 1000

tutorialMachine.placements = {
        name = "Ceiling Ultra Tutorial Machine",
        placementType = "rectangle",
}


local leftTexture = "CeilingUltra/Presentation/building/building_front_left"
local rightTexture = "CeilingUltra/Presentation/building/building_front_right"


function tutorialMachine.sprite(room, entity)
    local sprites = {}

    local leftSprite = drawableSprite.fromTexture(leftTexture, entity)
    local rightSprite = drawableSprite.fromTexture(rightTexture, entity)

    leftSprite:setJustification(0.5, 1.0)

    rightSprite:setJustification(0.5, 1.0)

    table.insert(sprites, rightSprite)
    table.insert(sprites, leftSprite)

    return sprites
end

function tutorialMachine.rectangle(room, entity)
    return utils.rectangle(entity.x - 60, entity.y - 64, 120, 64)
end

return tutorialMachine
