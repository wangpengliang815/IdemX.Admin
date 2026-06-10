namespace Core.Infrastructure.Utility;

/// <summary>
/// 树形结构工具类 - 将平铺数据转换为树形结构
/// </summary>
public static class TreeHelper
{
    /// <summary>
    /// 将平铺列表转换为树形结构
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <typeparam name="TKey">ID类型</typeparam>
    /// <param name="entities">平铺的实体列表</param>
    /// <param name="idSelector">ID选择器</param>
    /// <param name="parentIdSelector">ParentId选择器</param>
    /// <param name="sortSelector">排序选择器</param>
    /// <param name="entityToDynamic">实体转动态对象的转换函数</param>
    /// <param name="rootId">根节点ID，默认为default(TKey)</param>
    /// <returns>树形结构（每个节点包含children子数组）</returns>
    public static List<dynamic> ToTree<TEntity, TKey>(
        this IEnumerable<TEntity> entities,
        Func<TEntity, TKey> idSelector,
        Func<TEntity, TKey> parentIdSelector,
        Func<TEntity, int> sortSelector,
        Func<TEntity, dynamic> entityToDynamic,
        TKey rootId = default)
    {
        if (entities == null || !entities.Any())
            return [];

        var entityList = entities.ToList();

        var dynamicList = entityList.Select(e => new
        {
            Entity = e,
            Dynamic = entityToDynamic(e),
            Id = idSelector(e),
            ParentId = parentIdSelector(e)
        }).ToList();

        var lookup = dynamicList.ToLookup(x => x.ParentId);

        List<dynamic> BuildRecursive(TKey parentId)
        {
            return [.. lookup[parentId]
                .OrderBy(x => sortSelector(x.Entity))
                .Select(item =>
                {
                    var nodeObj = item.Dynamic;
                    var dict = new Dictionary<string, object>();

                    var objType = nodeObj.GetType();
                    foreach (var prop in objType.GetProperties())
                    {
                        dict[prop.Name] = prop.GetValue(nodeObj);
                    }

                    dict["children"] = BuildRecursive(item.Id);
                    return dict as dynamic;
                })
                .Cast<dynamic>()];
        }

        return BuildRecursive(rootId);
    }

    /// <summary>
    /// 将包含 Children 集合的节点列表转换为强类型树结构
    /// </summary>
    /// <typeparam name="TNode">节点 DTO/实体 类型</typeparam>
    /// <typeparam name="TKey">ID 类型</typeparam>
    /// <param name="nodes">平铺的节点集合</param>
    /// <param name="idSelector">节点 Id 选择器</param>
    /// <param name="parentIdSelector">ParentId 选择器</param>
    /// <param name="sortSelector">排序字段选择器</param>
    /// <param name="childrenSelector">返回节点 Children 集合的委托</param>
    /// <param name="rootId">根节点 Id，通常为 0 或 default</param>
    /// <returns>根节点列表（每个节点的 Children 已填充）</returns>
    public static List<TNode> ToTree<TNode, TKey>(
        this IEnumerable<TNode> nodes,
        Func<TNode, TKey> idSelector,
        Func<TNode, TKey> parentIdSelector,
        Func<TNode, int> sortSelector,
        Func<TNode, IList<TNode>> childrenSelector,
        TKey rootId = default)
    {
        if (nodes == null || !nodes.Any())
            return [];

        var nodeList = nodes.ToList();

        var lookup = nodeList
            .Select(n => new
            {
                Node = n,
                Id = idSelector(n),
                ParentId = parentIdSelector(n)
            })
            .ToLookup(x => x.ParentId);

        List<TNode> BuildRecursive(TKey parentId)
        {
            return [.. lookup[parentId]
                .OrderBy(x => sortSelector(x.Node))
                .Select(item =>
                {
                    var node = item.Node;
                    var children = childrenSelector(node);
                    children.Clear();
                    var childNodes = BuildRecursive(item.Id);
                    foreach (var child in childNodes)
                    {
                        children.Add(child);
                    }

                    return node;
                })];
        }

        return BuildRecursive(rootId);
    }
}
