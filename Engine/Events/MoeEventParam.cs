#if MOE_XLUA
[XLua.LuaCallCSharp]
#endif
public class MoeEventParam
{

}

#if MOE_XLUA
[XLua.LuaCallCSharp]
#endif
public class MoeIntEventParam : MoeEventParam
{
    public int value;

    public MoeIntEventParam(int _value)
    {
        this.value = _value;
    }
}