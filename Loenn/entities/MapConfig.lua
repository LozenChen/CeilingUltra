local map_config = {}

map_config.name = "CeilingUltra/CeilingUltraMapConfig"
map_config.depth = 0

map_config.placements = {
    name = "Ceiling Ultra Map Config",
    data = {
        Enabled = true,
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
}

map_config.fieldOrder = {
    "x", "y",
    "Enabled",
    "CeilingUltraEnabled",
    "CeilingRefillStamina",
    "CeilingRefillDash",
    "CeilingJumpEnabled",
    "CeilingHyperEnabled",
    "GroundJumpEnabled",
    "GroundUltraEnabled",
    "GroundHyperEnabled",
    "UpdiagDashEndNoHorizontalSpeedLoss",
    "VerticalUltraEnabled",
    "VerticalHyperEnabled",
    "DashBeginNoVerticalSpeedLoss",
    "UpdiagDashEndNoVerticalSpeedLoss",
    "WallRefillStamina",
    "WallRefillDash",
    "HorizontalUltraIntoVerticalUltra",
    "VerticalUltraIntoHorizontalUltra",
    "UpwardWallJumpAcceleration",
    "DownwardWallJumpAcceleration",
    "WaterWaveDashEnabled",
    "WaterCeilingHyperEnabled",
    "WaterBottomSurfaceJumpEnabled",
    "QoLBufferVerticalHyper",
    "QoLBufferVerticalUltra",
    "QoLRefillDashOnWallJump",
    "QoLBufferCeilingUltra",
}

return map_config
