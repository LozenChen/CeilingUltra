local map_config = {}

map_config.name = "CeilingUltra/CeilingUltraMapConfig"
map_config.depth = -20000
map_config.texture = "CeilingUltra/MapConfig/MapConfigIcon"

local advancedFieldOrder = {
    "x", "y",
    "ModEnable",
    "CeilingRefillStamina",
    "CeilingRefillDash",
    "CeilingJumpEnabled",
    "CeilingHyperEnabled",
    "CeilingUltraEnabled",
    "UpdiagDashEndNoHorizontalSpeedLoss",
    "WallRefillStamina",
    "WallRefillDash",
    "VerticalHyperEnabled",
    "VerticalUltraEnabled",
    "DashBeginNoVerticalSpeedLoss",
    "UpdiagDashEndNoVerticalSpeedLoss",
    "UpwardWallJumpAcceleration",
    "DownwardWallJumpAcceleration",
    "WaterWaveDashEnabled",
    "WaterCeilingHyperEnabled",
    "WaterBottomSurfaceJumpEnabled",
    "GroundJumpEnabled",
    "GroundHyperEnabled",
    "GroundUltraEnabled",
    "QoLBufferVerticalHyper",
    "QoLBufferVerticalUltra",
    "QoLRefillDashOnWallJump",
    "QoLBufferCeilingUltra",
}

local normalFieldOrder = {
    "x", "y",
    "ModEnable",
    "WallRefill",
    "WallTech",
    "CeilingRefill",
    "CeilingTech",
    "BigInertiaUpdiagDash",
    "UpwardWallJumpAcceleration",
    "DownwardWallJumpAcceleration",
    "WaterSurfaceTech",
    "GroundTech",
    "QoL",
}

map_config.placements = {
    {
        name = "Map Config (Advanced)",
        data = {
            ModEnable = true,
            CeilingUltraEnabled = true,
            CeilingRefillStamina = true,
            CeilingRefillDash = true,
            CeilingJumpEnabled = true,
            CeilingHyperEnabled = true,
            GroundJumpEnabled = true,
            GroundUltraEnabled = true,
            GroundHyperEnabled = true,
            UpdiagDashEndNoHorizontalSpeedLoss = true,
            VerticalUltraEnabled = true,
            VerticalHyperEnabled = true,
            DashBeginNoVerticalSpeedLoss = true,
            UpdiagDashEndNoVerticalSpeedLoss = true,
            WallRefillStamina = true,
            WallRefillDash = true,
            HorizontalUltraIntoVerticalUltra = true,
            VerticalUltraIntoHorizontalUltra = true,
            UpwardWallJumpAcceleration = true,
            DownwardWallJumpAcceleration = true,
            WaterWaveDashEnabled = true,
            WaterCeilingHyperEnabled = true,
            WaterBottomSurfaceJumpEnabled = true,
            QoLBufferVerticalHyper = true,
            QoLBufferVerticalUltra = true,
            QoLRefillDashOnWallJump = true,
            QoLBufferCeilingUltra = true,
        }
    },
    {
        name = "Map Config",
        data = {
            ModEnable = true,
            WallRefill = true,
            WallTech = true,
            CeilingRefill = true,
            CeilingTech = true,
            BigInertiaUpdiagDash = true,
            UpwardWallJumpAcceleration = true,
            DownwardWallJumpAcceleration = true,
            WaterSurfaceTech = true,
            GroundTech = true,
            QoL = true,
        }
    }
}

map_config.fieldOrder = function(entity)
    if entity ~= nil and entity.WallRefill ~= nil then
        return normalFieldOrder
    end
    return advancedFieldOrder
end

return map_config
