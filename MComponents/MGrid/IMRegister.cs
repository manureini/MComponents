namespace MComponents.MGrid
{
    public interface IMRegister
    {
        void RegisterColumn(IMGridColumn pColumn);

        void RegisterPagerSettings(MGridPager pPager);
    }
}