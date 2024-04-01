local trigger = {}

trigger.name = "CeilingUltra/EnableAllTrigger"
trigger.placements = {
        name = "Enable All Trigger",
        data = {
            OneUse = true,
            ModEnable = true,
            WallRefill = true,
            VerticalTech = true,
            CeilingRefill = true,
            CeilingTech = true,
            BigInertiaUpdiagDash = true,
            UpwardWallJumpAcceleration = true,
            DownwardWallJumpAcceleration = true,
            GroundTech = true
        }
}

trigger.fieldOrder = {"x", "y", "width", "height", "ModEnable", "OneUse", "WallRefill", "CeilingRefill", "VerticalTech", "CeilingTech", "GroundTech", "UpwardWallJumpAcceleration", "DownwardWallJumpAcceleration", "BigInertiaUpdiagDash"}

return trigger