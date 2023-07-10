namespace YYZ.JTS
{
    /*
     *  32---- 1
     * 16/    \2 
     *   \____/
     *  8     4
     */
    public enum UnitDirection
    {
        RightTop = 1,
        Right = 2,
        RightBottom = 4,
        LeftBottom = 8,
        Left = 16,
        LeftTop = 32
    }

    public enum HexDirection
    {
        Top,
        TopRight,
        BottomRight,
        Bottom,
        BottomLeft,
        TopLeft
    }

    /*
    public enum GroupSize
    {
        Army,
        Wing,
        Corp,
        Division,
        Brigade,
    }
    */

    public enum UnitCategory
    {
        Infantry,
        Cavalry,
        Artillery
    }

    public enum JTSSeries
    {
        NapoleonicBattle, // NB
        CivilWarBattle, // CWB
        PanzerCampaign, // PZC
        SquadBattle // SB
    }

}