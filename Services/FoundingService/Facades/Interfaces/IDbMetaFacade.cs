/*
重构代码：
*/
using MyBackend.Models;

namespace MyBackend.Facades.Interfaces;

public interface IDbMetaFacade
{
    // 获取当前用户的所有表名
    Task<IEnumerable<string>> FetchAllTableNamesAsync();
    
    // 获取指定表的列结构信息
    Task<IEnumerable<TableMetaResult>> FetchTableColumnsAsync(string tableName);
    
    // 获取标签表的原始数据
    Task<IEnumerable<object>> FetchAllTagsAsync();
}