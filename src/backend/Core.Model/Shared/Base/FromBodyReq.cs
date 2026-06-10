namespace Core.Model.Shared;

public class IdData<TId, TData>
{
    public TId Id { get; set; }

    public TData Data { get; set; }
}

public class IdListData<TId, TItem>
{
    public TId Id { get; set; }

    public List<TItem> Data { get; set; } = [];
}

public class IdsData<TId, TData>
{
    public TId[] Id { get; set; }

    public TData Data { get; set; }
}
