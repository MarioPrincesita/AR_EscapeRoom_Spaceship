using SpaceshipEscapeRoom.AR;

namespace SpaceshipEscapeRoom.Gameplay
{
    public enum Department
    {
        Commander = 0,
        Engineering = 1,
        Science = 2,
        Navigation = 3
    }

    public static class DepartmentInfo
    {
        public const int Count = 4;

        private static readonly string[] Ids =
        {
            "Commander",
            "Engineering",
            "Science",
            "Navigation"
        };

        private static readonly string[] CardNames =
        {
            CardTrackingHandler.CardCommander,
            CardTrackingHandler.CardEngineering,
            CardTrackingHandler.CardScience,
            CardTrackingHandler.CardNavigation
        };

        public static string GetId(Department department)
        {
            return Ids[(int)department];
        }

        public static string GetCardName(Department department)
        {
            return CardNames[(int)department];
        }
    }
}
