local trigger = {}

trigger.name = "CeilingUltra/EnableAllTrigger"
trigger.placements = {
        name = "Enable All Trigger",
        data = {
            OneUse = true,
            ModEnable = true,
            WallRefill = true,
            WallTech = true,
            CeilingRefill = true,
            CeilingTech = true,
            BigInertiaUpdiagDash = true,
            UpwardWallJumpAcceleration = true,
            DownwardWallJumpAcceleration = true,
            GroundTech = true,
            QoL = false
        }
}

trigger.fieldOrder = {"x", "y", "width", "height", "ModEnable", "OneUse", "WallRefill", "CeilingRefill", "WallTech", "CeilingTech", "GroundTech", "UpwardWallJumpAcceleration", "DownwardWallJumpAcceleration", "BigInertiaUpdiagDash", "QoL"}

return trigger