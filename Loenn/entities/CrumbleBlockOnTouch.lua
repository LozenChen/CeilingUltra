local fakeTilesHelper = require("helpers.fake_tiles")

local crumble_block_on_touch = {}

crumble_block_on_touch.name = "CeilingUltra/CrumbleBlockOnTouch"
crumble_block_on_touch.depth = 0
crumble_block_on_touch.placements = {
    name = "Crumble Block on Touch",
    data = {
        tiletype = "3",
        width = 8,
        height = 8,
        blendin = true,
        persistent = false,
        delay = 0.1,
        destroyStaticMovers = false,
        CheckLeft = true,
        CheckRight = true,
        CheckTop = true,
        CheckBottom = true,
        BreakOnDashCollide = true,
    }
}

crumble_block_on_touch.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype", "blendin")
crumble_block_on_touch.fieldInformation = fakeTilesHelper.getFieldInformation("tiletype")
crumble_block_on_touch.fieldOrder = {"x", "y", "width", "height", "delay", "tiletype", "CheckLeft", "CheckRight", "CheckTop", "CheckBottom", "BreakOnDashCollide", "persistent", "blendin", "destroyStaticMovers"}

return crumble_block_on_touch