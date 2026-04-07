using System;

[Flags]
public enum ESingletonFlag
{
    NONE,

    DONT_DESTROY,

    MAX
}

public sealed class SingletonFlag : Attribute
{
    private readonly ESingletonFlag _flag;

    public ESingletonFlag Flag => _flag;

    public SingletonFlag()
    {
        _flag = ESingletonFlag.NONE;
    }

    public SingletonFlag(ESingletonFlag flag)
    {
        _flag = flag;
    }
}
