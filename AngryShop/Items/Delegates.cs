namespace AngryShop.Items
{
    /// <summary> This is invoked when we just need to notify somebody that something has happened (no parameters is passed or returned) </summary>
    public delegate void SomethingHappenedDelegate();

    /// <summary> This is invoked when we close some dialog (saved data or not) </summary>
    public delegate void DialogClosedDelegate(bool saved);
}
