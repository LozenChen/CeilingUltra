namespace Celeste.Mod.CeilingUltra.Entities.TutorialMachine;
internal static class PageCollection {
    public static CeilingUltraPage Create(string id) {
        return id.ToLowerInvariant() switch {
            "0" or "00" => new CeilingUltraPage00(CeilingUltraPage00.TitleType.CeilingUltra),
            "0a" or "00a" => new CeilingUltraPage00(CeilingUltraPage00.TitleType.CeilingHyper),
            "0b" or "00b" => new CeilingUltraPage00(CeilingUltraPage00.TitleType.WallHyper),
            "1" or "01" => new CeilingUltraPage01(CeilingUltraPage01.TitleType.CeilingUltra),
            "1a" or "01a" => new CeilingUltraPage01(CeilingUltraPage01.TitleType.CeilingHyper),
            "1b" or "01b" => new CeilingUltraPage01(CeilingUltraPage01.TitleType.WallHyper),
            "2" or "02" or "2a" or "02a" or "2b" or "02b" => new CeilingUltraPage02(),
            "3" or "03" or "3a" or "03a" => new CeilingUltraPage03a(),
            "3b" or "03b" => new CeilingUltraPage03b(),
            "4" or "04" or "4a" or "04a" => new CeilingUltraPage04a(),
            "4b" or "04b" => new CeilingUltraPage04b(),
            "5" or "05" or "5a" or "05a" => new CeilingUltraPage05a(),
            "5b" or "05b" => new CeilingUltraPage05b(),
            "6" or "06" => new CeilingUltraPage06(CeilingUltraPage06.TitleType.CeilingUltra),
            "6a" or "06a" => new CeilingUltraPage06(CeilingUltraPage06.TitleType.CeilingHyper),
            "6b" or "06b" => new CeilingUltraPage06(CeilingUltraPage06.TitleType.WallHyper),
            _ => throw new Exception("[CeilingUltra] Presentation doesn't contain this page")
        };
    }

}
