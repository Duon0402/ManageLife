namespace ManageLife.Base
{
    public static class RazorPageConst
    {
        public static class Layout
        {
            public const string RootLayout = "~/Views/Shared/_RootLayout.cshtml";
            public const string AdminLayout = "~/Views/Shared/Admin/_AdminLayout.cshtml";
            public const string ClientLayout = "~/Views/Shared/Client/_ClientLayout.cshtml";
            public const string AdminDataGridLayout = "~/Views/Shared/Admin/_AdminDataGridLayout.cshtml";
        }

        public static class Partial
        {
            //TODO: Sử dụng các thằng này để sau tái sử dụng
            public const string AdminHeader = "Admin/_AdminHeader";
            public const string AdminSidebar = "Admin/_AdminSidebar";
        }

        public static class Section
        {
            public const string Form = "Form";
            public const string Grid = "Grid";
            public const string ViewScripts = "ViewScripts";
        }
    }
}
