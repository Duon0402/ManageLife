using ManageLife.Base;
using ManageLife.Data;

namespace ManageLife.Services
{
    public class GenCodeService : ServiceBase
    {
        public GenCodeService(AppDbContext context) : base(context)
        {

        }

        private string MapTypeCSharpToTS(string type)
        {
            return type switch
            {
                "string" => "string",
                "int" => "number",
                "decimal" => "number",
                "float" => "number",
                "double" => "number",
                "bool" => "boolean",
                "DateTime" => "Date",
                _ => "any"

            };
        }
    }
}
