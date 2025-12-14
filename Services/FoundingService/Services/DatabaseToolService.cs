/*
重构代码：
*/
using MyBackend.Facades.Interfaces;
using MyBackend.Models;

namespace MyBackend.Services;

public class DatabaseToolService
{
    private readonly IDbMetaFacade _metaFacade;

    public DatabaseToolService(IDbMetaFacade metaFacade)
    {
        _metaFacade = metaFacade;
    }

    public async Task<IEnumerable<string>> GetAllTablesAsync()
    {
        return await _metaFacade.FetchAllTableNamesAsync();
    }

    public async Task<IEnumerable<TableMetaResult>> GetTableColumnsAsync(string tableName)
    {
        // 可以在这里加业务逻辑，比如：禁止查询敏感表
        if (tableName.ToUpper() == "ADMIN_SECRETS")
        {
            throw new InvalidOperationException("Access denied to sensitive table.");
        }

        return await _metaFacade.FetchTableColumnsAsync(tableName);
    }

    public async Task<IEnumerable<object>> GetAllTagsRawAsync()
    {
        return await _metaFacade.FetchAllTagsAsync();
    }
}