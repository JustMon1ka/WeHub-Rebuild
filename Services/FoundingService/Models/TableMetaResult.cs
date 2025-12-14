/*
重构代码：

refactoring with Data Transfer Object
*/
namespace MyBackend.Models;

public class TableMetaResult
{
    public string ColumnName { get; set; } = "";
    public string DataType { get; set; } = "";
    public int Length { get; set; }
    public string Nullable { get; set; } = "";
}